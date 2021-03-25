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
    unsafe class LT_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(global::LT);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("UNLOAD_LAYOUT", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, UNLOAD_LAYOUT_0);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("LOAD_UGUI_SHOW", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, LOAD_UGUI_SHOW_1);
            args = new Type[]{typeof(System.Int32), typeof(System.Boolean), typeof(System.String)};
            method = type.GetMethod("HIDE_LAYOUT", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, HIDE_LAYOUT_2);


        }


        static StackObject* UNLOAD_LAYOUT_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 @id = ptr_of_this_method->Value;


            global::LT.UNLOAD_LAYOUT(@id);

            return __ret;
        }

        static StackObject* LOAD_UGUI_SHOW_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 @id = ptr_of_this_method->Value;


            global::LT.LOAD_UGUI_SHOW(@id);

            return __ret;
        }

        static StackObject* HIDE_LAYOUT_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.String @param = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Boolean @immediately = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Int32 @id = ptr_of_this_method->Value;


            global::LT.HIDE_LAYOUT(@id, @immediately, @param);

            return __ret;
        }



    }
}
