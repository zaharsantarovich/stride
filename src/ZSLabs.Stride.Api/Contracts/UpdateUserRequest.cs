namespace ZSLabs.Stride.Api.Contracts;

public sealed record UpdateUserRequest(string? Username, string? Password, string? Email);