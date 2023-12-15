using CustomerService.Entities;
using CustomerService.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Threading.Channels;

namespace CustomerService.Services
{
    public class Customers : ICustomerServices
    {
        private readonly ILogger<Customers> _logger;
        public readonly CustomerContext _context;
        public Customers(CustomerContext context, ILogger<Customers> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ResponseData> AllPendingUser()
        {
            ResponseData responseData = new ResponseData();

            try
            {
                var pendingUsers = await _context.customer
                    .Where(u => u.IsApproved == false)
                    .ToListAsync();

                if (pendingUsers != null)
                {
                    return new ResponseData
                    {
                        Status = HttpStatusCode.OK,
                        ResponseMessage = "Successful",
                        data = pendingUsers
                    };
                }
                return new ResponseData
                {
                    Status = HttpStatusCode.OK,
                    ResponseMessage = "Successful",
                    data = null
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<ResponseData> AllPendingAdminUser()
        {
            ResponseData responseData = new ResponseData();

            try
            {
                var pendingUsers = await _context.admin
                    .Where(u => u.IsApproved == false)
                    .ToListAsync();

                if (pendingUsers != null)
                {
                    return new ResponseData
                    {
                        Status = HttpStatusCode.OK,
                        ResponseMessage = "Successful",
                        data = pendingUsers
                    };
                }
                return new ResponseData
                {
                    Status = HttpStatusCode.OK,
                    ResponseMessage = "Successful",
                    data = null
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<Admin> GetAdminbyEmail(string email)
        {
            ResponseData responseData = new ResponseData();

            try
            {
                var adminuser = await _context.admin
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (adminuser != null)
                {
                    return adminuser;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<bool> ApprovePendingAdminUser(long id, bool IsApprove, string email)
        {
            ResponseData responseData = new();
            try
            {
                var adminUser = await _context.admin.FirstOrDefaultAsync(u => u.Id == id);
                if (adminUser != null)
                {
                    adminUser.IsApproved = IsApprove;
                    adminUser.IsActive = IsApprove;
                    adminUser.ApprovedBy = email;
                    adminUser.DateModified = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> ApprovePendingUser(long id, bool IsApprove, string email)
        {
            ResponseData responseData = new();
            try
            {
                var user1 = await _context.customer.FirstOrDefaultAsync(u => u.Id == id);
                if (user1 != null)
                {
                    user1.IsApproved = IsApprove;
                    user1.IsActive = IsApprove;
                    user1.ApprovedBy = email;
                    user1.DateModified = DateTime.UtcNow;
                    user1.Status = IsApprove? "Approved" : "Rejected";

                    await _context.SaveChangesAsync();
                   if(IsApprove)
                    {
                        await CreateCustomerAccount(user1);
                        //send login mail to the customer after the approval
                        await NotifyCustomer(user1);
                    }



                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> CreateCustomerAccount(Customer user)
        {
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "sp_CustomerAccount";
                    command.CommandType = CommandType.StoredProcedure;
                    
                    // Add parameters
                    command.Parameters.AddWithValue("@Status", 1);
                    command.Parameters.AddWithValue("@CustomerId", user.Id); 
                    command.Parameters.AddWithValue("@CustomerName", user.FirstName + " " + user.Surname);                   
                    command.Parameters.AddWithValue("@CustomerPhoneNumber", user.PhoneNumber);

                    try
                    {
                        await connection.OpenAsync();
                        var result = await command.ExecuteNonQueryAsync();
                        return result > 0;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"An error occurred: {ex.Message}");
                        return false;
                    }

                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it in a meaningful way
                _logger.LogError($"An error occurred: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> NotifyCustomer(Customer customer)
        {
            try
            {
                var customerAccount = await _context.customerAccount.FirstOrDefaultAsync(u => u.CustomerId == customer.Id);

                if (customerAccount != null)
                {
                    string fromEmail = "abcbankinggroupltd@gmail.com";
                    string password = "qlyd qpuq msno exxn";
                    string toEmail = customer.Email.Trim();

                    // Create the MailMessage object
                    using (MailMessage mailMessage = new MailMessage(fromEmail, toEmail)
                    {
                        Subject = $"Welcome To ABC BANKING GROUP",
                        Body = $" Hello {customer.FirstName}, We are happy to welcome you to ABC banking group, your account has been approved and the AccountNumber generated for is {customerAccount.AccountNumber} and AccountName is {customerAccount.AccountName}. You can now log in to your account using the username and password that were created during onboarding. Thanks",
                        IsBodyHtml = true
                    })
                    {

                        // Create the SmtpClient
                        using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com"))
                        {
                            smtpClient.Port = 587;
                            smtpClient.Credentials = new NetworkCredential(fromEmail, password);
                            smtpClient.EnableSsl = true;

                            try
                            {
                                // Send the email
                                await smtpClient.SendMailAsync(mailMessage);
                                return true;
                            }
                            catch (Exception ex)
                            {
                                // Log the exception using a logging library
                                Console.WriteLine($"Error sending email: {ex.Message}");
                            }
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                // Log the exception using a logging library
                throw ex;
            }
        }

        public async Task<ResponseData> AllApprovedCustomerAccount()
        {
            
            try
            {
                var allUsers = await _context.customerAccount
                    .ToListAsync();

                return new ResponseData
                {
                    Status = HttpStatusCode.OK,
                    ResponseMessage = "Successful",
                    data = allUsers
                };
            }
            catch (Exception ex)
            {
              
                return new ResponseData
                {
                    Status = HttpStatusCode.InternalServerError,
                    ResponseMessage = $"An error occurred {ex.Message}",
                    data = null
                };
            }

        }

        public async Task<ResponseData> CustomerAccount(int id)
        {
            ResponseData responseData = new ResponseData();

            try
            {
                var pendingUsers = await _context.customerAccount
                    .FirstOrDefaultAsync(u => u.CustomerId == id);
                   

                if (pendingUsers != null)
                {
                    return new ResponseData
                    {
                        Status = HttpStatusCode.OK,
                        ResponseMessage = "Successful",
                        data = pendingUsers
                    };
                }
                return new ResponseData
                {
                    Status = HttpStatusCode.OK,
                    ResponseMessage = "Successful",
                    data = null
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task<ResponseData> DeleteCustomerAccount(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "sp_CustomerAccount";
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@Status", 3);
                    command.Parameters.AddWithValue("@CustomerId", id);               

                        await connection.OpenAsync();
                        var result = await command.ExecuteNonQueryAsync();
                        if(result > 0)
                        {
                            return new ResponseData()
                            {
                                Status = HttpStatusCode.OK,
                                ResponseMessage = "Account Deleted Successfully"
                            };
                        }
                    return new ResponseData()
                    {
                        Status = HttpStatusCode.NotFound,
                        ResponseMessage = "Failed to delete account"
                    };
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it in a meaningful way
                _logger.LogError($"An error occurred: {ex.Message}");
              
                return new ResponseData()
                {
                    Status = HttpStatusCode.BadGateway,
                    ResponseMessage = $"failed {ex.Message}"
                };
            }
        }
        public async Task<ResponseData> DeActivateCustomerAccount(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "sp_CustomerAccount";
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@Status", 4);
                    command.Parameters.AddWithValue("@CustomerId", id);

                    await connection.OpenAsync();
                    var result = await command.ExecuteNonQueryAsync();
                    if (result > 0)
                    {
                        return new ResponseData()
                        {
                            Status = HttpStatusCode.OK,
                            ResponseMessage = "Account Deactivated Successfully"
                        };
                    }
                    return new ResponseData()
                    {
                        Status = HttpStatusCode.NotFound,
                        ResponseMessage = "Fail to Deactivate account"
                    };
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it in a meaningful way
                _logger.LogError($"An error occurred: {ex.Message}");

                return new ResponseData()
                {
                    Status = HttpStatusCode.BadGateway,
                    ResponseMessage = $"failed {ex.Message}"
                };
            }
        }
        public async Task<ResponseData> ReActivateCustomerAccount(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "sp_CustomerAccount";
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@Status", 5);
                    command.Parameters.AddWithValue("@CustomerId", id);

                    await connection.OpenAsync();
                    var result = await command.ExecuteNonQueryAsync();
                    if (result > 0)
                    {
                        return new ResponseData()
                        {
                            Status = HttpStatusCode.OK,
                            ResponseMessage = "Account Activated Successfully "
                        };
                    }
                    return new ResponseData()
                    {
                        Status = HttpStatusCode.NotFound,
                        ResponseMessage = "Failed to re activate account "
                    };
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it in a meaningful way
                _logger.LogError($"An error occurred: {ex.Message}");

                return new ResponseData()
                {
                    Status = HttpStatusCode.BadGateway,
                    ResponseMessage = $"failed {ex.Message}"
                };
            }
        }
        public async Task<ResponseData> GetAllCustomer()
        {

            try
            {
                var allUsers = await _context.customer
                    .ToListAsync();

                return new ResponseData
                {
                    Status = HttpStatusCode.OK,
                    ResponseMessage = "Successful",
                    data = allUsers
                };
            }
            catch (Exception ex)
            {

                return new ResponseData
                {
                    Status = HttpStatusCode.InternalServerError,
                    ResponseMessage = $"An error occurred {ex.Message}",
                    data = null
                };
            }
        }

        public async Task<ResponseData> Summary()
        {
            try
            {
                using (var connection = new SqlConnection(_context.Database.GetConnectionString()))
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "[dbo].[Sp_CustmerDetails]";
                    command.CommandType = CommandType.StoredProcedure;

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var summary = new Summary
                            {
                                TotalUser = reader.GetString(reader.GetOrdinal("TotalUser")),
                                TotalBalance = reader.GetDecimal(reader.GetOrdinal("TotalBalance")),
                                TotalCredit = reader.GetDecimal(reader.GetOrdinal("TotalCredit")),
                                TotalDebit = reader.GetDecimal(reader.GetOrdinal("TotalDebit")),
                                TotalOverDraft = reader.GetDecimal(reader.GetOrdinal("TotalOverDraft")),
                            };

                            return new ResponseData
                            {
                                Status = HttpStatusCode.OK,
                                ResponseMessage = "Successful",
                                data = summary
                            };
                        }
                        else
                        {
                            return new ResponseData
                            {
                                Status = HttpStatusCode.OK,
                                ResponseMessage = "No record at this time"
                               
                            };
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL error occurred: {sqlEx.Message}. Procedure: [dbo].[Sp_CustmerDetails]");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                throw;
            }
        }

    }
}