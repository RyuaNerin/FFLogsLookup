using System.Numerics;
using FFLogsLookup.Game;

namespace FFLogsLookup.FFlogs
{
    internal static class FFlogsColor
    {
        public static Vector4 GetJobColor(this GameJob gameJob)
        {
            return gameJob switch
            {
                GameJob.Astrologian => new Vector4(255.0f / 255.0f, 231.0f / 255.0f,  74.0f / 255.0f, 1.0f),
                GameJob.Bard        => new Vector4(145.0f / 255.0f, 150.0f / 255.0f, 186.0f / 255.0f, 1.0f),
                GameJob.BlackMage   => new Vector4(165.0f / 255.0f, 121.0f / 255.0f, 214.0f / 255.0f, 1.0f),
                GameJob.Dancer      => new Vector4(226.0f / 255.0f, 176.0f / 255.0f, 175.0f / 255.0f, 1.0f),
                GameJob.DarkKnight  => new Vector4(209.0f / 255.0f,  38.0f / 255.0f, 204.0f / 255.0f, 1.0f),
                GameJob.Dragoon     => new Vector4( 65.0f / 255.0f, 100.0f / 255.0f, 205.0f / 255.0f, 1.0f),
                GameJob.Gunbreaker  => new Vector4(121.0f / 255.0f, 109.0f / 255.0f,  48.0f / 255.0f, 1.0f),
                GameJob.Machinist   => new Vector4(110.0f / 255.0f, 225.0f / 255.0f, 214.0f / 255.0f, 1.0f),
                GameJob.Monk        => new Vector4(214.0f / 255.0f, 156.0f / 255.0f,   0.0f / 255.0f, 1.0f),
                GameJob.Ninja       => new Vector4(175.0f / 255.0f,  25.0f / 255.0f, 100.0f / 255.0f, 1.0f),
                GameJob.Paladin     => new Vector4(168.0f / 255.0f, 210.0f / 255.0f, 230.0f / 255.0f, 1.0f),
                GameJob.RedMage     => new Vector4(232.0f / 255.0f, 123.0f / 255.0f, 123.0f / 255.0f, 1.0f),
                GameJob.Samurai     => new Vector4(228.0f / 255.0f, 109.0f / 255.0f,   4.0f / 255.0f, 1.0f),
                GameJob.Scholar     => new Vector4(134.0f / 255.0f,  87.0f / 255.0f, 255.0f / 255.0f, 1.0f),
                GameJob.Summoner    => new Vector4( 45.0f / 255.0f, 155.0f / 255.0f, 120.0f / 255.0f, 1.0f),
                GameJob.Warrior     => new Vector4(207.0f / 255.0f,  38.0f / 255.0f,  33.0f / 255.0f, 1.0f),
                GameJob.WhiteMage   => new Vector4(255.0f / 255.0f, 240.0f / 255.0f, 220.0f / 255.0f, 1.0f),

                _ => new(1.0f, 1.0f, 1.0f, 1.0f),
            };
        }

        public static Vector4 GetColor(float percent)
        {
            return percent switch
            {
                float f when (f == 100) => new Vector4(229.0f / 255.0f, 204.0f / 255.0f, 128.0f / 255.0f, 1.0f),
                float f when (f  >= 99) => new Vector4(226.0f / 255.0f, 104.0f / 255.0f, 168.0f / 255.0f, 1.0f),
                float f when (f  >= 95) => new Vector4(255.0f / 255.0f, 128.0f / 255.0f,   0.0f / 255.0f, 1.0f),
                float f when (f  >= 75) => new Vector4(163.0f / 255.0f,  53.0f / 255.0f, 238.0f / 255.0f, 1.0f),
                float f when (f  >= 50) => new Vector4(  0.0f / 255.0f, 112.0f / 255.0f, 255.0f / 255.0f, 1.0f),
                float f when (f  >= 25) => new Vector4( 30.0f / 255.0f, 255.0f / 255.0f,   0.0f / 255.0f, 1.0f),
                _                       => new Vector4(102.0f / 255.0f, 102.0f / 255.0f, 102.0f / 255.0f, 1.0f),
            };
        }
    }
}
