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
using ImGuiNET;

namespace FFLogsLookup.Gui
{
    internal class DetailGui : BaseGui
    {
        public enum Status
        {
            Normal,
            Loading,
            Error,
            Success,
        }

        public static readonly Vector4 ColorLoading = new(165 / 255f, 214 / 255f, 167 / 255f, 1f); // #90CAF9
        public static readonly Vector4 ColorError = new(239 / 255f, 154 / 255f, 154 / 255f, 1f); // #EF9A9A
        public static readonly Vector4 ColorSuccess = new(230 / 255f, 238 / 255f, 156 / 255f, 1f); // #A5D6A7

        public DetailGui(Plugin plugin)
            : base(plugin, "FFLogsLookup: 캐릭터 정보 조회")
        {
            this.drawingData = new();
            this.drawingData.Update(null);

            this.Size = Vector2.Zero;
            this.Flags = ImGuiWindowFlags.AlwaysAutoResize;
            this.SizeCondition = ImGuiCond.Always;
        }

        private readonly object drawLock = new();

        private readonly float[] button1Width = new float[3];

        private bool refreshButtonEnabled;

        private string charName;
        private GameServer charServer;

        private Status status = Status.Normal;
        private string statusString;

        private readonly Spinner spinner = new()
        {
            Radius = 7,
            Thickness = 3,
        };

