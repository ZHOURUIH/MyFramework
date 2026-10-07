using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;

namespace Qiniu.Http
{
    public class HttpRequestOptions
    {
        // Options of HttpWebRequest
        public bool? AllowAutoRedirect { get; set; }
        public bool? AllowReadStreamBuffering { get; set; }
        public bool? AllowWriteStreamBuffering { get; set; }
        public AuthenticationLevel? AuthenticationLevel { get; set; }
        public DecompressionMethods? AutomaticDecompression { get; set; }
        public RequestCachePolicy CachePolicy { get; set; }
        public X509CertificateCollection ClientCertificates { get; set; }
        public string ConnectionGroupName { get; set; }
        public HttpContinueDelegate ContinueDelegate { get; set; }
        public int? ContinueTimeout { get; set; }
        public CookieContainer CookieContainer { get; set; }
        public ICredentials Credentials { get; set; }
        public TokenImpersonationLevel? ImpersonationLevel { get; set; }
        public bool? KeepAlive { get; set; }
        public int? MaximumAutomaticRedirections { get; set; }
        public int? MaximumResponseHeadersLength { get; set; }
        public string MediaType { get; set; }
        public string Method { get; set; }
        public bool? Pipelined { get; set; }
        public bool? PreAuthenticate { get; set; }
        public IWebProxy Proxy { get; set; }
        public int? ReadWriteTimeout { get; set; }
        public bool? SendChunked { get; set; }
        public RemoteCertificateValidationCallback ServerCertificateValidationCallback { get; set; }
        public int? Timeout { get; set; }
        public bool? UnsafeAuthenticatedConnectionSharing { get; set; }
        public bool? UseDefaultCredentials { get; set; }

        // Custom Options
        public string Url;
        public StringDictionary Headers;
        public Stream RequestStream;
        public byte[] RequestData;

        public HttpRequestOptions()
        {
            Headers = new StringDictionary();
        }

        public HttpWebRequest CreateHttpWebRequest()
        {
            HttpWebRequest wReq = WebRequest.Create(Url) as HttpWebRequest;
            if (wReq == null)
            {
                StringBuilder msg = new StringBuilder();
                msg.AppendFormat("Failed to create HttpWebRequest with URL \"{0}\".", Url);
                throw new InvalidOperationException(msg.ToString());
            }

            SetProperties(wReq);
            SetHeaders(wReq);
            wReq.ServicePoint.Expect100Continue = false;
            SetBody(wReq);

            return wReq;
        }

