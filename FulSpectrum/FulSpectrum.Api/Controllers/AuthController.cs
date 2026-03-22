//using Asp.Versioning;
//using FulSpectrum.Api.Auth;
//using FulSpectrum.Api.Security;
//using FulSpectrum.Domain.Identity;
//using FulSpectrum.Infrastructure.Persistence;
//using Microsoft.AspNetCore.Antiforgery;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Options;

//namespace FulSpectrum.Api.Controllers;

//[ApiController]
//[ApiVersion("1.0")]
//[Route("api/v{version:apiVersion}/auth")]
//public sealed class AuthController : ControllerBase
//{
//    private const string RefreshCookieName = "refreshToken";

//    private readonly FulSpectrumDbContext _db;
//    private readonly ITokenService _tokenService;
//    private readonly IPasswordHasher<AppUser> _passwordHasher;
//    private readonly JwtOptions _jwtOptions;
//    private readonly IAntiforgery _antiforgery;
//    private readonly ILogger<AuthController> _logger;

//    public AuthController(
//        FulSpectrumDbContext db,
//        ITokenService tokenService,
//        IPasswordHasher<AppUser> passwordHasher,
//        IOptions<JwtOptions> jwtOptions,
//        IAntiforgery antiforgery,
//        ILogger<AuthController> logger)
//    {
//        _db = db;
//        _tokenService = tokenService;
//        _passwordHasher = passwordHasher;
//        _jwtOptions = jwtOptions.Value;
//        _antiforgery = antiforgery;
//        _logger = logger;
//    }

//    [HttpGet("csrf-token")]
//    [AllowAnonymous]
//    public IActionResult GetCsrfToken()
//    {
//        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
//        return Ok(new { csrfToken = tokens.RequestToken });
//    }

//    [HttpPost("register")]
//    [AllowAnonymous]
//    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
//    {
//        var email = InputSanitizer.Clean(request.Email).ToLowerInvariant();
//        var normalizedEmail = email.ToUpperInvariant();

//        if (await _db.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail, ct))
//        {
//            return Conflict(new { message = "El email ya está registrado." });
//        }

//        var user = new AppUser
//        {
//            Id = Guid.NewGuid(),
//            Email = email,
//            NormalizedEmail = normalizedEmail,
//            FirstName = InputSanitizer.Clean(request.FirstName),
//            LastName = InputSanitizer.Clean(request.LastName),
//            Role = UserRoles.Customer,
//            CreatedAtUtc = DateTime.UtcNow,
//            IsActive = true
//        };

//        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

//        _db.Users.Add(user);
//        await _db.SaveChangesAsync(ct);

//        _logger.LogInformation(
//            "AuditEvent Action={Action} UserId={UserId} Email={Email}",
//            "register_success",
//            user.Id,
//            user.Email);

//        return Ok(await CreateSessionAsync(user, ct));
//    }

//    [HttpPost("login")]
//    [AllowAnonymous]
//    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
//    {
//        var normalizedEmail = InputSanitizer.Clean(request.Email).ToUpperInvariant();
//        var user = await _db.Users.FirstOrDefaultAsync(
//            x => x.NormalizedEmail == normalizedEmail && x.IsActive,
//            ct);

//        if (user is null)
//        {
//            _logger.LogWarning(
//                "AuditEvent Action={Action} Email={Email}",
//                "login_failed_user_not_found",
//                normalizedEmail);

//            return Unauthorized(new { message = "Credenciales inválidas." });
//        }

//        var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
//        if (verify == PasswordVerificationResult.Failed)
//        {
//            _logger.LogWarning(
//                "AuditEvent Action={Action} UserId={UserId}",
//                "login_failed_bad_password",
//                user.Id);

//            return Unauthorized(new { message = "Credenciales inválidas." });
//        }

//        _logger.LogInformation(
//            "AuditEvent Action={Action} UserId={UserId}",
//            "login_success",
//            user.Id);

//        return Ok(await CreateSessionAsync(user, ct));
//    }

//    [HttpPost("refresh")]
//    [AllowAnonymous]
//    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken ct)
//    {
//        if (!Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
//        {
//            _logger.LogWarning("AuditEvent Action={Action}", "refresh_failed_missing_cookie");
//            return Unauthorized();
//        }

