using Venflow.Shared;

namespace Venflow.Tests.Models
{
    public class ReverseRelationDatabase : Database
    {
        public Table<ReversePerson> People { get; set; }
        public Table<ReverseEmail> Emails { get; set; }
        public Table<ReverseEmailContent> EmailContents { get; set; }

        public ReverseRelationDatabase() : base(SecretsHandler.GetConnectionString<ReverseRelationDatabase>("Tests"))
        {
            UnitTestHandler.Wait();
        }
    }
}
