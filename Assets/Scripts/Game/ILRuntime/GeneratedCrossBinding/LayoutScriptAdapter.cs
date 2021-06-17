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
        static CrossBindingFunctionInfo<System.Boolean> monESCDown_2 = new CrossBindingFunctionInfo<System.Boolean>("onESCDown");
        static CrossBindingMethodInfo massignWindow_3 = new CrossBindingMethodInfo("assignWindow");
        static CrossBindingMethodInfo minit_4 = new CrossBindingMethodInfo("init");
        static CrossBindingMethodInfo<System.Single> mupdate_5 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo<System.Single> mlateUpdate_6 = new CrossBindingMethodInfo<System.Single>("lateUpdate");
        static CrossBindingMethodInfo monReset_7 = new CrossBindingMethodInfo("onReset");
        static CrossBindingMethodInfo monGameState_8 = new CrossBindingMethodInfo("onGameState");
        static CrossBindingMethodInfo monDrawGizmos_9 = new CrossBindingMethodInfo("onDrawGizmos");
        static CrossBindingMethodInfo<System.Boolean, System.String> monShow_10 = new CrossBindingMethodInfo<System.Boolean, System.String>("onShow");
        static CrossBindingMethodInfo<System.Boolean, System.String> monHide_11 = new CrossBindingMethodInfo<System.Boolean, System.String>("onHide");
        static CrossBindingMethodInfo<global::Command> maddDelayCmd_12 = new CrossBindingMethodInfo<global::Command>("addDelayCmd");
        static CrossBindingMethodInfo<System.Int64, System.Boolean> minterruptCommand_13 = new CrossBindingMethodInfo<System.Int64, System.Boolean>("interruptCommand");
        static CrossBindingMethodInfo<global::Command> monCmdStarted_14 = new CrossBindingMethodInfo<global::Command>("onCmdStarted");
        static CrossBindingMethodInfo minterruptAllCommand_15 = new CrossBindingMethodInfo("interruptAllCommand");
        static CrossBindingMethodInfo mnotifyConstructDone_16 = new CrossBindingMethodInfo("notifyConstructDone");
        static CrossBindingMethodInfo mresetProperty_17 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo<System.Boolean> msetDestroy_18 = new CrossBindingMethodInfo<System.Boolean>("setDestroy");
        static CrossBindingFunctionInfo<System.Boolean> misDestroy_19 = new CrossBindingFunctionInfo<System.Boolean>("isDestroy");
        static CrossBindingMethodInfo<System.Int64> msetAssignID_20 = new CrossBindingMethodInfo<System.Int64>("setAssignID");
        static CrossBindingFunctionInfo<System.Int64> mgetAssignID_21 = new CrossBindingFunctionInfo<System.Int64>("getAssignID");
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

            public override System.Boolean onESCDown()
            {
                if (monESCDown_2.CheckShouldInvokeBase(this.instance))
                    return base.onESCDown();
                else
                    return monESCDown_2.Invoke(this.instance);
            }

            public override void assignWindow()
            {
                massignWindow_3.Invoke(this.instance);
            }

            public override void init()
            {
                if (minit_4.CheckShouldInvokeBase(this.instance))
                    base.init();
                else
                    minit_4.Invoke(this.instance);
            }

            public override void update(System.Single elapsedTime)
            {
                if (mupdate_5.CheckShouldInvokeBase(this.instance))
                    base.update(elapsedTime);
                else
                    mupdate_5.Invoke(this.instance, elapsedTime);
            }

            public override void lateUpdate(System.Single elapsedTime)
            {
                if (mlateUpdate_6.CheckShouldInvokeBase(this.instance))
                    base.lateUpdate(elapsedTime);
                else
                    mlateUpdate_6.Invoke(this.instance, elapsedTime);
            }

            public override void onReset()
            {
                if (monReset_7.CheckShouldInvokeBase(this.instance))
                    base.onReset();
                else
                    monReset_7.Invoke(this.instance);
            }

            public override void onGameState()
            {
                if (monGameState_8.CheckShouldInvokeBase(this.instance))
                    base.onGameState();
                else
                    monGameState_8.Invoke(this.instance);
            }

            public override void onDrawGizmos()
            {
                if (monDrawGizmos_9.CheckShouldInvokeBase(this.instance))
                    base.onDrawGizmos();
                else
                    monDrawGizmos_9.Invoke(this.instance);
            }

            public override void onShow(System.Boolean immediately, System.String param)
            {
                if (monShow_10.CheckShouldInvokeBase(this.instance))
                    base.onShow(immediately, param);
                else
                    monShow_10.Invoke(this.instance, immediately, param);
            }

            public override void onHide(System.Boolean immediately, System.String param)
            {
                if (monHide_11.CheckShouldInvokeBase(this.instance))
                    base.onHide(immediately, param);
                else
                    monHide_11.Invoke(this.instance, immediately, param);
            }

            public override void addDelayCmd(global::Command cmd)
            {
                if (maddDelayCmd_12.CheckShouldInvokeBase(this.instance))
                    base.addDelayCmd(cmd);
                else
                    maddDelayCmd_12.Invoke(this.instance, cmd);
            }

            public override void interruptCommand(System.Int64 assignID, System.Boolean showError)
            {
                if (minterruptCommand_13.CheckShouldInvokeBase(this.instance))
                    base.interruptCommand(assignID, showError);
                else
                    minterruptCommand_13.Invoke(this.instance, assignID, showError);
            }

            public override void onCmdStarted(global::Command cmd)
            {
                if (monCmdStarted_14.CheckShouldInvokeBase(this.instance))
                    base.onCmdStarted(cmd);
                else
                    monCmdStarted_14.Invoke(this.instance, cmd);
            }

            public override void interruptAllCommand()
            {
                if (minterruptAllCommand_15.CheckShouldInvokeBase(this.instance))
                    base.interruptAllCommand();
                else
                    minterruptAllCommand_15.Invoke(this.instance);
            }

            public override void notifyConstructDone()
            {
                if (mnotifyConstructDone_16.CheckShouldInvokeBase(this.instance))
                    base.notifyConstructDone();
                else
                    mnotifyConstructDone_16.Invoke(this.instance);
            }

            public override void resetProperty()
            {
                if (mresetProperty_17.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_17.Invoke(this.instance);
            }

            public override void setDestroy(System.Boolean isDestroy)
            {
                if (msetDestroy_18.CheckShouldInvokeBase(this.instance))
                    base.setDestroy(isDestroy);
                else
                    msetDestroy_18.Invoke(this.instance, isDestroy);
            }

            public override System.Boolean isDestroy()
            {
                if (misDestroy_19.CheckShouldInvokeBase(this.instance))
                    return base.isDestroy();
                else
                    return misDestroy_19.Invoke(this.instance);
            }

            public override void setAssignID(System.Int64 assignID)
            {
                if (msetAssignID_20.CheckShouldInvokeBase(this.instance))
                    base.setAssignID(assignID);
                else
                    msetAssignID_20.Invoke(this.instance, assignID);
            }

            public override System.Int64 getAssignID()
            {
                if (mgetAssignID_21.CheckShouldInvokeBase(this.instance))
                    return base.getAssignID();
                else
                    return mgetAssignID_21.Invoke(this.instance);
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

