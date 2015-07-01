using System;
using System.Drawing;

namespace AIMP.DiskCover
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
