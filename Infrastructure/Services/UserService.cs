using Application.Common.Pagination;
using Application.Features.Users.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Events;
using Domain.Exceptions;
using Domain.Repositories;
using Domain.ValueObjects;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ITenantRepository _tenantRepository;
        private readonly ICitizenProfileRepository _citizenProfileRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly ITenantService _tenantService;
        private readonly TimeProvider _timeProvider;

        public UserService(
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            ITenantRepository tenantRepository,
            ICitizenProfileRepository citizenProfileRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            ITenantService tenantService,
            TimeProvider timeProvider)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tenantRepository = tenantRepository;
            _citizenProfileRepository = citizenProfileRepository;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _tenantService = tenantService;
            _timeProvider = timeProvider;
        }

        public async Task<PagedResult<UserSearchDto>> SearchUsersAsync(string searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _userManager.Users.AsNoTracking();

            if (_currentUser.TenantType != TenantType.Central)
            {
                var allowedTenantIds = await _tenantService.GetAllowedTenantIdsAsync(_currentUser.TenantId, cancellationToken);
                query = query.Where(u => allowedTenantIds.Contains(u.TenantId));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(searchTerm) || (u.Email != null && u.Email.ToLower().Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var users = await query
                .OrderBy(u => u.FullName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserSearchDto(u.Id, u.FullName, u.Email ?? string.Empty))
                .ToListAsync(cancellationToken);

            return new PagedResult<UserSearchDto>(users, totalCount, pageNumber, pageSize);
        }

        public async Task SuspendUserAsync(Guid userId, TimeSpan duration, CancellationToken cancellationToken = default)
        {
            var user = await GetUserEnsuringSecurityAsync(userId);

            user.SuspensionEndDate = _timeProvider.GetUtcNow().Add(duration);
            user.LockoutEnd = user.SuspensionEndDate;

            user.AddDomainEvent(new UserSuspendedEvent(userId, duration));

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new DomainException("An error occurred while attempting to suspend the user.");
        }

        public async Task BanUserAsync(Guid userId, string reason, CancellationToken cancellationToken = default)
        {
            var user = await GetUserEnsuringSecurityAsync(userId);

            user.IsBanned = true;
            user.BanReason = reason;
            user.LockoutEnd = DateTimeOffset.MaxValue;

            user.AddDomainEvent(new UserBannedEvent(userId, reason));

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new DomainException("An error occurred while attempting to ban the user.");
        }

        public async Task AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        {
            var user = await GetUserEnsuringSecurityAsync(userId);

            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null) throw new DomainException("Role not found.");

            if (_currentUser.TenantType != TenantType.Central && role.TenantId != _currentUser.TenantId)
                throw new DomainException("Cannot assign a role that does not strictly belong to your operational tier framework.");

            if (!await _userManager.IsInRoleAsync(user, role.Name!))
            {
                var assignResult = await _userManager.AddToRoleAsync(user, role.Name!);
                if (!assignResult.Succeeded) throw new DomainException("Failed to assign role to the selected user.");

                user.AddDomainEvent(new UserRoleAssignedEvent(userId, role.Id));
                await _userManager.UpdateAsync(user);
            }
        }

        public async Task<Guid> CreateTenantAdminAsync(string email, string fullName, string password, Guid tenantId, Guid roleId, CancellationToken cancellationToken = default)
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant == null) throw new DomainException("Target tenant does not exist.");

            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null) throw new DomainException("Role not found.");

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                TenantId = tenantId,
                TenantType = tenant.TenantType,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new DomainException($"Failed to create admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await _userManager.AddToRoleAsync(user, role.Name!);

            tenant.MarkAdminAssigned(_currentUser.UserId);

            await _tenantRepository.UpdateAsync(tenant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _unitOfWork.ClearTracker();

            return user.Id;
        }

        public async Task<Guid> RegisterPublicCitizenAsync(string email, string fullName, string password, string registrationId, string phoneNumber, CancellationToken cancellationToken = default)
        {
            var ward = await _tenantRepository.GetByRegistrationIdAsync(registrationId, cancellationToken);

            if (ward == null || !ward.IsActivated)
                throw new DomainException("Invalid or inactive registration link. Public registration is bound exclusively to Active Wards.");

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                TenantId = ward.Id,
                TenantType = TenantType.Ward
            };

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                    throw new DomainException($"Failed to register citizen: {string.Join(", ", result.Errors.Select(e => e.Description))}");

                // Citizens no longer require specific security role bindings per requirement
                var profile = new CitizenProfile(user.Id, fullName, DateTime.UtcNow.AddYears(-18), Gender.NotSpecified, new PhoneNumber(phoneNumber), ward.Id, user.Id);

                await _citizenProfileRepository.AddAsync(profile, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            return user.Id;
        }

        public async Task<(string FullName, string PrimaryRole)> GetUserDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new DomainException("User record not found in the directory.");

            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "Citizen";

            return (user.FullName, primaryRole);
        }

        private async Task<AppUser> GetUserEnsuringSecurityAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new DomainException("User record not found in the directory.");

            if (user.IsDefault == true)
                throw new DomainException("This user is a protected system entity and cannot be managed operationally.");

            if (_currentUser.TenantType != TenantType.Central)
            {
                var allowedTenantIds = await _tenantService.GetAllowedTenantIdsAsync(_currentUser.TenantId);

                if (!allowedTenantIds.Contains(user.TenantId))
                    throw new DomainException("Security Violation: Target user exists outside of your administrative tier.");
            }

            return user;
        }
    }
}
