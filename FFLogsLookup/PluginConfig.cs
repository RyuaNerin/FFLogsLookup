using Dalamud.Configuration;
using Dalamud.Plugin;

namespace FFLogsLookup
{
    internal class PluginConfig : IPluginConfiguration
    {
        public int Version { get; set; } = 1;

        public static PluginConfig Load(DalamudPluginInterface pluginInterface)
        {
            if (pluginInterface.GetPluginConfig() is PluginConfig config)
            {
                config = Migrate(config);
                return config;
            }

            config = new PluginConfig();
            config.Save();
            return config;
        }

        public static PluginConfig Migrate(PluginConfig config)
        {
            return config;
        }

        public void Save()
        {
            DalamudInstance.PluginInterface.SavePluginConfig(this);
        }

        public string ApiClientId     { get; set; }
        public string ApiClientSecret { get; set; }
        public bool   ApiContainsEcho { get; set; }
    }
}
