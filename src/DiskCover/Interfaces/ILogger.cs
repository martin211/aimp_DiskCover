using System;

namespace AIMP.DiskCover.Interfaces
{
    public interface ILogger
    {
        void Write(string message);

        void Write(Exception exception);

        void Close();
    }
}