﻿namespace Reflow.Analyzer.Shared
{
    internal static class KnownPublicKeys
    {
        internal static readonly IReadOnlyList<byte> Reflow = typeof(KnownPublicKeys).Assembly
            .GetName()
            .GetPublicKey();

        internal static readonly IReadOnlyList<byte> SystemTextJson = new byte[160]
        {
            0,
            36,
            0,
            0,
            4,
            128,
            0,
            0,
            148,
            0,
            0,
            0,
            6,
            2,
            0,
            0,
            0,
            36,
            0,
            0,
            82,
            83,
            65,
            49,
            0,
            4,
            0,
            0,
            1,
            0,
            1,
            0,
            75,
            134,
            196,
            203,
            120,
            84,
            155,
            52,
            186,
            182,
            26,
            59,
            24,
            0,
            226,
            59,
            254,
            181,
            179,
            236,
            57,
            0,
            116,
            4,
            21,
            54,
            167,
            227,
            203,
            217,
            127,
            95,
            4,
            207,
            15,
            133,
            113,
            85,
            168,
            146,
            142,
            170,
            41,
            235,
            253,
            17,
            207,
            187,
            173,
            59,
            167,
            14,
            254,
            167,
            189,
            163,
            34,
            108,
            106,
            141,
            55,
            10,
            76,
            211,
            3,
            247,
            20,
            72,
            107,
            110,
            188,
            34,
            89,
            133,
            166,
            56,
            71,
            30,
            110,
            245,
            113,
            204,
            146,
            164,
            97,
            60,
            0,
            184,
            250,
            101,
            214,
            28,
            206,
            224,
            203,
            229,
            243,
            99,
            48,
            201,
            160,
            31,
            65,
            131,
            85,
            159,
            27,
            239,
            36,
            204,
            41,
            23,
            198,
            217,
            19,
            227,
            165,
            65,
            51,
            58,
            29,
            5,
            217,
            190,
            210,
            43,
            56,
            203
        };

        internal static readonly IReadOnlyList<byte> NewtonsoftJson = new byte[160]
        {
            0,
            36,
            0,
            0,
            4,
            128,
            0,
            0,
            148,
            0,
            0,
            0,
            6,
            2,
            0,
            0,
            0,
            36,
            0,
            0,
            82,
            83,
            65,
            49,
            0,
            4,
            0,
            0,
            1,
            0,
            1,
            0,
            245,
            97,
            223,
            39,
            124,
            108,
            11,
            73,
            125,
            98,
            144,
            50,
            180,
            16,
            205,
            207,
            40,
            110,
            83,
            124,
            5,
            71,
            36,
            247,
            255,
            160,
            22,
            67,
            69,
            246,
            43,
            62,
            100,
            32,
            41,
            215,
            168,
            12,
            195,
            81,
            145,
            137,
            85,
            50,
            140,
            74,
            220,
            138,
            4,
            136,
            35,
            239,
            144,
            176,
            207,
            56,
            234,
            125,
            176,
            215,
            41,
            202,
            242,
            182,
            51,
            195,
            186,
            190,
            8,
            176,
            49,
            1,
            152,
            193,
            8,
            25,
            149,
            193,
            144,
            41,
            188,
            103,
            81,
            147,
            116,
            78,
            171,
            157,
            115,
            69,
            184,
            166,
            114,
            88,
            236,
            23,
            209,
            18,
            206,
            189,
            187,
            178,
            162,
            129,
            72,
            125,
            206,
            234,
            251,
            157,
            131,
            170,
            147,
            15,
            50,
            16,
            63,
            190,
            29,
            41,
            17,
            66,
            91,
            197,
            116,
            64,
            2,
            199
        };
    }
}
