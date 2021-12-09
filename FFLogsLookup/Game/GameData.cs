using System;
using System.Collections.Generic;
using System.Linq;

namespace FFLogsLookup.Game
{
    public enum GameEncounter : short
    {
        E9s        = 73,
        E10s       = 74,
        E11s       = 75,
        E12sDoor   = 76,
        E12sOracle = 77,

        Tea = 1050,
        Ucob = 1047,
        Uwu = 1048,
    };

    public enum GameServer : byte
    {
        Moogle,
        Chocobo,
        Tonberry,
        Carbuncle,
        Fenrir,
    }

    public enum GameJob : byte
    {
        None = 0,
        Best = 1,

        Paladin    = 11,
        Warrior    = 12,
        DarkKnight = 13,
        Gunbreaker = 14,

        WhiteMage   = 20,
        Scholar     = 21,
        Astrologian = 22,
        //Sage        = 23,

        Monk    = 31,
        Dragoon = 32,
        Ninja   = 33,
        Samurai = 34,
        //Reaper  = 35,

        Bard      = 40,
        Machinist = 41,
        Dancer    = 42,

        BlackMage = 50,
        Summoner  = 51,
        RedMage   = 52,
    };

    internal static class GameData
    {
        public static Dictionary<GameServer, List<string>> ServerAlias { get; } = new()
        {
            { GameServer.Moogle   , new() { "모그리", "모", "ㅁ", "KrMoogle"   , "moogle"   , "m"  } },
            { GameServer.Chocobo  , new() { "초코보", "초", "ㅊ", "KrChocobo"  , "chocobo"  , "ch" } },
            { GameServer.Tonberry , new() { "톤베리", "톤", "ㅌ", "KrTonberry" , "tonberry" , "t"  } },
            { GameServer.Carbuncle, new() { "카벙클", "카", "ㅋ", "KrCarbuncle", "carbuncle", "ca" } },
            { GameServer.Fenrir   , new() { "펜리르", "펜", "ㅍ", "KrFenrir"   , "fenrir"   , "f"  } },
        };

        /// <exception cref="Exception"></exception>
        public static GameServer GetGameServer(string gameServer)
        {
            return ServerAlias.First(e => e.Value.Contains(gameServer, StringComparer.CurrentCultureIgnoreCase)).Key;
        }

        public static string S(this GameServer gameServer)
        {
            return gameServer switch
            {
                GameServer.Moogle    => "모그리",
                GameServer.Chocobo   => "초코보",
                GameServer.Tonberry  => "톤베리",
                GameServer.Carbuncle => "카벙클",
                GameServer.Fenrir    => "펜리르",
                _ => throw new NotImplementedException(),
            };
        }

        public static GameJob ParseGameJob(string value)
        {
            return value switch
            {
                "Astrologian" => GameJob.Astrologian,
                "Bard"        => GameJob.Bard,
                "BlackMage"   => GameJob.BlackMage,
                "DarkKnight"  => GameJob.DarkKnight,
                "Dragoon"     => GameJob.Dragoon,
                "Machinist"   => GameJob.Machinist,
                "Monk"        => GameJob.Monk,
                "Ninja"       => GameJob.Ninja,
                "Paladin"     => GameJob.Paladin,
                "Scholar"     => GameJob.Scholar,
                "Summoner"    => GameJob.Summoner,
                "Warrior"     => GameJob.Warrior,
                "WhiteMage"   => GameJob.WhiteMage,
                "RedMage"     => GameJob.RedMage,
                "Samurai"     => GameJob.Samurai,
                "Dancer"      => GameJob.Dancer,
                "Gunbreaker"  => GameJob.Gunbreaker,
                //"Reaper"      => GameJob.Reaper,
                //"Sage"        => GameJob.Sage,
                _ => throw new Exception("Cannot unmarshal type Spec"),
            };
        }

        public static string GetDescription(this GameJob gameJob)
        {
            return gameJob switch
            {
                GameJob.Best        => "BEST",
                GameJob.Astrologian => "점성술사",
                GameJob.Bard        => "음유시인",
                GameJob.BlackMage   => "흑마도사",
                GameJob.Dancer      => "무도가",
                GameJob.DarkKnight  => "암흑기사",
                GameJob.Dragoon     => "용기사",
                GameJob.Gunbreaker  => "건브레이커",
                GameJob.Machinist   => "기공사",
                GameJob.Monk        => "몽크",
                GameJob.Ninja       => "닌자",
                GameJob.Paladin     => "나이트",
                GameJob.RedMage     => "적마도사",
                GameJob.Samurai     => "사무라이",
                GameJob.Scholar     => "학자",
                GameJob.Summoner    => "소환사",
                GameJob.Warrior     => "전사",
                GameJob.WhiteMage   => "백마도사",

                _ => "",
            };
        }

        public static string GetGlyph(this GameJob gameJob)
        {
            return gameJob switch
            {
                GameJob.Astrologian => "\uf033",
                GameJob.Bard        => "\uf023",
                GameJob.BlackMage   => "\uf025",
                GameJob.Dancer      => "\uf038",
                GameJob.DarkKnight  => "\uf032",
                GameJob.Dragoon     => "\uf022",
                GameJob.Gunbreaker  => "\uf037",
                GameJob.Machinist   => "\uf031",
                GameJob.Monk        => "\uf020",
                GameJob.Ninja       => "\uf030",
                GameJob.Paladin     => "\uf019",
                GameJob.RedMage     => "\uf035",
                GameJob.Samurai     => "\uf034",
                GameJob.Scholar     => "\uf028",
                GameJob.Summoner    => "\uf027",
                GameJob.Warrior     => "\uf021",
                GameJob.WhiteMage   => "\uf024",

                _ => null,
            };
        }

        public static bool IsRaids(this GameEncounter gameEncounter)
            => gameEncounter is GameEncounter.E9s or
                                GameEncounter.E10s or
                                GameEncounter.E11s or
                                GameEncounter.E12sDoor or
                                GameEncounter.E12sOracle;
    }
}
