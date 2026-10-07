using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using Qiniu.Util;
using Qiniu.Http;
using Newtonsoft.Json;

namespace Qiniu.Storage
{
    /// <summary>
    /// 分片上传/断点续上传，适合于以下"情形2~3":
    /// (1)网络较好并且待上传的文件体积较小时(比如100MB或更小一点)使用简单上传;
    /// (2)文件较大或者网络状况不理想时请使用分片上传;
    /// (3)文件较大并且需要支持断点续上传，请使用分片上传(断点续上传)
    /// 上传时需要提供正确的上传凭证(由对应的上传策略生成)
    /// 上传策略 https://developer.qiniu.com/kodo/manual/1206/put-policy
    /// 上传凭证 https://developer.qiniu.com/kodo/manual/1208/upload-token
    /// </summary>
    public class ResumableUploader
    {
        private Config config;

        // HTTP请求管理器(GET/POST等)
        private HttpManager httpManager;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="config">分片上传的配置信息</param>
        public ResumableUploader(Config config)
        {
            if (config == null)
            {
                this.config = new Config();
            }
            else
            {
                this.config = config;
            }
            this.httpManager = new HttpManager();
        }


        /// <summary>
        /// 分片上传，支持断点续上传，带有自定义进度处理、高级控制功能
        /// </summary>
        /// <param name="localFile">本地待上传的文件名</param>
        /// <param name="key">要保存的文件名称</param>
        /// <param name="token">上传凭证</param>
        /// <param name="putExtra">上传可选配置</param>
        /// <returns>上传文件后的返回结果</returns>
        public HttpResult UploadFile(string localFile, string key, string token, PutExtra putExtra)
        {
            try
            {
                FileStream fs = new FileStream(localFile, FileMode.Open);
                return this.UploadStream(fs, key, token, putExtra);
            }
            catch (Exception ex)
            {
                HttpResult ret = HttpResult.InvalidFile;
                ret.RefText = ex.Message;
                return ret;
            }
        }



