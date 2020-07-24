using Venflow.Modeling.Definitions;
using Venflow.Modeling.Definitions.Builder;

namespace Venflow.Tests.DatabaseTests.Models
{
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
