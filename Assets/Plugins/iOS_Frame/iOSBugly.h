#ifndef _IOS_LOG_h
#define _IOS_LOG_h

#ifdef __cplusplus
extern "C"
{
#endif

    void reportException(const char* name, const char* reason, const char* stack);

#ifdef __cplusplus
}
#endif

#endif
