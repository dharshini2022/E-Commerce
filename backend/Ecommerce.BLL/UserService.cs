using Ecommerce.Contracts.Services;
using Ecommerce.Contracts.Repositories;
using AutoMapper;
using Ecommerce.Models.DTOs;
using Ecommerce.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Ecommerce.Contracts;

namespace Ecommerce.BLL
{                     
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public UserService(IUserRepository userRepository, IMapper mapper, ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<UserProfileResponse> GetUserDetails(int userId)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null) throw new KeyNotFoundException("User not found");
            return _mapper.Map<UserProfileResponse>(user);
        }


        public async Task<UserProfileResponse> UpdateProfile(UserProfileRequest request)
        {
            int userId = _currentUserService.UserId;
            var user = await _userRepository.GetById(userId);
            if (user == null) throw new KeyNotFoundException("User not found");
            _mapper.Map(request,user);

            if(!await _userRepository.VerifyEmailUnique(request.Email, userId)){
                throw new Exception("Email already exists");
            }  
            var result = await _userRepository.Update(userId, user);
            if (result == null) throw new InvalidOperationException("Failed to update profile");
            
            return _mapper.Map<UserProfileResponse>(result);
        }

        public async Task<PageResponse<UserProfileResponse>> ListUsers(PageRequest query)
        {
            var (pagedUsers, totalCount) = await _userRepository.GetPagedUsers(query.SearchTerm ?? "", query.PageNumber,  query.PageSize); 

            var items = _mapper.Map<List<UserProfileResponse>>(pagedUsers);
            return new PageResponse<UserProfileResponse>
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<bool> ChangePassword(ChangePasswordRequest request)
        {
            var userId = _currentUserService.UserId;
            var user = await _userRepository.GetById(userId);
            if (user == null) throw new KeyNotFoundException("User not found");

            bool isValid = BCrypt.Net.BCrypt.Verify(request.OldPassword.Trim(), user.PasswordHash!.Trim());

            if (!isValid)
            {
                throw new UnauthorizedAccessException("Invalid Old Password for UserID : {userId}");
            }

            string newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            var success = await _userRepository.ChangePassword(userId, newHash);
            if (!success) throw new InvalidOperationException("Failed to update password");

            return isValid;
        }

        public async Task<bool> ToggleAccountStatus()
        {
            var userId = _currentUserService.UserId;
            var user = await _userRepository.GetById(userId);
            if (user == null) throw new KeyNotFoundException("Target user not found");

            user.IsActive = !user.IsActive;
            var result = await _userRepository.Update(userId, user);
            return result != null;
        }

        public async Task<UserProfileResponse> RevokeAdmin(int userId)
        {
            var user = await  _userRepository.GetById(userId);
            if(user == null)    throw new KeyNotFoundException("Admin Not Found");
            user.IsActive = false;
            var result = await _userRepository.Update(userId,user);
            return _mapper.Map<UserProfileResponse>(result);

        }

        public async System.Threading.Tasks.Task<Ecommerce.Models.DTOs.UserProfileResponse> ChangeRole(Ecommerce.Models.DTOs.ChangeRoleRequest request)
        {
            var user = await _userRepository.GetById(request.UserId);
            if (user == null) throw new KeyNotFoundException("User not found");
            user.Role = request.NewRole;
            var result = await _userRepository.Update(request.UserId, user);
            return _mapper.Map<UserProfileResponse>(result);
        }

        public async Task<AddAddressRequest> AddUserAddress(AddAddressRequest address)
        {
            var addressModel = _mapper.Map<UserAddress>(address);
            addressModel.UserId = _currentUserService.UserId;
            var result = await _userRepository.AddUserAddress(addressModel);
            return _mapper.Map<AddAddressRequest>(result);
        }
        public async Task<ICollection<AddAddressRequest>> GetAllUserAddress()
        {
            var userId = _currentUserService.UserId;
            var result = await _userRepository.GetAllAddressByUserId(userId);
            return _mapper.Map<ICollection<AddAddressRequest>>(result);
        }

        
    }
}