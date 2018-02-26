using System;
using System.Drawing;

namespace AIMP.DiskCover.Infrastructure
{
    public class FinderEvent : EventArgs
    {
        public FinderEvent(Bitmap cover)
        {
            CoverBitmap = cover;
        }

        public Bitmap CoverBitmap { get; private set; }
    }
}
