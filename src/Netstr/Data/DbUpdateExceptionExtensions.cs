using Microsoft.EntityFrameworkCore;

namespace Netstr.Data
{
    public static class DbUpdateExceptionExtensions
    {
        private static readonly string[] UniqueIndexNames = [
            "UNIQUE",
            NetstrDbContext.EventIdIndexName, 
            NetstrDbContext.ReplaceableUniqueIndexName
        ];

        public static bool IsUniqueIndexViolation(this DbUpdateException exception)
        {
            var message = exception.ToString();

            return UniqueIndexNames.Any(message.Contains);
        }
    }
}
