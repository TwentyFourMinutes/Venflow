using Venflow.Modeling.Definitions;
using Venflow.Modeling.Definitions.Builder;

namespace Venflow.Tests.Models
{
    public class ReverseEmailContentConfiguration : EntityConfiguration<ReverseEmailContent>
    {
        protected override void Configure(IEntityBuilder<ReverseEmailContent> entityBuilder)
        {
            entityBuilder.HasOne(x => x.Email)
                         .WithMany(x => x.Contents)
                         .UsingForeignKey(x => x.EmailId);
        }
    }
}
