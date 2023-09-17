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
    public class GameSceneAdapter : CrossBindingAdaptor
    {
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
            CrossBindingMethodInfo minit_0 = new CrossBindingMethodInfo("init");
            CrossBindingMethodInfo mresetProperty_1 = new CrossBindingMethodInfo("resetProperty");
            CrossBindingMethodInfo mdestroy_2 = new CrossBindingMethodInfo("destroy");
            CrossBindingMethodInfo<System.Single> mupdate_3 = new CrossBindingMethodInfo<System.Single>("update");
            CrossBindingMethodInfo<System.Single> mlateUpdate_4 = new CrossBindingMethodInfo<System.Single>("lateUpdate");
            CrossBindingMethodInfo<System.Single> mkeyProcess_5 = new CrossBindingMethodInfo<System.Single>("keyProcess");
            CrossBindingMethodInfo mexit_6 = new CrossBindingMethodInfo("exit");
            CrossBindingMethodInfo massignStartExitProcedure_7 = new CrossBindingMethodInfo("assignStartExitProcedure");
            CrossBindingMethodInfo mcreateSceneProcedure_8 = new CrossBindingMethodInfo("createSceneProcedure");
            CrossBindingMethodInfo<System.Boolean> msetActive_9 = new CrossBindingMethodInfo<System.Boolean>("setActive");
            CrossBindingMethodInfo<System.Single> mfixedUpdate_10 = new CrossBindingMethodInfo<System.Single>("fixedUpdate");
            CrossBindingMethodInfo<global::GameComponent> mnotifyAddComponent_11 = new CrossBindingMethodInfo<global::GameComponent>("notifyAddComponent");
            CrossBindingMethodInfo<System.Boolean, System.Boolean> msetIgnoreTimeScale_12 = new CrossBindingMethodInfo<System.Boolean, System.Boolean>("setIgnoreTimeScale");
            CrossBindingMethodInfo minitComponents_13 = new CrossBindingMethodInfo("initComponents");
            CrossBindingMethodInfo<System.String> msetName_14 = new CrossBindingMethodInfo<System.String>("setName");

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

            public override void notifyAddComponent(global::GameComponent com)
            {
                if (mnotifyAddComponent_11.CheckShouldInvokeBase(this.instance))
                    base.notifyAddComponent(com);
                else
                    mnotifyAddComponent_11.Invoke(this.instance, com);
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

            public override void setName(System.String name)
            {
                if (msetName_14.CheckShouldInvokeBase(this.instance))
                    base.setName(name);
                else
                    msetName_14.Invoke(this.instance, name);
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

