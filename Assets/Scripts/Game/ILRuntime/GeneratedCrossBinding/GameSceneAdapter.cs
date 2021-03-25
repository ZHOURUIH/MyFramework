using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class GameSceneAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo minit_0 = new CrossBindingMethodInfo("init");
        static CrossBindingMethodInfo mresetProperty_1 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo mdestroy_2 = new CrossBindingMethodInfo("destroy");
        static CrossBindingMethodInfo<System.Single> mupdate_3 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo<System.Single> mlateUpdate_4 = new CrossBindingMethodInfo<System.Single>("lateUpdate");
        static CrossBindingMethodInfo<System.Single> mkeyProcess_5 = new CrossBindingMethodInfo<System.Single>("keyProcess");
        static CrossBindingMethodInfo mexit_6 = new CrossBindingMethodInfo("exit");
        static CrossBindingMethodInfo massignStartExitProcedure_7 = new CrossBindingMethodInfo("assignStartExitProcedure");
        static CrossBindingMethodInfo mcreateSceneProcedure_8 = new CrossBindingMethodInfo("createSceneProcedure");
        static CrossBindingMethodInfo<System.Boolean> msetActive_9 = new CrossBindingMethodInfo<System.Boolean>("setActive");
        static CrossBindingMethodInfo<System.Single> mfixedUpdate_10 = new CrossBindingMethodInfo<System.Single>("fixedUpdate");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyAddComponent_11 = new CrossBindingMethodInfo<global::GameComponent>("notifyAddComponent");
        static CrossBindingMethodInfo<System.Boolean, System.Boolean> msetIgnoreTimeScale_12 = new CrossBindingMethodInfo<System.Boolean, System.Boolean>("setIgnoreTimeScale");
        static CrossBindingMethodInfo minitComponents_13 = new CrossBindingMethodInfo("initComponents");
        static CrossBindingMethodInfo<global::Command> mreceiveCommand_14 = new CrossBindingMethodInfo<global::Command>("receiveCommand");
        static CrossBindingFunctionInfo<System.String> mgetName_15 = new CrossBindingFunctionInfo<System.String>("getName");
        static CrossBindingMethodInfo<System.String> msetName_16 = new CrossBindingMethodInfo<System.String>("setName");
        static CrossBindingMethodInfo<System.Boolean> msetDestroy_17 = new CrossBindingMethodInfo<System.Boolean>("setDestroy");
        static CrossBindingFunctionInfo<System.Boolean> misDestroy_18 = new CrossBindingFunctionInfo<System.Boolean>("isDestroy");
        static CrossBindingMethodInfo<System.UInt64> msetAssignID_19 = new CrossBindingMethodInfo<System.UInt64>("setAssignID");
        static CrossBindingFunctionInfo<System.UInt64> mgetAssignID_20 = new CrossBindingFunctionInfo<System.UInt64>("getAssignID");
        static CrossBindingMethodInfo mnotifyConstructDone_21 = new CrossBindingMethodInfo("notifyConstructDone");
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

            public override void resetProperty()
            {
                if (mresetProperty_1.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_1.Invoke(this.instance);
            }

            public override void destroy()
            {
                if (mdestroy_2.CheckShouldInvokeBase(this.instance))
                    base.destroy();
                else
                    mdestroy_2.Invoke(this.instance);
            }

            public override void update(System.Single elapsedTime)
            {
                if (mupdate_3.CheckShouldInvokeBase(this.instance))
                    base.update(elapsedTime);
                else
                    mupdate_3.Invoke(this.instance, elapsedTime);
            }

            public override void lateUpdate(System.Single elapsedTime)
            {
                if (mlateUpdate_4.CheckShouldInvokeBase(this.instance))
                    base.lateUpdate(elapsedTime);
                else
                    mlateUpdate_4.Invoke(this.instance, elapsedTime);
            }

            public override void keyProcess(System.Single elapsedTime)
            {
                if (mkeyProcess_5.CheckShouldInvokeBase(this.instance))
                    base.keyProcess(elapsedTime);
                else
                    mkeyProcess_5.Invoke(this.instance, elapsedTime);
            }

            public override void exit()
            {
                if (mexit_6.CheckShouldInvokeBase(this.instance))
                    base.exit();
                else
                    mexit_6.Invoke(this.instance);
            }

            public override void assignStartExitProcedure()
            {
                massignStartExitProcedure_7.Invoke(this.instance);
            }

            public override void createSceneProcedure()
            {
                if (mcreateSceneProcedure_8.CheckShouldInvokeBase(this.instance))
                    base.createSceneProcedure();
                else
                    mcreateSceneProcedure_8.Invoke(this.instance);
            }

            public override void setActive(System.Boolean active)
            {
                if (msetActive_9.CheckShouldInvokeBase(this.instance))
                    base.setActive(active);
                else
                    msetActive_9.Invoke(this.instance, active);
            }

            public override void fixedUpdate(System.Single elapsedTime)
            {
                if (mfixedUpdate_10.CheckShouldInvokeBase(this.instance))
                    base.fixedUpdate(elapsedTime);
                else
                    mfixedUpdate_10.Invoke(this.instance, elapsedTime);
            }

            public override void notifyAddComponent(global::GameComponent component)
            {
                if (mnotifyAddComponent_11.CheckShouldInvokeBase(this.instance))
                    base.notifyAddComponent(component);
                else
                    mnotifyAddComponent_11.Invoke(this.instance, component);
            }

            public override void setIgnoreTimeScale(System.Boolean ignore, System.Boolean componentOnly)
            {
                if (msetIgnoreTimeScale_12.CheckShouldInvokeBase(this.instance))
                    base.setIgnoreTimeScale(ignore, componentOnly);
                else
                    msetIgnoreTimeScale_12.Invoke(this.instance, ignore, componentOnly);
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

            public override void setDestroy(System.Boolean isDestroy)
            {
                if (msetDestroy_17.CheckShouldInvokeBase(this.instance))
                    base.setDestroy(isDestroy);
                else
                    msetDestroy_17.Invoke(this.instance, isDestroy);
            }

            public override System.Boolean isDestroy()
            {
                if (misDestroy_18.CheckShouldInvokeBase(this.instance))
                    return base.isDestroy();
                else
                    return misDestroy_18.Invoke(this.instance);
            }

            public override void setAssignID(System.UInt64 assignID)
            {
                if (msetAssignID_19.CheckShouldInvokeBase(this.instance))
                    base.setAssignID(assignID);
                else
                    msetAssignID_19.Invoke(this.instance, assignID);
            }

            public override System.UInt64 getAssignID()
            {
                if (mgetAssignID_20.CheckShouldInvokeBase(this.instance))
                    return base.getAssignID();
                else
                    return mgetAssignID_20.Invoke(this.instance);
            }

            public override void notifyConstructDone()
            {
                if (mnotifyConstructDone_21.CheckShouldInvokeBase(this.instance))
                    base.notifyConstructDone();
                else
                    mnotifyConstructDone_21.Invoke(this.instance);
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