//        var tokenHash = _tokenService.HashToken(refreshToken);
//        var session = await _db.RefreshSessions
//            .Include(x => x.User)
//            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);

//        if (session is null || !session.IsActive || !session.User.IsActive)
//        {
//            _logger.LogWarning("AuditEvent Action={Action}", "refresh_failed_invalid_session");
//            return Unauthorized();
//        }

//        session.RevokedAtUtc = DateTime.UtcNow;

//        var response = await CreateSessionAsync(session.User, ct, session.Id);

//        _logger.LogInformation(
//            "AuditEvent Action={Action} UserId={UserId}",
//            "refresh_success",
//            session.User.Id);

//        return Ok(response);
//    }

//    [HttpPost("logout")]
//    [Authorize]
//    public async Task<IActionResult> Logout(CancellationToken ct)
//    {
//        if (Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken) && !string.IsNullOrWhiteSpace(refreshToken))
//        {
//            var tokenHash = _tokenService.HashToken(refreshToken);
//            var session = await _db.RefreshSessions.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);

//            if (session is not null && session.RevokedAtUtc is null)
//            {
//                session.RevokedAtUtc = DateTime.UtcNow;
//                await _db.SaveChangesAsync(ct);
//            }
//        }

//        Response.Cookies.Delete(RefreshCookieName);

//        _logger.LogInformation(
//            "AuditEvent Action={Action} UserId={UserId}",
//            "logout",
//            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown");

//        return NoContent();
//    }

//    [HttpGet("me")]
//    [Authorize]
//    public async Task<ActionResult<UserProfile>> Me(CancellationToken ct)
//    {
//        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
//        if (!Guid.TryParse(userId, out var id))
//        {
//            return Unauthorized();
//        }

//        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
//        if (user is null)
//        {
//            return Unauthorized();
//        }

//        return Ok(ToProfile(user));
//    }

//    [HttpPost("forgot-password")]
//    [AllowAnonymous]
//    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
//    {
//        var normalizedEmail = InputSanitizer.Clean(request.Email).ToUpperInvariant();
//        var user = await _db.Users.FirstOrDefaultAsync(
//            x => x.NormalizedEmail == normalizedEmail && x.IsActive,
//            ct);

//        string? rawToken = null;

//        if (user is not null)
//        {
//            rawToken = _tokenService.CreateRefreshToken();

//            _db.PasswordResetTokens.Add(new PasswordResetToken
//            {
//                Id = Guid.NewGuid(),
//                UserId = user.Id,
//                TokenHash = _tokenService.HashToken(rawToken),
//                CreatedAtUtc = DateTime.UtcNow,
//                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
//            });

//            await _db.SaveChangesAsync(ct);

//            _logger.LogInformation(
//                "AuditEvent Action={Action} UserId={UserId}",
//                "password_reset_requested",
//                user.Id);
//        }

//        return Ok(new
//        {
//            message = "Si el email existe, enviamos instrucciones para restablecer la contraseña.",
//            resetToken = rawToken
//        });
//    }

//    [HttpPost("reset-password")]
//    [AllowAnonymous]
//    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
//    {
//        var hash = _tokenService.HashToken(InputSanitizer.Clean(request.Token));

//        var resetToken = await _db.PasswordResetTokens
//            .Include(x => x.User)
//            .FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

//        if (resetToken is null || !resetToken.IsActive || !resetToken.User.IsActive)
//        {
//            _logger.LogWarning("AuditEvent Action={Action}", "password_reset_failed_invalid_token");
//            return BadRequest(new { message = "Token inválido o expirado." });
//        }

//        resetToken.UsedAtUtc = DateTime.UtcNow;
//        resetToken.User.PasswordHash = _passwordHasher.HashPassword(resetToken.User, request.NewPassword);

//        foreach (var activeSession in _db.RefreshSessions.Where(x => x.UserId == resetToken.UserId && x.RevokedAtUtc == null))
//        {
//            activeSession.RevokedAtUtc = DateTime.UtcNow;
//        }

//        await _db.SaveChangesAsync(ct);

//        _logger.LogInformation(
//            "AuditEvent Action={Action} UserId={UserId}",
//            "password_reset_success",
//            resetToken.UserId);

//        return Ok(new { message = "Contraseña actualizada correctamente." });
//    }

