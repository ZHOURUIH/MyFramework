using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
using AutoList = System.Collections.Generic.List<object>;
#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif

namespace HotFix
{   
    public class SerializableBitAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::SerializableBit);
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adapter);
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adapter(appdomain, instance);
        }

        public class Adapter : global::SerializableBit, CrossBindingAdaptorType
        {
            CrossBindingFunctionInfo<global::SerializerBitRead, System.Boolean> mread_0 = new CrossBindingFunctionInfo<global::SerializerBitRead, System.Boolean>("read");
            CrossBindingMethodInfo<global::SerializerBitWrite> mwrite_1 = new CrossBindingMethodInfo<global::SerializerBitWrite>("write");
            CrossBindingMethodInfo mresetProperty_2 = new CrossBindingMethodInfo("resetProperty");

            bool isInvokingToString;
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;

            public Adapter()
            {

            }

            public Adapter(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance { get { return instance; } }

            public override System.Boolean read(global::SerializerBitRead reader)
            {
                return mread_0.Invoke(this.instance, reader);
            }

            public override void write(global::SerializerBitWrite writer)
            {
                mwrite_1.Invoke(this.instance, writer);
            }

            public override void resetProperty()
            {
                if (mresetProperty_2.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_2.Invoke(this.instance);
            }

            public override string ToString()
            {
                IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
                m = instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    if (!isInvokingToString)
                    {
                        isInvokingToString = true;
                        string res = instance.ToString();
                        isInvokingToString = false;
                        return res;
                    }
                    else
                        return instance.Type.FullName;
                }
                else
                    return instance.Type.FullName;
            }
        }
    }
}

