using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FFLogsLookup.FFlogs;
using FFLogsLookup.Game;
using ImGuiNET;

namespace FFLogsLookup.Gui
{
    internal class PartyGui : BaseGui
    {
        private readonly DrawingData drawingData;

        public PartyGui(Plugin plugin)
            : base(plugin, "FFLogsLookup: 캐릭터 정보 조회")
        {
            this.drawingData = new(this);

            this.Size = Vector2.Zero;
            this.Flags = ImGuiWindowFlags.AlwaysAutoResize;
            this.SizeCondition = ImGuiCond.Always;
        }

        public override void Draw()
        {

        }

        private class DrawingData
        {
            private static readonly (string name, float width)[] tableColumStr =
            {
                ("Rank"  , -1),
                ("AVG %" , -1),
                ("rDPS"  , -1),
                ("BEST %", -1),
            };
            private static readonly (string name, float width)[] tableRow0Str =
            {
                ("캐릭명"   , -1),
                ("서버"     , -1),
                ("에덴 영식", -1),
                ("절 토벌전", -1),
            };
            private static readonly (string name, float width, GameEncounter enc)[] raidEncounters =
            {
                ("1층"     , -1, GameEncounter.E9s       ),
                ("2층"     , -1, GameEncounter.E10s      ),
                ("3층"     , -1, GameEncounter.E11s      ),
                ("4층 전반", -1, GameEncounter.E12sDoor  ),
                ("4층 후반", -1, GameEncounter.E12sOracle),
            };
            private static readonly (string name, float width, GameEncounter enc)[] ultiEncounters =
            {
                ("알렉산더", -1, GameEncounter.Tea ),
                ("알테마"  , -1, GameEncounter.Ucob),
                ("바하무트", -1, GameEncounter.Uwu ),
            };

            private readonly PartyGui partyGui;
            private readonly MemberData[] memberDatas = new MemberData[8];

            private readonly float columnDescWidth;

            public float Width { get; set; }

            public DrawingData(PartyGui partyGui)
            {
                this.partyGui = partyGui;

                for (int idx = 0; idx < 8; idx++)
                {
                    this.memberDatas[idx] = new MemberData();
                    this.Update(idx, null);
                }

                for (int idx = 0; idx < tableColumStr.Length; idx++)
                {
                    tableColumStr[idx].width = ImGui.CalcTextSize(tableColumStr[idx].name).X;
                }

                for (int idx = 0; idx < tableRow0Str.Length; idx++)
                {
                    tableRow0Str[idx].width = ImGui.CalcTextSize(tableRow0Str[idx].name).X;
                }
                for (int idx = 0; idx < raidEncounters.Length; idx++)
                {
                    raidEncounters[idx].width = ImGui.CalcTextSize(raidEncounters[idx].name).X;
                }
                for (int idx = 0; idx < ultiEncounters.Length; idx++)
                {
                    ultiEncounters[idx].width = ImGui.CalcTextSize(ultiEncounters[idx].name).X;
                }

                this.columnDescWidth = Math.Max(
                    tableRow0Str.Max(e => e.width),
                    Math.Max(
                        raidEncounters.Max(e => e.width),
                        ultiEncounters.Max(e => e.width)
                    )
                );
            }

