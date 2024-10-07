using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System;
using User.Management.Service.Models;
using User.Management.Service.Models.Authentication.SignUp;
using Microsoft.AspNetCore.Identity;
using User.Management.Service.Models.Authentication.User;
using System.Security.Claims;
using System.Text;
using User.Management.Service.Models.Authentication.Login;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Security.Policy;
using User.Management.Service.Models.Authentication.ResetPassword;

namespace User.Management.Service.Services
{
    public class UserManagement : IUserManagement
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public UserManagement(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }
        public async Task<ApiResponse<CreateUserResponse>> CreateUserWithTokenAsync(RegisterUser registerUser)
        {
            //Checking if User Exists
            var userExist = await _userManager.FindByEmailAsync(registerUser.Email);
            if (userExist != null)
            {
                return new ApiResponse<CreateUserResponse> { IsSuccess = false, StatusCode = 403, Message = "User already exists :)" };
            }

            //Adding the User to the database
            IdentityUser user = new()
            {
                Email = registerUser.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerUser.Username,
                TwoFactorEnabled = true
            };

            var result = await _userManager.CreateAsync(user, registerUser.Password);
            if (result.Succeeded)
            {
                //Adding Token to Verify the email

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                return new ApiResponse<CreateUserResponse> { IsSuccess = true, StatusCode = 201, Message = "User is created!", Response = new CreateUserResponse() { Token = token, User = user } };

            }
            else
            {

                return new ApiResponse<CreateUserResponse> { IsSuccess = false, StatusCode = 500, Message = result.Errors.FirstOrDefault().Description };
            }

        }
        public async Task<ApiResponse<ForgotPasswordResponse>> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                return new ApiResponse<ForgotPasswordResponse>
                {
                    IsSuccess = true,
                    Message = $"Reset link is send to the email {user.Email}",
                    StatusCode = 200,
                    Response = new ForgotPasswordResponse
                    {
                        Token = token
                    }

                };
            }
            else
            {
                return new ApiResponse<ForgotPasswordResponse>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"Link sending failed, try again!"
                };
            }

        }
        public async Task<ApiResponse<DeleteUserResponse>> DeleteUserAsync(string email)
        {
            var userToDelete = await _userManager.FindByEmailAsync(email);

            if (userToDelete != null)
            {
                try
                {
                    var result = await _userManager.DeleteAsync(userToDelete);

                    if (result.Succeeded)
                    {
                        return new ApiResponse<DeleteUserResponse>
                        {
                            IsSuccess = true,
                            StatusCode = 200,
                            Message = $"User with email {userToDelete.Email} has been deleted successfully.",
                            Response = new DeleteUserResponse
                            {
                                Result = "We will miss you!"
                            }
                        };
                    }
                    else
                    {
                        return new ApiResponse<DeleteUserResponse>
                        {
                            IsSuccess = false,
                            StatusCode = 500,
                            Message = "User deletion failed.",
                            Errors = result.Errors.Select(e => e.Description).ToList()
                        };
                    }
                }
                catch (Exception ex)
                {
                    return new ApiResponse<DeleteUserResponse>
                    {
                        IsSuccess = false,
                        StatusCode = 500,
                        Message = "An error occurred while deleting the user.",
                        Errors = new List<string> { ex.Message }
                    };
                }
            }
            else
            {
                return new ApiResponse<DeleteUserResponse>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"User with email {email} not found."
                };
            }



        }



        public async Task<ApiResponse<LoginOtpResponse>> GetOtpByLoginAsync(LoginModel loginModel)
        {
            var user = await _userManager.FindByEmailAsync(loginModel.Email);
            if (user != null)
            {
                await _signInManager.SignOutAsync();
                await _signInManager.PasswordSignInAsync(user, loginModel.Password, false, true);
                if (user.TwoFactorEnabled)
                {
                    var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                    return new ApiResponse<LoginOtpResponse>
                    {
                        Response = new LoginOtpResponse()
                        {
                            User = user,
                            Token = token,
                            IsTwoFactorEnable = user.TwoFactorEnabled
                        },
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = $"Code is send to the email {user.Email}"
                    };

                }
                else
                {
                    return new ApiResponse<LoginOtpResponse>
                    {
                        Response = new LoginOtpResponse()
                        {
                            User = user,
                            Token = string.Empty,
                            IsTwoFactorEnable = user.TwoFactorEnabled
                        },
                        IsSuccess = true,
                        StatusCode = 200,
                        Message = $"2FA is not enabled"
                    };
                }
            }
            else
            {
                return new ApiResponse<LoginOtpResponse>
                {
                    IsSuccess = false,
                    StatusCode = 404,
                    Message = $"User does not exist."
                };
            }
        }
        public async Task<ApiResponse<JwtToken>> GetJwtTokenAsync(IdentityUser user)
        {
            var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            var jwtToken = GetToken(authClaims);


            return new ApiResponse<JwtToken>
            {
                Response = new JwtToken()
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    ExpiryTokenDate = jwtToken.ValidTo
                },
                IsSuccess = true,
                StatusCode = 200,
                Message = $"Token created"
            };
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddDays(2),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }



    }
}

