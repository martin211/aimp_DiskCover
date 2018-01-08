using System;

namespace AIMP.DiskCover.Core
{
    public interface IPluginEvents
    {
        event EventHandler ConfigUpdated;

        event EventHandler SaveConfig;
    }

    public interface IPluginEventsExecutor
    {
        void OnConfigUpdated();

        void OnSaveConfig();
    }

    public class PluginEvents : IPluginEvents, IPluginEventsExecutor
    {
        public event EventHandler ConfigUpdated;
        public event EventHandler SaveConfig;

        public void OnConfigUpdated()
        {
            ConfigUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void OnSaveConfig()
        {
            SaveConfig?.Invoke(this, EventArgs.Empty);
        }
    }
}