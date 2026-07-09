namespace ZSLabs.Stride.Api.Contracts;

public sealed record CreateUserRequest(string Username, string Password, string? Email);