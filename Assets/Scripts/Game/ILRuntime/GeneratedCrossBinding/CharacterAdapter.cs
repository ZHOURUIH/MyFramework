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
        static CrossBindingMethodInfo mdestroyModel_2 = new CrossBindingMethodInfo("destroyModel");
        static CrossBindingFunctionInfo<System.String, System.Single> mgetAnimationLength_3 = new CrossBindingFunctionInfo<System.String, System.Single>("getAnimationLength");
        static CrossBindingMethodInfo mnotifyModelLoaded_4 = new CrossBindingMethodInfo("notifyModelLoaded");
        static CrossBindingFunctionInfo<global::CharacterData> mcreateCharacterData_5 = new CrossBindingFunctionInfo<global::CharacterData>("createCharacterData");
        static CrossBindingMethodInfo minitComponents_6 = new CrossBindingMethodInfo("initComponents");
        static CrossBindingMethodInfo mdestroy_7 = new CrossBindingMethodInfo("destroy");
        static CrossBindingMethodInfo<UnityEngine.GameObject, System.Boolean> msetObject_8 = new CrossBindingMethodInfo<UnityEngine.GameObject, System.Boolean>("setObject");
        static CrossBindingMethodInfo<System.Single> mupdate_9 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo<System.Single> mfixedUpdate_10 = new CrossBindingMethodInfo<System.Single>("fixedUpdate");
        static CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3> mlocalToWorld_11 = new CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3>("localToWorld");
        static CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3> mworldToLocal_12 = new CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3>("worldToLocal");
        static CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3> mlocalToWorldDirection_13 = new CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3>("localToWorldDirection");
        static CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3> mworldToLocalDirection_14 = new CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3>("worldToLocalDirection");
        static CrossBindingFunctionInfo<UnityEngine.GameObject> mgetObject_15 = new CrossBindingFunctionInfo<UnityEngine.GameObject>("getObject");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetPosition_16 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getPosition");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetRotation_17 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getRotation");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetScale_18 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getScale");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldPosition_19 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldPosition");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldScale_20 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldScale");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldRotation_21 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldRotation");
        static CrossBindingFunctionInfo<System.Boolean> misActive_22 = new CrossBindingFunctionInfo<System.Boolean>("isActive");
        static CrossBindingFunctionInfo<System.Boolean> misActiveInHierarchy_23 = new CrossBindingFunctionInfo<System.Boolean>("isActiveInHierarchy");
        static CrossBindingFunctionInfo<System.Boolean> misHandleInput_24 = new CrossBindingFunctionInfo<System.Boolean>("isHandleInput");
        static CrossBindingFunctionInfo<UnityEngine.Collider> mgetCollider_25 = new CrossBindingFunctionInfo<UnityEngine.Collider>("getCollider");
        static CrossBindingFunctionInfo<global::UIDepth> mgetDepth_26 = new CrossBindingFunctionInfo<global::UIDepth>("getDepth");
        static CrossBindingFunctionInfo<System.Boolean> misReceiveScreenMouse_27 = new CrossBindingFunctionInfo<System.Boolean>("isReceiveScreenMouse");
        static CrossBindingFunctionInfo<System.Boolean> misPassRay_28 = new CrossBindingFunctionInfo<System.Boolean>("isPassRay");
        static CrossBindingFunctionInfo<System.Boolean> misDragable_29 = new CrossBindingFunctionInfo<System.Boolean>("isDragable");
        static CrossBindingFunctionInfo<System.Boolean> misMouseHovered_30 = new CrossBindingFunctionInfo<System.Boolean>("isMouseHovered");
        static CrossBindingFunctionInfo<global::IMouseEventCollect, System.Boolean> misChildOf_31 = new CrossBindingFunctionInfo<global::IMouseEventCollect, System.Boolean>("isChildOf");
        static CrossBindingMethodInfo<System.Boolean> msetPassRay_32 = new CrossBindingMethodInfo<System.Boolean>("setPassRay");
        static CrossBindingMethodInfo<System.Boolean> msetHandleInput_33 = new CrossBindingMethodInfo<System.Boolean>("setHandleInput");
        static CrossBindingMethodInfo<System.String> msetName_34 = new CrossBindingMethodInfo<System.String>("setName");
        static CrossBindingMethodInfo<System.Boolean> msetActive_35 = new CrossBindingMethodInfo<System.Boolean>("setActive");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetPosition_36 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setPosition");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetScale_37 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setScale");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetRotation_38 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setRotation");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldPosition_39 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldPosition");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldRotation_40 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldRotation");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldScale_41 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldScale");
        static CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Space> mmove_42 = new CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Space>("move");
        static CrossBindingMethodInfo<global::ObjectClickCallback> msetClickCallback_43 = new CrossBindingMethodInfo<global::ObjectClickCallback>("setClickCallback");
        static CrossBindingMethodInfo<global::ObjectHoverCallback> msetHoverCallback_44 = new CrossBindingMethodInfo<global::ObjectHoverCallback>("setHoverCallback");
        static CrossBindingMethodInfo<global::ObjectPressCallback> msetPressCallback_45 = new CrossBindingMethodInfo<global::ObjectPressCallback>("setPressCallback");
        static CrossBindingMethodInfo<System.Int32> monMouseEnter_46 = new CrossBindingMethodInfo<System.Int32>("onMouseEnter");
        static CrossBindingMethodInfo<System.Int32> monMouseLeave_47 = new CrossBindingMethodInfo<System.Int32>("onMouseLeave");
        static CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32> monMouseDown_48 = new CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32>("onMouseDown");
        static CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32> monMouseUp_49 = new CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32>("onMouseUp");
        static CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Vector3, System.Single, System.Int32> monMouseMove_50 = new CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Vector3, System.Single, System.Int32>("onMouseMove");
        static CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32> monMouseStay_51 = new CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32>("onMouseStay");
        static CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32> monScreenMouseDown_52 = new CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32>("onScreenMouseDown");
        static CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32> monScreenMouseUp_53 = new CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32>("onScreenMouseUp");
        static CrossBindingMethodInfo<global::IMouseEventCollect, global::BOOL> monReceiveDrag_54 = new CrossBindingMethodInfo<global::IMouseEventCollect, global::BOOL>("onReceiveDrag");
        static CrossBindingMethodInfo<global::IMouseEventCollect, System.Boolean> monDragHoverd_55 = new CrossBindingMethodInfo<global::IMouseEventCollect, System.Boolean>("onDragHoverd");
        static CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Vector3> monMultiTouchStart_56 = new CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Vector3>("onMultiTouchStart");
        static CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector3> monMultiTouchMove_57 = new CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector3>("onMultiTouchMove");
        static CrossBindingMethodInfo monMultiTouchEnd_58 = new CrossBindingMethodInfo("onMultiTouchEnd");
        static CrossBindingMethodInfo<System.Single> msetAlpha_59 = new CrossBindingMethodInfo<System.Single>("setAlpha");
        static CrossBindingFunctionInfo<System.Single> mgetAlpha_60 = new CrossBindingFunctionInfo<System.Single>("getAlpha");
        static CrossBindingFunctionInfo<System.Boolean> misEnable_61 = new CrossBindingFunctionInfo<System.Boolean>("isEnable");
        static CrossBindingMethodInfo<System.Boolean> msetEnable_62 = new CrossBindingMethodInfo<System.Boolean>("setEnable");
        static CrossBindingMethodInfo<System.Single> mlateUpdate_63 = new CrossBindingMethodInfo<System.Single>("lateUpdate");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyAddComponent_64 = new CrossBindingMethodInfo<global::GameComponent>("notifyAddComponent");
        static CrossBindingMethodInfo<System.Boolean, System.Boolean> msetIgnoreTimeScale_65 = new CrossBindingMethodInfo<System.Boolean, System.Boolean>("setIgnoreTimeScale");
        static CrossBindingMethodInfo mnotifyConstructDone_66 = new CrossBindingMethodInfo("notifyConstructDone");
        static CrossBindingMethodInfo<System.Boolean> msetDestroy_67 = new CrossBindingMethodInfo<System.Boolean>("setDestroy");
        static CrossBindingFunctionInfo<System.Boolean> misDestroy_68 = new CrossBindingFunctionInfo<System.Boolean>("isDestroy");
        static CrossBindingMethodInfo<System.Int64> msetAssignID_69 = new CrossBindingMethodInfo<System.Int64>("setAssignID");
        static CrossBindingFunctionInfo<System.Int64> mgetAssignID_70 = new CrossBindingFunctionInfo<System.Int64>("getAssignID");
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

            public override void destroyModel()
            {
                if (mdestroyModel_2.CheckShouldInvokeBase(this.instance))
                    base.destroyModel();
                else
                    mdestroyModel_2.Invoke(this.instance);
            }

            public override System.Single getAnimationLength(System.String name)
            {
                if (mgetAnimationLength_3.CheckShouldInvokeBase(this.instance))
                    return base.getAnimationLength(name);
                else
                    return mgetAnimationLength_3.Invoke(this.instance, name);
            }

            public override void notifyModelLoaded()
            {
                if (mnotifyModelLoaded_4.CheckShouldInvokeBase(this.instance))
                    base.notifyModelLoaded();
                else
                    mnotifyModelLoaded_4.Invoke(this.instance);
            }

            protected override global::CharacterData createCharacterData()
            {
                if (mcreateCharacterData_5.CheckShouldInvokeBase(this.instance))
                    return base.createCharacterData();
                else
                    return mcreateCharacterData_5.Invoke(this.instance);
            }

            protected override void initComponents()
            {
                if (minitComponents_6.CheckShouldInvokeBase(this.instance))
                    base.initComponents();
                else
                    minitComponents_6.Invoke(this.instance);
            }

            public override void destroy()
            {
                if (mdestroy_7.CheckShouldInvokeBase(this.instance))
                    base.destroy();
                else
                    mdestroy_7.Invoke(this.instance);
            }

            public override void setObject(UnityEngine.GameObject obj, System.Boolean destroyOld)
            {
                if (msetObject_8.CheckShouldInvokeBase(this.instance))
                    base.setObject(obj, destroyOld);
                else
                    msetObject_8.Invoke(this.instance, obj, destroyOld);
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

            public override UnityEngine.Vector3 localToWorld(UnityEngine.Vector3 point)
            {
                if (mlocalToWorld_11.CheckShouldInvokeBase(this.instance))
                    return base.localToWorld(point);
                else
                    return mlocalToWorld_11.Invoke(this.instance, point);
            }

            public override UnityEngine.Vector3 worldToLocal(UnityEngine.Vector3 point)
            {
                if (mworldToLocal_12.CheckShouldInvokeBase(this.instance))
                    return base.worldToLocal(point);
                else
                    return mworldToLocal_12.Invoke(this.instance, point);
            }

            public override UnityEngine.Vector3 localToWorldDirection(UnityEngine.Vector3 direction)
            {
                if (mlocalToWorldDirection_13.CheckShouldInvokeBase(this.instance))
                    return base.localToWorldDirection(direction);
                else
                    return mlocalToWorldDirection_13.Invoke(this.instance, direction);
            }

            public override UnityEngine.Vector3 worldToLocalDirection(UnityEngine.Vector3 direction)
            {
                if (mworldToLocalDirection_14.CheckShouldInvokeBase(this.instance))
                    return base.worldToLocalDirection(direction);
                else
                    return mworldToLocalDirection_14.Invoke(this.instance, direction);
            }

            public override UnityEngine.GameObject getObject()
            {
                if (mgetObject_15.CheckShouldInvokeBase(this.instance))
                    return base.getObject();
                else
                    return mgetObject_15.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getPosition()
            {
                if (mgetPosition_16.CheckShouldInvokeBase(this.instance))
                    return base.getPosition();
                else
                    return mgetPosition_16.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getRotation()
            {
                if (mgetRotation_17.CheckShouldInvokeBase(this.instance))
                    return base.getRotation();
                else
                    return mgetRotation_17.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getScale()
            {
                if (mgetScale_18.CheckShouldInvokeBase(this.instance))
                    return base.getScale();
                else
                    return mgetScale_18.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldPosition()
            {
                if (mgetWorldPosition_19.CheckShouldInvokeBase(this.instance))
                    return base.getWorldPosition();
                else
                    return mgetWorldPosition_19.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldScale()
            {
                if (mgetWorldScale_20.CheckShouldInvokeBase(this.instance))
                    return base.getWorldScale();
                else
                    return mgetWorldScale_20.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldRotation()
            {
                if (mgetWorldRotation_21.CheckShouldInvokeBase(this.instance))
                    return base.getWorldRotation();
                else
                    return mgetWorldRotation_21.Invoke(this.instance);
            }

            public override System.Boolean isActive()
            {
                if (misActive_22.CheckShouldInvokeBase(this.instance))
                    return base.isActive();
                else
                    return misActive_22.Invoke(this.instance);
            }

            public override System.Boolean isActiveInHierarchy()
            {
                if (misActiveInHierarchy_23.CheckShouldInvokeBase(this.instance))
                    return base.isActiveInHierarchy();
                else
                    return misActiveInHierarchy_23.Invoke(this.instance);
            }

            public override System.Boolean isHandleInput()
            {
                if (misHandleInput_24.CheckShouldInvokeBase(this.instance))
                    return base.isHandleInput();
                else
                    return misHandleInput_24.Invoke(this.instance);
            }

            public override UnityEngine.Collider getCollider()
            {
                if (mgetCollider_25.CheckShouldInvokeBase(this.instance))
                    return base.getCollider();
                else
                    return mgetCollider_25.Invoke(this.instance);
            }

            public override global::UIDepth getDepth()
            {
                if (mgetDepth_26.CheckShouldInvokeBase(this.instance))
                    return base.getDepth();
                else
                    return mgetDepth_26.Invoke(this.instance);
            }

            public override System.Boolean isReceiveScreenMouse()
            {
                if (misReceiveScreenMouse_27.CheckShouldInvokeBase(this.instance))
                    return base.isReceiveScreenMouse();
                else
                    return misReceiveScreenMouse_27.Invoke(this.instance);
            }

            public override System.Boolean isPassRay()
            {
                if (misPassRay_28.CheckShouldInvokeBase(this.instance))
                    return base.isPassRay();
                else
                    return misPassRay_28.Invoke(this.instance);
            }

            public override System.Boolean isDragable()
            {
                if (misDragable_29.CheckShouldInvokeBase(this.instance))
                    return base.isDragable();
                else
                    return misDragable_29.Invoke(this.instance);
            }

            public override System.Boolean isMouseHovered()
            {
                if (misMouseHovered_30.CheckShouldInvokeBase(this.instance))
                    return base.isMouseHovered();
                else
                    return misMouseHovered_30.Invoke(this.instance);
            }

            public override System.Boolean isChildOf(global::IMouseEventCollect parent)
            {
                if (misChildOf_31.CheckShouldInvokeBase(this.instance))
                    return base.isChildOf(parent);
                else
                    return misChildOf_31.Invoke(this.instance, parent);
            }

            public override void setPassRay(System.Boolean passRay)
            {
                if (msetPassRay_32.CheckShouldInvokeBase(this.instance))
                    base.setPassRay(passRay);
                else
                    msetPassRay_32.Invoke(this.instance, passRay);
            }

            public override void setHandleInput(System.Boolean handleInput)
            {
                if (msetHandleInput_33.CheckShouldInvokeBase(this.instance))
                    base.setHandleInput(handleInput);
                else
                    msetHandleInput_33.Invoke(this.instance, handleInput);
            }

            public override void setName(System.String name)
            {
                if (msetName_34.CheckShouldInvokeBase(this.instance))
                    base.setName(name);
                else
                    msetName_34.Invoke(this.instance, name);
            }

            public override void setActive(System.Boolean active)
            {
                if (msetActive_35.CheckShouldInvokeBase(this.instance))
                    base.setActive(active);
                else
                    msetActive_35.Invoke(this.instance, active);
            }

            public override void setPosition(UnityEngine.Vector3 pos)
            {
                if (msetPosition_36.CheckShouldInvokeBase(this.instance))
                    base.setPosition(pos);
                else
                    msetPosition_36.Invoke(this.instance, pos);
            }

            public override void setScale(UnityEngine.Vector3 scale)
            {
                if (msetScale_37.CheckShouldInvokeBase(this.instance))
                    base.setScale(scale);
                else
                    msetScale_37.Invoke(this.instance, scale);
            }

            public override void setRotation(UnityEngine.Vector3 rot)
            {
                if (msetRotation_38.CheckShouldInvokeBase(this.instance))
                    base.setRotation(rot);
                else
                    msetRotation_38.Invoke(this.instance, rot);
            }

            public override void setWorldPosition(UnityEngine.Vector3 pos)
            {
                if (msetWorldPosition_39.CheckShouldInvokeBase(this.instance))
                    base.setWorldPosition(pos);
                else
                    msetWorldPosition_39.Invoke(this.instance, pos);
            }

            public override void setWorldRotation(UnityEngine.Vector3 rot)
            {
                if (msetWorldRotation_40.CheckShouldInvokeBase(this.instance))
                    base.setWorldRotation(rot);
                else
                    msetWorldRotation_40.Invoke(this.instance, rot);
            }

            public override void setWorldScale(UnityEngine.Vector3 scale)
            {
                if (msetWorldScale_41.CheckShouldInvokeBase(this.instance))
                    base.setWorldScale(scale);
                else
                    msetWorldScale_41.Invoke(this.instance, scale);
            }

            public override void move(UnityEngine.Vector3 moveDelta, UnityEngine.Space space)
            {
                if (mmove_42.CheckShouldInvokeBase(this.instance))
                    base.move(moveDelta, space);
                else
                    mmove_42.Invoke(this.instance, moveDelta, space);
            }

            public override void setClickCallback(global::ObjectClickCallback callback)
            {
                if (msetClickCallback_43.CheckShouldInvokeBase(this.instance))
                    base.setClickCallback(callback);
                else
                    msetClickCallback_43.Invoke(this.instance, callback);
            }

            public override void setHoverCallback(global::ObjectHoverCallback callback)
            {
                if (msetHoverCallback_44.CheckShouldInvokeBase(this.instance))
                    base.setHoverCallback(callback);
                else
                    msetHoverCallback_44.Invoke(this.instance, callback);
            }

            public override void setPressCallback(global::ObjectPressCallback callback)
            {
                if (msetPressCallback_45.CheckShouldInvokeBase(this.instance))
                    base.setPressCallback(callback);
                else
                    msetPressCallback_45.Invoke(this.instance, callback);
            }

            public override void onMouseEnter(System.Int32 touchID)
            {
                if (monMouseEnter_46.CheckShouldInvokeBase(this.instance))
                    base.onMouseEnter(touchID);
                else
                    monMouseEnter_46.Invoke(this.instance, touchID);
            }

            public override void onMouseLeave(System.Int32 touchID)
            {
                if (monMouseLeave_47.CheckShouldInvokeBase(this.instance))
                    base.onMouseLeave(touchID);
                else
                    monMouseLeave_47.Invoke(this.instance, touchID);
            }

            public override void onMouseDown(UnityEngine.Vector3 mousePos, System.Int32 touchID)
            {
                if (monMouseDown_48.CheckShouldInvokeBase(this.instance))
                    base.onMouseDown(mousePos, touchID);
                else
                    monMouseDown_48.Invoke(this.instance, mousePos, touchID);
            }

            public override void onMouseUp(UnityEngine.Vector3 mousePos, System.Int32 touchID)
            {
                if (monMouseUp_49.CheckShouldInvokeBase(this.instance))
                    base.onMouseUp(mousePos, touchID);
                else
                    monMouseUp_49.Invoke(this.instance, mousePos, touchID);
            }

            public override void onMouseMove(UnityEngine.Vector3 mousePos, UnityEngine.Vector3 moveDelta, System.Single moveTime, System.Int32 touchID)
            {
                if (monMouseMove_50.CheckShouldInvokeBase(this.instance))
                    base.onMouseMove(mousePos, moveDelta, moveTime, touchID);
                else
                    monMouseMove_50.Invoke(this.instance, mousePos, moveDelta, moveTime, touchID);
            }

            public override void onMouseStay(UnityEngine.Vector3 mousePos, System.Int32 touchID)
            {
                if (monMouseStay_51.CheckShouldInvokeBase(this.instance))
                    base.onMouseStay(mousePos, touchID);
                else
                    monMouseStay_51.Invoke(this.instance, mousePos, touchID);
            }

            public override void onScreenMouseDown(UnityEngine.Vector3 mousePos, System.Int32 touchID)
            {
                if (monScreenMouseDown_52.CheckShouldInvokeBase(this.instance))
                    base.onScreenMouseDown(mousePos, touchID);
                else
                    monScreenMouseDown_52.Invoke(this.instance, mousePos, touchID);
            }

            public override void onScreenMouseUp(UnityEngine.Vector3 mousePos, System.Int32 touchID)
            {
                if (monScreenMouseUp_53.CheckShouldInvokeBase(this.instance))
                    base.onScreenMouseUp(mousePos, touchID);
                else
                    monScreenMouseUp_53.Invoke(this.instance, mousePos, touchID);
            }

            public override void onReceiveDrag(global::IMouseEventCollect dragObj, global::BOOL continueEvent)
            {
                if (monReceiveDrag_54.CheckShouldInvokeBase(this.instance))
                    base.onReceiveDrag(dragObj, continueEvent);
                else
                    monReceiveDrag_54.Invoke(this.instance, dragObj, continueEvent);
            }

            public override void onDragHoverd(global::IMouseEventCollect dragObj, System.Boolean hover)
            {
                if (monDragHoverd_55.CheckShouldInvokeBase(this.instance))
                    base.onDragHoverd(dragObj, hover);
                else
                    monDragHoverd_55.Invoke(this.instance, dragObj, hover);
            }

            public override void onMultiTouchStart(UnityEngine.Vector3 touch0, UnityEngine.Vector3 touch1)
            {
                if (monMultiTouchStart_56.CheckShouldInvokeBase(this.instance))
                    base.onMultiTouchStart(touch0, touch1);
                else
                    monMultiTouchStart_56.Invoke(this.instance, touch0, touch1);
            }

            public override void onMultiTouchMove(UnityEngine.Vector3 touch0, UnityEngine.Vector3 lastTouch0, UnityEngine.Vector3 touch1, UnityEngine.Vector3 lastTouch1)
            {
                if (monMultiTouchMove_57.CheckShouldInvokeBase(this.instance))
                    base.onMultiTouchMove(touch0, lastTouch0, touch1, lastTouch1);
                else
                    monMultiTouchMove_57.Invoke(this.instance, touch0, lastTouch0, touch1, lastTouch1);
            }

            public override void onMultiTouchEnd()
            {
                if (monMultiTouchEnd_58.CheckShouldInvokeBase(this.instance))
                    base.onMultiTouchEnd();
                else
                    monMultiTouchEnd_58.Invoke(this.instance);
            }

            public override void setAlpha(System.Single alpha)
            {
                if (msetAlpha_59.CheckShouldInvokeBase(this.instance))
                    base.setAlpha(alpha);
                else
                    msetAlpha_59.Invoke(this.instance, alpha);
            }

            public override System.Single getAlpha()
            {
                if (mgetAlpha_60.CheckShouldInvokeBase(this.instance))
                    return base.getAlpha();
                else
                    return mgetAlpha_60.Invoke(this.instance);
            }

            public override System.Boolean isEnable()
            {
                if (misEnable_61.CheckShouldInvokeBase(this.instance))
                    return base.isEnable();
                else
                    return misEnable_61.Invoke(this.instance);
            }

            public override void setEnable(System.Boolean enable)
            {
                if (msetEnable_62.CheckShouldInvokeBase(this.instance))
                    base.setEnable(enable);
                else
                    msetEnable_62.Invoke(this.instance, enable);
            }

            public override void lateUpdate(System.Single elapsedTime)
            {
                if (mlateUpdate_63.CheckShouldInvokeBase(this.instance))
                    base.lateUpdate(elapsedTime);
                else
                    mlateUpdate_63.Invoke(this.instance, elapsedTime);
            }

            public override void notifyAddComponent(global::GameComponent com)
            {
                if (mnotifyAddComponent_64.CheckShouldInvokeBase(this.instance))
                    base.notifyAddComponent(com);
                else
                    mnotifyAddComponent_64.Invoke(this.instance, com);
            }

            public override void setIgnoreTimeScale(System.Boolean ignore, System.Boolean componentOnly)
            {
                if (msetIgnoreTimeScale_65.CheckShouldInvokeBase(this.instance))
                    base.setIgnoreTimeScale(ignore, componentOnly);
                else
                    msetIgnoreTimeScale_65.Invoke(this.instance, ignore, componentOnly);
            }

            public override void notifyConstructDone()
            {
                if (mnotifyConstructDone_66.CheckShouldInvokeBase(this.instance))
                    base.notifyConstructDone();
                else
                    mnotifyConstructDone_66.Invoke(this.instance);
            }

            public override void setDestroy(System.Boolean isDestroy)
            {
                if (msetDestroy_67.CheckShouldInvokeBase(this.instance))
                    base.setDestroy(isDestroy);
                else
                    msetDestroy_67.Invoke(this.instance, isDestroy);
            }

            public override System.Boolean isDestroy()
            {
                if (misDestroy_68.CheckShouldInvokeBase(this.instance))
                    return base.isDestroy();
                else
                    return misDestroy_68.Invoke(this.instance);
            }

            public override void setAssignID(System.Int64 assignID)
            {
                if (msetAssignID_69.CheckShouldInvokeBase(this.instance))
                    base.setAssignID(assignID);
                else
                    msetAssignID_69.Invoke(this.instance, assignID);
            }

            public override System.Int64 getAssignID()
            {
                if (mgetAssignID_70.CheckShouldInvokeBase(this.instance))
                    return base.getAssignID();
                else
                    return mgetAssignID_70.Invoke(this.instance);
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

