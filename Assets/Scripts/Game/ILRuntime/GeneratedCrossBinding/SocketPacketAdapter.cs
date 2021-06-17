using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class SocketPacketAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo minit_0 = new CrossBindingMethodInfo("init");
        static CrossBindingMethodInfo mexecute_1 = new CrossBindingMethodInfo("execute");
        static CrossBindingMethodInfo mfillParams_2 = new CrossBindingMethodInfo("fillParams");
        static CrossBindingFunctionInfo<System.String> mdebugInfo_3 = new CrossBindingFunctionInfo<System.String>("debugInfo");
        static CrossBindingFunctionInfo<System.Boolean> mshowInfo_4 = new CrossBindingFunctionInfo<System.Boolean>("showInfo");
        static CrossBindingMethodInfo mresetProperty_5 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo mnotifyConstructDone_6 = new CrossBindingMethodInfo("notifyConstructDone");
        static CrossBindingMethodInfo<System.Boolean> msetDestroy_7 = new CrossBindingMethodInfo<System.Boolean>("setDestroy");
        static CrossBindingFunctionInfo<System.Boolean> misDestroy_8 = new CrossBindingFunctionInfo<System.Boolean>("isDestroy");
        static CrossBindingMethodInfo<System.Int64> msetAssignID_9 = new CrossBindingMethodInfo<System.Int64>("setAssignID");
        static CrossBindingFunctionInfo<System.Int64> mgetAssignID_10 = new CrossBindingFunctionInfo<System.Int64>("getAssignID");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::SocketPacket);
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

        public class Adapter : global::SocketPacket, CrossBindingAdaptorType
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

            protected override void fillParams()
            {
                if (mfillParams_2.CheckShouldInvokeBase(this.instance))
                    base.fillParams();
                else
                    mfillParams_2.Invoke(this.instance);
            }

            public override System.String debugInfo()
            {
                if (mdebugInfo_3.CheckShouldInvokeBase(this.instance))
                    return base.debugInfo();
                else
                    return mdebugInfo_3.Invoke(this.instance);
            }

            public override System.Boolean showInfo()
            {
                if (mshowInfo_4.CheckShouldInvokeBase(this.instance))
                    return base.showInfo();
                else
                    return mshowInfo_4.Invoke(this.instance);
            }

            public override void resetProperty()
            {
                if (mresetProperty_5.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_5.Invoke(this.instance);
            }

            public override void notifyConstructDone()
            {
                if (mnotifyConstructDone_6.CheckShouldInvokeBase(this.instance))
                    base.notifyConstructDone();
                else
                    mnotifyConstructDone_6.Invoke(this.instance);
            }

            public override void setDestroy(System.Boolean isDestroy)
            {
                if (msetDestroy_7.CheckShouldInvokeBase(this.instance))
                    base.setDestroy(isDestroy);
                else
                    msetDestroy_7.Invoke(this.instance, isDestroy);
            }

            public override System.Boolean isDestroy()
            {
                if (misDestroy_8.CheckShouldInvokeBase(this.instance))
                    return base.isDestroy();
                else
                    return misDestroy_8.Invoke(this.instance);
            }

            public override void setAssignID(System.Int64 assignID)
            {
                if (msetAssignID_9.CheckShouldInvokeBase(this.instance))
                    base.setAssignID(assignID);
                else
                    msetAssignID_9.Invoke(this.instance, assignID);
            }

            public override System.Int64 getAssignID()
            {
                if (mgetAssignID_10.CheckShouldInvokeBase(this.instance))
                    return base.getAssignID();
                else
                    return mgetAssignID_10.Invoke(this.instance);
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

