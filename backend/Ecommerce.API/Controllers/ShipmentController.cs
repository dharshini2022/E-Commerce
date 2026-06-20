using AutoMapper;
using Ecommerce.Contracts.Services;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentController : ControllerBase
    {
        private readonly IShipmentService _shipmentService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IVendorRepository _vendorRepository;
        private readonly IMapper _mapper;

        public ShipmentController(IShipmentService shipmentService,ICurrentUserService currentUserService,IVendorRepository vendorRepository, IMapper mapper)
        {
            _shipmentService = shipmentService;
            _currentUserService = currentUserService;
            _vendorRepository = vendorRepository;
            _mapper = mapper;
        }

        [HttpGet("{shipmentId}")]
        [Authorize(Roles = "Customer,Vendor,Admin")]
        public async Task<IActionResult> GetShipmentDetails([FromRoute] int shipmentId)
        {
            var shipment = await _shipmentService.GetShipmentDetails(shipmentId);
            var role = _currentUserService.Role;
            var userId = _currentUserService.UserId;

            if (role == "Customer")
            {
                if (shipment.UserAddress == null || shipment.UserAddress.UserId != userId)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied. You do not own this shipment address." });
                }
            }
            else if (role == "Vendor")
            {
                var vendor = await _vendorRepository.GetByUserId(userId);
                if (vendor == null)
                {
                    return NotFound(new { message = "Vendor profile not found." });
                }
                if (!shipment.OrderItems.Any(oi => oi.VendorId == vendor.Id))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied. You do not own any items in this shipment." });
                }
            }

            var response = _mapper.Map<ShipmentResponseDTO>(shipment);
            return Ok(response);
        }

        [HttpGet("my-shipments")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyShipments()
        {
            var userId = _currentUserService.UserId;
            var shipments = await _shipmentService.GetCustomerShipments(userId);
            var response = _mapper.Map<ICollection<ShipmentResponseDTO>>(shipments);
            return Ok(response);
        }

        [HttpGet("vendor-shipments")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> GetVendorShipments()
        {
            var userId = _currentUserService.UserId;
            var vendor = await _vendorRepository.GetByUserId(userId);
            if (vendor == null)
            {
                return NotFound(new { message = "Vendor profile not found." });
            }

            var shipments = await _shipmentService.GetVendorShipments(vendor.Id);
            var response = _mapper.Map<ICollection<ShipmentResponseDTO>>(shipments);
            return Ok(response);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllShipments([FromQuery] int? productId, [FromQuery] int? vendorId)
        {
            var shipments = await _shipmentService.GetAllShipments(productId, vendorId);
            var response = _mapper.Map<ICollection<ShipmentResponseDTO>>(shipments);
            return Ok(response);
        }
    }
}
