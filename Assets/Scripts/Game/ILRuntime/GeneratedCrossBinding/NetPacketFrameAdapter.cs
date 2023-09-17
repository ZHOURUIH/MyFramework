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
    public class NetPacketFrameAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::NetPacketFrame);
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

        public class Adapter : global::NetPacketFrame, CrossBindingAdaptorType
        {
            CrossBindingMethodInfo mresetProperty_0 = new CrossBindingMethodInfo("resetProperty");
            CrossBindingFunctionInfo<System.Boolean> mcanExecute_1 = new CrossBindingFunctionInfo<System.Boolean>("canExecute");
            CrossBindingMethodInfo minit_2 = new CrossBindingMethodInfo("init");
            CrossBindingMethodInfo mexecute_3 = new CrossBindingMethodInfo("execute");
            CrossBindingFunctionInfo<System.String> mdebugInfo_4 = new CrossBindingFunctionInfo<System.String>("debugInfo");
            CrossBindingFunctionInfo<System.Boolean> mshowInfo_5 = new CrossBindingFunctionInfo<System.Boolean>("showInfo");

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

            public override System.Boolean canExecute()
            {
                if (mcanExecute_1.CheckShouldInvokeBase(this.instance))
                    return base.canExecute();
                else
                    return mcanExecute_1.Invoke(this.instance);
            }

            public override void init()
            {
                if (minit_2.CheckShouldInvokeBase(this.instance))
                    base.init();
                else
                    minit_2.Invoke(this.instance);
            }

            public override void execute()
            {
                if (mexecute_3.CheckShouldInvokeBase(this.instance))
                    base.execute();
                else
                    mexecute_3.Invoke(this.instance);
            }

            public override System.String debugInfo()
            {
                if (mdebugInfo_4.CheckShouldInvokeBase(this.instance))
                    return base.debugInfo();
                else
                    return mdebugInfo_4.Invoke(this.instance);
            }

            public override System.Boolean showInfo()
            {
                if (mshowInfo_5.CheckShouldInvokeBase(this.instance))
                    return base.showInfo();
                else
                    return mshowInfo_5.Invoke(this.instance);
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