//    private async Task<AuthResponse> CreateSessionAsync(AppUser user, CancellationToken ct, Guid? replacedSessionId = null)
//    {
//        var (accessToken, jti, accessExpiresAtUtc) = _tokenService.CreateAccessToken(user);
//        var refreshToken = _tokenService.CreateRefreshToken();

//        var session = new RefreshSession
//        {
//            Id = Guid.NewGuid(),
//            UserId = user.Id,
//            TokenHash = _tokenService.HashToken(refreshToken),
//            JwtId = jti,
//            CreatedAtUtc = DateTime.UtcNow,
//            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
//            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString()
//        };

//        if (replacedSessionId.HasValue)
//        {
//            var previous = await _db.RefreshSessions.FirstOrDefaultAsync(x => x.Id == replacedSessionId.Value, ct);
//            if (previous is not null)
//            {
//                previous.ReplacedBySessionId = session.Id;
//            }
//        }

//        _db.RefreshSessions.Add(session);
//        await _db.SaveChangesAsync(ct);

//        Response.Cookies.Append(RefreshCookieName, refreshToken, new CookieOptions
//        {
//            HttpOnly = true,
//            Secure = !HttpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment(),
//            SameSite = SameSiteMode.Strict,
//            Expires = session.ExpiresAtUtc
//        });

//        return new AuthResponse(accessToken, accessExpiresAtUtc, ToProfile(user));
//    }

//    private static UserProfile ToProfile(AppUser user)
//        => new(user.Id, user.Email, user.FirstName, user.LastName, user.Role);
//}








