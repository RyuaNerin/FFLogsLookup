using System;
using System.Threading.Tasks;
using ImGuiNET;

namespace FFLogsLookup.Gui
{
    internal class ConfigGui : BaseGui
    {
        public ConfigGui(Plugin plugin)
            : base(plugin, "FFLogsLookup: Config")
        {
        }

        private string clientId = "";
        private string clientSecret = "";

        private readonly object applyEnabledLock = new();
        private bool applyEnabled = true;

        private readonly object errorMessageLock = new();
        private string errorMessage = "";

        public override void Draw()
        {
            ImGui.PushItemWidth(-1);

            lock (this.plugin.PluginConfig)
            {
                ////////////////////////////////////////////////////////////////////////////////////////////////////

                ImGui.TextUnformatted("FFLogs Client Id");
                var c = this.clientId ?? "";
                if (ImGui.InputText("##api-key", ref c, 64))
                {
                    this.clientId = c;
                }

                ////////////////////////////////////////////////////////////////////////////////////////////////////

                ImGui.TextUnformatted("FFLogs Client Secret");
                var cc = this.clientSecret ?? "";
                if (ImGui.InputText("##api-client-secret", ref cc, 64))
                {
                    this.clientSecret = cc;
                }

                /*
                ////////////////////////////////////////////////////////////////////////////////////////////////////

                ImGui.Separator();

                ImGui.TextUnformatted("초월하는 힘 로그 포함");
                this.containsEcho = this.plugin.PluginConfig.ApiContainsEcho;
                if (ImGui.BeginCombo("##api-partition", ))
                {

                }
                */

                ////////////////////////////////////////////////////////////////////////////////////////////////////

                lock (applyEnabledLock)
                {
                    if (!this.applyEnabled)
                    {
                        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.2f);
                    }
                    var applied = ImGui.Button("적용") && this.applyEnabled;
                    if (!this.applyEnabled)
                    {
                        ImGui.PopStyleVar();
                    }

                    if (applied)
                    {
                        this.Authorize(this.clientId, this.clientSecret);
                    }
                }

                lock (this.errorMessageLock)
                {
                    ImGui.TextUnformatted(this.errorMessage ?? "");
                }
            }

            ImGui.PopItemWidth();
        }

        public void Authorize(string clientId, string clientSecret)
        {
            clientId = new string(clientId);
            clientSecret = new string(clientSecret);

            lock (this.applyEnabledLock)
            {
                this.applyEnabled = false;
            }

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await this.plugin.FFlogsClient.Authorize(clientId, clientSecret);

                    lock (this.errorMessageLock)
                    {
                        this.errorMessage = "FFLogs 키를 불러왔습니다.";
                    }

                    lock (this.plugin.PluginConfig)
                    {
                        this.clientId = clientId;
                        this.clientSecret = clientSecret;

                        this.plugin.PluginConfig.ApiClientId = clientId;
                        this.plugin.PluginConfig.ApiClientSecret = clientSecret;

                        this.plugin.PluginConfig.Save();
                    }
                }
                catch (Exception ex)
                {
                    lock (this.errorMessageLock)
                    {
                        this.errorMessage = (string)ex.Message.Clone();
                    }
                }
                finally
                {
                    lock (this.applyEnabledLock)
                    {
                        this.applyEnabled = true;
                    }
                }
            });
        }
    }
}
