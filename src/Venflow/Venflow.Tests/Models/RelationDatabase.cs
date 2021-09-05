using Venflow.Shared;

namespace Venflow.Tests.Models
{
    public class RelationDatabase : Database
    {
        public Table<Person> People { get; set; }
        public Table<Email> Emails { get; set; }
        public Table<EmailContent> EmailContents { get; set; }

        public Table<UncommonType> UncommonTypes { get; set; }
        public Table<User> Users { get; set; }
        public Table<Blog> Blogs { get; set; }

        public RelationDatabase() : base(SecretsHandler.GetConnectionString<RelationDatabase>("Tests"))
        {
            UnitTestHandler.Init(this);
        }

        protected override void Configure(DatabaseConfigurationOptionsBuilder optionsBuilder)
        {
            optionsBuilder.RegisterPostgresEnum<PostgreEnum>();
        }
    }
}
