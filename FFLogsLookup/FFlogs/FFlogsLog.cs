using System;
using System.Collections.Generic;
using FFLogsLookup.Game;

namespace FFLogsLookup.FFlogs
{
    public enum FFlogsPartition
    {
        None,
        GameNonEcho,
        GameEcho,
    }

    public struct ZoneData
    {
        public float Point { get; set; }
        public int   Rank  { get; set; }
        public int   Total { get; set; }
    }

    public struct EncounterDataKey
    {
        public GameEncounter EncounterId { get; set; }
        public GameJob       JobId       { get; set; }
    }

    public struct EncounterData
    {
        public float MaxRdps { get; set; }
        public float MaxPer  { get; set; }
        public float MedPer  { get; set; }
        public int   Kills   { get; set; }
    }

    public class FFlogsLog
    {
        public static FFlogsLog Empty { get; } = new FFlogsLog();

        public string CharName { get; set; }
        public GameServer CharServer { get; set; }

        public bool Hidden { get; set; }

        public DateTime UpdatedAtUtc { get; set; }

        public static string GetKey(string charName, GameServer gameServer) => $"{charName}@{gameServer}";

        public Dictionary<GameJob, ZoneData> RaidAllstarNe { get; } = new(); // non-echo
        //public Dictionary<GameJob, ZoneData> RaidAllstarEc { get; } = new(); // echo

        public Dictionary<EncounterDataKey, EncounterData> EncountersNe { get; } = new(); // non-echo
        //public Dictionary<EncounterDataKey, EncounterData> EncountersEc { get; } = new(); // echo
    }
}
