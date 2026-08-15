using System;
using System.Collections.Generic;

namespace HybridWash.Services.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Tier { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class UserListDto
    {
        public List<UserDto> Admins { get; set; } = new();
        public List<UserDto> Staffs { get; set; } = new();
        public List<UserDto> Customers { get; set; } = new();
    }
}
