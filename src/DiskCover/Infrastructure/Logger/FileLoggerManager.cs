using System;
using System.IO;
using System.Management;
using AIMP.DiskCover.Infrastructure.Events;
using AIMP.DiskCover.Interfaces;
using AIMP.SDK;
using AIMP.SDK.MessageDispatcher;

namespace AIMP.DiskCover.Infrastructure.Logger
{
    internal class FileLoggerManager : ILogger
    {
        private readonly string _fileLog;
        private StreamWriter _logFileStream;
        private readonly IPluginSettings _pluginSettings;
        private IAimpPlayer _player;

        public FileLoggerManager(IAimpPlayer aimpPlayer, IPluginSettings pluginSettings, IEventAggregator aggregator)
        {
            _pluginSettings = pluginSettings;
            aggregator.Register<SaveConfigEventArgs>(PluginEvents_SaveConfig);
            _player = aimpPlayer;

            var dir = aimpPlayer.Core.GetPath(AimpCorePathType.Profile);
            _fileLog = Path.Combine(dir, "diskcover.log");

            if (pluginSettings.DebugMode)
            {
                Init();
            }
        }

        private void PluginEvents_SaveConfig(SaveConfigEventArgs e)
        {
            if (_pluginSettings.DebugMode && _logFileStream == null)
            {
                Init();
            }
        }

        public void Write(string operation, string module, object obj)
        {
            if (_pluginSettings.DebugMode)
            {
                //Write($"{operation} [{module}]: {JsonConvert.SerializeObject(obj)}");
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
        }
    }
}