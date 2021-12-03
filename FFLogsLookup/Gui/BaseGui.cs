using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace FFLogsLookup.Gui
{
    internal abstract class BaseGui : Window, IDisposable
    {
        protected readonly Plugin plugin;

        protected BaseGui(Plugin plugin, string name) : base(name)
        {
            this.plugin = plugin;

            this.Size = new Vector2(-1, -1);
            this.SizeCondition = ImGuiCond.Appearing;
        }
        ~BaseGui()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
