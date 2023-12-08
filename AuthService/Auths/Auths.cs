using AuthService.Entities;
using AuthService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Auths
{
    public class Auths : IAuths
    {

        private readonly string secretKey;
        private readonly string issuer;
        private readonly string audience;
        private readonly AuthContext _authContext;
        private readonly IConfiguration _configuration;

        public Auths(IConfiguration configuration, AuthContext authContext)
        {

            _configuration = configuration;
            secretKey = _configuration["Jwt:Key"];
            issuer = _configuration["Jwt:Issuer"];
            audience = _configuration["Jwt:Audience"];
           _authContext = authContext;
        }

        public string GetAuthToken(string userId)
        {
            //byte[] secretKeyBytes = new byte[32]; // 256 bits
            //using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            //{
            //    rng.GetBytes(secretKeyBytes);
            //}

            //string secretKey1 = Convert.ToBase64String(secretKeyBytes);

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24), // Token expiration time
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        public async Task<ResponseData> login(Login login)
        {
            ResponseData response = new();
            try
            {
                var user1 = await _authContext.customer.SingleOrDefaultAsync(u => u.Email == login.Email && u.IsApproved == true);

                if (user1 != null)
                {
                    if (VerifyPassword(login.Password, user1.hashedPassed))
                    {
                        return new ResponseData
                        {
                            Status = HttpStatusCode.OK,
                            ResponseMessage = "Login Successful",
                            data = user1,
                            Token = GetAuthToken(login.Email)
                        };
                    }
                    else
                    {
                        return new ResponseData
                        {
                            Status = HttpStatusCode.NotFound,
                            ResponseMessage = "invalid email or password "
                        };
                    }
                }
                else
                {
                    return new ResponseData
                    {
                        Status = HttpStatusCode.NotFound,
                        ResponseMessage = "invalid email or password"
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32);

                byte[] hashBytes = new byte[48];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 32);

                return Convert.ToBase64String(hashBytes);
            }

        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);


            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(32);

                for (int i = 0; i < 32; i++)
                {
                    if (hashBytes[i + 16] != hash[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public async Task<ResponseData> Signup(Customerdto user)
        {
            ResponseData response = new();
            try
            {
                string hashedpass = HashPassword(user.Password);
                var execution = _authContext.Database.CreateExecutionStrategy();
                await execution.ExecuteAsync(async () =>
                {
                    using (var dbContextTransaction = _authContext.Database.BeginTransaction())
                    {
                        Customer user1 = new ()
                        {
                            FirstName = user.FirstName,
                            hashedPassed = hashedpass,
                            Surname = user.Surname,
                            Address = user.Address,
                            Dob= user.Dob,
                            Email= user.Email,
                            MiddleName= user.MiddleName,
                            Gender = user.Gender,
                            PhoneNumber = user.PhoneNumber
                        };


                        _authContext.customer.Add(user1);
                        if (await _authContext.SaveChangesAsync() > 0)
                        {
                            dbContextTransaction.Commit();
                        }
                        else
                        {
                            dbContextTransaction.Rollback();
                        }
                    }

                });

                return response = new ResponseData
                {
                    Status = HttpStatusCode.OK,
                    ResponseMessage = $"Customer  created successfully, Kindly wait for approval"
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<ResponseData> AdminSignUp(AdminDto admin)
        {
            ResponseData response = new();
            try
            {
                string hashedpass = HashPassword(admin.Password);
                var execution = _authContext.Database.CreateExecutionStrategy();
                await execution.ExecuteAsync(async () =>
                {
                    using (var dbContextTransaction = _authContext.Database.BeginTransaction())
                    {
                        Admin admin1 = new ()
                        {
                            Address = admin.Address,
                            Branch = admin.Branch,
                            Email = admin.Email,
                            FullName = admin.FullName,
                            UserName = admin.UserName,
                            RoleId = admin.RoleId,
                            
                            HashedPassword = hashedpass
                        };


                        _authContext.admin.Add(admin1);
                        if (await _authContext.SaveChangesAsync() > 0)
                        {
                            dbContextTransaction.Commit();
                        }
                        else
                        {
                            dbContextTransaction.Rollback();
                        }
                    }

                });

                return response = new ResponseData
                {
                    Status = HttpStatusCode.OK,
                    ResponseMessage = $"Record  created successfully, Kindly wait for approval"
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<ResponseData> Admin(Login admin)
        {
            ResponseData response = new ();
            try
            {
                var admin1 = await _authContext.admin.SingleOrDefaultAsync(u => u.Email == admin.Email);

                if (admin1 != null)
                {
                    if(VerifyPassword(admin.Password, admin1.HashedPassword))
                    {
                        return new ResponseData
                        {
                            Status = HttpStatusCode.OK,
                            ResponseMessage = "Login Successful",
                            data = admin1,
                            Token = GetAuthToken(admin.Email)
                        };
                    }
                    else
                    {
                        return new ResponseData
                        {
                            Status = HttpStatusCode.NotFound,
                            ResponseMessage = "invalid email or password "
                        };
                    }
                }
                else
                {
                    return new ResponseData
                    {
                        Status = HttpStatusCode.NotFound,
                        ResponseMessage = "invalid email or password"
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<ResponseData> ChangePassword(string email, string enteredToken, string newPassword)
        {
            try
            {
                if (await VerifyToken(email, enteredToken))
                {
                    string passsword = HashPassword(newPassword);

                    var result = await _authContext.customer.FirstOrDefaultAsync(u => u.Email == email);
                    if (result != null)
                    {
                        result.hashedPassed = passsword;
                        result.DateModified = DateTime.UtcNow;
                        await _authContext.SaveChangesAsync();

                        return new ResponseData()
                        {
                            Status = HttpStatusCode.OK,
                            ResponseMessage = "Password reset successful"
                        };
                    }
                    return new ResponseData()
                    {
                        Status = HttpStatusCode.NotFound,
                        ResponseMessage = "Token verification failed. Password reset aborted. invalid token or email"
                    };
                }
                else
                {
                    return new ResponseData()
                    {
                        Status = HttpStatusCode.NotFound,
                        ResponseMessage = "Token verification failed. Password reset aborted. invalid token or email"
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<ResponseData> ResetPassword(string email)
        {
            var user1 = await _authContext.customer.SingleOrDefaultAsync(u => u.Email == email && u.IsApproved == true);

            if (user1 != null && user1.Email.Contains(email))
            {
                string token = GenerateToken();
                 Tokens tokens = new()
                 {
                     Email = email,
                     Token = token,
                     DateCreated = DateTime.Now,
                     IsActive = true
                 };

                 _authContext.tokens.Add(tokens);
                await _authContext.SaveChangesAsync();
                // send it to email 

                await NotifyCustomer(tokens, user1.FirstName);

                return new ResponseData()
                {
                    Status = HttpStatusCode.OK,
                    ResponseMessage = "Reset code sent to your email"
                };
               
            }
            else
            {
                return new ResponseData()
                {
                    Status = HttpStatusCode.OK,
                    ResponseMessage = "User not found. Password reset aborted."
                }; 
            }
        }

        static string GenerateToken()
        {
            return Guid.NewGuid().ToString();
        }

       public async Task<bool> VerifyToken(string email, string enteredToken)
        {
            var tokenDatabase = await _authContext.tokens.FirstOrDefaultAsync(u => u.Email == email && u.Token == enteredToken && u.IsActive == true);

            if (tokenDatabase != null)
            {
                tokenDatabase.IsActive =false;
                tokenDatabase.DateUpdated = DateTime.Now;
                await _authContext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> NotifyCustomer(Tokens customer, string name)
        {
            try
            {
                string fromEmail = "abcbankinggroupltd@gmail.com";
                string password = "qlyd qpuq msno exxn";
                string toEmail = customer.Email.Trim();

                // Create the MailMessage object
                using (MailMessage mailMessage = new MailMessage(fromEmail, toEmail)
                {
                    Subject = $"Password Reset Token",
                    Body = $" Hello {name}, kindly find the token to change your password {customer.Token}. Thanks",
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
                            throw ex;
                        }
                    }
                }
            }


            catch (Exception ex)
            {
                // Log the exception using a logging library
                throw ex;
            }
        }
    }
}
