using System.ComponentModel;
using AutoMapper;
using Ecommerce.Contracts.Services;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Models.DTOs;
using Ecommerce.Models;
using Ecommerce.Shared.Exceptions;

namespace Ecommerce.BLL
{
    public class VendorService : IVendorService
    {
        private readonly IVendorRepository _vendorRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public VendorService(IVendorRepository vendorRepository, IUserRepository userRepository, ICurrentUserService currentUserService, IMapper mapper)
        {
            _vendorRepository = vendorRepository;
            _userRepository = userRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<VendorProfileResponse> CreateVendor(CreateVendorRequest vendor)
        {

            vendor.StoreName = vendor.StoreName.Trim().ToLower();
            vendor.StoreEmail = vendor.StoreEmail?.Trim();
            vendor.GSTNumber = vendor.GSTNumber.Trim().ToUpper();
            vendor.PANNumber = vendor.PANNumber.Trim().ToUpper();
            if (!await _vendorRepository.VerifyGSTUnique(vendor.GSTNumber)) throw new InvalidVendorException("GST Number must be unique.");
            if (!await _vendorRepository.VerifyPANUnique(vendor.PANNumber)) throw new InvalidVendorException("PAN Number must be unique.");

            var vendorModel = _mapper.Map<Vendor>(vendor);

            vendorModel.UserId = _currentUserService.UserId;
            vendorModel.IsActive = true;
            vendorModel.Status = VendorStatus.Pending;
            var createdVendor = await _vendorRepository.Create(vendorModel);
            
            createdVendor.User = await _userRepository.GetById(createdVendor.UserId) ?? throw new KeyNotFoundException("User not found.");
            
            return _mapper.Map<VendorProfileResponse>(createdVendor);
        }

        public async Task<VendorProfileResponse> UpdateVendor(UpdateVendorRequest vendor)
        {
            var existingVendor = await _vendorRepository.GetByUserId(_currentUserService.UserId)
                ?? throw new InvalidVendorException("Vendor not found for the current user.");
            
            existingVendor.User.FullName   = vendor.FullName?.Trim() ?? existingVendor.User.FullName;
            existingVendor.StoreName   = vendor.StoreName?.Trim().ToLower() ?? existingVendor.StoreName;
            existingVendor.StoreEmail  = vendor.StoreEmail?.Trim() ?? existingVendor.StoreEmail;
            existingVendor.Description = vendor.Description?.Trim() ?? existingVendor.Description;
            existingVendor.LogoUrl     = vendor.LogoUrl?.Trim() ?? existingVendor.LogoUrl;

            if (!string.IsNullOrWhiteSpace(vendor.Email))
            {
                if(!await _userRepository.VerifyEmailUnique(vendor.Email.Trim(), _currentUserService.UserId))
                    throw new InvalidVendorException("Email already exists.");
                existingVendor.User.Email = vendor.Email.Trim();
            }

            if (!string.IsNullOrWhiteSpace(vendor.StoreEmail))
            {
                if(!await _vendorRepository.VerifyEmailUnique(vendor.StoreEmail.Trim(), existingVendor.Id))
                    throw new InvalidVendorException("Store Email already exists.");
                existingVendor.StoreEmail = vendor.StoreEmail.Trim();
            }

            if (!string.IsNullOrWhiteSpace(vendor.GSTNumber))
            {
                var gst = vendor.GSTNumber.Trim().ToUpper();
                if (!await _vendorRepository.VerifyGSTUnique(gst, existingVendor.Id))
                    throw new InvalidVendorException("GST Number must be unique.");
                existingVendor.GSTNumber = gst;
            }

            if (!string.IsNullOrWhiteSpace(vendor.PANNumber))
            {
                var pan = vendor.PANNumber.Trim().ToUpper();
                if (!await _vendorRepository.VerifyPANUnique(pan, existingVendor.Id))
                    throw new InvalidVendorException("PAN Number must be unique.");
                existingVendor.PANNumber = pan;
            }

            try
            {
                await _userRepository.SaveChangesAsync();
                return _mapper.Map<VendorProfileResponse>(existingVendor);
            }
            catch (Exception ex)
            {
                throw new InvalidVendorException("Failed to update user information for the vendor."+ ex.Message);
            }
        }

        public async Task<VendorStatusResponse> ToggleVendorStatus(int id)
        {
            var vendor = await _vendorRepository.GetByUserId(id);
            if (vendor == null) throw new InvalidVendorException("Vendor not found for the current user.");
            await _vendorRepository.ToggleVendorStatus(vendor.Id);
            return _mapper.Map<VendorStatusResponse>(vendor);
        }

        public async Task<PageResponse<VendorProfileResponse>> GetAllVendors(PageRequest query)
        {
            var (pagedVendors, totalCount) = await _vendorRepository.GetPagedVendors(
                query.SearchTerm ?? "",
                query.PageNumber,
                query.PageSize);

            var items = _mapper.Map<List<VendorProfileResponse>>(pagedVendors);

            return new PageResponse<VendorProfileResponse>
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<VendorProfileResponse?> GetVendorByUserId(int userId)
        {
            var vendor = await _vendorRepository.GetByUserId(userId);
            if (vendor == null) throw new InvalidVendorException($"Vendor not found for the current user : {userId}");
            Console.WriteLine($"At Service: {vendor.User.FullName}, {vendor.User.Email}");

            return _mapper.Map<VendorProfileResponse>(vendor);
        }

        public async Task<VendorProfileResponse?> GetVendorById(int id)
        {
            var vendor = await _vendorRepository.GetById(id);
            if (vendor == null) throw new InvalidVendorException($"Vendor not found with VendorID : {id}");
            return _mapper.Map<VendorProfileResponse>(vendor);
        }

        public async Task<VendorBasicResponse?> GetVendorBasicById(int id)
        {
            var vendor = await _vendorRepository.GetById(id);
            if (vendor == null) throw new InvalidVendorException($"Vendor not found with VendorID : {id}");
            return _mapper.Map<VendorBasicResponse>(vendor);
        }

        public async Task<ICollection<VendorProfileResponse>> GetVendorByStoreName(string storeName)
        {
            var vendors = await _vendorRepository.GetByStoreName(storeName.Trim().ToLower());
            if (vendors == null) throw new InvalidVendorException($"Vendor not found with Store Name : {storeName}");
            return _mapper.Map<ICollection<VendorProfileResponse>>(vendors);
        }

        public async Task<ICollection<VendorProfileResponse>> GetVendorsByStatus(string status)
        {
            VendorStatus vendorStatus = status.ToLower() switch
            {
                "pending"   => VendorStatus.Pending,
                "approved"  => VendorStatus.Approved,
                "cancelled" => VendorStatus.Cancelled,
                _ => throw new InvalidVendorException("Invalid Status. Must be 'Pending', 'Approved', or 'cancelled'.")
            };
            var result = await _vendorRepository.GetVendorsByStatus(vendorStatus);
            return _mapper.Map<ICollection<VendorProfileResponse>>(result);
        }

        public async Task<VendorProfileResponse> ApproveVendor(int id)
        {

            var existingVendor = await _vendorRepository.GetById(id);
            if (existingVendor == null) throw new InvalidVendorException("Vendor not found.");

            existingVendor.Status = VendorStatus.Approved;
            existingVendor.ApprovedAt = DateTime.Now;

            if (!await ChangeUserRole(existingVendor.UserId, UserRole.Vendor)) throw new InvalidOperationException("Failed to update user status for the vendor.");

            var approvedResult = await _vendorRepository.Update(existingVendor.Id, existingVendor);
            return _mapper.Map<VendorProfileResponse>(approvedResult ?? throw new InvalidVendorException("Failed to approve vendor."));
        }

        public async Task<VendorProfileResponse> CancelVendor(int id)
        {
            var existingVendor = await _vendorRepository.GetById(id);
            if (existingVendor == null) throw new InvalidVendorException("Vendor not found.");

            existingVendor.Status = VendorStatus.Cancelled;
            existingVendor.IsActive = false;

            if (!await ChangeUserRole(existingVendor.UserId, UserRole.Customer)) throw new InvalidOperationException("Failed to update user status for the vendor.");

            var cancelledResult = await _vendorRepository.Update(existingVendor.Id, existingVendor);
            return _mapper.Map<VendorProfileResponse>(cancelledResult ?? throw new InvalidVendorException("Failed to suspend vendor."));
        }

        private async Task<bool> ChangeUserRole(int userId, UserRole role)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null) throw new KeyNotFoundException("User not found");
            user.Role = role;
            var result = await _userRepository.Update(userId, user);
            return result != null;
        }
        private async Task<bool> VerifyGSTUnique(string gstNumber)
        {
            var vendor = await _vendorRepository.GetByUserId(_currentUserService.UserId);
            int vendorId = vendor?.Id ?? 0;
            if (gstNumber.Length != 15) throw new InvalidVendorException("Invalid GST Number. GST Number must be 15 characters long.");
            return await _vendorRepository.VerifyGSTUnique(gstNumber.ToUpper(), vendorId);
        }

        private async Task<bool> VerifyPANUnique(string panNumber)
        {
            var vendor = await _vendorRepository.GetByUserId(_currentUserService.UserId);
            int vendorId = vendor?.Id ?? 0;
            if (panNumber.Length != 10) throw new InvalidVendorException("Invalid PAN Number. PAN Number must be 10 characters long.");
            return await _vendorRepository.VerifyPANUnique(panNumber.ToUpper(), vendorId);
        }
    }
}