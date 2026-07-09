namespace ZSLabs.Stride.Api.Contracts;

public sealed record CreateSubtaskRequest(string Title, string? Description, SubtaskStatus? Status, int? AssigneeId, DateTime? DueDate);