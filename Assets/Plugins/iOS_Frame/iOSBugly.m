#import <Bugly/Bugly.h>
#include "iOSBugly.h"

#ifdef __cplusplus
extern "C"
{
#endif
    
    void reportException(const char* name, const char* reason, const char* stack)
    {
        NSString* nsName   = [NSString stringWithUTF8String:name];
        NSString* nsReason = [NSString stringWithUTF8String:reason];
        NSArray* stackArray = [[NSString stringWithUTF8String:stack] componentsSeparatedByString:@"\n"];
        NSLog(@"[Bugly] UnityBugly_ReportCSharpException called: %@, %@", nsName, nsReason);
        [Bugly reportExceptionWithCategory:4
                                    name:nsName
                                    reason:nsReason
                                    callStack:stackArray
                                    extraInfo:nil
                                    terminateApp:NO];
    }
	
	void setUserData(const char* name, const char* value)
	{
		if (name == nullptr || value == nullptr)
		{
			return;
		}
		NSString* key   = [NSString stringWithUTF8String:name];
		NSString* val   = [NSString stringWithUTF8String:value];
		NSLog(@"[Bugly] setUserData %@ = %@", key, val);
		[Bugly setUserValue:val forKey:key];
	}
    
#ifdef __cplusplus
}
#endif
