using CustomerService.Models;
using CustomerService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CustomerService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [EnableCors]
    public class CustomerController : ControllerBase
    {
       
        private readonly ILogger<CustomerController> _logger;
        private readonly ICustomerServices _customerServices;

        public CustomerController(ILogger<CustomerController> logger, ICustomerServices customer)
        {
            _logger = logger;
            _customerServices = customer;
        }

        [HttpGet("GetAllPendingApproval")]
        [Authorize]
        public async Task<ResponseData> GetAllPendingApproval()
        {
            return await _customerServices.AllPendingUser();
        }

        [HttpGet("GetAllPendingAdmin")]
        [Authorize]
        public async Task<ResponseData> GetAllPendingAdmin()
        {
            return await _customerServices.AllPendingAdminUser();
        }

        [HttpGet("AllApprovedCustomerAccount")]
        [Authorize]
        public async Task<ResponseData> AllApprovedCustomerAccount()
        {
            return await _customerServices.AllApprovedCustomerAccount();
        }

        [HttpPost("ApprovePendingAdmin/{id}")]
        [Authorize]
        public async Task<ResponseData> ApprovePendingAdmin(long id, bool isApprove)
        {
            string email = this.User.Claims.ToList()[0].Value;
            Admin adminuser = await _customerServices.GetAdminbyEmail(email.Trim());
            if (adminuser != null && adminuser.RoleId == Role.SuperAdmin)
            {
                if (await _customerServices.ApprovePendingAdminUser(id, isApprove, email))
                {
                    return new ResponseData()
                    {
                        Status = HttpStatusCode.OK,
                        ResponseMessage = isApprove ? "User Account Approved" : "User Account Disapproved"
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
            Admin adminuser = await _customerServices.GetAdminbyEmail(email.Trim());
            if (adminuser != null && adminuser.RoleId == Role.SuperAdmin)
            {
                if (await _customerServices.ApprovePendingUser(id, isApprove, email))
                {
                    return new ResponseData()
                    {
                        Status = HttpStatusCode.OK,
                        ResponseMessage = isApprove ?  "User Account Approved" : "User Account Disapproved"
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
        
        [HttpGet("GetAccountDetails/{customerid}")]
        [Authorize]
        public async Task<ResponseData> GetAccountDetails(int customerid)
        {
            return await _customerServices.CustomerAccount(customerid);
        }
       
        [HttpGet("GetAllCustomer")]
        [Authorize]
        public async Task<ResponseData> GetAllCustomer()
        {
            return await _customerServices.GetAllCustomer();
        }

        [HttpPost("Delete/{customerid}")]
        [Authorize]
        public async Task<ResponseData> DeleteCustomer(int customerid)
        {
            return await _customerServices.DeleteCustomerAccount(customerid);
        }

        [HttpPost("DeActivateCustomerAccount/{customerid}")]
        [Authorize]
        public async Task<ResponseData> DeActivateCustomerAccount(int customerid)
        {
            return await _customerServices.DeActivateCustomerAccount(customerid);
        }

        [HttpPost("ReActivateCustomerAccount/{customerid}")]
        [Authorize]
        public async Task<ResponseData> ReActivateCustomerAccount(int customerid)
        {
            return await _customerServices.ReActivateCustomerAccount(customerid);
        }

        [HttpPost("summary")]
        [Authorize]
        public async Task<ResponseData> Summary()
        {
            return await _customerServices.Summary();
        }
    }
}