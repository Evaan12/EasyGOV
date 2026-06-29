using System;

namespace Application.Features.Users.DTOs
{
    public record UserSearchDto(Guid Id, string FullName, string Email);
}