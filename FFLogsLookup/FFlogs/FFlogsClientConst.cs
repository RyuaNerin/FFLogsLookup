using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FFLogsLookup.Game;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace FFLogsLookup.FFlogs
{
    internal partial class FFlogsClient
    {
        private static readonly string BaseQuery = BuildBaseQuery();

        private static string BuildBaseQuery()
        {
            var sb = new StringBuilder();

            sb.Append(@"
{
  characterData {
    character(name: ""{0}"", serverSlug:""{1}"", serverRegion: ""kr"")
    {
      hidden
");

            sb.Append(@"
      eden_promise_ne:   zoneRankings(zoneID: 38, difficulty: 101, includePrivateLogs: true)
      eden_promise_ec: zoneRankings(zoneID: 38, difficulty: 101, includePrivateLogs: true, partition: 17)
");

            /*
            sb.Append(@"
      eden_promise_ec: zoneRankings(zoneID: 38, difficulty: 101, includePrivateLogs: true, partition: 17)
");
            */

            foreach (var job in Enum.GetValues<GameJob>())
            {
                sb.AppendFormat(@"
      e9s_ne_{0}  : encounterRankings(encounterID: 73, difficulty: 101, includePrivateLogs: true, specName: ""{0}"")
      e10s_ne_{0} : encounterRankings(encounterID: 74, difficulty: 101, includePrivateLogs: true, specName: ""{0}"")
      e11s_ne_{0} : encounterRankings(encounterID: 75, difficulty: 101, includePrivateLogs: true, specName: ""{0}"")
      e12sd_ne_{0}: encounterRankings(encounterID: 76, difficulty: 101, includePrivateLogs: true, specName: ""{0}"")
      e12so_ne_{0}: encounterRankings(encounterID: 77, difficulty: 101, includePrivateLogs: true, specName: ""{0}"")

      tea_{0} : encounterRankings(encounterID: 1050, includePrivateLogs: true, specName: ""{0}"")
      ucob_{0}: encounterRankings(encounterID: 1048, includePrivateLogs: true, specName: ""{0}"")
      uwu_{0} : encounterRankings(encounterID: 1047, includePrivateLogs: true, specName: ""{0}"")
",
                job.ToString());

                /*
                sb.AppendFormat(@"
      e9s_ec_{0}  : encounterRankings(encounterID: 73, difficulty: 101, includePrivateLogs: true, specName: ""{0}"", partition: 17)
      e10s_ec_{0} : encounterRankings(encounterID: 74, difficulty: 101, includePrivateLogs: true, specName: ""{0}"", partition: 17)
      e11s_ec_{0} : encounterRankings(encounterID: 75, difficulty: 101, includePrivateLogs: true, specName: ""{0}"", partition: 17)
      e12sd_ec_{0}: encounterRankings(encounterID: 76, difficulty: 101, includePrivateLogs: true, specName: ""{0}"", partition: 17)
      e12so_ec_{0}: encounterRankings(encounterID: 77, difficulty: 101, includePrivateLogs: true, specName: ""{0}"", partition: 17)
",
                job.ToString());
                */
            }

            sb.Append(@"    
    }
  }
}");
            var jo = new JObject
            {
                { "query", Regex.Replace(sb.ToString(), " {2,}|\r\n *| *\r\n", " ", RegexOptions.Compiled | RegexOptions.IgnoreCase) }
            };
            return Regex.Replace(jo.ToString(Formatting.None), @"({(?!\d)|(?<!\d)})", "$1$1", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        private class ResponseData
        {
            [JsonProperty("data")]
            public Data Data { get; set; }
        }

        private class Data
        {
            [JsonProperty("characterData")]
            public CharacterData CharacterData { get; set; }
        }

        private class CharacterData
        {
            [JsonProperty("character")]
            public Character Character { get; set; }
        }

        private class Character
        {
            [JsonProperty("hidden")]
            public bool Hidden { get; set; }


            [JsonProperty("eden_promise_ne")] public ZoneRankings EdenPromiseNe { get; set; }
            [JsonProperty("eden_promise_ec")] public ZoneRankings EdenPromiseEc { get; set; }

            [JsonProperty("e9s_ne_Astrologian")] public EncounterRankings E9SNeAstrologian { get; set; }
            [JsonProperty("e9s_ec_Astrologian")] public EncounterRankings E9SEcAstrologian { get; set; }
            [JsonProperty("e10s_ne_Astrologian")] public EncounterRankings E10SNeAstrologian { get; set; }
            [JsonProperty("e10s_ec_Astrologian")] public EncounterRankings E10SEcAstrologian { get; set; }
            [JsonProperty("e11s_ne_Astrologian")] public EncounterRankings E11SNeAstrologian { get; set; }
            [JsonProperty("e11s_ec_Astrologian")] public EncounterRankings E11SEcAstrologian { get; set; }
            [JsonProperty("e12sd_ne_Astrologian")] public EncounterRankings E12SdNeAstrologian { get; set; }
            [JsonProperty("e12sd_ec_Astrologian")] public EncounterRankings E12SdEcAstrologian { get; set; }
            [JsonProperty("e12so_ne_Astrologian")] public EncounterRankings E12SoNeAstrologian { get; set; }
            [JsonProperty("e12so_ec_Astrologian")] public EncounterRankings E12SoEcAstrologian { get; set; }
            [JsonProperty("tea_Astrologian")] public EncounterRankings TeaAstrologian { get; set; }
            [JsonProperty("ucob_Astrologian")] public EncounterRankings UcobAstrologian { get; set; }
            [JsonProperty("uwu_Astrologian")] public EncounterRankings UwuAstrologian { get; set; }
            [JsonProperty("e9s_ne_Bard")] public EncounterRankings E9SNeBard { get; set; }
            [JsonProperty("e9s_ec_Bard")] public EncounterRankings E9SEcBard { get; set; }
            [JsonProperty("e10s_ne_Bard")] public EncounterRankings E10SNeBard { get; set; }
            [JsonProperty("e10s_ec_Bard")] public EncounterRankings E10SEcBard { get; set; }
            [JsonProperty("e11s_ne_Bard")] public EncounterRankings E11SNeBard { get; set; }
            [JsonProperty("e11s_ec_Bard")] public EncounterRankings E11SEcBard { get; set; }
            [JsonProperty("e12sd_ne_Bard")] public EncounterRankings E12SdNeBard { get; set; }
            [JsonProperty("e12sd_ec_Bard")] public EncounterRankings E12SdEcBard { get; set; }
            [JsonProperty("e12so_ne_Bard")] public EncounterRankings E12SoNeBard { get; set; }
            [JsonProperty("e12so_ec_Bard")] public EncounterRankings E12SoEcBard { get; set; }
            [JsonProperty("tea_Bard")] public EncounterRankings TeaBard { get; set; }
            [JsonProperty("ucob_Bard")] public EncounterRankings UcobBard { get; set; }
            [JsonProperty("uwu_Bard")] public EncounterRankings UwuBard { get; set; }
            [JsonProperty("e9s_ne_BlackMage")] public EncounterRankings E9SNeBlackMage { get; set; }
            [JsonProperty("e9s_ec_BlackMage")] public EncounterRankings E9SEcBlackMage { get; set; }
            [JsonProperty("e10s_ne_BlackMage")] public EncounterRankings E10SNeBlackMage { get; set; }
            [JsonProperty("e10s_ec_BlackMage")] public EncounterRankings E10SEcBlackMage { get; set; }
            [JsonProperty("e11s_ne_BlackMage")] public EncounterRankings E11SNeBlackMage { get; set; }
            [JsonProperty("e11s_ec_BlackMage")] public EncounterRankings E11SEcBlackMage { get; set; }
            [JsonProperty("e12sd_ne_BlackMage")] public EncounterRankings E12SdNeBlackMage { get; set; }
            [JsonProperty("e12sd_ec_BlackMage")] public EncounterRankings E12SdEcBlackMage { get; set; }
            [JsonProperty("e12so_ne_BlackMage")] public EncounterRankings E12SoNeBlackMage { get; set; }
            [JsonProperty("e12so_ec_BlackMage")] public EncounterRankings E12SoEcBlackMage { get; set; }
            [JsonProperty("tea_BlackMage")] public EncounterRankings TeaBlackMage { get; set; }
            [JsonProperty("ucob_BlackMage")] public EncounterRankings UcobBlackMage { get; set; }
            [JsonProperty("uwu_BlackMage")] public EncounterRankings UwuBlackMage { get; set; }
            [JsonProperty("e9s_ne_DarkKnight")] public EncounterRankings E9SNeDarkKnight { get; set; }
            [JsonProperty("e9s_ec_DarkKnight")] public EncounterRankings E9SEcDarkKnight { get; set; }
            [JsonProperty("e10s_ne_DarkKnight")] public EncounterRankings E10SNeDarkKnight { get; set; }
            [JsonProperty("e10s_ec_DarkKnight")] public EncounterRankings E10SEcDarkKnight { get; set; }
            [JsonProperty("e11s_ne_DarkKnight")] public EncounterRankings E11SNeDarkKnight { get; set; }
            [JsonProperty("e11s_ec_DarkKnight")] public EncounterRankings E11SEcDarkKnight { get; set; }
            [JsonProperty("e12sd_ne_DarkKnight")] public EncounterRankings E12SdNeDarkKnight { get; set; }
            [JsonProperty("e12sd_ec_DarkKnight")] public EncounterRankings E12SdEcDarkKnight { get; set; }
            [JsonProperty("e12so_ne_DarkKnight")] public EncounterRankings E12SoNeDarkKnight { get; set; }
            [JsonProperty("e12so_ec_DarkKnight")] public EncounterRankings E12SoEcDarkKnight { get; set; }
            [JsonProperty("tea_DarkKnight")] public EncounterRankings TeaDarkKnight { get; set; }
            [JsonProperty("ucob_DarkKnight")] public EncounterRankings UcobDarkKnight { get; set; }
            [JsonProperty("uwu_DarkKnight")] public EncounterRankings UwuDarkKnight { get; set; }
            [JsonProperty("e9s_ne_Dragoon")] public EncounterRankings E9SNeDragoon { get; set; }
            [JsonProperty("e9s_ec_Dragoon")] public EncounterRankings E9SEcDragoon { get; set; }
            [JsonProperty("e10s_ne_Dragoon")] public EncounterRankings E10SNeDragoon { get; set; }
            [JsonProperty("e10s_ec_Dragoon")] public EncounterRankings E10SEcDragoon { get; set; }
            [JsonProperty("e11s_ne_Dragoon")] public EncounterRankings E11SNeDragoon { get; set; }
            [JsonProperty("e11s_ec_Dragoon")] public EncounterRankings E11SEcDragoon { get; set; }
            [JsonProperty("e12sd_ne_Dragoon")] public EncounterRankings E12SdNeDragoon { get; set; }
            [JsonProperty("e12sd_ec_Dragoon")] public EncounterRankings E12SdEcDragoon { get; set; }
            [JsonProperty("e12so_ne_Dragoon")] public EncounterRankings E12SoNeDragoon { get; set; }
            [JsonProperty("e12so_ec_Dragoon")] public EncounterRankings E12SoEcDragoon { get; set; }
            [JsonProperty("tea_Dragoon")] public EncounterRankings TeaDragoon { get; set; }
            [JsonProperty("ucob_Dragoon")] public EncounterRankings UcobDragoon { get; set; }
            [JsonProperty("uwu_Dragoon")] public EncounterRankings UwuDragoon { get; set; }
            [JsonProperty("e9s_ne_Machinist")] public EncounterRankings E9SNeMachinist { get; set; }
            [JsonProperty("e9s_ec_Machinist")] public EncounterRankings E9SEcMachinist { get; set; }
            [JsonProperty("e10s_ne_Machinist")] public EncounterRankings E10SNeMachinist { get; set; }
            [JsonProperty("e10s_ec_Machinist")] public EncounterRankings E10SEcMachinist { get; set; }
            [JsonProperty("e11s_ne_Machinist")] public EncounterRankings E11SNeMachinist { get; set; }
            [JsonProperty("e11s_ec_Machinist")] public EncounterRankings E11SEcMachinist { get; set; }
            [JsonProperty("e12sd_ne_Machinist")] public EncounterRankings E12SdNeMachinist { get; set; }
            [JsonProperty("e12sd_ec_Machinist")] public EncounterRankings E12SdEcMachinist { get; set; }
            [JsonProperty("e12so_ne_Machinist")] public EncounterRankings E12SoNeMachinist { get; set; }
            [JsonProperty("e12so_ec_Machinist")] public EncounterRankings E12SoEcMachinist { get; set; }
            [JsonProperty("tea_Machinist")] public EncounterRankings TeaMachinist { get; set; }
            [JsonProperty("ucob_Machinist")] public EncounterRankings UcobMachinist { get; set; }
            [JsonProperty("uwu_Machinist")] public EncounterRankings UwuMachinist { get; set; }
            [JsonProperty("e9s_ne_Monk")] public EncounterRankings E9SNeMonk { get; set; }
            [JsonProperty("e9s_ec_Monk")] public EncounterRankings E9SEcMonk { get; set; }
            [JsonProperty("e10s_ne_Monk")] public EncounterRankings E10SNeMonk { get; set; }
            [JsonProperty("e10s_ec_Monk")] public EncounterRankings E10SEcMonk { get; set; }
            [JsonProperty("e11s_ne_Monk")] public EncounterRankings E11SNeMonk { get; set; }
            [JsonProperty("e11s_ec_Monk")] public EncounterRankings E11SEcMonk { get; set; }
            [JsonProperty("e12sd_ne_Monk")] public EncounterRankings E12SdNeMonk { get; set; }
            [JsonProperty("e12sd_ec_Monk")] public EncounterRankings E12SdEcMonk { get; set; }
            [JsonProperty("e12so_ne_Monk")] public EncounterRankings E12SoNeMonk { get; set; }
            [JsonProperty("e12so_ec_Monk")] public EncounterRankings E12SoEcMonk { get; set; }
            [JsonProperty("tea_Monk")] public EncounterRankings TeaMonk { get; set; }
            [JsonProperty("ucob_Monk")] public EncounterRankings UcobMonk { get; set; }
            [JsonProperty("uwu_Monk")] public EncounterRankings UwuMonk { get; set; }
            [JsonProperty("e9s_ne_Ninja")] public EncounterRankings E9SNeNinja { get; set; }
            [JsonProperty("e9s_ec_Ninja")] public EncounterRankings E9SEcNinja { get; set; }
            [JsonProperty("e10s_ne_Ninja")] public EncounterRankings E10SNeNinja { get; set; }
            [JsonProperty("e10s_ec_Ninja")] public EncounterRankings E10SEcNinja { get; set; }
            [JsonProperty("e11s_ne_Ninja")] public EncounterRankings E11SNeNinja { get; set; }
            [JsonProperty("e11s_ec_Ninja")] public EncounterRankings E11SEcNinja { get; set; }
            [JsonProperty("e12sd_ne_Ninja")] public EncounterRankings E12SdNeNinja { get; set; }
            [JsonProperty("e12sd_ec_Ninja")] public EncounterRankings E12SdEcNinja { get; set; }
            [JsonProperty("e12so_ne_Ninja")] public EncounterRankings E12SoNeNinja { get; set; }
            [JsonProperty("e12so_ec_Ninja")] public EncounterRankings E12SoEcNinja { get; set; }
            [JsonProperty("tea_Ninja")] public EncounterRankings TeaNinja { get; set; }
            [JsonProperty("ucob_Ninja")] public EncounterRankings UcobNinja { get; set; }
            [JsonProperty("uwu_Ninja")] public EncounterRankings UwuNinja { get; set; }
            [JsonProperty("e9s_ne_Paladin")] public EncounterRankings E9SNePaladin { get; set; }
            [JsonProperty("e9s_ec_Paladin")] public EncounterRankings E9SEcPaladin { get; set; }
            [JsonProperty("e10s_ne_Paladin")] public EncounterRankings E10SNePaladin { get; set; }
            [JsonProperty("e10s_ec_Paladin")] public EncounterRankings E10SEcPaladin { get; set; }
            [JsonProperty("e11s_ne_Paladin")] public EncounterRankings E11SNePaladin { get; set; }
            [JsonProperty("e11s_ec_Paladin")] public EncounterRankings E11SEcPaladin { get; set; }
            [JsonProperty("e12sd_ne_Paladin")] public EncounterRankings E12SdNePaladin { get; set; }
            [JsonProperty("e12sd_ec_Paladin")] public EncounterRankings E12SdEcPaladin { get; set; }
            [JsonProperty("e12so_ne_Paladin")] public EncounterRankings E12SoNePaladin { get; set; }
            [JsonProperty("e12so_ec_Paladin")] public EncounterRankings E12SoEcPaladin { get; set; }
            [JsonProperty("tea_Paladin")] public EncounterRankings TeaPaladin { get; set; }
            [JsonProperty("ucob_Paladin")] public EncounterRankings UcobPaladin { get; set; }
            [JsonProperty("uwu_Paladin")] public EncounterRankings UwuPaladin { get; set; }
            [JsonProperty("e9s_ne_Scholar")] public EncounterRankings E9SNeScholar { get; set; }
            [JsonProperty("e9s_ec_Scholar")] public EncounterRankings E9SEcScholar { get; set; }
            [JsonProperty("e10s_ne_Scholar")] public EncounterRankings E10SNeScholar { get; set; }
            [JsonProperty("e10s_ec_Scholar")] public EncounterRankings E10SEcScholar { get; set; }
            [JsonProperty("e11s_ne_Scholar")] public EncounterRankings E11SNeScholar { get; set; }
            [JsonProperty("e11s_ec_Scholar")] public EncounterRankings E11SEcScholar { get; set; }
            [JsonProperty("e12sd_ne_Scholar")] public EncounterRankings E12SdNeScholar { get; set; }
            [JsonProperty("e12sd_ec_Scholar")] public EncounterRankings E12SdEcScholar { get; set; }
            [JsonProperty("e12so_ne_Scholar")] public EncounterRankings E12SoNeScholar { get; set; }
            [JsonProperty("e12so_ec_Scholar")] public EncounterRankings E12SoEcScholar { get; set; }
            [JsonProperty("tea_Scholar")] public EncounterRankings TeaScholar { get; set; }
            [JsonProperty("ucob_Scholar")] public EncounterRankings UcobScholar { get; set; }
            [JsonProperty("uwu_Scholar")] public EncounterRankings UwuScholar { get; set; }
            [JsonProperty("e9s_ne_Summoner")] public EncounterRankings E9SNeSummoner { get; set; }
            [JsonProperty("e9s_ec_Summoner")] public EncounterRankings E9SEcSummoner { get; set; }
            [JsonProperty("e10s_ne_Summoner")] public EncounterRankings E10SNeSummoner { get; set; }
            [JsonProperty("e10s_ec_Summoner")] public EncounterRankings E10SEcSummoner { get; set; }
            [JsonProperty("e11s_ne_Summoner")] public EncounterRankings E11SNeSummoner { get; set; }
            [JsonProperty("e11s_ec_Summoner")] public EncounterRankings E11SEcSummoner { get; set; }
            [JsonProperty("e12sd_ne_Summoner")] public EncounterRankings E12SdNeSummoner { get; set; }
            [JsonProperty("e12sd_ec_Summoner")] public EncounterRankings E12SdEcSummoner { get; set; }
            [JsonProperty("e12so_ne_Summoner")] public EncounterRankings E12SoNeSummoner { get; set; }
            [JsonProperty("e12so_ec_Summoner")] public EncounterRankings E12SoEcSummoner { get; set; }
            [JsonProperty("tea_Summoner")] public EncounterRankings TeaSummoner { get; set; }
            [JsonProperty("ucob_Summoner")] public EncounterRankings UcobSummoner { get; set; }
            [JsonProperty("uwu_Summoner")] public EncounterRankings UwuSummoner { get; set; }
            [JsonProperty("e9s_ne_Warrior")] public EncounterRankings E9SNeWarrior { get; set; }
            [JsonProperty("e9s_ec_Warrior")] public EncounterRankings E9SEcWarrior { get; set; }
            [JsonProperty("e10s_ne_Warrior")] public EncounterRankings E10SNeWarrior { get; set; }
            [JsonProperty("e10s_ec_Warrior")] public EncounterRankings E10SEcWarrior { get; set; }
            [JsonProperty("e11s_ne_Warrior")] public EncounterRankings E11SNeWarrior { get; set; }
            [JsonProperty("e11s_ec_Warrior")] public EncounterRankings E11SEcWarrior { get; set; }
            [JsonProperty("e12sd_ne_Warrior")] public EncounterRankings E12SdNeWarrior { get; set; }
            [JsonProperty("e12sd_ec_Warrior")] public EncounterRankings E12SdEcWarrior { get; set; }
            [JsonProperty("e12so_ne_Warrior")] public EncounterRankings E12SoNeWarrior { get; set; }
            [JsonProperty("e12so_ec_Warrior")] public EncounterRankings E12SoEcWarrior { get; set; }
            [JsonProperty("tea_Warrior")] public EncounterRankings TeaWarrior { get; set; }
            [JsonProperty("ucob_Warrior")] public EncounterRankings UcobWarrior { get; set; }
            [JsonProperty("uwu_Warrior")] public EncounterRankings UwuWarrior { get; set; }
            [JsonProperty("e9s_ne_WhiteMage")] public EncounterRankings E9SNeWhiteMage { get; set; }
            [JsonProperty("e9s_ec_WhiteMage")] public EncounterRankings E9SEcWhiteMage { get; set; }
            [JsonProperty("e10s_ne_WhiteMage")] public EncounterRankings E10SNeWhiteMage { get; set; }
            [JsonProperty("e10s_ec_WhiteMage")] public EncounterRankings E10SEcWhiteMage { get; set; }
            [JsonProperty("e11s_ne_WhiteMage")] public EncounterRankings E11SNeWhiteMage { get; set; }
            [JsonProperty("e11s_ec_WhiteMage")] public EncounterRankings E11SEcWhiteMage { get; set; }
            [JsonProperty("e12sd_ne_WhiteMage")] public EncounterRankings E12SdNeWhiteMage { get; set; }
            [JsonProperty("e12sd_ec_WhiteMage")] public EncounterRankings E12SdEcWhiteMage { get; set; }
            [JsonProperty("e12so_ne_WhiteMage")] public EncounterRankings E12SoNeWhiteMage { get; set; }
            [JsonProperty("e12so_ec_WhiteMage")] public EncounterRankings E12SoEcWhiteMage { get; set; }
            [JsonProperty("tea_WhiteMage")] public EncounterRankings TeaWhiteMage { get; set; }
            [JsonProperty("ucob_WhiteMage")] public EncounterRankings UcobWhiteMage { get; set; }
            [JsonProperty("uwu_WhiteMage")] public EncounterRankings UwuWhiteMage { get; set; }
            [JsonProperty("e9s_ne_RedMage")] public EncounterRankings E9SNeRedMage { get; set; }
            [JsonProperty("e9s_ec_RedMage")] public EncounterRankings E9SEcRedMage { get; set; }
            [JsonProperty("e10s_ne_RedMage")] public EncounterRankings E10SNeRedMage { get; set; }
            [JsonProperty("e10s_ec_RedMage")] public EncounterRankings E10SEcRedMage { get; set; }
            [JsonProperty("e11s_ne_RedMage")] public EncounterRankings E11SNeRedMage { get; set; }
            [JsonProperty("e11s_ec_RedMage")] public EncounterRankings E11SEcRedMage { get; set; }
            [JsonProperty("e12sd_ne_RedMage")] public EncounterRankings E12SdNeRedMage { get; set; }
            [JsonProperty("e12sd_ec_RedMage")] public EncounterRankings E12SdEcRedMage { get; set; }
            [JsonProperty("e12so_ne_RedMage")] public EncounterRankings E12SoNeRedMage { get; set; }
            [JsonProperty("e12so_ec_RedMage")] public EncounterRankings E12SoEcRedMage { get; set; }
            [JsonProperty("tea_RedMage")] public EncounterRankings TeaRedMage { get; set; }
            [JsonProperty("ucob_RedMage")] public EncounterRankings UcobRedMage { get; set; }
            [JsonProperty("uwu_RedMage")] public EncounterRankings UwuRedMage { get; set; }
            [JsonProperty("e9s_ne_Samurai")] public EncounterRankings E9SNeSamurai { get; set; }
            [JsonProperty("e9s_ec_Samurai")] public EncounterRankings E9SEcSamurai { get; set; }
            [JsonProperty("e10s_ne_Samurai")] public EncounterRankings E10SNeSamurai { get; set; }
            [JsonProperty("e10s_ec_Samurai")] public EncounterRankings E10SEcSamurai { get; set; }
            [JsonProperty("e11s_ne_Samurai")] public EncounterRankings E11SNeSamurai { get; set; }
            [JsonProperty("e11s_ec_Samurai")] public EncounterRankings E11SEcSamurai { get; set; }
            [JsonProperty("e12sd_ne_Samurai")] public EncounterRankings E12SdNeSamurai { get; set; }
            [JsonProperty("e12sd_ec_Samurai")] public EncounterRankings E12SdEcSamurai { get; set; }
            [JsonProperty("e12so_ne_Samurai")] public EncounterRankings E12SoNeSamurai { get; set; }
            [JsonProperty("e12so_ec_Samurai")] public EncounterRankings E12SoEcSamurai { get; set; }
            [JsonProperty("tea_Samurai")] public EncounterRankings TeaSamurai { get; set; }
            [JsonProperty("ucob_Samurai")] public EncounterRankings UcobSamurai { get; set; }
            [JsonProperty("uwu_Samurai")] public EncounterRankings UwuSamurai { get; set; }
            [JsonProperty("e9s_ne_Dancer")] public EncounterRankings E9SNeDancer { get; set; }
            [JsonProperty("e9s_ec_Dancer")] public EncounterRankings E9SEcDancer { get; set; }
            [JsonProperty("e10s_ne_Dancer")] public EncounterRankings E10SNeDancer { get; set; }
            [JsonProperty("e10s_ec_Dancer")] public EncounterRankings E10SEcDancer { get; set; }
            [JsonProperty("e11s_ne_Dancer")] public EncounterRankings E11SNeDancer { get; set; }
            [JsonProperty("e11s_ec_Dancer")] public EncounterRankings E11SEcDancer { get; set; }
            [JsonProperty("e12sd_ne_Dancer")] public EncounterRankings E12SdNeDancer { get; set; }
            [JsonProperty("e12sd_ec_Dancer")] public EncounterRankings E12SdEcDancer { get; set; }
            [JsonProperty("e12so_ne_Dancer")] public EncounterRankings E12SoNeDancer { get; set; }
            [JsonProperty("e12so_ec_Dancer")] public EncounterRankings E12SoEcDancer { get; set; }
            [JsonProperty("tea_Dancer")] public EncounterRankings TeaDancer { get; set; }
            [JsonProperty("ucob_Dancer")] public EncounterRankings UcobDancer { get; set; }
            [JsonProperty("uwu_Dancer")] public EncounterRankings UwuDancer { get; set; }
            [JsonProperty("e9s_ne_Gunbreaker")] public EncounterRankings E9SNeGunbreaker { get; set; }
            [JsonProperty("e9s_ec_Gunbreaker")] public EncounterRankings E9SEcGunbreaker { get; set; }
            [JsonProperty("e10s_ne_Gunbreaker")] public EncounterRankings E10SNeGunbreaker { get; set; }
            [JsonProperty("e10s_ec_Gunbreaker")] public EncounterRankings E10SEcGunbreaker { get; set; }
            [JsonProperty("e11s_ne_Gunbreaker")] public EncounterRankings E11SNeGunbreaker { get; set; }
            [JsonProperty("e11s_ec_Gunbreaker")] public EncounterRankings E11SEcGunbreaker { get; set; }
            [JsonProperty("e12sd_ne_Gunbreaker")] public EncounterRankings E12SdNeGunbreaker { get; set; }
            [JsonProperty("e12sd_ec_Gunbreaker")] public EncounterRankings E12SdEcGunbreaker { get; set; }
            [JsonProperty("e12so_ne_Gunbreaker")] public EncounterRankings E12SoNeGunbreaker { get; set; }
            [JsonProperty("e12so_ec_Gunbreaker")] public EncounterRankings E12SoEcGunbreaker { get; set; }
            [JsonProperty("tea_Gunbreaker")] public EncounterRankings TeaGunbreaker { get; set; }
            [JsonProperty("ucob_Gunbreaker")] public EncounterRankings UcobGunbreaker { get; set; }
            [JsonProperty("uwu_Gunbreaker")] public EncounterRankings UwuGunbreaker { get; set; }
        }

        private class ZoneRankings
        {
            [JsonProperty("allStars")]
            public AllStar[] AllStars { get; set; }
        }

        private class AllStar
        {
            [JsonProperty("spec", NullValueHandling = NullValueHandling.Ignore)]
            public GameJob Spec { get; set; }

            [JsonProperty("points")]
            public float Points { get; set; }

            [JsonProperty("rank")]
            public int Rank { get; set; }

            [JsonProperty("rankPercent")]
            public float RankPercent { get; set; }

            [JsonProperty("total")]
            public int Total { get; set; }
        }

        private class EncounterRankings
        {
            [JsonProperty("bestAmount")]
            public float BestAmount { get; set; } // rdps

            [JsonProperty("medianPerformance")]
            public float? MedianPerformance { get; set; }

            [JsonProperty("totalKills")]
            public int TotalKills { get; set; }

            [JsonProperty("ranks")]
            public Rank[] Ranks { get; set; }
        }

        private class Rank
        {
            [JsonProperty("rankPercent")]
            public float RankPercent { get; set; }

            [JsonProperty("amount")]
            public float Amount { get; set; }
        }

        private static readonly JsonSerializer jsonSerializer = new()
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            ContractResolver = new DictionaryAsArrayResolver(),
            Converters =
            {
                new SpecConverter(),
                //new EncounterDataKeyConverter(),
            },
        };
        private static readonly JsonSerializerSettings jsonSerializerSettings = new()
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            ContractResolver = new DictionaryAsArrayResolver(),
            Converters =
            {
                new SpecConverter(),
                //new EncounterDataKeyConverter(),
            },
        };

        private class SpecConverter : JsonConverter
        {
            public override bool CanRead => true;
            public override bool CanWrite => true;

            public override bool CanConvert(Type t) => t == typeof(GameJob) || t == typeof(GameJob?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;

                var value = serializer.Deserialize<string>(reader);
                return GameData.ParseGameJob(value);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(((GameJob)value).ToString());
            }
        }

        private class EncounterDataKeyConverter : JsonConverter
        {
            public override bool CanRead => true;
            public override bool CanWrite => true;

            public override bool CanConvert(Type t) => t == typeof(EncounterDataKey);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;

                var value = serializer.Deserialize<string>(reader);

                var sp = value.IndexOf(',');

                return new EncounterDataKey
                {
                    EncounterId = (GameEncounter)int.Parse(value[..sp]),
                    JobId       = (GameJob      )int.Parse(value[(sp + 1)..]),
                };
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var v = (EncounterDataKey)value;

                writer.WriteValue($"{(int)v.EncounterId},{(int)v.JobId}");

            }
        }

        private class DictionaryAsArrayResolver : DefaultContractResolver
        {
            protected override JsonContract CreateContract(Type objectType)
            {
                if (objectType.GetInterfaces().Any(i => i == typeof(IDictionary) ||
                    (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>))))
                {
                    return base.CreateArrayContract(objectType);
                }

                return base.CreateContract(objectType);
            }
        }
    }
}
