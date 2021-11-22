namespace Reflow.Analyzer.Models
{
    internal enum RelationType : byte
    {
        OneToOne,
        OneToMany,
        ManyToOne
    }

    internal static class RelationTypeExtensions
    {
        internal static RelationType GetMirror(this RelationType relationType)
        {
            return relationType switch
            {
                RelationType.ManyToOne => RelationType.OneToMany,
                RelationType.OneToMany => RelationType.ManyToOne,
                RelationType.OneToOne => RelationType.OneToOne,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
