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
    public class CommandAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::Command);
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

        public class Adapter : global::Command, CrossBindingAdaptorType
        {
            CrossBindingMethodInfo mresetProperty_0 = new CrossBindingMethodInfo("resetProperty");
            CrossBindingMethodInfo mexecute_1 = new CrossBindingMethodInfo("execute");
            CrossBindingMethodInfo monInterrupted_2 = new CrossBindingMethodInfo("onInterrupted");
            CrossBindingMethodInfo<global::MyStringBuilder> mdebugInfo_3 = new CrossBindingMethodInfo<global::MyStringBuilder>("debugInfo");

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

            public override void resetProperty()
            {
                if (mresetProperty_0.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_0.Invoke(this.instance);
            }

            public override void execute()
            {
                if (mexecute_1.CheckShouldInvokeBase(this.instance))
                    base.execute();
                else
                    mexecute_1.Invoke(this.instance);
            }

            public override void onInterrupted()
            {
                if (monInterrupted_2.CheckShouldInvokeBase(this.instance))
                    base.onInterrupted();
                else
                    monInterrupted_2.Invoke(this.instance);
            }

            public override void debugInfo(global::MyStringBuilder builder)
            {
                if (mdebugInfo_3.CheckShouldInvokeBase(this.instance))
                    base.debugInfo(builder);
                else
                    mdebugInfo_3.Invoke(this.instance, builder);
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
