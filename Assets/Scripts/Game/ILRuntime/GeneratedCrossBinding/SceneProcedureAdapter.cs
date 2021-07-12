using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class SceneProcedureAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo mdestroy_0 = new CrossBindingMethodInfo("destroy");
        static CrossBindingMethodInfo<global::SceneProcedure, System.String> monInitFromChild_1 = new CrossBindingMethodInfo<global::SceneProcedure, System.String>("onInitFromChild");
        static CrossBindingMethodInfo<global::SceneProcedure, System.String> monInit_2 = new CrossBindingMethodInfo<global::SceneProcedure, System.String>("onInit");
        static CrossBindingMethodInfo<System.Single> monUpdate_3 = new CrossBindingMethodInfo<System.Single>("onUpdate");
        static CrossBindingMethodInfo<System.Single> monLateUpdate_4 = new CrossBindingMethodInfo<System.Single>("onLateUpdate");
        static CrossBindingMethodInfo<System.Single> monKeyProcess_5 = new CrossBindingMethodInfo<System.Single>("onKeyProcess");
        static CrossBindingMethodInfo<global::SceneProcedure> monExit_6 = new CrossBindingMethodInfo<global::SceneProcedure>("onExit");
        static CrossBindingMethodInfo<global::SceneProcedure> monExitToChild_7 = new CrossBindingMethodInfo<global::SceneProcedure>("onExitToChild");
        static CrossBindingMethodInfo monExitSelf_8 = new CrossBindingMethodInfo("onExitSelf");
        static CrossBindingMethodInfo<global::SceneProcedure> monNextProcedurePrepared_9 = new CrossBindingMethodInfo<global::SceneProcedure>("onNextProcedurePrepared");
        static CrossBindingMethodInfo<global::SceneProcedure> monPrepareExit_10 = new CrossBindingMethodInfo<global::SceneProcedure>("onPrepareExit");
        static CrossBindingMethodInfo<global::Command> monCmdStarted_11 = new CrossBindingMethodInfo<global::Command>("onCmdStarted");
        static CrossBindingMethodInfo mnotifyConstructDone_12 = new CrossBindingMethodInfo("notifyConstructDone");
        static CrossBindingMethodInfo mresetProperty_13 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo<System.Boolean> msetDestroy_14 = new CrossBindingMethodInfo<System.Boolean>("setDestroy");
        static CrossBindingFunctionInfo<System.Boolean> misDestroy_15 = new CrossBindingFunctionInfo<System.Boolean>("isDestroy");
        static CrossBindingMethodInfo<System.Int64> msetAssignID_16 = new CrossBindingMethodInfo<System.Int64>("setAssignID");
        static CrossBindingFunctionInfo<System.Int64> mgetAssignID_17 = new CrossBindingFunctionInfo<System.Int64>("getAssignID");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::SceneProcedure);
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

        public class Adapter : global::SceneProcedure, CrossBindingAdaptorType
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

            public override void destroy()
            {
                if (mdestroy_0.CheckShouldInvokeBase(this.instance))
                    base.destroy();
                else
                    mdestroy_0.Invoke(this.instance);
            }

            protected override void onInitFromChild(global::SceneProcedure lastProcedure, System.String intent)
            {
                if (monInitFromChild_1.CheckShouldInvokeBase(this.instance))
                    base.onInitFromChild(lastProcedure, intent);
                else
                    monInitFromChild_1.Invoke(this.instance, lastProcedure, intent);
            }

            protected override void onInit(global::SceneProcedure lastProcedure, System.String intent)
            {
                monInit_2.Invoke(this.instance, lastProcedure, intent);
            }

            protected override void onUpdate(System.Single elapsedTime)
            {
                if (monUpdate_3.CheckShouldInvokeBase(this.instance))
                    base.onUpdate(elapsedTime);
                else
                    monUpdate_3.Invoke(this.instance, elapsedTime);
            }

            protected override void onLateUpdate(System.Single elapsedTime)
            {
                if (monLateUpdate_4.CheckShouldInvokeBase(this.instance))
                    base.onLateUpdate(elapsedTime);
                else
                    monLateUpdate_4.Invoke(this.instance, elapsedTime);
            }

            protected override void onKeyProcess(System.Single elapsedTime)
            {
                if (monKeyProcess_5.CheckShouldInvokeBase(this.instance))
                    base.onKeyProcess(elapsedTime);
                else
                    monKeyProcess_5.Invoke(this.instance, elapsedTime);
            }

            protected override void onExit(global::SceneProcedure nextProcedure)
            {
                monExit_6.Invoke(this.instance, nextProcedure);
            }

            protected override void onExitToChild(global::SceneProcedure nextProcedure)
            {
                if (monExitToChild_7.CheckShouldInvokeBase(this.instance))
                    base.onExitToChild(nextProcedure);
                else
                    monExitToChild_7.Invoke(this.instance, nextProcedure);
            }

            protected override void onExitSelf()
            {
                if (monExitSelf_8.CheckShouldInvokeBase(this.instance))
                    base.onExitSelf();
                else
                    monExitSelf_8.Invoke(this.instance);
            }

            public override void onNextProcedurePrepared(global::SceneProcedure nextPreocedure)
            {
                if (monNextProcedurePrepared_9.CheckShouldInvokeBase(this.instance))
                    base.onNextProcedurePrepared(nextPreocedure);
                else
                    monNextProcedurePrepared_9.Invoke(this.instance, nextPreocedure);
            }

            protected override void onPrepareExit(global::SceneProcedure nextPreocedure)
            {
                if (monPrepareExit_10.CheckShouldInvokeBase(this.instance))
                    base.onPrepareExit(nextPreocedure);
                else
                    monPrepareExit_10.Invoke(this.instance, nextPreocedure);
            }

            public override void onCmdStarted(global::Command cmd)
            {
                if (monCmdStarted_11.CheckShouldInvokeBase(this.instance))
                    base.onCmdStarted(cmd);
                else
                    monCmdStarted_11.Invoke(this.instance, cmd);
            }

            public override void notifyConstructDone()
            {
                if (mnotifyConstructDone_12.CheckShouldInvokeBase(this.instance))
                    base.notifyConstructDone();
                else
                    mnotifyConstructDone_12.Invoke(this.instance);
            }

            public override void resetProperty()
            {
                if (mresetProperty_13.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_13.Invoke(this.instance);
            }

            public override void setDestroy(System.Boolean isDestroy)
            {
                if (msetDestroy_14.CheckShouldInvokeBase(this.instance))
                    base.setDestroy(isDestroy);
                else
                    msetDestroy_14.Invoke(this.instance, isDestroy);
            }

            public override System.Boolean isDestroy()
            {
                if (misDestroy_15.CheckShouldInvokeBase(this.instance))
                    return base.isDestroy();
                else
                    return misDestroy_15.Invoke(this.instance);
            }

            public override void setAssignID(System.Int64 assignID)
            {
                if (msetAssignID_16.CheckShouldInvokeBase(this.instance))
                    base.setAssignID(assignID);
                else
                    msetAssignID_16.Invoke(this.instance, assignID);
            }

            public override System.Int64 getAssignID()
            {
                if (mgetAssignID_17.CheckShouldInvokeBase(this.instance))
                    return base.getAssignID();
                else
                    return mgetAssignID_17.Invoke(this.instance);
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

