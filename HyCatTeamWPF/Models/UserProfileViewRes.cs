using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HyCatTeamWPF.Models
{
    public class UserProfileViewRes
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? Sex { get; set; }
        public DateTimeOffset? DateOfBirth { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Phone { get; set; }
        public string? LicenseUrl { get; set; }
        public string? CitizenUrl { get; set; }
        public RoleViewRes? Role { get; set; }
        public bool NeedSetPassword { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
