using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using InventoryManagement.Application.DTOs;
using InventoryManagement.Application.Services;

namespace InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Đăng nhập người dùng
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponseDto<AuthResponseDto>.ErrorResponse("Dữ liệu không hợp lệ", errors));
                }

                var result = await _authService.LoginAsync(loginDto);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                // Set refresh token in HTTP-only cookie
                if (result.Data != null && !string.IsNullOrEmpty(result.Data.RefreshToken))
                {
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddDays(7)
                    };
                    Response.Cookies.Append("refreshToken", result.Data.RefreshToken, cookieOptions);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in login endpoint");
                return StatusCode(500, ApiResponseDto<AuthResponseDto>.ErrorResponse("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Đăng ký người dùng mới (chỉ Admin)
        /// </summary>
        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponseDto<UserDto>>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponseDto<UserDto>.ErrorResponse("Dữ liệu không hợp lệ", errors));
                }

                var result = await _authService.RegisterAsync(registerDto);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetMe), result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in register endpoint");
                return StatusCode(500, ApiResponseDto<UserDto>.ErrorResponse("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Làm mới token
        /// </summary>
        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponseDto<AuthResponseDto>>> RefreshToken()
        {
            try
            {
                var refreshToken = Request.Cookies["refreshToken"];
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return BadRequest(ApiResponseDto<AuthResponseDto>.ErrorResponse("Refresh token không tồn tại"));
                }

                var result = await _authService.RefreshTokenAsync(refreshToken);
                
                if (!result.Success)
                {
                    Response.Cookies.Delete("refreshToken");
                    return BadRequest(result);
                }

                // Update refresh token cookie
                if (result.Data != null && !string.IsNullOrEmpty(result.Data.RefreshToken))
                {
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddDays(7)
                    };
                    Response.Cookies.Append("refreshToken", result.Data.RefreshToken, cookieOptions);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in refresh token endpoint");
                return StatusCode(500, ApiResponseDto<AuthResponseDto>.ErrorResponse("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Đăng xuất người dùng
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<string>>> Logout()
        {
            try
            {
                var userId = GetCurrentUserId();
                var refreshToken = Request.Cookies["refreshToken"] ?? string.Empty;

                var result = await _authService.LogoutAsync(userId, refreshToken);
                
                // Clear refresh token cookie
                Response.Cookies.Delete("refreshToken");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in logout endpoint");
                return StatusCode(500, ApiResponseDto<string>.ErrorResponse("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Lấy thông tin người dùng hiện tại
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<UserDto>>> GetMe()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _authService.GetCurrentUserAsync(userId);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in get current user endpoint");
                return StatusCode(500, ApiResponseDto<UserDto>.ErrorResponse("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Cập nhật thông tin cá nhân
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<UserDto>>> UpdateProfile([FromBody] UpdateProfileDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponseDto<UserDto>.ErrorResponse("Dữ liệu không hợp lệ", errors));
                }

                var userId = GetCurrentUserId();
                var result = await _authService.UpdateProfileAsync(userId, updateDto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in update profile endpoint");
                return StatusCode(500, ApiResponseDto<UserDto>.ErrorResponse("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<string>>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(ApiResponseDto<string>.ErrorResponse("Dữ liệu không hợp lệ", errors));
                }

                var userId = GetCurrentUserId();
                var result = await _authService.ChangePasswordAsync(userId, changePasswordDto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in change password endpoint");
                return StatusCode(500, ApiResponseDto<string>.ErrorResponse("Lỗi hệ thống"));
            }
        }

        /// <summary>
        /// Kiểm tra quyền truy cập
        /// </summary>
        [HttpGet("permissions/{resource}")]
        [Authorize]
        public async Task<ActionResult<ApiResponseDto<bool>>> CheckPermission(string resource)
        {
            try
            {
                var userId = GetCurrentUserId();
                var hasPermission = await _authService.ValidateUserPermissionAsync(userId, resource);
                
                return Ok(ApiResponseDto<bool>.SuccessResponse(hasPermission, "Kiểm tra quyền thành công"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission for resource {Resource}", resource);
                return StatusCode(500, ApiResponseDto<bool>.ErrorResponse("Lỗi hệ thống"));
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng");
            }
            return userId;
        }
    }
}