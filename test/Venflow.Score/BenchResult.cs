namespace Venflow.Score
{
    public class BenchResult
    {
        public string Method { get; set; } = null!;
        public string Job { get; set; } = null!;
        public string Mean { get; set; } = null!;
        public string Allocated { get; set; } = null!;
    }

    public class BatchBenchResult : BenchResult
    {
        public int? BatchCount { get; set; }
    }
}
