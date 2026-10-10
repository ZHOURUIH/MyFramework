using System;
using System.Collections.Generic;

namespace Qiniu.Http
{
    public delegate HttpResult DNextSend(HttpRequestOptions reqOpts);

    public interface IMiddleware
    {
        HttpResult Send(HttpRequestOptions req, DNextSend next);
    }

    public delegate bool DRetryCondition(HttpResult respResult, HttpRequestOptions reqOpts);

    public class RetryDomainsMiddleware : IMiddleware
    {
        private List<string> _backupDomains;

        private int _maxRetryTimes;

        private DRetryCondition _retryCondition;

        public RetryDomainsMiddleware(
            List<string> backupDomains,
            int maxRetryTimes = 2,
            DRetryCondition retryCondition = null
        )
        {
            _backupDomains = backupDomains;
            _maxRetryTimes = maxRetryTimes;
            _retryCondition = retryCondition;
        }

        public HttpResult Send(HttpRequestOptions reqOpts, DNextSend next)
        {
            HttpResult result = null;

            UriBuilder uriBuilder = new UriBuilder(reqOpts.Url);
            List<string> domains = new List<string>(_backupDomains);
            domains.Insert(0, uriBuilder.Host);

            foreach (string domain in domains)
            {
                uriBuilder.Host = domain;
                reqOpts.Url = uriBuilder.ToString();

                for (int retriedTimes = 0; retriedTimes < _maxRetryTimes; retriedTimes++)
                {
                    result = next(reqOpts);
                    if (!ShouldRetry(result, reqOpts))
                    {
                        return result;
                    }
                }
            }

            return result;
        }

        private bool ShouldRetry(HttpResult respResult, HttpRequestOptions reqOpts)
        {
            if (_retryCondition != null)
            {
                return _retryCondition(respResult, reqOpts);
            }

            return respResult != null && respResult.NeedRetry();
        }
    }
}