using System;
using System.Drawing;
using AIMP.DiskCover.Infrastructure;
using AIMP.SDK.FileManager.Objects;

namespace AIMP.DiskCover.Interfaces
{
    public interface ICoverFinderManager
    {
        /// <summary>
        /// An event that is raised prior to starting loading a fresh cover image.
        /// </summary>
        event EventHandler BeginRequest;

        /// <summary>
        /// An event that is raised when cover finder finishes its work.
        /// </summary>
        event EndRequestHandler EndRequest;

        /// <summary>
        /// Starts loading a new cover image.
        /// The result will come in an event arguments.
        /// </summary>
        void FindCoverImageAsync(UIntPtr taskId);

        Bitmap FindCoverImage(IAimpFileInfo trackInfo);

        Bitmap FindCoverImage(string fileUrl, string artist, string album);
    }
}