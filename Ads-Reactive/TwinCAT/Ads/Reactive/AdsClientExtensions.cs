namespace TwinCAT.Ads.Reactive
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;
    using TwinCAT.ValueAccess;

    public static class AdsClientExtensions
    {
        public static IObservable<AdsState> PollAdsState(this IAdsConnection client, IObservable<Unit> trigger)
        {
            Func<Unit, AdsState> func = delegate (Unit o) {
                StateInfo info;
                client.TryReadState(out info);
                return info.AdsState;
            };
            return Observable.Select<Unit, AdsState>(trigger, func);
        }

        public static IObservable<AdsState> PollAdsState(this IAdsConnection client, TimeSpan period)
        {
            long[] numArray1 = new long[] { -1L };
            return client.PollAdsState(Observable.Select<long, Unit>(Observable.StartWith<long>(Observable.Interval(period), numArray1), e => Unit.get_Default()));
        }

        public static IObservable<AdsState> WhenAdsStateChanges(this TcAdsClient client) => 
            Observable.Select<EventPattern<AdsStateChangedEventArgs>, AdsState>(Observable.FromEventPattern<AdsStateChangedEventHandler, AdsStateChangedEventArgs>(delegate (AdsStateChangedEventHandler h) {
                client.AdsStateChanged += h;
            }, delegate (AdsStateChangedEventHandler h) {
                client.AdsStateChanged -= h;
            }), ev => ev.get_EventArgs().State.AdsState);

        public static IObservable<Notification> WhenNotification(this IAdsConnection client, ISymbol symbol) => 
            client.WhenNotification(symbol, NotificationSettings.Default);

        public static IObservable<SymbolNotification> WhenNotification(this IAdsConnection client, ISymbolCollection symbols) => 
            client.WhenNotification(symbols, NotificationSettings.Default);

        public static IObservable<SymbolNotification> WhenNotification(this IAdsConnection client, ISymbol symbol, NotificationSettings settings)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException("symbol");
            }
            if (symbol == null)
            {
                throw new ArgumentOutOfRangeException("Symbol is not an IValueSymbol", "symbol");
            }
            List<ISymbol> symbols = new List<ISymbol> {
                symbol
            };
            IDisposableSymbolHandleBag bag = null;
            IAccessorValueFactory fac = ((IAccessorValue) (symbol as IValueSymbol).ValueAccessor).ValueFactory;
            object userData = new object();
            return Observable.Select<EventPattern<AdsNotificationEventArgs>, SymbolNotification>(Observable.Where<EventPattern<AdsNotificationEventArgs>>(Observable.FromEventPattern<AdsNotificationEventHandler, AdsNotificationEventArgs>(delegate (AdsNotificationEventHandler h) {
                client.AdsNotification += h;
                bag = new DisposableNotificationHandleBag(client, symbols, settings, userData);
            }, delegate (AdsNotificationEventHandler h) {
                bag.Dispose();
                bag = null;
                client.AdsNotification -= h;
            }), ev => bag.Contains((uint) ev.get_EventArgs().NotificationHandle)), ev => new SymbolNotification(ev.get_EventArgs(), bag.GetSymbol((uint) ev.get_EventArgs().NotificationHandle), fac));
        }

        public static IObservable<SymbolNotification> WhenNotification(this IAdsConnection client, ISymbolCollection symbols, NotificationSettings settings)
        {
            if (symbols == null)
            {
                throw new ArgumentNullException("symbols");
            }
            ISymbol local1 = Enumerable.FirstOrDefault<ISymbol>(symbols);
            if (local1 == null)
            {
                throw new ArgumentOutOfRangeException("Symbols list is empty!", "symbols");
            }
            IValueSymbol symbol = local1 as IValueSymbol;
            if (symbol == null)
            {
                throw new ArgumentOutOfRangeException("Symbols in list are not IValueSymbol", "symbols");
            }
            IAccessorValueFactory valueFactory = symbol.ValueAccessor.ValueFactory;
            IDisposableSymbolHandleBag bag = null;
            object userData = new object();
            return Observable.Where<SymbolNotification>(Observable.Select<EventPattern<AdsNotificationEventArgs>, SymbolNotification>(Observable.Where<EventPattern<AdsNotificationEventArgs>>(Observable.FromEventPattern<AdsNotificationEventHandler, AdsNotificationEventArgs>(delegate (AdsNotificationEventHandler h) {
                client.AdsNotification += h;
                bag = new DisposableNotificationHandleBag(client, symbols, settings, userData);
            }, delegate (AdsNotificationEventHandler h) {
                bag.Dispose();
                bag = null;
                client.AdsNotification -= h;
            }), ev => bag.Contains((uint) ev.get_EventArgs().NotificationHandle)), ev => new SymbolNotification(ev.get_EventArgs(), bag.GetSymbol((uint) ev.get_EventArgs().NotificationHandle), valueFactory)), s => symbols.Contains(s.Symbol));
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public static IObservable<NotificationEx> WhenNotificationEx(this IAdsConnection client, IDictionary<string, AnyTypeSpecifier> symbols, NotificationSettings settings, object userData)
        {
            if (symbols == null)
            {
                throw new ArgumentNullException("symbols");
            }
            if (symbols.Count == 0)
            {
                throw new ArgumentOutOfRangeException("Symbols list is empty!", "symbols");
            }
            IDisposableHandleBag bag = null;
            return Observable.Select<EventPattern<AdsNotificationExEventArgs>, NotificationEx>(Observable.Where<EventPattern<AdsNotificationExEventArgs>>(Observable.FromEventPattern<AdsNotificationExEventHandler, AdsNotificationExEventArgs>(delegate (AdsNotificationExEventHandler h) {
                client.AdsNotificationEx += h;
                bag = new DisposableNotificationExHandleBag(client, symbols, settings, userData);
            }, delegate (AdsNotificationExEventHandler h) {
                bag.Dispose();
                bag = null;
                client.AdsNotificationEx -= h;
            }), ev => bag.Contains((uint) ev.get_EventArgs().NotificationHandle)), ev => new NotificationEx(ev.get_EventArgs()));
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public static IObservable<Notification> WhenRegisteredNotification(this IAdsNotifications client) => 
            Observable.Select<EventPattern<AdsNotificationEventArgs>, Notification>(Observable.FromEventPattern<AdsNotificationEventHandler, AdsNotificationEventArgs>(delegate (AdsNotificationEventHandler h) {
                client.AdsNotification += h;
            }, delegate (AdsNotificationEventHandler h) {
                client.AdsNotification -= h;
            }), ev => new Notification(ev.get_EventArgs()));

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public static IObservable<Notification> WhenRegisteredNotification(this IAdsNotifications client, uint[] handles) => 
            Observable.Select<EventPattern<AdsNotificationEventArgs>, Notification>(Observable.Where<EventPattern<AdsNotificationEventArgs>>(Observable.FromEventPattern<AdsNotificationEventHandler, AdsNotificationEventArgs>(delegate (AdsNotificationEventHandler h) {
                client.AdsNotification += h;
            }, delegate (AdsNotificationEventHandler h) {
                client.AdsNotification -= h;
            }), ev => Enumerable.Contains<uint>(handles, (uint) ev.get_EventArgs().NotificationHandle)), ev => new Notification(ev.get_EventArgs()));

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public static IObservable<AdsNotificationExEventArgs> WhenRegisteredNotificationEx(this IAdsNotifications client) => 
            Observable.Select<EventPattern<AdsNotificationExEventArgs>, AdsNotificationExEventArgs>(Observable.FromEventPattern<AdsNotificationExEventHandler, AdsNotificationExEventArgs>(delegate (AdsNotificationExEventHandler h) {
                client.AdsNotificationEx += h;
            }, delegate (AdsNotificationExEventHandler h) {
                client.AdsNotificationEx -= h;
            }), ev => ev.get_EventArgs());

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly AdsClientExtensions.<>c <>9 = new AdsClientExtensions.<>c();
            public static Func<EventPattern<AdsNotificationExEventArgs>, AdsNotificationExEventArgs> <>9__0_2;
            public static Func<EventPattern<AdsNotificationEventArgs>, Notification> <>9__1_2;
            public static Func<EventPattern<AdsNotificationEventArgs>, Notification> <>9__2_3;
            public static Func<EventPattern<AdsStateChangedEventArgs>, AdsState> <>9__3_2;
            public static Func<long, Unit> <>9__5_0;
            public static Func<EventPattern<AdsNotificationExEventArgs>, NotificationEx> <>9__9_3;

            internal Unit <PollAdsState>b__5_0(long e) => 
                Unit.get_Default();

            internal AdsState <WhenAdsStateChanges>b__3_2(EventPattern<AdsStateChangedEventArgs> ev) => 
                ev.get_EventArgs().State.AdsState;

            internal NotificationEx <WhenNotificationEx>b__9_3(EventPattern<AdsNotificationExEventArgs> ev) => 
                new NotificationEx(ev.get_EventArgs());

            internal Notification <WhenRegisteredNotification>b__1_2(EventPattern<AdsNotificationEventArgs> ev) => 
                new Notification(ev.get_EventArgs());

            internal Notification <WhenRegisteredNotification>b__2_3(EventPattern<AdsNotificationEventArgs> ev) => 
                new Notification(ev.get_EventArgs());

            internal AdsNotificationExEventArgs <WhenRegisteredNotificationEx>b__0_2(EventPattern<AdsNotificationExEventArgs> ev) => 
                ev.get_EventArgs();
        }
    }
}

