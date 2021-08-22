using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class CharacterAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo minit_0 = new CrossBindingMethodInfo("init");
        static CrossBindingMethodInfo mresetProperty_1 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo mdestroy_2 = new CrossBindingMethodInfo("destroy");
        static CrossBindingMethodInfo<UnityEngine.GameObject> msetObject_3 = new CrossBindingMethodInfo<UnityEngine.GameObject>("setObject");
        static CrossBindingMethodInfo mdestroyModel_4 = new CrossBindingMethodInfo("destroyModel");
        static CrossBindingFunctionInfo<System.String, System.Single> mgetAnimationLength_5 = new CrossBindingFunctionInfo<System.String, System.Single>("getAnimationLength");
        static CrossBindingMethodInfo mnotifyModelLoaded_6 = new CrossBindingMethodInfo("notifyModelLoaded");
        static CrossBindingFunctionInfo<global::CharacterData> mcreateCharacterData_7 = new CrossBindingFunctionInfo<global::CharacterData>("createCharacterData");
        static CrossBindingMethodInfo minitComponents_8 = new CrossBindingMethodInfo("initComponents");
        static CrossBindingMethodInfo<System.Single> mupdate_9 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo<System.Single> mfixedUpdate_10 = new CrossBindingMethodInfo<System.Single>("fixedUpdate");
        static CrossBindingFunctionInfo<System.Boolean> misHandleInput_11 = new CrossBindingFunctionInfo<System.Boolean>("isHandleInput");
        static CrossBindingFunctionInfo<global::UIDepth> mgetDepth_12 = new CrossBindingFunctionInfo<global::UIDepth>("getDepth");
        static CrossBindingFunctionInfo<System.Boolean> misReceiveScreenMouse_13 = new CrossBindingFunctionInfo<System.Boolean>("isReceiveScreenMouse");
        static CrossBindingFunctionInfo<System.Boolean> misPassRay_14 = new CrossBindingFunctionInfo<System.Boolean>("isPassRay");
        static CrossBindingFunctionInfo<System.Boolean> misDragable_15 = new CrossBindingFunctionInfo<System.Boolean>("isDragable");
        static CrossBindingFunctionInfo<System.Boolean> misMouseHovered_16 = new CrossBindingFunctionInfo<System.Boolean>("isMouseHovered");
        static CrossBindingMethodInfo<System.Boolean> msetPassRay_17 = new CrossBindingMethodInfo<System.Boolean>("setPassRay");
        static CrossBindingMethodInfo<System.Boolean> msetHandleInput_18 = new CrossBindingMethodInfo<System.Boolean>("setHandleInput");
        static CrossBindingMethodInfo<global::ObjectClickCallback> msetClickCallback_19 = new CrossBindingMethodInfo<global::ObjectClickCallback>("setClickCallback");
        static CrossBindingMethodInfo<global::ObjectHoverCallback> msetHoverCallback_20 = new CrossBindingMethodInfo<global::ObjectHoverCallback>("setHoverCallback");
        static CrossBindingMethodInfo<global::ObjectPressCallback> msetPressCallback_21 = new CrossBindingMethodInfo<global::ObjectPressCallback>("setPressCallback");
        static CrossBindingMethodInfo<System.Int32> monMouseEnter_22 = new CrossBindingMethodInfo<System.Int32>("onMouseEnter");
        static CrossBindingMethodInfo<System.Int32> monMouseLeave_23 = new CrossBindingMethodInfo<System.Int32>("onMouseLeave");
        static CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32> monMouseDown_24 = new CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32>("onMouseDown");
        static CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32> monMouseUp_25 = new CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32>("onMouseUp");
        static CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Vector3, System.Single, System.Int32> monMouseMove_26 = new CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Vector3, System.Single, System.Int32>("onMouseMove");
        static CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32> monMouseStay_27 = new CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32>("onMouseStay");
        static CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32> monScreenMouseDown_28 = new CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32>("onScreenMouseDown");
        static CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32> monScreenMouseUp_29 = new CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32>("onScreenMouseUp");
        static CrossBindingMethodInfo<global::IMouseEventCollect, global::BOOL> monReceiveDrag_30 = new CrossBindingMethodInfo<global::IMouseEventCollect, global::BOOL>("onReceiveDrag");
        static CrossBindingMethodInfo<global::IMouseEventCollect, System.Boolean> monDragHoverd_31 = new CrossBindingMethodInfo<global::IMouseEventCollect, System.Boolean>("onDragHoverd");
        static CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Vector3> monMultiTouchStart_32 = new CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Vector3>("onMultiTouchStart");
        static CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector3> monMultiTouchMove_33 = new CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector3>("onMultiTouchMove");
        static CrossBindingMethodInfo monMultiTouchEnd_34 = new CrossBindingMethodInfo("onMultiTouchEnd");
        static CrossBindingMethodInfo<System.Boolean> msetActive_35 = new CrossBindingMethodInfo<System.Boolean>("setActive");
        static CrossBindingMethodInfo<System.String> msetName_36 = new CrossBindingMethodInfo<System.String>("setName");
        class raycast_37Info : CrossBindingMethodInfo
        {
            static Type[] pTypes = new Type[] {typeof(UnityEngine.Ray).MakeByRefType(), typeof(UnityEngine.RaycastHit).MakeByRefType(), typeof(System.Single), typeof(System.Boolean)};

            public raycast_37Info()
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
        static raycast_37Info mraycast_37 = new raycast_37Info();
        static CrossBindingFunctionInfo<UnityEngine.GameObject> mgetObject_38 = new CrossBindingFunctionInfo<UnityEngine.GameObject>("getObject");
        static CrossBindingFunctionInfo<System.Boolean> misActive_39 = new CrossBindingFunctionInfo<System.Boolean>("isActive");
        static CrossBindingFunctionInfo<System.Boolean> misActiveInHierarchy_40 = new CrossBindingFunctionInfo<System.Boolean>("isActiveInHierarchy");
        static CrossBindingFunctionInfo<System.Boolean> misEnable_41 = new CrossBindingFunctionInfo<System.Boolean>("isEnable");
        static CrossBindingMethodInfo<System.Boolean> msetEnable_42 = new CrossBindingMethodInfo<System.Boolean>("setEnable");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetPosition_43 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getPosition");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetRotation_44 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getRotation");
        static CrossBindingMethodInfo<global::myUIObject> mcloneFrom_45 = new CrossBindingMethodInfo<global::myUIObject>("cloneFrom");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetScale_46 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getScale");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldPosition_47 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldPosition");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldScale_48 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldScale");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldRotation_49 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldRotation");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetPosition_50 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setPosition");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetScale_51 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setScale");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetRotation_52 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setRotation");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldPosition_53 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldPosition");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldRotation_54 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldRotation");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldScale_55 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldScale");
        static CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Space> mmove_56 = new CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Space>("move");
        static CrossBindingFunctionInfo<global::IMouseEventCollect, System.Boolean> misChildOf_57 = new CrossBindingFunctionInfo<global::IMouseEventCollect, System.Boolean>("isChildOf");
        static CrossBindingMethodInfo<System.Single> msetAlpha_58 = new CrossBindingMethodInfo<System.Single>("setAlpha");
        static CrossBindingFunctionInfo<System.Single> mgetAlpha_59 = new CrossBindingFunctionInfo<System.Single>("getAlpha");
        static CrossBindingMethodInfo<System.Single> mlateUpdate_60 = new CrossBindingMethodInfo<System.Single>("lateUpdate");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyAddComponent_61 = new CrossBindingMethodInfo<global::GameComponent>("notifyAddComponent");
        static CrossBindingMethodInfo<System.Boolean, System.Boolean> msetIgnoreTimeScale_62 = new CrossBindingMethodInfo<System.Boolean, System.Boolean>("setIgnoreTimeScale");
        static CrossBindingMethodInfo<System.Boolean> msetDestroy_63 = new CrossBindingMethodInfo<System.Boolean>("setDestroy");
        static CrossBindingFunctionInfo<System.Boolean> misDestroy_64 = new CrossBindingFunctionInfo<System.Boolean>("isDestroy");
        static CrossBindingMethodInfo<System.Int64> msetAssignID_65 = new CrossBindingMethodInfo<System.Int64>("setAssignID");
        static CrossBindingFunctionInfo<System.Int64> mgetAssignID_66 = new CrossBindingFunctionInfo<System.Int64>("getAssignID");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::Character);
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

        public class Adapter : global::Character, CrossBindingAdaptorType
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

            public override void setObject(UnityEngine.GameObject obj)
            {
                if (msetObject_3.CheckShouldInvokeBase(this.instance))
                    base.setObject(obj);
                else
                    msetObject_3.Invoke(this.instance, obj);
            }

            public override void destroyModel()
            {
                if (mdestroyModel_4.CheckShouldInvokeBase(this.instance))
                    base.destroyModel();
                else
                    mdestroyModel_4.Invoke(this.instance);
            }

            public override System.Single getAnimationLength(System.String name)
            {
                if (mgetAnimationLength_5.CheckShouldInvokeBase(this.instance))
                    return base.getAnimationLength(name);
                else
                    return mgetAnimationLength_5.Invoke(this.instance, name);
            }

            public override void notifyModelLoaded()
            {
                if (mnotifyModelLoaded_6.CheckShouldInvokeBase(this.instance))
                    base.notifyModelLoaded();
                else
                    mnotifyModelLoaded_6.Invoke(this.instance);
            }

            protected override global::CharacterData createCharacterData()
            {
                if (mcreateCharacterData_7.CheckShouldInvokeBase(this.instance))
                    return base.createCharacterData();
                else
                    return mcreateCharacterData_7.Invoke(this.instance);
            }

            protected override void initComponents()
            {
                if (minitComponents_8.CheckShouldInvokeBase(this.instance))
                    base.initComponents();
                else
                    minitComponents_8.Invoke(this.instance);
            }

            public override void update(System.Single elapsedTime)
            {
                if (mupdate_9.CheckShouldInvokeBase(this.instance))
                    base.update(elapsedTime);
                else
                    mupdate_9.Invoke(this.instance, elapsedTime);
            }

            public override void fixedUpdate(System.Single elapsedTime)
            {
                if (mfixedUpdate_10.CheckShouldInvokeBase(this.instance))
                    base.fixedUpdate(elapsedTime);
                else
                    mfixedUpdate_10.Invoke(this.instance, elapsedTime);
            }

            public override System.Boolean isHandleInput()
            {
                if (misHandleInput_11.CheckShouldInvokeBase(this.instance))
                    return base.isHandleInput();
                else
                    return misHandleInput_11.Invoke(this.instance);
            }

            public override global::UIDepth getDepth()
            {
                if (mgetDepth_12.CheckShouldInvokeBase(this.instance))
                    return base.getDepth();
                else
                    return mgetDepth_12.Invoke(this.instance);
            }

            public override System.Boolean isReceiveScreenMouse()
            {
                if (misReceiveScreenMouse_13.CheckShouldInvokeBase(this.instance))
                    return base.isReceiveScreenMouse();
                else
                    return misReceiveScreenMouse_13.Invoke(this.instance);
            }

            public override System.Boolean isPassRay()
            {
                if (misPassRay_14.CheckShouldInvokeBase(this.instance))
                    return base.isPassRay();
                else
                    return misPassRay_14.Invoke(this.instance);
            }

            public override System.Boolean isDragable()
            {
                if (misDragable_15.CheckShouldInvokeBase(this.instance))
                    return base.isDragable();
                else
                    return misDragable_15.Invoke(this.instance);
            }

            public override System.Boolean isMouseHovered()
            {
                if (misMouseHovered_16.CheckShouldInvokeBase(this.instance))
                    return base.isMouseHovered();
                else
                    return misMouseHovered_16.Invoke(this.instance);
            }

            public override void setPassRay(System.Boolean passRay)
            {
                if (msetPassRay_17.CheckShouldInvokeBase(this.instance))
                    base.setPassRay(passRay);
                else
                    msetPassRay_17.Invoke(this.instance, passRay);
            }

            public override void setHandleInput(System.Boolean handleInput)
            {
                if (msetHandleInput_18.CheckShouldInvokeBase(this.instance))
                    base.setHandleInput(handleInput);
                else
                    msetHandleInput_18.Invoke(this.instance, handleInput);
            }

            public override void setClickCallback(global::ObjectClickCallback callback)
            {
                if (msetClickCallback_19.CheckShouldInvokeBase(this.instance))
                    base.setClickCallback(callback);
                else
                    msetClickCallback_19.Invoke(this.instance, callback);
            }

            public override void setHoverCallback(global::ObjectHoverCallback callback)
            {
                if (msetHoverCallback_20.CheckShouldInvokeBase(this.instance))
                    base.setHoverCallback(callback);
                else
                    msetHoverCallback_20.Invoke(this.instance, callback);
            }

            public override void setPressCallback(global::ObjectPressCallback callback)
            {
                if (msetPressCallback_21.CheckShouldInvokeBase(this.instance))
                    base.setPressCallback(callback);
                else
                    msetPressCallback_21.Invoke(this.instance, callback);
            }

            public override void onMouseEnter(System.Int32 touchID)
            {
                if (monMouseEnter_22.CheckShouldInvokeBase(this.instance))
                    base.onMouseEnter(touchID);
                else
                    monMouseEnter_22.Invoke(this.instance, touchID);
            }

            public override void onMouseLeave(System.Int32 touchID)
            {
                if (monMouseLeave_23.CheckShouldInvokeBase(this.instance))
                    base.onMouseLeave(touchID);
                else
                    monMouseLeave_23.Invoke(this.instance, touchID);
            }

            public override void onMouseDown(UnityEngine.Vector3 mousePos, System.Int32 touchID)
            {
                if (monMouseDown_24.CheckShouldInvokeBase(this.instance))
                    base.onMouseDown(mousePos, touchID);
                else
                    monMouseDown_24.Invoke(this.instance, mousePos, touchID);
            }

            public override void onMouseUp(UnityEngine.Vector3 mousePos, System.Int32 touchID)
            {
                if (monMouseUp_25.CheckShouldInvokeBase(this.instance))
                    base.onMouseUp(mousePos, touchID);
                else
                    monMouseUp_25.Invoke(this.instance, mousePos, touchID);
            }

            public override void onMouseMove(UnityEngine.Vector3 mousePos, UnityEngine.Vector3 moveDelta, System.Single moveTime, System.Int32 touchID)
            {
                if (monMouseMove_26.CheckShouldInvokeBase(this.instance))
                    base.onMouseMove(mousePos, moveDelta, moveTime, touchID);
                else
                    monMouseMove_26.Invoke(this.instance, mousePos, moveDelta, moveTime, touchID);
            }

            public override void onMouseStay(UnityEngine.Vector3 mousePos, System.Int32 touchID)
            {
                if (monMouseStay_27.CheckShouldInvokeBase(this.instance))
                    base.onMouseStay(mousePos, touchID);
                else
                    monMouseStay_27.Invoke(this.instance, mousePos, touchID);
            }

            public override void onScreenMouseDown(UnityEngine.Vector3 mousePos, System.Int32 touchID)
            {
                if (monScreenMouseDown_28.CheckShouldInvokeBase(this.instance))
                    base.onScreenMouseDown(mousePos, touchID);
                else
                    monScreenMouseDown_28.Invoke(this.instance, mousePos, touchID);
            }

            public override void onScreenMouseUp(UnityEngine.Vector3 mousePos, System.Int32 touchID)
            {
                if (monScreenMouseUp_29.CheckShouldInvokeBase(this.instance))
                    base.onScreenMouseUp(mousePos, touchID);
                else
                    monScreenMouseUp_29.Invoke(this.instance, mousePos, touchID);
            }

            public override void onReceiveDrag(global::IMouseEventCollect dragObj, global::BOOL continueEvent)
            {
                if (monReceiveDrag_30.CheckShouldInvokeBase(this.instance))
                    base.onReceiveDrag(dragObj, continueEvent);
                else
                    monReceiveDrag_30.Invoke(this.instance, dragObj, continueEvent);
            }

            public override void onDragHoverd(global::IMouseEventCollect dragObj, System.Boolean hover)
            {
                if (monDragHoverd_31.CheckShouldInvokeBase(this.instance))
                    base.onDragHoverd(dragObj, hover);
                else
                    monDragHoverd_31.Invoke(this.instance, dragObj, hover);
            }

            public override void onMultiTouchStart(UnityEngine.Vector3 touch0, UnityEngine.Vector3 touch1)
            {
                if (monMultiTouchStart_32.CheckShouldInvokeBase(this.instance))
                    base.onMultiTouchStart(touch0, touch1);
                else
                    monMultiTouchStart_32.Invoke(this.instance, touch0, touch1);
            }

            public override void onMultiTouchMove(UnityEngine.Vector3 touch0, UnityEngine.Vector3 lastTouch0, UnityEngine.Vector3 touch1, UnityEngine.Vector3 lastTouch1)
            {
                if (monMultiTouchMove_33.CheckShouldInvokeBase(this.instance))
                    base.onMultiTouchMove(touch0, lastTouch0, touch1, lastTouch1);
                else
                    monMultiTouchMove_33.Invoke(this.instance, touch0, lastTouch0, touch1, lastTouch1);
            }

            public override void onMultiTouchEnd()
            {
                if (monMultiTouchEnd_34.CheckShouldInvokeBase(this.instance))
                    base.onMultiTouchEnd();
                else
                    monMultiTouchEnd_34.Invoke(this.instance);
            }

            public override void setActive(System.Boolean active)
            {
                if (msetActive_35.CheckShouldInvokeBase(this.instance))
                    base.setActive(active);
                else
                    msetActive_35.Invoke(this.instance, active);
            }

            public override void setName(System.String name)
            {
                if (msetName_36.CheckShouldInvokeBase(this.instance))
                    base.setName(name);
                else
                    msetName_36.Invoke(this.instance, name);
            }

            public override System.Boolean raycast(ref UnityEngine.Ray ray, out UnityEngine.RaycastHit hit, System.Single maxDistance)
            {
                if (mraycast_37.CheckShouldInvokeBase(this.instance))
                    return base.raycast(ref ray, out hit, maxDistance);
                else
                    return mraycast_37.Invoke(this.instance, ref ray, out hit, maxDistance);
            }

            public override UnityEngine.GameObject getObject()
            {
                if (mgetObject_38.CheckShouldInvokeBase(this.instance))
                    return base.getObject();
                else
                    return mgetObject_38.Invoke(this.instance);
            }

            public override System.Boolean isActive()
            {
                if (misActive_39.CheckShouldInvokeBase(this.instance))
                    return base.isActive();
                else
                    return misActive_39.Invoke(this.instance);
            }

            public override System.Boolean isActiveInHierarchy()
            {
                if (misActiveInHierarchy_40.CheckShouldInvokeBase(this.instance))
                    return base.isActiveInHierarchy();
                else
                    return misActiveInHierarchy_40.Invoke(this.instance);
            }

            public override System.Boolean isEnable()
            {
                if (misEnable_41.CheckShouldInvokeBase(this.instance))
                    return base.isEnable();
                else
                    return misEnable_41.Invoke(this.instance);
            }

            public override void setEnable(System.Boolean enable)
            {
                if (msetEnable_42.CheckShouldInvokeBase(this.instance))
                    base.setEnable(enable);
                else
                    msetEnable_42.Invoke(this.instance, enable);
            }

            public override UnityEngine.Vector3 getPosition()
            {
                if (mgetPosition_43.CheckShouldInvokeBase(this.instance))
                    return base.getPosition();
                else
                    return mgetPosition_43.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getRotation()
            {
                if (mgetRotation_44.CheckShouldInvokeBase(this.instance))
                    return base.getRotation();
                else
                    return mgetRotation_44.Invoke(this.instance);
            }

            public override void cloneFrom(global::myUIObject obj)
            {
                if (mcloneFrom_45.CheckShouldInvokeBase(this.instance))
                    base.cloneFrom(obj);
                else
                    mcloneFrom_45.Invoke(this.instance, obj);
            }

            public override UnityEngine.Vector3 getScale()
            {
                if (mgetScale_46.CheckShouldInvokeBase(this.instance))
                    return base.getScale();
                else
                    return mgetScale_46.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldPosition()
            {
                if (mgetWorldPosition_47.CheckShouldInvokeBase(this.instance))
                    return base.getWorldPosition();
                else
                    return mgetWorldPosition_47.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldScale()
            {
                if (mgetWorldScale_48.CheckShouldInvokeBase(this.instance))
                    return base.getWorldScale();
                else
                    return mgetWorldScale_48.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldRotation()
            {
                if (mgetWorldRotation_49.CheckShouldInvokeBase(this.instance))
                    return base.getWorldRotation();
                else
                    return mgetWorldRotation_49.Invoke(this.instance);
            }

            public override void setPosition(UnityEngine.Vector3 pos)
            {
                if (msetPosition_50.CheckShouldInvokeBase(this.instance))
                    base.setPosition(pos);
                else
                    msetPosition_50.Invoke(this.instance, pos);
            }

            public override void setScale(UnityEngine.Vector3 scale)
            {
                if (msetScale_51.CheckShouldInvokeBase(this.instance))
                    base.setScale(scale);
                else
                    msetScale_51.Invoke(this.instance, scale);
            }

            public override void setRotation(UnityEngine.Vector3 rot)
            {
                if (msetRotation_52.CheckShouldInvokeBase(this.instance))
                    base.setRotation(rot);
                else
                    msetRotation_52.Invoke(this.instance, rot);
            }

            public override void setWorldPosition(UnityEngine.Vector3 pos)
            {
                if (msetWorldPosition_53.CheckShouldInvokeBase(this.instance))
                    base.setWorldPosition(pos);
                else
                    msetWorldPosition_53.Invoke(this.instance, pos);
            }

            public override void setWorldRotation(UnityEngine.Vector3 rot)
            {
                if (msetWorldRotation_54.CheckShouldInvokeBase(this.instance))
                    base.setWorldRotation(rot);
                else
                    msetWorldRotation_54.Invoke(this.instance, rot);
            }

            public override void setWorldScale(UnityEngine.Vector3 scale)
            {
                if (msetWorldScale_55.CheckShouldInvokeBase(this.instance))
                    base.setWorldScale(scale);
                else
                    msetWorldScale_55.Invoke(this.instance, scale);
            }

            public override void move(UnityEngine.Vector3 moveDelta, UnityEngine.Space space)
            {
                if (mmove_56.CheckShouldInvokeBase(this.instance))
                    base.move(moveDelta, space);
                else
                    mmove_56.Invoke(this.instance, moveDelta, space);
            }

            public override System.Boolean isChildOf(global::IMouseEventCollect parent)
            {
                if (misChildOf_57.CheckShouldInvokeBase(this.instance))
                    return base.isChildOf(parent);
                else
                    return misChildOf_57.Invoke(this.instance, parent);
            }

            public override void setAlpha(System.Single alpha)
            {
                if (msetAlpha_58.CheckShouldInvokeBase(this.instance))
                    base.setAlpha(alpha);
                else
                    msetAlpha_58.Invoke(this.instance, alpha);
            }

            public override System.Single getAlpha()
            {
                if (mgetAlpha_59.CheckShouldInvokeBase(this.instance))
                    return base.getAlpha();
                else
                    return mgetAlpha_59.Invoke(this.instance);
            }

            public override void lateUpdate(System.Single elapsedTime)
            {
                if (mlateUpdate_60.CheckShouldInvokeBase(this.instance))
                    base.lateUpdate(elapsedTime);
                else
                    mlateUpdate_60.Invoke(this.instance, elapsedTime);
            }

            public override void notifyAddComponent(global::GameComponent com)
            {
                if (mnotifyAddComponent_61.CheckShouldInvokeBase(this.instance))
                    base.notifyAddComponent(com);
                else
                    mnotifyAddComponent_61.Invoke(this.instance, com);
            }

            public override void setIgnoreTimeScale(System.Boolean ignore, System.Boolean componentOnly)
            {
                if (msetIgnoreTimeScale_62.CheckShouldInvokeBase(this.instance))
                    base.setIgnoreTimeScale(ignore, componentOnly);
                else
                    msetIgnoreTimeScale_62.Invoke(this.instance, ignore, componentOnly);
            }

            public override void setDestroy(System.Boolean isDestroy)
            {
                if (msetDestroy_63.CheckShouldInvokeBase(this.instance))
                    base.setDestroy(isDestroy);
                else
                    msetDestroy_63.Invoke(this.instance, isDestroy);
            }

            public override System.Boolean isDestroy()
            {
                if (misDestroy_64.CheckShouldInvokeBase(this.instance))
                    return base.isDestroy();
                else
                    return misDestroy_64.Invoke(this.instance);
            }

            public override void setAssignID(System.Int64 assignID)
            {
                if (msetAssignID_65.CheckShouldInvokeBase(this.instance))
                    base.setAssignID(assignID);
                else
                    msetAssignID_65.Invoke(this.instance, assignID);
            }

            public override System.Int64 getAssignID()
            {
                if (mgetAssignID_66.CheckShouldInvokeBase(this.instance))
                    return base.getAssignID();
                else
                    return mgetAssignID_66.Invoke(this.instance);
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

