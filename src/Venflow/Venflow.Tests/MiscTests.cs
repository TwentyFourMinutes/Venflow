using System.Linq;
using Xunit;

namespace Venflow.Tests
{
    public class MiscTests
    {
        [Fact]
        public void EnsureFormattableSqlStringBuilderUsesParameters()
        {
            var stringBuilder = new FormattableSqlStringBuilder();

            stringBuilder.AppendInterpolatedLine($"This {"is"} an interpolated {"string"}");
            stringBuilder.Append(".");
            stringBuilder.AppendInterpolated($"It parameterizes the {"argument"}.");
            stringBuilder.AppendLine("This is very nice number:");
            stringBuilder.AppendParameter(10);

            Assert.Equal(@"This @p0 an interpolated @p1
.It parameterizes the @p2.This is very nice number:
@p3", stringBuilder.Build());

            Assert.True(stringBuilder.Parameters.Select(x => (x.ParameterName, x.Value))
                                    .SequenceEqual(new (string, object)[] {
                                        ("@p0", "is"),
                                        ("@p1", "string"),
                                        ("@p2", "argument"),
                                        ("@p3", 10),
                                    }));
        }
    }
}
