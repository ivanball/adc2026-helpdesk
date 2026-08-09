Rules to enforce in Helpdesk.ArchitectureTests:
1. Tickets.Domain may depend only on Helpdesk.SharedKernel and System.
2. Tickets.Domain must not reference Microsoft.EntityFrameworkCore anywhere.
3. Tickets.Application must not depend on Tickets.Infrastructure or Tickets.Presentation.
4. Command and query handlers must be internal, not public.
5. No type outside Helpdesk.Tickets.* may depend on Helpdesk.Tickets.Infrastructure
   (only the Host composition root may, by exception).
