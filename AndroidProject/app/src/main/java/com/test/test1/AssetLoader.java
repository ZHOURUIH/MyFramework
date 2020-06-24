package 此处替换为自己的包名;

import android.content.res.AssetManager;

import com.unity3d.player.UnityPlayer;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.util.ArrayList;
import java.util.List;

public class AssetLoader
{
    protected static byte[] mHelperBytes;
    protected AssetManager mAssetManager;
    public void setAssetManager(AssetManager assetManager){mAssetManager = assetManager;}
    public boolean isAssetExist(String path)
    {
        try
        {
            return mAssetManager.open(path) != null;
        }
        catch (IOException e)
        {
            unityLog("isAssetExist exception : path : " + path + ", " + e.getStackTrace());
            return false;
        }
    }
    //读取assetbund并且返回字节数组
    public byte[] loadAsset(String path, boolean errorIfNull)
    {
        try
        {
            InputStream inputStream = mAssetManager.open(path);
            if(inputStream == null)
            {
                if(errorIfNull)
                {
                    unityError("can not open file : " + path);
                }
                return null;
            }
            long length = mAssetManager.openFd(path).getLength();
            return streamToBytes(inputStream, (int)length);
        }
        catch (IOException e)
        {
            unityLog("loadAsset exception : path : " + path + ", " + e.getStackTrace());
            return null;
        }
    }
    public String loadTxtAsset(String path, boolean errorIfNull)
    {
        try
        {
            byte[] buffer = loadAsset(path, errorIfNull);
            if(buffer != null)
            {
                String str = new String(buffer, "UTF-8");
                return str;
            }
            else
            {
                unityError("buffer is null!");
                return "";
            }
        }
        catch (Exception e)
        {
            unityLog("loadTxtAsset exception : path : " + path + ", " + e.getStackTrace());
            return "";
        }
    }
    // 获取Assets文件列表,path是相对于StreamingAssets的相对路径
    public List<String> startFindAssets(String path, String patterns, boolean recursive)
    {
        List<String> fileList = new ArrayList<String>();
        String[] patternList = patterns.split(" ");
        findAssets(path, fileList, patternList, recursive);
        return fileList;
    }
    // 开始获取Asset中的文件夹列表,path是相对于StreamingAssets的相对路径
    public List<String> startFindAssetsFolder(String path, boolean recursive)
    {
        List<String> folderList = new ArrayList<String>();
        findAssetFolders(path, folderList, recursive);
        return folderList;
    }
    // 获取列表中指定下标的文件列表,因为c#中没办法直接使用java的List对象(原谅我没有别的办法了),所以需要传入java中获取string
    public static String getListElement(List<String> list, int index)
    {
        int count = list.size();
        if(index >= 0 && index < count)
        {
            return list.get(index);
        }
        return "";
    }
    //-------------------------------------------------------------------------------------------------------------------------------------------------------------
    // 以下函数只用于persistentDataPath的读写
    public static String loadTxtFile(String path, boolean errorIfNull)
    {
        byte[] buffer = loadFile(path, errorIfNull);
        if(buffer == null)
        {
            return  "";
        }
        try
        {
            String str = new String(buffer, "UTF-8");
            return str;
        }
        catch (Exception e)
        {
            unityLog("loadTxtFile exception : path : " + path + ", " + e.getStackTrace());
        }
        return "";
    }
    public static byte[] loadFile(String path, boolean errorIfNull)
    {
        File file = new File(path);
        if(!file.exists())
        {
            if(errorIfNull)
            {
                unityError("can not find file : " + path);
            }
            return null;
        }
        try
        {
            FileInputStream fileStream = new FileInputStream(file);
            int fileSize = fileStream.available();
            byte[] bytes = streamToBytes(fileStream, fileSize);
            fileStream.close();
            return bytes;
        }
        catch (Exception e)
        {
            unityError("load file exception : path : " + path + ", " + e.getStackTrace());
            return null;
        }
    }
    public static String getFilePath(String fullName)
    {
        int lastPos = fullName.lastIndexOf('/');
        if(lastPos != -1)
        {
            return fullName.substring(0, lastPos);
        }
        return "";
    }
    public static void writeFile(String path, byte[] buffer, int writeCount, boolean appendData)
    {
        try
        {
            createDirectory(getFilePath(path));
            File file = new File(path);
            if(!file.exists())
            {
                file.createNewFile();
            }
            if(!appendData)
            {
                file.deleteOnExit();
                file.createNewFile();
            }
            FileOutputStream outputStream = new FileOutputStream(file);
            outputStream.write(buffer, 0, writeCount);
            outputStream.close();
        }
        catch (Exception e)
        {
            unityError("writeFile exception, path:" + path + ", info:" + e.getMessage() + ",stack:" + e.getStackTrace());
        }
    }
    public static void writeTxtFile(String path, String str, boolean appendData)
    {
        try
        {
            byte[] bytes = str.getBytes("UTF-8");
            writeFile(path, bytes, bytes.length, appendData);
        }
        catch (Exception e)
        {
            unityError("writeTxtFile exception, path:" + path + ", info:" + e.getMessage() + ",stack:" + e.getStackTrace());
        }
    }
    public static void deleteFile(String path)
    {
        File file = new File(path);
        file.deleteOnExit();
    }
    public static boolean isDirExist(String path)
    {
        File dir = new File(path);
        if(!dir.isDirectory())
        {
            return false;
        }
        return dir.exists();
    }
    public static boolean isFileExist(String path)
    {
        File dir = new File(path);
        if(!dir.isFile())
        {
            return false;
        }
        return dir.exists();
    }
    public static int getFileSize(String path)
    {
        File file = new File(path);
        if(!file.isFile())
        {
            return 0;
        }
        try
        {
            FileInputStream fileStream = new FileInputStream(file);
            int size = fileStream.available();
            fileStream.close();
            return size;
        }
        catch (Exception e)
        {
            unityError("getFileSize exception : " + e.getStackTrace());
            return 0;
        }
    }
    // 开始获取非Assets文件列表
    public static List<String> startFindFiles(String path, String patterns, boolean recursive)
    {
        List<String> fileList = new ArrayList<String>();
        String[] patternList = patterns.split(" ");
        findFiles(path, fileList, patternList, recursive);
        return fileList;
    }
    // 开始获取非Assets文件列表
    public static List<String> startFindFolders(String path, boolean recursive)
    {
        List<String> folderList = new ArrayList<String>();
        findFolders(path, folderList, recursive);
        return folderList;
    }
    public static void createDirectory(String path)
    {
        File file = new File(path);
        if(!file.exists())
        {
            file.mkdir();
        }
    }
    public static void unityLog(String info)
    {
        UnityPlayer.UnitySendMessage("UnityLog", "log", info);
    }
    public static void unityError(String info)
    {
        UnityPlayer.UnitySendMessage("UnityLog", "logError", info);
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------------
    private static byte[] streamToBytes(InputStream inputStream, int length)
    {
        try
        {
            ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
            if(mHelperBytes == null)
            {
                mHelperBytes = new byte[1024 * 1024];
            }
            while(true)
            {
                int len = inputStream.read(mHelperBytes);
                if(len < 0)
                {
                    break;
                }
                outputStream.write(mHelperBytes, 0, len);
            }
            outputStream.close();
            return outputStream.toByteArray();
        }
        catch(Exception e)
        {
            unityError("streamToBytes exception : " + e.getStackTrace());
           return null;
        }
    }
    protected static boolean endsWith(String str, String pattern, boolean caseSensitive)
    {
        if(caseSensitive)
        {
            String newStr = str.toLowerCase();
            return newStr.endsWith(pattern.toLowerCase());
        }
        else
        {
            return str.endsWith(pattern);
        }
    }
    // 查找文件夹,只能查找Assets以外的文件夹
    protected static void findFolders(String path, List<String> fileList, boolean recursive)
    {
        File folder = new File(path);
        if(!path.endsWith("/"))
        {
            path += "/";
        }
        File[] fileInfoList = folder.listFiles();
        int fileCount = fileInfoList.length;
        for (int i = 0; i < fileCount; ++i)
        {
            String fileName = fileInfoList[i].getName();
            if(fileInfoList[i].isDirectory())
            {
                fileList.add(path + fileName);
                // 查找所有子目录
                if (recursive)
                {
                    findFolders(path + fileName, fileList, recursive);
                }
            }
        }
    }
    // 查找文件,只能查找Assets以外的文件
    protected static void findFiles(String path, List<String> fileList, String[] patterns, boolean recursive)
    {
        File folder = new File(path);
        if(!path.endsWith("/"))
        {
            path += "/";
        }
        File[] fileInfoList = folder.listFiles();
        int fileCount = fileInfoList.length;
        int patternCount = patterns != null ? patterns.length : 0;
        for (int i = 0; i < fileCount; ++i)
        {
            File file = fileInfoList[i];
            String fileName = file.getName();
            if(file.isFile())
            {
                // 如果需要过滤后缀名,则判断后缀
                if (patternCount > 0)
                {
                    for (int j = 0; j < patternCount; ++j)
                    {
                        if (endsWith(fileName, patterns[j], false))
                        {
                            fileList.add(path + fileName);
                            break;
                        }
                    }
                }
                // 不需要过滤,则直接放入列表
                else
                {
                    fileList.add(path + fileName);
                }
            }
            else if(file.isDirectory())
            {
                // 查找所有子目录
                if (recursive)
                {
                    findFiles(path + fileName, fileList, patterns, recursive);
                }
            }
        }
    }
    // 查找Asset文件夹
    protected void findAssetFolders(String path, List<String> fileList, boolean recursive)
    {
        try
        {
            // path不能以/结尾
            if(path.endsWith("/"))
            {
                path = path.substring(0, path.length() - 1);
            }
            String[] assetList = mAssetManager.list(path);
            // 添加/是为了递归时方便字符串拼接
            if(!path.equals("") && !path.endsWith("/"))
            {
                path += "/";
            }
            int fileCount = assetList.length;
            for(int i = 0; i < fileCount; ++i)
            {
                String assetName = assetList[i];
                // 包含后缀名的认为是文件,否则认为是文件夹,不考虑文件名不含后缀名的情况
                if(assetName.lastIndexOf(".") == -1)
                {
                    fileList.add(path + assetName);
                    // 查找所有子目录
                    if (recursive)
                    {
                        findAssetFolders(path + assetName, fileList, recursive);
                    }
                }
            }
        }
        catch(Exception e)
        {
            unityError("findAssetFolders exception : " + e.getMessage() + ", stack:" + e.getStackTrace());
        }
    }
    // 查找Asset文件
    protected void findAssets(String path, List<String> fileList, String[] patterns, boolean recursive)
    {
        try
        {
            // path不能以/结尾
            if(path.endsWith("/"))
            {
                path = path.substring(0, path.length() - 1);
            }
            String[] assetList = mAssetManager.list(path);
            // 添加/是为了递归时方便字符串拼接
            if(!path.equals("") && !path.endsWith("/"))
            {
                path += "/";
            }
            int fileCount = assetList.length;
            int patternCount = patterns != null ? patterns.length : 0;
            for(int i = 0; i < fileCount; ++i)
            {
                String assetName = assetList[i];
                // 包含后缀名的认为是文件,否则认为是文件夹,不考虑文件名不含后缀名的情况
                if(assetName.lastIndexOf(".") != -1)
                {
                    // 如果需要过滤后缀名,则判断后缀
                    if (patternCount > 0)
                    {
                        for (int j = 0; j < patternCount; ++j)
                        {
                            if (endsWith(assetName, patterns[j], false))
                            {
                                fileList.add(path + assetName);
                                break;
                            }
                        }
                    }
                    // 不需要过滤,则直接放入列表
                    else
                    {
                        fileList.add(path + assetName);
                    }
                }
                else
                {
                    // 查找所有子目录
                    if (recursive)
                    {
                        findAssets(path + assetName, fileList, patterns, recursive);
                    }
                }
            }
        }
        catch(Exception e)
        {
            unityError("findAssets exception : " + e.getMessage() + ", stack:" + e.getStackTrace());
        }
    }
}
