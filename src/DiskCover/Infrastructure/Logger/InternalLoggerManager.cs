using System;
using AIMP.DiskCover.Interfaces;

namespace AIMP.DiskCover.Infrastructure.Logger
{
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
}