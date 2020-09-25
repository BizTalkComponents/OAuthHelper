using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace TemporaryStore
{
    public static class StoreHandler
    {
        private static ConcurrentDictionary<string, TokenInfo> tokenDic;
        private static System.Timers.Timer localTimer;
        private static void Initialize()
        {
            tokenDic = new ConcurrentDictionary<string, TokenInfo>();
            localTimer = new System.Timers.Timer();///(new TimerCallback(CheckTokens), null, 0, 3000);            
            localTimer.Elapsed += LocalTimer_Elapsed;
            localTimer.Interval = 1000;
            localTimer.Enabled = true;
        }

        private static void LocalTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            CheckTokens(null);
        }

        internal static void Add(string key, TokenInfo value)
        {
            if (tokenDic == null)
            {
                Initialize();
            }
            tokenDic.TryAdd(key, value);
        }

        internal static bool ContainsKey(string key)
        {
            return tokenDic != null && tokenDic.ContainsKey(key);
        }

        internal static bool TryGetTokenInfo(string key, out TokenInfo tokenInfo)
        {
            tokenInfo = null;
            bool retval = tokenDic != null && tokenDic.TryGetValue(key, out tokenInfo);
            if (tokenInfo != null && tokenInfo.ValidUntil > DateTime.Now.AddMinutes(-5))
            {
                tokenDic.TryRemove(key, out tokenInfo);
                retval = false;
                tokenInfo = null;
            }
            return retval;
        }


        private static void CheckTokens(object state)
        {
            var keys = tokenDic.Keys.ToList();
            foreach (string key in keys)
            {

                if (tokenDic[key].ValidUntil > DateTime.Now.AddMinutes(-5))
                {
                    TokenInfo ti = null;
                    tokenDic.TryRemove(key, out ti);
                }

            }
        }
    }
}
