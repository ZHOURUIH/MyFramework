using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class FrameSystemAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo minit_0 = new CrossBindingMethodInfo("init");
        static CrossBindingMethodInfo mlateInit_1 = new CrossBindingMethodInfo("lateInit");
        static CrossBindingMethodInfo mwillDestroy_2 = new CrossBindingMethodInfo("willDestroy");
        static CrossBindingMethodInfo mdestroy_3 = new CrossBindingMethodInfo("destroy");
        static CrossBindingMethodInfo mhotFixInited_4 = new CrossBindingMethodInfo("hotFixInited");
        static CrossBindingMethodInfo mresourceAvailable_5 = new CrossBindingMethodInfo("resourceAvailable");
        static CrossBindingMethodInfo monDrawGizmos_6 = new CrossBindingMethodInfo("onDrawGizmos");
        static CrossBindingMethodInfo<System.Boolean> msetActive_7 = new CrossBindingMethodInfo<System.Boolean>("setActive");
        static CrossBindingMethodInfo<System.Single> mupdate_8 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo<System.Single> mlateUpdate_9 = new CrossBindingMethodInfo<System.Single>("lateUpdate");
        static CrossBindingMethodInfo<System.Single> mfixedUpdate_10 = new CrossBindingMethodInfo<System.Single>("fixedUpdate");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyAddComponent_11 = new CrossBindingMethodInfo<global::GameComponent>("notifyAddComponent");
        static CrossBindingMethodInfo<System.Boolean, System.Boolean> msetIgnoreTimeScale_12 = new CrossBindingMethodInfo<System.Boolean, System.Boolean>("setIgnoreTimeScale");
        static CrossBindingMethodInfo mresetProperty_13 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo minitComponents_14 = new CrossBindingMethodInfo("initComponents");
        static CrossBindingMethodInfo<System.String> msetName_15 = new CrossBindingMethodInfo<System.String>("setName");
        static CrossBindingMethodInfo<System.Boolean> msetDestroy_16 = new CrossBindingMethodInfo<System.Boolean>("setDestroy");
        static CrossBindingFunctionInfo<System.Boolean> misDestroy_17 = new CrossBindingFunctionInfo<System.Boolean>("isDestroy");
        static CrossBindingMethodInfo<System.Int64> msetAssignID_18 = new CrossBindingMethodInfo<System.Int64>("setAssignID");
        static CrossBindingFunctionInfo<System.Int64> mgetAssignID_19 = new CrossBindingFunctionInfo<System.Int64>("getAssignID");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::FrameSystem);
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

        public class Adapter : global::FrameSystem, CrossBindingAdaptorType
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

            public override void lateInit()
            {
                if (mlateInit_1.CheckShouldInvokeBase(this.instance))
                    base.lateInit();
                else
                    mlateInit_1.Invoke(this.instance);
            }

            public override void willDestroy()
            {
                if (mwillDestroy_2.CheckShouldInvokeBase(this.instance))
                    base.willDestroy();
                else
                    mwillDestroy_2.Invoke(this.instance);
            }

            public override void destroy()
            {
                if (mdestroy_3.CheckShouldInvokeBase(this.instance))
                    base.destroy();
                else
                    mdestroy_3.Invoke(this.instance);
            }

            public override void hotFixInited()
            {
                if (mhotFixInited_4.CheckShouldInvokeBase(this.instance))
                    base.hotFixInited();
                else
                    mhotFixInited_4.Invoke(this.instance);
            }

            public override void resourceAvailable()
            {
                if (mresourceAvailable_5.CheckShouldInvokeBase(this.instance))
                    base.resourceAvailable();
                else
                    mresourceAvailable_5.Invoke(this.instance);
            }

            public override void onDrawGizmos()
            {
                if (monDrawGizmos_6.CheckShouldInvokeBase(this.instance))
                    base.onDrawGizmos();
                else
                    monDrawGizmos_6.Invoke(this.instance);
            }

            public override void setActive(System.Boolean active)
            {
                if (msetActive_7.CheckShouldInvokeBase(this.instance))
                    base.setActive(active);
                else
                    msetActive_7.Invoke(this.instance, active);
            }

            public override void update(System.Single elapsedTime)
            {
                if (mupdate_8.CheckShouldInvokeBase(this.instance))
                    base.update(elapsedTime);
                else
                    mupdate_8.Invoke(this.instance, elapsedTime);
            }

            public override void lateUpdate(System.Single elapsedTime)
            {
                if (mlateUpdate_9.CheckShouldInvokeBase(this.instance))
                    base.lateUpdate(elapsedTime);
                else
                    mlateUpdate_9.Invoke(this.instance, elapsedTime);
            }

            public override void fixedUpdate(System.Single elapsedTime)
            {
                if (mfixedUpdate_10.CheckShouldInvokeBase(this.instance))
                    base.fixedUpdate(elapsedTime);
                else
                    mfixedUpdate_10.Invoke(this.instance, elapsedTime);
            }

            public override void notifyAddComponent(global::GameComponent com)
            {
                if (mnotifyAddComponent_11.CheckShouldInvokeBase(this.instance))
                    base.notifyAddComponent(com);
                else
                    mnotifyAddComponent_11.Invoke(this.instance, com);
            }

            public override void setIgnoreTimeScale(System.Boolean ignore, System.Boolean componentOnly)
            {
                if (msetIgnoreTimeScale_12.CheckShouldInvokeBase(this.instance))
                    base.setIgnoreTimeScale(ignore, componentOnly);
                else
                    msetIgnoreTimeScale_12.Invoke(this.instance, ignore, componentOnly);
            }

            public override void resetProperty()
            {
                if (mresetProperty_13.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_13.Invoke(this.instance);
            }

            protected override void initComponents()
            {
                if (minitComponents_14.CheckShouldInvokeBase(this.instance))
                    base.initComponents();
                else
                    minitComponents_14.Invoke(this.instance);
            }

            public override void setName(System.String name)
            {
                if (msetName_15.CheckShouldInvokeBase(this.instance))
                    base.setName(name);
                else
                    msetName_15.Invoke(this.instance, name);
            }

            public override void setDestroy(System.Boolean isDestroy)
            {
                if (msetDestroy_16.CheckShouldInvokeBase(this.instance))
                    base.setDestroy(isDestroy);
                else
                    msetDestroy_16.Invoke(this.instance, isDestroy);
            }

            public override System.Boolean isDestroy()
            {
                if (misDestroy_17.CheckShouldInvokeBase(this.instance))
                    return base.isDestroy();
                else
                    return misDestroy_17.Invoke(this.instance);
            }

            public override void setAssignID(System.Int64 assignID)
            {
                if (msetAssignID_18.CheckShouldInvokeBase(this.instance))
                    base.setAssignID(assignID);
                else
                    msetAssignID_18.Invoke(this.instance, assignID);
            }

            public override System.Int64 getAssignID()
            {
                if (mgetAssignID_19.CheckShouldInvokeBase(this.instance))
                    return base.getAssignID();
                else
                    return mgetAssignID_19.Invoke(this.instance);
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

