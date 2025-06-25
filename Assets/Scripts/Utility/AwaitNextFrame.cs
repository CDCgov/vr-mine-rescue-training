using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Threading;

public struct AwaitNextFrame
{
    public AwaitNextFrameAwaiter GetAwaiter()
    {
        return new AwaitNextFrameAwaiter();
    }

    public struct AwaitNextFrameAwaiter : ICriticalNotifyCompletion
    {
        public bool IsCompleted
        {
            get { return false; }
        }

        public void OnCompleted(Action continuation)
        {
            if (continuation == null)
                return;

            if (SynchronizationContext.Current == null)
                return;

            SynchronizationContext.Current.Post(SendOrPostCallback, continuation);
        }

        private static void SendOrPostCallback(object state)
        {
            ((Action)state)();
        }

        public void GetResult() { }

        public void UnsafeOnCompleted(Action continuation)
        {
            if (continuation == null)
                return;

            if (SynchronizationContext.Current == null)
                return;

            SynchronizationContext.Current.Post(SendOrPostCallback, continuation);
        }
    }

}
