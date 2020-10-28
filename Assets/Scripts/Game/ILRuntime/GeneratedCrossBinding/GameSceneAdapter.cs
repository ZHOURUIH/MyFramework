using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class GameSceneAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo minit_0 = new CrossBindingMethodInfo("init");
        static CrossBindingMethodInfo mdestroy_1 = new CrossBindingMethodInfo("destroy");
        static CrossBindingMethodInfo<System.Single> mupdate_2 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo<System.Single> mlateUpdate_3 = new CrossBindingMethodInfo<System.Single>("lateUpdate");
        static CrossBindingMethodInfo<System.Single> mkeyProcess_4 = new CrossBindingMethodInfo<System.Single>("keyProcess");
        static CrossBindingMethodInfo mexit_5 = new CrossBindingMethodInfo("exit");
        static CrossBindingMethodInfo massignStartExitProcedure_6 = new CrossBindingMethodInfo("assignStartExitProcedure");
        static CrossBindingMethodInfo mcreateSceneProcedure_7 = new CrossBindingMethodInfo("createSceneProcedure");
        static CrossBindingMethodInfo<System.Boolean> msetActive_8 = new CrossBindingMethodInfo<System.Boolean>("setActive");
        static CrossBindingMethodInfo<System.Single> mfixedUpdate_9 = new CrossBindingMethodInfo<System.Single>("fixedUpdate");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyAddComponent_10 = new CrossBindingMethodInfo<global::GameComponent>("notifyAddComponent");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyComponentDetached_11 = new CrossBindingMethodInfo<global::GameComponent>("notifyComponentDetached");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyComponentAttached_12 = new CrossBindingMethodInfo<global::GameComponent>("notifyComponentAttached");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyComponentDestroied_13 = new CrossBindingMethodInfo<global::GameComponent>("notifyComponentDestroied");
        static CrossBindingMethodInfo<System.Boolean, System.Boolean> msetIgnoreTimeScale_14 = new CrossBindingMethodInfo<System.Boolean, System.Boolean>("setIgnoreTimeScale");
        static CrossBindingMethodInfo mresetProperty_15 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo minitComponents_16 = new CrossBindingMethodInfo("initComponents");
        static CrossBindingMethodInfo<global::Command> mreceiveCommand_17 = new CrossBindingMethodInfo<global::Command>("receiveCommand");
        static CrossBindingFunctionInfo<System.String> mgetName_18 = new CrossBindingFunctionInfo<System.String>("getName");
        static CrossBindingMethodInfo<System.String> msetName_19 = new CrossBindingMethodInfo<System.String>("setName");
        static CrossBindingMethodInfo mnotifyConstructDone_20 = new CrossBindingMethodInfo("notifyConstructDone");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::GameScene);
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

        public class Adapter : global::GameScene, CrossBindingAdaptorType
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

            public override void update(System.Single elapsedTime)
            {
                if (mupdate_2.CheckShouldInvokeBase(this.instance))
                    base.update(elapsedTime);
                else
                    mupdate_2.Invoke(this.instance, elapsedTime);
            }

            public override void lateUpdate(System.Single elapsedTime)
            {
                if (mlateUpdate_3.CheckShouldInvokeBase(this.instance))
                    base.lateUpdate(elapsedTime);
                else
                    mlateUpdate_3.Invoke(this.instance, elapsedTime);
            }

            public override void keyProcess(System.Single elapsedTime)
            {
                if (mkeyProcess_4.CheckShouldInvokeBase(this.instance))
                    base.keyProcess(elapsedTime);
                else
                    mkeyProcess_4.Invoke(this.instance, elapsedTime);
            }

            public override void exit()
            {
                if (mexit_5.CheckShouldInvokeBase(this.instance))
                    base.exit();
                else
                    mexit_5.Invoke(this.instance);
            }

            public override void assignStartExitProcedure()
            {
                massignStartExitProcedure_6.Invoke(this.instance);
            }

            public override void createSceneProcedure()
            {
                if (mcreateSceneProcedure_7.CheckShouldInvokeBase(this.instance))
                    base.createSceneProcedure();
                else
                    mcreateSceneProcedure_7.Invoke(this.instance);
            }

            public override void setActive(System.Boolean active)
            {
                if (msetActive_8.CheckShouldInvokeBase(this.instance))
                    base.setActive(active);
                else
                    msetActive_8.Invoke(this.instance, active);
            }

            public override void fixedUpdate(System.Single elapsedTime)
            {
                if (mfixedUpdate_9.CheckShouldInvokeBase(this.instance))
                    base.fixedUpdate(elapsedTime);
                else
                    mfixedUpdate_9.Invoke(this.instance, elapsedTime);
            }

            public override void notifyAddComponent(global::GameComponent component)
            {
                if (mnotifyAddComponent_10.CheckShouldInvokeBase(this.instance))
                    base.notifyAddComponent(component);
                else
                    mnotifyAddComponent_10.Invoke(this.instance, component);
            }

            public override void notifyComponentDetached(global::GameComponent component)
            {
                if (mnotifyComponentDetached_11.CheckShouldInvokeBase(this.instance))
                    base.notifyComponentDetached(component);
                else
                    mnotifyComponentDetached_11.Invoke(this.instance, component);
            }

            public override void notifyComponentAttached(global::GameComponent component)
            {
                if (mnotifyComponentAttached_12.CheckShouldInvokeBase(this.instance))
                    base.notifyComponentAttached(component);
                else
                    mnotifyComponentAttached_12.Invoke(this.instance, component);
            }

            public override void notifyComponentDestroied(global::GameComponent component)
            {
                if (mnotifyComponentDestroied_13.CheckShouldInvokeBase(this.instance))
                    base.notifyComponentDestroied(component);
                else
                    mnotifyComponentDestroied_13.Invoke(this.instance, component);
            }

            public override void setIgnoreTimeScale(System.Boolean ignore, System.Boolean componentOnly)
            {
                if (msetIgnoreTimeScale_14.CheckShouldInvokeBase(this.instance))
                    base.setIgnoreTimeScale(ignore, componentOnly);
                else
                    msetIgnoreTimeScale_14.Invoke(this.instance, ignore, componentOnly);
            }

            public override void resetProperty()
            {
                if (mresetProperty_15.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_15.Invoke(this.instance);
            }

            protected override void initComponents()
            {
                if (minitComponents_16.CheckShouldInvokeBase(this.instance))
                    base.initComponents();
                else
                    minitComponents_16.Invoke(this.instance);
            }

            public override void receiveCommand(global::Command cmd)
            {
                if (mreceiveCommand_17.CheckShouldInvokeBase(this.instance))
                    base.receiveCommand(cmd);
                else
                    mreceiveCommand_17.Invoke(this.instance, cmd);
            }

            public override System.String getName()
            {
                if (mgetName_18.CheckShouldInvokeBase(this.instance))
                    return base.getName();
                else
                    return mgetName_18.Invoke(this.instance);
            }

            public override void setName(System.String name)
            {
                if (msetName_19.CheckShouldInvokeBase(this.instance))
                    base.setName(name);
                else
                    msetName_19.Invoke(this.instance, name);
            }

            public override void notifyConstructDone()
            {
                if (mnotifyConstructDone_20.CheckShouldInvokeBase(this.instance))
                    base.notifyConstructDone();
                else
                    mnotifyConstructDone_20.Invoke(this.instance);
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

