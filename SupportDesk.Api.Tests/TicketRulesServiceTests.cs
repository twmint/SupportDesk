using SupportDesk.Api.Models;
using SupportDesk.Api.Services;

namespace SupportDesk.Api.Tests;

public class TicketRulesServiceTests
{
    [Theory]
    [InlineData(TicketPriority.Critical, 4)]
    [InlineData(TicketPriority.High, 24)]
    [InlineData(TicketPriority.Normal, 72)]
    [InlineData(TicketPriority.Low, 168)]
    public void CalculateDueDate_ByPriority_AddsExpectedOffset(TicketPriority priority, int expectedHours)
    {
        var createdAt = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc);

        var dueDate = TicketRulesService.CalculateDueDate(priority, createdAt);

        var expectedDueDate = createdAt.AddHours(expectedHours);
        Assert.Equal(expectedDueDate, dueDate);
    }

    [Fact]
    public void CalculateDueDate_UnknownPriority_Throws()
    {
        var unknown = (TicketPriority)99;
        Assert.Throws<ArgumentOutOfRangeException>(() => TicketRulesService.CalculateDueDate(unknown, new DateTime(2026, 1, 1)));
    }

    [Theory]
    [InlineData(TicketStatus.New, TicketStatus.InProgress)]
    [InlineData(TicketStatus.InProgress, TicketStatus.Resolved)]
    [InlineData(TicketStatus.Resolved, TicketStatus.InProgress)]
    [InlineData(TicketStatus.Resolved, TicketStatus.Closed)]
    public void IsValidTransition_ValidTransitions_ReturnsTrue(TicketStatus from, TicketStatus to)
    {
        var isValid = TicketRulesService.IsValidTransition(from, to, out var reason);
        Assert.True(isValid);
        Assert.Null(reason);
    }

    [Theory]
    [InlineData(TicketStatus.New, TicketStatus.Resolved)]
    [InlineData(TicketStatus.InProgress, TicketStatus.Closed)]
    [InlineData(TicketStatus.Closed, TicketStatus.New)]
    [InlineData(TicketStatus.Resolved, TicketStatus.New)]
    public void IsValidTransition_InvalidTransitions_ReturnsFalse(TicketStatus from, TicketStatus to)
    {
        var isValid = TicketRulesService.IsValidTransition(from, to, out var reason);
        Assert.False(isValid);
        Assert.NotNull(reason);
    }
}
