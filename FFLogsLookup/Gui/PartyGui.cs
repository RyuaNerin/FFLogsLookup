using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Interface;
using Dalamud.Logging;
using FFLogsLookup.FFlogs;
using FFLogsLookup.Game;
using FFLogsLookup.Gui.Components;
using FFLogsLookup.Utils;
using FFXIVClientStructs.FFXIV.Client.UI;
using ImGuiNET;

namespace FFLogsLookup.Gui
{
    internal class PartyGui : BaseGui
    {
        private readonly DrawingData drawingData;

        public PartyGui(Plugin plugin)
            : base(plugin, "FFLogsLookup: 파티 정보 조회")
        {
            this.drawingData = new DrawingData(this);

            this.Size = Vector2.Zero;
            this.Flags = ImGuiWindowFlags.AlwaysAutoResize;
            this.SizeCondition = ImGuiCond.Always;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.OnClose();
            }
        }

        private CancellationTokenSource ctsRefresh;
        public override void OnOpen()
        {
            this.ctsRefresh?.Dispose();
            this.ctsRefresh = new CancellationTokenSource();
            Task.Factory.StartNew(() => this.Refresh(this.ctsRefresh.Token), this.ctsRefresh.Token);
        }

        public override void OnClose()
        {
            try
            {
                this.ctsRefresh?.Cancel();
                this.ctsRefresh?.Dispose();
            }
            catch
            {
            }
        }

