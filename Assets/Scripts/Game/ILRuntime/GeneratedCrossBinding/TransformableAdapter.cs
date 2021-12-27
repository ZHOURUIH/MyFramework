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
        static CrossBindingMethodInfo<UnityEngine.GameObject> msetObject_2 = new CrossBindingMethodInfo<UnityEngine.GameObject>("setObject");
        static CrossBindingMethodInfo<System.Boolean> msetActive_3 = new CrossBindingMethodInfo<System.Boolean>("setActive");
        static CrossBindingMethodInfo<System.String> msetName_4 = new CrossBindingMethodInfo<System.String>("setName");
        class raycast_5Info : CrossBindingMethodInfo
        {
            static Type[] pTypes = new Type[] {typeof(UnityEngine.Ray).MakeByRefType(), typeof(UnityEngine.RaycastHit).MakeByRefType(), typeof(System.Single), typeof(System.Boolean)};

            public raycast_5Info()
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
                            ctx.PushFloat(maxDistance);
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
        static raycast_5Info mraycast_5 = new raycast_5Info();
        static CrossBindingFunctionInfo<UnityEngine.GameObject> mgetObject_6 = new CrossBindingFunctionInfo<UnityEngine.GameObject>("getObject");
        static CrossBindingFunctionInfo<System.Boolean> misActive_7 = new CrossBindingFunctionInfo<System.Boolean>("isActive");
        static CrossBindingFunctionInfo<System.Boolean> misActiveInHierarchy_8 = new CrossBindingFunctionInfo<System.Boolean>("isActiveInHierarchy");
        static CrossBindingFunctionInfo<System.Boolean> misEnable_9 = new CrossBindingFunctionInfo<System.Boolean>("isEnable");
        static CrossBindingMethodInfo<System.Boolean> msetEnable_10 = new CrossBindingMethodInfo<System.Boolean>("setEnable");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetPosition_11 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getPosition");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetRotation_12 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getRotation");
        static CrossBindingMethodInfo<global::myUIObject> mcloneFrom_13 = new CrossBindingMethodInfo<global::myUIObject>("cloneFrom");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetScale_14 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getScale");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldPosition_15 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldPosition");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldScale_16 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldScale");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldRotation_17 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldRotation");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetPosition_18 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setPosition");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetScale_19 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setScale");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetRotation_20 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setRotation");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldPosition_21 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldPosition");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldRotation_22 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldRotation");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldScale_23 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldScale");
        static CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Space> mmove_24 = new CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Space>("move");
        static CrossBindingFunctionInfo<global::IMouseEventCollect, System.Boolean> misChildOf_25 = new CrossBindingFunctionInfo<global::IMouseEventCollect, System.Boolean>("isChildOf");
        static CrossBindingMethodInfo<System.Single> msetAlpha_26 = new CrossBindingMethodInfo<System.Single>("setAlpha");
        static CrossBindingFunctionInfo<System.Single> mgetAlpha_27 = new CrossBindingFunctionInfo<System.Single>("getAlpha");
        static CrossBindingMethodInfo<System.Single> mupdate_28 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo<System.Single> mlateUpdate_29 = new CrossBindingMethodInfo<System.Single>("lateUpdate");
        static CrossBindingMethodInfo<System.Single> mfixedUpdate_30 = new CrossBindingMethodInfo<System.Single>("fixedUpdate");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyAddComponent_31 = new CrossBindingMethodInfo<global::GameComponent>("notifyAddComponent");
        static CrossBindingMethodInfo<System.Boolean, System.Boolean> msetIgnoreTimeScale_32 = new CrossBindingMethodInfo<System.Boolean, System.Boolean>("setIgnoreTimeScale");
        static CrossBindingMethodInfo minitComponents_33 = new CrossBindingMethodInfo("initComponents");
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

            public override void setObject(UnityEngine.GameObject obj)
            {
                if (msetObject_2.CheckShouldInvokeBase(this.instance))
                    base.setObject(obj);
                else
                    msetObject_2.Invoke(this.instance, obj);
            }

            public override void setActive(System.Boolean active)
            {
                if (msetActive_3.CheckShouldInvokeBase(this.instance))
                    base.setActive(active);
                else
                    msetActive_3.Invoke(this.instance, active);
            }

            public override void setName(System.String name)
            {
                if (msetName_4.CheckShouldInvokeBase(this.instance))
                    base.setName(name);
                else
                    msetName_4.Invoke(this.instance, name);
            }

            public override System.Boolean raycast(ref UnityEngine.Ray ray, out UnityEngine.RaycastHit hit, System.Single maxDistance)
            {
                if (mraycast_5.CheckShouldInvokeBase(this.instance))
                    return base.raycast(ref ray, out hit, maxDistance);
                else
                    return mraycast_5.Invoke(this.instance, ref ray, out hit, maxDistance);
            }

            public override UnityEngine.GameObject getObject()
            {
                if (mgetObject_6.CheckShouldInvokeBase(this.instance))
                    return base.getObject();
                else
                    return mgetObject_6.Invoke(this.instance);
            }

            public override System.Boolean isActive()
            {
                if (misActive_7.CheckShouldInvokeBase(this.instance))
                    return base.isActive();
                else
                    return misActive_7.Invoke(this.instance);
            }

            public override System.Boolean isActiveInHierarchy()
            {
                if (misActiveInHierarchy_8.CheckShouldInvokeBase(this.instance))
                    return base.isActiveInHierarchy();
                else
                    return misActiveInHierarchy_8.Invoke(this.instance);
            }

            public override System.Boolean isEnable()
            {
                if (misEnable_9.CheckShouldInvokeBase(this.instance))
                    return base.isEnable();
                else
                    return misEnable_9.Invoke(this.instance);
            }

            public override void setEnable(System.Boolean enable)
            {
                if (msetEnable_10.CheckShouldInvokeBase(this.instance))
                    base.setEnable(enable);
                else
                    msetEnable_10.Invoke(this.instance, enable);
            }

            public override UnityEngine.Vector3 getPosition()
            {
                if (mgetPosition_11.CheckShouldInvokeBase(this.instance))
                    return base.getPosition();
                else
                    return mgetPosition_11.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getRotation()
            {
                if (mgetRotation_12.CheckShouldInvokeBase(this.instance))
                    return base.getRotation();
                else
                    return mgetRotation_12.Invoke(this.instance);
            }

            public override void cloneFrom(global::myUIObject obj)
            {
                if (mcloneFrom_13.CheckShouldInvokeBase(this.instance))
                    base.cloneFrom(obj);
                else
                    mcloneFrom_13.Invoke(this.instance, obj);
            }

            public override UnityEngine.Vector3 getScale()
            {
                if (mgetScale_14.CheckShouldInvokeBase(this.instance))
                    return base.getScale();
                else
                    return mgetScale_14.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldPosition()
            {
                if (mgetWorldPosition_15.CheckShouldInvokeBase(this.instance))
                    return base.getWorldPosition();
                else
                    return mgetWorldPosition_15.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldScale()
            {
                if (mgetWorldScale_16.CheckShouldInvokeBase(this.instance))
                    return base.getWorldScale();
                else
                    return mgetWorldScale_16.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldRotation()
            {
                if (mgetWorldRotation_17.CheckShouldInvokeBase(this.instance))
                    return base.getWorldRotation();
                else
                    return mgetWorldRotation_17.Invoke(this.instance);
            }

            public override void setPosition(UnityEngine.Vector3 pos)
            {
                if (msetPosition_18.CheckShouldInvokeBase(this.instance))
                    base.setPosition(pos);
                else
                    msetPosition_18.Invoke(this.instance, pos);
            }

            public override void setScale(UnityEngine.Vector3 scale)
            {
                if (msetScale_19.CheckShouldInvokeBase(this.instance))
                    base.setScale(scale);
                else
                    msetScale_19.Invoke(this.instance, scale);
            }

            public override void setRotation(UnityEngine.Vector3 rot)
            {
                if (msetRotation_20.CheckShouldInvokeBase(this.instance))
                    base.setRotation(rot);
                else
                    msetRotation_20.Invoke(this.instance, rot);
            }

            public override void setWorldPosition(UnityEngine.Vector3 pos)
            {
                if (msetWorldPosition_21.CheckShouldInvokeBase(this.instance))
                    base.setWorldPosition(pos);
                else
                    msetWorldPosition_21.Invoke(this.instance, pos);
            }

            public override void setWorldRotation(UnityEngine.Vector3 rot)
            {
                if (msetWorldRotation_22.CheckShouldInvokeBase(this.instance))
                    base.setWorldRotation(rot);
                else
                    msetWorldRotation_22.Invoke(this.instance, rot);
            }

            public override void setWorldScale(UnityEngine.Vector3 scale)
            {
                if (msetWorldScale_23.CheckShouldInvokeBase(this.instance))
                    base.setWorldScale(scale);
                else
                    msetWorldScale_23.Invoke(this.instance, scale);
            }

            public override void move(UnityEngine.Vector3 moveDelta, UnityEngine.Space space)
            {
                if (mmove_24.CheckShouldInvokeBase(this.instance))
                    base.move(moveDelta, space);
                else
                    mmove_24.Invoke(this.instance, moveDelta, space);
            }

            public override System.Boolean isChildOf(global::IMouseEventCollect parent)
            {
                if (misChildOf_25.CheckShouldInvokeBase(this.instance))
                    return base.isChildOf(parent);
                else
                    return misChildOf_25.Invoke(this.instance, parent);
            }

            public override void setAlpha(System.Single alpha)
            {
                if (msetAlpha_26.CheckShouldInvokeBase(this.instance))
                    base.setAlpha(alpha);
                else
                    msetAlpha_26.Invoke(this.instance, alpha);
            }

            public override System.Single getAlpha()
            {
                if (mgetAlpha_27.CheckShouldInvokeBase(this.instance))
                    return base.getAlpha();
                else
                    return mgetAlpha_27.Invoke(this.instance);
            }

            public override void update(System.Single elapsedTime)
            {
                if (mupdate_28.CheckShouldInvokeBase(this.instance))
                    base.update(elapsedTime);
                else
                    mupdate_28.Invoke(this.instance, elapsedTime);
            }

            public override void lateUpdate(System.Single elapsedTime)
            {
                if (mlateUpdate_29.CheckShouldInvokeBase(this.instance))
                    base.lateUpdate(elapsedTime);
                else
                    mlateUpdate_29.Invoke(this.instance, elapsedTime);
            }

            public override void fixedUpdate(System.Single elapsedTime)
            {
                if (mfixedUpdate_30.CheckShouldInvokeBase(this.instance))
                    base.fixedUpdate(elapsedTime);
                else
                    mfixedUpdate_30.Invoke(this.instance, elapsedTime);
            }

            public override void notifyAddComponent(global::GameComponent com)
            {
                if (mnotifyAddComponent_31.CheckShouldInvokeBase(this.instance))
                    base.notifyAddComponent(com);
                else
                    mnotifyAddComponent_31.Invoke(this.instance, com);
            }

            public override void setIgnoreTimeScale(System.Boolean ignore, System.Boolean componentOnly)
            {
                if (msetIgnoreTimeScale_32.CheckShouldInvokeBase(this.instance))
                    base.setIgnoreTimeScale(ignore, componentOnly);
                else
                    msetIgnoreTimeScale_32.Invoke(this.instance, ignore, componentOnly);
            }

            protected override void initComponents()
            {
                if (minitComponents_33.CheckShouldInvokeBase(this.instance))
                    base.initComponents();
                else
                    minitComponents_33.Invoke(this.instance);
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

