using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class CharacterAdapter : CrossBindingAdaptor
    {
        static CrossBindingFunctionInfo<global::CharacterBaseData> mcreateCharacterData_0 = new CrossBindingFunctionInfo<global::CharacterBaseData>("createCharacterData");
        static CrossBindingMethodInfo minit_1 = new CrossBindingMethodInfo("init");
        static CrossBindingMethodInfo mresetProperty_2 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo mdestroyModel_3 = new CrossBindingMethodInfo("destroyModel");
        static CrossBindingFunctionInfo<System.String, System.Single> mgetAnimationLength_4 = new CrossBindingFunctionInfo<System.String, System.Single>("getAnimationLength");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyComponentChanged_5 = new CrossBindingMethodInfo<global::GameComponent>("notifyComponentChanged");
        static CrossBindingMethodInfo<global::PlayerState> mnotifyStateChanged_6 = new CrossBindingMethodInfo<global::PlayerState>("notifyStateChanged");
        static CrossBindingMethodInfo minitComponents_7 = new CrossBindingMethodInfo("initComponents");
        static CrossBindingMethodInfo<UnityEngine.GameObject> mnotifyModelLoaded_8 = new CrossBindingMethodInfo<UnityEngine.GameObject>("notifyModelLoaded");
        static CrossBindingMethodInfo mdestroy_9 = new CrossBindingMethodInfo("destroy");
        static CrossBindingMethodInfo<UnityEngine.GameObject, System.Boolean> msetObject_10 = new CrossBindingMethodInfo<UnityEngine.GameObject, System.Boolean>("setObject");
        static CrossBindingMethodInfo<System.Single> mupdate_11 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo<System.Single> mfixedUpdate_12 = new CrossBindingMethodInfo<System.Single>("fixedUpdate");
        static CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3> mlocalToWorld_13 = new CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3>("localToWorld");
        static CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3> mworldToLocal_14 = new CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3>("worldToLocal");
        static CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3> mlocalToWorldDirection_15 = new CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3>("localToWorldDirection");
        static CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3> mworldToLocalDirection_16 = new CrossBindingFunctionInfo<UnityEngine.Vector3, UnityEngine.Vector3>("worldToLocalDirection");
        static CrossBindingFunctionInfo<UnityEngine.GameObject> mgetObject_17 = new CrossBindingFunctionInfo<UnityEngine.GameObject>("getObject");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetPosition_18 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getPosition");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetRotation_19 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getRotation");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetScale_20 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getScale");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldPosition_21 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldPosition");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldScale_22 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldScale");
        static CrossBindingFunctionInfo<UnityEngine.Vector3> mgetWorldRotation_23 = new CrossBindingFunctionInfo<UnityEngine.Vector3>("getWorldRotation");
        static CrossBindingFunctionInfo<System.Boolean> misActive_24 = new CrossBindingFunctionInfo<System.Boolean>("isActive");
        static CrossBindingFunctionInfo<System.Boolean> misActiveInHierarchy_25 = new CrossBindingFunctionInfo<System.Boolean>("isActiveInHierarchy");
        static CrossBindingFunctionInfo<System.Boolean> misHandleInput_26 = new CrossBindingFunctionInfo<System.Boolean>("isHandleInput");
        static CrossBindingFunctionInfo<UnityEngine.Collider> mgetCollider_27 = new CrossBindingFunctionInfo<UnityEngine.Collider>("getCollider");
        static CrossBindingFunctionInfo<global::UIDepth> mgetDepth_28 = new CrossBindingFunctionInfo<global::UIDepth>("getDepth");
        static CrossBindingFunctionInfo<System.Boolean> misReceiveScreenMouse_29 = new CrossBindingFunctionInfo<System.Boolean>("isReceiveScreenMouse");
        static CrossBindingFunctionInfo<System.Boolean> misPassRay_30 = new CrossBindingFunctionInfo<System.Boolean>("isPassRay");
        static CrossBindingFunctionInfo<System.Boolean> misDragable_31 = new CrossBindingFunctionInfo<System.Boolean>("isDragable");
        static CrossBindingFunctionInfo<System.Boolean> misMouseHovered_32 = new CrossBindingFunctionInfo<System.Boolean>("isMouseHovered");
        static CrossBindingFunctionInfo<global::IMouseEventCollect, System.Boolean> misChildOf_33 = new CrossBindingFunctionInfo<global::IMouseEventCollect, System.Boolean>("isChildOf");
        static CrossBindingMethodInfo<System.Boolean> msetPassRay_34 = new CrossBindingMethodInfo<System.Boolean>("setPassRay");
        static CrossBindingMethodInfo<System.Boolean> msetHandleInput_35 = new CrossBindingMethodInfo<System.Boolean>("setHandleInput");
        static CrossBindingMethodInfo<System.String> msetName_36 = new CrossBindingMethodInfo<System.String>("setName");
        static CrossBindingMethodInfo<System.Boolean> msetActive_37 = new CrossBindingMethodInfo<System.Boolean>("setActive");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetPosition_38 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setPosition");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetScale_39 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setScale");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetRotation_40 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setRotation");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldPosition_41 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldPosition");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldRotation_42 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldRotation");
        static CrossBindingMethodInfo<UnityEngine.Vector3> msetWorldScale_43 = new CrossBindingMethodInfo<UnityEngine.Vector3>("setWorldScale");
        static CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Space> mmove_44 = new CrossBindingMethodInfo<UnityEngine.Vector3, UnityEngine.Space>("move");
        static CrossBindingMethodInfo<global::ObjectClickCallback> msetClickCallback_45 = new CrossBindingMethodInfo<global::ObjectClickCallback>("setClickCallback");
        static CrossBindingMethodInfo<global::ObjectHoverCallback> msetHoverCallback_46 = new CrossBindingMethodInfo<global::ObjectHoverCallback>("setHoverCallback");
        static CrossBindingMethodInfo<global::ObjectPressCallback> msetPressCallback_47 = new CrossBindingMethodInfo<global::ObjectPressCallback>("setPressCallback");
        static CrossBindingMethodInfo monMouseEnter_48 = new CrossBindingMethodInfo("onMouseEnter");
        static CrossBindingMethodInfo monMouseLeave_49 = new CrossBindingMethodInfo("onMouseLeave");
        static CrossBindingMethodInfo<UnityEngine.Vector3> monMouseDown_50 = new CrossBindingMethodInfo<UnityEngine.Vector3>("onMouseDown");
        static CrossBindingMethodInfo<UnityEngine.Vector3> monMouseUp_51 = new CrossBindingMethodInfo<UnityEngine.Vector3>("onMouseUp");
        class onMouseMove_52Info : CrossBindingMethodInfo
        {
            static Type[] pTypes = new Type[] {typeof(UnityEngine.Vector3).MakeByRefType(), typeof(UnityEngine.Vector3).MakeByRefType(), typeof(System.Single)};

            public onMouseMove_52Info()
                : base("onMouseMove")
            {

            }

            protected override Type ReturnType { get { return null; } }

            protected override Type[] Parameters { get { return pTypes; } }
            public void Invoke(ILTypeInstance instance, ref UnityEngine.Vector3 mousePos, ref UnityEngine.Vector3 moveDelta, System.Single moveTime)
            {
                EnsureMethod(instance);
                if (method != null)
                {
                    invoking = true;
                    try
                    {
                        using (var ctx = domain.BeginInvoke(method))
                        {
                            ctx.PushObject(mousePos);
                            ctx.PushObject(moveDelta);
                            ctx.PushObject(instance);
                            ctx.PushReference(0);
                            ctx.PushReference(1);
                            ctx.PushInteger(moveTime);
                            ctx.Invoke();
                            mousePos = ctx.ReadObject<UnityEngine.Vector3>(0);
                            moveDelta = ctx.ReadObject<UnityEngine.Vector3>(1);
                        }
                    }
                    finally
                    {
                        invoking = false;
                    }
                }
            }

            public override void Invoke(ILTypeInstance instance)
            {
                throw new NotSupportedException();
            }
        }
        static onMouseMove_52Info monMouseMove_52 = new onMouseMove_52Info();
        static CrossBindingMethodInfo<UnityEngine.Vector3> monMouseStay_53 = new CrossBindingMethodInfo<UnityEngine.Vector3>("onMouseStay");
        static CrossBindingMethodInfo<UnityEngine.Vector3> monScreenMouseDown_54 = new CrossBindingMethodInfo<UnityEngine.Vector3>("onScreenMouseDown");
        static CrossBindingMethodInfo<UnityEngine.Vector3> monScreenMouseUp_55 = new CrossBindingMethodInfo<UnityEngine.Vector3>("onScreenMouseUp");
        class onReceiveDrag_56Info : CrossBindingMethodInfo
        {
            static Type[] pTypes = new Type[] {typeof(global::IMouseEventCollect), typeof(System.Boolean).MakeByRefType()};

            public onReceiveDrag_56Info()
                : base("onReceiveDrag")
            {

            }

            protected override Type ReturnType { get { return null; } }

            protected override Type[] Parameters { get { return pTypes; } }
            public void Invoke(ILTypeInstance instance, global::IMouseEventCollect dragObj, ref System.Boolean continueEvent)
            {
                EnsureMethod(instance);
                if (method != null)
                {
                    invoking = true;
                    try
                    {
                        using (var ctx = domain.BeginInvoke(method))
                        {
                            ctx.PushObject(continueEvent);
                            ctx.PushObject(instance);
                            ctx.PushObject(dragObj);
                            ctx.PushReference(0);
                            ctx.Invoke();
                            continueEvent = ctx.ReadObject<System.Boolean>(0);
                        }
                    }
                    finally
                    {
                        invoking = false;
                    }
                }
            }

            public override void Invoke(ILTypeInstance instance)
            {
                throw new NotSupportedException();
            }
        }
        static onReceiveDrag_56Info monReceiveDrag_56 = new onReceiveDrag_56Info();
        static CrossBindingMethodInfo<global::IMouseEventCollect, System.Boolean> monDragHoverd_57 = new CrossBindingMethodInfo<global::IMouseEventCollect, System.Boolean>("onDragHoverd");
        static CrossBindingMethodInfo<UnityEngine.Vector2, UnityEngine.Vector2> monMultiTouchStart_58 = new CrossBindingMethodInfo<UnityEngine.Vector2, UnityEngine.Vector2>("onMultiTouchStart");
        static CrossBindingMethodInfo<UnityEngine.Vector2, UnityEngine.Vector2, UnityEngine.Vector2, UnityEngine.Vector2> monMultiTouchMove_59 = new CrossBindingMethodInfo<UnityEngine.Vector2, UnityEngine.Vector2, UnityEngine.Vector2, UnityEngine.Vector2>("onMultiTouchMove");
        static CrossBindingMethodInfo monMultiTouchEnd_60 = new CrossBindingMethodInfo("onMultiTouchEnd");
        static CrossBindingMethodInfo<System.Single> msetAlpha_61 = new CrossBindingMethodInfo<System.Single>("setAlpha");
        static CrossBindingFunctionInfo<System.Single> mgetAlpha_62 = new CrossBindingFunctionInfo<System.Single>("getAlpha");
        static CrossBindingFunctionInfo<System.Boolean> misEnable_63 = new CrossBindingFunctionInfo<System.Boolean>("isEnable");
        static CrossBindingMethodInfo<System.Boolean> msetEnable_64 = new CrossBindingMethodInfo<System.Boolean>("setEnable");
        static CrossBindingMethodInfo<System.Single> mlateUpdate_65 = new CrossBindingMethodInfo<System.Single>("lateUpdate");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyAddComponent_66 = new CrossBindingMethodInfo<global::GameComponent>("notifyAddComponent");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyComponentDetached_67 = new CrossBindingMethodInfo<global::GameComponent>("notifyComponentDetached");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyComponentAttached_68 = new CrossBindingMethodInfo<global::GameComponent>("notifyComponentAttached");
        static CrossBindingMethodInfo<global::GameComponent> mnotifyComponentDestroied_69 = new CrossBindingMethodInfo<global::GameComponent>("notifyComponentDestroied");
        static CrossBindingMethodInfo<System.Boolean, System.Boolean> msetIgnoreTimeScale_70 = new CrossBindingMethodInfo<System.Boolean, System.Boolean>("setIgnoreTimeScale");
        static CrossBindingMethodInfo<global::Command> mreceiveCommand_71 = new CrossBindingMethodInfo<global::Command>("receiveCommand");
        static CrossBindingFunctionInfo<System.String> mgetName_72 = new CrossBindingFunctionInfo<System.String>("getName");
        static CrossBindingMethodInfo mnotifyConstructDone_73 = new CrossBindingMethodInfo("notifyConstructDone");
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

            protected override global::CharacterBaseData createCharacterData()
            {
                if (mcreateCharacterData_0.CheckShouldInvokeBase(this.instance))
                    return base.createCharacterData();
                else
                    return mcreateCharacterData_0.Invoke(this.instance);
            }

            public override void init()
            {
                if (minit_1.CheckShouldInvokeBase(this.instance))
                    base.init();
                else
                    minit_1.Invoke(this.instance);
            }

            public override void resetProperty()
            {
                if (mresetProperty_2.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_2.Invoke(this.instance);
            }

            public override void destroyModel()
            {
                if (mdestroyModel_3.CheckShouldInvokeBase(this.instance))
                    base.destroyModel();
                else
                    mdestroyModel_3.Invoke(this.instance);
            }

            public override System.Single getAnimationLength(System.String name)
            {
                if (mgetAnimationLength_4.CheckShouldInvokeBase(this.instance))
                    return base.getAnimationLength(name);
                else
                    return mgetAnimationLength_4.Invoke(this.instance, name);
            }

            public override void notifyComponentChanged(global::GameComponent component)
            {
                if (mnotifyComponentChanged_5.CheckShouldInvokeBase(this.instance))
                    base.notifyComponentChanged(component);
                else
                    mnotifyComponentChanged_5.Invoke(this.instance, component);
            }

            public override void notifyStateChanged(global::PlayerState state)
            {
                if (mnotifyStateChanged_6.CheckShouldInvokeBase(this.instance))
                    base.notifyStateChanged(state);
                else
                    mnotifyStateChanged_6.Invoke(this.instance, state);
            }

            protected override void initComponents()
            {
                if (minitComponents_7.CheckShouldInvokeBase(this.instance))
                    base.initComponents();
                else
                    minitComponents_7.Invoke(this.instance);
            }

            protected override void notifyModelLoaded(UnityEngine.GameObject go)
            {
                if (mnotifyModelLoaded_8.CheckShouldInvokeBase(this.instance))
                    base.notifyModelLoaded(go);
                else
                    mnotifyModelLoaded_8.Invoke(this.instance, go);
            }

            public override void destroy()
            {
                if (mdestroy_9.CheckShouldInvokeBase(this.instance))
                    base.destroy();
                else
                    mdestroy_9.Invoke(this.instance);
            }

            public override void setObject(UnityEngine.GameObject obj, System.Boolean destroyOld)
            {
                if (msetObject_10.CheckShouldInvokeBase(this.instance))
                    base.setObject(obj, destroyOld);
                else
                    msetObject_10.Invoke(this.instance, obj, destroyOld);
            }

            public override void update(System.Single elapsedTime)
            {
                if (mupdate_11.CheckShouldInvokeBase(this.instance))
                    base.update(elapsedTime);
                else
                    mupdate_11.Invoke(this.instance, elapsedTime);
            }

            public override void fixedUpdate(System.Single elapsedTime)
            {
                if (mfixedUpdate_12.CheckShouldInvokeBase(this.instance))
                    base.fixedUpdate(elapsedTime);
                else
                    mfixedUpdate_12.Invoke(this.instance, elapsedTime);
            }

            public override UnityEngine.Vector3 localToWorld(UnityEngine.Vector3 point)
            {
                if (mlocalToWorld_13.CheckShouldInvokeBase(this.instance))
                    return base.localToWorld(point);
                else
                    return mlocalToWorld_13.Invoke(this.instance, point);
            }

            public override UnityEngine.Vector3 worldToLocal(UnityEngine.Vector3 point)
            {
                if (mworldToLocal_14.CheckShouldInvokeBase(this.instance))
                    return base.worldToLocal(point);
                else
                    return mworldToLocal_14.Invoke(this.instance, point);
            }

            public override UnityEngine.Vector3 localToWorldDirection(UnityEngine.Vector3 direction)
            {
                if (mlocalToWorldDirection_15.CheckShouldInvokeBase(this.instance))
                    return base.localToWorldDirection(direction);
                else
                    return mlocalToWorldDirection_15.Invoke(this.instance, direction);
            }

            public override UnityEngine.Vector3 worldToLocalDirection(UnityEngine.Vector3 direction)
            {
                if (mworldToLocalDirection_16.CheckShouldInvokeBase(this.instance))
                    return base.worldToLocalDirection(direction);
                else
                    return mworldToLocalDirection_16.Invoke(this.instance, direction);
            }

            public override UnityEngine.GameObject getObject()
            {
                if (mgetObject_17.CheckShouldInvokeBase(this.instance))
                    return base.getObject();
                else
                    return mgetObject_17.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getPosition()
            {
                if (mgetPosition_18.CheckShouldInvokeBase(this.instance))
                    return base.getPosition();
                else
                    return mgetPosition_18.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getRotation()
            {
                if (mgetRotation_19.CheckShouldInvokeBase(this.instance))
                    return base.getRotation();
                else
                    return mgetRotation_19.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getScale()
            {
                if (mgetScale_20.CheckShouldInvokeBase(this.instance))
                    return base.getScale();
                else
                    return mgetScale_20.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldPosition()
            {
                if (mgetWorldPosition_21.CheckShouldInvokeBase(this.instance))
                    return base.getWorldPosition();
                else
                    return mgetWorldPosition_21.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldScale()
            {
                if (mgetWorldScale_22.CheckShouldInvokeBase(this.instance))
                    return base.getWorldScale();
                else
                    return mgetWorldScale_22.Invoke(this.instance);
            }

            public override UnityEngine.Vector3 getWorldRotation()
            {
                if (mgetWorldRotation_23.CheckShouldInvokeBase(this.instance))
                    return base.getWorldRotation();
                else
                    return mgetWorldRotation_23.Invoke(this.instance);
            }

            public override System.Boolean isActive()
            {
                if (misActive_24.CheckShouldInvokeBase(this.instance))
                    return base.isActive();
                else
                    return misActive_24.Invoke(this.instance);
            }

            public override System.Boolean isActiveInHierarchy()
            {
                if (misActiveInHierarchy_25.CheckShouldInvokeBase(this.instance))
                    return base.isActiveInHierarchy();
                else
                    return misActiveInHierarchy_25.Invoke(this.instance);
            }

            public override System.Boolean isHandleInput()
            {
                if (misHandleInput_26.CheckShouldInvokeBase(this.instance))
                    return base.isHandleInput();
                else
                    return misHandleInput_26.Invoke(this.instance);
            }

            public override UnityEngine.Collider getCollider()
            {
                if (mgetCollider_27.CheckShouldInvokeBase(this.instance))
                    return base.getCollider();
                else
                    return mgetCollider_27.Invoke(this.instance);
            }

            public override global::UIDepth getDepth()
            {
                if (mgetDepth_28.CheckShouldInvokeBase(this.instance))
                    return base.getDepth();
                else
                    return mgetDepth_28.Invoke(this.instance);
            }

            public override System.Boolean isReceiveScreenMouse()
            {
                if (misReceiveScreenMouse_29.CheckShouldInvokeBase(this.instance))
                    return base.isReceiveScreenMouse();
                else
                    return misReceiveScreenMouse_29.Invoke(this.instance);
            }

            public override System.Boolean isPassRay()
            {
                if (misPassRay_30.CheckShouldInvokeBase(this.instance))
                    return base.isPassRay();
                else
                    return misPassRay_30.Invoke(this.instance);
            }

            public override System.Boolean isDragable()
            {
                if (misDragable_31.CheckShouldInvokeBase(this.instance))
                    return base.isDragable();
                else
                    return misDragable_31.Invoke(this.instance);
            }

            public override System.Boolean isMouseHovered()
            {
                if (misMouseHovered_32.CheckShouldInvokeBase(this.instance))
                    return base.isMouseHovered();
                else
                    return misMouseHovered_32.Invoke(this.instance);
            }

            public override System.Boolean isChildOf(global::IMouseEventCollect parent)
            {
                if (misChildOf_33.CheckShouldInvokeBase(this.instance))
                    return base.isChildOf(parent);
                else
                    return misChildOf_33.Invoke(this.instance, parent);
            }

            public override void setPassRay(System.Boolean passRay)
            {
                if (msetPassRay_34.CheckShouldInvokeBase(this.instance))
                    base.setPassRay(passRay);
                else
                    msetPassRay_34.Invoke(this.instance, passRay);
            }

            public override void setHandleInput(System.Boolean handleInput)
            {
                if (msetHandleInput_35.CheckShouldInvokeBase(this.instance))
                    base.setHandleInput(handleInput);
                else
                    msetHandleInput_35.Invoke(this.instance, handleInput);
            }

            public override void setName(System.String name)
            {
                if (msetName_36.CheckShouldInvokeBase(this.instance))
                    base.setName(name);
                else
                    msetName_36.Invoke(this.instance, name);
            }

            public override void setActive(System.Boolean active)
            {
                if (msetActive_37.CheckShouldInvokeBase(this.instance))
                    base.setActive(active);
                else
                    msetActive_37.Invoke(this.instance, active);
            }

            public override void setPosition(UnityEngine.Vector3 pos)
            {
                if (msetPosition_38.CheckShouldInvokeBase(this.instance))
                    base.setPosition(pos);
                else
                    msetPosition_38.Invoke(this.instance, pos);
            }

            public override void setScale(UnityEngine.Vector3 scale)
            {
                if (msetScale_39.CheckShouldInvokeBase(this.instance))
                    base.setScale(scale);
                else
                    msetScale_39.Invoke(this.instance, scale);
            }

            public override void setRotation(UnityEngine.Vector3 rot)
            {
                if (msetRotation_40.CheckShouldInvokeBase(this.instance))
                    base.setRotation(rot);
                else
                    msetRotation_40.Invoke(this.instance, rot);
            }

            public override void setWorldPosition(UnityEngine.Vector3 pos)
            {
                if (msetWorldPosition_41.CheckShouldInvokeBase(this.instance))
                    base.setWorldPosition(pos);
                else
                    msetWorldPosition_41.Invoke(this.instance, pos);
            }

            public override void setWorldRotation(UnityEngine.Vector3 rot)
            {
                if (msetWorldRotation_42.CheckShouldInvokeBase(this.instance))
                    base.setWorldRotation(rot);
                else
                    msetWorldRotation_42.Invoke(this.instance, rot);
            }

            public override void setWorldScale(UnityEngine.Vector3 scale)
            {
                if (msetWorldScale_43.CheckShouldInvokeBase(this.instance))
                    base.setWorldScale(scale);
                else
                    msetWorldScale_43.Invoke(this.instance, scale);
            }

            public override void move(UnityEngine.Vector3 moveDelta, UnityEngine.Space space)
            {
                if (mmove_44.CheckShouldInvokeBase(this.instance))
                    base.move(moveDelta, space);
                else
                    mmove_44.Invoke(this.instance, moveDelta, space);
            }

            public override void setClickCallback(global::ObjectClickCallback callback)
            {
                if (msetClickCallback_45.CheckShouldInvokeBase(this.instance))
                    base.setClickCallback(callback);
                else
                    msetClickCallback_45.Invoke(this.instance, callback);
            }

            public override void setHoverCallback(global::ObjectHoverCallback callback)
            {
                if (msetHoverCallback_46.CheckShouldInvokeBase(this.instance))
                    base.setHoverCallback(callback);
                else
                    msetHoverCallback_46.Invoke(this.instance, callback);
            }

            public override void setPressCallback(global::ObjectPressCallback callback)
            {
                if (msetPressCallback_47.CheckShouldInvokeBase(this.instance))
                    base.setPressCallback(callback);
                else
                    msetPressCallback_47.Invoke(this.instance, callback);
            }

            public override void onMouseEnter()
            {
                if (monMouseEnter_48.CheckShouldInvokeBase(this.instance))
                    base.onMouseEnter();
                else
                    monMouseEnter_48.Invoke(this.instance);
            }

            public override void onMouseLeave()
            {
                if (monMouseLeave_49.CheckShouldInvokeBase(this.instance))
                    base.onMouseLeave();
                else
                    monMouseLeave_49.Invoke(this.instance);
            }

            public override void onMouseDown(UnityEngine.Vector3 mousePos)
            {
                if (monMouseDown_50.CheckShouldInvokeBase(this.instance))
                    base.onMouseDown(mousePos);
                else
                    monMouseDown_50.Invoke(this.instance, mousePos);
            }

            public override void onMouseUp(UnityEngine.Vector3 mousePos)
            {
                if (monMouseUp_51.CheckShouldInvokeBase(this.instance))
                    base.onMouseUp(mousePos);
                else
                    monMouseUp_51.Invoke(this.instance, mousePos);
            }

            public override void onMouseMove(ref UnityEngine.Vector3 mousePos, ref UnityEngine.Vector3 moveDelta, System.Single moveTime)
            {
                if (monMouseMove_52.CheckShouldInvokeBase(this.instance))
                    base.onMouseMove(ref mousePos, ref moveDelta, moveTime);
                else
                    monMouseMove_52.Invoke(this.instance, ref mousePos, ref moveDelta, moveTime);
            }

            public override void onMouseStay(UnityEngine.Vector3 mousePos)
            {
                if (monMouseStay_53.CheckShouldInvokeBase(this.instance))
                    base.onMouseStay(mousePos);
                else
                    monMouseStay_53.Invoke(this.instance, mousePos);
            }

            public override void onScreenMouseDown(UnityEngine.Vector3 mousePos)
            {
                if (monScreenMouseDown_54.CheckShouldInvokeBase(this.instance))
                    base.onScreenMouseDown(mousePos);
                else
                    monScreenMouseDown_54.Invoke(this.instance, mousePos);
            }

            public override void onScreenMouseUp(UnityEngine.Vector3 mousePos)
            {
                if (monScreenMouseUp_55.CheckShouldInvokeBase(this.instance))
                    base.onScreenMouseUp(mousePos);
                else
                    monScreenMouseUp_55.Invoke(this.instance, mousePos);
            }

            public override void onReceiveDrag(global::IMouseEventCollect dragObj, ref System.Boolean continueEvent)
            {
                if (monReceiveDrag_56.CheckShouldInvokeBase(this.instance))
                    base.onReceiveDrag(dragObj, ref continueEvent);
                else
                    monReceiveDrag_56.Invoke(this.instance, dragObj, ref continueEvent);
            }

            public override void onDragHoverd(global::IMouseEventCollect dragObj, System.Boolean hover)
            {
                if (monDragHoverd_57.CheckShouldInvokeBase(this.instance))
                    base.onDragHoverd(dragObj, hover);
                else
                    monDragHoverd_57.Invoke(this.instance, dragObj, hover);
            }

            public override void onMultiTouchStart(UnityEngine.Vector2 touch0, UnityEngine.Vector2 touch1)
            {
                if (monMultiTouchStart_58.CheckShouldInvokeBase(this.instance))
                    base.onMultiTouchStart(touch0, touch1);
                else
                    monMultiTouchStart_58.Invoke(this.instance, touch0, touch1);
            }

            public override void onMultiTouchMove(UnityEngine.Vector2 touch0, UnityEngine.Vector2 lastTouch0, UnityEngine.Vector2 touch1, UnityEngine.Vector2 lastTouch1)
            {
                if (monMultiTouchMove_59.CheckShouldInvokeBase(this.instance))
                    base.onMultiTouchMove(touch0, lastTouch0, touch1, lastTouch1);
                else
                    monMultiTouchMove_59.Invoke(this.instance, touch0, lastTouch0, touch1, lastTouch1);
            }

            public override void onMultiTouchEnd()
            {
                if (monMultiTouchEnd_60.CheckShouldInvokeBase(this.instance))
                    base.onMultiTouchEnd();
                else
                    monMultiTouchEnd_60.Invoke(this.instance);
            }

            public override void setAlpha(System.Single alpha)
            {
                if (msetAlpha_61.CheckShouldInvokeBase(this.instance))
                    base.setAlpha(alpha);
                else
                    msetAlpha_61.Invoke(this.instance, alpha);
            }

            public override System.Single getAlpha()
            {
                if (mgetAlpha_62.CheckShouldInvokeBase(this.instance))
                    return base.getAlpha();
                else
                    return mgetAlpha_62.Invoke(this.instance);
            }

            public override System.Boolean isEnable()
            {
                if (misEnable_63.CheckShouldInvokeBase(this.instance))
                    return base.isEnable();
                else
                    return misEnable_63.Invoke(this.instance);
            }

            public override void setEnable(System.Boolean enable)
            {
                if (msetEnable_64.CheckShouldInvokeBase(this.instance))
                    base.setEnable(enable);
                else
                    msetEnable_64.Invoke(this.instance, enable);
            }

            public override void lateUpdate(System.Single elapsedTime)
            {
                if (mlateUpdate_65.CheckShouldInvokeBase(this.instance))
                    base.lateUpdate(elapsedTime);
                else
                    mlateUpdate_65.Invoke(this.instance, elapsedTime);
            }

            public override void notifyAddComponent(global::GameComponent component)
            {
                if (mnotifyAddComponent_66.CheckShouldInvokeBase(this.instance))
                    base.notifyAddComponent(component);
                else
                    mnotifyAddComponent_66.Invoke(this.instance, component);
            }

            public override void notifyComponentDetached(global::GameComponent component)
            {
                if (mnotifyComponentDetached_67.CheckShouldInvokeBase(this.instance))
                    base.notifyComponentDetached(component);
                else
                    mnotifyComponentDetached_67.Invoke(this.instance, component);
            }

            public override void notifyComponentAttached(global::GameComponent component)
            {
                if (mnotifyComponentAttached_68.CheckShouldInvokeBase(this.instance))
                    base.notifyComponentAttached(component);
                else
                    mnotifyComponentAttached_68.Invoke(this.instance, component);
            }

            public override void notifyComponentDestroied(global::GameComponent component)
            {
                if (mnotifyComponentDestroied_69.CheckShouldInvokeBase(this.instance))
                    base.notifyComponentDestroied(component);
                else
                    mnotifyComponentDestroied_69.Invoke(this.instance, component);
            }

            public override void setIgnoreTimeScale(System.Boolean ignore, System.Boolean componentOnly)
            {
                if (msetIgnoreTimeScale_70.CheckShouldInvokeBase(this.instance))
                    base.setIgnoreTimeScale(ignore, componentOnly);
                else
                    msetIgnoreTimeScale_70.Invoke(this.instance, ignore, componentOnly);
            }

            public override void receiveCommand(global::Command cmd)
            {
                if (mreceiveCommand_71.CheckShouldInvokeBase(this.instance))
                    base.receiveCommand(cmd);
                else
                    mreceiveCommand_71.Invoke(this.instance, cmd);
            }

            public override System.String getName()
            {
                if (mgetName_72.CheckShouldInvokeBase(this.instance))
                    return base.getName();
                else
                    return mgetName_72.Invoke(this.instance);
            }

            public override void notifyConstructDone()
            {
                if (mnotifyConstructDone_73.CheckShouldInvokeBase(this.instance))
                    base.notifyConstructDone();
                else
                    mnotifyConstructDone_73.Invoke(this.instance);
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

