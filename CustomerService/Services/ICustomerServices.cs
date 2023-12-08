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
        Task<ResponseData> AllApprovedCustomerAccount(); 
        Task<ResponseData> CustomerAccount(int id);
        Task<ResponseData> DeleteCustomerAccount(int id);
        Task<ResponseData> DeActivateCustomerAccount(int id);
        Task<ResponseData> ReActivateCustomerAccount(int id);
        Task<ResponseData> GetAllCustomer();
    }
}
