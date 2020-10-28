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
            Dictionary<string, List<MethodInfo>> genericMethods = new Dictionary<string, List<MethodInfo>>();
            List<MethodInfo> lst = null;                    
            foreach(var m in type.GetMethods())
            {
                if(m.IsGenericMethodDefinition)
                {
                    if (!genericMethods.TryGetValue(m.Name, out lst))
                    {
                        lst = new List<MethodInfo>();
                        genericMethods[m.Name] = lst;
                    }
                    lst.Add(m);
                }
            }
            args = new Type[]{typeof(global::CommandCharacterManagerCreateCharacter)};
            if (genericMethods.TryGetValue("newMainCmd", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(global::CommandCharacterManagerCreateCharacter), typeof(global::CommandCharacterManagerCreateCharacter).MakeByRefType(), typeof(System.Boolean), typeof(System.Boolean)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, newMainCmd_3);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(global::Command), typeof(global::CommandReceiver)};
            method = type.GetMethod("pushCommand", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, pushCommand_4);
            args = new Type[]{typeof(global::CommandCharacterManagerDestroy)};
            if (genericMethods.TryGetValue("newMainCmd", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(global::CommandCharacterManagerDestroy), typeof(global::CommandCharacterManagerDestroy).MakeByRefType(), typeof(System.Boolean), typeof(System.Boolean)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, newMainCmd_5);

                        break;
                    }
                }
            }
            args = new Type[]{};
            method = type.GetMethod("notifyConstructDone", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, notifyConstructDone_6);
            args = new Type[]{typeof(System.Type), typeof(System.String)};
            method = type.GetMethod("changeProcedure", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, changeProcedure_7);

            field = type.GetField("mCommandSystem", flag);
            app.RegisterCLRFieldGetter(field, get_mCommandSystem_0);
            app.RegisterCLRFieldSetter(field, set_mCommandSystem_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_mCommandSystem_0, AssignFromStack_mCommandSystem_0);
            field = type.GetField("mCharacterManager", flag);
            app.RegisterCLRFieldGetter(field, get_mCharacterManager_1);
            app.RegisterCLRFieldSetter(field, set_mCharacterManager_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_mCharacterManager_1, AssignFromStack_mCharacterManager_1);
            field = type.GetField("mGameSceneManager", flag);
            app.RegisterCLRFieldGetter(field, get_mGameSceneManager_2);
            app.RegisterCLRFieldSetter(field, set_mGameSceneManager_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_mGameSceneManager_2, AssignFromStack_mGameSceneManager_2);
            field = type.GetField("mLayoutManager", flag);
            app.RegisterCLRFieldGetter(field, get_mLayoutManager_3);
            app.RegisterCLRFieldSetter(field, set_mLayoutManager_3);
            app.RegisterCLRFieldBinding(field, CopyToStack_mLayoutManager_3, AssignFromStack_mLayoutManager_3);


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

        static StackObject* newMainCmd_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @delay = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Boolean @show = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            global::CommandCharacterManagerCreateCharacter @cmd = (global::CommandCharacterManagerCreateCharacter)typeof(global::CommandCharacterManagerCreateCharacter).CheckCLRTypes(__intp.RetriveObject(ptr_of_this_method, __mStack));


            var result_of_this_method = global::FrameBase.newMainCmd<global::CommandCharacterManagerCreateCharacter>(out @cmd, @show, @delay);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.StackObjectReference:
                    {
                        var ___dst = ILIntepreter.ResolveReference(ptr_of_this_method);
                        object ___obj = @cmd;
                        if (___dst->ObjectType >= ObjectTypes.Object)
                        {
                            if (___obj is CrossBindingAdaptorType)
                                ___obj = ((CrossBindingAdaptorType)___obj).ILInstance;
                            __mStack[___dst->Value] = ___obj;
                        }
                        else
                        {
                            ILIntepreter.UnboxObject(___dst, ___obj, __mStack, __domain);
                        }
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = @cmd;
                        }
                        else
                        {
                            var ___type = __domain.GetType(___obj.GetType()) as CLRType;
                            ___type.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, @cmd);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var ___type = __domain.GetType(ptr_of_this_method->Value);
                        if(___type is ILType)
                        {
                            ((ILType)___type).StaticInstance[ptr_of_this_method->ValueLow] = @cmd;
                        }
                        else
                        {
                            ((CLRType)___type).SetStaticFieldValue(ptr_of_this_method->ValueLow, @cmd);
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as global::CommandCharacterManagerCreateCharacter[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = @cmd;
                    }
                    break;
            }

            __intp.Free(ptr_of_this_method);
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* pushCommand_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
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

        static StackObject* newMainCmd_5(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @delay = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Boolean @show = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            global::CommandCharacterManagerDestroy @cmd = (global::CommandCharacterManagerDestroy)typeof(global::CommandCharacterManagerDestroy).CheckCLRTypes(__intp.RetriveObject(ptr_of_this_method, __mStack));


            var result_of_this_method = global::FrameBase.newMainCmd<global::CommandCharacterManagerDestroy>(out @cmd, @show, @delay);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            __intp.Free(ptr_of_this_method);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.StackObjectReference:
                    {
                        var ___dst = ILIntepreter.ResolveReference(ptr_of_this_method);
                        object ___obj = @cmd;
                        if (___dst->ObjectType >= ObjectTypes.Object)
                        {
                            if (___obj is CrossBindingAdaptorType)
                                ___obj = ((CrossBindingAdaptorType)___obj).ILInstance;
                            __mStack[___dst->Value] = ___obj;
                        }
                        else
                        {
                            ILIntepreter.UnboxObject(___dst, ___obj, __mStack, __domain);
                        }
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = @cmd;
                        }
                        else
                        {
                            var ___type = __domain.GetType(___obj.GetType()) as CLRType;
                            ___type.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, @cmd);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var ___type = __domain.GetType(ptr_of_this_method->Value);
                        if(___type is ILType)
                        {
                            ((ILType)___type).StaticInstance[ptr_of_this_method->ValueLow] = @cmd;
                        }
                        else
                        {
                            ((CLRType)___type).SetStaticFieldValue(ptr_of_this_method->ValueLow, @cmd);
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as global::CommandCharacterManagerDestroy[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = @cmd;
                    }
                    break;
            }

            __intp.Free(ptr_of_this_method);
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* notifyConstructDone_6(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
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

        static StackObject* changeProcedure_7(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
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


        static object get_mCommandSystem_0(ref object o)
        {
            return global::FrameBase.mCommandSystem;
        }

        static StackObject* CopyToStack_mCommandSystem_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = global::FrameBase.mCommandSystem;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_mCommandSystem_0(ref object o, object v)
        {
            global::FrameBase.mCommandSystem = (global::CommandSystem)v;
        }

        static StackObject* AssignFromStack_mCommandSystem_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            global::CommandSystem @mCommandSystem = (global::CommandSystem)typeof(global::CommandSystem).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            global::FrameBase.mCommandSystem = @mCommandSystem;
            return ptr_of_this_method;
        }

        static object get_mCharacterManager_1(ref object o)
        {
            return global::FrameBase.mCharacterManager;
        }

        static StackObject* CopyToStack_mCharacterManager_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = global::FrameBase.mCharacterManager;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_mCharacterManager_1(ref object o, object v)
        {
            global::FrameBase.mCharacterManager = (global::CharacterManager)v;
        }

        static StackObject* AssignFromStack_mCharacterManager_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            global::CharacterManager @mCharacterManager = (global::CharacterManager)typeof(global::CharacterManager).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            global::FrameBase.mCharacterManager = @mCharacterManager;
            return ptr_of_this_method;
        }

        static object get_mGameSceneManager_2(ref object o)
        {
            return global::FrameBase.mGameSceneManager;
        }

        static StackObject* CopyToStack_mGameSceneManager_2(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = global::FrameBase.mGameSceneManager;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_mGameSceneManager_2(ref object o, object v)
        {
            global::FrameBase.mGameSceneManager = (global::GameSceneManager)v;
        }

        static StackObject* AssignFromStack_mGameSceneManager_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            global::GameSceneManager @mGameSceneManager = (global::GameSceneManager)typeof(global::GameSceneManager).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            global::FrameBase.mGameSceneManager = @mGameSceneManager;
            return ptr_of_this_method;
        }

        static object get_mLayoutManager_3(ref object o)
        {
            return global::FrameBase.mLayoutManager;
        }

        static StackObject* CopyToStack_mLayoutManager_3(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = global::FrameBase.mLayoutManager;
            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_mLayoutManager_3(ref object o, object v)
        {
            global::FrameBase.mLayoutManager = (global::LayoutManager)v;
        }

        static StackObject* AssignFromStack_mLayoutManager_3(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            global::LayoutManager @mLayoutManager = (global::LayoutManager)typeof(global::LayoutManager).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            global::FrameBase.mLayoutManager = @mLayoutManager;
            return ptr_of_this_method;
        }



    }
}
