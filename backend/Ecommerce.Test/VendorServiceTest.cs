using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.BLL;
using Ecommerce.Contracts.Repositories;
using Ecommerce.Contracts.Services;
using Ecommerce.Models;
using Ecommerce.Models.DTOs;
using Ecommerce.Shared.Exceptions;
using Moq;
using NUnit.Framework;

namespace Ecommerce.Test
{
    [TestFixture]
    public class VendorServiceTest
    {
        private Mock<IVendorRepository> _mockVendorRepo;
        private Mock<IUserRepository> _mockUserRepo;
        private Mock<ICurrentUserService> _mockCurrentUser;
        private Mock<IMapper> _mockMapper;
        private VendorService _vendorService;

        [SetUp]
        public void Setup()
        {
            _mockVendorRepo = new Mock<IVendorRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockCurrentUser = new Mock<ICurrentUserService>();
            _mockMapper = new Mock<IMapper>();

            _vendorService = new VendorService(
                _mockVendorRepo.Object,
                _mockUserRepo.Object,
                _mockCurrentUser.Object,
                _mockMapper.Object
            );
        }

        [Test]
        public void CreateVendor_ShouldThrowException_WhenGstNotUnique()
        {
           
            _mockVendorRepo.Setup(r => r.VerifyGSTUnique("GST123456789012")).ReturnsAsync(false);
            var request = new CreateVendorRequest { GSTNumber = "GST123456789012", PANNumber = "PAN1234567", StoreName = "Store", StoreEmail = "store@store.com" };

            Assert.ThrowsAsync<InvalidVendorException>(async () => await _vendorService.CreateVendor(request));
        }

        [Test]
        public void CreateVendor_ShouldThrowException_WhenPanNotUnique()
        {
           
            _mockVendorRepo.Setup(r => r.VerifyGSTUnique("GST123456789012")).ReturnsAsync(true);
            _mockVendorRepo.Setup(r => r.VerifyPANUnique("PAN1234567")).ReturnsAsync(false);
            var request = new CreateVendorRequest { GSTNumber = "GST123456789012", PANNumber = "PAN1234567", StoreName = "Store", StoreEmail = "store@store.com" };

            Assert.ThrowsAsync<InvalidVendorException>(async () => await _vendorService.CreateVendor(request));
        }

        [Test]
        public void CreateVendor_ShouldThrowException_WhenUserNotFound()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(5);
            _mockVendorRepo.Setup(r => r.VerifyGSTUnique("GST123456789012")).ReturnsAsync(true);
            _mockVendorRepo.Setup(r => r.VerifyPANUnique("PAN1234567")).ReturnsAsync(true);

            var vendor = new Vendor { UserId = 5 };
            _mockMapper.Setup(m => m.Map<Vendor>(It.IsAny<CreateVendorRequest>())).Returns(vendor);
            _mockVendorRepo.Setup(r => r.Create(vendor)).ReturnsAsync(vendor);
            _mockUserRepo.Setup(r => r.GetById(5)).ReturnsAsync((User?)null);

