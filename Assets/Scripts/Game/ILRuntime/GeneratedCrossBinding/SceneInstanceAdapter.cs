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
    public class SceneInstanceAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::SceneInstance);
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

        public class Adapter : global::SceneInstance, CrossBindingAdaptorType
        {
            CrossBindingMethodInfo minit_0 = new CrossBindingMethodInfo("init");
            CrossBindingMethodInfo mresetProperty_1 = new CrossBindingMethodInfo("resetProperty");
            CrossBindingMethodInfo mdestroy_2 = new CrossBindingMethodInfo("destroy");
            CrossBindingMethodInfo<System.Single> mupdate_3 = new CrossBindingMethodInfo<System.Single>("update");
            CrossBindingMethodInfo monShow_4 = new CrossBindingMethodInfo("onShow");
            CrossBindingMethodInfo monHide_5 = new CrossBindingMethodInfo("onHide");
            CrossBindingMethodInfo mfindGameObject_6 = new CrossBindingMethodInfo("findGameObject");
            CrossBindingMethodInfo minitGameObject_7 = new CrossBindingMethodInfo("initGameObject");
            CrossBindingMethodInfo<global::Command> monCmdStarted_8 = new CrossBindingMethodInfo<global::Command>("onCmdStarted");

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

            public override void onShow()
            {
                if (monShow_4.CheckShouldInvokeBase(this.instance))
                    base.onShow();
                else
                    monShow_4.Invoke(this.instance);
            }

            public override void onHide()
            {
                if (monHide_5.CheckShouldInvokeBase(this.instance))
                    base.onHide();
                else
                    monHide_5.Invoke(this.instance);
            }

            protected override void findGameObject()
            {
                if (mfindGameObject_6.CheckShouldInvokeBase(this.instance))
                    base.findGameObject();
                else
                    mfindGameObject_6.Invoke(this.instance);
            }

            protected override void initGameObject()
            {
                if (minitGameObject_7.CheckShouldInvokeBase(this.instance))
                    base.initGameObject();
                else
                    minitGameObject_7.Invoke(this.instance);
            }

            public override void onCmdStarted(global::Command cmd)
            {
                if (monCmdStarted_8.CheckShouldInvokeBase(this.instance))
                    base.onCmdStarted(cmd);
                else
                    monCmdStarted_8.Invoke(this.instance, cmd);
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

