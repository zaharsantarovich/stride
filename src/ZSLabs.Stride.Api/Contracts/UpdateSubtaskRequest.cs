namespace ZSLabs.Stride.Api.Contracts;

public sealed record UpdateSubtaskRequest(string? Title, string? Description, SubtaskStatus? Status, int? AssigneeId, DateTime? DueDate);