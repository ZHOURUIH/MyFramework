package 包名;

import android.app.Application;
import android.util.Log;

import com.tencent.bugly.crashreport.CrashReport;

public class MainApplication extends Application
{
    @Override
    public void onCreate()
    {
        super.onCreate();
        CrashReport.initCrashReport(getApplicationContext(), "bugly的AppID", false);
    }
}