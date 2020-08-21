using System.Collections.Generic;
using System.Linq;

namespace Venflow.Score
{

    public class Orm
    {
        public double BenchScore { get; private set; }
        public double AllocScore { get; private set; }
        public bool HasCurrentBench { get; private set; }
        public string OrmName { get; }

        private readonly List<double> _benchTimeScores;
        private readonly List<double> _benchAllocScores;

        public Orm(string ormName)
        {
            OrmName = ormName;

            _benchTimeScores = new List<double>();
            _benchAllocScores = new List<double>();
        }

        public void AddBenchScore(double benchScore, double allocScore)
        {
            _benchTimeScores.Add(benchScore);
            _benchAllocScores.Add(allocScore);

            HasCurrentBench = true;
        }

        public void FinishBench(double lowestBenchScore, double lowestAllocScore)
        {
            if (_benchTimeScores.Count != 0 &&
                !_benchTimeScores.Any(x => x == lowestBenchScore))
            {

                BenchScore += _benchTimeScores.Min() / lowestBenchScore - 1;
            }

            _benchTimeScores.Clear();

            if (_benchAllocScores.Count != 0 &&
                !_benchAllocScores.Any(x => x == lowestAllocScore))
            {

                AllocScore += (_benchAllocScores.Min() / lowestAllocScore - 1) / 10; // Divided, in order to lower the impact by memory usage on the final score.
            }

            _benchAllocScores.Clear();

            HasCurrentBench = false;
        }
    }
}
