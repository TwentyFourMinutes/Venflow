﻿using System.Runtime.CompilerServices;
using Fody;

#if NET5_0_OR_GREATER
[module: SkipLocalsInit]
#endif

[assembly: InternalsVisibleTo("Venflow.Dynamic" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Venflow.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Venflow.Extensions.Logging" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Venflow.NewtonsoftJson" + AssemblyInfo.PublicKey)]

[assembly: ConfigureAwait(false)]

namespace Venflow
{
    internal static class AssemblyInfo
    {
        internal const string PublicKey = ", PublicKey=" +
            "002400000480000094000000060200000024000052534131000400000100010099f3f8321d5f56" +
            "b9152b82741511adb2186619b29d92bcf32d16cbb2d6751ecdb4ea393cbd3e75648baf7ab3deb4" +
            "e15d8fb26de92c2eddf9f38d1d2749ee9a7ab31006caae731ff601d950b7cf87750fea04ddf857" +
            "d6187ea2060944d04cfd37e3bf82ec1bd94e7b912733bbb403cdc348a15b4ab9f29bf999f13f9d" +
            "08a576b3";
    }
}