        private readonly DrawingData drawingData;
        public override void Draw()
        {
            lock (this.drawLock)
            {
                this.DrawInternal();
            }
        }
        private void DrawInternal()
        {
            var authorized = this.plugin.FFlogsClient.Authorized;

            var style = ImGui.GetStyle();
            var paddingX = style.FramePadding.X * 2 + style.ColumnsMinSpacing * 2;

            if (this.button1Width[0] == 0)
            {
                this.button1Width[0] = ImGui.CalcTextSize("모그리").X + paddingX + style.ScrollbarSize + style.ScrollbarRounding;
                this.button1Width[1] = ImGui.CalcTextSize("가져오기").X + paddingX;
                this.button1Width[2] = ImGui.CalcTextSize("초기화").X + paddingX;
            }

            var charNameWidth = Math.Max(
                100,
                ImGui.GetWindowWidth() - this.button1Width[0] - this.button1Width[1] - this.button1Width[2]);

            this.Size = new Vector2(
                Math.Max(this.drawingData.Width, charNameWidth + this.button1Width[0] + this.button1Width[1] + this.button1Width[2]),
                0);

            ImGui.Columns(4, "##input-columns", false);
            ImGui.SetColumnWidth(0, charNameWidth);
            ImGui.SetColumnWidth(1, this.button1Width[0]);
            ImGui.SetColumnWidth(2, this.button1Width[1]);
            ImGui.SetColumnWidth(3, this.button1Width[2]);

            ImGui.SetNextItemWidth(charNameWidth - style.ColumnsMinSpacing * 2);
            var charName = this.charName ?? string.Empty;
            if (ImGui.InputTextWithHint("##charName", "캐릭터 명", ref charName, 256, ImGuiInputTextFlags.CharsNoBlank))
            {
                this.charName = charName;
            }

            ImGui.NextColumn();
            ImGui.SetNextItemWidth(this.button1Width[0] - style.ColumnsMinSpacing * 2);
            if (ImGui.BeginCombo("##serverName", this.charServer.S()))
            {
                var cur = this.charServer;
                foreach (var server in Enum.GetValues<GameServer>())
                {
                    if (ImGui.Selectable(server.S(), server == cur))
                    {
                        this.charServer = server;
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.NextColumn();
            ImGui.SetNextItemWidth(this.button1Width[1]);
            if (!this.refreshButtonEnabled || !authorized)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.2f);
            }
            var buttonClicked = ImGui.Button("가져오기");
            if (!this.refreshButtonEnabled || !authorized)
            {
                ImGui.PopStyleVar();
            }
            if (buttonClicked && this.refreshButtonEnabled && authorized)
            {
                this.refreshButtonEnabled = false;
                this.Update(this.charName, this.charServer);
            }

            ImGui.NextColumn();
            ImGui.SetNextItemWidth(this.button1Width[2]);
            if (ImGui.Button("초기화"))
            {
                this.charName = string.Empty;
                this.charServer = GameServer.Moogle;

                this.status = Status.Normal;
                this.statusString = null;
                this.drawingData.Update(null);
            }

            ImGui.Columns();

            ////////////////////////////////////////////////////////////////////////////////////////////////////

            ImGui.Separator();

            var c = ImGui.GetWindowWidth() - 30;
            ImGui.Columns(2, "##columns2", false);
            ImGui.SetColumnWidth(0, c);
            ImGui.SetColumnWidth(1, 30);

            ImGui.SetNextItemWidth(c - style.ColumnsMinSpacing * 2);
            var pop = true;
            switch(this.status)
            {
            case Status.Loading:
                ImGui.PushStyleColor(ImGuiCol.Text, ColorLoading);
                break;

            case Status.Error:
                ImGui.PushStyleColor(ImGuiCol.Text, ColorError);
                break;

            case Status.Success:
                ImGui.PushStyleColor(ImGuiCol.Text, ColorSuccess);
                break;

            default:
                pop = false;
                break;
            };
            if (!authorized)
            {
                ImGui.TextUnformatted("Api ID 와 Api Secret 을 설정해주세요.");
            }
            else
            {
                ImGui.TextUnformatted(this.statusString ?? string.Empty);
            }
            if (pop)
            {
                ImGui.PopStyleColor();
            }

            ImGui.NextColumn();
            if (this.status == Status.Loading)
            {
                this.spinner.Draw();
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            ImGui.Columns();

            ImGui.Separator();

            lock (this.drawingData)
            {
                this.drawingData.Draw();
            }
        }

        private CancellationTokenSource cancellationTokenSource;
        public void Update(string charName, GameServer charServer)
        {
            this.cancellationTokenSource?.Cancel();
            this.cancellationTokenSource?.Dispose();
            this.cancellationTokenSource = new CancellationTokenSource();

            var token = this.cancellationTokenSource.Token;

            this.IsOpen = true;

            var cn = (string)charName.Clone();
            var cs = charServer;

            this.charName = (string)charName.Clone();
            this.charServer = cs;

            lock (drawLock)
            {
                this.refreshButtonEnabled = false;

                this.status = Status.Loading;
                this.statusString = $"{cn}@{cs} 정보 조회 중";

                this.drawingData.Update(null);
            }

            _ = Task.Factory.StartNew(async () =>
            {
                try
                {
                    var (log, cached) = await this.plugin.FFlogsClient.GetLogs(cn, cs, false, token);
                    token.ThrowIfCancellationRequested();

                    var forced = false;
                    try
                    {
                        lock (this.drawingData)
                        {
                            this.drawingData.Update(log);

                            if (!cached) this.status = Status.Success;
                            this.statusString = $"{cn}@{cs.S()}";
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
                        (log, _) = await this.plugin.FFlogsClient.GetLogs(cn, cs, true, token);
                        token.ThrowIfCancellationRequested();

                        lock (this.drawingData)
                        {
                            this.drawingData.Update(log);

                            this.status = Status.Success;
                            this.statusString = $"{cn}@{cs.S()}";
                        }
                    }

                    lock (drawLock)
                    {
                        this.status = Status.Success;
                        this.refreshButtonEnabled = true;
                    }
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception ex)
                {
                    PluginLog.Error(ex, $"{nameof(DetailGui)}.{nameof(this.Update)}");
                    lock (this.drawLock)
                    {
                        this.status = Status.Error;
                        this.statusString = $"{cn}@{cs.S()} 정보 조회 오류";

                        this.refreshButtonEnabled = true;
                    }
                }
            });
        }

        private class DrawingData
        {
            private static readonly CellData StrRank = new("RANK");
            private static readonly CellData StrAvg = new("AVG %");
            private static readonly CellData StrJob = new("JOB");
            private static readonly CellData StrRdps = new("rDPS");
            private static readonly CellData StrBestPer = new("BEST %");
            private static readonly CellData StrMedPer = new("MED %");
            private static readonly CellData StrKills = new("Kills");
            private static readonly CellData StrNone = new("");
            private static readonly CellData StrAllstarEng = new("Allstar");

            private static readonly CellData StrRaid = new("에덴 영식");
            private static readonly CellData StrUlti = new("절 토벌전");
            private static readonly CellData StrAllstar = new("올스타");

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

            private static readonly GameJob[][] raidJobColumn = new GameJob[][]
            {
                new GameJob[]
                {
                    GameJob.Best, GameJob.Best,
                    GameJob.Paladin, GameJob.Warrior, GameJob.DarkKnight, GameJob.Gunbreaker, GameJob.Best,
                    GameJob.WhiteMage, GameJob.Scholar, GameJob.Astrologian,
                },
                new GameJob[]
                {
                    GameJob.Monk, GameJob.Dragoon, GameJob.Ninja, GameJob.Samurai, GameJob.Best,
                    GameJob.Bard, GameJob.Machinist, GameJob.Dancer, GameJob.Best,
                    GameJob.BlackMage, GameJob.Summoner, GameJob.RedMage,
                },
            };

            private class JobData
            {
                public class AllstarData
                {
                    public CellData Job     { get; } = new();
                    public CellData BestAvg { get; } = new();
                    public CellData Rank    { get; } = new();
                    public CellData AllStar { get; } = new();
                    public CellData Kills   { get; } = new();
                }
                public class EncounterData
                {
                    public CellData Job     { get; } = new();
                    public CellData Rdps    { get; } = new();
                    public CellData BestPer { get; } = new();
                    public CellData MedPer  { get; } = new();
                    public CellData Kills   { get; } = new();
                }
                public AllstarData AllStar { get; } = new();
                public Dictionary<GameEncounter, EncounterData> Encounter { get; } = new();
            }

            private readonly Dictionary<GameJob, JobData> jobData = new();

            private readonly Dictionary<GameJob, Vector4> raidPlayed = new();
            private readonly Dictionary<GameJob, Vector4> ultiPlayed = new();

            private GameJob jobCurrentRaid = GameJob.Best;
            private GameJob jobCurrentUlti = GameJob.Best;

            private readonly CellData UpdatedAt = new();

            private float? width;
            private float width0FirstColumns;
            private float width1Job;
            private float width2RdpsOrRank;
            private float width3AvgOrBest;
            private float width4AllstarOrMed;
            private float width5Kills;

            public float Width
            {
                get
                {
                    this.CalcWidth();
                    return this.width.Value;
                }
            }

            public DrawingData()
            {
                foreach (var job in Enum.GetValues<GameJob>())
                {
                    if (job == GameJob.None) continue;

                    var item = new JobData();
                    this.jobData[job] = item;

                    foreach (var (_, enc) in raidEncounters) item.Encounter[enc] = new JobData.EncounterData();
                    foreach (var (_, enc) in ultiEncounters) item.Encounter[enc] = new JobData.EncounterData();
                }
            }

            public void CalcWidth()
            {
                if (this.width.HasValue) return;

                var style = ImGui.GetStyle();
                var padding = style.ItemSpacing.X * 2;

                this.width0FirstColumns = padding + Mathx.Max(
                    StrRaid.ValueWidth,
                    StrUlti.ValueWidth,
                    StrAllstar.ValueWidth,
                    raidEncounters.Max(e => e.str.ValueWidth),
                    ultiEncounters.Max(e => e.str.ValueWidth)
                );

                this.width1Job = padding + Mathx.Max(
                    StrJob.ValueWidth,
                    jobData.Max(e => e.Value.AllStar.Job.ValueWidth),
                    jobData.Max(e => e.Value.Encounter.Max(ee => ee.Value.Job.ValueWidth))
                );

                this.width2RdpsOrRank = padding + Mathx.Max(
                    StrRank.ValueWidth,
                    StrRdps.ValueWidth,
                    jobData.Max(e => e.Value.AllStar.Rank.ValueWidth),
                    jobData.Max(e => e.Value.Encounter.Max(ee => ee.Value.Rdps.ValueWidth))
                );

                this.width3AvgOrBest = padding + Mathx.Max(
                    StrAvg.ValueWidth,
                    StrBestPer.ValueWidth,
                    jobData.Max(e => e.Value.AllStar.BestAvg.ValueWidth),
                    jobData.Max(e => e.Value.Encounter.Max(ee => ee.Value.BestPer.ValueWidth))
                );

                this.width4AllstarOrMed = padding + Mathx.Max(
                    StrAllstarEng.ValueWidth,
                    StrMedPer.ValueWidth,
                    jobData.Max(e => e.Value.AllStar.AllStar.ValueWidth),
                    jobData.Max(e => e.Value.Encounter.Max(ee => ee.Value.MedPer.ValueWidth))
                );

                this.width5Kills = padding + Mathx.Max(
                    StrKills.ValueWidth,
                    jobData.Max(e => e.Value.AllStar.Kills.ValueWidth),
                    jobData.Max(e => e.Value.Encounter.Max(ee => ee.Value.Kills.ValueWidth))
                );

                this.width =
                    this.width0FirstColumns +
                    this.width1Job +
                    this.width2RdpsOrRank +
                    this.width3AvgOrBest +
                    this.width4AllstarOrMed +
                    this.width5Kills;
            }

            private readonly object Lock = new();
            public void Draw()
            {
                lock (this.Lock)
                {
                    this.DrawInternal();
                }
            }
            public void DrawInternal()
            {
                this.CalcWidth();

                var style = ImGui.GetStyle();
                var padding = style.ItemSpacing.X * 2;

                this.jobCurrentRaid = DrawJobs("##logs-raid-job", this.raidPlayed, this.jobCurrentRaid);
                ImGui.Spacing();

                ImGui.Separator();

                DrawEncounter(this.jobData[this.jobCurrentRaid], raidEncounters, true);
                ImGui.Columns();

                ////////////////////////////////////////////////////////////////////////////////////////////////////

                ImGui.Spacing();

                this.jobCurrentUlti = DrawJobs("##logs-ulti-job", this.ultiPlayed, this.jobCurrentUlti);
                ImGui.Spacing();

                ImGui.Separator();

                ImGui.Columns(6, "##logs", false);
                DrawEncounter(this.jobData[this.jobCurrentUlti], ultiEncounters, false);
                ImGui.Columns();

                ////////////////////////////////////////////////////////////////////////////////////////////////////

                ImGui.Separator();

                this.UpdatedAt.Draw(ImGui.GetWindowWidth() - padding, padding, CellData.Align.Right);

                ////////////////////////////////////////////////////////////////////////////////////////////////////

                GameJob DrawJobs(string id, Dictionary<GameJob, Vector4> played, GameJob gameJob)
                {
                    var jobCellWidth = (ImGui.GetWindowWidth() - style.WindowPadding.X * 2) / 12;

                    ImGui.PushStyleColor(ImGuiCol.FrameBg, Vector4.Zero);
                    ImGui.PushStyleColor(ImGuiCol.FrameBgActive, Vector4.Zero);
                    ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, Vector4.Zero);
                    ImGui.PushStyleColor(ImGuiCol.ChildBg, Vector4.Zero);

                    ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, Vector2.Zero);
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, Vector2.Zero);
                    ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 2));
                    ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0);
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

