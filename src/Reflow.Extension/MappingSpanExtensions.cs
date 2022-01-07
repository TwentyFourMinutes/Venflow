using Microsoft.VisualStudio.Text;

namespace Reflow.Extension
{
    internal static class MappingSpanExtensions
    {
        internal static SnapshotSpan GetSnapshotSpan(this IMappingSpan mappingTagSpan)
        {
            var buffer = mappingTagSpan.AnchorBuffer;
            var span = GetSpan(mappingTagSpan);
            var snapshot =
                mappingTagSpan.Start.GetPoint(buffer, PositionAffinity.Successor)!.Value.Snapshot;
            return new SnapshotSpan(snapshot, span);
        }

        private static Span GetSpan(IMappingSpan mappingTagSpan)
        {
            var buffer = mappingTagSpan.AnchorBuffer;
            var startSnapshotPoint =
                mappingTagSpan.Start.GetPoint(buffer, PositionAffinity.Successor)!.Value;
            var endSnapshotPoint =
                mappingTagSpan.End.GetPoint(buffer, PositionAffinity.Successor)!.Value;
            var length = endSnapshotPoint.Position - startSnapshotPoint.Position;
            return new Span(startSnapshotPoint.Position, length);
        }
    }
}
