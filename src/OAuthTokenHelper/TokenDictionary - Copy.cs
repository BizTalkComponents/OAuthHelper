using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace BizTalkComponents.CustomComponents.OAuthTokenHelper
{
    public static class TokenDictionary
    {

        public static ConcurrentDictionary<string, TokenInfo> tokenDic;
        public static System.Timers.Timer localTimer;
        private static bool initialized, initializing;
        private static int initThreadId;
        internal static void InitializeFromWrapper()
        {
            TokenDictionary.WriteLine(AppDomain.CurrentDomain.FriendlyName);
            if (!initialized)
            {
                initThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
                tokenDic = new MyConcurrentDictionary<string, TokenInfo>();
                TokenDictionary.WriteLine("Dictionary Initialized, threadId:{0}", initThreadId);
                localTimer = new System.Timers.Timer(10000);
                localTimer.Elapsed += LocalTimer_Elapsed;
                //GC.SuppressFinalize(tokenDic);
                //GC.SuppressFinalize(localTimer);
                initialized = true;
            }
        }

        private static void Initialize()
        {
            TokenDictionary.WriteLine(AppDomain.CurrentDomain.FriendlyName);
            initializing = true;
            var host = new mscoree.CorRuntimeHost();
            object defaultAppDomain;
            host.GetDefaultDomain(out defaultAppDomain);
            AppDomain defAppDomain = (AppDomain)defaultAppDomain;
            defAppDomain.DoCallBack(new CrossAppDomainDelegate(InitializeFromWrapper));
            initializing = false;
        }

        private static void LocalTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TokenDictionary.WriteLine("Timer, Dic Count: {0}", tokenDic.Count);
            var keys = tokenDic.Keys.ToList();
            foreach (string key in keys)
            {
                TokenInfo ti = tokenDic[key];
                if (!ti.IsValid & !ti.IsWaiting)
                {
                    tokenDic.TryRemove(key, out ti);
                }
            }
            if (tokenDic.Count == 0)
            {
                localTimer.Enabled = false;
                TokenDictionary.WriteLine("Timer is disabled");
                WriteLogMessage("No more valid token available in the dictionary");
            }
        }
        public static void EnableTimer()
        {
            if (localTimer != null && !localTimer.Enabled)
            {
                localTimer.Enabled = true;
            }
        }

        internal static TokenInfo GetOrCreateTokenInfo(string key)
        {
            if (!initialized)
            {
                if (initializing)
                {
                    while (initializing)
                        Thread.Sleep(100);
                }
                else
                    Initialize();
            }
            var ti = tokenDic.GetOrAdd(key, k => new TokenInfo());
            localTimer.Enabled = true;
            //WriteLogMessage("Adding new token to the dictionary\nCount: " + tokenDic.Count);
            return ti;
        }


        internal static void WriteLogMessage(string message, System.Diagnostics.EventLogEntryType evType = System.Diagnostics.EventLogEntryType.Information, [CallerMemberName] string procName = "")
        {
            System.Diagnostics.EventLog.WriteEntry("TokenHelper", string.Format("{0}\n{1}", procName, message), evType);
        }

        public static void WriteLine(string message, params object[] arg)
        {
            //Console.WriteLine("{0:yyyy-MM-dd HH:mm:sss}-{1}", DateTime.Now, string.Format(message, arg));
            WriteLogMessage(string.Format("{0:yyyy-MM-dd HH:mm:sss}-{1}", DateTime.Now, string.Format(message, arg)));
        }
    }
    public class MyConcurrentDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>
    {
        Stopwatch sw;
        public MyConcurrentDictionary()
            : base()
        {
            sw = new Stopwatch();
            sw.Start();
        }

        ~MyConcurrentDictionary()
        {
            sw.Stop();
            TokenDictionary.WriteLine("Dictionary is destroyed, time lived: {0}", sw.Elapsed);
            //System.Diagnostics.EventLog.WriteEntry("TokenHelper", string.Format("Calling distructor time(sec): {0}", sw.Elapsed.Seconds), EventLogEntryType.Information);
        }
    }


    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class InstanceWrapper : MarshalByRefObject
    {
        public void InitializeInAppDomain()
        {
            TokenDictionary.InitializeFromWrapper();
        }
    }

}
