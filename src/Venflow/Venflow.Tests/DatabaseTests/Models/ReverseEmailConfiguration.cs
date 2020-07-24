using Venflow.Modeling.Definitions;
using Venflow.Modeling.Definitions.Builder;

namespace Venflow.Tests.DatabaseTests.Models
{
    public class ReverseEmailConfiguration : EntityConfiguration<ReverseEmail>
    {
        protected override void Configure(IEntityBuilder<ReverseEmail> entityBuilder)
        {
            entityBuilder.HasOne(x => x.Person)
                         .WithMany(x => x.Emails)
                         .UsingForeignKey(x => x.PersonId);
        }
    }
}
