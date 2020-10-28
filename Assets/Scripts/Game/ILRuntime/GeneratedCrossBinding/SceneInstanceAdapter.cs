using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class SceneInstanceAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo minit_0 = new CrossBindingMethodInfo("init");
        static CrossBindingMethodInfo mdestroy_1 = new CrossBindingMethodInfo("destroy");
        static CrossBindingMethodInfo<System.Single> mupdate_2 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo monShow_3 = new CrossBindingMethodInfo("onShow");
        static CrossBindingMethodInfo monHide_4 = new CrossBindingMethodInfo("onHide");
        static CrossBindingMethodInfo mfindGameObject_5 = new CrossBindingMethodInfo("findGameObject");
        static CrossBindingMethodInfo minitGameObject_6 = new CrossBindingMethodInfo("initGameObject");
        static CrossBindingMethodInfo mnotifyConstructDone_7 = new CrossBindingMethodInfo("notifyConstructDone");
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

            public override void onShow()
            {
                if (monShow_3.CheckShouldInvokeBase(this.instance))
                    base.onShow();
                else
                    monShow_3.Invoke(this.instance);
            }

            public override void onHide()
            {
                if (monHide_4.CheckShouldInvokeBase(this.instance))
                    base.onHide();
                else
                    monHide_4.Invoke(this.instance);
            }

            protected override void findGameObject()
            {
                if (mfindGameObject_5.CheckShouldInvokeBase(this.instance))
                    base.findGameObject();
                else
                    mfindGameObject_5.Invoke(this.instance);
            }

            protected override void initGameObject()
            {
                if (minitGameObject_6.CheckShouldInvokeBase(this.instance))
                    base.initGameObject();
                else
                    minitGameObject_6.Invoke(this.instance);
            }

            public override void notifyConstructDone()
            {
                if (mnotifyConstructDone_7.CheckShouldInvokeBase(this.instance))
                    base.notifyConstructDone();
                else
                    mnotifyConstructDone_7.Invoke(this.instance);
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

