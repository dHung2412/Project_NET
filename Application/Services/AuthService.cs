using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace InventoryManagement.Application.Services
{
    public interface IAuthService
    {
        Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginDto loginDto);
        Task<ApiResponseDto<UserDto>> RegisterAsync(RegisterDto registerDto);
        Task<ApiResponseDto<AuthResponseDto>> RefreshTokenAsync(string refreshToken);
        Task<ApiResponseDto<string>> LogoutAsync(Guid userId, string refreshToken);
        Task<ApiResponseDto<UserDto>> GetCurrentUserAsync(Guid userId);
        Task<ApiResponseDto<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto updateDto);
        Task<ApiResponseDto<string>> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto);
        Task<bool> ValidateUserPermissionAsync(Guid userId, string resource);
    }

    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AuthService> _logger;

        // Simple in-memory storage for refresh tokens (in production, use Redis or database)
        private readonly Dictionary<string, RefreshTokenInfo> _refreshTokens = new();

        public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, IMemoryCache cache, ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByUsernameAsync(loginDto.Username);
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("Login failed: User {Username} not found or inactive", loginDto.Username);
                    return ApiResponseDto<AuthResponseDto>.ErrorResponse("Tên đăng nhập hoặc mật khẩu không đúng");
                }

                if (!VerifyPassword(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed: Invalid password for user {Username}", loginDto.Username);
                    return ApiResponseDto<AuthResponseDto>.ErrorResponse("Tên đăng nhập hoặc mật khẩu không đúng");
                }

                // Update last login
                user.UpdateLastLogin();
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var accessToken = _jwtService.GenerateAccessToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Store refresh token
                _refreshTokens[refreshToken] = new RefreshTokenInfo
                {
                    UserId = user.Id,
                    ExpiresAt = DateTime.UtcNow.AddDays(7) // Refresh token expires in 7 days
                };

                var response = new AuthResponseDto
                {
                    AccessToken = accessToken,
                    ExpiresIn = 3600, // 1 hour
                    User = MapToUserDto(user)
                };

                _logger.LogInformation("User {Username} logged in successfully", loginDto.Username);
                return ApiResponseDto<AuthResponseDto>.SuccessResponse(response, "Đăng nhập thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", loginDto.Username);
                return ApiResponseDto<AuthResponseDto>.ErrorResponse("Lỗi hệ thống khi đăng nhập");
            }
        }

        public async Task<ApiResponseDto<UserDto>> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
                // Check if username already exists
                var existingUser = await _unitOfWork.Users.GetByUsernameAsync(registerDto.Username);
                if (existingUser != null)
                {
                    return ApiResponseDto<UserDto>.ErrorResponse("Tên đăng nhập đã tồn tại");
                }

                // Check if email already exists
                var existingEmail = await _unitOfWork.Users.GetByEmailAsync(registerDto.Email);
                if (existingEmail != null)
                {
                    return ApiResponseDto<UserDto>.ErrorResponse("Email đã được sử dụng");
                }

                var passwordHash = HashPassword(registerDto.Password);
                var user = new User(
                    registerDto.Username,
                    registerDto.Email,
                    passwordHash,
                    registerDto.FirstName,
                    registerDto.LastName,
                    registerDto.Role
                );

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("New user {Username} registered successfully", registerDto.Username);
                return ApiResponseDto<UserDto>.SuccessResponse(MapToUserDto(user), "Đăng ký thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Username}", registerDto.Username);
                return ApiResponseDto<UserDto>.ErrorResponse("Lỗi hệ thống khi đăng ký");
            }
        }

        public async Task<ApiResponseDto<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                if (!_refreshTokens.TryGetValue(refreshToken, out var tokenInfo) || tokenInfo.ExpiresAt < DateTime.UtcNow)
                {
                    return ApiResponseDto<AuthResponseDto>.ErrorResponse("Refresh token không hợp lệ hoặc đã hết hạn");
                }

                var user = await _unitOfWork.Users.GetByIdAsync(tokenInfo.UserId);
                if (user == null || !user.IsActive)
                {
                    _refreshTokens.Remove(refreshToken);
                    return ApiResponseDto<AuthResponseDto>.ErrorResponse("Người dùng không tồn tại hoặc đã bị vô hiệu hóa");
                }

                // Generate new tokens
                var newAccessToken = _jwtService.GenerateAccessToken(user);
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                // Remove old refresh token and add new one
                _refreshTokens.Remove(refreshToken);
                _refreshTokens[newRefreshToken] = new RefreshTokenInfo
                {
                    UserId = user.Id,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                };

                var response = new AuthResponseDto
                {
                    AccessToken = newAccessToken,
                    ExpiresIn = 3600,
                    User = MapToUserDto(user)
                };

                return ApiResponseDto<AuthResponseDto>.SuccessResponse(response, "Token đã được làm mới");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return ApiResponseDto<AuthResponseDto>.ErrorResponse("Lỗi hệ thống khi làm mới token");
            }
        }

        public Task<ApiResponseDto<string>> LogoutAsync(Guid userId, string refreshToken)
        {
            try
            {
                // Remove refresh token
                _refreshTokens.Remove(refreshToken);
                
                // Clear any cached user data
                _cache.Remove($"user_{userId}");

                _logger.LogInformation("User {UserId} logged out successfully", userId);
                return Task.FromResult(ApiResponseDto<string>.SuccessResponse("Success", "Đăng xuất thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user {UserId}", userId);
                return Task.FromResult(ApiResponseDto<string>.ErrorResponse("Lỗi hệ thống khi đăng xuất"));
            }
        }

        public async Task<ApiResponseDto<UserDto>> GetCurrentUserAsync(Guid userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null || !user.IsActive)
                {
                    return ApiResponseDto<UserDto>.ErrorResponse("Người dùng không tồn tại");
                }

                return ApiResponseDto<UserDto>.SuccessResponse(MapToUserDto(user), "Lấy thông tin người dùng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user {UserId}", userId);
                return ApiResponseDto<UserDto>.ErrorResponse("Lỗi hệ thống khi lấy thông tin người dùng");
            }
        }

        public async Task<ApiResponseDto<UserDto>> UpdateProfileAsync(Guid userId, UpdateProfileDto updateDto)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null || !user.IsActive)
                {
                    return ApiResponseDto<UserDto>.ErrorResponse("Người dùng không tồn tại");
                }

                user.UpdateProfile(updateDto.FirstName, updateDto.LastName);
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // Clear cache
                _cache.Remove($"user_{userId}");

                return ApiResponseDto<UserDto>.SuccessResponse(MapToUserDto(user), "Cập nhật thông tin thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
                return ApiResponseDto<UserDto>.ErrorResponse("Lỗi hệ thống khi cập nhật thông tin");
            }
        }

        public async Task<ApiResponseDto<string>> ChangePasswordAsync(Guid userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null || !user.IsActive)
                {
                    return ApiResponseDto<string>.ErrorResponse("Người dùng không tồn tại");
                }

                if (!VerifyPassword(changePasswordDto.CurrentPassword, user.PasswordHash))
                {
                    return ApiResponseDto<string>.ErrorResponse("Mật khẩu hiện tại không đúng");
                }

                var newPasswordHash = HashPassword(changePasswordDto.NewPassword);
                user.ChangePassword(newPasswordHash);
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Password changed successfully for user {UserId}", userId);
                return ApiResponseDto<string>.SuccessResponse("Success", "Đổi mật khẩu thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return ApiResponseDto<string>.ErrorResponse("Lỗi hệ thống khi đổi mật khẩu");
            }
        }

        public async Task<bool> ValidateUserPermissionAsync(Guid userId, string resource)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                return user?.IsActive == true && user.CanAccessResource(resource);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating permission for user {UserId} and resource {Resource}", userId, resource);
                return false;
            }
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "InventorySalt2024"));
            return Convert.ToBase64String(hashedBytes);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            var computedHash = HashPassword(password);
            return computedHash == hash;
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }
    }

    internal class RefreshTokenInfo
    {
        public Guid UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}