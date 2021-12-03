using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Logging;
using FFLogsLookup.FFlogs;
using FFLogsLookup.Game;
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
            this.drawingData = new(this);
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
                Math.Max(this.drawingData.GetWidth(), charNameWidth + this.button1Width[0] + this.button1Width[1] + this.button1Width[2]),
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
            private const float JobColumnWidth = 20;

            private struct Data
            {
                public Data(GameJob gameJob)
                {
                    this.Job = gameJob;
                    this.Text = gameJob.S();
                    this.Color = gameJob.C();
                    this.ToolTip = gameJob.S();
                    this.Width = 0;
                }
                public Data(string text, Vector4 color, string toolTip = null)
                {
                    this.Job     = null;
                    this.Text    = text;
                    this.Color   = color;
                    this.ToolTip = toolTip ?? string.Empty;
                    this.Width   = 0;
                }

                public GameJob? Job     { get; set; }
                public string   Text    { get; set; }
                public Vector4  Color   { get; set; }
                public string   ToolTip { get; set; }
                public float    Width   { get; set; }

                public Data UpdateWidth(float width)
                {
                    this.Width = width;
                    return this;
                }
            }

            private class JobData
            {
                public List<Data[]> TextData { get; } = new();
                public float[] ColumnWidth { get;} = new float[6];

                public void UpdateWidth(DrawingData parent, float spacingX)
                {
                    for (int i = 0; i < this.TextData.Count; i++)
                    {
                        var arr = this.TextData[i];
                        if (arr == null) continue;

                        for (int k = 0; k < 6; k++)
                        {
                            if (!string.IsNullOrWhiteSpace(arr[k].Text))
                            {
                                if (arr[k].Job.HasValue)
                                {
                                    /*
                                    ImGui.PushFont(parent.detailGui.plugin.PluginFont.JobIcon);
                                    arr[k] = arr[k].UpdateWidth(ImGui.CalcTextSize(PluginFont.GetJobGlyph(arr[k].Job.Value)).X);
                                    ImGui.PopFont();
                                    */
                                    arr[k] = arr[k].UpdateWidth(20);
                                }
                                else
                                {
                                    arr[k] = arr[k].UpdateWidth(ImGui.CalcTextSize(arr[k].Text).X);
                                }
                            }
                        }

                        this.TextData[i] = arr;
                    }

                    for (int i = 0; i < 6; i++)
                    {
                        this.ColumnWidth[i] = this.TextData.Where(e => e != null).Max(e => e[i].Width) + spacingX * 2;
                    }
                }
            }

            private readonly DetailGui detailGui;

            private readonly Dictionary<GameJob, JobData> drawDataRaid = new();
            private readonly Dictionary<GameJob, JobData> drawDataUlti = new();

            private readonly Dictionary<GameJob, Vector4> colorRaid = new();
            private readonly Dictionary<GameJob, Vector4> colorUlti = new();

            private GameJob jobCurrentRaid = GameJob.Best;
            private GameJob jobCurrentUlti = GameJob.Best;

            private string updatedAtString = string.Empty;
            private float updatedAtWidth = 0;

            private static readonly (string, GameEncounter)[] edenEncounters =
            {
                ("1층"     , GameEncounter.E9s       ),
                ("2층"     , GameEncounter.E10s      ),
                ("3층"     , GameEncounter.E11s      ),
                ("4층 전반", GameEncounter.E12sDoor  ),
                ("4층 후반", GameEncounter.E12sOracle),
            };
            private static readonly (string, GameEncounter)[] ultiEncounters =
            {
                ("알렉산더", GameEncounter.Tea ),
                ("알테마"  , GameEncounter.Ucob),
                ("바하무트", GameEncounter.Uwu ),
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

            public DrawingData(DetailGui detailGui)
            {
                this.detailGui = detailGui;

                foreach (var job in Enum.GetValues<GameJob>())
                {
                    this.drawDataRaid[job] = new JobData();
                    this.drawDataUlti[job] = new JobData();
                }
            }

            public float GetWidth()
            {
                var dataRaid = this.drawDataRaid[this.jobCurrentRaid];
                var dataUlti = this.drawDataUlti[this.jobCurrentUlti];

                return Math.Max(
                    Math.Max(
                        dataRaid.ColumnWidth[0] +
                            dataRaid.ColumnWidth[1] +
                            dataRaid.ColumnWidth[2] +
                            dataRaid.ColumnWidth[3] +
                            dataRaid.ColumnWidth[4] +
                            dataRaid.ColumnWidth[5],
                        dataUlti.ColumnWidth[0] +
                            dataUlti.ColumnWidth[1] +
                            dataUlti.ColumnWidth[2] +
                            dataUlti.ColumnWidth[3] +
                            dataUlti.ColumnWidth[4] +
                            dataUlti.ColumnWidth[5]
                    ),
                    JobColumnWidth * 12
                );
            }

            public void Draw()
            {
                var style = ImGui.GetStyle();

                var dataRaid = this.drawDataRaid[this.jobCurrentRaid];
                var dataUlti = this.drawDataUlti[this.jobCurrentUlti];

                var width = new float[]
                {
                    Math.Max(dataRaid.ColumnWidth[0], dataUlti.ColumnWidth[0]),
                    Math.Max(dataRaid.ColumnWidth[1], dataUlti.ColumnWidth[1]),
                    Math.Max(dataRaid.ColumnWidth[2], dataUlti.ColumnWidth[2]),
                    Math.Max(dataRaid.ColumnWidth[3], dataUlti.ColumnWidth[3]),
                    Math.Max(dataRaid.ColumnWidth[4], dataUlti.ColumnWidth[4]),
                    Math.Max(dataRaid.ColumnWidth[5], dataUlti.ColumnWidth[5]),
                };

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////////

                DrawJobs(this.colorRaid, style, ref this.jobCurrentRaid);
                ImGui.Spacing();

                ImGui.Separator();
                ImGui.Columns(6, "##logs", false);

                for (var k = 0; k < 6; k++)
                {
                    ImGui.SetColumnWidth(k, width[k]);
                    if (k != 0) ImGui.NextColumn();
                    DrawColumn(dataRaid, width[k], k, style.ItemSpacing.X);
                }

                ImGui.Columns();

                ////////////////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////////
                ////////////////////////////////////////////////////////////////////////////////////////////////////

                ImGui.Spacing();

                DrawJobs(this.colorUlti, style, ref this.jobCurrentUlti);
                ImGui.Spacing();

                ImGui.Separator();
                ImGui.Columns(6, "##logs", false);

                for (var k = 0; k < 6; k++)
                {
                    ImGui.SetColumnWidth(k, width[k]);
                    if (k != 0) ImGui.NextColumn();
                    DrawColumn(dataUlti, width[k], k, style.ItemSpacing.X);
                }

                ImGui.Columns();

                ////////////////////////////////////////////////////////////////////////////////////////////////////

                ImGui.Separator();

                ImGui.SetCursorPosX(ImGui.GetWindowWidth() - this.updatedAtWidth - style.ItemSpacing.X);
                ImGui.TextUnformatted(this.updatedAtString);

                ////////////////////////////////////////////////////////////////////////////////////////////////////

                void DrawJobs(Dictionary<GameJob, Vector4> colorData, ImGuiStylePtr style, ref GameJob gameJob)
                {
                    var jobCellWidth = Math.Max((ImGui.GetWindowWidth() - style.WindowPadding.X * 2) / 12, JobColumnWidth);

                    ImGui.PushStyleColor(ImGuiCol.FrameBg, Vector4.Zero);
                    ImGui.PushStyleColor(ImGuiCol.FrameBgActive, Vector4.Zero);
                    ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, Vector4.Zero);
                    ImGui.PushStyleColor(ImGuiCol.ChildBg, Vector4.Zero);

                    ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, Vector2.Zero);
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, Vector2.Zero);
                    ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, Vector2.Zero);
                    ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0);
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
                    {

                        ImGui.Columns(10, "##logs-raid-job", false);

                        for (int i = 0; i < raidJobColumn[0].Length; i++)
                        {
                            if (i == 0)
                            {
                                ImGui.SetColumnWidth(i, jobCellWidth * 3);
                            }
                            else
                            {
                                ImGui.SetColumnWidth(i, jobCellWidth);
                                ImGui.NextColumn();
                            }

                            if (i == 0)
                            {
                                ImGui.SetNextItemWidth(jobCellWidth * 3 - style.ColumnsMinSpacing * 2);
                                if (ImGui.Button("BEST", new(jobCellWidth * 3, 0)))
                                {
                                    gameJob = GameJob.Best;
                                }
                            }
                            else
                            {
                                Append(raidJobColumn[0][i], jobCellWidth, style, ref gameJob);
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
                            if (i != 0) ImGui.NextColumn();

                            Append(raidJobColumn[1][i], jobCellWidth, style, ref gameJob);
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

                    void Append(GameJob job, float width, ImGuiStylePtr style, ref GameJob gameJob)
                    {
                        if (job == GameJob.Best) return;

                        ImGui.SetNextItemWidth(width - style.ColumnsMinSpacing * 2);

                        ImGui.PushFont(this.detailGui.plugin.PluginFont.JobIcon);
                        if (colorData.TryGetValue(job, out var color))
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, color);
                            if (ImGui.Button(PluginFont.GetJobGlyph(job), new(jobCellWidth, 0)))
                            {
                                gameJob = job;
                            }
                            ImGui.PopStyleColor();
                        }
                        else
                        {
                            var chr = PluginFont.GetJobGlyph(job);

                            //ImGui.Button(PluginFont.GetJobGlyph(job));c
                            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.2f);
                            ImGui.TextUnformatted(chr);
                            ImGui.PopStyleVar();
                        }
                        ImGui.PopFont();
                    }
                }

                void DrawColumn(JobData data, float width, int k, float spacing)
                {
                    for (int i = 0; i < data.TextData.Count; i++)
                    {
                        if (data.TextData[i] == null)
                        {
                            if (k == 5) ImGui.Separator();
                            else ImGui.Spacing();

                            continue;
                        }

                        var d = data.TextData[i][k];

                        if (d.Job.HasValue)
                        {
                            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (width - spacing * 2 - d.Width) / 2);
                            ImGui.PushStyleColor(ImGuiCol.Text, d.Color);
                            ImGui.PushFont(this.detailGui.plugin.PluginFont.JobIcon);
                            ImGui.TextUnformatted(PluginFont.GetJobGlyph(d.Job.Value));
                            ImGui.PopFont();
                            ImGui.PopStyleColor();

                            if (ImGui.IsItemHovered() && !string.IsNullOrWhiteSpace(d.ToolTip))
                            {
                                ImGui.BeginTooltip();
                                ImGui.SetTooltip(d.ToolTip);
                                ImGui.EndTooltip();
                            }
                        }
                        else
                        {
                            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (width - spacing * 2 - d.Width) / 2);
                            ImGui.PushStyleColor(ImGuiCol.Text, d.Color);
                            ImGui.TextUnformatted(d.Text);
                            ImGui.PopStyleColor();

                            if (ImGui.IsItemHovered() && !string.IsNullOrWhiteSpace(d.ToolTip))
                            {
                                ImGui.BeginTooltip();
                                ImGui.SetTooltip(d.ToolTip);
                                ImGui.EndTooltip();
                            }
                        }
                    }
                }
            }

            public unsafe void Update(FFlogsLog log)
            {
                log ??= FFlogsLog.Empty;

                this.jobCurrentRaid = GameJob.Best;
                this.jobCurrentUlti = GameJob.Best;

                this.updatedAtString = log == FFlogsLog.Empty ? "-" : log.UpdatedAtUtc.ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss \"기준\"");
                this.updatedAtWidth = ImGui.CalcTextSize(this.updatedAtString).X;

                //var containsEcho = this.plugin.PluginConfig.ApiContainsEcho;

                //var allstarData   = (containsEcho ? log.RaidAllstarNc : log.RaidAllstarEc);
                //var encounterData = (containsEcho ? log.EncountersNc : log.EncountersEc);
                var allstarData   = log.RaidAllstarNe;
                var encounterData = log.EncountersNe ;

                ////////////////////////////////////////////////////////////////////////////////////////////////////

                var c = *ImGui.GetStyleColorVec4(ImGuiCol.Text);

                var style = ImGui.GetStyle();
                var spacingX = style.ColumnsMinSpacing;

                ////////////////////////////////////////////////////////////////////////////////////////////////////

                // 전체 클리어 횟수 미리 계산하기
                var raidKills = new Dictionary<GameEncounter, int>();
                foreach (var (_, enc) in edenEncounters)
                {
                    raidKills[enc] =
                        encounterData
                        .Where(e =>e.Key.EncounterId == enc)
                        .Sum(e => e.Value.Kills);
                }
                raidKills[0] = raidKills.Sum(e => e.Value);

                this.colorRaid.Clear();
                this.colorUlti.Clear();

                foreach (var currentJob in Enum.GetValues<GameJob>())
                {
                    UpdateRaid(encounterData, allstarData, raidKills, currentJob);
                    this.drawDataRaid[currentJob].UpdateWidth(this, spacingX);

                    UpdateUlti(encounterData, currentJob);
                    this.drawDataUlti[currentJob].UpdateWidth(this, spacingX);
                }

                void UpdateRaid(
                    Dictionary<EncounterDataKey, EncounterData> encounterData,
                    Dictionary<GameJob, ZoneData> allstarData,
                    Dictionary<GameEncounter, int> raidKills,
                    GameJob currentJob)
                {
                    var drawData = this.drawDataRaid[currentJob];
                    drawData.TextData.Clear();

                    drawData.TextData.Add(new[] {
                        new Data("BEST"     , c),
                        new Data("JOB"      , c),
                        new Data("RANK"     , c, "직업 순위"),
                        new Data("AVG %"    , c, "최고점수 평균 %%"),
                        new Data("AllStar"  , c, "각 층당 1위 dps를 기준으로 120점 만점 점수로 환산한 점수"),
                        new Data("KILLs"    , c),
                    });
                    drawData.TextData.Add(null);

                    ////////////////////////////////////////////////////////////////////////////////////////////////////

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
                        var rankPercent = (float)(item.Total - item.Rank) / item.Total * 100;
                        var color = FFlogsColor.GetColor(rankPercent);
                        rankPercent = (float)(Math.Floor(rankPercent * 10) / 10);

                        this.colorRaid[targetJob.Value] = color;

                        var kills = encounterData
                            .Where(e => e.Key.EncounterId.IsRaids() && e.Key.JobId == targetJob)
                            .Sum(e => e.Value.Kills);

                        var bestAvg = encounterData
                            .Where(e => e.Key.EncounterId.IsRaids())
                            .GroupBy(e => e.Key.EncounterId)
                            .Average(e => e.Max(ee => ee.Value.MaxPer));

                        drawData.TextData.Add(new[]
                        {
                            new Data(string.Empty, c, string.Empty),
                            new Data(targetJob.Value),
                            new Data(item.Rank.ToString("#,##0"), color, $"{item.Rank:#,##0} / {item.Total:#,##0}\n상위 {rankPercent:##0.0} %%"),
                            new Data(bestAvg.ToString("#0.0"), color),
                            new Data(item.Point.ToString("##0.00"), color),
                            new Data(kills.ToString("#,##0"), c, $"{targetJob?.S()} : {kills}\n전체 : {raidKills[0]}"),
                        });
                    }
                    else
                    {
                        drawData.TextData.Add(new[]
                        {
                            new Data(string.Empty, c),
                            new Data(string.Empty, c),
                            new Data(string.Empty, c),
                            new Data(string.Empty, c),
                            new Data(string.Empty, c),
                            new Data(string.Empty, c),
                        });
                    }

                    drawData.TextData.Add(null);

                    ////////////////////////////////////////////////////////////////////////////////////////////////////

                    drawData.TextData.Add(new[] {
                        new Data("에덴 영식", c),
                        new Data("JOB"      , c),
                        new Data("rDPS"     , c),
                        new Data("BEST %"   , c),
                        new Data("MED %"    , c),
                        new Data("KILLs"    , c),
                    });
                    drawData.TextData.Add(null);

                    // Encounters
                    foreach (var (name, enc) in edenEncounters)
                    {
                        if (currentJob != GameJob.Best)
                        {
                            targetJob = currentJob;
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

                        if (targetJob.HasValue && encounterData.TryGetValue(key, out var encData))
                        {
                            var color = FFlogsColor.GetColor(encData.MaxPer);

                            drawData.TextData.Add(new[]
                            {
                                new Data(name, c),
                                new Data(targetJob.Value),
                                new Data(encData.MaxRdps.ToString("#,##0.0"), color),
                                new Data((Math.Floor(encData.MaxPer * 10) / 10).ToString("##0.0"), color),
                                new Data((Math.Floor(encData.MedPer * 10) / 10).ToString("##0.0"), FFlogsColor.GetColor(encData.MedPer)),
                                new Data(encData.Kills.ToString("#,##0"), c, $"{targetJob?.S()} : {encData.Kills}\n전체 : {raidKills[enc]}"),
                            });
                        }
                        else
                        {
                            drawData.TextData.Add(new[]
                            {
                                new Data(name, c),
                                new Data(string.Empty, c),
                                new Data(string.Empty, c),
                                new Data(string.Empty, c),
                                new Data(string.Empty, c),
                                new Data(string.Empty, c),
                            });
                        }
                    }
                    drawData.TextData.Add(null);
                }

                void UpdateUlti(
                    Dictionary<EncounterDataKey, EncounterData> encounterData,
                    GameJob currentJob)
                {
                    var drawData = this.drawDataUlti[currentJob];
                    drawData.TextData.Clear();

                    drawData.TextData.Add(new[] {
                        new Data("절 토벌전", c),
                        new Data("JOB"      , c),
                        new Data("rDPS"     , c),
                        new Data("BEST %"   , c),
                        new Data("MED %"    , c),
                        new Data("KILLs"    , c),
                    });
                    drawData.TextData.Add(null);

                    ////////////////////////////////////////////////////////////////////////////////////////////////////

                    // Encounters
                    GameJob? targetJob;
                    foreach (var (name, enc) in ultiEncounters)
                    {
                        if (currentJob != GameJob.Best)
                        {
                            targetJob = currentJob;
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

                        if (targetJob.HasValue && encounterData.TryGetValue(key, out var encData))
                        {
                            var color = FFlogsColor.GetColor(encData.MaxPer);
                            this.colorUlti[targetJob.Value] = color;

                            drawData.TextData.Add(new[]
                            {
                                new Data(name, c),
                                new Data(targetJob.Value),
                                new Data(encData.MaxRdps.ToString("#,##0.0"), color),
                                new Data((Math.Floor(encData.MaxPer * 10) / 10).ToString("##0.0"), color),
                                new Data((Math.Floor(encData.MedPer * 10) / 10).ToString("##0.0"), FFlogsColor.GetColor(encData.MedPer)),
                                new Data(encData.Kills.ToString("#,##0"), c),
                            });
                        }
                        else
                        {
                            drawData.TextData.Add(new[]
                            {
                                new Data(name, c),
                                new Data(string.Empty, c),
                                new Data(string.Empty, c),
                                new Data(string.Empty, c),
                                new Data(string.Empty, c),
                                new Data(string.Empty, c),
                            });
                        }
                    }
                }
            }
        }
    }
}
