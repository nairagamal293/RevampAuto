using Microsoft.AspNetCore.Identity;
using RevampAuto.DTOs;
using RevampAuto.Models;

namespace RevampAuto.Services
{
    // Services/AuthService.cs (update the interface)
    public interface IAuthService
    {
        Task<IdentityResult> RegisterUserAsync(RegisterDto registerDto);
        Task<SignInResult> LoginUserAsync(UserLoginDto loginDto);
        Task LogoutUserAsync(); // Add this line
        Task<User> GetUserByIdAsync(string userId);
        Task<IdentityResult> UpdateUserProfileAsync(string userId, UpdateUserProfileDto updateDto);
        Task<IdentityResult> ChangePasswordAsync(string userId, UpdatePasswordDto passwordDto);
    }
}
