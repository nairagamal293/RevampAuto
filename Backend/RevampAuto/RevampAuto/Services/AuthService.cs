// Services/AuthService.cs
using Microsoft.AspNetCore.Identity;
using RevampAuto.DTOs;
using RevampAuto.Models;
using System.Threading.Tasks;

namespace RevampAuto.Services
{
    

    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto)
        {
            var user = new User
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Address = registerDto.Address,
                PhoneNumber = registerDto.PhoneNumber
            };

            return await _userManager.CreateAsync(user, registerDto.Password);
        }

        public async Task<SignInResult> LoginUserAsync(UserLoginDto loginDto)
        {
            return await _signInManager.PasswordSignInAsync(
                loginDto.Email,
                loginDto.Password,
                isPersistent: false,
                lockoutOnFailure: false);
        }
        // Services/AuthService.cs (update the class)
        public async Task LogoutUserAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<IdentityResult> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            user.FirstName = updateDto.FirstName;
            user.LastName = updateDto.LastName;
            user.Address = updateDto.Address;
            user.PhoneNumber = updateDto.PhoneNumber;

            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> ChangePasswordAsync(string userId, UpdatePasswordDto passwordDto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            return await _userManager.ChangePasswordAsync(
                user,
                passwordDto.CurrentPassword,
                passwordDto.NewPassword);
        }
    }
}