            public void Update(int idx, FFlogsLog log)
            {
                this.memberDatas[idx].Update(log);

                this.Width = this.columnDescWidth + this.memberDatas.Sum(e => e.Width);
            }
            private static void TextUnformattedCentered(string str, float width, float strWidth)
            {
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (width - strWidth) / 2);
                ImGui.TextUnformatted(str);
            }
            public void Draw()
            {
                var partyCount = this.memberDatas.Count(e => e.Exists);

                ////////////////////////////////////////////////////////////////////////////////

                ImGui.Columns(1 + partyCount);

                ImGui.SetColumnWidth(0, this.columnDescWidth);
                TextUnformattedCentered(tableRow0Str[0].name, this.columnDescWidth, tableRow0Str[0].width);
                TextUnformattedCentered(tableRow0Str[1].name, this.columnDescWidth, tableRow0Str[1].width);

                for (int idx = 0; idx < 8; idx++)
                {
                    var md = this.memberDatas[idx];
                    if (!md.Exists) continue;

                    ImGui.SetColumnWidth(1 + idx, md.Width);

                    ImGui.NextColumn();
                    TextUnformattedCentered(md.CharName, md.Width, md.CharNameWidth);
                    TextUnformattedCentered(md.CharServerStr, md.Width, md.CharServerWidth);
                }
                ImGui.Columns();

                ////////////////////////////////////////////////////////////////////////////////

                ImGui.Columns(1 + partyCount * 2);
                ImGui.SetColumnWidth(0, this.columnDescWidth);

                // 레이드
                TextUnformattedCentered(tableRow0Str[2].name, this.columnDescWidth, tableRow0Str[2].width);
                foreach (var (name, width, _) in raidEncounters)
                {
                    TextUnformattedCentered(name, this.columnDescWidth, width);
                }

                // 절 토벌전
                TextUnformattedCentered(tableRow0Str[3].name, this.columnDescWidth, tableRow0Str[3].width);
                foreach (var (name, width, _) in ultiEncounters)
                {
                    TextUnformattedCentered(name, this.columnDescWidth, width);
                }

                for (int idx = 0; idx < 8; idx++)
                {
                    var md = this.memberDatas[idx];
                    if (!this.memberDatas[idx].Exists) continue;

                    ImGui.SetColumnWidth(1 + idx * 2, md.WidthColumn0);
                    ImGui.SetColumnWidth(1 + idx * 2, md.WidthColumn1);
                    ImGui.NextColumn();

                    ///////////////////////////////////////////////////////////////////////////////////////////////
                    // Column[0] = Rank

                    // 올스타
                    TextUnformattedCentered(tableColumStr[0].name, md.WidthColumn0, tableColumStr[0].width); // RANK
                    TextUnformattedCentered(md.AllStar.Rank, this.columnDescWidth, md.AllStar.RankWidth);

                    // 레이드
                    TextUnformattedCentered(tableColumStr[2].name, md.WidthColumn0, tableColumStr[2].width); // rDPS
                    foreach (var (_, _, enc) in raidEncounters)
                    {
                        var ed = md.Encounter[enc];

                        if (ed.Color.HasValue) ImGui.PushStyleColor(ImGuiCol.Text, ed.Color.Value);
                        TextUnformattedCentered(ed.Rdps, md.WidthColumn0, ed.RdpsWidth);
                        if (ed.Color.HasValue) ImGui.PopStyleColor();
                    }

                    // 절토벌전
                    TextUnformattedCentered(tableColumStr[2].name, md.WidthColumn0, tableColumStr[2].width); // rDPS
                    foreach (var (_, _, enc) in raidEncounters)
                    {
                        var ed = md.Encounter[enc];

                        if (ed.Color.HasValue) ImGui.PushStyleColor(ImGuiCol.Text, ed.Color.Value);
                        TextUnformattedCentered(ed.Rdps, md.WidthColumn0, ed.RdpsWidth);
                        if (ed.Color.HasValue) ImGui.PopStyleColor();
                    }

                    ///////////////////////////////////////////////////////////////////////////////////////////////
                    // Column[1] = rDPS

                    ImGui.NextColumn();

                    TextUnformattedCentered(tableColumStr[1].name, md.WidthColumn0, tableColumStr[1].width); // AVG %
                    TextUnformattedCentered(md.AllStar.BestAvg, md.WidthColumn0, md.AllStar.BestAvgWidth);

                    // 레이드
                    TextUnformattedCentered(tableColumStr[3].name, md.WidthColumn0, tableColumStr[3].width); // BEST %
                    foreach (var (_, _, enc) in raidEncounters)
                    {
                        var ed = md.Encounter[enc];

                        if (ed.Color.HasValue) ImGui.PushStyleColor(ImGuiCol.Text, ed.Color.Value);
                        TextUnformattedCentered(ed.Rdps, md.WidthColumn1, ed.RdpsWidth);
                        if (ed.Color.HasValue) ImGui.PopStyleColor();
                    }

                    // 절토벌전
                    TextUnformattedCentered(tableColumStr[3].name, md.WidthColumn0, tableColumStr[3].width); // BEST %
                    foreach (var (_, _, enc) in raidEncounters)
                    {
                        var ed = md.Encounter[enc];

                        if (ed.Color.HasValue) ImGui.PushStyleColor(ImGuiCol.Text, ed.Color.Value);
                        TextUnformattedCentered(ed.Rdps, md.WidthColumn1, ed.RdpsWidth);
                        if (ed.Color.HasValue) ImGui.PopStyleColor();
                    }
                }
                ImGui.Columns();

                static void WriteData(MemberData.EncounterData encounterData, float width)
                {
                }
            }

