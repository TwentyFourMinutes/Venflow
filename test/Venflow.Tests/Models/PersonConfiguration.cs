using Venflow.Modeling.Definitions;
using Venflow.Modeling.Definitions.Builder;

namespace Venflow.Tests.Models
{
    public class PersonConfiguration : EntityConfiguration<Person>
    {
        protected override void Configure(IEntityBuilder<Person> entityBuilder)
        {
            entityBuilder.HasMany(x => x.Emails)
                         .WithOne(x => x.Person)
                         .UsingForeignKey(x => x.PersonId);

            entityBuilder.Column(x => x.DefaultValue)
                         .HasDefault();
        }
    }
}
