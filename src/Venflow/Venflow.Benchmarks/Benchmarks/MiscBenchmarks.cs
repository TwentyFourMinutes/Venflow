namespace Venflow.Benchmarks.Benchmarks
{
    //public class MiscBenchmarks
    //{
    //    Dictionary<ulong, string> _flagMap;
    //    Dictionary<int, string> _repoMap;
    //    string[] _map;

    //    [GlobalSetup]
    //    public void Setup()
    //    {
    //        _map = new string[64];
    //        _flagMap = new Dictionary<ulong, string>();
    //        _repoMap = new Dictionary<int, string>();

    //        for (ulong i = 1; i < ulong.MaxValue; i *= 2)
    //        {
    //            if (i == 0)
    //                break;

    //            var firstBit = BitOperations.TrailingZeroCount(i);

    //            _map[firstBit] = i.ToString();

    //            _flagMap.Add(i, i.ToString());
    //        }

    //        _repoMap.Add("ThisIsMyPropertyName".GetHashCode(), "cyka");
    //    }

    //    [Benchmark]
    //    public string NormalMap()
    //    {
    //        return _map[BitOperations.TrailingZeroCount(0b1)];
    //    }

    //    [Benchmark]
    //    public string ReverseMap()
    //    {
    //        return _map[BitOperations.LeadingZeroCount(0b1)];
    //    }

    //    //[Benchmark]
    //    //public string FlagMap()
    //    //{
    //    //    return _flagMap[0b100000000];
    //    //}

    //    //[Benchmark]
    //    //public string RepoDbMap()
    //    //{
    //    //    return _repoMap["ThisIsMyPropertyName".GetHashCode()];
    //    //}
    //}
}
