
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SupportTicketsAPI.Hubs
{
    [Authorize]
    public class TicketsHub : Hub
    {
        public static string TicketGroup(int ticketId) => $"ticket-{ticketId}";

        public Task JoinTicket(int ticketId)
            => Groups.AddToGroupAsync(Context.ConnectionId, TicketGroup(ticketId));

        public Task LeaveTicket(int ticketId)
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, TicketGroup(ticketId));
    }
}
