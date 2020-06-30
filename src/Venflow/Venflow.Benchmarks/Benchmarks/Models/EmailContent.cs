using System.ComponentModel.DataAnnotations.Schema;
using Venflow.Modeling.Definitions;

namespace Venflow.Benchmarks.Benchmarks.Models
{
    [Table("EmailContents")]
    public class EmailContent
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public int EmailId { get; set; }

        public Email Email { get; set; }
    }

    public class EmailContentConfiguration : EntityConfiguration<EmailContent>
    {
        protected override void Configure(IEntityBuilder<EmailContent> entityBuilder)
        {
            entityBuilder.MapId(x => x.Id, DatabaseGeneratedOption.Computed)
                         .MapToTable("EmailContents");
        }
    }
}
