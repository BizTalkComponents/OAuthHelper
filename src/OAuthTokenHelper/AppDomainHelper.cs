using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BizTalkComponents.CustomComponents.OAuthTokenHelper
{
    internal static class AppDomainHelper
    {
        private static readonly string dicGuid = "16059165-6947-4234-9BFD-6513173239E3";

        private static readonly Lazy<ICorRuntimeHost> RuntimeHost = new Lazy<ICorRuntimeHost>(() => (ICorRuntimeHost)Activator.CreateInstance(Type.GetTypeFromCLSID(Guid.Parse("CB2F6723-AB3A-11D2-9C40-00C04FA30A3E"))),
             LazyThreadSafetyMode.PublicationOnly);

        private static readonly Lazy<AppDomain> LazyDefaultAppDomain = new Lazy<AppDomain>(() =>
        {
            object appDom;
            RuntimeHost.Value.GetDefaultDomain(out appDom);
            return (AppDomain)appDom;
        }, LazyThreadSafetyMode.PublicationOnly);

        private static AppDomain DefaultAppDomain { get { return LazyDefaultAppDomain.Value; } }

        internal static readonly Lazy<TokenDictionary> LazyDic = AppDomain.CurrentDomain.IsDefaultAppDomain() ? CreateLazyWrapperOnDefaultAppDomain() : CreateLazyWrapperOnOtherDomain();

        internal static TokenDictionary TokenDictionary { get { return LazyDic.Value; } }

        private static Lazy<TokenDictionary> CreateLazyWrapperOnDefaultAppDomain()
        {
            return new Lazy<TokenDictionary>(() =>
            {
                var wrapper = new TokenDictionary();
                AppDomain.CurrentDomain.SetData(dicGuid, wrapper);
                return wrapper;
            });
        }

        private static Lazy<TokenDictionary> CreateLazyWrapperOnOtherDomain()
        {
            return new Lazy<TokenDictionary>(() =>
            {
                var defAppDomain = AppDomainHelper.DefaultAppDomain;
                TokenDictionary wrapper = (TokenDictionary)defAppDomain.GetData(dicGuid);
                if (wrapper == null)
                {
                    defAppDomain.DoCallBack(CreateCallback);
                    wrapper = (TokenDictionary)defAppDomain.GetData(dicGuid);
                }
                return wrapper;
            });
        }

        private static void CreateCallback()
        {
            //get wrapper in the default AppDomain, this way the
            var dic = LazyDic.Value;
        }

        [Guid("CB2F6722-AB3A-11D2-9C40-00C04FA30A3E")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ICorRuntimeHost
        {
            void _VtblGap_10();
            void GetDefaultDomain([MarshalAs(UnmanagedType.IUnknown)]out object appDomain);
            void EnumDomains(out IntPtr enumHandle);
            void NextDomain(IntPtr enumHandle, [MarshalAs(UnmanagedType.IUnknown)]ref object appDomain);
            void CloseEnum(IntPtr enumHandle);
        }

    }
}
