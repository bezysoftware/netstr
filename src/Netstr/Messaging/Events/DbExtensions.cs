using Microsoft.EntityFrameworkCore;
using Netstr.Data;

namespace Netstr.Messaging.Events
{
    public static class DbExtensions
    {
        public static Task<bool> IsDeleted(this DbSet<EventEntity> db, string id)
        {
            return db.AnyAsync(x => x.EventId == id && x.DeletedAt.HasValue);
        }
    }
}
