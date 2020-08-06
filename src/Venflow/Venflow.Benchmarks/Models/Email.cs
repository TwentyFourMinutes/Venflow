using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Venflow.Modeling.Definitions;
using Venflow.Modeling.Definitions.Builder;

namespace Venflow.Benchmarks.Models
{
    [Table("Emails")]
    public class Email
    {
        public int Id { get; set; }

        public string Address { get; set; }

        public int PersonId { get; set; }

        public Person Person { get; set; }

        public List<EmailContent> Contents { get; set; }
    }

    public class EmailConfiguration : EntityConfiguration<Email>
    {
        protected override void Configure(IEntityBuilder<Email> entityBuilder)
        {
            entityBuilder.HasMany(x => x.Contents)
                         .WithOne(x => x.Email)
                         .UsingForeignKey(x => x.EmailId);
        }
    }
}
