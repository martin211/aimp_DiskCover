// ----------------------------------------------------
// 
// AIMP DotNet SDK
// 
// Copyright (c) 2014 - 2020 Evgeniy Bogdan
// https://github.com/martin211/aimp_dotnet
// 
// Mail: mail4evgeniy@gmail.com
// 
// ----------------------------------------------------

using System;
using AIMP.SDK.MessageDispatcher;

namespace AIMP.DiskCover.Infrastructure.Events
{
    internal class CoreMessageEventArgs : EventArgs
    {
        public AimpCoreMessageType MessageType { get; }

        public int Param1 { get; }

        public IntPtr Param2 { get; }

        public CoreMessageEventArgs(AimpCoreMessageType messageType, int param1, IntPtr param2)
        {
            MessageType = messageType;
            Param1 = param1;
            Param2 = param2;
        }
    }
}
