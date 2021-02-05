using System;
using System.Globalization;

namespace Venflow.Score
{

    public static class StringExtensions
    {
        public static string TrimEnd(this string source, string value)
        {
            if (!source.EndsWith(value))
                return source;

            return source.Remove(source.LastIndexOf(value));
        }
        public static double GetNanoSecondTime(this string val)
        {
            var splittedVal = val.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (splittedVal.Length != 2)
            {
                throw new InvalidOperationException($"The format {val} is invalid.");
            }

            var time = double.Parse(splittedVal[0], NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat);

            switch (splittedVal[1])
            {
                case "ns":
                    break;
                case "μs":
                    time *= 1000;
                    break;
                case "ms":
                    time *= 1000000;
                    break;
                case "s":
                    time *= 1000000000;
                    break;
                default:
                    throw new InvalidOperationException($"The time format {splittedVal[1]} is invalid.");
            }

            return time;
        }

        public static double GetAllocation(this string val)
        {
            var splittedVal = val.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (splittedVal.Length != 2)
            {
                throw new InvalidOperationException($"The format {val} is invalid.");
            }

            var size = double.Parse(splittedVal[0], NumberStyles.Number, CultureInfo.InvariantCulture.NumberFormat);

            switch (splittedVal[1])
            {
                case "KB":
                    break;
                default:
                    throw new InvalidOperationException($"The size format {splittedVal[1]} is invalid.");
            }

            return size;
        }
    }
}
