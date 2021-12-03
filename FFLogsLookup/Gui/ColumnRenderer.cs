using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;

namespace FFLogsLookup.Gui
{
    internal class ColumnRenderer
    {
        internal struct ColumnData
        {
            public bool Empty  { get; set; }
            public int  Row    { get; set; }
            public int  Column { get; set; }

            public ImFontPtr? Font    { get; set; }
            public Vector4?   Color   { get; set; }
            public string     Text    { get; set; }
            public string     ToolTip { get; set; }
            public float      Width   { get; set; }
        }

        public int Columns { get; set; }

        public readonly List<ColumnData> columnDatas = new();

        public float Width { get; private set; }
        public float[] ColumnWidth { get; private set; }

        private bool built = false;

        public void Clear()
        {
            this.columnDatas.Clear();
        }

        public void Add(int row, int column, ImFontPtr? font, string text, Vector4 color, string toolTip)
        {
            this.built = false;

            this.columnDatas.RemoveAll(e => e.Row == row && e.Column == column);
            this.columnDatas.Add(
                new ColumnData()
                {
                    Empty   = true   ,
                    Row     = row    ,
                    Column  = column ,
                    Color   = color  ,
                    Font    = font   ,
                    Text    = text   ,
                    ToolTip = toolTip,
                    Width   = -1,
                });
        }

        public void Build()
        {
            if (this.built) return;
            this.built = true;

            for (var i = 0; i < this.columnDatas.Count; i++)
            {
                var c = this.columnDatas[i];
                if (c.Width == -1)
                {
                    if (c.Font.HasValue) ImGui.PushFont(c.Font.Value);
                    c.Width = ImGui.CalcTextSize(c.Text).X;
                    if (c.Font.HasValue) ImGui.PopFont(); ;
                }

                this.columnDatas[i] = c;
            }

            var rows = this.columnDatas.Max(e => e.Row);
            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < this.Columns; column++)
                {
                    if (!this.columnDatas.Any(e => e.Row == row && e.Column == column))
                    {
                        this.columnDatas.Add(new ColumnData { Empty = true });
                    }
                }
            }

            this.columnDatas.Sort((a, b) => a.Row != b.Row ? a.Row.CompareTo(b.Row) : a.Column.CompareTo(b.Column));
        }
        public void Draw()
        {
            this.Build();

            ImGui.Columns(this.Columns);

            for (var column = 0; column < this.Columns; column++)
            {
                ImGui.SetColumnWidth(column, this.ColumnWidth[column]);
            }

            var first = true;
            for (var column = 0; column < this.Columns; column++)
            {
                var idx = column;
                while (idx < this.columnDatas.Count)
                {
                    if (first)
                    {
                        ImGui.NextColumn();
                        first = false;
                    }

                    var c = this.columnDatas[idx];

                    if (c.Font.HasValue) ImGui.PushFont(c.Font.Value);
                    if (c.Color.HasValue) ImGui.PushStyleColor(ImGuiCol.Text, c.Color.Value);

                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (this.ColumnWidth[column] - c.Width) / 2);
                    ImGui.TextUnformatted(c.Text);

                    if (c.Color.HasValue) ImGui.PopStyleColor();
                    if (c.Font.HasValue) ImGui.PopFont();

                    idx += this.Columns;
                }
            }

            ImGui.Columns();
        }
    }
}
