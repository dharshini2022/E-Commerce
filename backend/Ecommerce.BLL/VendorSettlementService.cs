using AutoMapper;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.DAL.Context;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.BLL
{
    public class VendorSummary{
        public decimal GrossAmount {get; set;}
        public decimal ShippingAmount {get; set;}
        public decimal VendorDiscountAmount {get; set;}
        public decimal PlatformCommissionAmount {get; set;}
        public decimal NetPayoutAmount {get; set;}
        public string TransactionReference {get; set;}
        public SettlementStatus SettlementStatus {get; set;}
    }
    public class VendorSettlementService : IVendorSettlementService
    {
        private readonly IVendorSettlementRepository _vendorSettlementRepository;
        private readonly IMapper _mapper;

        public VendorSettlementService(IVendorSettlementRepository vendorSettlementRepository, IMapper mapper)
        {
            _vendorSettlementRepository = vendorSettlementRepository;
            _mapper = mapper;
        }

        public async Task CreateSettlementsForOrder(Order order, string chargeId, Discount? discount)
        {
            var byVendor = order.Items.GroupBy(i => i.VendorId).ToList();
            var summary = new VendorSummary();
            foreach (var vendorGroup in byVendor)
            {
                int vendorId = vendorGroup.Key;
                summary.GrossAmount = vendorGroup.Sum(i => i.UnitPrice * i.Quantity);
                summary.ShippingAmount = Math.Round(summary.GrossAmount * OrderService.VendorShippingRate, 2);
                summary.VendorDiscountAmount = CalculateVendorDiscount(order, vendorId, summary.GrossAmount, discount);

                decimal commissionBase = summary.GrossAmount + summary.ShippingAmount - summary.VendorDiscountAmount;
                summary.PlatformCommissionAmount = Math.Round(commissionBase * OrderService.PlatformCommissionRate, 2);
                summary.NetPayoutAmount = commissionBase - summary.PlatformCommissionAmount;
                summary.TransactionReference = chargeId;
                summary.SettlementStatus = SettlementStatus.Paid;
                await CreateVendorSettlement(vendorId, order.Id, summary);
            }
        }

        private decimal CalculateVendorDiscount(Order order, int vendorId, decimal vendorGrossAmount, Discount? discount)
        {
            decimal vendorDiscount = 0;
            if (discount != null)
            {
                if (discount.Scope == DiscountScope.Vendor)
                {
                    if (discount.VendorId == vendorId)
                    {
                        vendorDiscount = order.DiscountAmount;
                    }
                }
                else if (discount.Scope == DiscountScope.Product || discount.Scope == DiscountScope.Category)
                {
                    decimal vendorShare = vendorGrossAmount / order.Subtotal;
                    vendorDiscount = Math.Round(order.DiscountAmount * vendorShare, 2);
                }
            }
            return vendorDiscount;
        }

        private async Task CreateVendorSettlement(int vendorId, int orderId, VendorSummary summary)
        {
            var settlement = new VendorSettlement
            {
                VendorId = vendorId,
                OrderId = orderId,
                GrossAmount = summary.GrossAmount,
                ShippingAmount = summary.ShippingAmount,
                VendorDiscountAmount = summary.VendorDiscountAmount,
                PlatformCommissionAmount = summary.PlatformCommissionAmount,
                NetPayoutAmount = summary.NetPayoutAmount,
                TransactionReference = summary.TransactionReference,
                Status = summary.SettlementStatus,
                SettledAt = DateTime.Now
            };
            await _vendorSettlementRepository.Create(settlement);
        }

        public async Task<ICollection<VendorSettlementDTO>> GetVendorSettlements(int vendorId)
        {
            var settlements = await _vendorSettlementRepository.GetSettlementsByVendorId(vendorId);
            return _mapper.Map<ICollection<VendorSettlementDTO>>(settlements);
        }

        public async Task<PageResponse<VendorSettlementDTO>> GetOverallSettlements(PageRequest request)
        {
            var (pagedSettlements, totalCount) = await _vendorSettlementRepository.GetPagedSettlementsWithDetails(request.SearchTerm, request.PageNumber, request.PageSize);

            var items = _mapper.Map<List<VendorSettlementDTO>>(pagedSettlements);

            return new PageResponse<VendorSettlementDTO>
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
