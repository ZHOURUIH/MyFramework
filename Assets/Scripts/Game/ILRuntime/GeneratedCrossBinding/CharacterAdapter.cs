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
    public class CharacterAdapter : CrossBindingAdaptor
    {
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
            CrossBindingMethodInfo minit_0 = new CrossBindingMethodInfo("init");
            CrossBindingMethodInfo mresetProperty_1 = new CrossBindingMethodInfo("resetProperty");
            CrossBindingMethodInfo mdestroy_2 = new CrossBindingMethodInfo("destroy");
            CrossBindingMethodInfo<UnityEngine.GameObject> msetObject_3 = new CrossBindingMethodInfo<UnityEngine.GameObject>("setObject");
            CrossBindingMethodInfo mdestroyModel_4 = new CrossBindingMethodInfo("destroyModel");
            CrossBindingFunctionInfo<System.String, System.Single> mgetAnimationLength_5 = new CrossBindingFunctionInfo<System.String, System.Single>("getAnimationLength");
            CrossBindingMethodInfo mnotifyModelLoaded_6 = new CrossBindingMethodInfo("notifyModelLoaded");
            CrossBindingFunctionInfo<global::CharacterData> mcreateCharacterData_7 = new CrossBindingFunctionInfo<global::CharacterData>("createCharacterData");
            CrossBindingMethodInfo minitComponents_8 = new CrossBindingMethodInfo("initComponents");
            CrossBindingFunctionInfo<global::UIDepth> mgetDepth_9 = new CrossBindingFunctionInfo<global::UIDepth>("getDepth");
            CrossBindingFunctionInfo<System.Boolean> misHandleInput_10 = new CrossBindingFunctionInfo<System.Boolean>("isHandleInput");
            CrossBindingFunctionInfo<System.Boolean> misReceiveScreenMouse_11 = new CrossBindingFunctionInfo<System.Boolean>("isReceiveScreenMouse");
            CrossBindingFunctionInfo<System.Boolean> misPassRay_12 = new CrossBindingFunctionInfo<System.Boolean>("isPassRay");
            CrossBindingFunctionInfo<System.Boolean> misMouseHovered_13 = new CrossBindingFunctionInfo<System.Boolean>("isMouseHovered");
            CrossBindingFunctionInfo<System.Boolean> misDragable_14 = new CrossBindingFunctionInfo<System.Boolean>("isDragable");
            CrossBindingMethodInfo<System.Boolean> msetPassRay_15 = new CrossBindingMethodInfo<System.Boolean>("setPassRay");
            CrossBindingMethodInfo<System.Boolean> msetHandleInput_16 = new CrossBindingMethodInfo<System.Boolean>("setHandleInput");
            CrossBindingMethodInfo<global::ObjectClickCallback> msetClickCallback_17 = new CrossBindingMethodInfo<global::ObjectClickCallback>("setClickCallback");
            CrossBindingMethodInfo<global::ObjectHoverCallback> msetHoverCallback_18 = new CrossBindingMethodInfo<global::ObjectHoverCallback>("setHoverCallback");
            CrossBindingMethodInfo<global::ObjectPressCallback> msetPressCallback_19 = new CrossBindingMethodInfo<global::ObjectPressCallback>("setPressCallback");
            CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32> monMouseEnter_20 = new CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32>("onMouseEnter");
            CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32> monMouseLeave_21 = new CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32>("onMouseLeave");
            CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32> monMouseDown_22 = new CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32>("onMouseDown");
            CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32> monMouseUp_23 = new CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32>("onMouseUp");
            CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Vector3, System.Single, System.Int32> monMouseMove_24 = new CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Vector3, System.Single, System.Int32>("onMouseMove");
            CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32> monMouseStay_25 = new CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32>("onMouseStay");
            CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32> monScreenMouseDown_26 = new CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32>("onScreenMouseDown");
            CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32> monScreenMouseUp_27 = new CrossBindingMethodInfo<UnityEngine.Vector3, System.Int32>("onScreenMouseUp");
            CrossBindingMethodInfo<global::IMouseEventCollect, UnityEngine.Vector3, global::BOOL> monReceiveDrag_28 = new CrossBindingMethodInfo<global::IMouseEventCollect, UnityEngine.Vector3, global::BOOL>("onReceiveDrag");
            CrossBindingMethodInfo<global::IMouseEventCollect, UnityEngine.Vector3, System.Boolean> monDragHoverd_29 = new CrossBindingMethodInfo<global::IMouseEventCollect, UnityEngine.Vector3, System.Boolean>("onDragHoverd");
            CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Vector3> monMultiTouchStart_30 = new CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Vector3>("onMultiTouchStart");
            CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector3> monMultiTouchMove_31 = new CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector3, UnityEngine.Vector3>("onMultiTouchMove");
            CrossBindingMethodInfo monMultiTouchEnd_32 = new CrossBindingMethodInfo("onMultiTouchEnd");
            CrossBindingMethodInfo<System.Boolean> msetActive_33 = new CrossBindingMethodInfo<System.Boolean>("setActive");
            CrossBindingMethodInfo<System.String> msetName_34 = new CrossBindingMethodInfo<System.String>("setName");
            class raycast_35Info : CrossBindingMethodInfo
            {
                static Type[] pTypes = new Type[] {typeof(UnityEngine.Ray).MakeByRefType(), typeof(UnityEngine.RaycastHit).MakeByRefType(), typeof(System.Single), typeof(System.Boolean)};

                public raycast_35Info()
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
            raycast_35Info mraycast_35 = new raycast_35Info();
            CrossBindingFunctionInfo<UnityEngine.GameObject> mgetObject_36 = new CrossBindingFunctionInfo<UnityEngine.GameObject>("getObject");
            CrossBindingFunctionInfo<System.Boolean> misActive_37 = new CrossBindingFunctionInfo<System.Boolean>("isActive");
            CrossBindingFunctionInfo<System.Boolean> misActiveInHierarchy_38 = new CrossBindingFunctionInfo<System.Boolean>("isActiveInHierarchy");
            CrossBindingFunctionInfo<System.Boolean> misEnable_39 = new CrossBindingFunctionInfo<System.Boolean>("isEnable");
            CrossBindingMethodInfo<System.Boolean> msetEnable_40 = new CrossBindingMethodInfo<System.Boolean>("setEnable");
            CrossBindingFunctionInfo<UnityEngine.Vector3> mgetPosition_41 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getPosition");
            CrossBindingFunctionInfo<UnityEngine.Vector3> mgetRotation_42 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getRotation");
            CrossBindingMethodInfo<global::myUIObject> mcloneFrom_43 = new CrossBindingMethodInfo<global::myUIObject>("cloneFrom");
            CrossBindingFunctionInfo<UnityEngine.Vector3> mgetScale_44 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getScale");
            CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldPosition_45 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldPosition");
            CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldScale_46 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldScale");
            CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldRotation_47 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldRotation");
            CrossBindingMethodInfo<UnityEngine.Vector3> msetPosition_48 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setPosition");
            CrossBindingMethodInfo<UnityEngine.Vector3> msetScale_49 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setScale");
            CrossBindingMethodInfo<UnityEngine.Vector3> msetRotation_50 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setRotation");
            CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldPosition_51 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldPosition");
            CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldRotation_52 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldRotation");
            CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldScale_53 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldScale");
            CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Space> mmove_54 = new CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Space>("move");
            CrossBindingFunctionInfo<global::IMouseEventCollect, System.Boolean> misChildOf_55 = new CrossBindingFunctionInfo<global::IMouseEventCollect, System.Boolean>("isChildOf");
            CrossBindingMethodInfo<System.Single> msetAlpha_56 = new CrossBindingMethodInfo<System.Single>("setAlpha");
            CrossBindingFunctionInfo<System.Single> mgetAlpha_57 = new CrossBindingFunctionInfo<System.Single>("getAlpha");
            CrossBindingFunctionInfo<System.Boolean> mcanUpdate_58 = new CrossBindingFunctionInfo<System.Boolean>("canUpdate");
            CrossBindingMethodInfo<System.Single> mupdate_59 = new CrossBindingMethodInfo<System.Single>("update");
            CrossBindingMethodInfo<System.Single> mlateUpdate_60 = new CrossBindingMethodInfo<System.Single>("lateUpdate");
            CrossBindingMethodInfo<System.Single> mfixedUpdate_61 = new CrossBindingMethodInfo<System.Single>("fixedUpdate");
            CrossBindingMethodInfo<global::GameComponent> mnotifyAddComponent_62 = new CrossBindingMethodInfo<global::GameComponent>("notifyAddComponent");
            CrossBindingMethodInfo<System.Boolean, System.Boolean> msetIgnoreTimeScale_63 = new CrossBindingMethodInfo<System.Boolean, System.Boolean>("setIgnoreTimeScale");

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

            public override global::UIDepth getDepth()
            {
                if (mgetDepth_9.CheckShouldInvokeBase(this.instance))
                    return base.getDepth();
                else
                    return mgetDepth_9.Invoke(this.instance);
            }

            public override System.Boolean isHandleInput()
            {
                if (misHandleInput_10.CheckShouldInvokeBase(this.instance))
                    return base.isHandleInput();
                else
                    return misHandleInput_10.Invoke(this.instance);
            }

            public override System.Boolean isReceiveScreenMouse()
            {
                if (misReceiveScreenMouse_11.CheckShouldInvokeBase(this.instance))
                    return base.isReceiveScreenMouse();
                else
                    return misReceiveScreenMouse_11.Invoke(this.instance);
            }

            public override System.Boolean isPassRay()
            {
                if (misPassRay_12.CheckShouldInvokeBase(this.instance))
                    return base.isPassRay();
                else
                    return misPassRay_12.Invoke(this.instance);
            }

            public override System.Boolean isMouseHovered()
            {
                if (misMouseHovered_13.CheckShouldInvokeBase(this.instance))
                    return base.isMouseHovered();
                else
                    return misMouseHovered_13.Invoke(this.instance);
            }

            public override System.Boolean isDragable()
            {
                if (misDragable_14.CheckShouldInvokeBase(this.instance))
                    return base.isDragable();
                else
                    return misDragable_14.Invoke(this.instance);
            }

            public override void setPassRay(System.Boolean passRay)
            {
                if (msetPassRay_15.CheckShouldInvokeBase(this.instance))
                    base.setPassRay(passRay);
                else
                    msetPassRay_15.Invoke(this.instance, passRay);
            }

            public override void setHandleInput(System.Boolean handleInput)
            {
                if (msetHandleInput_16.CheckShouldInvokeBase(this.instance))
                    base.setHandleInput(handleInput);
                else
                    msetHandleInput_16.Invoke(this.instance, handleInput);
            }

            public override void setClickCallback(global::ObjectClickCallback callback)
            {
                if (msetClickCallback_17.CheckShouldInvokeBase(this.instance))
                    base.setClickCallback(callback);
                else
                    msetClickCallback_17.Invoke(this.instance, callback);
            }

            public override void setHoverCallback(global::ObjectHoverCallback callback)
            {
                if (msetHoverCallback_18.CheckShouldInvokeBase(this.instance))
                    base.setHoverCallback(callback);
                else
                    msetHoverCallback_18.Invoke(this.instance, callback);
            }

            public override void setPressCallback(global::ObjectPressCallback callback)
            {
                if (msetPressCallback_19.CheckShouldInvokeBase(this.instance))
                    base.setPressCallback(callback);
                else
                    msetPressCallback_19.Invoke(this.instance, callback);
            }

            public override void onMouseEnter(UnityEngine.Vector3 mousePos, System.Int32 touchID)
            {
                if (monMouseEnter_20.CheckShouldInvokeBase(this.instance))
                    base.onMouseEnter(mousePos, touchID);
                else
                    monMouseEnter_20.Invoke(this.instance, mousePos, touchID);
            }

            public override void onMouseLeave(UnityEngine.Vector3 mousePos, System.Int32 touchID)
            {
                if (monMouseLeave_21.CheckShouldInvokeBase(this.instance))
                    base.onMouseLeave(mousePos, touchID);
                else
                    monMouseLeave_21.Invoke(this.instance, mousePos, touchID);
            }

            public override void onMouseDown(UnityEngine.Vector3 mousePos, System.Int32 touchID)
            {
                if (monMouseDown_22.CheckShouldInvokeBase(this.instance))
                    base.onMouseDown(mousePos, touchID);
                else
                    monMouseDown_22.Invoke(this.instance, mousePos, touchID);
            }

            public override void onMouseUp(UnityEngine.Vector3 mousePos, System.Int32 touchID)
            {
                if (monMouseUp_23.CheckShouldInvokeBase(this.instance))
                    base.onMouseUp(mousePos, touchID);
                else
                    monMouseUp_23.Invoke(this.instance, mousePos, touchID);
            }

            public override void onMouseMove(UnityEngine.Vector3 mousePos, UnityEngine.Vector3 moveDelta, System.Single moveTime, System.Int32 touchID)
            {
                if (monMouseMove_24.CheckShouldInvokeBase(this.instance))
                    base.onMouseMove(mousePos, moveDelta, moveTime, touchID);
                else
                    monMouseMove_24.Invoke(this.instance, mousePos, moveDelta, moveTime, touchID);
            }

            public override void onMouseStay(UnityEngine.Vector3 mousePos, System.Int32 touchID)
            {
                if (monMouseStay_25.CheckShouldInvokeBase(this.instance))
                    base.onMouseStay(mousePos, touchID);
                else
                    monMouseStay_25.Invoke(this.instance, mousePos, touchID);
            }

            public override void onScreenMouseDown(UnityEngine.Vector3 mousePos, System.Int32 touchID)
            {
                if (monScreenMouseDown_26.CheckShouldInvokeBase(this.instance))
                    base.onScreenMouseDown(mousePos, touchID);
                else
                    monScreenMouseDown_26.Invoke(this.instance, mousePos, touchID);
            }

            public override void onScreenMouseUp(UnityEngine.Vector3 mousePos, System.Int32 touchID)
            {
                if (monScreenMouseUp_27.CheckShouldInvokeBase(this.instance))
                    base.onScreenMouseUp(mousePos, touchID);
                else
                    monScreenMouseUp_27.Invoke(this.instance, mousePos, touchID);
            }

            public override void onReceiveDrag(global::IMouseEventCollect dragObj, UnityEngine.Vector3 mousePos, global::BOOL continueEvent)
            {
                if (monReceiveDrag_28.CheckShouldInvokeBase(this.instance))
                    base.onReceiveDrag(dragObj, mousePos, continueEvent);
                else
                    monReceiveDrag_28.Invoke(this.instance, dragObj, mousePos, continueEvent);
            }

            public override void onDragHoverd(global::IMouseEventCollect dragObj, UnityEngine.Vector3 mousePos, System.Boolean hover)
            {
                if (monDragHoverd_29.CheckShouldInvokeBase(this.instance))
                    base.onDragHoverd(dragObj, mousePos, hover);
                else
                    monDragHoverd_29.Invoke(this.instance, dragObj, mousePos, hover);
            }

            public override void onMultiTouchStart(UnityEngine.Vector3 touch0, UnityEngine.Vector3 touch1)
            {
                if (monMultiTouchStart_30.CheckShouldInvokeBase(this.instance))
                    base.onMultiTouchStart(touch0, touch1);
                else
                    monMultiTouchStart_30.Invoke(this.instance, touch0, touch1);
            }

            public override void onMultiTouchMove(UnityEngine.Vector3 touch0, UnityEngine.Vector3 lastTouch0, UnityEngine.Vector3 touch1, UnityEngine.Vector3 lastTouch1)
            {
                if (monMultiTouchMove_31.CheckShouldInvokeBase(this.instance))
                    base.onMultiTouchMove(touch0, lastTouch0, touch1, lastTouch1);
                else
                    monMultiTouchMove_31.Invoke(this.instance, touch0, lastTouch0, touch1, lastTouch1);
            }

            public override void onMultiTouchEnd()
            {
                if (monMultiTouchEnd_32.CheckShouldInvokeBase(this.instance))
                    base.onMultiTouchEnd();
                else
                    monMultiTouchEnd_32.Invoke(this.instance);
            }

            public override void setActive(System.Boolean active)
            {
                if (msetActive_33.CheckShouldInvokeBase(this.instance))
                    base.setActive(active);
                else
                    msetActive_33.Invoke(this.instance, active);
            }

            public override void setName(System.String name)
            {
                if (msetName_34.CheckShouldInvokeBase(this.instance))
                    base.setName(name);
                else
                    msetName_34.Invoke(this.instance, name);
            }

            public override System.Boolean raycast(ref UnityEngine.Ray ray, out UnityEngine.RaycastHit hit, System.Single maxDistance)
            {
                if (mraycast_35.CheckShouldInvokeBase(this.instance))
                    return base.raycast(ref ray, out hit, maxDistance);
                else
                    return mraycast_35.Invoke(this.instance, ref ray, out hit, maxDistance);
            }

            public override UnityEngine.GameObject getObject()
            {
                if (mgetObject_36.CheckShouldInvokeBase(this.instance))
                    return base.getObject();
                else
                    return mgetObject_36.Invoke(this.instance);
            }

            public override System.Boolean isActive()
            {
                if (misActive_37.CheckShouldInvokeBase(this.instance))
                    return base.isActive();
                else
                    return misActive_37.Invoke(this.instance);
            }

            public override System.Boolean isActiveInHierarchy()
            {
                if (misActiveInHierarchy_38.CheckShouldInvokeBase(this.instance))
                    return base.isActiveInHierarchy();
                else
                    return misActiveInHierarchy_38.Invoke(this.instance);
            }

            public override System.Boolean isEnable()
            {
                if (misEnable_39.CheckShouldInvokeBase(this.instance))
                    return base.isEnable();
                else
                    return misEnable_39.Invoke(this.instance);
            }

            public override void setEnable(System.Boolean enable)
            {
                if (msetEnable_40.CheckShouldInvokeBase(this.instance))
                    base.setEnable(enable);
                else
                    msetEnable_40.Invoke(this.instance, enable);
            }

            public override UnityEngine.Vector3 getPosition()
            {
                if (mgetPosition_41.CheckShouldInvokeBase(this.instance))
                    return base.getPosition();
                else
                    return mgetPosition_41.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getRotation()
            {
                if (mgetRotation_42.CheckShouldInvokeBase(this.instance))
                    return base.getRotation();
                else
                    return mgetRotation_42.Invoke(this.instance);
            }

            public override void cloneFrom(global::myUIObject obj)
            {
                if (mcloneFrom_43.CheckShouldInvokeBase(this.instance))
                    base.cloneFrom(obj);
                else
                    mcloneFrom_43.Invoke(this.instance, obj);
            }

            public override UnityEngine.Vector3 getScale()
            {
                if (mgetScale_44.CheckShouldInvokeBase(this.instance))
                    return base.getScale();
                else
                    return mgetScale_44.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldPosition()
            {
                if (mgetWorldPosition_45.CheckShouldInvokeBase(this.instance))
                    return base.getWorldPosition();
                else
                    return mgetWorldPosition_45.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldScale()
            {
                if (mgetWorldScale_46.CheckShouldInvokeBase(this.instance))
                    return base.getWorldScale();
                else
                    return mgetWorldScale_46.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldRotation()
            {
                if (mgetWorldRotation_47.CheckShouldInvokeBase(this.instance))
                    return base.getWorldRotation();
                else
                    return mgetWorldRotation_47.Invoke(this.instance);
            }

            public override void setPosition(UnityEngine.Vector3 pos)
            {
                if (msetPosition_48.CheckShouldInvokeBase(this.instance))
                    base.setPosition(pos);
                else
                    msetPosition_48.Invoke(this.instance, pos);
            }

            public override void setScale(UnityEngine.Vector3 scale)
            {
                if (msetScale_49.CheckShouldInvokeBase(this.instance))
                    base.setScale(scale);
                else
                    msetScale_49.Invoke(this.instance, scale);
            }

            public override void setRotation(UnityEngine.Vector3 rot)
            {
                if (msetRotation_50.CheckShouldInvokeBase(this.instance))
                    base.setRotation(rot);
                else
                    msetRotation_50.Invoke(this.instance, rot);
            }

            public override void setWorldPosition(UnityEngine.Vector3 pos)
            {
                if (msetWorldPosition_51.CheckShouldInvokeBase(this.instance))
                    base.setWorldPosition(pos);
                else
                    msetWorldPosition_51.Invoke(this.instance, pos);
            }

            public override void setWorldRotation(UnityEngine.Vector3 rot)
            {
                if (msetWorldRotation_52.CheckShouldInvokeBase(this.instance))
                    base.setWorldRotation(rot);
                else
                    msetWorldRotation_52.Invoke(this.instance, rot);
            }

            public override void setWorldScale(UnityEngine.Vector3 scale)
            {
                if (msetWorldScale_53.CheckShouldInvokeBase(this.instance))
                    base.setWorldScale(scale);
                else
                    msetWorldScale_53.Invoke(this.instance, scale);
            }

            public override void move(UnityEngine.Vector3 moveDelta, UnityEngine.Space space)
            {
                if (mmove_54.CheckShouldInvokeBase(this.instance))
                    base.move(moveDelta, space);
                else
                    mmove_54.Invoke(this.instance, moveDelta, space);
            }

            public override System.Boolean isChildOf(global::IMouseEventCollect parent)
            {
                if (misChildOf_55.CheckShouldInvokeBase(this.instance))
                    return base.isChildOf(parent);
                else
                    return misChildOf_55.Invoke(this.instance, parent);
            }

            public override void setAlpha(System.Single alpha)
            {
                if (msetAlpha_56.CheckShouldInvokeBase(this.instance))
                    base.setAlpha(alpha);
                else
                    msetAlpha_56.Invoke(this.instance, alpha);
            }

            public override System.Single getAlpha()
            {
                if (mgetAlpha_57.CheckShouldInvokeBase(this.instance))
                    return base.getAlpha();
                else
                    return mgetAlpha_57.Invoke(this.instance);
            }

            public override System.Boolean canUpdate()
            {
                if (mcanUpdate_58.CheckShouldInvokeBase(this.instance))
                    return base.canUpdate();
                else
                    return mcanUpdate_58.Invoke(this.instance);
            }

            public override void update(System.Single elapsedTime)
            {
                if (mupdate_59.CheckShouldInvokeBase(this.instance))
                    base.update(elapsedTime);
                else
                    mupdate_59.Invoke(this.instance, elapsedTime);
            }

            public override void lateUpdate(System.Single elapsedTime)
            {
                if (mlateUpdate_60.CheckShouldInvokeBase(this.instance))
                    base.lateUpdate(elapsedTime);
                else
                    mlateUpdate_60.Invoke(this.instance, elapsedTime);
            }

            public override void fixedUpdate(System.Single elapsedTime)
            {
                if (mfixedUpdate_61.CheckShouldInvokeBase(this.instance))
                    base.fixedUpdate(elapsedTime);
                else
                    mfixedUpdate_61.Invoke(this.instance, elapsedTime);
            }

            public override void notifyAddComponent(global::GameComponent com)
            {
                if (mnotifyAddComponent_62.CheckShouldInvokeBase(this.instance))
                    base.notifyAddComponent(com);
                else
                    mnotifyAddComponent_62.Invoke(this.instance, com);
            }

            public override void setIgnoreTimeScale(System.Boolean ignore, System.Boolean componentOnly)
            {
                if (msetIgnoreTimeScale_63.CheckShouldInvokeBase(this.instance))
                    base.setIgnoreTimeScale(ignore, componentOnly);
                else
                    msetIgnoreTimeScale_63.Invoke(this.instance, ignore, componentOnly);
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

