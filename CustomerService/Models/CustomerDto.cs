using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;

namespace CustomerService.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string Surname { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string? hashedPassed { get; set; }
        public string? Dob { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public DateTime? DateModified { get; set; } 
        public string? ApprovedBy { get; set;}
        public string? Status { get; set; }
    }
    public class CustomerDto
    {
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? Surname { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Dob { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public string? Password {  get; set; }
    }
    public class Login
    {
        public string Email { get; set;}
        public string Password { get; set; }
    }
}
