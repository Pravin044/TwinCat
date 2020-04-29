namespace TwinCAT.Ads.Internal
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Forms;

    internal class WindowsFormTimer : ITimer, IDisposable
    {
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        [CompilerGenerated]
        private EventHandler Tick;
        public bool inTimer;

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

        public WindowsFormTimer()
        {
            this.timer.add_Tick(new EventHandler(this.OnTick));
            this.inTimer = false;
        }

        public void Dispose()
        {
            this.timer.Dispose();
        }

        private void OnTick(object sender, EventArgs e)
        {
            if (!this.inTimer)
            {
                this.inTimer = true;
                this.Tick(this, new EventArgs());
                this.inTimer = false;
            }
        }

        public int Interval
        {
            get => 
                this.timer.Interval;
            set => 
                (this.timer.Interval = value);
        }

        public bool Enabled
        {
            get => 
                this.timer.Enabled;
            set => 
                (this.timer.Enabled = value);
        }
    }
}

