using System;
using System.IO;
using System.Management;
using System.Reflection;
using AIMP.DiskCover.Core;
using AIMP.SDK;
using AIMP.SDK.Logger;

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

        public FileLoggerManager()
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _fileLog = Path.Combine(dir, "diskcover.log");

            if (!File.Exists(_fileLog))
            {
                Init();
            }
            else
            {
                _logFileStream = File.AppendText(_fileLog);
            }
        }

        public void Write(string message)
        {
            _logFileStream.WriteLine($"[{DateTime.Now:f}] {message}");
        }

        public void Write(Exception exception)
        {
            _logFileStream.WriteLine($"[{DateTime.Now:f}] {exception}");
        }

        public void Close()
        {
            _logFileStream.Flush();
            _logFileStream.Close();
        }

        private void Init()
        {
            _logFileStream = File.AppendText(_fileLog);

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

            _logFileStream.Flush();
        }
    }
}