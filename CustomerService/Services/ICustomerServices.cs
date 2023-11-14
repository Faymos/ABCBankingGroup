using CustomerService.Models;

namespace CustomerService.Services
{
    public interface ICustomerServices
    {
        Task<ResponseData> AllPendingUser();
        Task<ResponseData> AllPendingAdminUser();
        Task<Admin> GetAdminbyEmail(string email);
        Task<bool> ApprovePendingAdminUser(long id, bool IsApprove, string email);
        Task<bool> ApprovePendingUser(long id, bool IsApprove, string email);
    }
}
