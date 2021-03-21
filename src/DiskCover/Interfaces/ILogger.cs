using System;

namespace AIMP.DiskCover.Interfaces
{
    public interface ILogger
    {
        void Write(string operation, string module, object obj);

        void Write(string message);

        void Write(Exception exception);

        void Close();
    }
}