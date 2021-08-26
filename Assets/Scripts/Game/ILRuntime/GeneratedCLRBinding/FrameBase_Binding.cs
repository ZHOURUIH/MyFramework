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
            FieldInfo field;
            Type[] args;
            Type type = typeof(global::FrameBase);

            field = type.GetField("mLayoutManager", flag);
            app.RegisterCLRFieldGetter(field, get_mLayoutManager_0);
            app.RegisterCLRFieldSetter(field, set_mLayoutManager_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_mLayoutManager_0, AssignFromStack_mLayoutManager_0);
            field = type.GetField("mEventSystem", flag);
            app.RegisterCLRFieldGetter(field, get_mEventSystem_1);
            app.RegisterCLRFieldSetter(field, set_mEventSystem_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_mEventSystem_1, AssignFromStack_mEventSystem_1);
            field = type.GetField("mClassPool", flag);
            app.RegisterCLRFieldGetter(field, get_mClassPool_2);
            app.RegisterCLRFieldSetter(field, set_mClassPool_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_mClassPool_2, AssignFromStack_mClassPool_2);
            field = type.GetField("mClassPoolThread", flag);
            app.RegisterCLRFieldGetter(field, get_mClassPoolThread_3);
            app.RegisterCLRFieldSetter(field, set_mClassPoolThread_3);
            app.RegisterCLRFieldBinding(field, CopyToStack_mClassPoolThread_3, AssignFromStack_mClassPoolThread_3);
            field = type.GetField("mGameFramework", flag);
            app.RegisterCLRFieldGetter(field, get_mGameFramework_4);
            app.RegisterCLRFieldSetter(field, set_mGameFramework_4);
            app.RegisterCLRFieldBinding(field, CopyToStack_mGameFramework_4, AssignFromStack_mGameFramework_4);
            field = type.GetField("mCharacterManager", flag);
            app.RegisterCLRFieldGetter(field, get_mCharacterManager_5);
            app.RegisterCLRFieldSetter(field, set_mCharacterManager_5);
            app.RegisterCLRFieldBinding(field, CopyToStack_mCharacterManager_5, AssignFromStack_mCharacterManager_5);
            field = type.GetField("mGameSceneManager", flag);
            app.RegisterCLRFieldGetter(field, get_mGameSceneManager_6);
            app.RegisterCLRFieldSetter(field, set_mGameSceneManager_6);
            app.RegisterCLRFieldBinding(field, CopyToStack_mGameSceneManager_6, AssignFromStack_mGameSceneManager_6);


        }



        static object get_mLayoutManager_0(ref object o)
        {
            return global::FrameBase.mLayoutManager;
        }

        static StackObject* CopyToStack_mLayoutManager_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = global::FrameBase.mLayoutManager;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_mLayoutManager_0(ref object o, object v)
        {
            global::FrameBase.mLayoutManager = (global::LayoutManager)v;
        }

        static StackObject* AssignFromStack_mLayoutManager_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            global::LayoutManager @mLayoutManager = (global::LayoutManager)typeof(global::LayoutManager).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            global::FrameBase.mLayoutManager = @mLayoutManager;
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
            global::EventSystem @mEventSystem = (global::EventSystem)typeof(global::EventSystem).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            global::FrameBase.mEventSystem = @mEventSystem;
            return ptr_of_this_method;
        }

        static object get_mClassPool_2(ref object o)
        {
            return global::FrameBase.mClassPool;
        }

        static StackObject* CopyToStack_mClassPool_2(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = global::FrameBase.mClassPool;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_mClassPool_2(ref object o, object v)
        {
            global::FrameBase.mClassPool = (global::ClassPool)v;
        }

        static StackObject* AssignFromStack_mClassPool_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            global::ClassPool @mClassPool = (global::ClassPool)typeof(global::ClassPool).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            global::FrameBase.mClassPool = @mClassPool;
            return ptr_of_this_method;
        }

        static object get_mClassPoolThread_3(ref object o)
        {
            return global::FrameBase.mClassPoolThread;
        }

        static StackObject* CopyToStack_mClassPoolThread_3(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = global::FrameBase.mClassPoolThread;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_mClassPoolThread_3(ref object o, object v)
        {
            global::FrameBase.mClassPoolThread = (global::ClassPoolThread)v;
        }

        static StackObject* AssignFromStack_mClassPoolThread_3(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            global::ClassPoolThread @mClassPoolThread = (global::ClassPoolThread)typeof(global::ClassPoolThread).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            global::FrameBase.mClassPoolThread = @mClassPoolThread;
            return ptr_of_this_method;
        }

        static object get_mGameFramework_4(ref object o)
        {
            return global::FrameBase.mGameFramework;
        }

        static StackObject* CopyToStack_mGameFramework_4(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = global::FrameBase.mGameFramework;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_mGameFramework_4(ref object o, object v)
        {
            global::FrameBase.mGameFramework = (global::GameFramework)v;
        }

        static StackObject* AssignFromStack_mGameFramework_4(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            global::GameFramework @mGameFramework = (global::GameFramework)typeof(global::GameFramework).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            global::FrameBase.mGameFramework = @mGameFramework;
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
            global::CharacterManager @mCharacterManager = (global::CharacterManager)typeof(global::CharacterManager).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
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
            global::GameSceneManager @mGameSceneManager = (global::GameSceneManager)typeof(global::GameSceneManager).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            global::FrameBase.mGameSceneManager = @mGameSceneManager;
            return ptr_of_this_method;
        }



    }
}
