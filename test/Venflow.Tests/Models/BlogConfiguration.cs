using Venflow.Modeling.Definitions;
using Venflow.Modeling.Definitions.Builder;

namespace Venflow.Tests.Models
{

    public class BlogConfiguration : EntityConfiguration<Blog>
    {
        protected override void Configure(IEntityBuilder<Blog> entityBuilder)
        {
            entityBuilder.MapId(x => x.Id, System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);

            entityBuilder.HasOne(x => x.User)
                         .WithMany(x => x.Blogs)
                         .UsingForeignKey(x => x.UserId);
        }
    }
}
