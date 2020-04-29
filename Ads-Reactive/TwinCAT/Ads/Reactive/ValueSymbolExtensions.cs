namespace TwinCAT.Ads.Reactive
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using TwinCAT.Ads;
    using TwinCAT.TypeSystem;

    public static class ValueSymbolExtensions
    {
        public static IObservable<object> PollValues(this IValueSymbol symbol, IObservable<Unit> trigger)
        {
            Func<Unit, object> func = o => symbol.ReadValue();
            return Observable.Select<Unit, object>(trigger, func);
        }

        public static IObservable<object> PollValues(this IValueSymbol symbol, TimeSpan period)
        {
            long[] numArray1 = new long[] { -1L };
            return symbol.PollValues(Observable.Select<long, Unit>(Observable.StartWith<long>(Observable.Interval(period), numArray1), e => Unit.get_Default()));
        }

        public static IObservable<ValueChangedArgs> PollValuesAnnotated(this IValueSymbol symbol, IObservable<Unit> trigger) => 
            Observable.Select<object, ValueChangedArgs>(symbol.PollValues(trigger), o => new ValueChangedArgs(symbol, o, DateTime.UtcNow));

        public static IObservable<ValueChangedArgs> PollValuesAnnotated(this IValueSymbol symbol, TimeSpan period) => 
            Observable.Select<object, ValueChangedArgs>(symbol.PollValues(period), o => new ValueChangedArgs(symbol, o, DateTime.UtcNow));

        public static IObservable<object> WhenValueChanged(this IValueSymbol symbol) => 
            Observable.Select<EventPattern<ValueChangedArgs>, object>(Observable.FromEventPattern<EventHandler<ValueChangedArgs>, ValueChangedArgs>(delegate (EventHandler<ValueChangedArgs> h) {
                symbol.ValueChanged += h;
            }, delegate (EventHandler<ValueChangedArgs> h) {
                symbol.ValueChanged -= h;
            }), ev => ev.get_EventArgs().Value);

        public static IObservable<object> WhenValueChanged(this IAdsConnection connection, IEnumerable<ISymbol> symbols) => 
            Observable.Select<EventPattern<ValueChangedArgs>, object>(Observable.FromEventPattern<EventHandler<ValueChangedArgs>, ValueChangedArgs>(delegate (EventHandler<ValueChangedArgs> h) {
                using (IEnumerator<ISymbol> enumerator = symbols.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ((IValueSymbol) enumerator.Current).ValueChanged += h;
                    }
                }
            }, delegate (EventHandler<ValueChangedArgs> h) {
                using (IEnumerator<ISymbol> enumerator = symbols.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ((IValueSymbol) enumerator.Current).ValueChanged -= h;
                    }
                }
            }), ev => ev.get_EventArgs().Value);

        public static IObservable<ValueChangedArgs> WhenValueChangedAnnotated(this IValueSymbol symbol) => 
            Observable.Select<EventPattern<ValueChangedArgs>, ValueChangedArgs>(Observable.FromEventPattern<EventHandler<ValueChangedArgs>, ValueChangedArgs>(delegate (EventHandler<ValueChangedArgs> h) {
                symbol.ValueChanged += h;
            }, delegate (EventHandler<ValueChangedArgs> h) {
                symbol.ValueChanged -= h;
            }), ev => ev.get_EventArgs());

        public static IObservable<ValueChangedArgs> WhenValueChangedAnnotated(this IAdsConnection connection, IEnumerable<ISymbol> symbols) => 
            Observable.Select<EventPattern<ValueChangedArgs>, ValueChangedArgs>(Observable.FromEventPattern<EventHandler<ValueChangedArgs>, ValueChangedArgs>(delegate (EventHandler<ValueChangedArgs> h) {
                using (IEnumerator<ISymbol> enumerator = symbols.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ((IValueSymbol) enumerator.Current).ValueChanged += h;
                    }
                }
            }, delegate (EventHandler<ValueChangedArgs> h) {
                using (IEnumerator<ISymbol> enumerator = symbols.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        ((IValueSymbol) enumerator.Current).ValueChanged -= h;
                    }
                }
            }), ev => ev.get_EventArgs());

        public static IObservable<ValueChangedArgs> WhenValueChangedAnnotated(this IAdsConnection connection, IValueSymbol symbol) => 
            Observable.Select<EventPattern<ValueChangedArgs>, ValueChangedArgs>(Observable.FromEventPattern<EventHandler<ValueChangedArgs>, ValueChangedArgs>(delegate (EventHandler<ValueChangedArgs> h) {
                symbol.ValueChanged += h;
            }, delegate (EventHandler<ValueChangedArgs> h) {
                symbol.ValueChanged -= h;
            }), ev => ev.get_EventArgs());

        public static IDisposable WriteValues(this IValueSymbol symbol, IObservable<object> valueObservable)
        {
            Action<Exception> errorHandler = delegate (Exception ex) {
            };
            return symbol.WriteValues(valueObservable, errorHandler);
        }

        public static IDisposable WriteValues(this IValueSymbol symbol, IObservable<object> valueObservable, Action<Exception> errorHandler)
        {
            Action<object> action = delegate (object o) {
                try
                {
                    symbol.WriteValue(o);
                }
                catch (Exception exception)
                {
                    errorHandler(exception);
                }
            };
            Action<Exception> action2 = delegate (Exception ex) {
                errorHandler(ex);
            };
            return ObservableExtensions.Subscribe<object>(valueObservable, action, action2, delegate {
            });
        }

        public static void WriteValues(this IValueSymbol symbol, IObservable<object> valueObservable, CancellationToken cancel)
        {
            Action<Exception> errorHandler = delegate (Exception ex) {
            };
            symbol.WriteValues(valueObservable, errorHandler, cancel);
        }

        public static void WriteValues(this IValueSymbol symbol, IObservable<object> valueObservable, Action<Exception> errorHandler, CancellationToken cancel)
        {
            Action<object> action = delegate (object o) {
                try
                {
                    symbol.WriteValue(o);
                }
                catch (Exception exception)
                {
                    errorHandler(exception);
                }
            };
            Action<Exception> action2 = ex => errorHandler(ex);
            ObservableExtensions.Subscribe<object>(valueObservable, action, action2, delegate {
            }, cancel);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ValueSymbolExtensions.<>c <>9 = new ValueSymbolExtensions.<>c();
            public static Func<EventPattern<ValueChangedArgs>, ValueChangedArgs> <>9__0_2;
            public static Func<EventPattern<ValueChangedArgs>, ValueChangedArgs> <>9__1_2;
            public static Func<EventPattern<ValueChangedArgs>, object> <>9__2_2;
            public static Func<EventPattern<ValueChangedArgs>, object> <>9__3_2;
            public static Func<EventPattern<ValueChangedArgs>, ValueChangedArgs> <>9__4_2;
            public static Action<Exception> <>9__5_0;
            public static Action <>9__6_2;
            public static Action<Exception> <>9__7_0;
            public static Action <>9__8_2;
            public static Func<long, Unit> <>9__10_0;

            internal Unit <PollValues>b__10_0(long e) => 
                Unit.get_Default();

            internal object <WhenValueChanged>b__2_2(EventPattern<ValueChangedArgs> ev) => 
                ev.get_EventArgs().Value;

            internal object <WhenValueChanged>b__3_2(EventPattern<ValueChangedArgs> ev) => 
                ev.get_EventArgs().Value;

            internal ValueChangedArgs <WhenValueChangedAnnotated>b__0_2(EventPattern<ValueChangedArgs> ev) => 
                ev.get_EventArgs();

            internal ValueChangedArgs <WhenValueChangedAnnotated>b__1_2(EventPattern<ValueChangedArgs> ev) => 
                ev.get_EventArgs();

            internal ValueChangedArgs <WhenValueChangedAnnotated>b__4_2(EventPattern<ValueChangedArgs> ev) => 
                ev.get_EventArgs();

            internal void <WriteValues>b__5_0(Exception ex)
            {
            }

            internal void <WriteValues>b__6_2()
            {
            }

            internal void <WriteValues>b__7_0(Exception ex)
            {
            }

            internal void <WriteValues>b__8_2()
            {
            }
        }
    }
}