            var request = new CreateVendorRequest { GSTNumber = "GST123456789012", PANNumber = "PAN1234567", StoreName = "Store", StoreEmail = "store@store.com" };

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _vendorService.CreateVendor(request));
        }

        [Test]
        public async Task CreateVendor_ShouldCreateVendor_WhenValid()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(5);
            _mockVendorRepo.Setup(r => r.VerifyGSTUnique("GST123456789012")).ReturnsAsync(true);
            _mockVendorRepo.Setup(r => r.VerifyPANUnique("PAN1234567")).ReturnsAsync(true);

            var vendor = new Vendor { UserId = 5 };
            _mockMapper.Setup(m => m.Map<Vendor>(It.IsAny<CreateVendorRequest>())).Returns(vendor);
            _mockVendorRepo.Setup(r => r.Create(vendor)).ReturnsAsync(vendor);
            _mockUserRepo.Setup(r => r.GetById(5)).ReturnsAsync(new User());

            var expected = new VendorProfileResponse { Id = 100 };
            _mockMapper.Setup(m => m.Map<VendorProfileResponse>(vendor)).Returns(expected);

            var request = new CreateVendorRequest { GSTNumber = "GST123456789012", PANNumber = "PAN1234567", StoreName = "Store", StoreEmail = "store@store.com" };

            
            var result = await _vendorService.CreateVendor(request);

            
            Assert.That(result.Id, Is.EqualTo(100));
        }

        [Test]
        public void UpdateVendor_ShouldThrowException_WhenVendorNotFound()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync((Vendor?)null);
            var request = new UpdateVendorRequest();

            Assert.ThrowsAsync<InvalidVendorException>(async () => await _vendorService.UpdateVendor(request));
        }

        [Test]
        public void UpdateVendor_ShouldThrowException_WhenUserEmailExists()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var user = new User { Email = "old@user.com" };
            var vendor = new Vendor { Id = 100, User = user };
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(vendor);

            _mockUserRepo.Setup(r => r.VerifyEmailUnique("new@user.com", 1)).ReturnsAsync(false);
            var request = new UpdateVendorRequest { Email = "new@user.com" };

            Assert.ThrowsAsync<InvalidVendorException>(async () => await _vendorService.UpdateVendor(request));
        }

        [Test]
        public void UpdateVendor_ShouldThrowException_WhenStoreEmailExists()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var user = new User { Email = "old@user.com" };
            var vendor = new Vendor { Id = 100, User = user, StoreEmail = "old@store.com" };
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(vendor);

            _mockVendorRepo.Setup(r => r.VerifyEmailUnique("new@store.com", 100)).ReturnsAsync(false);
            var request = new UpdateVendorRequest { StoreEmail = "new@store.com" };

            Assert.ThrowsAsync<InvalidVendorException>(async () => await _vendorService.UpdateVendor(request));
        }

        [Test]
        public void UpdateVendor_ShouldThrowException_WhenGstNotUnique()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var user = new User();
            var vendor = new Vendor { Id = 100, User = user };
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(vendor);

            _mockVendorRepo.Setup(r => r.VerifyGSTUnique("GST123456789012", 100)).ReturnsAsync(false);
            var request = new UpdateVendorRequest { GSTNumber = "GST123456789012" };

            Assert.ThrowsAsync<InvalidVendorException>(async () => await _vendorService.UpdateVendor(request));
        }

        [Test]
        public void UpdateVendor_ShouldThrowException_WhenPanNotUnique()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var user = new User();
            var vendor = new Vendor { Id = 100, User = user };
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(vendor);

            _mockVendorRepo.Setup(r => r.VerifyPANUnique("PAN1234567", 100)).ReturnsAsync(false);
            var request = new UpdateVendorRequest { PANNumber = "PAN1234567" };

            Assert.ThrowsAsync<InvalidVendorException>(async () => await _vendorService.UpdateVendor(request));
        }

        [Test]
        public async Task UpdateVendor_ShouldUpdateFields_WhenValid()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var user = new User { Email = "old@user.com", FullName = "Old Name" };
            var vendor = new Vendor 
            { 
                Id = 100, 
                User = user, 
                StoreName = "old store", 
                StoreEmail = "old@store.com",
                Description = "old desc",
                LogoUrl = "old logo"
            };
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(vendor);

            _mockUserRepo.Setup(r => r.VerifyEmailUnique("new@user.com", 1)).ReturnsAsync(true);
            _mockVendorRepo.Setup(r => r.VerifyEmailUnique("new@store.com", 100)).ReturnsAsync(true);
            _mockVendorRepo.Setup(r => r.VerifyGSTUnique("GST123456789012", 100)).ReturnsAsync(true);
            _mockVendorRepo.Setup(r => r.VerifyPANUnique("PAN1234567", 100)).ReturnsAsync(true);
            _mockUserRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var expected = new VendorProfileResponse { Id = 100 };
            _mockMapper.Setup(m => m.Map<VendorProfileResponse>(vendor)).Returns(expected);

            var request = new UpdateVendorRequest 
            { 
                FullName = "New Name",
                StoreName = "New Store",
                StoreEmail = "new@store.com",
                Description = "New Desc",
                LogoUrl = "New Logo",
                Email = "new@user.com",
                GSTNumber = "GST123456789012",
                PANNumber = "PAN1234567"
            };

            
            var result = await _vendorService.UpdateVendor(request);

            
            Assert.That(result, Is.Not.Null);
            Assert.That(user.FullName, Is.EqualTo("New Name"));
            Assert.That(vendor.StoreName, Is.EqualTo("new store"));
            Assert.That(vendor.GSTNumber, Is.EqualTo("GST123456789012"));
            _mockUserRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void ToggleVendorStatus_ShouldThrowException_WhenVendorNotFound()
        {
           
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync((Vendor?)null);

            Assert.ThrowsAsync<InvalidVendorException>(async () => await _vendorService.ToggleVendorStatus(1));
        }

        [Test]
        public async Task ToggleVendorStatus_ShouldToggleStatus_WhenValid()
        {
           
            var vendor = new Vendor { Id = 100 };
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(vendor);
            _mockVendorRepo.Setup(r => r.ToggleVendorStatus(100)).ReturnsAsync(vendor);
            _mockMapper.Setup(m => m.Map<VendorStatusResponse>(vendor)).Returns(new VendorStatusResponse());

            
            var result = await _vendorService.ToggleVendorStatus(1);

            
            Assert.That(result, Is.Not.Null);
            _mockVendorRepo.Verify(r => r.ToggleVendorStatus(100), Times.Once);
        }

        [Test]
        public void GetVendorByUserId_ShouldThrowException_WhenNotFound()
        {
           
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync((Vendor?)null);

            Assert.ThrowsAsync<InvalidVendorException>(async () => await _vendorService.GetVendorByUserId(1));
        }

        [Test]
        public void GetVendorById_ShouldThrowException_WhenNotFound()
        {
           
            _mockVendorRepo.Setup(r => r.GetById(1)).ReturnsAsync((Vendor?)null);

            Assert.ThrowsAsync<InvalidVendorException>(async () => await _vendorService.GetVendorById(1));
        }

        [Test]
        public void GetVendorBasicById_ShouldThrowException_WhenNotFound()
        {
           
            _mockVendorRepo.Setup(r => r.GetById(1)).ReturnsAsync((Vendor?)null);

            Assert.ThrowsAsync<InvalidVendorException>(async () => await _vendorService.GetVendorBasicById(1));
        }

        [Test]
        public void GetVendorByStoreName_ShouldThrowException_WhenNotFound()
        {
           
            _mockVendorRepo.Setup(r => r.GetByStoreName("store")).ReturnsAsync((ICollection<Vendor>?)null);

            Assert.ThrowsAsync<InvalidVendorException>(async () => await _vendorService.GetVendorByStoreName("store"));
        }

        [Test]
        public void GetVendorsByStatus_ShouldThrowException_WhenStatusInvalid()
        {
            Assert.ThrowsAsync<InvalidVendorException>(async () => await _vendorService.GetVendorsByStatus("invalid"));
        }

        [Test]
        public async Task GetVendorsByStatus_ShouldReturnList_WhenStatusValid()
        {
           
            var list = new List<Vendor> { new Vendor() };
            _mockVendorRepo.Setup(r => r.GetVendorsByStatus(VendorStatus.Pending)).ReturnsAsync(list);
            _mockMapper.Setup(m => m.Map<ICollection<VendorProfileResponse>>(list)).Returns(new List<VendorProfileResponse> { new VendorProfileResponse() });

            
            var result = await _vendorService.GetVendorsByStatus("pending");

            
            Assert.That(result, Has.Count.EqualTo(1));
        }

        [Test]
        public void ApproveVendor_ShouldThrowException_WhenVendorNotFound()
        {
           
            _mockVendorRepo.Setup(r => r.GetById(1)).ReturnsAsync((Vendor?)null);

            Assert.ThrowsAsync<InvalidVendorException>(async () => await _vendorService.ApproveVendor(1));
        }

        [Test]
        public async Task ApproveVendor_ShouldApproveVendor_WhenValid()
        {
           
            var vendor = new Vendor { Id = 100, UserId = 5 };
            _mockVendorRepo.Setup(r => r.GetById(100)).ReturnsAsync(vendor);

            var user = new User { Id = 5, Role = UserRole.Customer };
            _mockUserRepo.Setup(r => r.GetById(5)).ReturnsAsync(user);
            _mockUserRepo.Setup(r => r.Update(5, user)).ReturnsAsync(user);
            _mockVendorRepo.Setup(r => r.Update(100, vendor)).ReturnsAsync(vendor);
            _mockMapper.Setup(m => m.Map<VendorProfileResponse>(vendor)).Returns(new VendorProfileResponse { Id = 100 });

            
            var result = await _vendorService.ApproveVendor(100);

            
            Assert.That(result.Id, Is.EqualTo(100));
            Assert.That(vendor.Status, Is.EqualTo(VendorStatus.Approved));
            Assert.That(user.Role, Is.EqualTo(UserRole.Vendor));
        }

        [Test]
        public void CancelVendor_ShouldThrowException_WhenVendorNotFound()
        {
           
            _mockVendorRepo.Setup(r => r.GetById(1)).ReturnsAsync((Vendor?)null);

            Assert.ThrowsAsync<InvalidVendorException>(async () => await _vendorService.CancelVendor(1));
        }

        [Test]
        public async Task CancelVendor_ShouldCancelVendor_WhenValid()
        {
           
            var vendor = new Vendor { Id = 100, UserId = 5 };
            _mockVendorRepo.Setup(r => r.GetById(100)).ReturnsAsync(vendor);

            var user = new User { Id = 5, Role = UserRole.Vendor };
            _mockUserRepo.Setup(r => r.GetById(5)).ReturnsAsync(user);
            _mockUserRepo.Setup(r => r.Update(5, user)).ReturnsAsync(user);
            _mockVendorRepo.Setup(r => r.Update(100, vendor)).ReturnsAsync(vendor);
            _mockMapper.Setup(m => m.Map<VendorProfileResponse>(vendor)).Returns(new VendorProfileResponse { Id = 100 });

            
            var result = await _vendorService.CancelVendor(100);

            
            Assert.That(result.Id, Is.EqualTo(100));
            Assert.That(vendor.Status, Is.EqualTo(VendorStatus.Cancelled));
            Assert.That(vendor.IsActive, Is.False);
            Assert.That(user.Role, Is.EqualTo(UserRole.Customer));
        }

        [Test]
        public async Task GetVendorByUserId_And_GetVendorById_And_StoreName_GetOverall()
        {
           
            var vendor = new Vendor { Id = 10, User = new User { FullName = "Name" } };
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(vendor);
            _mockVendorRepo.Setup(r => r.GetById(10)).ReturnsAsync(vendor);
            
            var list = new List<Vendor> { vendor };
            _mockVendorRepo.Setup(r => r.GetByStoreName("store")).ReturnsAsync(list);

            _mockVendorRepo.Setup(r => r.GetPagedVendors("search", 1, 10)).ReturnsAsync((list, 1));

            var expected = new VendorProfileResponse { Id = 10 };
            var basic = new VendorBasicResponse { StoreName = "store" };
            _mockMapper.Setup(m => m.Map<VendorProfileResponse>(vendor)).Returns(expected);
            _mockMapper.Setup(m => m.Map<VendorBasicResponse>(vendor)).Returns(basic);
            _mockMapper.Setup(m => m.Map<ICollection<VendorProfileResponse>>(list)).Returns(new List<VendorProfileResponse> { expected });
            _mockMapper.Setup(m => m.Map<List<VendorProfileResponse>>(list)).Returns(new List<VendorProfileResponse> { expected });

            var byUser = await _vendorService.GetVendorByUserId(1);
            Assert.That(byUser?.Id, Is.EqualTo(10));

            var byId = await _vendorService.GetVendorById(10);
            Assert.That(byId?.Id, Is.EqualTo(10));

            var basicResult = await _vendorService.GetVendorBasicById(10);
            Assert.That(basicResult?.StoreName, Is.EqualTo("store"));

            var byName = await _vendorService.GetVendorByStoreName("Store");
            Assert.That(byName.First().Id, Is.EqualTo(10));

            var paged = await _vendorService.GetAllVendors(new PageRequest { PageNumber = 1, PageSize = 10, SearchTerm = "search" });
            Assert.That(paged.Items.First().Id, Is.EqualTo(10));
        }

        [Test]
        public void UpdateVendor_ShouldThrowException_WhenDatabaseSaveFails()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            var vendor = new Vendor { Id = 10, User = new User { FullName = "Old Name" } };
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(vendor);

            _mockUserRepo.Setup(r => r.SaveChangesAsync()).ThrowsAsync(new Exception("DB Save Error"));

            var request = new UpdateVendorRequest { FullName = "New Name" };

            var ex = Assert.ThrowsAsync<InvalidVendorException>(async () => await _vendorService.UpdateVendor(request));
            Assert.That(ex.Message, Contains.Substring("Failed to update user information"));
        }

        [Test]
        public async Task GetVendorsByStatus_ShouldReturnApprovedVendors_WhenStatusApproved()
        {
           
            var list = new List<Vendor> { new Vendor() };
            _mockVendorRepo.Setup(r => r.GetVendorsByStatus(VendorStatus.Approved)).ReturnsAsync(list);
            _mockMapper.Setup(m => m.Map<ICollection<VendorProfileResponse>>(list)).Returns(new List<VendorProfileResponse> { new VendorProfileResponse() });

            
            var result = await _vendorService.GetVendorsByStatus("approved");

            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task GetVendorsByStatus_ShouldReturnCancelledVendors_WhenStatusCancelled()
        {
           
            var list = new List<Vendor> { new Vendor() };
            _mockVendorRepo.Setup(r => r.GetVendorsByStatus(VendorStatus.Cancelled)).ReturnsAsync(list);
            _mockMapper.Setup(m => m.Map<ICollection<VendorProfileResponse>>(list)).Returns(new List<VendorProfileResponse> { new VendorProfileResponse() });

            
            var result = await _vendorService.GetVendorsByStatus("cancelled");

            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void VerifyGSTUnique_ShouldThrowException_WhenGSTLengthInvalid()
        {
           
            var method = typeof(VendorService).GetMethod("VerifyGSTUnique", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var ex = Assert.ThrowsAsync<InvalidVendorException>(async () =>
            {
                var task = method?.Invoke(_vendorService, new object[] { "GST123" }) as Task<bool>;
                await task!;
            });
        }

        [Test]
        public async Task VerifyGSTUnique_ShouldCallRepository_WhenGSTLengthValid()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(new Vendor { Id = 10 });
            _mockVendorRepo.Setup(r => r.VerifyGSTUnique("GST123456789012", 10)).ReturnsAsync(true);

            var method = typeof(VendorService).GetMethod("VerifyGSTUnique", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = method?.Invoke(_vendorService, new object[] { "GST123456789012" }) as Task<bool>;

            
            var result = await task!;

            
            Assert.That(result, Is.True);
            _mockVendorRepo.Verify(r => r.VerifyGSTUnique("GST123456789012", 10), Times.Once);
        }

        [Test]
        public void VerifyPANUnique_ShouldThrowException_WhenPANLengthInvalid()
        {
           
            var method = typeof(VendorService).GetMethod("VerifyPANUnique", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var ex = Assert.ThrowsAsync<InvalidVendorException>(async () =>
            {
                var task = method?.Invoke(_vendorService, new object[] { "PAN123" }) as Task<bool>;
                await task!;
            });
        }

        [Test]
        public async Task VerifyPANUnique_ShouldCallRepository_WhenPANLengthValid()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(1);
            _mockVendorRepo.Setup(r => r.GetByUserId(1)).ReturnsAsync(new Vendor { Id = 10 });
            _mockVendorRepo.Setup(r => r.VerifyPANUnique("PAN1234567", 10)).ReturnsAsync(true);

            var method = typeof(VendorService).GetMethod("VerifyPANUnique", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var task = method?.Invoke(_vendorService, new object[] { "PAN1234567" }) as Task<bool>;

            
            var result = await task!;

            
            Assert.That(result, Is.True);
            _mockVendorRepo.Verify(r => r.VerifyPANUnique("PAN1234567", 10), Times.Once);
        }

        [Test]
        public async Task CreateVendor_ShouldHandleNullStoreEmail()
        {
           
            _mockCurrentUser.Setup(u => u.UserId).Returns(5);
            _mockVendorRepo.Setup(r => r.VerifyGSTUnique("GST123456789012")).ReturnsAsync(true);
            _mockVendorRepo.Setup(r => r.VerifyPANUnique("PAN1234567")).ReturnsAsync(true);

            var vendor = new Vendor { UserId = 5, StoreEmail = null };
            _mockMapper.Setup(m => m.Map<Vendor>(It.IsAny<CreateVendorRequest>())).Returns(vendor);
            _mockVendorRepo.Setup(r => r.Create(vendor)).ReturnsAsync(vendor);
            _mockUserRepo.Setup(r => r.GetById(5)).ReturnsAsync(new User());

            var expected = new VendorProfileResponse { Id = 100 };
            _mockMapper.Setup(m => m.Map<VendorProfileResponse>(vendor)).Returns(expected);

            var request = new CreateVendorRequest { GSTNumber = "GST123456789012", PANNumber = "PAN1234567", StoreName = "Store", StoreEmail = null };

            
            var result = await _vendorService.CreateVendor(request);

            
            Assert.That(result.Id, Is.EqualTo(100));
        }

        [Test]
        public void ApproveVendor_ShouldThrowException_WhenChangeUserRoleFails()
        {
           
            var vendor = new Vendor { Id = 100, UserId = 5 };
            _mockVendorRepo.Setup(r => r.GetById(100)).ReturnsAsync(vendor);

            var user = new User { Id = 5, Role = UserRole.Customer };
            _mockUserRepo.Setup(r => r.GetById(5)).ReturnsAsync(user);
            _mockUserRepo.Setup(r => r.Update(5, user)).ReturnsAsync((User?)null);

            Assert.ThrowsAsync<InvalidOperationException>(async () => await _vendorService.ApproveVendor(100));
        }

        [Test]
        public void ApproveVendor_ShouldThrowException_WhenUpdateReturnsNull()
        {
           
            var vendor = new Vendor { Id = 100, UserId = 5 };
            _mockVendorRepo.Setup(r => r.GetById(100)).ReturnsAsync(vendor);

            var user = new User { Id = 5, Role = UserRole.Customer };
            _mockUserRepo.Setup(r => r.GetById(5)).ReturnsAsync(user);
            _mockUserRepo.Setup(r => r.Update(5, user)).ReturnsAsync(user);
            _mockVendorRepo.Setup(r => r.Update(100, vendor)).ReturnsAsync((Vendor?)null);

            Assert.ThrowsAsync<InvalidVendorException>(async () => await _vendorService.ApproveVendor(100));
        }

        [Test]
        public void CancelVendor_ShouldThrowException_WhenChangeUserRoleFails()
        {
           
            var vendor = new Vendor { Id = 100, UserId = 5 };
            _mockVendorRepo.Setup(r => r.GetById(100)).ReturnsAsync(vendor);

            var user = new User { Id = 5, Role = UserRole.Vendor };
            _mockUserRepo.Setup(r => r.GetById(5)).ReturnsAsync(user);
            _mockUserRepo.Setup(r => r.Update(5, user)).ReturnsAsync((User?)null);

            Assert.ThrowsAsync<InvalidOperationException>(async () => await _vendorService.CancelVendor(100));
        }

        [Test]
        public void CancelVendor_ShouldThrowException_WhenUpdateReturnsNull()
        {
           
            var vendor = new Vendor { Id = 100, UserId = 5 };
            _mockVendorRepo.Setup(r => r.GetById(100)).ReturnsAsync(vendor);

            var user = new User { Id = 5, Role = UserRole.Vendor };
            _mockUserRepo.Setup(r => r.GetById(5)).ReturnsAsync(user);
            _mockUserRepo.Setup(r => r.Update(5, user)).ReturnsAsync(user);
            _mockVendorRepo.Setup(r => r.Update(100, vendor)).ReturnsAsync((Vendor?)null);

            Assert.ThrowsAsync<InvalidVendorException>(async () => await _vendorService.CancelVendor(100));
        }

        [Test]
        public void ChangeUserRole_ShouldThrowKeyNotFoundException_WhenUserNotFound()
        {
           
            var vendor = new Vendor { Id = 100, UserId = 5 };
            _mockVendorRepo.Setup(r => r.GetById(100)).ReturnsAsync(vendor);
            _mockUserRepo.Setup(r => r.GetById(5)).ReturnsAsync((User?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await _vendorService.ApproveVendor(100));
        }

        [Test]
        public async Task GetAllVendors_ShouldHandleNullSearchTerm()
        {
           
            var list = new List<Vendor> { new Vendor() };
            _mockVendorRepo.Setup(r => r.GetPagedVendors("", 1, 10)).ReturnsAsync((list, 1));
            _mockMapper.Setup(m => m.Map<List<VendorProfileResponse>>(list)).Returns(new List<VendorProfileResponse> { new VendorProfileResponse() });

            var query = new PageRequest { PageNumber = 1, PageSize = 10, SearchTerm = null };

            
            var result = await _vendorService.GetAllVendors(query);

            
            Assert.That(result, Is.Not.Null);
            _mockVendorRepo.Verify(r => r.GetPagedVendors("", 1, 10), Times.Once);
        }
    }
}
