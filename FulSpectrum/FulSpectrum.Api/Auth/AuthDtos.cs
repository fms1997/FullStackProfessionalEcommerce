namespace FulSpectrum.Api.Auth;

public sealed record RegisterRequest(string Email, string Password, string FirstName, string LastName);
public sealed record LoginRequest(string Email, string Password);
public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Token, string NewPassword);

public sealed record AuthResponse(string AccessToken, DateTime ExpiresAtUtc, UserProfile Profile);
public sealed record UserProfile(Guid Id, string Email, string FirstName, string LastName, string Role);
