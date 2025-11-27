#include "iOSLog.h"

#ifdef __cplusplus
extern "C"
{
#endif
    
    void iOSLog(const char* str)
    {
        NSLog(@"[iOS] %@", [NSString stringWithUTF8String:str]);
    }
    
#ifdef __cplusplus
}
#endif