        private void SetProperties(HttpWebRequest wReq)
        {
            if (AllowAutoRedirect.HasValue)
            {
                wReq.AllowAutoRedirect = AllowAutoRedirect.Value;
            }

            if (AllowReadStreamBuffering.HasValue)
            {
                wReq.AllowReadStreamBuffering = AllowReadStreamBuffering.Value;
            }

            if (AllowWriteStreamBuffering.HasValue)
            {
                wReq.AllowWriteStreamBuffering = AllowWriteStreamBuffering.Value;
            }

            if (AuthenticationLevel.HasValue)
            {
                wReq.AuthenticationLevel = AuthenticationLevel.Value;
            }

            if (AutomaticDecompression.HasValue)
            {
                wReq.AutomaticDecompression = AutomaticDecompression.Value;
            }

            if (CachePolicy != null)
            {
                wReq.CachePolicy = CachePolicy;
            }

            if (ClientCertificates != null)
            {
                wReq.ClientCertificates = ClientCertificates;
            }

            if (ConnectionGroupName != null)
            {
                wReq.ConnectionGroupName = ConnectionGroupName;
            }

            if (ContinueDelegate != null)
            {
                wReq.ContinueDelegate = ContinueDelegate;
            }

            if (ContinueTimeout.HasValue)
            {
                wReq.ContinueTimeout = ContinueTimeout.Value;
            }

            if (CookieContainer != null)
            {
                wReq.CookieContainer = CookieContainer;
            }

            if (Credentials != null)
            {
                wReq.Credentials = Credentials;
            }

            if (ImpersonationLevel.HasValue)
            {
                wReq.ImpersonationLevel = ImpersonationLevel.Value;
            }

            if (KeepAlive.HasValue)
            {
                wReq.KeepAlive = KeepAlive.Value;
            }

            if (MaximumAutomaticRedirections.HasValue)
            {
                wReq.MaximumAutomaticRedirections = MaximumAutomaticRedirections.Value;
            }

            if (MaximumResponseHeadersLength.HasValue)
            {
                wReq.MaximumResponseHeadersLength = MaximumResponseHeadersLength.Value;
            }

            if (MediaType != null)
            {
                wReq.MediaType = MediaType;
            }

            if (Method != null)
            {
                wReq.Method = Method;
            }

            if (Pipelined.HasValue)
            {
                wReq.Pipelined = Pipelined.Value;
            }

            if (PreAuthenticate.HasValue)
            {
                wReq.PreAuthenticate = PreAuthenticate.Value;
            }

            if (Proxy != null)
            {
                wReq.Proxy = Proxy;
            }

            if (ReadWriteTimeout.HasValue)
            {
                wReq.ReadWriteTimeout = ReadWriteTimeout.Value;
            }

            if (SendChunked.HasValue)
            {
                wReq.SendChunked = SendChunked.Value;
            }

            if (ServerCertificateValidationCallback != null)
            {
                wReq.ServerCertificateValidationCallback = ServerCertificateValidationCallback;
            }

            if (Timeout.HasValue)
            {
                wReq.Timeout = Timeout.Value;
            }

            if (UnsafeAuthenticatedConnectionSharing.HasValue)
            {
                wReq.UnsafeAuthenticatedConnectionSharing = UnsafeAuthenticatedConnectionSharing.Value;
            }

            if (UseDefaultCredentials.HasValue)
            {
                wReq.UseDefaultCredentials = UseDefaultCredentials.Value;
            }
        }

        private void SetHeaders(HttpWebRequest wReq)
        {
            if (Headers == null || wReq == null)
            {
                return;
            }

            foreach (string fieldName in Headers.Keys)
            {
                string fieldVal = Headers[fieldName];
                if (WebHeaderCollection.IsRestricted(fieldName))
                {
                    switch (fieldName)
                    {
                        case "accept":
                            wReq.Accept = fieldVal;
                            break;
                        // should be set by KeepAlive property
                        // case "connection":
                        //     wReq.Connection = fieldVal;
                        //     break;
                        case "content-type":
                            wReq.ContentType = fieldVal;
                            break;
                        case "date":
                            wReq.Date = DateTime.Parse(fieldVal);
                            break;
                        case "expect":
                            wReq.Expect = fieldVal;
                            break;
                        case "host":
                            wReq.Host = fieldVal;
                            break;
                        case "if-modified-since":
                            wReq.IfModifiedSince = DateTime.Parse(fieldVal);
                            break;
                        case "referer":
                            wReq.Referer = fieldVal;
                            break;
                        case "transfer-encoding":
                            wReq.TransferEncoding = fieldVal;
                            break;
                        case "user-agent":
                            wReq.UserAgent = fieldVal;
                            break;
                    }
                }
                else
                {
                    wReq.Headers.Add(fieldName, fieldVal);
                }
            }
        }

        private void SetBody(HttpWebRequest wReq)
        {
            if (RequestData != null)
            {
                wReq.ContentLength = RequestData.Length;
                wReq.AllowWriteStreamBuffering = true;
                using (Stream sReq = wReq.GetRequestStream())
                {
                    sReq.Write(RequestData, 0, RequestData.Length);
                    sReq.Flush();
                }

                return;
            }

            if (RequestStream != null)
            {
                wReq.ContentLength = RequestStream.Length;
                using (Stream sReq = wReq.GetRequestStream())
                {
                    RequestStream.CopyTo(sReq);
                }
            }
        }
    }
}