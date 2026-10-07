using Qiniu.Http;

namespace Qiniu.Storage
{
    public static class UploadUtil
    {
        public static bool ShouldRetry(int code, int refCode)
        {
            if (code == (int) HttpCode.OK)
            {
                return false;
            }

            // allow list
            if (
                refCode == (int)HttpCode.USER_UNDEF ||
                refCode == (int)HttpCode.USER_NEED_RETRY
            )
            {
                return true;
            }
            
            // block list
            int codeSeries = code / 100;

            if (codeSeries == 4 && code != (int)HttpCode.CRC32_CHECK_FAILEd)
            {
                return false;
            }

            if (
                code == (int)HttpCode.FILE_NOT_EXIST ||
                code == (int)HttpCode.FILE_EXISTS ||
                code == (int)HttpCode.CALLBACK_FAILED
            )
            {
                return false;
            }
            
            // others need retry
            return true;
        }
    }
}