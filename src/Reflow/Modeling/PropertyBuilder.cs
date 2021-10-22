using NpgsqlTypes;

namespace Reflow.Modeling
{
    public interface IPropertyBuilder
    {
        IPropertyBuilder IsId();
        IPropertyBuilder HasName(string name);
        IPropertyBuilder HasType(NpgsqlDbType dbType);
        IPropertyBuilder HasDefault();
    }
}
