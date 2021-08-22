using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class NetPacketAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo minit_0 = new CrossBindingMethodInfo("init");
        static CrossBindingMethodInfo mexecute_1 = new CrossBindingMethodInfo("execute");
        static CrossBindingFunctionInfo<System.String> mdebugInfo_2 = new CrossBindingFunctionInfo<System.String>("debugInfo");
        static CrossBindingFunctionInfo<System.Boolean> mshowInfo_3 = new CrossBindingFunctionInfo<System.Boolean>("showInfo");
        static CrossBindingMethodInfo mresetProperty_4 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo<System.Boolean> msetDestroy_5 = new CrossBindingMethodInfo<System.Boolean>("setDestroy");
        static CrossBindingFunctionInfo<System.Boolean> misDestroy_6 = new CrossBindingFunctionInfo<System.Boolean>("isDestroy");
        static CrossBindingMethodInfo<System.Int64> msetAssignID_7 = new CrossBindingMethodInfo<System.Int64>("setAssignID");
        static CrossBindingFunctionInfo<System.Int64> mgetAssignID_8 = new CrossBindingFunctionInfo<System.Int64>("getAssignID");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::NetPacket);
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

        public class Adapter : global::NetPacket, CrossBindingAdaptorType
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

            public override System.String debugInfo()
            {
                if (mdebugInfo_2.CheckShouldInvokeBase(this.instance))
                    return base.debugInfo();
                else
                    return mdebugInfo_2.Invoke(this.instance);
            }

            public override System.Boolean showInfo()
            {
                if (mshowInfo_3.CheckShouldInvokeBase(this.instance))
                    return base.showInfo();
                else
                    return mshowInfo_3.Invoke(this.instance);
            }

            public override void resetProperty()
            {
                if (mresetProperty_4.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_4.Invoke(this.instance);
            }

            public override void setDestroy(System.Boolean isDestroy)
            {
                if (msetDestroy_5.CheckShouldInvokeBase(this.instance))
                    base.setDestroy(isDestroy);
                else
                    msetDestroy_5.Invoke(this.instance, isDestroy);
            }

            public override System.Boolean isDestroy()
            {
                if (misDestroy_6.CheckShouldInvokeBase(this.instance))
                    return base.isDestroy();
                else
                    return misDestroy_6.Invoke(this.instance);
            }

            public override void setAssignID(System.Int64 assignID)
            {
                if (msetAssignID_7.CheckShouldInvokeBase(this.instance))
                    base.setAssignID(assignID);
                else
                    msetAssignID_7.Invoke(this.instance, assignID);
            }

            public override System.Int64 getAssignID()
            {
                if (mgetAssignID_8.CheckShouldInvokeBase(this.instance))
                    return base.getAssignID();
                else
                    return mgetAssignID_8.Invoke(this.instance);
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

