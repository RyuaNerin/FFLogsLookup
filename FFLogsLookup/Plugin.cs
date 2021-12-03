using System;
using System.Runtime.InteropServices;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFLogsLookup.FFlogs;
using FFLogsLookup.Gui;
using ImGuiNET;
using XivCommon;

namespace FFLogsLookup
{
    public class Plugin : IDalamudPlugin, IDisposable
    {
        public string Name => "FFLogsLookup";

        internal PluginConfig PluginConfig { get; }
        internal FFlogsClient FFlogsClient { get; }

        private readonly PluginCommand pluginCommand;
        private readonly PluginContextMenu pluginContextMenu;

        //////////////////////////////////////////////////

        internal ConfigGui WindowConfig { get; }
        internal DetailGui WindowDetail { get; }

        internal XivCommonBase XivCommon { get; }

        internal PluginFont PluginFont { get; }

        private readonly WindowSystem windowSystem = new("FFLogsLookup");

        public Plugin(DalamudPluginInterface pluginInterface)
        {
            try
            {
                DalamudInstance.Initialize(pluginInterface);

                this.XivCommon = new XivCommonBase(Hooks.ContextMenu | Hooks.PartyFinderJoins | Hooks.PartyFinder);
                this.pluginContextMenu = new PluginContextMenu(this);

                this.pluginCommand = new PluginCommand(this);

                //////////////////////////////////////////////////
                
                this.PluginFont = new PluginFont();

                //////////////////////////////////////////////////

                this.PluginConfig = PluginConfig.Load(pluginInterface);

                this.FFlogsClient = new FFlogsClient();

                this.WindowConfig = new ConfigGui(this);
                this.WindowDetail = new DetailGui(this);

                this.windowSystem.AddWindow(this.WindowConfig);
                this.windowSystem.AddWindow(this.WindowDetail);
                DalamudInstance.PluginInterface.UiBuilder.OpenConfigUi += this.UiBuilder_OpenConfigUi;
                DalamudInstance.PluginInterface.UiBuilder.Draw += this.UiBuilder_Draw;

                //////////////////////////////////////////////////

                this.WindowConfig.Authorize(this.PluginConfig.ApiClientId, this.PluginConfig.ApiClientSecret);
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "FFLogsLookup .Ctor");
                this.Dispose(true);

                throw;
            }
        }
        ~Plugin()
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
                this.PluginConfig?.Save();

                DalamudInstance.PluginInterface.UiBuilder.Draw -= this.UiBuilder_Draw;
                DalamudInstance.PluginInterface.UiBuilder.OpenConfigUi -= this.UiBuilder_OpenConfigUi;
                this.windowSystem.RemoveAllWindows();

                this.WindowConfig?.Dispose();
                this.WindowDetail?.Dispose();

                this.FFlogsClient?.Dispose();

                //////////////////////////////////////////////////

                this.PluginFont?.Dispose();

                //////////////////////////////////////////////////

                this.pluginCommand?.Dispose();

                this.pluginContextMenu?.Dispose();
                this.XivCommon?.Dispose();
            }
        }

        private void UiBuilder_OpenConfigUi()
        {
            this.WindowConfig.IsOpen = true;
        }
        private void UiBuilder_Draw()
        {
            try
            {
                this.windowSystem.Draw();
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, $"{nameof(Plugin)}.{nameof(UiBuilder_Draw)}");
            }
        }
    }
}
