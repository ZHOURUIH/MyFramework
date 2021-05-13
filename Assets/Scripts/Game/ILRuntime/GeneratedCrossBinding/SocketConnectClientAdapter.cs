using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class SocketConnectClientAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo<System.Net.IPAddress, System.Int32, System.Single> minit_0 = new CrossBindingMethodInfo<System.Net.IPAddress, System.Int32, System.Single>("init");
        static CrossBindingMethodInfo mresetProperty_1 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo<System.Single> mupdate_2 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo mdestroy_3 = new CrossBindingMethodInfo("destroy");
        static CrossBindingFunctionInfo<System.Int32, System.Boolean> mcheckPacketType_4 = new CrossBindingFunctionInfo<System.Int32, System.Boolean>("checkPacketType");
        static CrossBindingMethodInfo mheartBeat_5 = new CrossBindingMethodInfo("heartBeat");
        static CrossBindingMethodInfo<global::Command> mreceiveCommand_6 = new CrossBindingMethodInfo<global::Command>("receiveCommand");
        static CrossBindingFunctionInfo<System.String> mgetName_7 = new CrossBindingFunctionInfo<System.String>("getName");
        static CrossBindingMethodInfo<System.String> msetName_8 = new CrossBindingMethodInfo<System.String>("setName");
        static CrossBindingMethodInfo mnotifyConstructDone_9 = new CrossBindingMethodInfo("notifyConstructDone");
        static CrossBindingMethodInfo<System.Boolean> msetDestroy_10 = new CrossBindingMethodInfo<System.Boolean>("setDestroy");
        static CrossBindingFunctionInfo<System.Boolean> misDestroy_11 = new CrossBindingFunctionInfo<System.Boolean>("isDestroy");
        static CrossBindingMethodInfo<System.Int64> msetAssignID_12 = new CrossBindingMethodInfo<System.Int64>("setAssignID");
        static CrossBindingFunctionInfo<System.Int64> mgetAssignID_13 = new CrossBindingFunctionInfo<System.Int64>("getAssignID");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::SocketConnectClient);
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

        public class Adapter : global::SocketConnectClient, CrossBindingAdaptorType
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

            public override void init(System.Net.IPAddress ip, System.Int32 port, System.Single heartBeatTimeOut)
            {
                if (minit_0.CheckShouldInvokeBase(this.instance))
                    base.init(ip, port, heartBeatTimeOut);
                else
                    minit_0.Invoke(this.instance, ip, port, heartBeatTimeOut);
            }

            public override void resetProperty()
            {
                if (mresetProperty_1.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_1.Invoke(this.instance);
            }

            public override void update(System.Single elapsedTime)
            {
                if (mupdate_2.CheckShouldInvokeBase(this.instance))
                    base.update(elapsedTime);
                else
                    mupdate_2.Invoke(this.instance, elapsedTime);
            }

            public override void destroy()
            {
                if (mdestroy_3.CheckShouldInvokeBase(this.instance))
                    base.destroy();
                else
                    mdestroy_3.Invoke(this.instance);
            }

            protected override System.Boolean checkPacketType(System.Int32 type)
            {
                return mcheckPacketType_4.Invoke(this.instance, type);
            }

            protected override void heartBeat()
            {
                mheartBeat_5.Invoke(this.instance);
            }

            public override void receiveCommand(global::Command cmd)
            {
                if (mreceiveCommand_6.CheckShouldInvokeBase(this.instance))
                    base.receiveCommand(cmd);
                else
                    mreceiveCommand_6.Invoke(this.instance, cmd);
            }

            public override System.String getName()
            {
                if (mgetName_7.CheckShouldInvokeBase(this.instance))
                    return base.getName();
                else
                    return mgetName_7.Invoke(this.instance);
            }

            public override void setName(System.String name)
            {
                if (msetName_8.CheckShouldInvokeBase(this.instance))
                    base.setName(name);
                else
                    msetName_8.Invoke(this.instance, name);
            }

            public override void notifyConstructDone()
            {
                if (mnotifyConstructDone_9.CheckShouldInvokeBase(this.instance))
                    base.notifyConstructDone();
                else
                    mnotifyConstructDone_9.Invoke(this.instance);
            }

            public override void setDestroy(System.Boolean isDestroy)
            {
                if (msetDestroy_10.CheckShouldInvokeBase(this.instance))
                    base.setDestroy(isDestroy);
                else
                    msetDestroy_10.Invoke(this.instance, isDestroy);
            }

            public override System.Boolean isDestroy()
            {
                if (misDestroy_11.CheckShouldInvokeBase(this.instance))
                    return base.isDestroy();
                else
                    return misDestroy_11.Invoke(this.instance);
            }

            public override void setAssignID(System.Int64 assignID)
            {
                if (msetAssignID_12.CheckShouldInvokeBase(this.instance))
                    base.setAssignID(assignID);
                else
                    msetAssignID_12.Invoke(this.instance, assignID);
            }

            public override System.Int64 getAssignID()
            {
                if (mgetAssignID_13.CheckShouldInvokeBase(this.instance))
                    return base.getAssignID();
                else
                    return mgetAssignID_13.Invoke(this.instance);
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

