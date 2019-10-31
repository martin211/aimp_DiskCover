using System;
using System.IO;
using System.Management;
using AIMP.DiskCover.Interfaces;
using AIMP.SDK.MessageDispatcher;
using AIMP.SDK.Player;

namespace AIMP.DiskCover.Infrastructure.Logger
{
    internal class FileLoggerManager : ILogger
    {
        private readonly string _fileLog;
        private StreamWriter _logFileStream;
        private readonly IPluginSettings _pluginSettings;

        public FileLoggerManager(
            IAimpPlayer aimpPlayer,
            IPluginSettings pluginSettings,
            IPluginEvents pluginEvents)
        {
            _pluginSettings = pluginSettings;
            pluginEvents.SaveConfig += PluginEvents_SaveConfig;

            var dir = aimpPlayer.Core.GetPath(AimpCorePathType.AIMP_CORE_PATH_PROFILE);
            _fileLog = Path.Combine(dir, "diskcover.log");

            if (pluginSettings.DebugMode)
            {
                Init();
            }
        }

        private void PluginEvents_SaveConfig(object sender, EventArgs e)
        {
            if (_pluginSettings.DebugMode && _logFileStream == null)
            {
                Init();
            }
        }

        public void Write(string message)
        {
            if (_pluginSettings.DebugMode)
            {
                _logFileStream.WriteLine($"[{DateTime.Now:f}] {message}");
                _logFileStream.Flush();
            }
        }

        public void Write(Exception exception)
        {
            if (_pluginSettings.DebugMode)
            {
                _logFileStream.WriteLine($"[{DateTime.Now:f}] {exception}");
                _logFileStream.Flush();
            }
        }

        public void Close()
        {
            if (_pluginSettings.DebugMode)
            {
                _logFileStream.Flush();
                _logFileStream.Close();
            }
        }

        private void Init()
        {
            var writeOsInfo = !File.Exists(_fileLog);
            _logFileStream = File.AppendText(_fileLog);

            if (writeOsInfo)
            {
                var mos = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
                foreach (var managementObject in mos.Get())
                {
                    if (managementObject["Caption"] != null)
                    {
                        _logFileStream.WriteLine($"Operating System Name: {managementObject["Caption"]}");
                    }

                    if (managementObject["OSArchitecture"] != null)
                    {
                        _logFileStream.WriteLine($"Operating System Architecture: {managementObject["OSArchitecture"]}");
                    }

                    if (managementObject["CSDVersion"] != null)
                    {
                        _logFileStream.WriteLine($"Operating System Service Pack: {managementObject["CSDVersion"]}");
                    }
                }

                _logFileStream.WriteLine(Environment.NewLine);
                _logFileStream.Flush();
            }
        }
    }
}