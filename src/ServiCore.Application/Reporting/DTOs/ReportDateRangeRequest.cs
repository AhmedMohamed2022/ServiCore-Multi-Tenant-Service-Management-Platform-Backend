namespace ServiCore.Application.Reporting.DTOs;

public sealed record ReportDateRangeRequest(
    DateTime? From,
    DateTime? To);