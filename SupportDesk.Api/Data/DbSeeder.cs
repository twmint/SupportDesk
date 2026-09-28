using Microsoft.EntityFrameworkCore;
using SupportDesk.Api.Models;
using SupportDesk.Api.Services;

namespace SupportDesk.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Agents.AnyAsync())
        {
            return;
        }

        var agents = new List<Agent>
        {
            new() { FullName = "Alice Cooper", Email = "alice.cooper@supportdesk.test", Department = Department.Technical, Active = true },
            new() { FullName = "Bob Marley", Email = "bob.marley@supportdesk.test", Department = Department.Billing, Active = true },
            new() { FullName = "Carol Danvers", Email = "carol.danvers@supportdesk.test", Department = Department.General, Active = true },
            new() { FullName = "Dave Grohl", Email = "dave.grohl@supportdesk.test", Department = Department.Technical, Active = true },
            new() { FullName = "Eve Torres", Email = "eve.torres@supportdesk.test", Department = Department.Billing, Active = false },
        };

        context.Agents.AddRange(agents);
        await context.SaveChangesAsync();

        var alice = agents[0];
        var bob = agents[1];
        var carol = agents[2];
        var dave = agents[3];
        // Eve (agents[4]) is inactive on purpose — stays unassigned everywhere below.

        var now = DateTime.UtcNow;
        var seq = 0;
        string NextReference() => $"REF-{now.Year}-{++seq:D4}";

        Ticket NewTicket(
            string title, string customerName, string customerEmail,
            TicketPriority priority, TicketStatus status,
            int ageInDays, Agent? agent = null)
        {
            var createdAt = now.AddDays(-ageInDays);
            var dueDate = TicketRulesService.CalculateDueDate(priority, createdAt);

            var ticket = new Ticket
            {
                Reference = NextReference(),
                Title = title,
                Description = $"Seed data description for '{title}'.",
                CustomerName = customerName,
                CustomerEmail = customerEmail,
                Priority = priority,
                Status = status,
                CreatedAt = createdAt,
                DueDate = dueDate,
                AgentId = agent?.Id,
            };

            if (status is TicketStatus.Resolved or TicketStatus.Closed)
            {
                ticket.ResolvedAt = createdAt.AddHours(1);
            }
            if (status is TicketStatus.Closed)
            {
                ticket.ClosedAt = createdAt.AddHours(2);
            }
            if (status is not TicketStatus.New)
            {
                ticket.LastModifiedAt = createdAt.AddHours(1);
            }

            return ticket;
        }

        var tickets = new List<Ticket>
        {
            // New — unassigned, fresh
            NewTicket("Cannot log in to portal", "John Smith", "john.smith@example.com", TicketPriority.Critical, TicketStatus.New, ageInDays: 0),
            NewTicket("Feature request: dark mode", "Priya Patel", "priya.patel@example.com", TicketPriority.Low, TicketStatus.New, ageInDays: 0),
            NewTicket("Invoice shows wrong total", "Liam O'Connor", "liam.oconnor@example.com", TicketPriority.Normal, TicketStatus.New, ageInDays: 1),
            NewTicket("Slow page load on dashboard", "Mei Lin", "mei.lin@example.com", TicketPriority.High, TicketStatus.New, ageInDays: 0),
            NewTicket("Typo in welcome email", "Noah Bennett", "noah.bennett@example.com", TicketPriority.Low, TicketStatus.New, ageInDays: 2),

            // In Progress — must have an active agent assigned (business rule 3)
            NewTicket("Password reset email not arriving", "Sara Ahmed", "sara.ahmed@example.com", TicketPriority.High, TicketStatus.InProgress, ageInDays: 0, agent: alice),
            NewTicket("Billing address update request", "Tom Walsh", "tom.walsh@example.com", TicketPriority.Normal, TicketStatus.InProgress, ageInDays: 1, agent: bob),
            NewTicket("API returns 500 on export", "Grace Kim", "grace.kim@example.com", TicketPriority.Critical, TicketStatus.InProgress, ageInDays: 0, agent: dave),
            NewTicket("Question about refund policy", "Omar Haddad", "omar.haddad@example.com", TicketPriority.Low, TicketStatus.InProgress, ageInDays: 1, agent: carol),

            // Resolved — has an agent, awaiting close/reopen
            NewTicket("Two-factor auth setup help", "Elena Petrova", "elena.petrova@example.com", TicketPriority.Normal, TicketStatus.Resolved, ageInDays: 4, agent: alice),
            NewTicket("Duplicate charge on card", "Marcus Reid", "marcus.reid@example.com", TicketPriority.High, TicketStatus.Resolved, ageInDays: 2, agent: bob),
            NewTicket("Report export missing columns", "Yuki Tanaka", "yuki.tanaka@example.com", TicketPriority.Normal, TicketStatus.Resolved, ageInDays: 3, agent: dave),

            // Closed — fully read-only, has an agent
            NewTicket("Account deletion request", "Fatima Zahra", "fatima.zahra@example.com", TicketPriority.Normal, TicketStatus.Closed, ageInDays: 10, agent: carol),
            NewTicket("Upgrade plan question", "Henry Osei", "henry.osei@example.com", TicketPriority.Low, TicketStatus.Closed, ageInDays: 15, agent: bob),
            NewTicket("SSO integration inquiry", "Isabel Cruz", "isabel.cruz@example.com", TicketPriority.High, TicketStatus.Closed, ageInDays: 8, agent: alice),

            // Overdue — DueDate already passed, status still open (rule 7)
            NewTicket("Data export stuck at 0%", "Ravi Shankar", "ravi.shankar@example.com", TicketPriority.Critical, TicketStatus.New, ageInDays: 1),
            NewTicket("Mobile app crashes on launch", "Anna Kowalski", "anna.kowalski@example.com", TicketPriority.High, TicketStatus.InProgress, ageInDays: 5, agent: dave),
            NewTicket("Contract renewal terms unclear", "Lucas Ferreira", "lucas.ferreira@example.com", TicketPriority.Normal, TicketStatus.New, ageInDays: 6),
            NewTicket("Old ticket, still unresolved", "Chidi Okafor", "chidi.okafor@example.com", TicketPriority.Low, TicketStatus.InProgress, ageInDays: 10, agent: alice),
            NewTicket("Overlooked follow-up", "Sofia Rossi", "sofia.rossi@example.com", TicketPriority.Normal, TicketStatus.New, ageInDays: 5),
        };

        context.Tickets.AddRange(tickets);
        await context.SaveChangesAsync();
    }
}