        private async void Refresh(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    this.drawingData.Refresh();

                    await Task.Delay(TimeSpan.FromSeconds(1), token);
                }
            }
            catch
            {
            }
        }

        public override void Draw()
        {
            this.Size = new Vector2(this.drawingData.Draw(), -1);
        }

        private class DrawingData
        {
            private static bool inited;

            private static readonly CellData StrRank = new("RANK");
            private static readonly CellData StrAvg = new("AVG %");
            private static readonly CellData StrJob = new("JOB");
            private static readonly CellData StrRdps = new("rDPS");
            private static readonly CellData StrBestPer = new("BEST %");
            private static readonly CellData StrDash = new("-");
            private static readonly CellData StrNone = new("");

            private static readonly CellData StrCharName = new("캐릭명");
            private static readonly CellData StrServer = new("서버");
            private static readonly CellData StrRaid = new("에덴 영식");
            private static readonly CellData StrUlti = new("절 토벌전");
            private static readonly CellData StrAllstar = new("올스타");

            private static readonly CellData StrErrorIcon = new()
            {
                Value   = FontAwesomeExtensions.ToIconString(FontAwesomeIcon.ExclamationCircle),
                FontPtr = UiBuilder.IconFont,
            };

            private static readonly (CellData str, GameEncounter enc)[] raidEncounters =
            {
                (new("1층"     ), GameEncounter.E9s       ),
                (new("2층"     ), GameEncounter.E10s      ),
                (new("3층"     ), GameEncounter.E11s      ),
                (new("4층 전반"), GameEncounter.E12sDoor  ),
                (new("4층 후반"), GameEncounter.E12sOracle),
            };
            private static readonly (CellData str, GameEncounter enc)[] ultiEncounters =
            {
                (new("알렉산더"), GameEncounter.Tea ),
                (new("알테마"  ), GameEncounter.Ucob),
                (new("바하무트"), GameEncounter.Uwu ),
            };

            protected readonly PartyGui partyGui;

            private readonly List<MemberData> memberData = new();
            private float columnFirstWidth;

            public DrawingData(PartyGui partyGui)
            {
                this.partyGui = partyGui;
            }

            public unsafe void Refresh()
            {
                var partyList = (AddonPartyList*)DalamudInstance.GameGui.GetAddonByName("_PartyList", 1);
                if (partyList == null) return;

                var idx = 0;

                var memberList = new (string name, GameServer server)[8];

                for (idx = 0; idx < 8; idx++)
                {
                    var member = partyList->PartyMember[idx];
                    member.Name->
                }

                var newList = new int[8];
                var newListIdx = 0;

                lock (this.memberData)
                {
                    foreach (var member in memberList)
                    {
                        var cn = member.Name.TextValue;
                        var cs = GameData.GetGameServer(member.World.GameData.Name);
                        var key = $"{cn}@{cs}".GetHashCode(StringComparison.CurrentCultureIgnoreCase);

                        newList[newListIdx++] = key;
                        if (!this.memberData.Any(e => e.Key == key))
                        {
                            this.memberData.Add(new MemberData(this, key, cn, cs));
                        }
                    }

                    idx = 0;
                    while (idx < this.memberData.Count)
                    {
                        if (!newList.Any(e => e == this.memberData[idx].Key))
                        {
                            this.memberData[idx].Dispose();
                            this.memberData.RemoveAt(idx);
                        }
                        else
                        {
                            idx++;
                        }
                    }

                    this.memberData.Sort((a, b) => Array.IndexOf(newList, a).CompareTo(Array.IndexOf(newList, b)));
                }
            }

            private readonly Spinner spinner = new()
            {
                Radius = 7,
                Thickness = 3,
            };
            public float Draw()
            {
                int idx;

                var style = ImGui.GetStyle();
                var padding = style.ItemSpacing.X * 2;

                if (!inited)
                {
                    inited = true;

                    this.columnFirstWidth = Mathx.Max(
                        StrCharName.ValueWidth,
                        StrServer.ValueWidth,
                        StrRaid.ValueWidth,
                        StrUlti.ValueWidth,
                        StrAllstar.ValueWidth,
                        raidEncounters.Max(e => e.str.ValueWidth),
                        ultiEncounters.Max(e => e.str.ValueWidth)
                    ) + padding;
                }

                lock (this.memberData)
                {
                    ImGui.Columns(1 + this.memberData.Count, "##grid-charname", false);

                    if (this.memberData.Count > 0)
                    {
                        ImGui.SetColumnWidth(0, this.columnFirstWidth);
                    }
                    StrCharName.Draw(this.columnFirstWidth, padding, CellData.Align.Center);
                    StrServer.Draw(this.columnFirstWidth, padding, CellData.Align.Center);

                    for (idx = 0; idx < this.memberData.Count; idx++)
                    {
                        var md = this.memberData[idx];
                        lock (md.Lock)
                        {
                            ImGui.SetColumnWidth(1 + idx, md.Width);
                        }
                    }

                    for (idx = 0; idx < this.memberData.Count; idx++)
                    {
                        ImGui.NextColumn();

                        var md = this.memberData[idx];
                        lock (md.Lock)
                        {
                            md.CharName.Draw(md.Width, padding, CellData.Align.Center);
                            md.CharServer.Draw(md.Width, padding, CellData.Align.Center);
                        }
                    }
                    ImGui.Columns();

                    ImGui.Separator();

                    ////////////////////////////////////////////////////////////////////////////////

                    ImGui.Columns(1 + this.memberData.Count * 3, "##grid-chardata", true);
                    if (this.memberData.Count > 0)
                    {
                        ImGui.SetColumnWidth(0, this.columnFirstWidth);
                    }

                    // 레이드
                    StrAllstar.Draw(this.columnFirstWidth, padding, CellData.Align.Center);
                    ImGui.Spacing();
                    StrNone.Draw(this.columnFirstWidth, padding, CellData.Align.Center);
                    ImGui.Spacing();

                    StrRaid.Draw(this.columnFirstWidth, padding, CellData.Align.Center);
                    ImGui.Spacing();
                    foreach (var (str, _) in raidEncounters)
                    {
                        str.Draw(this.columnFirstWidth, padding, CellData.Align.Center);
                    }
                    ImGui.Spacing();

                    // 절 토벌전
                    StrUlti.Draw(this.columnFirstWidth, padding, CellData.Align.Center);
                    ImGui.Spacing();
                    foreach (var (str, _) in ultiEncounters)
                    {
                        str.Draw(this.columnFirstWidth, padding, CellData.Align.Center);
                    }

                    float totalWidth = this.columnFirstWidth;
                    for (idx = 0; idx < this.memberData.Count; idx++)
                    {
                        var md = this.memberData[idx];
                        lock (md.Lock)
                        {
                            totalWidth += md.Width;
                            ImGui.SetColumnWidth(1 + idx * 3, md.WidthJob);
                            ImGui.SetColumnWidth(2 + idx * 3, md.WidthRdps);
                            ImGui.SetColumnWidth(3 + idx * 3, md.WidthBestPer);
                        }
                    }

                    for (idx = 0; idx < this.memberData.Count; idx++)
                    {
                        var isLast = idx == this.memberData.Count - 1;

                        var md = this.memberData[idx];
                        lock (md.Lock)
                        {

                            ImGui.NextColumn();

                            if (md.Status == MemberData.Statuses.Loading)
                            {
                                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (md.WidthRdps - this.spinner.Radius * 2) / 2);
                                this.spinner.Draw();

                                continue;
                            }
                            else if (md.Status == MemberData.Statuses.Error)
                            {
                                StrErrorIcon.Draw(md.Width, padding, CellData.Align.Center);

                                continue;
                            }

                            /////////////////////////////////////////////////////////////////////////////////////////////// Job

                            // 올스타
                            StrJob.Draw(md.WidthRdps, padding, CellData.Align.Center);
                            ImGui.Spacing();
                            md.AllStar.Job.Draw(md.WidthRdps, padding, CellData.Align.Center);
                            ImGui.Spacing();

                            // 레이드
                            StrJob.Draw(md.WidthRdps, StrRdps.ValueWidth, CellData.Align.Center);
                            ImGui.Spacing();
                            foreach (var (_, enc) in raidEncounters)
                            {
                                md.Encounter[enc].Job.Draw(md.WidthRdps, padding, CellData.Align.Center);
                            }
                            ImGui.Spacing();

                            // 절토벌전
                            StrJob.Draw(md.WidthRdps, StrRdps.ValueWidth, CellData.Align.Center);
                            ImGui.Spacing();
                            foreach (var (_, enc) in ultiEncounters)
                            {
                                md.Encounter[enc].Job.Draw(md.WidthRdps, padding, CellData.Align.Center);
                            }

                            /////////////////////////////////////////////////////////////////////////////////////////////// Rank , rDPS

                            // 올스타
                            StrRank.Draw(md.WidthRdps, padding, CellData.Align.Center); // RANK
                            ImGui.Spacing();
                            md.AllStar.Job.Draw(md.WidthRdps, padding, CellData.Align.Center);
                            ImGui.Spacing();

                            // 레이드
                            StrRdps.Draw(md.WidthRdps, StrRdps.ValueWidth, CellData.Align.Center); // rDPS
                            ImGui.Spacing();
                            foreach (var (_, enc) in raidEncounters)
                            {
                                md.Encounter[enc].Rdps.Draw(md.WidthRdps, padding, CellData.Align.Right);
                            }
                            ImGui.Spacing();

                            // 절토벌전
                            StrRdps.Draw(md.WidthRdps, StrRdps.ValueWidth, CellData.Align.Center); // rDPS
                            ImGui.Spacing();
                            foreach (var (_, enc) in ultiEncounters)
                            {
                                md.Encounter[enc].Rdps.Draw(md.WidthRdps, padding, CellData.Align.Right);
                            }

                            /////////////////////////////////////////////////////////////////////////////////////////////// AVG % / BEST %

                            ImGui.NextColumn();

                            StrAvg.Draw(md.WidthBestPer, StrAvg.ValueWidth, CellData.Align.Center); // AVG %
                            if (isLast) ImGui.Separator(); else ImGui.Spacing();
                            md.AllStar.BestAvg.Draw(md.WidthBestPer, padding, CellData.Align.Right);
                            if (isLast) ImGui.Separator(); else ImGui.Spacing();

                            // 레이드
                            StrBestPer.Draw(md.WidthBestPer, padding, CellData.Align.Center); // BEST %
                            if (isLast) ImGui.Separator(); else ImGui.Spacing();
                            foreach (var (_, enc) in raidEncounters)
                            {
                                md.Encounter[enc].BestPer.Draw(md.WidthRdps, padding, CellData.Align.Right);
                            }
                            if (isLast) ImGui.Separator(); else ImGui.Spacing();

                            // 절토벌전
                            StrBestPer.Draw(md.WidthBestPer, padding, CellData.Align.Center); // BEST %
                            if (isLast) ImGui.Separator(); else ImGui.Spacing();
                            foreach (var (_, enc) in ultiEncounters)
                            {
                                md.Encounter[enc].BestPer.Draw(md.WidthRdps, padding, CellData.Align.Right);
                            }
                        }
                    }

                    ImGui.Columns();

                    return totalWidth;
                }
            }

            public class MemberData : IDisposable
            {
                public const float MinWidth = 20;

                public enum Statuses
                {
                    Loading,
                    Ok,
                    Error,
                }

                public object Lock { get; } = new();

                public Statuses Status { get; set; }

                public int Key { get; }

                public CellData CharName   { get; }
                public CellData CharServer { get; }

                public struct AllstarData
                {
                    public CellData Job     { get; set; } 
                    public CellData BestAvg { get; set; }
                    public CellData Rank    { get; set; }
                }
                public struct EncounterData
                {
                    public CellData Job     { get; set; }
                    public CellData Rdps    { get; set; }
                    public CellData BestPer { get; set; }
                }
                public AllstarData AllStar { get; set; }
                public Dictionary<GameEncounter, EncounterData> Encounter { get; } = new();

                private readonly CancellationTokenSource cancellationTokenSource = new();

                private readonly DrawingData drawingData;

                public MemberData(DrawingData drawingData, int key, string charName, GameServer gameServer)
                {
                    this.drawingData = drawingData;

                    this.Key = key;
                    this.CharName = new(charName);
                    this.CharServer = new(gameServer.S());

                    var token = this.cancellationTokenSource.Token;
                    _ = Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            var (log, cached) = await this.drawingData.partyGui.plugin.FFlogsClient.GetLogs(charName,gameServer, false, token);
                            token.ThrowIfCancellationRequested();

                            var forced = false;
                            try
                            {
                                lock (this.Lock)
                                {
                                    this.Status = Statuses.Ok;
                                    this.Update(log);
                                }
                            }
                            catch (TaskCanceledException)
                            {
                                throw;
                            }
                            catch (Exception ex)
                            {
                                PluginLog.Error(ex, $"{nameof(DetailGui)}.{nameof(this.Update)}");
                                forced = true;
                            }

                            // 1시간 이전에 갱신된 거면 다시 불러오기
                            if (forced || (cached && log.UpdatedAtUtc < DateTime.UtcNow.AddHours(-1)))
                            {
                                (log, _) = await this.drawingData.partyGui.plugin.FFlogsClient.GetLogs(charName, gameServer, true, token);
                                token.ThrowIfCancellationRequested();

                                lock (this.Lock)
                                {
                                    this.Status = Statuses.Ok;
                                    this.Update(log);
                                }
                            }
                        }
                        catch (TaskCanceledException)
                        {
                        }
                        catch (Exception ex)
                        {
                            PluginLog.Error(ex, $"{nameof(DetailGui)}.{nameof(this.Update)}");
                            lock (this.Lock)
                            {
                                this.Status = Statuses.Error;
                            }
                        }
                    });
                }
                
                ~MemberData()
                {
                    this.Dispose(false);
                }

                public void Dispose()
                {
                    this.Dispose(true);
                    GC.SuppressFinalize(this);
                }
                private bool disposed;
                protected void Dispose(bool disposing)
                {
                    if (this.disposed) return;
                    this.disposed = true;

                    if (disposing)
                    {
                        this.cancellationTokenSource.Cancel();
                        this.cancellationTokenSource.Dispose();
                    }
                }

                private float? width;
                public float Width
                {
                    get
                    {
                        this.CalcWidth();
                        return this.width.Value;
                    }
                }

                private float widthRdps;
                public float WidthRdps
                {
                    get
                    {
                        this.CalcWidth();
                        return this.widthRdps;
                    }
                }

                private float widthBestPer;
                public float WidthBestPer
                {
                    get
                    {
                        this.CalcWidth();
                        return this.widthBestPer;
                    }
                }

                private float widthJob;
                public float WidthJob
                {
                    get
                    {
                        this.CalcWidth();
                        return this.widthJob;
                    }
                }

                private void CalcWidth()
                {
                    lock (this.Lock)
                    {
                        if (this.width.HasValue) return;

                        var style = ImGui.GetStyle();
                        var padding = style.ItemSpacing.X * 2;

                        this.widthJob = Math.Max(this.Encounter.Max(e => e.Value.Job.ValueWidth), StrJob.ValueWidth) + padding;
                        this.widthRdps = Math.Max(this.Encounter.Max(e => e.Value.Rdps.ValueWidth), StrRdps.ValueWidth) + padding;
                        this.widthBestPer = Math.Max(this.Encounter.Max(e => e.Value.BestPer.ValueWidth), StrBestPer.ValueWidth) + padding;

                        this.width = Mathx.Max(
                            this.CharName.ValueWidth,
                            this.CharServer.ValueWidth,
                            this.widthJob + this.WidthRdps + this.WidthBestPer,
                            MinWidth
                        );

                        this.widthRdps = (float)(Math.Floor(this.width.Value - this.widthJob) / (this.widthRdps + this.widthBestPer) * this.widthRdps);
                        this.widthBestPer = this.width.Value - this.widthJob - this.widthRdps;
                    }
                }

                private void Update(FFlogsLog log)
                {
                    lock (this.Lock)
                    {
                        this.width = null;
                        this.UpdateInternal(log);
                    }
                }

                private void UpdateInternal(FFlogsLog log)
                {
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
                            Job     = StrNone,
                            BestAvg = StrDash,
                            Rank    = StrDash,
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
                            Job = new CellData(allstarJobBest),
                            BestAvg = new CellData
                            {
                                Value   = $"{bestAvg:#0.0}",
                                Color   = color,
                                ToolTip = $"{allstarJobBest.GetDescription()} : {killsTotal:#,##0} 킬\n전체 : {raidKills[0]:#,##0} Kills"
                            },
                            Rank = new CellData
                            {
                                Value   = $"{allstarData.Rank:#,##0}",
                                Color   = color,
                                ToolTip = $"상위 {rankPercent:##0.0} %%\n전체 {allstarData.Total:#,##0} 명",
                            },
                        };
                    }

                    foreach (var (_, enc) in raidEncounters) UpdateEncounter(enc);
                    foreach (var (_, enc) in ultiEncounters) UpdateEncounter(enc);

                    ////////////////////////////////////////////////////////////////////////////////////////////////////

                    void UpdateEncounter(GameEncounter enc)
                    {
                        var bestPerfJob =
                            log.EncountersNe
                            .Where(e => e.Key.EncounterId == enc)
                            .OrderByDescending(e => e.Value.MaxPer)
                            .Select(e => e.Key.JobId)
                            .FirstOrDefault();

                        if (bestPerfJob == GameJob.None || bestPerfJob == GameJob.Best)
                        {
                            this.Encounter[enc] = new EncounterData
                            {
                                Job = new CellData(),
                            };
                        }
                        else
                        {
                            var encData = log.EncountersNe[new EncounterDataKey
                            {
                                EncounterId = enc,
                                JobId = bestPerfJob,
                            }];

                            var color = FFlogsColor.GetColor(encData.MaxPer);
                            this.Encounter[enc] = new EncounterData
                            {
                                Job = new CellData(bestPerfJob),
                                Rdps = new CellData
                                {
                                    Value   = $"{encData.MaxRdps:#,##0.0}",
                                    Color   = color,
                                    ToolTip = $"{bestPerfJob.GetDescription()} : {encData.Kills:#,##0} 킬\n전체 : {raidKills[enc]:#,##0} Kills",
                                },
                                BestPer = new CellData
                                {
                                    Value   = $"{(Math.Floor(encData.MaxPer * 10) / 10):#0.0}",
                                    Color   = color,
                                    ToolTip = $"Max : {(Math.Floor(encData.MaxPer * 10) / 10):##0.0} %%\nMed : {(Math.Floor(encData.MedPer * 10) / 10):##0.0} %%",
                                }
                            };
                        }
                    }
                }
            }
        }
    }
}
