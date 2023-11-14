namespace CustomerService.Models
{
    public enum Role
    {
        SuperAdmin =1,
        Admin,
        Supervisor
    }
    public class Admin
    {
        public long Id { get; set; }
        public string? FullName { get; set;}
        public string? Address { get; set;}
        public string? ApprovedBy { get; set;}
        public string? Email { get; set; }
        public string? Branch { get; set;}
        public string? UserName { get; set;}
        public string? HashedPassword { get; set;}
        public Role RoleId { get; set; }
        public bool IsActive { get; set; }
        public bool IsApproved { get; set;}
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime? DateModified { get; set; }
    }

    public class AdminDto
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Branch { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public Role RoleId { get; set; }
    }
}
