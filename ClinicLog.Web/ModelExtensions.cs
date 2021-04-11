using ClinicLog.Web.Models;
using ClinicLog.Web.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace ClinicLog.Web
{
    public static class ModelExtensions
    {
        public static User ToUserViewModel(this ApplicationUser applicationUser)
        {
            return new User
            {
                Id = applicationUser.Id,
                Email = applicationUser.Email,
                FullName = applicationUser.FullName,
                PhoneNumber = applicationUser.PhoneNumber
            };
        }

        public static ApplicationUser ToUserModel(this User user)
        {
            return new ApplicationUser
            {
                Id = user.Id,
                UserName = user.Email,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber
            };
        }
    }
}
