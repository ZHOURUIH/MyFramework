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

