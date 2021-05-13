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
    unsafe class CmdCharacterManagerDestroy_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(global::CmdCharacterManagerDestroy);

            field = type.GetField("mGUID", flag);
            app.RegisterCLRFieldGetter(field, get_mGUID_0);
            app.RegisterCLRFieldSetter(field, set_mGUID_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_mGUID_0, AssignFromStack_mGUID_0);


        }



        static object get_mGUID_0(ref object o)
        {
            return ((global::CmdCharacterManagerDestroy)o).mGUID;
        }

        static StackObject* CopyToStack_mGUID_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((global::CmdCharacterManagerDestroy)o).mGUID;
            __ret->ObjectType = ObjectTypes.Long;
            *(long*)&__ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static void set_mGUID_0(ref object o, object v)
        {
            ((global::CmdCharacterManagerDestroy)o).mGUID = (System.Int64)v;
        }

        static StackObject* AssignFromStack_mGUID_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Int64 @mGUID = *(long*)&ptr_of_this_method->Value;
            ((global::CmdCharacterManagerDestroy)o).mGUID = @mGUID;
            return ptr_of_this_method;
        }



    }
}
