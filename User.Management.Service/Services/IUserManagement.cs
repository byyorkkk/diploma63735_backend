using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using User.Management.Service.Models;
using User.Management.Service.Models.Authentication.Login;
using User.Management.Service.Models.Authentication.SignUp;
using User.Management.Service.Models.Authentication.User;

namespace User.Management.Service.Services
{
    public interface IUserManagement
    {
        Task<ApiResponse<CreateUserResponse>> CreateUserWithTokenAsync(RegisterUser registerUser);
        Task<ApiResponse<LoginOtpResponse>> GetOtpByLoginAsync(LoginModel loginModel);
        Task<ApiResponse<JwtToken>> GetJwtTokenAsync(IdentityUser user);
        Task<ApiResponse<ForgotPasswordResponse>> ForgotPasswordAsync(string email);
        Task<ApiResponse<DeleteUserResponse>> DeleteUserAsync(string email);

    }
}

