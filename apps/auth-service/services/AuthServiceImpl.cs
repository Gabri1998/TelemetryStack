using Shared.Contracts.DTOs.Auth;
using AuthService.Models;
using AuthService.Repositories;
using BCrypt.Net;

namespace AuthService.Services;

public class AuthServiceImpl
{
    private readonly UserRepository _repo;
    private readonly JwtService _jwt;

   public AuthServiceImpl(UserRepository repo, JwtService jwt)
    {
        _repo = repo;
        _jwt = jwt;
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        var existing = await _repo.GetByEmailAsync(request.Email);
        if (existing != null)
            throw new Exception("User already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _repo.CreateAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _repo.GetByEmailAsync(request.Email);
        if (user == null)
            throw new Exception("Invalid credentials");

        var valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!valid)
            throw new Exception("Invalid credentials");

        var token = _jwt.GenerateToken(user.Id, user.Email);

        return new AuthResponse
        {
            Token = token
        };
    }
}