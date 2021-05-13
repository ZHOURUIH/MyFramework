using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class LayoutScriptAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo mdestroy_0 = new CrossBindingMethodInfo("destroy");
        static CrossBindingMethodInfo<global::GameLayout> msetLayout_1 = new CrossBindingMethodInfo<global::GameLayout>("setLayout");
        static CrossBindingMethodInfo massignWindow_2 = new CrossBindingMethodInfo("assignWindow");
        static CrossBindingMethodInfo minit_3 = new CrossBindingMethodInfo("init");
        static CrossBindingMethodInfo<System.Single> mupdate_4 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo<System.Single> mlateUpdate_5 = new CrossBindingMethodInfo<System.Single>("lateUpdate");
        static CrossBindingMethodInfo monReset_6 = new CrossBindingMethodInfo("onReset");
        static CrossBindingMethodInfo monGameState_7 = new CrossBindingMethodInfo("onGameState");
        static CrossBindingMethodInfo monDrawGizmos_8 = new CrossBindingMethodInfo("onDrawGizmos");
        static CrossBindingMethodInfo<System.Boolean, System.String> monShow_9 = new CrossBindingMethodInfo<System.Boolean, System.String>("onShow");
        static CrossBindingMethodInfo<System.Boolean, System.String> monHide_10 = new CrossBindingMethodInfo<System.Boolean, System.String>("onHide");
        static CrossBindingMethodInfo<global::Command> maddDelayCmd_11 = new CrossBindingMethodInfo<global::Command>("addDelayCmd");
        static CrossBindingMethodInfo<System.Int64, System.Boolean> minterruptCommand_12 = new CrossBindingMethodInfo<System.Int64, System.Boolean>("interruptCommand");
        static CrossBindingMethodInfo<global::Command> monCmdStarted_13 = new CrossBindingMethodInfo<global::Command>("onCmdStarted");
        static CrossBindingMethodInfo minterruptAllCommand_14 = new CrossBindingMethodInfo("interruptAllCommand");
        static CrossBindingMethodInfo mnotifyConstructDone_15 = new CrossBindingMethodInfo("notifyConstructDone");
        static CrossBindingMethodInfo mresetProperty_16 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo<System.Boolean> msetDestroy_17 = new CrossBindingMethodInfo<System.Boolean>("setDestroy");
        static CrossBindingFunctionInfo<System.Boolean> misDestroy_18 = new CrossBindingFunctionInfo<System.Boolean>("isDestroy");
        static CrossBindingMethodInfo<System.Int64> msetAssignID_19 = new CrossBindingMethodInfo<System.Int64>("setAssignID");
        static CrossBindingFunctionInfo<System.Int64> mgetAssignID_20 = new CrossBindingFunctionInfo<System.Int64>("getAssignID");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::LayoutScript);
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

        public class Adapter : global::LayoutScript, CrossBindingAdaptorType
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

            public override void setLayout(global::GameLayout layout)
            {
                if (msetLayout_1.CheckShouldInvokeBase(this.instance))
                    base.setLayout(layout);
                else
                    msetLayout_1.Invoke(this.instance, layout);
            }

            public override void assignWindow()
            {
                massignWindow_2.Invoke(this.instance);
            }

            public override void init()
            {
                if (minit_3.CheckShouldInvokeBase(this.instance))
                    base.init();
                else
                    minit_3.Invoke(this.instance);
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

            public override void onReset()
            {
                if (monReset_6.CheckShouldInvokeBase(this.instance))
                    base.onReset();
                else
                    monReset_6.Invoke(this.instance);
            }

            public override void onGameState()
            {
                if (monGameState_7.CheckShouldInvokeBase(this.instance))
                    base.onGameState();
                else
                    monGameState_7.Invoke(this.instance);
            }

            public override void onDrawGizmos()
            {
                if (monDrawGizmos_8.CheckShouldInvokeBase(this.instance))
                    base.onDrawGizmos();
                else
                    monDrawGizmos_8.Invoke(this.instance);
            }

            public override void onShow(System.Boolean immediately, System.String param)
            {
                if (monShow_9.CheckShouldInvokeBase(this.instance))
                    base.onShow(immediately, param);
                else
                    monShow_9.Invoke(this.instance, immediately, param);
            }

            public override void onHide(System.Boolean immediately, System.String param)
            {
                if (monHide_10.CheckShouldInvokeBase(this.instance))
                    base.onHide(immediately, param);
                else
                    monHide_10.Invoke(this.instance, immediately, param);
            }

            public override void addDelayCmd(global::Command cmd)
            {
                if (maddDelayCmd_11.CheckShouldInvokeBase(this.instance))
                    base.addDelayCmd(cmd);
                else
                    maddDelayCmd_11.Invoke(this.instance, cmd);
            }

            public override void interruptCommand(System.Int64 assignID, System.Boolean showError)
            {
                if (minterruptCommand_12.CheckShouldInvokeBase(this.instance))
                    base.interruptCommand(assignID, showError);
                else
                    minterruptCommand_12.Invoke(this.instance, assignID, showError);
            }

            public override void onCmdStarted(global::Command cmd)
            {
                if (monCmdStarted_13.CheckShouldInvokeBase(this.instance))
                    base.onCmdStarted(cmd);
                else
                    monCmdStarted_13.Invoke(this.instance, cmd);
            }

            public override void interruptAllCommand()
            {
                if (minterruptAllCommand_14.CheckShouldInvokeBase(this.instance))
                    base.interruptAllCommand();
                else
                    minterruptAllCommand_14.Invoke(this.instance);
            }

            public override void notifyConstructDone()
            {
                if (mnotifyConstructDone_15.CheckShouldInvokeBase(this.instance))
                    base.notifyConstructDone();
                else
                    mnotifyConstructDone_15.Invoke(this.instance);
            }

            public override void resetProperty()
            {
                if (mresetProperty_16.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_16.Invoke(this.instance);
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

            public override void setAssignID(System.Int64 assignID)
            {
                if (msetAssignID_19.CheckShouldInvokeBase(this.instance))
                    base.setAssignID(assignID);
                else
                    msetAssignID_19.Invoke(this.instance, assignID);
            }

            public override System.Int64 getAssignID()
            {
                if (mgetAssignID_20.CheckShouldInvokeBase(this.instance))
                    return base.getAssignID();
                else
                    return mgetAssignID_20.Invoke(this.instance);
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

