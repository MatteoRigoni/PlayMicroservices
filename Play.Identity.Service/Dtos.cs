using System;
using System.ComponentModel.DataAnnotations;

namespace Play.Identity.Service
{
    public record UserDto (Guid id, string Username, string Email, decimal Coins, DateTimeOffset CreatedDate);
    public record UpdateUserDto([Required][EmailAddress] string Email, [Range(0, 1000000)] decimal Coins);
}