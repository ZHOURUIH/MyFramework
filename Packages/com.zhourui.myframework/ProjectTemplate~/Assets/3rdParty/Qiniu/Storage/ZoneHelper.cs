using System;
using System.Text;
using System.Collections.Generic;
using Qiniu.Http;
using Newtonsoft.Json;

namespace Qiniu.Storage
{
    internal class ZoneCacheValue
    {
        public DateTime Deadline { get; set; }
        public Zone Zone { get; set; }
    }
    
    /// <summary>
    /// Zone辅助类，查询及配置Zone
    /// </summary>
    public class ZoneHelper
    {
        private static Dictionary<string, ZoneCacheValue> zoneCache = new Dictionary<string, ZoneCacheValue>();
        private static object rwLock = new object();

        /// <summary>
        /// 从 UC 服务查询得到各个服务域名，生成 Zone 对象并返回
        /// </summary>
        /// <param name="accessKey">AccessKey</param>
        /// <param name="bucket">空间名称</param>
        /// <param name="ucHost">查询域名</param>
        /// <param name="backupUcHosts">备用查询域名</param>
        public static Zone QueryZone(
            string accessKey,
            string bucket,
            string ucHost = null,
            List<string> backupUcHosts = null
        )
        {
            ZoneCacheValue zoneCacheValue = null;
            string cacheKey = string.Format("{0}:{1}", accessKey, bucket);

            //check from cache
            lock (rwLock)
            {
                if (zoneCache.ContainsKey(cacheKey))
                {
                    zoneCacheValue = zoneCache[cacheKey];
                }
            }

            if (
                zoneCacheValue != null &&
                DateTime.Now < zoneCacheValue.Deadline &&
                zoneCacheValue.Zone != null
            )
            {
                return zoneCacheValue.Zone;
            }

            //query from uc api
            Zone zone;
            HttpResult hr = null;
            if (String.IsNullOrEmpty(ucHost))
            {
                ucHost = "https://" + Config.DefaultQueryRegionHost;
            }

            if (backupUcHosts == null)
            {
                backupUcHosts = Config.DefaultBackupQueryRegionHosts;
            }
            
            try
            {
                string queryUrl = string.Format("{0}/v4/query?ak={1}&bucket={2}",
                    ucHost,
                    accessKey,
                    bucket
                );
                HttpManager httpManager = new HttpManager();
                List<IMiddleware> middlewares = new List<IMiddleware>
                {
                    new RetryDomainsMiddleware(
                        backupUcHosts
                    )
                };
                hr = httpManager.Get(queryUrl, null, null, middlewares);
                if (hr.Code != (int) HttpCode.OK || string.IsNullOrEmpty(hr.Text))
                {
                    throw new Exception("code: " + hr.Code + ", text: " + hr.Text + ", ref-text:" + hr.RefText);
                }

                ZoneInfo zInfo = JsonConvert.DeserializeObject<ZoneInfo>(hr.Text);
                if (zInfo == null)
                {
                    throw new Exception("JSON Deserialize failed: " + hr.Text);
                }
                
                if (zInfo.Hosts.Length == 0)
                {
                    throw new Exception("There are no hosts available: " + hr.Text);
                }

                ZoneHost zHost = zInfo.Hosts[0];

                zone = new Zone();
                zone.SrcUpHosts = zHost.Up.Domains;
                zone.CdnUpHosts = zHost.Up.Domains;

                if (!string.IsNullOrEmpty(zHost.Io.Domains[0]))
                {
                    zone.IovipHost = zHost.Io.Domains[0];
                }
                else
                {
                    zone.IovipHost = Config.DefaultIoHost;
                }

                if (!string.IsNullOrEmpty(zHost.Api.Domains[0]))
                {
                    zone.ApiHost = zHost.Api.Domains[0];
                }
                else
                {
                    zone.ApiHost = Config.DefaultApiHost;
                }

                if (!string.IsNullOrEmpty(zHost.Rs.Domains[0]))
                {
                    zone.RsHost = zHost.Rs.Domains[0];
                }
                else
                {
                    zone.RsHost = Config.DefaultRsHost;
                }

                if (!string.IsNullOrEmpty(zHost.Rsf.Domains[0]))
                {
                    zone.RsfHost = zHost.Rsf.Domains[0];
                }
                else
                {
                    zone.RsfHost = Config.DefaultRsfHost;
                }

                lock (rwLock)
                {
                    zoneCacheValue = new ZoneCacheValue();
                    TimeSpan ttl = TimeSpan.FromSeconds(zHost.Ttl);
                    zoneCacheValue.Deadline = DateTime.Now.Add(ttl);
                    zoneCacheValue.Zone = zone;
                    zoneCache[cacheKey] = zoneCacheValue;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("[{0}] QueryZone Error:  ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                Exception e = ex;
                while (e != null)
                {
                    sb.Append(e.Message + " ");
                    e = e.InnerException;
                }
                sb.AppendLine();

                throw new QiniuException(hr, sb.ToString());
            }

            return zone;
        }
    }

}
