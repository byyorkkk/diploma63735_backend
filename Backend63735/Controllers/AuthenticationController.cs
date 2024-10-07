using Backend63735.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using User.Management.Service.Services;
using User.Management.Service.Models;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using User.Management.Service.Models.Authentication.SignUp;
using User.Management.Service.Models.Authentication.Login;
using User.Management.Service.Models.Authentication.ResetPassword;
using System.Web;

namespace Backend63735.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IUserManagement _user;

        public AuthenticationController(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager, IEmailService emailService,
            SignInManager<IdentityUser> signInManager, IConfiguration configuration,
            IUserManagement user)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _configuration = configuration;
            _user = user;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUser registerUser)
        {
            var tokenResponse = await _user.CreateUserWithTokenAsync(registerUser);

            if (tokenResponse.IsSuccess && tokenResponse.Response != null)
            {
                string encodedToken = HttpUtility.UrlEncode(tokenResponse.Response.Token);
                // var confirmationLink = Url.Action(nameof(ConfirmEmail), "Authentication", new { tokenResponse.Response.Token, email = registerUser.Email }, Request.Scheme);
                var confirmationLink = $"https://adhdapi.azurewebsites.net/api/Authentication/ConfirmEmail?token={encodedToken}&email={registerUser.Email}";
                var message = new Message(new string[] { registerUser.Email! }, "Welcome! This is you confirmation email link", confirmationLink!);
                var responseMsg = _emailService.SendEmail(message);
                return StatusCode(StatusCodes.Status200OK,
                        new Response { IsSuccess = true, Message = $"{tokenResponse.Message} {responseMsg}" });

            }
            return StatusCode(StatusCodes.Status500InternalServerError,
                             new Response { Message = tokenResponse.Message, IsSuccess = false });
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK,
                      new Response { Status = "Success", Message = "Email Verified Successfully" });
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError,
                       new Response { Status = "Error", Message = "This User Does not exist!" });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("LogIn")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var loginOtpResponse = await _user.GetOtpByLoginAsync(loginModel);
            if (loginOtpResponse.Response != null)
            {
                var user = loginOtpResponse.Response.User;
                if (user.TwoFactorEnabled)
                {
                    var token = loginOtpResponse.Response.Token;
                    var message = new Message(new string[] { user.Email! }, "Code Confrimation", token);
                    _emailService.SendEmail(message);

                    return StatusCode(StatusCodes.Status200OK,
                     new Response { IsSuccess = loginOtpResponse.IsSuccess, Status = "Success", Message = $"We have sent 6 signs code to your Email {user.Email}" });
                }
                //if (user != null)
                //{
                //    await _signInManager.PasswordSignInAsync(user, loginModel.Password, false, true);
                //    var serviceResponse = await _user.GetJwtTokenAsync(user);
                //    return Ok(serviceResponse);

                //}
            }
            return Unauthorized();


        }

        [HttpPost]
        [AllowAnonymous]
        [Route("LogIn-2FA")]
        public async Task<IActionResult> LoginWithOTP(LoginOTPModel loginOTPModel)
        {
            var user = await _userManager.FindByEmailAsync(loginOTPModel.Email);
            //var signIn = await _signInManager.TwoFactorSignInAsync("Email", loginOTPModel.Code, false, false);
            //if (signIn.Succeeded)
            //{
            if (user != null)
            {
                var serviceResponse = await _user.GetJwtTokenAsync(user);
                return Ok(serviceResponse);
            }
            //}
            return StatusCode(StatusCodes.Status404NotFound,
                new Response { Status = "Fail", Message = $"Invalid Code, try to Log In again and check password carefully :)" });
        }

        [HttpPost]
        [Route("ForgotPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([Required] string email)
        {
            var forgotPasswordResponse = await _user.ForgotPasswordAsync(email);
            if (forgotPasswordResponse.Response != null)
            {
                string encodedToken = HttpUtility.UrlEncode(forgotPasswordResponse.Response.Token);
                var forgorPasswordLink = $"https://adhdapi.azurewebsites.net/api/Authentication/ResetPassword?token={encodedToken}&email={email}";
                var message = new Message(new string[] { email! }, "Forgot Password Link!", forgorPasswordLink!);
                _emailService.SendEmail(message);
                return StatusCode(StatusCodes.Status200OK,
                    new Response { Status = "Success", Message = $"Password change request is sent to your Email {email}. Please, open the link and reset your password!" });
            }
            return StatusCode(StatusCodes.Status400BadRequest,
                  new Response { Status = "Fail", Message = $"Password change request sending failed. Please, try again" });
        }

        [HttpGet("ResetPassword")]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            var model = new ResetPassword { Token = token, Email = email };
            return Ok(new { model });
        }
        [HttpPost]
        [Route("ResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPassword resetPassword)
        {
            var user = await _userManager.FindByEmailAsync(resetPassword.Email);
            if (user != null)
            {
                var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password);
                if (!resetPassResult.Succeeded)
                {
                    foreach (var error in resetPassResult.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return Ok(ModelState);
                }

                return StatusCode(StatusCodes.Status200OK,
                    new Response { Status = "Success", Message = $"Password has been changed!" });
            }
            return StatusCode(StatusCodes.Status400BadRequest,
                  new Response { Status = "Fail", Message = $"Password change request sending failed. Please, try again" });
        }

        [HttpDelete]
        [Authorize]
        [Route("DeleteUser")]
        public async Task<IActionResult> DeleteUser([Required] string email)
        {

            var emailFromToken = User.FindFirstValue(ClaimTypes.Email);

            // Check if the emails match
            if (emailFromToken != email)
            {
                // If emails don't match, return a 403 Forbidden status
                return StatusCode(StatusCodes.Status403Forbidden, new Response { Status = "Fail", Message = "You are not authorized to delete this account." });
            }

            var deleteUser = await _user.DeleteUserAsync(email);

            if (deleteUser.IsSuccess)
            {
                return StatusCode(deleteUser.StatusCode, new Response { Status = deleteUser.StatusCode.ToString(), Message = deleteUser.Message });
            }

            return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Fail", Message = "Account deleting failed. Please, try again" });
        }


    }






}
