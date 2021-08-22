using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class NetConnectTCPAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo<System.Net.IPAddress, System.Int32, System.Single> minit_0 = new CrossBindingMethodInfo<System.Net.IPAddress, System.Int32, System.Single>("init");
        static CrossBindingMethodInfo mresetProperty_1 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo<System.Single> mupdate_2 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo mdestroy_3 = new CrossBindingMethodInfo("destroy");
        static CrossBindingMethodInfo<global::NetPacketTCP> msendPacket_4 = new CrossBindingMethodInfo<global::NetPacketTCP>("sendPacket");
        class packetRead_5Info : CrossBindingMethodInfo
        {
            static Type[] pTypes = new Type[] {typeof(System.Byte[]), typeof(System.Int32), typeof(System.Int32).MakeByRefType(), typeof(global::NetPacketTCP).MakeByRefType(), typeof(global::PARSE_RESULT)};

            public packetRead_5Info()
                : base("packetRead")
            {

            }

            protected override Type ReturnType { get { return typeof(global::PARSE_RESULT); } }

            protected override Type[] Parameters { get { return pTypes; } }
            public global::PARSE_RESULT Invoke(ILTypeInstance instance, System.Byte[] buffer, System.Int32 size, ref System.Int32 index, out global::NetPacketTCP packet)
            {
                EnsureMethod(instance);
                    packet = default(NetPacketTCP);

                if (method != null)
                {
                    invoking = true;
                    global::PARSE_RESULT __res = default(global::PARSE_RESULT);
                    try
                    {
                        using (var ctx = domain.BeginInvoke(method))
                        {
                            ctx.PushObject(index);
                            ctx.PushObject(packet);
                            ctx.PushObject(instance);
                            ctx.PushObject(buffer);
                            ctx.PushInteger(size);
                            ctx.PushReference(0);
                            ctx.PushReference(1);
                            ctx.Invoke();
                            __res = ctx.ReadObject<global::PARSE_RESULT>();
                            index = ctx.ReadObject<System.Int32>(0);
                            packet = ctx.ReadObject<global::NetPacketTCP>(1);
                        }
                    }
                    finally
                    {
                        invoking = false;
                    }
                   return __res;
                }
                else
                    return default(global::PARSE_RESULT);
            }

            public override void Invoke(ILTypeInstance instance)
            {
                throw new NotSupportedException();
            }
        }
        static packetRead_5Info mpacketRead_5 = new packetRead_5Info();
        static CrossBindingMethodInfo<System.String> msetName_6 = new CrossBindingMethodInfo<System.String>("setName");
        static CrossBindingMethodInfo<System.Boolean> msetDestroy_7 = new CrossBindingMethodInfo<System.Boolean>("setDestroy");
        static CrossBindingFunctionInfo<System.Boolean> misDestroy_8 = new CrossBindingFunctionInfo<System.Boolean>("isDestroy");
        static CrossBindingMethodInfo<System.Int64> msetAssignID_9 = new CrossBindingMethodInfo<System.Int64>("setAssignID");
        static CrossBindingFunctionInfo<System.Int64> mgetAssignID_10 = new CrossBindingFunctionInfo<System.Int64>("getAssignID");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::NetConnectTCP);
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

        public class Adapter : global::NetConnectTCP, CrossBindingAdaptorType
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

            public override void sendPacket(global::NetPacketTCP packet)
            {
                msendPacket_4.Invoke(this.instance, packet);
            }

            protected override global::PARSE_RESULT packetRead(System.Byte[] buffer, System.Int32 size, ref System.Int32 index, out global::NetPacketTCP packet)
            {
                return mpacketRead_5.Invoke(this.instance, buffer, size, ref index, out packet);
            }

            public override void setName(System.String name)
            {
                if (msetName_6.CheckShouldInvokeBase(this.instance))
                    base.setName(name);
                else
                    msetName_6.Invoke(this.instance, name);
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

