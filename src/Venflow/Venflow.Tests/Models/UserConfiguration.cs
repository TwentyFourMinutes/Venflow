using Venflow.Modeling.Definitions;
using Venflow.Modeling.Definitions.Builder;
using Venflow.Shared;

namespace Venflow.Tests.Models
{

    public class UserConfiguration : EntityConfiguration<User>
    {
        protected override void Configure(IEntityBuilder<User> entityBuilder)
        {
            entityBuilder.MapId(x => x.Id, System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);
        }
    }
}
