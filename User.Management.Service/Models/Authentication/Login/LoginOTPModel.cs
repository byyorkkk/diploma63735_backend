using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Management.Service.Models.Authentication.Login
{
    public class LoginOTPModel
    {
        [Required(ErrorMessage = "User Name is required")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Code { get; set; }
    }
}