            public class MemberData
            {
                private static readonly GameEncounter[] encounterList =
                {
                    GameEncounter.E9s,
                    GameEncounter.E10s,
                    GameEncounter.E11s,
                    GameEncounter.E12sDoor,
                    GameEncounter.E12sOracle,
                    GameEncounter.Tea,
                    GameEncounter.Ucob,
                    GameEncounter.Uwu,
                };

                public bool Exists { get; set; }

                public string CharName      { get; set; }
                public float  CharNameWidth { get; set; }

                public GameServer CharServer      { get; set; }
                public string     CharServerStr   { get; set; }
                public float      CharServerWidth { get; set; }

                public float Width { get; set; }
                public float WidthColumn0 { get; set; }
                public float WidthColumn1 { get; set; }

                public struct AllstarData
                {
                    public GameJob? Job   { get; set; }
                    public Vector4? Color { get; set; }
 
                    public string BestAvg        { get; set; }
                    public float  BestAvgWidth   { get; set; }
                    public string BestAvgTooltip { get; set; }

                    public string Rank        { get; set; }
                    public float  RankWidth   { get; set; }
                    public string RankTooltip { get; set; }
                }
                public struct EncounterData
                {
                    public GameJob? Job   { get; set; }
                    public Vector4? Color { get; set; }

                    public string Rdps        { get; set; }
                    public float  RdpsWidth   { get; set; }
                    public string RdpsTooltip { get; set; }

                    public string MaxPer        { get; set; }
                    public float  MaxPerWidth   { get; set; }
                    public string MaxPerTooltip { get; set; }
                }
                public AllstarData AllStar { get; set; }
                public Dictionary<GameEncounter, EncounterData> Encounter { get; } = new();

