using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class CommandAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo minit_0 = new CrossBindingMethodInfo("init");
        static CrossBindingMethodInfo mexecute_1 = new CrossBindingMethodInfo("execute");
        static CrossBindingFunctionInfo<System.String> mshowDebugInfo_2 = new CrossBindingFunctionInfo<System.String>("showDebugInfo");
        static CrossBindingMethodInfo mnotifyConstructDone_3 = new CrossBindingMethodInfo("notifyConstructDone");
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

            public override void init()
            {
                if (minit_0.CheckShouldInvokeBase(this.instance))
                    base.init();
                else
                    minit_0.Invoke(this.instance);
            }

            public override void execute()
            {
                if (mexecute_1.CheckShouldInvokeBase(this.instance))
                    base.execute();
                else
                    mexecute_1.Invoke(this.instance);
            }

            public override System.String showDebugInfo()
            {
                if (mshowDebugInfo_2.CheckShouldInvokeBase(this.instance))
                    return base.showDebugInfo();
                else
                    return mshowDebugInfo_2.Invoke(this.instance);
            }

            public override void notifyConstructDone()
            {
                if (mnotifyConstructDone_3.CheckShouldInvokeBase(this.instance))
                    base.notifyConstructDone();
                else
                    mnotifyConstructDone_3.Invoke(this.instance);
            }

            public override string ToString()
            {
                IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
                m = instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    return instance.ToString();
                }
                else
                    return instance.Type.FullName;
            }
        }
    }
}

