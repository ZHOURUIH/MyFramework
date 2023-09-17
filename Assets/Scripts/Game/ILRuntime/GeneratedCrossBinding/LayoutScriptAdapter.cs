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
    public class LayoutScriptAdapter : CrossBindingAdaptor
    {
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
            CrossBindingMethodInfo mdestroy_0 = new CrossBindingMethodInfo("destroy");
            CrossBindingMethodInfo mresetProperty_1 = new CrossBindingMethodInfo("resetProperty");
            CrossBindingMethodInfo<global::GameLayout> msetLayout_2 = new CrossBindingMethodInfo<global::GameLayout>("setLayout");
            CrossBindingFunctionInfo<System.Boolean> monESCDown_3 = new CrossBindingFunctionInfo<System.Boolean>("onESCDown");
            CrossBindingMethodInfo massignWindow_4 = new CrossBindingMethodInfo("assignWindow");
            CrossBindingMethodInfo minit_5 = new CrossBindingMethodInfo("init");
            CrossBindingMethodInfo<System.Single> mupdate_6 = new CrossBindingMethodInfo<System.Single>("update");
            CrossBindingMethodInfo<System.Single> mlateUpdate_7 = new CrossBindingMethodInfo<System.Single>("lateUpdate");
            CrossBindingMethodInfo monReset_8 = new CrossBindingMethodInfo("onReset");
            CrossBindingMethodInfo monGameState_9 = new CrossBindingMethodInfo("onGameState");
            CrossBindingMethodInfo monDrawGizmos_10 = new CrossBindingMethodInfo("onDrawGizmos");
            CrossBindingMethodInfo<System.Boolean, System.String> monShow_11 = new CrossBindingMethodInfo<System.Boolean, System.String>("onShow");
            CrossBindingMethodInfo<System.Boolean, System.String> monHide_12 = new CrossBindingMethodInfo<System.Boolean, System.String>("onHide");
            CrossBindingMethodInfo<global::Command> monCmdStarted_13 = new CrossBindingMethodInfo<global::Command>("onCmdStarted");

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

            public override void destroy()
            {
                if (mdestroy_0.CheckShouldInvokeBase(this.instance))
                    base.destroy();
                else
                    mdestroy_0.Invoke(this.instance);
            }

            public override void resetProperty()
            {
                if (mresetProperty_1.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_1.Invoke(this.instance);
            }

            public override void setLayout(global::GameLayout layout)
            {
                if (msetLayout_2.CheckShouldInvokeBase(this.instance))
                    base.setLayout(layout);
                else
                    msetLayout_2.Invoke(this.instance, layout);
            }

            public override System.Boolean onESCDown()
            {
                if (monESCDown_3.CheckShouldInvokeBase(this.instance))
                    return base.onESCDown();
                else
                    return monESCDown_3.Invoke(this.instance);
            }

            public override void assignWindow()
            {
                massignWindow_4.Invoke(this.instance);
            }

            public override void init()
            {
                if (minit_5.CheckShouldInvokeBase(this.instance))
                    base.init();
                else
                    minit_5.Invoke(this.instance);
            }

            public override void update(System.Single elapsedTime)
            {
                if (mupdate_6.CheckShouldInvokeBase(this.instance))
                    base.update(elapsedTime);
                else
                    mupdate_6.Invoke(this.instance, elapsedTime);
            }

            public override void lateUpdate(System.Single elapsedTime)
            {
                if (mlateUpdate_7.CheckShouldInvokeBase(this.instance))
                    base.lateUpdate(elapsedTime);
                else
                    mlateUpdate_7.Invoke(this.instance, elapsedTime);
            }

            public override void onReset()
            {
                if (monReset_8.CheckShouldInvokeBase(this.instance))
                    base.onReset();
                else
                    monReset_8.Invoke(this.instance);
            }

            public override void onGameState()
            {
                if (monGameState_9.CheckShouldInvokeBase(this.instance))
                    base.onGameState();
                else
                    monGameState_9.Invoke(this.instance);
            }

            public override void onDrawGizmos()
            {
                if (monDrawGizmos_10.CheckShouldInvokeBase(this.instance))
                    base.onDrawGizmos();
                else
                    monDrawGizmos_10.Invoke(this.instance);
            }

            public override void onShow(System.Boolean immediately, System.String param)
            {
                if (monShow_11.CheckShouldInvokeBase(this.instance))
                    base.onShow(immediately, param);
                else
                    monShow_11.Invoke(this.instance, immediately, param);
            }

            public override void onHide(System.Boolean immediately, System.String param)
            {
                if (monHide_12.CheckShouldInvokeBase(this.instance))
                    base.onHide(immediately, param);
                else
                    monHide_12.Invoke(this.instance, immediately, param);
            }

            public override void onCmdStarted(global::Command cmd)
            {
                if (monCmdStarted_13.CheckShouldInvokeBase(this.instance))
                    base.onCmdStarted(cmd);
                else
                    monCmdStarted_13.Invoke(this.instance, cmd);
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
