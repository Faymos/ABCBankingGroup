using AuthService.Models;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace AuthService.Auths
{
    public interface IAuths
    {
        Task<ResponseData> login(Login login);
        Task<ResponseData> Signup(Customerdto user);
        Task<ResponseData> Admin(Login login);
        Task<ResponseData> ChangePassword(string email, string enteredToken, string newPassword);
        Task<ResponseData> ResetPassword(string email);
    }
}
