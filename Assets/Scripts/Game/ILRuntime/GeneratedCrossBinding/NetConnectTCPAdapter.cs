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
    public class NetConnectTCPAdapter : CrossBindingAdaptor
    {
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
            CrossBindingMethodInfo<System.Net.IPAddress, System.Int32> minit_0 = new CrossBindingMethodInfo<System.Net.IPAddress, System.Int32>("init");
            CrossBindingMethodInfo mresetProperty_1 = new CrossBindingMethodInfo("resetProperty");
            CrossBindingMethodInfo<System.Single> mupdate_2 = new CrossBindingMethodInfo<System.Single>("update");
            CrossBindingMethodInfo mdestroy_3 = new CrossBindingMethodInfo("destroy");
            CrossBindingMethodInfo<global::NetPacket> msendNetPacket_4 = new CrossBindingMethodInfo<global::NetPacket>("sendNetPacket");
            CrossBindingMethodInfo mclearSocket_5 = new CrossBindingMethodInfo("clearSocket");
            CrossBindingFunctionInfo<System.UInt16, System.Byte[], System.Int32, System.Int32, System.UInt64, global::NetPacket> mparsePacket_6 = new CrossBindingFunctionInfo<System.UInt16, System.Byte[], System.Int32, System.Int32, System.UInt64, global::NetPacket>("parsePacket");
            class preParsePacket_7Info : CrossBindingMethodInfo
            {
                static Type[] pTypes = new Type[] {typeof(System.Byte[]), typeof(System.Int32), typeof(System.Int32).MakeByRefType(), typeof(System.Byte[]).MakeByRefType(), typeof(System.UInt16).MakeByRefType(), typeof(System.Int32).MakeByRefType(), typeof(System.Int32).MakeByRefType(), typeof(System.UInt64).MakeByRefType(), typeof(global::PARSE_RESULT)};

                public preParsePacket_7Info()
                    : base("preParsePacket")
                {

                }

                protected override Type ReturnType { get { return typeof(global::PARSE_RESULT); } }

                protected override Type[] Parameters { get { return pTypes; } }
                public global::PARSE_RESULT Invoke(ILTypeInstance instance, System.Byte[] buffer, System.Int32 size, ref System.Int32 bitIndex, out System.Byte[] outPacketData, out System.UInt16 packetType, out System.Int32 packetSize, out System.Int32 sequence, out System.UInt64 fieldFlag)
                {
                    EnsureMethod(instance);
                    outPacketData = default(System.Byte[]);
                    packetType = default(System.UInt16);
                    packetSize = default(System.Int32);
                    sequence = default(System.Int32);
                    fieldFlag = default(System.UInt64);

                    if (method != null)
                    {
                        invoking = true;
                        global::PARSE_RESULT __res = default(global::PARSE_RESULT);
                        try
                        {
                            using (var ctx = domain.BeginInvoke(method))
                            {
                            ctx.PushInteger(bitIndex);
                            ctx.PushObject(outPacketData);
                            ctx.PushInteger(packetType);
                            ctx.PushInteger(packetSize);
                            ctx.PushInteger(sequence);
                            ctx.PushLong((long)fieldFlag);
                                ctx.PushObject(instance);
                            ctx.PushObject(buffer);
                            ctx.PushInteger(size);
                                ctx.PushReference(0);
                                ctx.PushReference(1);
                                ctx.PushReference(2);
                                ctx.PushReference(3);
                                ctx.PushReference(4);
                                ctx.PushReference(5);
                                ctx.Invoke();
                            __res = ctx.ReadObject<global::PARSE_RESULT>();
                             bitIndex = ctx.ReadInteger(0);
                            outPacketData = ctx.ReadObject<System.Byte[]>(1);
                            packetType = (ushort)ctx.ReadInteger(2);
                             packetSize = ctx.ReadInteger(3);
                             sequence = ctx.ReadInteger(4);
                            fieldFlag = (ulong)ctx.ReadLong(5);
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
            preParsePacket_7Info mpreParsePacket_7 = new preParsePacket_7Info();
            CrossBindingMethodInfo<System.String> msetName_8 = new CrossBindingMethodInfo<System.String>("setName");

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

            public override void init(System.Net.IPAddress ip, System.Int32 port)
            {
                if (minit_0.CheckShouldInvokeBase(this.instance))
                    base.init(ip, port);
                else
                    minit_0.Invoke(this.instance, ip, port);
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

            public override void sendNetPacket(global::NetPacket packet)
            {
                msendNetPacket_4.Invoke(this.instance, packet);
            }

            public override void clearSocket()
            {
                if (mclearSocket_5.CheckShouldInvokeBase(this.instance))
                    base.clearSocket();
                else
                    mclearSocket_5.Invoke(this.instance);
            }

            protected override global::NetPacket parsePacket(System.UInt16 packetType, System.Byte[] buffer, System.Int32 size, System.Int32 sequence, System.UInt64 fieldFlag)
            {
                return mparsePacket_6.Invoke(this.instance, packetType, buffer, size, sequence, fieldFlag);
            }

            protected override global::PARSE_RESULT preParsePacket(System.Byte[] buffer, System.Int32 size, ref System.Int32 bitIndex, out System.Byte[] outPacketData, out System.UInt16 packetType, out System.Int32 packetSize, out System.Int32 sequence, out System.UInt64 fieldFlag)
            {
                return mpreParsePacket_7.Invoke(this.instance, buffer, size, ref bitIndex, out outPacketData, out packetType, out packetSize, out sequence, out fieldFlag);
            }

            public override void setName(System.String name)
            {
                if (msetName_8.CheckShouldInvokeBase(this.instance))
                    base.setName(name);
                else
                    msetName_8.Invoke(this.instance, name);
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

