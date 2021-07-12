using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class TransformableAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo mdestroy_0 = new CrossBindingMethodInfo("destroy");
        static CrossBindingMethodInfo mresetProperty_1 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo<System.Boolean> msetActive_2 = new CrossBindingMethodInfo<System.Boolean>("setActive");
        static CrossBindingMethodInfo<System.String> msetName_3 = new CrossBindingMethodInfo<System.String>("setName");
        class raycast_4Info : CrossBindingMethodInfo
        {
            static Type[] pTypes = new Type[] {typeof(UnityEngine.Ray).MakeByRefType(), typeof(UnityEngine.RaycastHit).MakeByRefType(), typeof(System.Single), typeof(System.Boolean)};

            public raycast_4Info()
                : base("raycast")
            {

            }

            protected override Type ReturnType { get { return typeof(System.Boolean); } }

            protected override Type[] Parameters { get { return pTypes; } }
            public System.Boolean Invoke(ILTypeInstance instance, ref UnityEngine.Ray ray, out UnityEngine.RaycastHit hit, System.Single maxDistance)
            {
                EnsureMethod(instance);
                    hit = default(UnityEngine.RaycastHit);

                if (method != null)
                {
                    invoking = true;
                    System.Boolean __res = default(System.Boolean);
                    try
                    {
                        using (var ctx = domain.BeginInvoke(method))
                        {
                            ctx.PushObject(ray);
                            ctx.PushObject(hit);
                            ctx.PushObject(instance);
                            ctx.PushReference(0);
                            ctx.PushReference(1);
                            ctx.PushInteger(maxDistance);
                            ctx.Invoke();
                            __res = ctx.ReadBool();
                            ray = ctx.ReadObject<UnityEngine.Ray>(0);
                            hit = ctx.ReadObject<UnityEngine.RaycastHit>(1);
                        }
                    }
                    finally
                    {
                        invoking = false;
                    }
                   return __res;
                }
                else
                    return default(System.Boolean);
            }

            public override void Invoke(ILTypeInstance instance)
            {
                throw new NotSupportedException();
            }
        }
        static raycast_4Info mraycast_4 = new raycast_4Info();
        static CrossBindingFunctionInfo<UnityEngine.GameObject> mgetObject_5 = new CrossBindingFunctionInfo<UnityEngine.GameObject>("getObject");
        static CrossBindingFunctionInfo<System.Boolean> misActive_6 = new CrossBindingFunctionInfo<System.Boolean>("isActive");
        static CrossBindingFunctionInfo<System.Boolean> misActiveInHierarchy_7 = new CrossBindingFunctionInfo<System.Boolean>("isActiveInHierarchy");
        static CrossBindingFunctionInfo<System.Boolean> misEnable_8 = new CrossBindingFunctionInfo<System.Boolean>("isEnable");
        static CrossBindingMethodInfo<System.Boolean> msetEnable_9 = new CrossBindingMethodInfo<System.Boolean>("setEnable");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetPosition_10 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getPosition");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetRotation_11 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getRotation");
        static CrossBindingMethodInfo<global::myUIObject> mcloneFrom_12 = new CrossBindingMethodInfo<global::myUIObject>("cloneFrom");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetScale_13 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getScale");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldPosition_14 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldPosition");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldScale_15 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldScale");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldRotation_16 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldRotation");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetPosition_17 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setPosition");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetScale_18 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setScale");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetRotation_19 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setRotation");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldPosition_20 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldPosition");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldRotation_21 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldRotation");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldScale_22 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldScale");
        static CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Space> mmove_23 = new CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Space>("move");
        static CrossBindingFunctionInfo<global::IMouseEventCollect, System.Boolean> misChildOf_24 = new CrossBindingFunctionInfo<global::IMouseEventCollect, System.Boolean>("isChildOf");
        static CrossBindingMethodInfo<System.Single> msetAlpha_25 = new CrossBindingMethodInfo<System.Single>("setAlpha");
        static CrossBindingFunctionInfo<System.Single> mgetAlpha_26 = new CrossBindingFunctionInfo<System.Single>("getAlpha");
        static CrossBindingMethodInfo<System.Single> mupdate_27 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo<System.Single> mlateUpdate_28 = new CrossBindingMethodInfo<System.Single>("lateUpdate");
        static CrossBindingMethodInfo<System.Single> mfixedUpdate_29 = new CrossBindingMethodInfo<System.Single>("fixedUpdate");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyAddComponent_30 = new CrossBindingMethodInfo<global::GameComponent>("notifyAddComponent");
        static CrossBindingMethodInfo<System.Boolean, System.Boolean> msetIgnoreTimeScale_31 = new CrossBindingMethodInfo<System.Boolean, System.Boolean>("setIgnoreTimeScale");
        static CrossBindingMethodInfo minitComponents_32 = new CrossBindingMethodInfo("initComponents");
        static CrossBindingMethodInfo mnotifyConstructDone_33 = new CrossBindingMethodInfo("notifyConstructDone");
        static CrossBindingMethodInfo<System.Boolean> msetDestroy_34 = new CrossBindingMethodInfo<System.Boolean>("setDestroy");
        static CrossBindingFunctionInfo<System.Boolean> misDestroy_35 = new CrossBindingFunctionInfo<System.Boolean>("isDestroy");
        static CrossBindingMethodInfo<System.Int64> msetAssignID_36 = new CrossBindingMethodInfo<System.Int64>("setAssignID");
        static CrossBindingFunctionInfo<System.Int64> mgetAssignID_37 = new CrossBindingFunctionInfo<System.Int64>("getAssignID");
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

            public override void setActive(System.Boolean active)
            {
                if (msetActive_2.CheckShouldInvokeBase(this.instance))
                    base.setActive(active);
                else
                    msetActive_2.Invoke(this.instance, active);
            }

            public override void setName(System.String name)
            {
                if (msetName_3.CheckShouldInvokeBase(this.instance))
                    base.setName(name);
                else
                    msetName_3.Invoke(this.instance, name);
            }

            public override System.Boolean raycast(ref UnityEngine.Ray ray, out UnityEngine.RaycastHit hit, System.Single maxDistance)
            {
                if (mraycast_4.CheckShouldInvokeBase(this.instance))
                    return base.raycast(ref ray, out hit, maxDistance);
                else
                    return mraycast_4.Invoke(this.instance, ref ray, out hit, maxDistance);
            }

            public override UnityEngine.GameObject getObject()
            {
                if (mgetObject_5.CheckShouldInvokeBase(this.instance))
                    return base.getObject();
                else
                    return mgetObject_5.Invoke(this.instance);
            }

            public override System.Boolean isActive()
            {
                if (misActive_6.CheckShouldInvokeBase(this.instance))
                    return base.isActive();
                else
                    return misActive_6.Invoke(this.instance);
            }

            public override System.Boolean isActiveInHierarchy()
            {
                if (misActiveInHierarchy_7.CheckShouldInvokeBase(this.instance))
                    return base.isActiveInHierarchy();
                else
                    return misActiveInHierarchy_7.Invoke(this.instance);
            }

            public override System.Boolean isEnable()
            {
                if (misEnable_8.CheckShouldInvokeBase(this.instance))
                    return base.isEnable();
                else
                    return misEnable_8.Invoke(this.instance);
            }

            public override void setEnable(System.Boolean enable)
            {
                if (msetEnable_9.CheckShouldInvokeBase(this.instance))
                    base.setEnable(enable);
                else
                    msetEnable_9.Invoke(this.instance, enable);
            }

            public override UnityEngine.Vector3 getPosition()
            {
                if (mgetPosition_10.CheckShouldInvokeBase(this.instance))
                    return base.getPosition();
                else
                    return mgetPosition_10.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getRotation()
            {
                if (mgetRotation_11.CheckShouldInvokeBase(this.instance))
                    return base.getRotation();
                else
                    return mgetRotation_11.Invoke(this.instance);
            }

            public override void cloneFrom(global::myUIObject obj)
            {
                if (mcloneFrom_12.CheckShouldInvokeBase(this.instance))
                    base.cloneFrom(obj);
                else
                    mcloneFrom_12.Invoke(this.instance, obj);
            }

            public override UnityEngine.Vector3 getScale()
            {
                if (mgetScale_13.CheckShouldInvokeBase(this.instance))
                    return base.getScale();
                else
                    return mgetScale_13.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldPosition()
            {
                if (mgetWorldPosition_14.CheckShouldInvokeBase(this.instance))
                    return base.getWorldPosition();
                else
                    return mgetWorldPosition_14.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldScale()
            {
                if (mgetWorldScale_15.CheckShouldInvokeBase(this.instance))
                    return base.getWorldScale();
                else
                    return mgetWorldScale_15.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldRotation()
            {
                if (mgetWorldRotation_16.CheckShouldInvokeBase(this.instance))
                    return base.getWorldRotation();
                else
                    return mgetWorldRotation_16.Invoke(this.instance);
            }

            public override void setPosition(UnityEngine.Vector3 pos)
            {
                if (msetPosition_17.CheckShouldInvokeBase(this.instance))
                    base.setPosition(pos);
                else
                    msetPosition_17.Invoke(this.instance, pos);
            }

            public override void setScale(UnityEngine.Vector3 scale)
            {
                if (msetScale_18.CheckShouldInvokeBase(this.instance))
                    base.setScale(scale);
                else
                    msetScale_18.Invoke(this.instance, scale);
            }

            public override void setRotation(UnityEngine.Vector3 rot)
            {
                if (msetRotation_19.CheckShouldInvokeBase(this.instance))
                    base.setRotation(rot);
                else
                    msetRotation_19.Invoke(this.instance, rot);
            }

            public override void setWorldPosition(UnityEngine.Vector3 pos)
            {
                if (msetWorldPosition_20.CheckShouldInvokeBase(this.instance))
                    base.setWorldPosition(pos);
                else
                    msetWorldPosition_20.Invoke(this.instance, pos);
            }

            public override void setWorldRotation(UnityEngine.Vector3 rot)
            {
                if (msetWorldRotation_21.CheckShouldInvokeBase(this.instance))
                    base.setWorldRotation(rot);
                else
                    msetWorldRotation_21.Invoke(this.instance, rot);
            }

            public override void setWorldScale(UnityEngine.Vector3 scale)
            {
                if (msetWorldScale_22.CheckShouldInvokeBase(this.instance))
                    base.setWorldScale(scale);
                else
                    msetWorldScale_22.Invoke(this.instance, scale);
            }

            public override void move(UnityEngine.Vector3 moveDelta, UnityEngine.Space space)
            {
                if (mmove_23.CheckShouldInvokeBase(this.instance))
                    base.move(moveDelta, space);
                else
                    mmove_23.Invoke(this.instance, moveDelta, space);
            }

            public override System.Boolean isChildOf(global::IMouseEventCollect parent)
            {
                if (misChildOf_24.CheckShouldInvokeBase(this.instance))
                    return base.isChildOf(parent);
                else
                    return misChildOf_24.Invoke(this.instance, parent);
            }

            public override void setAlpha(System.Single alpha)
            {
                if (msetAlpha_25.CheckShouldInvokeBase(this.instance))
                    base.setAlpha(alpha);
                else
                    msetAlpha_25.Invoke(this.instance, alpha);
            }

            public override System.Single getAlpha()
            {
                if (mgetAlpha_26.CheckShouldInvokeBase(this.instance))
                    return base.getAlpha();
                else
                    return mgetAlpha_26.Invoke(this.instance);
            }

            public override void update(System.Single elapsedTime)
            {
                if (mupdate_27.CheckShouldInvokeBase(this.instance))
                    base.update(elapsedTime);
                else
                    mupdate_27.Invoke(this.instance, elapsedTime);
            }

            public override void lateUpdate(System.Single elapsedTime)
            {
                if (mlateUpdate_28.CheckShouldInvokeBase(this.instance))
                    base.lateUpdate(elapsedTime);
                else
                    mlateUpdate_28.Invoke(this.instance, elapsedTime);
            }

            public override void fixedUpdate(System.Single elapsedTime)
            {
                if (mfixedUpdate_29.CheckShouldInvokeBase(this.instance))
                    base.fixedUpdate(elapsedTime);
                else
                    mfixedUpdate_29.Invoke(this.instance, elapsedTime);
            }

            public override void notifyAddComponent(global::GameComponent com)
            {
                if (mnotifyAddComponent_30.CheckShouldInvokeBase(this.instance))
                    base.notifyAddComponent(com);
                else
                    mnotifyAddComponent_30.Invoke(this.instance, com);
            }

            public override void setIgnoreTimeScale(System.Boolean ignore, System.Boolean componentOnly)
            {
                if (msetIgnoreTimeScale_31.CheckShouldInvokeBase(this.instance))
                    base.setIgnoreTimeScale(ignore, componentOnly);
                else
                    msetIgnoreTimeScale_31.Invoke(this.instance, ignore, componentOnly);
            }

            protected override void initComponents()
            {
                if (minitComponents_32.CheckShouldInvokeBase(this.instance))
                    base.initComponents();
                else
                    minitComponents_32.Invoke(this.instance);
            }

            public override void notifyConstructDone()
            {
                if (mnotifyConstructDone_33.CheckShouldInvokeBase(this.instance))
                    base.notifyConstructDone();
                else
                    mnotifyConstructDone_33.Invoke(this.instance);
            }

            public override void setDestroy(System.Boolean isDestroy)
            {
                if (msetDestroy_34.CheckShouldInvokeBase(this.instance))
                    base.setDestroy(isDestroy);
                else
                    msetDestroy_34.Invoke(this.instance, isDestroy);
            }

            public override System.Boolean isDestroy()
            {
                if (misDestroy_35.CheckShouldInvokeBase(this.instance))
                    return base.isDestroy();
                else
                    return misDestroy_35.Invoke(this.instance);
            }

            public override void setAssignID(System.Int64 assignID)
            {
                if (msetAssignID_36.CheckShouldInvokeBase(this.instance))
                    base.setAssignID(assignID);
                else
                    msetAssignID_36.Invoke(this.instance, assignID);
            }

            public override System.Int64 getAssignID()
            {
                if (mgetAssignID_37.CheckShouldInvokeBase(this.instance))
                    return base.getAssignID();
                else
                    return mgetAssignID_37.Invoke(this.instance);
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

