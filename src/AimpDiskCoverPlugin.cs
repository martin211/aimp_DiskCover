using System;
using System.IO;
using System.Management;
using AIMP.DiskCover.Infrastructure;
using AIMP.DiskCover.Interfaces;
using AIMP.SDK;
using AIMP.SDK.Logger;
using AIMP.SDK.Player;

namespace AIMP.DiskCover
{
    [AimpPlugin(PluginName, "Evgeniy Bogdan, Roman Nikitin", Version)]
// ReSharper disable ClassNeverInstantiated.Global
    public class AimpDiskCoverPlugin : AimpPlugin
// ReSharper restore ClassNeverInstantiated.Global
    {
        public const String PluginName = "AIMP Disc Cover";
        public const String Version = "2.0.0";

        private IDiskCoverPlugin _plugin;

        public override void Initialize()
        {
            UnityConfig.Iitialize(Player);
            _plugin = DependencyResolver.Current.ResolveService<IDiskCoverPlugin>();
            _plugin.Initialize();
        }

        public override void Dispose()
        {
           _plugin.Dispose();
        }
    }

    internal class InternalLoggerManager : ILogger
    {
        public void Write(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Write(Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception.ToString());
        }

        public void Close()
        {
        }
    }

    internal class FileLoggerManager : ILogger
    {
        private readonly string _fileLog;
        private StreamWriter _logFileStream;
        private readonly IPluginSettings _pluginSettings;
        private readonly IAimpPlayer _player;

        public FileLoggerManager(
            IAimpPlayer aimpPlayer,
            IPluginSettings pluginSettings,
            IPluginEvents pluginEvents)
        {
            _pluginSettings = pluginSettings;
            _player = aimpPlayer;
            pluginEvents.SaveConfig += PluginEvents_SaveConfig;

            var dir = _player.Core.GetPath(AimpMessages.AimpCorePathType.AIMP_CORE_PATH_PROFILE);
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
                _logFileStream.WriteLine($"[{DateTime.Now:dd.MM.yy HH:mm:ss:ff}] {message}");

                _logFileStream.Flush();
            }
        }

        public void Write(Exception exception)
        {
            if (_pluginSettings.DebugMode)
            {
                _logFileStream.WriteLine($"[{DateTime.Now:dd.MM.yy HH:mm:ss:ff}] {exception}");
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