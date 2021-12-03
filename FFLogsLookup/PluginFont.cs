using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Dalamud.Logging;
using FFLogsLookup.Game;
using ImGuiNET;

namespace FFLogsLookup
{
    internal class PluginFont : IDisposable
    {
        public ImFontPtr JobIcon { get; set; }

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

            this.JobIcon = ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPath, 17f, null, iconRangeHandle.AddrOfPinnedObject());

            iconRangeHandle.Free();
        }

        public static string GetJobGlyph(GameJob gameJob)
        {
            return gameJob switch
            {
                GameJob.Astrologian => "\uf033",
                GameJob.Bard        => "\uf023",
                GameJob.BlackMage   => "\uf025",
                GameJob.Dancer      => "\uf038",
                GameJob.DarkKnight  => "\uf032",
                GameJob.Dragoon     => "\uf022",
                GameJob.Gunbreaker  => "\uf037",
                GameJob.Machinist   => "\uf031",
                GameJob.Monk        => "\uf020",
                GameJob.Ninja       => "\uf030",
                GameJob.Paladin     => "\uf019",
                GameJob.RedMage     => "\uf035",
                GameJob.Samurai     => "\uf034",
                GameJob.Scholar     => "\uf028",
                GameJob.Summoner    => "\uf027",
                GameJob.Warrior     => "\uf021",
                GameJob.WhiteMage   => "\uf024",

                _ => null,
            };
        }
    }
}
