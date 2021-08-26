using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class SceneInstanceAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo minit_0 = new CrossBindingMethodInfo("init");
        static CrossBindingMethodInfo mresetProperty_1 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo mdestroy_2 = new CrossBindingMethodInfo("destroy");
        static CrossBindingMethodInfo<System.Single> mupdate_3 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo monShow_4 = new CrossBindingMethodInfo("onShow");
        static CrossBindingMethodInfo monHide_5 = new CrossBindingMethodInfo("onHide");
        static CrossBindingMethodInfo mfindGameObject_6 = new CrossBindingMethodInfo("findGameObject");
        static CrossBindingMethodInfo minitGameObject_7 = new CrossBindingMethodInfo("initGameObject");
        static CrossBindingMethodInfo<global::Command> monCmdStarted_8 = new CrossBindingMethodInfo<global::Command>("onCmdStarted");
        static CrossBindingMethodInfo<System.Boolean> msetDestroy_9 = new CrossBindingMethodInfo<System.Boolean>("setDestroy");
        static CrossBindingFunctionInfo<System.Boolean> misDestroy_10 = new CrossBindingFunctionInfo<System.Boolean>("isDestroy");
        static CrossBindingMethodInfo<System.Int64> msetAssignID_11 = new CrossBindingMethodInfo<System.Int64>("setAssignID");
        static CrossBindingFunctionInfo<System.Int64> mgetAssignID_12 = new CrossBindingFunctionInfo<System.Int64>("getAssignID");
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

            public override void setDestroy(System.Boolean isDestroy)
            {
                if (msetDestroy_9.CheckShouldInvokeBase(this.instance))
                    base.setDestroy(isDestroy);
                else
                    msetDestroy_9.Invoke(this.instance, isDestroy);
            }

            public override System.Boolean isDestroy()
            {
                if (misDestroy_10.CheckShouldInvokeBase(this.instance))
                    return base.isDestroy();
                else
                    return misDestroy_10.Invoke(this.instance);
            }

            public override void setAssignID(System.Int64 assignID)
            {
                if (msetAssignID_11.CheckShouldInvokeBase(this.instance))
                    base.setAssignID(assignID);
                else
                    msetAssignID_11.Invoke(this.instance, assignID);
            }

            public override System.Int64 getAssignID()
            {
                if (mgetAssignID_12.CheckShouldInvokeBase(this.instance))
                    return base.getAssignID();
                else
                    return mgetAssignID_12.Invoke(this.instance);
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

