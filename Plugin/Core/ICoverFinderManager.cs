using System;

namespace AIMP.DiskCover
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
        event EventHandler<FinderEvent> EndRequest;

        /// <summary>
        /// Starts loading a new cover image.
        /// The result will come in an event arguments.
        /// </summary>
        void StartLoadingBitmap();
    }
}