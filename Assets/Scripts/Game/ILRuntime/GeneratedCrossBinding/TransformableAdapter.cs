using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class TransformableAdapter : CrossBindingAdaptor
    {
        static CrossBindingFunctionInfo<System.Boolean> misActive_0 = new CrossBindingFunctionInfo<System.Boolean>("isActive");
        static CrossBindingFunctionInfo<System.Boolean> misEnable_1 = new CrossBindingFunctionInfo<System.Boolean>("isEnable");
        static CrossBindingMethodInfo<System.Boolean> msetEnable_2 = new CrossBindingMethodInfo<System.Boolean>("setEnable");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetPosition_3 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getPosition");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetRotation_4 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getRotation");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetScale_5 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getScale");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldPosition_6 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldPosition");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldRotation_7 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldRotation");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldScale_8 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldScale");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetPosition_9 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setPosition");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetRotation_10 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setRotation");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetScale_11 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setScale");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldPosition_12 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldPosition");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldRotation_13 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldRotation");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldScale_14 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldScale");
        static CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3> mlocalToWorld_15 = new CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3>("localToWorld");
        static CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3> mworldToLocal_16 = new CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3>("worldToLocal");
        static CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3> mlocalToWorldDirection_17 = new CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3>("localToWorldDirection");
        static CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3> mworldToLocalDirection_18 = new CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3>("worldToLocalDirection");
        static CrossBindingMethodInfo mdestroy_19 = new CrossBindingMethodInfo("destroy");
        static CrossBindingMethodInfo<System.Boolean> msetActive_20 = new CrossBindingMethodInfo<System.Boolean>("setActive");
        static CrossBindingMethodInfo<System.Single> mupdate_21 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo<System.Single> mlateUpdate_22 = new CrossBindingMethodInfo<System.Single>("lateUpdate");
        static CrossBindingMethodInfo<System.Single> mfixedUpdate_23 = new CrossBindingMethodInfo<System.Single>("fixedUpdate");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyAddComponent_24 = new CrossBindingMethodInfo<global::GameComponent>("notifyAddComponent");
        static CrossBindingMethodInfo<System.Boolean, System.Boolean> msetIgnoreTimeScale_25 = new CrossBindingMethodInfo<System.Boolean, System.Boolean>("setIgnoreTimeScale");
        static CrossBindingMethodInfo mresetProperty_26 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo minitComponents_27 = new CrossBindingMethodInfo("initComponents");
        static CrossBindingMethodInfo<global::Command> mreceiveCommand_28 = new CrossBindingMethodInfo<global::Command>("receiveCommand");
        static CrossBindingFunctionInfo<System.String> mgetName_29 = new CrossBindingFunctionInfo<System.String>("getName");
        static CrossBindingMethodInfo<System.String> msetName_30 = new CrossBindingMethodInfo<System.String>("setName");
        static CrossBindingMethodInfo mnotifyConstructDone_31 = new CrossBindingMethodInfo("notifyConstructDone");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::Transformable);
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

        public class Adapter : global::Transformable, CrossBindingAdaptorType
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

            public override System.Boolean isActive()
            {
                return misActive_0.Invoke(this.instance);
            }

            public override System.Boolean isEnable()
            {
                if (misEnable_1.CheckShouldInvokeBase(this.instance))
                    return base.isEnable();
                else
                    return misEnable_1.Invoke(this.instance);
            }

            public override void setEnable(System.Boolean enable)
            {
                if (msetEnable_2.CheckShouldInvokeBase(this.instance))
                    base.setEnable(enable);
                else
                    msetEnable_2.Invoke(this.instance, enable);
            }

            public override UnityEngine.Vector3 getPosition()
            {
                return mgetPosition_3.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getRotation()
            {
                return mgetRotation_4.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getScale()
            {
                return mgetScale_5.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldPosition()
            {
                return mgetWorldPosition_6.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldRotation()
            {
                return mgetWorldRotation_7.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldScale()
            {
                return mgetWorldScale_8.Invoke(this.instance);
            }

            public override void setPosition(UnityEngine.Vector3 pos)
            {
                msetPosition_9.Invoke(this.instance, pos);
            }

            public override void setRotation(UnityEngine.Vector3 rot)
            {
                msetRotation_10.Invoke(this.instance, rot);
            }

            public override void setScale(UnityEngine.Vector3 scale)
            {
                msetScale_11.Invoke(this.instance, scale);
            }

            public override void setWorldPosition(UnityEngine.Vector3 pos)
            {
                msetWorldPosition_12.Invoke(this.instance, pos);
            }

            public override void setWorldRotation(UnityEngine.Vector3 rot)
            {
                msetWorldRotation_13.Invoke(this.instance, rot);
            }

            public override void setWorldScale(UnityEngine.Vector3 scale)
            {
                msetWorldScale_14.Invoke(this.instance, scale);
            }

            public override UnityEngine.Vector3 localToWorld(UnityEngine.Vector3 point)
            {
                return mlocalToWorld_15.Invoke(this.instance, point);
            }

            public override UnityEngine.Vector3 worldToLocal(UnityEngine.Vector3 point)
            {
                return mworldToLocal_16.Invoke(this.instance, point);
            }

            public override UnityEngine.Vector3 localToWorldDirection(UnityEngine.Vector3 direction)
            {
                return mlocalToWorldDirection_17.Invoke(this.instance, direction);
            }

            public override UnityEngine.Vector3 worldToLocalDirection(UnityEngine.Vector3 direction)
            {
                return mworldToLocalDirection_18.Invoke(this.instance, direction);
            }

            public override void destroy()
            {
                if (mdestroy_19.CheckShouldInvokeBase(this.instance))
                    base.destroy();
                else
                    mdestroy_19.Invoke(this.instance);
            }

            public override void setActive(System.Boolean active)
            {
                if (msetActive_20.CheckShouldInvokeBase(this.instance))
                    base.setActive(active);
                else
                    msetActive_20.Invoke(this.instance, active);
            }

            public override void update(System.Single elapsedTime)
            {
                if (mupdate_21.CheckShouldInvokeBase(this.instance))
                    base.update(elapsedTime);
                else
                    mupdate_21.Invoke(this.instance, elapsedTime);
            }

            public override void lateUpdate(System.Single elapsedTime)
            {
                if (mlateUpdate_22.CheckShouldInvokeBase(this.instance))
                    base.lateUpdate(elapsedTime);
                else
                    mlateUpdate_22.Invoke(this.instance, elapsedTime);
            }

            public override void fixedUpdate(System.Single elapsedTime)
            {
                if (mfixedUpdate_23.CheckShouldInvokeBase(this.instance))
                    base.fixedUpdate(elapsedTime);
                else
                    mfixedUpdate_23.Invoke(this.instance, elapsedTime);
            }

            public override void notifyAddComponent(global::GameComponent component)
            {
                if (mnotifyAddComponent_24.CheckShouldInvokeBase(this.instance))
                    base.notifyAddComponent(component);
                else
                    mnotifyAddComponent_24.Invoke(this.instance, component);
            }

            public override void setIgnoreTimeScale(System.Boolean ignore, System.Boolean componentOnly)
            {
                if (msetIgnoreTimeScale_25.CheckShouldInvokeBase(this.instance))
                    base.setIgnoreTimeScale(ignore, componentOnly);
                else
                    msetIgnoreTimeScale_25.Invoke(this.instance, ignore, componentOnly);
            }

            public override void resetProperty()
            {
                if (mresetProperty_26.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_26.Invoke(this.instance);
            }

            protected override void initComponents()
            {
                if (minitComponents_27.CheckShouldInvokeBase(this.instance))
                    base.initComponents();
                else
                    minitComponents_27.Invoke(this.instance);
            }

            public override void receiveCommand(global::Command cmd)
            {
                if (mreceiveCommand_28.CheckShouldInvokeBase(this.instance))
                    base.receiveCommand(cmd);
                else
                    mreceiveCommand_28.Invoke(this.instance, cmd);
            }

            public override System.String getName()
            {
                if (mgetName_29.CheckShouldInvokeBase(this.instance))
                    return base.getName();
                else
                    return mgetName_29.Invoke(this.instance);
            }

            public override void setName(System.String name)
            {
                if (msetName_30.CheckShouldInvokeBase(this.instance))
                    base.setName(name);
                else
                    msetName_30.Invoke(this.instance, name);
            }

            public override void notifyConstructDone()
            {
                if (mnotifyConstructDone_31.CheckShouldInvokeBase(this.instance))
                    base.notifyConstructDone();
                else
                    mnotifyConstructDone_31.Invoke(this.instance);
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