                    {
                        ImGui.Columns(10, id, false);

                        for (int i = 0; i < raidJobColumn[0].Length; i++)
                        {
                            if (i == 0)
                            {
                                ImGui.SetColumnWidth(i, jobCellWidth * 3);
                            }
                            else
                            {
                                ImGui.SetColumnWidth(i, jobCellWidth);
                            }
                        }

                        for (int i = 0; i < raidJobColumn[0].Length; i++)
                        {
                            if (i == 0)
                            {
                                ImGui.SetNextItemWidth(jobCellWidth * 3 - style.ColumnsMinSpacing * 2);
                                if (ImGui.Button($"BEST{id}-best", new(jobCellWidth * 3, 0)))
                                {
                                    PluginLog.Debug("BEST");
                                    gameJob = GameJob.Best;
                                }
                            }
                            else
                            {
                                ImGui.NextColumn();
                                gameJob = Append(raidJobColumn[0][i], jobCellWidth, gameJob);
                            }
                        }

                        ImGui.Columns();

                        ImGui.PopStyleVar();
                        ImGui.Spacing();
                        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

                        ////////////////////////////////////////////////////////////

                        ImGui.Columns(12, "##logs-raid-job2", false);

                        for (int i = 0; i < raidJobColumn[1].Length; i++)
                        {
                            ImGui.SetColumnWidth(i, jobCellWidth);
                        }
                        for (int i = 0; i < raidJobColumn[1].Length; i++)
                        {
                            if (i != 0) ImGui.NextColumn();

                            gameJob = Append(raidJobColumn[1][i], jobCellWidth, gameJob);
                        }

                        ImGui.Columns();
                    }
                    ImGui.PopStyleVar();
                    ImGui.PopStyleVar();
                    ImGui.PopStyleVar();
                    ImGui.PopStyleVar();
                    ImGui.PopStyleVar();

