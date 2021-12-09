using System.Numerics;
using FFLogsLookup.FFlogs;
using FFLogsLookup.Game;
using ImGuiNET;

namespace FFLogsLookup.Gui.Components
{
    internal class CellData
    {
        public CellData()
        {
        }
        public CellData(string value)
        {
            this.Value = value;
        }
        public CellData(GameJob gameJob)
        {
            this.SetByJob(gameJob);
        }

        public void SetByJob(GameJob gameJob)
        {
            this.Value   = gameJob.GetGlyph();
            this.Color   = gameJob.GetJobColor();
            this.FontPtr = PluginFont.JobIcon;
            this.ToolTip = gameJob.GetDescription();
        }

        private string value;
        public string Value
        {
            get => this.value ?? string.Empty;
            set
            {
                this.valueWidth = null;
                this.value = value;
            }
        }

        public string ToolTip { get; set; }

        public ImFontPtr? FontPtr { get; set; }
        public Vector4? Color { get; set; }

        private float? valueWidth;
        public float ValueWidth
        {
            get
            {
                if (!this.valueWidth.HasValue)
                {
                    if (this.FontPtr.HasValue) ImGui.PushFont(this.FontPtr.Value);
                    this.valueWidth = ImGui.CalcTextSize(this.Value).X;
                    if (this.FontPtr.HasValue) ImGui.PopFont();
                }

                return this.valueWidth.Value;
            }
        }

        public enum Align
        {
            Left,
            Center,
            Right,
        }

        private void Reallocation(float cellWidth, float padding, Align align)
        {
            switch (align)
            {
            case Align.Left  : ImGui.SetCursorPosX(ImGui.GetCursorPosX()                                              ); break;
            case Align.Center: ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (cellWidth - padding - this.ValueWidth) / 2); break;
            case Align.Right : ImGui.SetCursorPosX(ImGui.GetCursorPosX() +  cellWidth - padding - this.ValueWidth     ); break;
            };
        }

        public void Draw(float cellWidth, float padding, Align align)
        {
            //if (string.IsNullOrWhiteSpace(this.Value)) return;

            if (this.FontPtr.HasValue) ImGui.PushFont(this.FontPtr.Value);
            if (this.Color.HasValue) ImGui.PushStyleColor(ImGuiCol.Text, this.Color.Value);

            this.Reallocation(cellWidth, padding, align);
            ImGui.TextUnformatted(this.Value);

            if (this.Color.HasValue) ImGui.PopStyleColor();
            if (this.FontPtr.HasValue) ImGui.PopFont();

            if (ImGui.IsItemHovered() && !string.IsNullOrWhiteSpace(this.ToolTip))
            {
                ImGui.BeginTooltip();
                ImGui.SetTooltip(this.ToolTip);
                ImGui.EndTooltip();
            }
        }
    }
}
