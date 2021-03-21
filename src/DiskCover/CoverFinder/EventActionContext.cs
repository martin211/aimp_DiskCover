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
using System.Threading;

namespace AIMP.DiskCover.CoverFinder
{
    internal class EventActionContext<TEvent>
    {
        internal EventActionContext(Action<TEvent> eventAction, SynchronizationContext context)
        {
            EventAction = eventAction;
            Context = context;
        }

        public SynchronizationContext Context { get; }

        public Action<TEvent> EventAction { get; }
    }
}
