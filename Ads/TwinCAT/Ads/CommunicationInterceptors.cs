namespace TwinCAT.Ads
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    [EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Never)]
    public class CommunicationInterceptors : CommunicationInterceptor
    {
        private List<ICommunicationInterceptHandler> _list;
        private static int _id;

        public CommunicationInterceptors() : base($"CombinedInterceptor_{++_id}")
        {
            this._list = new List<ICommunicationInterceptHandler>();
        }

        public CommunicationInterceptors(IEnumerable<ICommunicationInterceptHandler> interceptors) : this()
        {
            this._list.AddRange(interceptors);
        }

        public ICommunicationInterceptor Combine(ICommunicationInterceptor interceptor)
        {
            this._list.Add((ICommunicationInterceptHandler) interceptor);
            return this;
        }

        public T Find<T>() => 
            Enumerable.FirstOrDefault<ICommunicationInterceptHandler>(this._list, interceptor => typeof(T).IsAssignableFrom(interceptor.GetType()));

        public ICommunicationInterceptor Find(string id) => 
            Enumerable.FirstOrDefault<ICommunicationInterceptor>(Enumerable.Cast<ICommunicationInterceptor>(this._list), interceptor => StringComparer.OrdinalIgnoreCase.Compare(interceptor.ID, id) == 0);

        public ICommunicationInterceptor Lookup(Type interceptorType) => 
            ((ICommunicationInterceptor) this._list.Find(element => element.GetType().IsAssignableFrom(interceptorType)));

        protected override void OnAfterCommunicate(AdsErrorCode errorCode)
        {
            foreach (ICommunicationInterceptHandler handler in this._list)
            {
                handler.AfterCommunicate(errorCode);
            }
        }

        protected override void OnAfterConnect(AdsErrorCode errorCode)
        {
            foreach (ICommunicationInterceptHandler handler in this._list)
            {
                handler.AfterConnect(errorCode);
            }
        }

        protected override void OnAfterDisconnect(AdsErrorCode errorCode)
        {
            foreach (ICommunicationInterceptHandler handler in this._list)
            {
                handler.AfterDisconnect(errorCode);
            }
        }

        protected override void OnAfterReadState(StateInfo adsState, AdsErrorCode result)
        {
            foreach (ICommunicationInterceptHandler handler in this._list)
            {
                handler.AfterReadState(adsState, result);
            }
        }

        protected override void OnAfterWriteState(StateInfo adsState, AdsErrorCode result)
        {
            foreach (ICommunicationInterceptHandler handler in this._list)
            {
                handler.AfterWriteState(adsState, result);
            }
        }

        protected override AdsErrorCode OnBeforeCommunicate()
        {
            AdsErrorCode noError = AdsErrorCode.NoError;
            foreach (ICommunicationInterceptHandler handler in this._list)
            {
                noError = handler.BeforeCommunicate();
                if (noError != AdsErrorCode.NoError)
                {
                    break;
                }
            }
            return noError;
        }

        protected override AdsErrorCode OnBeforeConnect()
        {
            AdsErrorCode noError = AdsErrorCode.NoError;
            foreach (ICommunicationInterceptHandler handler in this._list)
            {
                noError = handler.BeforeConnect();
                if (noError != AdsErrorCode.NoError)
                {
                    break;
                }
            }
            return noError;
        }

        protected override AdsErrorCode OnBeforeDisconnect()
        {
            AdsErrorCode noError = AdsErrorCode.NoError;
            foreach (ICommunicationInterceptHandler handler in this._list)
            {
                noError = handler.BeforeDisconnect();
                if (noError != AdsErrorCode.NoError)
                {
                    break;
                }
            }
            return noError;
        }

        protected override AdsErrorCode OnBeforeReadState()
        {
            AdsErrorCode noError = AdsErrorCode.NoError;
            foreach (ICommunicationInterceptHandler handler in this._list)
            {
                noError = handler.BeforeReadState();
                if (noError != AdsErrorCode.NoError)
                {
                    break;
                }
            }
            return noError;
        }

        protected override AdsErrorCode OnBeforeWriteState(StateInfo adsState)
        {
            AdsErrorCode noError = AdsErrorCode.NoError;
            foreach (ICommunicationInterceptHandler handler in this._list)
            {
                noError = handler.BeforeWriteState(adsState);
                if (noError != AdsErrorCode.NoError)
                {
                    break;
                }
            }
            return noError;
        }

        internal ReadOnlyCollection<ICommunicationInterceptor> CombinedInterceptors =>
            new List<ICommunicationInterceptor>(Enumerable.Cast<ICommunicationInterceptor>(this._list)).AsReadOnly();

        [Serializable, CompilerGenerated]
        private sealed class <>c__6<T>
        {
            public static readonly CommunicationInterceptors.<>c__6<T> <>9;
            public static Func<ICommunicationInterceptHandler, bool> <>9__6_0;

            static <>c__6()
            {
                CommunicationInterceptors.<>c__6<T>.<>9 = new CommunicationInterceptors.<>c__6<T>();
            }

            internal bool <Find>b__6_0(ICommunicationInterceptHandler interceptor) => 
                typeof(T).IsAssignableFrom(interceptor.GetType());
        }
    }
}

