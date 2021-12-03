using System;
using System.Diagnostics;
using System.Linq;
using Dalamud.Logging;
using FFLogsLookup.Game;
using Lumina.Excel.GeneratedSheets;
using XivCommon.Functions.ContextMenu;

namespace FFLogsLookup
{
    internal class PluginContextMenu : IDisposable
    {
        private readonly Plugin plugin;

        public PluginContextMenu(Plugin plugin)
        {
            this.plugin = plugin;

            this.plugin.XivCommon.Functions.ContextMenu.OpenContextMenu += this.ContextMenu_OpenContextMenu;
        }

        ~PluginContextMenu()
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
                this.plugin.XivCommon.Functions.ContextMenu.OpenContextMenu -= this.ContextMenu_OpenContextMenu;
            }
        }

        private void ContextMenu_OpenContextMenu(ContextMenuOpenArgs args)
        {
            if (args.ObjectId != 0 && args.ObjectWorld != 0)
            {
                args.Items.Add(new NormalContextMenuItem("FFLogs 점수 보기", this.Search));
                args.Items.Add(new NormalContextMenuItem("FFLogs 홈페이지 열기", this.OpenWebSite));
            }
        }

        private void Search(ContextMenuItemSelectedArgs args)
        {
            try
            {
                if (!IsMenuValid(args))
                    return;

                var world = DalamudInstance.DataManager.GetExcelSheet<World>()?.FirstOrDefault(x => x.RowId == args.ObjectWorld);
                if (world == null)
                    return;

                this.plugin.WindowDetail.Update(args.Text.TextValue, GameData.GetGameServer(world.Name));
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "PluginContextMenu.Search");
            }
        }

        private void OpenWebSite(ContextMenuItemSelectedArgs args)
        {
            try
            {
                if (!IsMenuValid(args))
                    return;

                var world = DalamudInstance.DataManager.GetExcelSheet<World>()?.FirstOrDefault(x => x.RowId == args.ObjectWorld);
                if (world == null)
                    return;

                Process.Start(new ProcessStartInfo()
                {
                    FileName = $"https://ko.fflogs.com/character/kr/{GameData.GetGameServer(world.Name)}/{Uri.EscapeUriString(args.Text.TextValue)}",
                    UseShellExecute = true,
                });
            }
            catch
            {
            }
        }

        private static bool IsMenuValid(BaseContextMenuArgs args)
        {
            switch (args.ParentAddonName)
            {
            case null: // Nameplate/Model menu
            case "LookingForGroup":
            case "PartyMemberList":
            case "FriendList":
            case "FreeCompany":
            case "SocialList":
            case "ContactList":
            case "ChatLog":
            case "_PartyList":
            case "LinkShell":
            case "CrossWorldLinkshell":
            case "ContentMemberList": // Eureka/Bozja/...
                return args.Text != null && args.ObjectWorld != 0 && args.ObjectWorld != 65535;

            default:
                return false;
            }
        }
    }
}