using Asp.Versioning;
using FulSpectrum.Api.Auth;
using FulSpectrum.Api.Security;
using FulSpectrum.Domain.Identity;
using FulSpectrum.Infrastructure.Persistence;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace FulSpectrum.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : ControllerBase
{
    private const string RefreshCookieName = "refreshToken";

    private readonly FulSpectrumDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly JwtOptions _jwtOptions;
    private readonly IAntiforgery _antiforgery;
    private readonly ILogger<AuthController> _logger;
    private readonly IHostEnvironment _hostEnvironment;

    public AuthController(
        FulSpectrumDbContext db,
        ITokenService tokenService,
        IPasswordHasher<AppUser> passwordHasher,
        IOptions<JwtOptions> jwtOptions,
        IAntiforgery antiforgery,
        ILogger<AuthController> logger,
        IHostEnvironment hostEnvironment)
    {
        _db = db;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _jwtOptions = jwtOptions.Value;
        _antiforgery = antiforgery;
        _logger = logger;
        _hostEnvironment = hostEnvironment;
    }

    [HttpGet("csrf-token")]
    [AllowAnonymous]
    public IActionResult GetCsrfToken()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);

        return Ok(new
        {
            csrfToken = tokens.RequestToken
        });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var email = InputSanitizer.Clean(request.Email).ToLowerInvariant();
        var normalizedEmail = email.ToUpperInvariant();

        var emailExists = await _db.Users.AnyAsync(
            x => x.NormalizedEmail == normalizedEmail,
            ct);

        if (emailExists)
        {
            return Conflict(new { message = "El email ya está registrado." });
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = normalizedEmail,
            FirstName = InputSanitizer.Clean(request.FirstName),
            LastName = InputSanitizer.Clean(request.LastName),
            Role = UserRoles.Customer,
            CreatedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "AuditEvent Action={Action} UserId={UserId} Email={Email}",
            "register_success",
            user.Id,
            user.Email);

        return Ok(await CreateSessionAsync(user, ct));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var normalizedEmail = InputSanitizer.Clean(request.Email).ToUpperInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(
            x => x.NormalizedEmail == normalizedEmail && x.IsActive,
            ct);

        if (user is null)
        {
            _logger.LogWarning(
                "AuditEvent Action={Action} Email={Email}",
                "login_failed_user_not_found",
                normalizedEmail);

            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning(
                "AuditEvent Action={Action} UserId={UserId}",
                "login_failed_bad_password",
                user.Id);

            return Unauthorized(new { message = "Credenciales inválidas." });
        }

        _logger.LogInformation(
            "AuditEvent Action={Action} UserId={UserId}",
            "login_success",
            user.Id);

        return Ok(await CreateSessionAsync(user, ct));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken ct)
    {
        if (!Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
        {
            _logger.LogWarning(
                "AuditEvent Action={Action}",
                "refresh_failed_missing_cookie");

            return Unauthorized();
        }

        var tokenHash = _tokenService.HashToken(refreshToken);

        var session = await _db.RefreshSessions
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);

        if (session is null || !session.IsActive || !session.User.IsActive)
        {
            _logger.LogWarning(
                "AuditEvent Action={Action}",
                "refresh_failed_invalid_session");

            return Unauthorized();
        }

        session.RevokedAtUtc = DateTime.UtcNow;

        var response = await CreateSessionAsync(session.User, ct, session.Id);

        _logger.LogInformation(
            "AuditEvent Action={Action} UserId={UserId}",
            "refresh_success",
            session.User.Id);

        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (Request.Cookies.TryGetValue(RefreshCookieName, out var refreshToken) &&
            !string.IsNullOrWhiteSpace(refreshToken))
        {
            var tokenHash = _tokenService.HashToken(refreshToken);

            var session = await _db.RefreshSessions.FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                ct);

            if (session is not null && session.RevokedAtUtc is null)
            {
                session.RevokedAtUtc = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
            }
        }

        Response.Cookies.Delete(RefreshCookieName);

        _logger.LogInformation(
            "AuditEvent Action={Action} UserId={UserId}",
            "logout",
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown");

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserProfile>> Me(CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userId, out var id))
        {
            return Unauthorized();
        }

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(ToProfile(user));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct)
    {
        var normalizedEmail = InputSanitizer.Clean(request.Email).ToUpperInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(
            x => x.NormalizedEmail == normalizedEmail && x.IsActive,
            ct);

        string? rawToken = null;

        if (user is not null)
        {
            rawToken = _tokenService.CreateRefreshToken();

            _db.PasswordResetTokens.Add(new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = _tokenService.HashToken(rawToken),
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
            });

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "AuditEvent Action={Action} UserId={UserId}",
                "password_reset_requested",
                user.Id);
        }

        return Ok(new
        {
            message = "Si el email existe, enviamos instrucciones para restablecer la contraseña.",
            resetToken = rawToken
        });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct)
    {
        var hash = _tokenService.HashToken(InputSanitizer.Clean(request.Token));

        var resetToken = await _db.PasswordResetTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == hash, ct);

        if (resetToken is null || !resetToken.IsActive || !resetToken.User.IsActive)
        {
            _logger.LogWarning(
                "AuditEvent Action={Action}",
                "password_reset_failed_invalid_token");

            return BadRequest(new { message = "Token inválido o expirado." });
        }

        resetToken.UsedAtUtc = DateTime.UtcNow;
        resetToken.User.PasswordHash = _passwordHasher.HashPassword(
            resetToken.User,
            request.NewPassword);

        foreach (var activeSession in _db.RefreshSessions.Where(
                     x => x.UserId == resetToken.UserId && x.RevokedAtUtc == null))
        {
            activeSession.RevokedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "AuditEvent Action={Action} UserId={UserId}",
            "password_reset_success",
            resetToken.UserId);

        return Ok(new { message = "Contraseña actualizada correctamente." });
    }

    private async Task<AuthResponse> CreateSessionAsync(
        AppUser user,
        CancellationToken ct,
        Guid? replacedSessionId = null)
    {
        var (accessToken, jwtId, accessExpiresAtUtc) = _tokenService.CreateAccessToken(user);
        var refreshToken = _tokenService.CreateRefreshToken();

        var session = new RefreshSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = _tokenService.HashToken(refreshToken),
            JwtId = jwtId,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays),
            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        if (replacedSessionId.HasValue)
        {
            var previousSession = await _db.RefreshSessions.FirstOrDefaultAsync(
                x => x.Id == replacedSessionId.Value,
                ct);

            if (previousSession is not null)
            {
                previousSession.ReplacedBySessionId = session.Id;
            }
        }

        _db.RefreshSessions.Add(session);
        await _db.SaveChangesAsync(ct);

        Response.Cookies.Append(
            RefreshCookieName,
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = !_hostEnvironment.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                Expires = session.ExpiresAtUtc
            });

        return new AuthResponse(
            accessToken,
            accessExpiresAtUtc,
            ToProfile(user));
    }

    private static UserProfile ToProfile(AppUser user) =>
        new(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role);
}