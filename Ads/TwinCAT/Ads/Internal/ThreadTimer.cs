namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class ThreadTimer : ITimer, IDisposable
    {
        private readonly object padlock = new object();
        private bool bEnabled;
        private Timer timer;
        private TimerCallback timerDelegate = new TimerCallback(ThreadTimer.CheckStatus);
        private TimerState state = new TimerState();
        private int interval = 100;
        private bool delay = true;
        private bool inTimer;
        [CompilerGenerated]
        private EventHandler Tick;

        public event EventHandler Tick
        {
            [CompilerGenerated] add
            {
                EventHandler tick = this.Tick;
                while (true)
                {
                    EventHandler a = tick;
                    EventHandler handler3 = (EventHandler) Delegate.Combine(a, value);
                    tick = Interlocked.CompareExchange<EventHandler>(ref this.Tick, handler3, a);
                    if (ReferenceEquals(tick, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler tick = this.Tick;
                while (true)
                {
                    EventHandler source = tick;
                    EventHandler handler3 = (EventHandler) Delegate.Remove(source, value);
                    tick = Interlocked.CompareExchange<EventHandler>(ref this.Tick, handler3, source);
                    if (ReferenceEquals(tick, source))
                    {
                        return;
                    }
                }
            }
        }

        public ThreadTimer()
        {
            this.timer = new Timer(this.timerDelegate, this.state, -1, 0);
            this.state.timer = this;
        }

        private static void CheckStatus(object state)
        {
            ((TimerState) state).timer.OnElapsed();
        }

        public void Dispose()
        {
            this.timer.Dispose();
        }

        protected void OnElapsed()
        {
            if (this.bEnabled && !this.inTimer)
            {
                object padlock = this.padlock;
                lock (padlock)
                {
                    this.inTimer = true;
                    this.Tick(this, new EventArgs());
                    this.inTimer = false;
                }
            }
        }

        public int Interval
        {
            get => 
                this.interval;
            set => 
                (this.interval = value);
        }

        public bool Enabled
        {
            get => 
                this.bEnabled;
            set
            {
                this.bEnabled = value;
                if (!this.bEnabled)
                {
                    this.timer.Change(-1, -1);
                }
                else if (this.delay)
                {
                    this.timer.Change(this.interval, this.interval);
                }
                else
                {
                    this.timer.Change(0, this.interval);
                }
            }
        }

        public bool Delay
        {
            get => 
                this.delay;
            set => 
                (this.delay = value);
        }

        private class TimerState
        {
            public ThreadTimer timer;
        }
    }
}