                    ImGui.PopStyleColor();
                    ImGui.PopStyleColor();
                    ImGui.PopStyleColor();
                    ImGui.PopStyleColor();

                    return gameJob;

                    GameJob Append(GameJob job, float width, GameJob gameJob)
                    {
                        if (job == GameJob.Best || job == GameJob.None) return gameJob;

                        ImGui.SetNextItemWidth(width - style.ColumnsMinSpacing * 2);

                        ImGui.PushFont(PluginFont.JobIcon);
                        if (played.TryGetValue(job, out var color))
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, color);
                            if (ImGui.Button($"{GameData.GetGlyph(job)}{id}-best", new(jobCellWidth, 0)))
                            {
                                PluginLog.Debug(job.GetDescription());
                                gameJob = job;
                            }
                            ImGui.PopStyleColor();
                        }
                        else
                        {
                            //ImGui.Button(PluginFont.GetJobGlyph(job));
                            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.2f);
                            ImGui.TextUnformatted(job.GetGlyph());
                            ImGui.PopStyleVar();
                        }
                        ImGui.PopFont();

                        return gameJob;
                    }
                }

                void DrawEncounter(JobData jobData, (CellData str, GameEncounter enc)[] encounters, bool drawAllstar)
                {
                    ImGui.Columns(6, "##dps", true);

                    ImGui.SetColumnWidth(0, this.width0FirstColumns);
                    ImGui.SetColumnWidth(1, this.width1Job);
                    ImGui.SetColumnWidth(2, this.width2RdpsOrRank);
                    ImGui.SetColumnWidth(3, this.width3AvgOrBest);
                    ImGui.SetColumnWidth(4, this.width4AllstarOrMed);
                    ImGui.SetColumnWidth(5, this.width5Kills);

                    //////////////////////////////////////////////////////////////////////////////////////////////////// 0

                    if (drawAllstar)
                    {
                        StrAllstar.Draw(this.width0FirstColumns, padding, CellData.Align.Center);
                        ImGui.Spacing();
                        StrNone.Draw(this.width0FirstColumns, padding, CellData.Align.Center);
                        ImGui.Spacing();
                    }

                    StrRaid.Draw(this.width0FirstColumns, padding, CellData.Align.Center);
                    ImGui.Spacing();
                    foreach (var (str, _) in encounters)
                    {
                        str.Draw(this.width0FirstColumns, padding, CellData.Align.Center);
                    }
                    ImGui.Spacing();

                    //////////////////////////////////////////////////////////////////////////////////////////////////// 1

                    ImGui.NextColumn();
                    if (drawAllstar)
                    {
                        StrJob.Draw(this.width1Job, padding, CellData.Align.Center);
                        ImGui.Spacing();
                        jobData.AllStar.Job.Draw(this.width1Job, padding, CellData.Align.Center);
                        ImGui.Spacing();
                    }

                    StrJob.Draw(this.width1Job, padding, CellData.Align.Center);
                    ImGui.Spacing();
                    foreach (var (_, enc) in encounters)
                    {
                        jobData.Encounter[enc].Job.Draw(this.width1Job, padding, CellData.Align.Center);
                    }
                    ImGui.Spacing();

                    //////////////////////////////////////////////////////////////////////////////////////////////////// 2

                    ImGui.NextColumn();
                    if (drawAllstar)
                    {
                        StrRank.Draw(this.width2RdpsOrRank, padding, CellData.Align.Center);
                        ImGui.Spacing();
                        jobData.AllStar.Rank.Draw(this.width2RdpsOrRank, padding, CellData.Align.Center);
                        ImGui.Spacing();
                    }

                    StrRdps.Draw(this.width2RdpsOrRank, padding, CellData.Align.Center);
                    ImGui.Spacing();
                    foreach (var (_, enc) in encounters)
                    {
                        jobData.Encounter[enc].Rdps.Draw(this.width2RdpsOrRank, padding, CellData.Align.Right);
                    }
                    ImGui.Spacing();

                    //////////////////////////////////////////////////////////////////////////////////////////////////// 3

                    ImGui.NextColumn();
                    if (drawAllstar)
                    {
                        StrAvg.Draw(this.width3AvgOrBest, padding, CellData.Align.Center);
                        ImGui.Spacing();
                        jobData.AllStar.BestAvg.Draw(this.width3AvgOrBest, padding, CellData.Align.Center);
                        ImGui.Spacing();
                    }

                    StrBestPer.Draw(this.width3AvgOrBest, padding, CellData.Align.Center);
                    ImGui.Spacing();
                    foreach (var (_, enc) in encounters)
                    {
                        jobData.Encounter[enc].BestPer.Draw(this.width3AvgOrBest, padding, CellData.Align.Right);
                    }
                    ImGui.Spacing();

                    //////////////////////////////////////////////////////////////////////////////////////////////////// 4

                    ImGui.NextColumn();
                    if (drawAllstar)
                    {
                        StrAllstar.Draw(this.width4AllstarOrMed, padding, CellData.Align.Center);
                        ImGui.Spacing();
                        jobData.AllStar.AllStar.Draw(this.width4AllstarOrMed, padding, CellData.Align.Center);
                        ImGui.Spacing();
                    }

                    StrMedPer.Draw(this.width4AllstarOrMed, padding, CellData.Align.Center);
                    ImGui.Spacing();
                    foreach (var (_, enc) in encounters)
                    {
                        jobData.Encounter[enc].MedPer.Draw(this.width4AllstarOrMed, padding, CellData.Align.Right);
                    }
                    ImGui.Spacing();

                    //////////////////////////////////////////////////////////////////////////////////////////////////// 5

                    ImGui.NextColumn();
                    if (drawAllstar)
                    {
                        StrKills.Draw(this.width5Kills, padding, CellData.Align.Center);
                        ImGui.Separator();
                        jobData.AllStar.Kills.Draw(this.width5Kills, padding, CellData.Align.Center);
                        ImGui.Separator();
                    }

                    StrKills.Draw(this.width5Kills, padding, CellData.Align.Center);
                    ImGui.Separator();
                    foreach (var (_, enc) in encounters)
                    {
                        jobData.Encounter[enc].Kills.Draw(this.width5Kills, padding, CellData.Align.Right);
                    }
                    ImGui.Separator();

                    ImGui.Columns();
                }
            }

            public void Update(FFlogsLog log)
            {
                lock (this.Lock)
                {
                    this.UpdateInternal(log);
                }
            }

            private void UpdateInternal(FFlogsLog log)
            {
                log ??= FFlogsLog.Empty;

                this.jobCurrentRaid = GameJob.Best;
                this.jobCurrentUlti = GameJob.Best;

                this.width = null;
                this.UpdatedAt.Value = log == FFlogsLog.Empty ? "-" : log.UpdatedAtUtc.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss \"기준\"");

                //var containsEcho = this.plugin.PluginConfig.ApiContainsEcho;

                //var allstarData   = (containsEcho ? log.RaidAllstarNc : log.RaidAllstarEc);
                //var encounterData = (containsEcho ? log.EncountersNc : log.EncountersEc);
                var allstarData   = log.RaidAllstarNe;
                var encounterData = log.EncountersNe ;

                ////////////////////////////////////////////////////////////////////////////////////////////////////

                // 전체 클리어 횟수 미리 계산하기
                var raidKills = new Dictionary<GameEncounter, int>();
                foreach (var (_, enc) in raidEncounters)
                {
                    raidKills[enc] =
                        encounterData
                        .Where(e =>e.Key.EncounterId == enc)
                        .Sum(e => e.Value.Kills);
                }
                raidKills[0] = raidKills.Sum(e => e.Value);

                this.raidPlayed.Clear();
                this.ultiPlayed.Clear();

                foreach (var currentJob in Enum.GetValues<GameJob>())
                {
                    if (currentJob == GameJob.None) continue;

                    UpdateAllstar(currentJob);

                    foreach (var (_, enc) in raidEncounters)
                    {
                        UpdateEncounter(currentJob, enc);
                    }
                    foreach (var (_, enc) in ultiEncounters)
                    {
                        UpdateEncounter(currentJob, enc);
                    }

                    // 절 토벌전 평균
                    var jobData = this.jobData[currentJob];

                    if (encounterData.Any(e => !e.Key.EncounterId.IsRaids() && e.Key.JobId == currentJob))
                    {
                        var avg = encounterData.Where(e => !e.Key.EncounterId.IsRaids() && e.Key.JobId == currentJob).Average(e => e.Value.MaxPer);
                        this.ultiPlayed[currentJob] = FFlogsColor.GetColor(avg);
                    }

                }
                ////////////////////////////////////////////////////////////////////////////////////////////////////

                void UpdateAllstar(GameJob currentJob)
                {
                    var jobData = this.jobData[currentJob];

                    GameJob? targetJob;
                    if (currentJob != GameJob.Best)
                    {
                        targetJob = currentJob;
                    }
                    else
                    {
                        targetJob =
                            allstarData
                            .OrderBy(e => e.Value.Rank)
                            .Select(e => e.Key)
                            .FirstOrDefault();
                    }

                    if (targetJob.HasValue && allstarData.TryGetValue(targetJob.Value, out var item))
                    {
                        var jobKills = encounterData
                            .Where(e => e.Key.EncounterId.IsRaids() && e.Key.JobId == targetJob)
                            .Sum(e => e.Value.Kills);

                        var bestAvg = encounterData
                            .Where(e => e.Key.EncounterId.IsRaids() && e.Key.JobId == targetJob)
                            .GroupBy(e => e.Key.EncounterId)
                            .Average(e => e.Max(ee => ee.Value.MaxPer));

                        jobData.AllStar.Job.SetByJob(targetJob.Value);
                        jobData.AllStar.BestAvg.Value = bestAvg.ToString("#0.0");
                        jobData.AllStar.Rank   .Value = item.Rank.ToString("#,##0");
                        jobData.AllStar.AllStar.Value = item.Point.ToString("##0.00");
                        jobData.AllStar.Kills  .Value = jobKills.ToString("#,##0");

                        // ----------------------------------------------------------------------------------------------------

                        var rankPercent = (float)(item.Total - item.Rank + 1) / item.Total * 100;
                        jobData.AllStar.Rank.Color = FFlogsColor.GetColor(rankPercent);

                        var avgColor = FFlogsColor.GetColor(bestAvg);
                        this.raidPlayed[targetJob.Value] = avgColor;
                        jobData.AllStar.BestAvg.Color = avgColor;
                        jobData.AllStar.AllStar.Color = avgColor;

                        // ----------------------------------------------------------------------------------------------------

                        jobData.AllStar.Rank   .ToolTip = $"{item.Rank:#,##0} / {item.Total:#,##0}\n상위 {(Math.Floor(rankPercent * 10) / 10):##0.0} %%";
                        jobData.AllStar.Kills  .ToolTip = $"{targetJob?.GetDescription()} : {jobKills}\n전체 : {raidKills[0]}";
                    }
                    else
                    {
                        jobData.AllStar.Job    .Value = null;
                        jobData.AllStar.BestAvg.Value = null;
                        jobData.AllStar.Rank   .Value = null;
                        jobData.AllStar.AllStar.Value = null;
                        jobData.AllStar.Kills  .Value = null;
                    }
                }

                void UpdateEncounter(GameJob job, GameEncounter enc)
                {
                    var jobData = this.jobData[job];

                    GameJob? targetJob;

                    if (job != GameJob.Best)
                    {
                        targetJob = job;
                    }
                    else
                    {
                        targetJob =
                            encounterData
                            .Where(e => e.Key.EncounterId == enc)
                            .OrderByDescending(e => e.Value.MaxPer)
                            .Select(e => e.Key.JobId)
                            .FirstOrDefault();
                    }

                    var key = new EncounterDataKey
                    {
                        JobId = targetJob.Value,
                        EncounterId = enc,
                    };

                    var jobEncData = jobData.Encounter[enc];

                    if (targetJob.HasValue && encounterData.TryGetValue(key, out var encData))
                    {
                        var color = FFlogsColor.GetColor(encData.MaxPer);

                        jobEncData.Job.SetByJob(targetJob.Value);
                        jobEncData.Rdps   .Value = encData.MaxRdps.ToString("#,##0.0");
                        jobEncData.BestPer.Value = (Math.Floor(encData.MaxPer * 10) / 10).ToString("##0.0");
                        jobEncData.MedPer .Value = (Math.Floor(encData.MedPer * 10) / 10).ToString("##0.0");
                        jobEncData.Kills  .Value = encData.Kills.ToString("#,##0");

                        // ----------------------------------------------------------------------------------------------------

                        var colorbestPer = FFlogsColor.GetColor(encData.MaxPer);
                        jobEncData.Rdps.Color = colorbestPer;
                        jobEncData.BestPer.Color = colorbestPer;

                        jobEncData.MedPer.Color = FFlogsColor.GetColor(encData.MedPer);

                        // ----------------------------------------------------------------------------------------------------



                        jobEncData.Kills.ToolTip = $"{targetJob?.GetDescription()} : {encData.Kills}\n전체 : {raidKills[0]}";
                    }
                    else
                    {
                        jobEncData.Job    .Value = null;
                        jobEncData.Rdps   .Value = null;
                        jobEncData.BestPer.Value = null;
                        jobEncData.MedPer .Value = null;
                        jobEncData.Kills  .Value = null;
                    }
                }
            }
        }
    }
}
