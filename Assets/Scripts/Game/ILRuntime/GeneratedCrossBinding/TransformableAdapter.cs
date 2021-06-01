using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class TransformableAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo mresetProperty_0 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingFunctionInfo<System.Boolean> misActive_1 = new CrossBindingFunctionInfo<System.Boolean>("isActive");
        static CrossBindingFunctionInfo<System.Boolean> misEnable_2 = new CrossBindingFunctionInfo<System.Boolean>("isEnable");
        static CrossBindingMethodInfo<System.Boolean> msetEnable_3 = new CrossBindingMethodInfo<System.Boolean>("setEnable");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetPosition_4 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getPosition");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetRotation_5 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getRotation");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetScale_6 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getScale");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldPosition_7 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldPosition");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldRotation_8 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldRotation");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldScale_9 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldScale");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetPosition_10 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setPosition");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetRotation_11 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setRotation");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetScale_12 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setScale");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldPosition_13 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldPosition");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldRotation_14 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldRotation");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldScale_15 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldScale");
        static CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3> mlocalToWorld_16 = new CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3>("localToWorld");
        static CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3> mworldToLocal_17 = new CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3>("worldToLocal");
        static CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3> mlocalToWorldDirection_18 = new CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3>("localToWorldDirection");
        static CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3> mworldToLocalDirection_19 = new CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3>("worldToLocalDirection");
        static CrossBindingMethodInfo mdestroy_20 = new CrossBindingMethodInfo("destroy");
        static CrossBindingMethodInfo<System.Boolean> msetActive_21 = new CrossBindingMethodInfo<System.Boolean>("setActive");
        static CrossBindingMethodInfo<System.Single> mupdate_22 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo<System.Single> mlateUpdate_23 = new CrossBindingMethodInfo<System.Single>("lateUpdate");
        static CrossBindingMethodInfo<System.Single> mfixedUpdate_24 = new CrossBindingMethodInfo<System.Single>("fixedUpdate");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyAddComponent_25 = new CrossBindingMethodInfo<global::GameComponent>("notifyAddComponent");
        static CrossBindingMethodInfo<System.Boolean, System.Boolean> msetIgnoreTimeScale_26 = new CrossBindingMethodInfo<System.Boolean, System.Boolean>("setIgnoreTimeScale");
        static CrossBindingMethodInfo minitComponents_27 = new CrossBindingMethodInfo("initComponents");
        static CrossBindingMethodInfo<System.String> msetName_28 = new CrossBindingMethodInfo<System.String>("setName");
        static CrossBindingMethodInfo mnotifyConstructDone_29 = new CrossBindingMethodInfo("notifyConstructDone");
        static CrossBindingMethodInfo<System.Boolean> msetDestroy_30 = new CrossBindingMethodInfo<System.Boolean>("setDestroy");
        static CrossBindingFunctionInfo<System.Boolean> misDestroy_31 = new CrossBindingFunctionInfo<System.Boolean>("isDestroy");
        static CrossBindingMethodInfo<System.Int64> msetAssignID_32 = new CrossBindingMethodInfo<System.Int64>("setAssignID");
        static CrossBindingFunctionInfo<System.Int64> mgetAssignID_33 = new CrossBindingFunctionInfo<System.Int64>("getAssignID");
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

            public override void resetProperty()
            {
                if (mresetProperty_0.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_0.Invoke(this.instance);
            }

            public override System.Boolean isActive()
            {
                return misActive_1.Invoke(this.instance);
            }

            public override System.Boolean isEnable()
            {
                if (misEnable_2.CheckShouldInvokeBase(this.instance))
                    return base.isEnable();
                else
                    return misEnable_2.Invoke(this.instance);
            }

            public override void setEnable(System.Boolean enable)
            {
                if (msetEnable_3.CheckShouldInvokeBase(this.instance))
                    base.setEnable(enable);
                else
                    msetEnable_3.Invoke(this.instance, enable);
            }

            public override UnityEngine.Vector3 getPosition()
            {
                return mgetPosition_4.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getRotation()
            {
                return mgetRotation_5.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getScale()
            {
                return mgetScale_6.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldPosition()
            {
                return mgetWorldPosition_7.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldRotation()
            {
                return mgetWorldRotation_8.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldScale()
            {
                return mgetWorldScale_9.Invoke(this.instance);
            }

            public override void setPosition(UnityEngine.Vector3 pos)
            {
                msetPosition_10.Invoke(this.instance, pos);
            }

            public override void setRotation(UnityEngine.Vector3 rot)
            {
                msetRotation_11.Invoke(this.instance, rot);
            }

            public override void setScale(UnityEngine.Vector3 scale)
            {
                msetScale_12.Invoke(this.instance, scale);
            }

            public override void setWorldPosition(UnityEngine.Vector3 pos)
            {
                msetWorldPosition_13.Invoke(this.instance, pos);
            }

            public override void setWorldRotation(UnityEngine.Vector3 rot)
            {
                msetWorldRotation_14.Invoke(this.instance, rot);
            }

            public override void setWorldScale(UnityEngine.Vector3 scale)
            {
                msetWorldScale_15.Invoke(this.instance, scale);
            }

            public override UnityEngine.Vector3 localToWorld(UnityEngine.Vector3 point)
            {
                return mlocalToWorld_16.Invoke(this.instance, point);
            }

            public override UnityEngine.Vector3 worldToLocal(UnityEngine.Vector3 point)
            {
                return mworldToLocal_17.Invoke(this.instance, point);
            }

            public override UnityEngine.Vector3 localToWorldDirection(UnityEngine.Vector3 direction)
            {
                return mlocalToWorldDirection_18.Invoke(this.instance, direction);
            }

            public override UnityEngine.Vector3 worldToLocalDirection(UnityEngine.Vector3 direction)
            {
                return mworldToLocalDirection_19.Invoke(this.instance, direction);
            }

            public override void destroy()
            {
                if (mdestroy_20.CheckShouldInvokeBase(this.instance))
                    base.destroy();
                else
                    mdestroy_20.Invoke(this.instance);
            }

            public override void setActive(System.Boolean active)
            {
                if (msetActive_21.CheckShouldInvokeBase(this.instance))
                    base.setActive(active);
                else
                    msetActive_21.Invoke(this.instance, active);
            }

            public override void update(System.Single elapsedTime)
            {
                if (mupdate_22.CheckShouldInvokeBase(this.instance))
                    base.update(elapsedTime);
                else
                    mupdate_22.Invoke(this.instance, elapsedTime);
            }

            public override void lateUpdate(System.Single elapsedTime)
            {
                if (mlateUpdate_23.CheckShouldInvokeBase(this.instance))
                    base.lateUpdate(elapsedTime);
                else
                    mlateUpdate_23.Invoke(this.instance, elapsedTime);
            }

            public override void fixedUpdate(System.Single elapsedTime)
            {
                if (mfixedUpdate_24.CheckShouldInvokeBase(this.instance))
                    base.fixedUpdate(elapsedTime);
                else
                    mfixedUpdate_24.Invoke(this.instance, elapsedTime);
            }

            public override void notifyAddComponent(global::GameComponent com)
            {
                if (mnotifyAddComponent_25.CheckShouldInvokeBase(this.instance))
                    base.notifyAddComponent(com);
                else
                    mnotifyAddComponent_25.Invoke(this.instance, com);
            }

            public override void setIgnoreTimeScale(System.Boolean ignore, System.Boolean componentOnly)
            {
                if (msetIgnoreTimeScale_26.CheckShouldInvokeBase(this.instance))
                    base.setIgnoreTimeScale(ignore, componentOnly);
                else
                    msetIgnoreTimeScale_26.Invoke(this.instance, ignore, componentOnly);
            }

            protected override void initComponents()
            {
                if (minitComponents_27.CheckShouldInvokeBase(this.instance))
                    base.initComponents();
                else
                    minitComponents_27.Invoke(this.instance);
            }

            public override void setName(System.String name)
            {
                if (msetName_28.CheckShouldInvokeBase(this.instance))
                    base.setName(name);
                else
                    msetName_28.Invoke(this.instance, name);
            }

            public override void notifyConstructDone()
            {
                if (mnotifyConstructDone_29.CheckShouldInvokeBase(this.instance))
                    base.notifyConstructDone();
                else
                    mnotifyConstructDone_29.Invoke(this.instance);
            }

            public override void setDestroy(System.Boolean isDestroy)
            {
                if (msetDestroy_30.CheckShouldInvokeBase(this.instance))
                    base.setDestroy(isDestroy);
                else
                    msetDestroy_30.Invoke(this.instance, isDestroy);
            }

            public override System.Boolean isDestroy()
            {
                if (misDestroy_31.CheckShouldInvokeBase(this.instance))
                    return base.isDestroy();
                else
                    return misDestroy_31.Invoke(this.instance);
            }

            public override void setAssignID(System.Int64 assignID)
            {
                if (msetAssignID_32.CheckShouldInvokeBase(this.instance))
                    base.setAssignID(assignID);
                else
                    msetAssignID_32.Invoke(this.instance, assignID);
            }

            public override System.Int64 getAssignID()
            {
                if (mgetAssignID_33.CheckShouldInvokeBase(this.instance))
                    return base.getAssignID();
                else
                    return mgetAssignID_33.Invoke(this.instance);
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

