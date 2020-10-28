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
    unsafe class CommandCharacterManagerCreateCharacter_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(global::CommandCharacterManagerCreateCharacter);

            field = type.GetField("mCharacterType", flag);
            app.RegisterCLRFieldGetter(field, get_mCharacterType_0);
            app.RegisterCLRFieldSetter(field, set_mCharacterType_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_mCharacterType_0, AssignFromStack_mCharacterType_0);
            field = type.GetField("mName", flag);
            app.RegisterCLRFieldGetter(field, get_mName_1);
            app.RegisterCLRFieldSetter(field, set_mName_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_mName_1, AssignFromStack_mName_1);
            field = type.GetField("mID", flag);
            app.RegisterCLRFieldGetter(field, get_mID_2);
            app.RegisterCLRFieldSetter(field, set_mID_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_mID_2, AssignFromStack_mID_2);


        }



        static object get_mCharacterType_0(ref object o)
        {
            return ((global::CommandCharacterManagerCreateCharacter)o).mCharacterType;
        }

        static StackObject* CopyToStack_mCharacterType_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((global::CommandCharacterManagerCreateCharacter)o).mCharacterType;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_mCharacterType_0(ref object o, object v)
        {
            ((global::CommandCharacterManagerCreateCharacter)o).mCharacterType = (System.Type)v;
        }

        static StackObject* AssignFromStack_mCharacterType_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Type @mCharacterType = (System.Type)typeof(System.Type).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((global::CommandCharacterManagerCreateCharacter)o).mCharacterType = @mCharacterType;
            return ptr_of_this_method;
        }

        static object get_mName_1(ref object o)
        {
            return ((global::CommandCharacterManagerCreateCharacter)o).mName;
        }

        static StackObject* CopyToStack_mName_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((global::CommandCharacterManagerCreateCharacter)o).mName;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_mName_1(ref object o, object v)
        {
            ((global::CommandCharacterManagerCreateCharacter)o).mName = (System.String)v;
        }

        static StackObject* AssignFromStack_mName_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @mName = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack));
            ((global::CommandCharacterManagerCreateCharacter)o).mName = @mName;
            return ptr_of_this_method;
        }

        static object get_mID_2(ref object o)
        {
            return ((global::CommandCharacterManagerCreateCharacter)o).mID;
        }

        static StackObject* CopyToStack_mID_2(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((global::CommandCharacterManagerCreateCharacter)o).mID;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = (int)result_of_this_method;
            return __ret + 1;
        }

        static void set_mID_2(ref object o, object v)
        {
            ((global::CommandCharacterManagerCreateCharacter)o).mID = (System.UInt32)v;
        }

        static StackObject* AssignFromStack_mID_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.UInt32 @mID = (uint)ptr_of_this_method->Value;
            ((global::CommandCharacterManagerCreateCharacter)o).mID = @mID;
            return ptr_of_this_method;
        }



    }
}
