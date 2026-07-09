using ZSLabs.Stride.Domain.Enums;

namespace ZSLabs.Stride.Api.Contracts;

public sealed record User(int Id, string Username, string? Email, UserRole Role, DateTime CreatedAt);