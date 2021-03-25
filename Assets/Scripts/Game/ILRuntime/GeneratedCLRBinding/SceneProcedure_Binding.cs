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
    unsafe class SceneProcedure_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(global::SceneProcedure);
            args = new Type[]{};
            method = type.GetMethod("destroy", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, destroy_0);
            args = new Type[]{typeof(global::SceneProcedure)};
            method = type.GetMethod("onNextProcedurePrepared", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, onNextProcedurePrepared_1);
            args = new Type[]{typeof(global::Command)};
            method = type.GetMethod("addDelayCmd", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, addDelayCmd_2);
            args = new Type[]{typeof(global::Command)};
            method = type.GetMethod("onCmdStarted", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, onCmdStarted_3);
            args = new Type[]{typeof(System.UInt64), typeof(System.Boolean)};
            method = type.GetMethod("interruptCommand", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, interruptCommand_4);
            args = new Type[]{};
            method = type.GetMethod("interruptAllCommand", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, interruptAllCommand_5);


        }


        static StackObject* destroy_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            global::SceneProcedure instance_of_this_method = (global::SceneProcedure)typeof(global::SceneProcedure).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.destroy();

            return __ret;
        }

        static StackObject* onNextProcedurePrepared_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            global::SceneProcedure @nextPreocedure = (global::SceneProcedure)typeof(global::SceneProcedure).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            global::SceneProcedure instance_of_this_method = (global::SceneProcedure)typeof(global::SceneProcedure).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.onNextProcedurePrepared(@nextPreocedure);

            return __ret;
        }

        static StackObject* addDelayCmd_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            global::Command @cmd = (global::Command)typeof(global::Command).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            global::SceneProcedure instance_of_this_method = (global::SceneProcedure)typeof(global::SceneProcedure).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.addDelayCmd(@cmd);

            return __ret;
        }

        static StackObject* onCmdStarted_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            global::Command @cmd = (global::Command)typeof(global::Command).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            global::SceneProcedure instance_of_this_method = (global::SceneProcedure)typeof(global::SceneProcedure).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.onCmdStarted(@cmd);

            return __ret;
        }

        static StackObject* interruptCommand_4(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @showError = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.UInt64 @assignID = *(ulong*)&ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            global::SceneProcedure instance_of_this_method = (global::SceneProcedure)typeof(global::SceneProcedure).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.interruptCommand(@assignID, @showError);

            return __ret;
        }

        static StackObject* interruptAllCommand_5(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            global::SceneProcedure instance_of_this_method = (global::SceneProcedure)typeof(global::SceneProcedure).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.interruptAllCommand();

            return __ret;
        }



    }
}
