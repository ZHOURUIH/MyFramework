using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class FrameSystemAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo minit_0 = new CrossBindingMethodInfo("init");
        static CrossBindingMethodInfo mdestroy_1 = new CrossBindingMethodInfo("destroy");
        static CrossBindingMethodInfo monDrawGizmos_2 = new CrossBindingMethodInfo("onDrawGizmos");
        static CrossBindingMethodInfo<System.Boolean> msetActive_3 = new CrossBindingMethodInfo<System.Boolean>("setActive");
        static CrossBindingMethodInfo<System.Single> mupdate_4 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo<System.Single> mlateUpdate_5 = new CrossBindingMethodInfo<System.Single>("lateUpdate");
        static CrossBindingMethodInfo<System.Single> mfixedUpdate_6 = new CrossBindingMethodInfo<System.Single>("fixedUpdate");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyAddComponent_7 = new CrossBindingMethodInfo<global::GameComponent>("notifyAddComponent");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyComponentDetached_8 = new CrossBindingMethodInfo<global::GameComponent>("notifyComponentDetached");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyComponentAttached_9 = new CrossBindingMethodInfo<global::GameComponent>("notifyComponentAttached");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyComponentDestroied_10 = new CrossBindingMethodInfo<global::GameComponent>("notifyComponentDestroied");
        static CrossBindingMethodInfo<System.Boolean, System.Boolean> msetIgnoreTimeScale_11 = new CrossBindingMethodInfo<System.Boolean, System.Boolean>("setIgnoreTimeScale");
        static CrossBindingMethodInfo mresetProperty_12 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo minitComponents_13 = new CrossBindingMethodInfo("initComponents");
        static CrossBindingMethodInfo<global::Command> mreceiveCommand_14 = new CrossBindingMethodInfo<global::Command>("receiveCommand");
        static CrossBindingFunctionInfo<System.String> mgetName_15 = new CrossBindingFunctionInfo<System.String>("getName");
        static CrossBindingMethodInfo<System.String> msetName_16 = new CrossBindingMethodInfo<System.String>("setName");
        static CrossBindingMethodInfo mnotifyConstructDone_17 = new CrossBindingMethodInfo("notifyConstructDone");
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

            public override void destroy()
            {
                if (mdestroy_1.CheckShouldInvokeBase(this.instance))
                    base.destroy();
                else
                    mdestroy_1.Invoke(this.instance);
            }

            public override void onDrawGizmos()
            {
                if (monDrawGizmos_2.CheckShouldInvokeBase(this.instance))
                    base.onDrawGizmos();
                else
                    monDrawGizmos_2.Invoke(this.instance);
            }

            public override void setActive(System.Boolean active)
            {
                if (msetActive_3.CheckShouldInvokeBase(this.instance))
                    base.setActive(active);
                else
                    msetActive_3.Invoke(this.instance, active);
            }

            public override void update(System.Single elapsedTime)
            {
                if (mupdate_4.CheckShouldInvokeBase(this.instance))
                    base.update(elapsedTime);
                else
                    mupdate_4.Invoke(this.instance, elapsedTime);
            }

            public override void lateUpdate(System.Single elapsedTime)
            {
                if (mlateUpdate_5.CheckShouldInvokeBase(this.instance))
                    base.lateUpdate(elapsedTime);
                else
                    mlateUpdate_5.Invoke(this.instance, elapsedTime);
            }

            public override void fixedUpdate(System.Single elapsedTime)
            {
                if (mfixedUpdate_6.CheckShouldInvokeBase(this.instance))
                    base.fixedUpdate(elapsedTime);
                else
                    mfixedUpdate_6.Invoke(this.instance, elapsedTime);
            }

            public override void notifyAddComponent(global::GameComponent component)
            {
                if (mnotifyAddComponent_7.CheckShouldInvokeBase(this.instance))
                    base.notifyAddComponent(component);
                else
                    mnotifyAddComponent_7.Invoke(this.instance, component);
            }

            public override void notifyComponentDetached(global::GameComponent component)
            {
                if (mnotifyComponentDetached_8.CheckShouldInvokeBase(this.instance))
                    base.notifyComponentDetached(component);
                else
                    mnotifyComponentDetached_8.Invoke(this.instance, component);
            }

            public override void notifyComponentAttached(global::GameComponent component)
            {
                if (mnotifyComponentAttached_9.CheckShouldInvokeBase(this.instance))
                    base.notifyComponentAttached(component);
                else
                    mnotifyComponentAttached_9.Invoke(this.instance, component);
            }

            public override void notifyComponentDestroied(global::GameComponent component)
            {
                if (mnotifyComponentDestroied_10.CheckShouldInvokeBase(this.instance))
                    base.notifyComponentDestroied(component);
                else
                    mnotifyComponentDestroied_10.Invoke(this.instance, component);
            }

            public override void setIgnoreTimeScale(System.Boolean ignore, System.Boolean componentOnly)
            {
                if (msetIgnoreTimeScale_11.CheckShouldInvokeBase(this.instance))
                    base.setIgnoreTimeScale(ignore, componentOnly);
                else
                    msetIgnoreTimeScale_11.Invoke(this.instance, ignore, componentOnly);
            }

            public override void resetProperty()
            {
                if (mresetProperty_12.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_12.Invoke(this.instance);
            }

            protected override void initComponents()
            {
                if (minitComponents_13.CheckShouldInvokeBase(this.instance))
                    base.initComponents();
                else
                    minitComponents_13.Invoke(this.instance);
            }

            public override void receiveCommand(global::Command cmd)
            {
                if (mreceiveCommand_14.CheckShouldInvokeBase(this.instance))
                    base.receiveCommand(cmd);
                else
                    mreceiveCommand_14.Invoke(this.instance, cmd);
            }

            public override System.String getName()
            {
                if (mgetName_15.CheckShouldInvokeBase(this.instance))
                    return base.getName();
                else
                    return mgetName_15.Invoke(this.instance);
            }

            public override void setName(System.String name)
            {
                if (msetName_16.CheckShouldInvokeBase(this.instance))
                    base.setName(name);
                else
                    msetName_16.Invoke(this.instance, name);
            }

            public override void notifyConstructDone()
            {
                if (mnotifyConstructDone_17.CheckShouldInvokeBase(this.instance))
                    base.notifyConstructDone();
                else
                    mnotifyConstructDone_17.Invoke(this.instance);
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

