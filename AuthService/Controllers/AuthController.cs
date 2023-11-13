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
    [EnableCors]
    public class AuthController : ControllerBase
    {
        private readonly IAuths _auths;
        public AuthController(IAuths auths)
        {
            _auths = auths;

        }

        [HttpPost("SignUp")]
        public async Task<ResponseData> SignUp([FromBody] Customer user)
        {
            return await _auths.Signup(user);
        }

        [HttpPost("Login")]
        public async Task<ResponseData> Login(Login login)
        {
            return await _auths.login(login);
        }

        [HttpPost("Admin")]
        public async Task<ResponseData> Admin([FromBody] Login login)
        {
            return await _auths.Admin(login);
        }

        [HttpGet("GetAllPendingApproval")]
        [Authorize]
        public async Task<ResponseData> GetAllPendingApproval()
        {
            return await _auths.AllPendingUser();
        }

        [HttpGet("GetAllPendingAdmin")]
        [Authorize]
        public async Task<ResponseData> GetAllPendingAdmin()
        {
            return await _auths.AllPendingAdminUser();
        }

        [HttpPost("ApprovePendingAdmin/{id}")]
        [Authorize]
        public async Task<ResponseData> ApprovePendingAdmin(long id, bool isApprove)
        {
            string email = this.User.Claims.ToList()[0].Value;
            Admin adminuser = await _auths.GetAdminbyEmail(email.Trim());
            if (adminuser != null && adminuser.RoleId == Role.SuperAdmin)
            {
                 if(await _auths.ApprovePendingAdminUser(id, isApprove, email))
                {
                    return new ResponseData()
                    {
                        Status = HttpStatusCode.OK,
                        ResponseMessage = "Successful"
                    };
                }

                return new ResponseData()
                {
                    Status = HttpStatusCode.BadRequest,
                    ResponseMessage = "Failed"
                };
            }

            return new ResponseData()
            {
                Status = HttpStatusCode.Unauthorized,
                ResponseMessage = "You Did not have permission to Approve"
            };
           
        }

        [HttpPost("ApprovePendingUser/{id}")]
        [Authorize]
        public async Task<ResponseData> ApprovePendingUser(long id, bool isApprove)
        {
            string email = this.User.Claims.ToList()[0].Value;
            Admin adminuser = await _auths.GetAdminbyEmail(email.Trim());
            if (adminuser != null && adminuser.RoleId == Role.SuperAdmin)
            {
                if (await _auths.ApprovePendingUser(id, isApprove, email))
                {
                    return new ResponseData()
                    {
                        Status = HttpStatusCode.OK,
                        ResponseMessage = "Successful"
                    };
                }

                return new ResponseData()
                {
                    Status = HttpStatusCode.BadRequest,
                    ResponseMessage = "Failed"
                };
            }

            return new ResponseData()
            {
                Status = HttpStatusCode.Unauthorized,
                ResponseMessage = "You Did not have permission to Approve"
            };
            
        }
    }
}