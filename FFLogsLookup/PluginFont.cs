using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace FFLogsLookup
{
    internal class PluginFont : IDisposable
    {
        public static ImFontPtr JobIcon { get; private set; }

        public unsafe PluginFont()
        {
            DalamudInstance.PluginInterface.UiBuilder.BuildFonts += this.UiBuilder_BuildFonts;
            DalamudInstance.PluginInterface.UiBuilder.RebuildFonts();
        }
        ~PluginFont()
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
                DalamudInstance.PluginInterface.UiBuilder.BuildFonts -= this.UiBuilder_BuildFonts;
                DalamudInstance.PluginInterface.UiBuilder.RebuildFonts();
            }
        }

        private void UiBuilder_BuildFonts()
        {
            var iconRangeHandle = GCHandle.Alloc(
                new ushort[]
                {
                    0xF019,
                    0xF038,
                    0,
                },
                GCHandleType.Pinned);

            var fontPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "FFXIVAppIcons.otf");

            JobIcon = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, 17f, null, iconRangeHandle.AddrOfPinnedObject());

            iconRangeHandle.Free();
        }
    }
}
