using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                var id = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(id, out var guid) ? guid : Guid.Empty;
            }
        }

        public Guid TenantId
        {
            get
            {
                var id = _httpContextAccessor.HttpContext?.User?.FindFirstValue("TenantId");
                return Guid.TryParse(id, out var guid) ? guid : Guid.Empty;
            }
        }

        public TenantType TenantType
        {
            get
            {
                var type = _httpContextAccessor.HttpContext?.User?.FindFirstValue("TenantType");
                return Enum.TryParse<TenantType>(type, out var enumType) ? enumType : TenantType.Central;
            }
        }

        public bool HasUnrestrictedAdmin => 
            _httpContextAccessor.HttpContext?.User?.HasClaim("UnrestrictedAdmin", "true") == true;
    }
}