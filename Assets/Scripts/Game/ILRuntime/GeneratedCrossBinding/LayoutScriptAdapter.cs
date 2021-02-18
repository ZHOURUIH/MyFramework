using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class LayoutScriptAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo mdestroy_0 = new CrossBindingMethodInfo("destroy");
        static CrossBindingMethodInfo massignWindow_1 = new CrossBindingMethodInfo("assignWindow");
        static CrossBindingMethodInfo minit_2 = new CrossBindingMethodInfo("init");
        static CrossBindingMethodInfo<System.Single> mupdate_3 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo<System.Single> mlateUpdate_4 = new CrossBindingMethodInfo<System.Single>("lateUpdate");
        static CrossBindingMethodInfo monReset_5 = new CrossBindingMethodInfo("onReset");
        static CrossBindingMethodInfo monGameState_6 = new CrossBindingMethodInfo("onGameState");
        static CrossBindingMethodInfo monDrawGizmos_7 = new CrossBindingMethodInfo("onDrawGizmos");
        static CrossBindingMethodInfo<System.Boolean, System.String> monShow_8 = new CrossBindingMethodInfo<System.Boolean, System.String>("onShow");
        static CrossBindingMethodInfo<System.Boolean, System.String> monHide_9 = new CrossBindingMethodInfo<System.Boolean, System.String>("onHide");
        static CrossBindingMethodInfo<global::Command> maddDelayCmd_10 = new CrossBindingMethodInfo<global::Command>("addDelayCmd");
        static CrossBindingMethodInfo<System.Int32, System.Boolean> minterruptCommand_11 = new CrossBindingMethodInfo<System.Int32, System.Boolean>("interruptCommand");
        static CrossBindingMethodInfo<global::Command> monCmdStarted_12 = new CrossBindingMethodInfo<global::Command>("onCmdStarted");
        static CrossBindingMethodInfo minterruptAllCommand_13 = new CrossBindingMethodInfo("interruptAllCommand");
        static CrossBindingMethodInfo mnotifyConstructDone_14 = new CrossBindingMethodInfo("notifyConstructDone");
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

            public override void assignWindow()
            {
                massignWindow_1.Invoke(this.instance);
            }

            public override void init()
            {
                if (minit_2.CheckShouldInvokeBase(this.instance))
                    base.init();
                else
                    minit_2.Invoke(this.instance);
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

            public override void onReset()
            {
                if (monReset_5.CheckShouldInvokeBase(this.instance))
                    base.onReset();
                else
                    monReset_5.Invoke(this.instance);
            }

            public override void onGameState()
            {
                if (monGameState_6.CheckShouldInvokeBase(this.instance))
                    base.onGameState();
                else
                    monGameState_6.Invoke(this.instance);
            }

            public override void onDrawGizmos()
            {
                if (monDrawGizmos_7.CheckShouldInvokeBase(this.instance))
                    base.onDrawGizmos();
                else
                    monDrawGizmos_7.Invoke(this.instance);
            }

            public override void onShow(System.Boolean immediately, System.String param)
            {
                if (monShow_8.CheckShouldInvokeBase(this.instance))
                    base.onShow(immediately, param);
                else
                    monShow_8.Invoke(this.instance, immediately, param);
            }

            public override void onHide(System.Boolean immediately, System.String param)
            {
                if (monHide_9.CheckShouldInvokeBase(this.instance))
                    base.onHide(immediately, param);
                else
                    monHide_9.Invoke(this.instance, immediately, param);
            }

            public override void addDelayCmd(global::Command cmd)
            {
                if (maddDelayCmd_10.CheckShouldInvokeBase(this.instance))
                    base.addDelayCmd(cmd);
                else
                    maddDelayCmd_10.Invoke(this.instance, cmd);
            }

            public override void interruptCommand(System.Int32 assignID, System.Boolean showError)
            {
                if (minterruptCommand_11.CheckShouldInvokeBase(this.instance))
                    base.interruptCommand(assignID, showError);
                else
                    minterruptCommand_11.Invoke(this.instance, assignID, showError);
            }

            public override void onCmdStarted(global::Command cmd)
            {
                if (monCmdStarted_12.CheckShouldInvokeBase(this.instance))
                    base.onCmdStarted(cmd);
                else
                    monCmdStarted_12.Invoke(this.instance, cmd);
            }

            public override void interruptAllCommand()
            {
                if (minterruptAllCommand_13.CheckShouldInvokeBase(this.instance))
                    base.interruptAllCommand();
                else
                    minterruptAllCommand_13.Invoke(this.instance);
            }

            public override void notifyConstructDone()
            {
                if (mnotifyConstructDone_14.CheckShouldInvokeBase(this.instance))
                    base.notifyConstructDone();
                else
                    mnotifyConstructDone_14.Invoke(this.instance);
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

