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
    unsafe class FrameDefine_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(global::FrameDefine);

            field = type.GetField("F_CONFIG_PATH", flag);
            app.RegisterCLRFieldGetter(field, get_F_CONFIG_PATH_0);
            app.RegisterCLRFieldSetter(field, set_F_CONFIG_PATH_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_F_CONFIG_PATH_0, AssignFromStack_F_CONFIG_PATH_0);


        }



        static object get_F_CONFIG_PATH_0(ref object o)
        {
            return global::FrameDefine.F_CONFIG_PATH;
        }

        static StackObject* CopyToStack_F_CONFIG_PATH_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = global::FrameDefine.F_CONFIG_PATH;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_F_CONFIG_PATH_0(ref object o, object v)
        {
            global::FrameDefine.F_CONFIG_PATH = (System.String)v;
        }

        static StackObject* AssignFromStack_F_CONFIG_PATH_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @F_CONFIG_PATH = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            global::FrameDefine.F_CONFIG_PATH = @F_CONFIG_PATH;
            return ptr_of_this_method;
        }



    }
}
