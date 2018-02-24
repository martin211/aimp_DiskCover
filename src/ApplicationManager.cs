using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIMP.DiskCover
{
    internal sealed class ApplicationManager
    {
        private static ApplicationManager _instance;
        public static ApplicationManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ApplicationManager();
                return _instance;
            }
        }
    }
}
