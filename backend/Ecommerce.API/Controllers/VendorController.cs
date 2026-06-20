using Ecommerce.Contracts.Services;
using Ecommerce.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Ecommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VendorController : ControllerBase
    {
        private readonly IVendorService _vendorService;
        private readonly ICurrentUserService _currentUserService;
        public VendorController(IVendorService vendorService, ICurrentUserService currentUserService)
        {
            _vendorService = vendorService;
            _currentUserService = currentUserService;
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("register")]
        public async Task<IActionResult> CreateVendor([FromBody] CreateVendorRequest request)
        {
            var result = await _vendorService.CreateVendor(request);
            return Ok(new { message = "Vendor registered. Waiting for approval.", data = result });
        }

        [Authorize(Roles = "Vendor")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateVendor([FromBody] UpdateVendorRequest request)
        {
            var result = await _vendorService.UpdateVendor(request);
            return Ok(new { message = "Vendor profile updated successfully.", data = result });
        }

        [Authorize(Roles = "Vendor")]
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyVendorProfile()
        {
            var userId = _currentUserService.UserId;
            var result = await _vendorService.GetVendorByUserId(userId);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [Authorize(Roles = "Vendor")]
        [HttpPut]
        public async Task<IActionResult> ToggleVendorStatus()
        {   int userId = _currentUserService.UserId;
            var result = await _vendorService.ToggleVendorStatus(userId); 
            return Ok(new
            {
                message = "Vendor profile status toggled successfully.",
                data = result
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("listVendors")]
        public async Task<IActionResult> GetAllVendors([FromQuery] PageRequest query)
        {
            var vendors = await _vendorService.GetAllVendors(query);
            return Ok(vendors);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("search/user/{userId}")]
        public async Task<IActionResult> GetVendorProfileByUserId([FromRoute] int userId)
        {
            var result = await _vendorService.GetVendorByUserId(userId);
            if (result == null) return NotFound();
            return Ok(result);
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("searchById/{Id}")]
        public async Task<IActionResult> GetVendorProfileById([FromRoute] int Id)
        {
            var result = await _vendorService.GetVendorById(Id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("searchByStore/{storeName}")]
        public async Task<ActionResult<ICollection<VendorProfileResponse>>> GetVendorByStoreName([FromRoute] string storeName)
        {   if(string.IsNullOrWhiteSpace(storeName)) return BadRequest("Store name is required.");
            var profiles = await _vendorService.GetVendorByStoreName(storeName);
            return Ok(profiles);
        }

        [Authorize(Roles = "Admin")] 
        [HttpGet("searchByStatus/{status}")]
        public async Task<ActionResult<ICollection<VendorProfileResponse>>> GetVendorsByStatus([FromRoute] string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return BadRequest("Status query parameter is required.");
            
            var profiles = await _vendorService.GetVendorsByStatus(status);
            return Ok(profiles);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("approve/{id}")]
        public async Task<ActionResult<VendorProfileResponse>> ApproveVendor([FromRoute]int id)
        {
            if (id == 0) return BadRequest("Vendor ID is required.");
            var result = await _vendorService.ApproveVendor(id);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("cancel/{id}")]
        public async Task<ActionResult<VendorProfileResponse>> CancelVendor([FromRoute]int id)
        {
            if (id == 0) return BadRequest("Vendor ID is required.");
            var result = await _vendorService.CancelVendor(id);
            return Ok(result);
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("searchBasicById/{Id}")]
        public async Task<IActionResult> GetVendorBasicProfileById([FromRoute] int Id)
        {
            var result = await _vendorService.GetVendorBasicById(Id);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}