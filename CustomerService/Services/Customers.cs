﻿using CustomerService.Entities;
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
                    command.CommandText = "InsertCustomerAccount";
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

    }
}