        /// <summary>
        /// 分片上传/断点续上传，带有自定义进度处理和上传控制，检查CRC32，可自动重试
        /// </summary>
        /// <param name="stream">待上传文件流</param>
        /// <param name="key">要保存的文件名称</param>
        /// <param name="upToken">上传凭证</param>
        /// <param name="putExtra">可选配置参数</param>
        /// <returns>上传文件后返回结果</returns>
        public HttpResult UploadStream(Stream stream, string key, string upToken, PutExtra putExtra)
        {
            HttpResult result = new HttpResult();

            //check put extra
            if (putExtra == null)
            {
                putExtra = new PutExtra();
                putExtra.MaxRetryTimes = config.MaxRetryTimes;
            }
            if (putExtra.ProgressHandler == null)
            {
                putExtra.ProgressHandler = DefaultUploadProgressHandler;
            }
            if (putExtra.UploadController == null)
            {
                putExtra.UploadController = DefaultUploadController;
            }

            if (!(putExtra.BlockUploadThreads > 0 && putExtra.BlockUploadThreads <= 64))
            {
                putExtra.BlockUploadThreads = 1;
            }

            using (stream)
            {
                //start to upload
                try
                {
                    // load resume record file
                    ResumeInfo resumeInfo = null;
                    if (File.Exists(putExtra.ResumeRecordFile))
                    {
                        resumeInfo = ResumeHelper.Load(putExtra.ResumeRecordFile);
                    }
                    if (putExtra.Version == "v1")
                    {
                        result = UploadStreamV1(stream, key, upToken, putExtra, resumeInfo);
                    }
                    else if (putExtra.Version == "v2")
                    {
                        result = UploadStreamV2(stream, key, upToken, putExtra, resumeInfo);
                    } else {
                        throw new Exception("Invalid Version, only supports v1 / v2");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("[{0}] [ResumableUpload] Error: ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                    Exception e = ex;
                    while (e != null)
                    {
                        sb.Append(e.Message + " ");
                        e = e.InnerException;
                    }
                    sb.AppendLine();

                    result.RefCode = (int)HttpCode.USER_UNDEF;
                    result.RefText += sb.ToString();
                }
            }

            return result;
        }

        private HttpResult UploadStreamV1(Stream stream, string key, string upToken, PutExtra putExtra, ResumeInfo resumeInfo)
        {
            HttpResult result = new HttpResult();
            bool isResumeUpload = resumeInfo != null;
            try
            {
                string encodedObjectName = "";
                long uploadedBytes = 0;
                long fileSize = stream.Length;
                long blockCount = (fileSize + putExtra.PartSize - 1) / putExtra.PartSize;
                if (resumeInfo == null)
                {
                    resumeInfo = new ResumeInfo()
                    {
                        FileSize = fileSize,
                        BlockCount = blockCount,
                        Contexts = new string[blockCount],
                        ContextsExpiredAt = new long[blockCount],
                        ExpiredAt = 0,
                    };
                }

                // init block upload error
                UploadControllerAction upCtrl = putExtra.UploadController();
                object progressLock = new object();
                ManualResetEvent upCtrlManualResetEvent = new ManualResetEvent(false);
                List<ManualResetEvent> runningTaskEvents = new List<ManualResetEvent>();
                Dictionary<long, HttpResult> blockMakeResults = new Dictionary<long, HttpResult>();
                Dictionary<string, long> uploadedBytesDict = new Dictionary<string, long>();
                uploadedBytesDict.Add("UploadProgress", uploadedBytes);
                byte[] blockBuffer = new byte[putExtra.PartSize];

                // check not finished blocks to upload
                for (long blockIndex = 0; blockIndex < blockCount; blockIndex++)
                {
                    string context = resumeInfo.Contexts[blockIndex];
                    long contextExpiredAt = resumeInfo.ContextsExpiredAt[blockIndex];
                    if (!string.IsNullOrEmpty(context) && !UnixTimestamp.IsContextExpired(contextExpiredAt))
                    {
                        uploadedBytesDict["UploadProgress"] += putExtra.PartSize;
                        continue;
                    }
                    //check upload controller action before each chunk
                    while (true)
                    {
                        upCtrl = putExtra.UploadController();

                        if (upCtrl == UploadControllerAction.Aborted)
                        {
                            result.Code = (int)HttpCode.USER_CANCELED;
                            result.RefCode = (int)HttpCode.USER_CANCELED;
                            result.RefText += string.Format("[{0}] [ResumableUpload] Info: upload task is aborted\n",
                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                            upCtrlManualResetEvent.Set();
                            return result;
                        }
                        else if (upCtrl == UploadControllerAction.Suspended)
                        {
                            result.RefCode = (int)HttpCode.USER_PAUSED;
                            result.RefText += string.Format("[{0}] [ResumableUpload] Info: upload task is paused\n",
                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                            upCtrlManualResetEvent.WaitOne(1000);
                        }
                        else if (upCtrl == UploadControllerAction.Activated)
                        {
                            break;
                        }
                    }

                    // upload blocks concurrently
                    // wait any task done
                    while (runningTaskEvents.Count >= putExtra.BlockUploadThreads)
                    {
                        int doneIndex = WaitHandle.WaitAny(runningTaskEvents.ToArray());
                        runningTaskEvents.RemoveAt(doneIndex);
                    }
                    // check mkblk results
                    foreach (HttpResult mkblkRet in blockMakeResults.Values)
                    {
                        if (mkblkRet.Code != 200)
                        {
                            result = mkblkRet;
                            upCtrlManualResetEvent.Set();
                            return result;
                        }
                    }
                    blockMakeResults.Clear();
                    // add task
                    long offset = blockIndex * putExtra.PartSize;
                    stream.Seek(offset, SeekOrigin.Begin);
                    int blockLen = stream.Read(blockBuffer, 0, putExtra.PartSize);
                    byte[] blockData = new byte[blockLen];
                    Array.Copy(blockBuffer, blockData, blockLen);

                    ManualResetEvent makeBlockEvent = createMakeBlockTask(
                        blockIndex,
                        blockData,
                        upToken,
                        putExtra,
                        resumeInfo,
                        blockMakeResults,
                        uploadedBytesDict,
                        fileSize,
                        encodedObjectName,
                        progressLock
                    );
                    runningTaskEvents.Add(makeBlockEvent);
                }
                
                
                if (runningTaskEvents.Count > 0)
                {
                    WaitHandle.WaitAll(runningTaskEvents.ToArray());
                    //check mkblk results
                    foreach (HttpResult mkblkRet in blockMakeResults.Values)
                    {
                        if (mkblkRet.Code != 200)
                        {
                            result = mkblkRet;
                            upCtrlManualResetEvent.Set();
                            return result;
                        }
                    }
                    blockMakeResults.Clear();
                }

                if (upCtrl == UploadControllerAction.Activated)
                {
                    HttpResult hr = new HttpResult();
                    hr = MakeFile(key, fileSize, key, upToken, putExtra, resumeInfo.Contexts);

                    if (hr.Code != (int)HttpCode.OK)
                    {
                        result.Shadow(hr);
                        result.RefText += string.Format("[{0}] [ResumableUpload] Error: mkfile: code = {1}, text = {2}\n",
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), hr.Code, hr.Text);
                    }

                    if (File.Exists(putExtra.ResumeRecordFile))
                    {
                        File.Delete(putExtra.ResumeRecordFile);
                    }
                    result.Shadow(hr);
                    result.RefText += string.Format("[{0}] [ResumableUpload] Uploaded: \"{1}\" ==> \"{2}\"\n",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), putExtra.ResumeRecordFile, key);
                }
                else
                {
                    result.Code = (int)HttpCode.USER_CANCELED;
                    result.RefCode = (int)HttpCode.USER_CANCELED;
                    result.RefText += string.Format("[{0}] [ResumableUpload] Info: upload task is aborted, mkfile\n",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                }

                upCtrlManualResetEvent.Set();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("[{0}] [ResumableUpload] Error: ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                Exception e = ex;
                while (e != null)
                {
                    sb.Append(e.Message + " ");
                    e = e.InnerException;
                }
                sb.AppendLine();

                result.RefCode = (int)HttpCode.USER_UNDEF;
                result.RefText += sb.ToString();
            }

            if (isResumeUpload && result.Code == (int)HttpCode.CONTEXT_EXPIRED)
            {
                stream.Seek(0, SeekOrigin.Begin);
                return UploadStreamV1(stream, key, upToken, putExtra, null);
            }

            return result;
        }

        private HttpResult UploadStreamV2(Stream stream, string key, string upToken, PutExtra putExtra, ResumeInfo resumeInfo)
        {
            HttpResult result = new HttpResult();
            bool isResumeUpload = resumeInfo != null;

            try
            {
                string encodedObjectName = "~";
                if (key != null)
                {
                    encodedObjectName = Base64.GetEncodedObjectName(key);
                }
                long uploadedBytes = 0;
                long fileSize = stream.Length;
                long blockCount = (fileSize + putExtra.PartSize - 1) / putExtra.PartSize;

                if (resumeInfo == null || UnixTimestamp.IsContextExpired(resumeInfo.ExpiredAt))
                {
                    HttpResult res = initReq(encodedObjectName, upToken);
                    Dictionary<string, string> responseBody = JsonConvert.DeserializeObject<Dictionary<string, string>>(res.Text);
                    if (res.Code != 200)
                    {
                        return res;
                    }

                    resumeInfo = new ResumeInfo()
                    {
                        FileSize = fileSize,
                        BlockCount = blockCount,
                        Etags = new Dictionary<string, object>[blockCount],
                        Uploaded = 0,
                        ExpiredAt = long.Parse(responseBody["expireAt"]),
                        UploadId = responseBody["uploadId"]
                    };
                }

                // calc upload progress
                for (long blockIndex = 0; blockIndex < blockCount; blockIndex++)
                {
                    Dictionary<string, object> etag = resumeInfo.Etags[blockIndex];
                    if (etag != null)
                    {
                        uploadedBytes += putExtra.PartSize;
                        resumeInfo.Uploaded = uploadedBytes;
                    }
                }
                // set upload progress
                putExtra.ProgressHandler(uploadedBytes, fileSize);

                
                // init block upload error
                // check not finished blocks to upload
                UploadControllerAction upCtrl = putExtra.UploadController();
                object progressLock = new object();
                ManualResetEvent upCtrlManualResetEvent = new ManualResetEvent(false);
                List<ManualResetEvent> runningTaskEvents = new List<ManualResetEvent>();
                Dictionary<long, HttpResult> blockMakeResults = new Dictionary<long, HttpResult>();
                Dictionary<string, long> uploadedBytesDict = new Dictionary<string, long>();
                uploadedBytesDict.Add("UploadProgress", uploadedBytes);
                byte[] blockBuffer = new byte[putExtra.PartSize];

                // check not finished blocks to upload
                for (long blockIndex = 0; blockIndex < blockCount; blockIndex++)
                {
                    bool isBlockExists = false;
                    Dictionary<string, object> etag = resumeInfo.Etags[blockIndex];
                    if (etag != null && etag.Count > 0)
                    {
                        isBlockExists = true;
                    }

                    if (isBlockExists) {
                        continue;
                    }

                    // check upload controller action before each chunk
                    while (true)
                    {
                        upCtrl = putExtra.UploadController();

                        if (upCtrl == UploadControllerAction.Aborted)
                        {
                            result.Code = (int)HttpCode.USER_CANCELED;
                            result.RefCode = (int)HttpCode.USER_CANCELED;
                            result.RefText += string.Format("[{0}] [ResumableUpload] Info: upload task is aborted\n",
                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                            upCtrlManualResetEvent.Set();
                            return result;
                        }
                        else if (upCtrl == UploadControllerAction.Suspended)
                        {
                            result.RefCode = (int)HttpCode.USER_PAUSED;
                            result.RefText += string.Format("[{0}] [ResumableUpload] Info: upload task is paused\n",
                                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                            upCtrlManualResetEvent.WaitOne(1000);
                        }
                        else if (upCtrl == UploadControllerAction.Activated)
                        {
                            break;
                        }
                    }

                    // upload blocks concurrently
                    // wait any task done
                    while (runningTaskEvents.Count >= putExtra.BlockUploadThreads)
                    {
                        int doneIndex = WaitHandle.WaitAny(runningTaskEvents.ToArray());
                        runningTaskEvents.RemoveAt(doneIndex);
                    }
                    // check mkblk results
                    foreach (HttpResult mkblkRet in blockMakeResults.Values)
                    {
                        if (mkblkRet.Code == (int) HttpCode.FILE_NOT_EXIST)
                        {
                            if (File.Exists(putExtra.ResumeRecordFile))
                            {
                                File.Delete(putExtra.ResumeRecordFile);
                            }
                        }
                        if (isResumeUpload && mkblkRet.Code == (int)HttpCode.FILE_NOT_EXIST)
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            return UploadStreamV2(stream, key, upToken, putExtra, null);
                        }
                        if (mkblkRet.Code != (int)HttpCode.OK)
                        {
                            result = mkblkRet;
                            upCtrlManualResetEvent.Set();
                            return result;
                        }
                    }
                    blockMakeResults.Clear();
                    // add task
                    long offset = blockIndex * putExtra.PartSize;
                    stream.Seek(offset, SeekOrigin.Begin);
                    int blockLen = stream.Read(blockBuffer, 0, putExtra.PartSize);
                    byte[] blockData = new byte[blockLen];
                    Array.Copy(blockBuffer, blockData, blockLen);

                    ManualResetEvent makeBlockEvent = createMakeBlockTask(
                        blockIndex,
                        blockData,
                        upToken,
                        putExtra,
                        resumeInfo,
                        blockMakeResults,
                        uploadedBytesDict,
                        fileSize,
                        encodedObjectName,
                        progressLock
                    );
                    runningTaskEvents.Add(makeBlockEvent);
                }

                WaitHandle.WaitAll(runningTaskEvents.ToArray());
                //check mkblk results
                foreach (HttpResult mkblkRet in blockMakeResults.Values)
                {
                    if (mkblkRet.Code == (int) HttpCode.FILE_NOT_EXIST)
                    {
                        if (File.Exists(putExtra.ResumeRecordFile))
                        {
                            File.Delete(putExtra.ResumeRecordFile);
                        }
                    }
                    if (isResumeUpload && mkblkRet.Code == (int)HttpCode.FILE_NOT_EXIST)
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        return UploadStreamV2(stream, key, upToken, putExtra, null);
                    }
                    if (mkblkRet.Code != (int)HttpCode.OK)
                    {
                        result = mkblkRet;
                        upCtrlManualResetEvent.Set();
                        return result;
                    }
                }
                blockMakeResults.Clear();

                if (upCtrl == UploadControllerAction.Activated)
                {
                    HttpResult hr = new HttpResult();;
                    hr = completeParts(key, resumeInfo, key, upToken, putExtra, encodedObjectName);

                    if (result.Code == (int) HttpCode.FILE_NOT_EXIST)
                    {
                        if (File.Exists(putExtra.ResumeRecordFile))
                        {
                            File.Delete(putExtra.ResumeRecordFile);
                        }
                    }
                    if (isResumeUpload && result.Code == (int)HttpCode.FILE_NOT_EXIST)
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        return UploadStreamV2(stream, key, upToken, putExtra, null);
                    }

                    if (hr.Code != (int)HttpCode.OK)
                    {
                        result.Shadow(hr);
                        result.RefText += string.Format("[{0}] [ResumableUpload] Error: mkfile: code = {1}, text = {2}\n",
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), hr.Code, hr.Text);
                    }

                    if (File.Exists(putExtra.ResumeRecordFile))
                    {
                        File.Delete(putExtra.ResumeRecordFile);
                    }
                    result.Shadow(hr);
                    result.RefText += string.Format("[{0}] [ResumableUpload] Uploaded: \"{1}\" ==> \"{2}\"\n",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), putExtra.ResumeRecordFile, key);
                }
                else
                {
                    result.Code = (int)HttpCode.USER_CANCELED;
                    result.RefCode = (int)HttpCode.USER_CANCELED;
                    result.RefText += string.Format("[{0}] [ResumableUpload] Info: upload task is aborted, mkfile\n",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                }

                upCtrlManualResetEvent.Set();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("[{0}] [ResumableUpload] Error: ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                Exception e = ex;
                while (e != null)
                {
                    sb.Append(e.Message + " ");
                    e = e.InnerException;
                }
                sb.AppendLine();

                result.RefCode = (int)HttpCode.USER_UNDEF;
                result.RefText += sb.ToString();
            }

            if (isResumeUpload && result.Code == (int)HttpCode.FILE_NOT_EXIST)
            {
                stream.Seek(0, SeekOrigin.Begin);
                return UploadStreamV2(stream, key, upToken, putExtra, null);
            }

            return result;
        }

        private ManualResetEvent createMakeBlockTask(
            long blockIndex,
            byte[] blockData,
            string upToken,
            PutExtra putExtra,
            ResumeInfo resumeInfo,
            Dictionary<long, HttpResult> blockMakeResults,
            Dictionary<string, long> uploadedBytesDict, // total uploaded size
            long fileSize,
            string encodedObjectName,
            object progressLock
        )
        {
            ManualResetEvent doneEvent = new ManualResetEvent(false);
            ResumeBlocker resumeBlocker = new ResumeBlocker(
                doneEvent,
                blockData,
                blockIndex,
                upToken,
                putExtra,
                resumeInfo,
                blockMakeResults,
                progressLock,
                uploadedBytesDict,
                fileSize,
                encodedObjectName
            );
            ThreadPool.QueueUserWorkItem(MakeBlockWithRetry, resumeBlocker);
            return doneEvent;
        }

        private void MakeBlockWithRetry(object resumeBlockerObj)
        {
            ResumeBlocker resumeBlocker = (ResumeBlocker)resumeBlockerObj;
            ManualResetEvent doneEvent = resumeBlocker.DoneEvent;
            Dictionary<long, HttpResult> blockMakeResults = resumeBlocker.BlockMakeResults;
            long blockIndex = resumeBlocker.BlockIndex;
            PutExtra putExtra = resumeBlocker.PutExtra;
            ResumeInfo resumeInfo = resumeBlocker.ResumeInfo;

            HttpResult result = MakeBlock(resumeBlockerObj);


            int retryTimes = 0;
            while (
                retryTimes < putExtra.MaxRetryTimes &&
                UploadUtil.ShouldRetry(result.Code, result.RefCode)
            )
            {
                result = MakeBlock(resumeBlockerObj);
                retryTimes += 1;
            }

            if (!string.IsNullOrEmpty(putExtra.ResumeRecordFile))
            {
                ResumeHelper.Save(resumeInfo, putExtra.ResumeRecordFile);
            }

            //return the http result
            blockMakeResults.Add(blockIndex, result);
            doneEvent.Set();
        }

        /// <summary>
        /// 创建块(携带首片数据),v1检查CRC32,v2检查md5
        /// </summary>
        /// <param name="resumeBlockerObj">创建分片上传的块请求</param>
        private HttpResult MakeBlock(object resumeBlockerObj)
        {
            ResumeBlocker resumeBlocker = (ResumeBlocker)resumeBlockerObj;
            ManualResetEvent doneEvent = resumeBlocker.DoneEvent;
            Dictionary<long, HttpResult> blockMakeResults = resumeBlocker.BlockMakeResults;
            PutExtra putExtra = resumeBlocker.PutExtra;
            long blockIndex = resumeBlocker.BlockIndex;
            HttpResult result = new HttpResult();
            //check whether to cancel
            while (true)
            {
                UploadControllerAction upCtl = resumeBlocker.PutExtra.UploadController();
                if (upCtl == UploadControllerAction.Suspended)
                {
                    doneEvent.WaitOne(1000);
                    continue;
                }
                else if (upCtl == UploadControllerAction.Aborted)
                {
                    doneEvent.Set();

                    result.Code = (int)HttpCode.USER_CANCELED;
                    result.RefCode = (int)HttpCode.USER_CANCELED;
                    result.RefText += string.Format("[{0}] [ResumableUpload] Info: upload task is aborted, mkblk {1}\n",
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), blockIndex);
                    blockMakeResults.Add(blockIndex, result);
                    return result;
                }
                else
                {
                    break;
                }
            }

            byte[] blockBuffer = resumeBlocker.BlockBuffer;
            int blockSize = blockBuffer.Length;

            string upToken = resumeBlocker.UploadToken;
            Dictionary<string, long> uploadedBytesDict = resumeBlocker.UploadedBytesDict;
            long fileSize = resumeBlocker.FileSize;
            object progressLock = resumeBlocker.ProgressLock;
            ResumeInfo resumeInfo = resumeBlocker.ResumeInfo;

            try
            {
                //get upload host
                string ak = UpToken.GetAccessKeyFromUpToken(upToken);
                string bucket = UpToken.GetBucketFromUpToken(upToken);
                if (ak == null || bucket == null)
                {
                    result = HttpResult.InvalidToken;
                    doneEvent.Set();
                    return result;
                }

                string uploadHost = this.config.UpHost(ak, bucket);
                string url = "";
                if (putExtra.Version == "v1")
                {
                    url = string.Format("{0}/mkblk/{1}", uploadHost, blockSize);
                }
                else if (putExtra.Version == "v2")
                {
                    url = string.Format("{0}/buckets/{1}/objects/{2}/uploads/{3}/{4}", uploadHost, bucket, resumeBlocker.encodedObjectName,
                                        resumeInfo.UploadId, blockIndex+1);
                } else {
                    throw new Exception("Invalid Version, only supports v1 / v2");
                }

                string upTokenStr = string.Format("UpToken {0}", upToken);
                using (MemoryStream ms = new MemoryStream(blockBuffer, 0, blockSize))
                {
                    byte[] data = ms.ToArray();
                    if (putExtra.Version == "v1")
                    {
                        result = httpManager.PostData(url, data, upTokenStr);
                    }
                    else if (putExtra.Version == "v2")
                    {
                        Dictionary<string, string> headers = new Dictionary<string, string>();
                        headers.Add("Authorization", upTokenStr);
                        // data to md5
                        string md5 = LabMD5.GenerateMD5(blockBuffer);
                        headers.Add("Content-MD5", md5);
                        result = httpManager.PutDataWithHeaders(url, data, headers);
                    } else {
                        throw new Exception("Invalid Version, only supports v1 / v2");
                    }


                    if (result.Code == (int)HttpCode.OK)
                    {
                        if (putExtra.Version == "v1")
                        {
                            ResumeContext rc = JsonConvert.DeserializeObject<ResumeContext>(result.Text);

                            if (rc.Crc32 > 0)
                            {
                                uint crc_1 = rc.Crc32;
                                uint crc_2 = CRC32.CheckSumSlice(blockBuffer, 0, blockSize);
                                if (crc_1 != crc_2)
                                {
                                    result.RefCode = (int)HttpCode.USER_NEED_RETRY;
                                    result.RefText += string.Format(" CRC32: remote={0}, local={1}\n", crc_1, crc_2);
                                }
                                else
                                {
                                    //write the mkblk context
                                    resumeInfo.Contexts[blockIndex] = rc.Ctx;
                                    resumeInfo.ContextsExpiredAt[blockIndex] = rc.ExpiredAt;
                                    lock (progressLock)
                                    {
                                        uploadedBytesDict["UploadProgress"] += blockSize;
                                    }
                                    putExtra.ProgressHandler(uploadedBytesDict["UploadProgress"], fileSize);
                                }
                            }
                            // TODO: unreachable? bug?
                            else if (putExtra.Version == "v2")
                            {
                                result.RefText += string.Format("[{0}] JSON Decode Error: text = {1}",
                                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), result.Text);
                                result.RefCode = (int)HttpCode.USER_NEED_RETRY;
                            } else {
                                throw new Exception("Invalid Version, only supports v1 / v2");
                            }
                        }
                        else if (putExtra.Version == "v2")
                        {
                            Dictionary<string, string> rc = JsonConvert.DeserializeObject<Dictionary<string, string>>(result.Text);
                            string md5 = LabMD5.GenerateMD5(blockBuffer);
                            if (md5 != rc["md5"])
                            {
                                result.RefCode = (int)HttpCode.USER_NEED_RETRY;
                                result.RefText += string.Format(" md5: remote={0}, local={1}\n", rc["md5"], md5);
                            }
                            else
                            {
                                Dictionary<string, object> etag = new Dictionary<string, object>();
                                etag.Add("etag", rc["etag"]);
                                etag.Add("partNumber", blockIndex + 1);
                                resumeInfo.Etags[blockIndex] =  etag;
                                lock (progressLock)
                                    {
                                        uploadedBytesDict["UploadProgress"] += blockSize;
                                        resumeInfo.Uploaded += blockSize;
                                    }
                                    putExtra.ProgressHandler(uploadedBytesDict["UploadProgress"], fileSize);
                            }
                        } else {
                            throw new Exception("Invalid Version, only supports v1 / v2");
                        }

                    }
                    else
                    {
                        result.RefCode = (int)HttpCode.USER_NEED_RETRY;
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("[{0}] mkblk Error: ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                Exception e = ex;
                while (e != null)
                {
                    sb.Append(e.Message + " ");
                    e = e.InnerException;
                }
                sb.AppendLine();

                if (ex is QiniuException)
                {
                    QiniuException qex = (QiniuException)ex;
                    result.Code = qex.HttpResult.Code;
                    result.RefCode = qex.HttpResult.Code;
                    result.Text = qex.HttpResult.Text;
                    result.RefText += sb.ToString();
                }
                else
                {
                    result.RefCode = (int)HttpCode.USER_UNDEF;
                    result.RefText += sb.ToString();
                }
            }

            return result;
        }

        /// <summary>
        /// 根据已上传的所有分片数据创建文件
        /// </summary>
        /// <param name="fileName">源文件名</param>
        /// <param name="size">文件大小</param>
        /// <param name="key">要保存的文件名</param>
        /// <param name="contexts">所有数据块的Context</param>
        /// <param name="upToken">上传凭证</param>
        /// <param name="putExtra">用户指定的额外参数</param>
        /// <returns>此操作执行后的返回结果</returns>
        private HttpResult MakeFile(string fileName, long size, string key, string upToken, PutExtra putExtra, string[] contexts)
        {
            HttpResult result = new HttpResult();

            try
            {
                string fnameStr = "fname";
                string mimeTypeStr = "";
                string keyStr = "";
                string paramStr = "";
                //check file name
                if (!string.IsNullOrEmpty(fileName))
                {
                    fnameStr = string.Format("/fname/{0}", Base64.UrlSafeBase64Encode(fileName));
                }

                //check mime type
                if (!string.IsNullOrEmpty(putExtra.MimeType))
                {
                    mimeTypeStr = string.Format("/mimeType/{0}", Base64.UrlSafeBase64Encode(putExtra.MimeType));
                }

                //check key
                if (!string.IsNullOrEmpty(key))
                {
                    keyStr = string.Format("/key/{0}", Base64.UrlSafeBase64Encode(key));
                }

                //check extra params
                if (putExtra.Params != null && putExtra.Params.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var kvp in putExtra.Params)
                    {
                        string k = kvp.Key;
                        string v = kvp.Value;
                        if (k.StartsWith("x:") && !string.IsNullOrEmpty(v))
                        {
                            sb.AppendFormat("/{0}/{1}", k, Base64.UrlSafeBase64Encode(v));
                        }
                    }

                    paramStr = sb.ToString();
                }

                //get upload host
                string ak = UpToken.GetAccessKeyFromUpToken(upToken);
                string bucket = UpToken.GetBucketFromUpToken(upToken);
                if (ak == null || bucket == null)
                {
                    return HttpResult.InvalidToken;
                }

                string uploadHost = this.config.UpHost(ak, bucket);

                string url = string.Format("{0}/mkfile/{1}{2}{3}{4}{5}", uploadHost, size, mimeTypeStr, fnameStr, keyStr, paramStr);
                string body = string.Join(",", contexts);
                string upTokenStr = string.Format("UpToken {0}", upToken);

                result = httpManager.PostText(url, body, upTokenStr);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("[{0}] mkfile Error: ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                Exception e = ex;
                while (e != null)
                {
                    sb.Append(e.Message + " ");
                    e = e.InnerException;
                }
                sb.AppendLine();

                if (ex is QiniuException)
                {
                    QiniuException qex = (QiniuException)ex;
                    result.Code = qex.HttpResult.Code;
                    result.RefCode = qex.HttpResult.Code;
                    result.Text = qex.HttpResult.Text;
                    result.RefText += sb.ToString();
                }
                else
                {
                    result.RefCode = (int)HttpCode.USER_UNDEF;
                    result.RefText += sb.ToString();
                }
            }

            return result;
        }

        /// <summary>
        /// 初始化上传任务，仅用于分片上传 V2
        /// </summary>
        /// <param name="upToken">上传凭证</param>
        /// <param name="encodedObjectName">Base64编码后的资源名</param>
        /// <returns>此操作执行后的返回结果</returns>
        private HttpResult initReq(string encodedObjectName, string upToken)
        {
            HttpResult result = new HttpResult();

            try
            {
                string ak = UpToken.GetAccessKeyFromUpToken(upToken);
                string bucket = UpToken.GetBucketFromUpToken(upToken);
                if (ak == null || bucket == null)
                {
                    return HttpResult.InvalidToken;
                }

                string uploadHost = this.config.UpHost(ak, bucket);
                string url = string.Format("{0}/buckets/{1}/objects/{2}/uploads", uploadHost, bucket, encodedObjectName);
                string upTokenStr = string.Format("UpToken {0}", upToken);
                result = httpManager.PostText(url, null, upTokenStr);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("[{0}] mkfile Error: ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                Exception e = ex;
                while (e != null)
                {
                    sb.Append(e.Message + " ");
                    e = e.InnerException;
                }
                sb.AppendLine();

                if (ex is QiniuException)
                {
                    QiniuException qex = (QiniuException)ex;
                    result.Code = qex.HttpResult.Code;
                    result.RefCode = qex.HttpResult.Code;
                    result.Text = qex.HttpResult.Text;
                    result.RefText += sb.ToString();
                }
                else
                {
                    result.RefCode = (int)HttpCode.USER_UNDEF;
                    result.RefText += sb.ToString();
                }
            }
            return result;
        }

        /// <summary>
        /// 根据已上传的所有分片数据创建文件，仅用于分片上传 V2
        /// </summary>
        /// <param name="fileName">源文件名</param>
        /// <param name="resumeInfo">分片上传记录信息</param>
        /// <param name="key">要保存的文件名</param>
        /// <param name="upToken">上传凭证</param>
        /// <param name="putExtra">用户指定的额外参数</param>
        /// <param name="encodedObjectName">Base64编码后的资源名</param>
        /// <returns>此操作执行后的返回结果</returns>
        private HttpResult completeParts(string fileName, ResumeInfo resumeInfo, string key, string upToken, PutExtra putExtra, string encodedObjectName)
        {
            HttpResult result = new HttpResult();

            try
            {
                if (string.IsNullOrEmpty(fileName)) {
                    fileName = "fname";
                }
                if (string.IsNullOrEmpty(putExtra.MimeType))
                {
                    putExtra.MimeType = "";
                }
                if (string.IsNullOrEmpty(key))
                {
                    key = "";
                }
                //get upload host
                string ak = UpToken.GetAccessKeyFromUpToken(upToken);
                string bucket = UpToken.GetBucketFromUpToken(upToken);
                if (ak == null || bucket == null)
                {
                    return HttpResult.InvalidToken;
                }

                string uploadHost = this.config.UpHost(ak, bucket);

                string upTokenStr = string.Format("UpToken {0}", upToken);
                Dictionary<string, object> body = new Dictionary<string, object>();
                body.Add("fname", fileName);
                body.Add("mimeType", putExtra.MimeType);
                body.Add("customVars", putExtra.Params);
                body.Add("parts", resumeInfo.Etags);
                string url = string.Format("{0}/buckets/{1}/objects/{2}/uploads/{3}", uploadHost, bucket, encodedObjectName, resumeInfo.UploadId);
                string bodyStr = JsonConvert.SerializeObject(body);
                result = httpManager.PostJson(url, bodyStr, upTokenStr);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("[{0}] completeParts Error: ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                Exception e = ex;
                while (e != null)
                {
                    sb.Append(e.Message + " ");
                    e = e.InnerException;
                }
                sb.AppendLine();

                if (ex is QiniuException)
                {
                    QiniuException qex = (QiniuException)ex;
                    result.Code = qex.HttpResult.Code;
                    result.RefCode = qex.HttpResult.Code;
                    result.Text = qex.HttpResult.Text;
                    result.RefText += sb.ToString();
                }
                else
                {
                    result.RefCode = (int)HttpCode.USER_UNDEF;
                    result.RefText += sb.ToString();
                }
            }

            return result;
        }

        /// <summary>
        /// 默认的进度处理函数-上传文件
        /// </summary>
        /// <param name="uploadedBytes">已上传的字节数</param>
        /// <param name="totalBytes">文件总字节数</param>
        public static void DefaultUploadProgressHandler(long uploadedBytes, long totalBytes)
        {
            if (uploadedBytes < totalBytes)
            {
                Console.WriteLine("[{0}] [ResumableUpload] Progress: {1,7:0.000}%", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), 100.0 * uploadedBytes / totalBytes);
            }
            else
            {
                Console.WriteLine("[{0}] [ResumableUpload] Progress: {1,7:0.000}%\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), 100.0);
            }
        }

        /// <summary>
        /// 默认的上传控制函数，默认不执行任何控制
        /// </summary>
        /// <returns>控制状态</returns>
        public static UploadControllerAction DefaultUploadController()
        {
            return UploadControllerAction.Activated;
        }
    }
}
