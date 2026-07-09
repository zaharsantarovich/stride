namespace ZSLabs.Stride.Api.Contracts;

public sealed record CreateSpaceRequest(string Key, string Name, bool IsPublic);