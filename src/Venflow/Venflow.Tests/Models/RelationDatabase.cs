using Venflow.Modeling;
using Venflow.Shared;

namespace Venflow.Tests.Models
{
    public class RelationDatabase : Database
    {
        public Table<Person> People { get; set; }
        public Table<Email> Emails { get; set; }
        public Table<EmailContent> EmailContents { get; set; }

        public Table<UncommonType> UncommonTypes { get; set; }

        public RelationDatabase() : base(SecretsHandler.GetConnectionString<RelationDatabase>())
        {
        }
    }
}
