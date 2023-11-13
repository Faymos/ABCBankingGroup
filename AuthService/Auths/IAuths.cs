using AuthService.Models;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace AuthService.Auths
{
    public interface IAuths
    {
        Task<ResponseData> login(Login login);
        Task<ResponseData> Signup(Customer user);
        Task<ResponseData> Admin(Login login);
        Task<ResponseData> AllPendingUser();
        Task<ResponseData> AllPendingAdminUser();
        Task<Admin> GetAdminbyEmail(string email);
        Task<bool> ApprovePendingAdminUser(long id, bool IsApprove, string email);
        Task<bool> ApprovePendingUser(long id, bool IsApprove, string email);
    }
}
