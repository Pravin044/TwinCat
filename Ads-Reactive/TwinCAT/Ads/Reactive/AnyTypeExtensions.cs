namespace TwinCAT.Ads.Reactive
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Runtime.CompilerServices;
    using TwinCAT.Ads;
    using TwinCAT.Ads.Internal;
    using TwinCAT.TypeSystem;

    public static class AnyTypeExtensions
    {
        public static IObservable<T> PollValues<T>(this IAdsConnection connection, string instancePath, IObservable<Unit> trigger) => 
            connection.PollValues<T>(instancePath, null, trigger, null);

        public static IObservable<T> PollValues<T>(this IAdsConnection connection, string instancePath, TimeSpan period) => 
            connection.PollValues<T>(instancePath, null, period, null);

        public static IObservable<T> PollValues<T>(this IAdsConnection connection, string instancePath, IObservable<Unit> trigger, Func<Exception, T> errorHandler) => 
            connection.PollValues<T>(instancePath, null, trigger, errorHandler);

        public static IObservable<T> PollValues<T>(this IAdsConnection connection, string instancePath, TimeSpan period, Func<Exception, T> errorHandler) => 
            connection.PollValues<T>(instancePath, null, period, errorHandler);

        public static IObservable<object> PollValues(this IAdsConnection connection, string instancePath, Type type, IObservable<Unit> trigger) => 
            connection.PollValues(instancePath, type, null, trigger, null);

        public static IObservable<T> PollValues<T>(this IAdsConnection connection, string instancePath, int[] args, IObservable<Unit> trigger) => 
            connection.PollValues<T>(instancePath, args, trigger, null);

        public static IObservable<object> PollValues(this IAdsConnection connection, string instancePath, Type type, TimeSpan period) => 
            connection.PollValues(instancePath, type, null, period, null);

        public static IObservable<T> PollValues<T>(this IAdsConnection connection, string instancePath, int[] args, TimeSpan period)
        {
            long[] numArray1 = new long[] { -1L };
            return connection.PollValues<T>(instancePath, Observable.Select<long, Unit>(Observable.StartWith<long>(Observable.Interval(period), numArray1), e => Unit.get_Default()), null);
        }

        public static IObservable<object> PollValues(this IAdsConnection connection, string instancePath, Type type, IObservable<Unit> trigger, Func<Exception, object> errorHandler) => 
            connection.PollValues(instancePath, type, null, trigger, errorHandler);

        public static IObservable<T> PollValues<T>(this IAdsConnection connection, string instancePath, int[] args, IObservable<Unit> trigger, Func<Exception, T> errorHandler)
        {
            string[] symbolPaths = new string[] { instancePath };
            DisposableHandleBag bag = new DisposableHandleBag(connection, symbolPaths);
            Func<Unit, T> func = delegate (Unit o) {
                try
                {
                    return (T) connection.ReadAny(0xf005, bag.GetHandle(instancePath), typeof(T), args);
                }
                catch (Exception exception)
                {
                    if (errorHandler == null)
                    {
                        throw;
                    }
                    return errorHandler(exception);
                }
            };
            Action action = delegate {
                bag.Dispose();
                bag = null;
            };
            return Observable.Finally<T>(Observable.Select<Unit, T>(trigger, func), action);
        }

        public static IObservable<object> PollValues(this IAdsConnection connection, string instancePath, Type type, TimeSpan period, Func<Exception, object> errorHandler) => 
            connection.PollValues(instancePath, type, null, period, errorHandler);

        public static IObservable<T> PollValues<T>(this IAdsConnection connection, string instancePath, int[] args, TimeSpan period, Func<Exception, T> errorHandler)
        {
            long[] numArray1 = new long[] { -1L };
            return connection.PollValues<T>(instancePath, Observable.Select<long, Unit>(Observable.StartWith<long>(Observable.Interval(period), numArray1), e => Unit.get_Default()), errorHandler);
        }

        public static IObservable<object> PollValues(this IAdsConnection connection, string instancePath, Type type, int[] args, TimeSpan period) => 
            connection.PollValues(instancePath, type, args, period, null);

        public static IObservable<object> PollValues(this IAdsConnection connection, string instancePath, Type type, int[] args, IObservable<Unit> trigger, Func<Exception, object> errorHandler)
        {
            string[] symbolPaths = new string[] { instancePath };
            DisposableHandleBag bag = new DisposableHandleBag(connection, symbolPaths);
            Func<Unit, object> func = delegate (Unit o) {
                try
                {
                    return connection.ReadAny(0xf005, bag.GetHandle(instancePath), type, args);
                }
                catch (Exception exception)
                {
                    if (errorHandler == null)
                    {
                        throw;
                    }
                    return errorHandler(exception);
                }
            };
            Action action = delegate {
                bag.Dispose();
                bag = null;
            };
            return Observable.Finally<object>(Observable.Select<Unit, object>(trigger, func), action);
        }

        public static IObservable<object> PollValues(this IAdsConnection connection, string instancePath, Type type, int[] args, TimeSpan period, Func<Exception, object> errorHandler)
        {
            long[] numArray1 = new long[] { -1L };
            return connection.PollValues(instancePath, type, args, Observable.Select<long, Unit>(Observable.StartWith<long>(Observable.Interval(period), numArray1), e => Unit.get_Default()), errorHandler);
        }

        public static IObservable<T> WhenNotification<T>(this IAdsConnection connection, string instancePath, NotificationSettings settings)
        {
            Dictionary<string, AnyTypeSpecifier> symbols = new Dictionary<string, AnyTypeSpecifier>();
            AnyTypeSpecifier specifier = new AnyTypeSpecifier(typeof(T));
            symbols.Add(instancePath, specifier);
            return Observable.Select<NotificationEx, T>(connection.WhenNotificationEx(symbols, settings, null), n => (T) n.Value);
        }

        [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
        public static IObservable<NotificationEx> WhenNotification(this IAdsConnection connection, IDictionary<string, Type> symbols, NotificationSettings settings, object userData)
        {
            Dictionary<string, AnyTypeSpecifier> dictionary = new Dictionary<string, AnyTypeSpecifier>();
            foreach (KeyValuePair<string, Type> pair in symbols)
            {
                dictionary.Add(pair.Key, new AnyTypeSpecifier(pair.Value));
            }
            return connection.WhenNotificationEx(dictionary, settings, userData);
        }

        public static IObservable<object> WhenNotification(this IAdsConnection connection, string instancePath, Type type, NotificationSettings settings)
        {
            Dictionary<string, AnyTypeSpecifier> symbols = new Dictionary<string, AnyTypeSpecifier>();
            AnyTypeSpecifier specifier = new AnyTypeSpecifier(type);
            symbols.Add(instancePath, specifier);
            return Observable.Select<NotificationEx, object>(connection.WhenNotificationEx(symbols, settings, null), n => n.Value);
        }

        public static IDisposable WriteValues<T>(this IAdsConnection connection, string instancePath, IObservable<T> valueSequence) => 
            connection.WriteValues<T>(instancePath, valueSequence, null);

        public static IDisposable WriteValues<T>(this IAdsConnection connection, string instancePath, IObservable<T> valueSequence, Action<Exception> errorHandler)
        {
            string[] symbolPaths = new string[] { instancePath };
            DisposableHandleBag bag = new DisposableHandleBag(connection, symbolPaths);
            Action<T> action = delegate (T v) {
                try
                {
                    uint handle = 0;
                    if (!bag.TryGetHandle(instancePath, out handle))
                    {
                        throw new AdsException($"Handle for '{instancePath}' is not created!");
                    }
                    connection.WriteAny((int) handle, v);
                }
                catch (Exception exception)
                {
                    errorHandler(exception);
                }
            };
            return ObservableExtensions.Subscribe<T>(valueSequence, action, delegate (Exception ex) {
                try
                {
                    if (errorHandler == null)
                    {
                        throw ex;
                    }
                    errorHandler(ex);
                }
                finally
                {
                    bag.Dispose();
                }
            }, delegate {
                bag.Dispose();
            });
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly AnyTypeExtensions.<>c <>9 = new AnyTypeExtensions.<>c();
            public static Func<NotificationEx, object> <>9__2_0;
            public static Func<long, Unit> <>9__6_0;

            internal Unit <PollValues>b__6_0(long e) => 
                Unit.get_Default();

            internal object <WhenNotification>b__2_0(NotificationEx n) => 
                n.Value;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c__1<T>
        {
            public static readonly AnyTypeExtensions.<>c__1<T> <>9;
            public static Func<NotificationEx, T> <>9__1_0;

            static <>c__1()
            {
                AnyTypeExtensions.<>c__1<T>.<>9 = new AnyTypeExtensions.<>c__1<T>();
            }

            internal T <WhenNotification>b__1_0(NotificationEx n) => 
                ((T) n.Value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c__18<T>
        {
            public static readonly AnyTypeExtensions.<>c__18<T> <>9;
            public static Func<long, Unit> <>9__18_0;

            static <>c__18()
            {
                AnyTypeExtensions.<>c__18<T>.<>9 = new AnyTypeExtensions.<>c__18<T>();
            }

            internal Unit <PollValues>b__18_0(long e) => 
                Unit.get_Default();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c__19<T>
        {
            public static readonly AnyTypeExtensions.<>c__19<T> <>9;
            public static Func<long, Unit> <>9__19_0;

            static <>c__19()
            {
                AnyTypeExtensions.<>c__19<T>.<>9 = new AnyTypeExtensions.<>c__19<T>();
            }

            internal Unit <PollValues>b__19_0(long e) => 
                Unit.get_Default();
        }
    }
}

