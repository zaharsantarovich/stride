namespace ZSLabs.Stride.Api.Contracts;

public sealed record Space(int Id, string Key, string Name, int AuthorId, bool IsPublic, DateTime CreatedAt);