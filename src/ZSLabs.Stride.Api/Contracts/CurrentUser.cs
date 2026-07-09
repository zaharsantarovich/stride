using ZSLabs.Stride.Domain.Enums;

namespace ZSLabs.Stride.Api.Contracts;

public sealed record CurrentUser(int Id, string Username, string? Email, UserRole Role);