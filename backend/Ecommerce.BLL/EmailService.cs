using System;
using System.Text;
using System.Threading.Tasks;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Shared.Exceptions;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Ecommerce.BLL
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOrderConfirmationEmail(Order order)
        {
            var host = _configuration["Smtp:Host"];
            var portStr = _configuration["Smtp:Port"];
            var senderEmail = _configuration["Smtp:SenderEmail"];
            var password = _configuration["Smtp:Password"];
            var fromName = _configuration["Smtp:FromName"] ?? "BBS";

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password) || !int.TryParse(portStr, out int port))
            {
                throw new InvalidEmailCredsException("Invalid Email Credentials");
            }

            var recipientEmail = order.User?.Email;
            if (string.IsNullOrEmpty(recipientEmail))
            {
                throw new InvalidEmailCredsException("Invalid Recipient Email");
            }

            var subject = $"Order Confirmed - #{order.Id}";
            var body = BuildOrderConfirmationEmailBody(order);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, senderEmail));
            message.To.Add(new MailboxAddress(order.User?.FullName ?? "Valued Customer", recipientEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            await SendEmailClientAsync(message, host, port, senderEmail, password);
        }

        protected virtual async Task SendEmailClientAsync(MimeMessage mimeMessage, string host, int port, string senderEmail, string password)
        {
            using var client = new SmtpClient();

            SecureSocketOptions options = SecureSocketOptions.Auto;
            if (port == 587)
            {
                options = SecureSocketOptions.StartTls;
            }
            else if (port == 465)
            {
                options = SecureSocketOptions.SslOnConnect;
            }
            else if (port == 25 || port == 2525)
            {
                options = SecureSocketOptions.None;
            }

            await client.ConnectAsync(host, port, options);
            await client.AuthenticateAsync(senderEmail, password);
            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(true);
        }

        private string BuildOrderConfirmationEmailBody(Order order)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style='font-family: Arial, sans-serif;'>");
            sb.Append($"<h2>Hello {order.User?.FullName ?? "Valued Customer"},</h2>");
            sb.Append("<p>Thank you for your order! Your payment was successful, and your order has been confirmed.</p>");
            sb.Append($"<p><strong>Order ID:</strong> #{order.Id}</p>");
            sb.Append($"<p><strong>Order Date:</strong> {order.PlacedAt:f}</p>");
            sb.Append("<hr/>");
            sb.Append("<h3>Order Details:</h3>");
            sb.Append("<table style='width: 100%; border-collapse: collapse;'>");
            sb.Append("<thead>");
            sb.Append("<tr style='background-color: #f2f2f2;'>");
            sb.Append("<th style='padding: 8px; border: 1px solid #ddd; text-align: left;'>Product</th>");
            sb.Append("<th style='padding: 8px; border: 1px solid #ddd; text-align: center;'>Quantity</th>");
            sb.Append("<th style='padding: 8px; border: 1px solid #ddd; text-align: right;'>Unit Price</th>");
            sb.Append("<th style='padding: 8px; border: 1px solid #ddd; text-align: right;'>Total Price</th>");
            sb.Append("</tr>");
            sb.Append("</thead>");
            sb.Append("<tbody>");

            if (order.Items != null)
            {
                foreach (var item in order.Items)
                {
                    var productName = item.Variant?.Product?.Name ?? "Product";
                    var qty = item.Quantity;
                    var price = item.UnitPrice;
                    var total = qty * price;

                    sb.Append("<tr>");
                    sb.Append($"<td style='padding: 8px; border: 1px solid #ddd;'>{productName}</td>");
                    sb.Append($"<td style='padding: 8px; border: 1px solid #ddd; text-align: center;'>{qty}</td>");
                    sb.Append($"<td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>₹ {price:F2}</td>");
                    sb.Append($"<td style='padding: 8px; border: 1px solid #ddd; text-align: right;'>₹ {total:F2}</td>");
                    sb.Append("</tr>");
                }
            }

            sb.Append("</tbody>");
            sb.Append("</table>");
            sb.Append("<hr/>");

            sb.Append("<div style='text-align: right; font-size: 14px;'>");
            sb.Append($"<p><strong>Subtotal:</strong> ₹ {order.Subtotal:F2}</p>");
            if (order.DiscountAmount > 0)
            {
                sb.Append($"<p style='color: green;'><strong>Discount:</strong> -₹ {order.DiscountAmount:F2}</p>");
            }
            sb.Append($"<p><strong>Tax:</strong> ₹ {order.TaxAmount:F2}</p>");
            sb.Append($"<p><strong>Shipping:</strong> ₹ {order.ShippingAmount:F2}</p>");
            sb.Append($"<h3 style='color: #2e7d32;'><strong>Total Paid:</strong> ₹ {order.Total:F2}</h3>");
            sb.Append("</div>");

            sb.Append("<p style='font-size: 12px; color: #666;'>If you have any questions, feel free to reply to this email.</p>");
            sb.Append("<p>Best regards,<br/><strong>BBS Team</strong></p>");
            sb.Append("</body></html>");

            return sb.ToString();
        }
    }
}
