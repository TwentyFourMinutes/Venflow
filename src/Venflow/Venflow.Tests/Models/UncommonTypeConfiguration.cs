using Venflow.Modeling.Definitions;
using Venflow.Modeling.Definitions.Builder;

namespace Venflow.Tests.Models
{
    public class UncommonTypeConfiguration : EntityConfiguration<UncommonType>
    {
        protected override void Configure(IEntityBuilder<UncommonType> entityBuilder)
        {
            entityBuilder.MapPostgresEnum(x => x.PostgreEnum);
            entityBuilder.MapPostgresEnum(x => x.NPostgreEnum);
        }
    }
}
