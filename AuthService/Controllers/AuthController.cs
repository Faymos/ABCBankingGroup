using AuthService.Auths;
using AuthService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/v1/[Controller]")]
   
    public class AuthController : ControllerBase
    {
        private readonly IAuths _auths;
        public AuthController(IAuths auths)
        {
            _auths = auths;

        }

        [HttpPost("SignUp")]
        public async Task<ResponseData> SignUp([FromBody] Customerdto user)
        {
            return await _auths.Signup(user);
        }

        [HttpPost("Login")]
        public async Task<ResponseData> Login(Login login)
        {
            if(string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Password))
            {
                return new ResponseData()
                {
                    Status = HttpStatusCode.Unauthorized,
                    ResponseMessage = "Invalid input details "
                };
            }
            return await _auths.login(login);
        }

        [HttpPost("Admin")]
        public async Task<ResponseData> Admin([FromBody] Login login)
        {
            if (string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Password))
            {
                return new ResponseData()
                {
                    Status = HttpStatusCode.Unauthorized,
                    ResponseMessage = "Invalid input details "
                };
            }
            return await _auths.Admin(login);
        }
        [HttpPost("changepassword")]
        public async Task<ResponseData> changepassword(string email, string enteredToken, string newPassword)
        {
            return await _auths.ChangePassword(email, enteredToken, newPassword);
        }

        [HttpPost("resetPassword")]
        public async Task<ResponseData> resetPassword(string email)
        {
            return await _auths.ResetPassword(email);
        }
    }
}