                public void Update(FFlogsLog log)
                {
                    log ??= FFlogsLog.Empty;

                    this.Exists = log != null;

                    this.CharName = log.CharName;

                    this.CharServer    = log.CharServer;
                    this.CharServerStr = log.CharServer.S();

                    ////////////////////////////////////////////////////////////////////////////////////////////////////

                    // 전체 클리어 횟수 미리 계산하기
                    var raidKills =
                        log.EncountersNe
                        .GroupBy(e => e.Key.EncounterId)
                        .ToDictionary(
                            ee => ee.Key,
                            ee => ee.Sum(eee => eee.Value.Kills)
                        );
                    raidKills[0] = raidKills.Sum(e => e.Value);

                    var allstarJobBest = log.RaidAllstarNe.OrderByDescending(e => e.Value.Point).First().Key;
                    var allstarData = log.RaidAllstarNe[allstarJobBest];

                    if (allstarData.Point == 0)
                    {
                        this.AllStar = new AllstarData()
                        {
                            Job     = null,
                            Color   = null,
                            BestAvg = "-",
                            Rank    = "-",
                        };
                    }
                    else
                    {
                        var rankPercent = (float)(allstarData.Total - allstarData.Rank) / allstarData.Total * 100;
                        var color = FFlogsColor.GetColor(rankPercent);
                        rankPercent = (float)(Math.Floor(rankPercent * 10) / 10);

                        var killsTotal = log.EncountersNe
                            .Where(e => e.Key.EncounterId.IsRaids())
                            .Sum(e => e.Value.Kills);

                        var bestAvg = log.EncountersNe
                            .Where(e => e.Key.EncounterId.IsRaids())
                            .GroupBy(e => e.Key.EncounterId)
                            .Average(e => e.Max(ee => ee.Value.MaxPer));

                        this.AllStar = new AllstarData
                        {
                            Job   = allstarJobBest,
                            Color = color,
                                
                            BestAvg        = $"{bestAvg:#0.0}",
                            BestAvgTooltip = $"{allstarJobBest.S()} : {killsTotal:#,##0} 킬\n전체 : {raidKills[0]:#,##0} Kills",

                            Rank        = $"{allstarData.Rank:#,##0}",
                            RankTooltip = $"상위 {rankPercent:##0.0} %%\n전체 {allstarData.Total:#,##0} 명",
                        };
                    }

                    foreach (var enc in encounterList)
                    {
                        UpdateEncounter(enc);
                    }

                    ////////////////////////////////////////////////////////////////////////////////////////////////////

                    var allStar = this.AllStar;
                    allStar.BestAvgWidth = ImGui.CalcTextSize(allStar.BestAvg).X;
                    allStar.RankWidth    = ImGui.CalcTextSize(allStar.Rank).X;
                    this.AllStar = allStar;

                    this.CharNameWidth = ImGui.CalcTextSize(this.CharName).X;
                    this.CharServerWidth = ImGui.CalcTextSize(this.CharServerStr).X;

                    foreach (var enc in encounterList)
                    {
                        var d = this.Encounter[enc];
                        d.RdpsWidth   = ImGui.CalcTextSize(d.Rdps).X;
                        d.MaxPerWidth = ImGui.CalcTextSize(d.MaxPer).X;

                        this.Encounter[enc] = d;
                    }

                    this.WidthColumn0 = this.Encounter.Max(e => e.Value.RdpsWidth);
                    this.WidthColumn1 = this.Encounter.Max(e => e.Value.MaxPerWidth);

                    this.Width = Math.Max(
                        Math.Max(
                            this.CharNameWidth,
                            this.CharServerWidth
                        ),
                        this.WidthColumn0 + this.WidthColumn1
                    );

                    ////////////////////////////////////////////////////////////////////////////////////////////////////

                    void UpdateEncounter(GameEncounter enc)
                    {
                        GameJob? bestPerfJob =
                            log.EncountersNe
                            .Where(e => e.Key.EncounterId == enc)
                            .OrderByDescending(e => e.Value.MaxPer)
                            .Select(e => e.Key.JobId)
                            .FirstOrDefault();

                        if (!bestPerfJob.HasValue)
                        {
                            this.Encounter[enc] = new EncounterData
                            {
                                Job = null,
                                Color = null,
                            };
                        }
                        else
                        {
                            var encData = log.EncountersNe[new EncounterDataKey
                            {
                                EncounterId = enc,
                                JobId = bestPerfJob.Value,
                            }];

                            this.Encounter[enc] = new EncounterData
                            {
                                Job = bestPerfJob,
                                Color = FFlogsColor.GetColor(encData.MaxPer),

                                Rdps = $"{encData.MaxRdps:#,##0.0}",
                                RdpsTooltip = $"{bestPerfJob?.S()} : {encData.Kills:#,##0} 킬\n전체 : {raidKills[enc]:#,##0} Kills",

                                MaxPer = $"{(Math.Floor(encData.MaxPer * 10) / 10):#0.0}",
                                MaxPerTooltip = $"MedPer : {(Math.Floor(encData.MaxPer * 10) / 10):#0.0}",
                            };
                        }
                    }
                }
            }
        }
    }
}
