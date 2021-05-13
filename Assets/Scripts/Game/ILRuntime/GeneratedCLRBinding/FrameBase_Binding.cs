using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class FrameBase_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(global::FrameBase);
            args = new Type[]{typeof(UnityEngine.KeyCode), typeof(global::FOCUS_MASK)};
            method = type.GetMethod("getKeyDown", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, getKeyDown_0);
            args = new Type[]{typeof(UnityEngine.KeyCode), typeof(global::FOCUS_MASK)};
            method = type.GetMethod("getKeyCurrentDown", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, getKeyCurrentDown_1);
            args = new Type[]{typeof(System.Type), typeof(System.Single), typeof(System.String)};
            method = type.GetMethod("changeProcedureDelay", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, changeProcedureDelay_2);
            args = new Type[]{typeof(global::Command), typeof(global::CommandReceiver)};
            method = type.GetMethod("pushCommand", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, pushCommand_3);
            args = new Type[]{};
            method = type.GetMethod("notifyConstructDone", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, notifyConstructDone_4);
            args = new Type[]{typeof(System.Type), typeof(System.String)};
            method = type.GetMethod("changeProcedure", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, changeProcedure_5);

            field = type.GetField("mGameFramework", flag);
            app.RegisterCLRFieldGetter(field, get_mGameFramework_0);
            app.RegisterCLRFieldSetter(field, set_mGameFramework_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_mGameFramework_0, AssignFromStack_mGameFramework_0);
            field = type.GetField("mEventSystem", flag);
            app.RegisterCLRFieldGetter(field, get_mEventSystem_1);
            app.RegisterCLRFieldSetter(field, set_mEventSystem_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_mEventSystem_1, AssignFromStack_mEventSystem_1);
            field = type.GetField("mCommandSystem", flag);
            app.RegisterCLRFieldGetter(field, get_mCommandSystem_2);
            app.RegisterCLRFieldSetter(field, set_mCommandSystem_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_mCommandSystem_2, AssignFromStack_mCommandSystem_2);
            field = type.GetField("mClassPool", flag);
            app.RegisterCLRFieldGetter(field, get_mClassPool_3);
            app.RegisterCLRFieldSetter(field, set_mClassPool_3);
            app.RegisterCLRFieldBinding(field, CopyToStack_mClassPool_3, AssignFromStack_mClassPool_3);
            field = type.GetField("mClassPoolThread", flag);
            app.RegisterCLRFieldGetter(field, get_mClassPoolThread_4);
            app.RegisterCLRFieldSetter(field, set_mClassPoolThread_4);
            app.RegisterCLRFieldBinding(field, CopyToStack_mClassPoolThread_4, AssignFromStack_mClassPoolThread_4);
            field = type.GetField("mCharacterManager", flag);
            app.RegisterCLRFieldGetter(field, get_mCharacterManager_5);
            app.RegisterCLRFieldSetter(field, set_mCharacterManager_5);
            app.RegisterCLRFieldBinding(field, CopyToStack_mCharacterManager_5, AssignFromStack_mCharacterManager_5);
            field = type.GetField("mGameSceneManager", flag);
            app.RegisterCLRFieldGetter(field, get_mGameSceneManager_6);
            app.RegisterCLRFieldSetter(field, set_mGameSceneManager_6);
            app.RegisterCLRFieldBinding(field, CopyToStack_mGameSceneManager_6, AssignFromStack_mGameSceneManager_6);
            field = type.GetField("mLayoutManager", flag);
            app.RegisterCLRFieldGetter(field, get_mLayoutManager_7);
            app.RegisterCLRFieldSetter(field, set_mLayoutManager_7);
            app.RegisterCLRFieldBinding(field, CopyToStack_mLayoutManager_7, AssignFromStack_mLayoutManager_7);


        }


        static StackObject* getKeyDown_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            global::FOCUS_MASK @mask = (global::FOCUS_MASK)typeof(global::FOCUS_MASK).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            UnityEngine.KeyCode @key = (UnityEngine.KeyCode)typeof(UnityEngine.KeyCode).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = global::FrameBase.getKeyDown(@key, @mask);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static StackObject* getKeyCurrentDown_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            global::FOCUS_MASK @mask = (global::FOCUS_MASK)typeof(global::FOCUS_MASK).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            UnityEngine.KeyCode @key = (UnityEngine.KeyCode)typeof(UnityEngine.KeyCode).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = global::FrameBase.getKeyCurrentDown(@key, @mask);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static StackObject* changeProcedureDelay_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.String @intent = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Single @delayTime = *(float*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Type @procedure = (System.Type)typeof(System.Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = global::FrameBase.changeProcedureDelay(@procedure, @delayTime, @intent);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* pushCommand_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            global::CommandReceiver @cmdReceiver = (global::CommandReceiver)typeof(global::CommandReceiver).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            global::Command @cmd = (global::Command)typeof(global::Command).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            global::FrameBase.pushCommand(@cmd, @cmdReceiver);

            return __ret;
        }

        static StackObject* notifyConstructDone_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            global::FrameBase instance_of_this_method = (global::FrameBase)typeof(global::FrameBase).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.notifyConstructDone();

            return __ret;
        }

        static StackObject* changeProcedure_5(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.String @intent = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Type @procedure = (System.Type)typeof(System.Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);


            global::FrameBase.changeProcedure(@procedure, @intent);

            return __ret;
        }


        static object get_mGameFramework_0(ref object o)
        {
            return global::FrameBase.mGameFramework;
        }

        static StackObject* CopyToStack_mGameFramework_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = global::FrameBase.mGameFramework;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_mGameFramework_0(ref object o, object v)
        {
            global::FrameBase.mGameFramework = (global::GameFramework)v;
        }

        static StackObject* AssignFromStack_mGameFramework_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            global::GameFramework @mGameFramework = (global::GameFramework)typeof(global::GameFramework).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            global::FrameBase.mGameFramework = @mGameFramework;
            return ptr_of_this_method;
        }

        static object get_mEventSystem_1(ref object o)
        {
            return global::FrameBase.mEventSystem;
        }

        static StackObject* CopyToStack_mEventSystem_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = global::FrameBase.mEventSystem;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_mEventSystem_1(ref object o, object v)
        {
            global::FrameBase.mEventSystem = (global::EventSystem)v;
        }

        static StackObject* AssignFromStack_mEventSystem_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            global::EventSystem @mEventSystem = (global::EventSystem)typeof(global::EventSystem).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            global::FrameBase.mEventSystem = @mEventSystem;
            return ptr_of_this_method;
        }

        static object get_mCommandSystem_2(ref object o)
        {
            return global::FrameBase.mCommandSystem;
        }

        static StackObject* CopyToStack_mCommandSystem_2(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = global::FrameBase.mCommandSystem;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_mCommandSystem_2(ref object o, object v)
        {
            global::FrameBase.mCommandSystem = (global::CommandSystem)v;
        }

        static StackObject* AssignFromStack_mCommandSystem_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            global::CommandSystem @mCommandSystem = (global::CommandSystem)typeof(global::CommandSystem).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            global::FrameBase.mCommandSystem = @mCommandSystem;
            return ptr_of_this_method;
        }

        static object get_mClassPool_3(ref object o)
        {
            return global::FrameBase.mClassPool;
        }

        static StackObject* CopyToStack_mClassPool_3(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = global::FrameBase.mClassPool;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_mClassPool_3(ref object o, object v)
        {
            global::FrameBase.mClassPool = (global::ClassPool)v;
        }

        static StackObject* AssignFromStack_mClassPool_3(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            global::ClassPool @mClassPool = (global::ClassPool)typeof(global::ClassPool).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            global::FrameBase.mClassPool = @mClassPool;
            return ptr_of_this_method;
        }

        static object get_mClassPoolThread_4(ref object o)
        {
            return global::FrameBase.mClassPoolThread;
        }

        static StackObject* CopyToStack_mClassPoolThread_4(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = global::FrameBase.mClassPoolThread;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_mClassPoolThread_4(ref object o, object v)
        {
            global::FrameBase.mClassPoolThread = (global::ClassPoolThread)v;
        }

        static StackObject* AssignFromStack_mClassPoolThread_4(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            global::ClassPoolThread @mClassPoolThread = (global::ClassPoolThread)typeof(global::ClassPoolThread).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            global::FrameBase.mClassPoolThread = @mClassPoolThread;
            return ptr_of_this_method;
        }

        static object get_mCharacterManager_5(ref object o)
        {
            return global::FrameBase.mCharacterManager;
        }

        static StackObject* CopyToStack_mCharacterManager_5(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = global::FrameBase.mCharacterManager;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_mCharacterManager_5(ref object o, object v)
        {
            global::FrameBase.mCharacterManager = (global::CharacterManager)v;
        }

        static StackObject* AssignFromStack_mCharacterManager_5(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            global::CharacterManager @mCharacterManager = (global::CharacterManager)typeof(global::CharacterManager).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            global::FrameBase.mCharacterManager = @mCharacterManager;
            return ptr_of_this_method;
        }

        static object get_mGameSceneManager_6(ref object o)
        {
            return global::FrameBase.mGameSceneManager;
        }

        static StackObject* CopyToStack_mGameSceneManager_6(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = global::FrameBase.mGameSceneManager;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_mGameSceneManager_6(ref object o, object v)
        {
            global::FrameBase.mGameSceneManager = (global::GameSceneManager)v;
        }

        static StackObject* AssignFromStack_mGameSceneManager_6(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            global::GameSceneManager @mGameSceneManager = (global::GameSceneManager)typeof(global::GameSceneManager).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            global::FrameBase.mGameSceneManager = @mGameSceneManager;
            return ptr_of_this_method;
        }

        static object get_mLayoutManager_7(ref object o)
        {
            return global::FrameBase.mLayoutManager;
        }

        static StackObject* CopyToStack_mLayoutManager_7(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = global::FrameBase.mLayoutManager;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_mLayoutManager_7(ref object o, object v)
        {
            global::FrameBase.mLayoutManager = (global::LayoutManager)v;
        }

        static StackObject* AssignFromStack_mLayoutManager_7(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            global::LayoutManager @mLayoutManager = (global::LayoutManager)typeof(global::LayoutManager).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            global::FrameBase.mLayoutManager = @mLayoutManager;
            return ptr_of_this_method;
        }



    }
}
