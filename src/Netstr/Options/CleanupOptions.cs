namespace Netstr.Options
{
    public class CleanupOptions
    {
        public CleanupOptions()
        {
            DeleteEventsRules = [];    
        }

        public int DeleteDeletedEventsAfterDays { get; set; }
        public int DeleteExpiredEventsAfterDays { get; set; }
        public DeleteEventsRule[] DeleteEventsRules { get; set; }
    }

    public class DeleteEventsRule
    {
        public DeleteEventsRule()
        {
            Kinds = [];
        }

        public string[] Kinds { get; set; }

        public int DeleteAfterDays { get; set; }
    }
}
