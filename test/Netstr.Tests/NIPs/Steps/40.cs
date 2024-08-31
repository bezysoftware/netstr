using Microsoft.EntityFrameworkCore;
using Netstr.Data;
using TechTalk.SpecFlow;

namespace Netstr.Tests.NIPs.Steps
{
    public partial class Steps
    {
        [Given(@"(.*) previously published events")]
        public void GivenBobPreviouslyPublishedEvents(string client, Table table)
        {
            var c = this.scenarioContext.Get<Clients>()[client];
            var events = Transforms
                .CreateEvents(table, c)
                .Select(x => x.ToEntity(DateTimeOffset.UtcNow))
                .ToArray();
        
            using var context = this.factory.Services.GetRequiredService<IDbContextFactory<NetstrDbContext>>().CreateDbContext();

            context.Events.AddRange(events);
            context.SaveChanges();
        }
    }
}
