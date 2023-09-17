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
    public class GameComponentAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::GameComponent);
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

        public class Adapter : global::GameComponent, CrossBindingAdaptorType
        {
            CrossBindingMethodInfo<global::ComponentOwner> minit_0 = new CrossBindingMethodInfo<global::ComponentOwner>("init");
            CrossBindingMethodInfo<System.Single> mupdate_1 = new CrossBindingMethodInfo<System.Single>("update");
            CrossBindingMethodInfo<System.Single> mfixedUpdate_2 = new CrossBindingMethodInfo<System.Single>("fixedUpdate");
            CrossBindingMethodInfo<System.Single> mlateUpdate_3 = new CrossBindingMethodInfo<System.Single>("lateUpdate");
            CrossBindingMethodInfo mdestroy_4 = new CrossBindingMethodInfo("destroy");
            CrossBindingMethodInfo mresetProperty_5 = new CrossBindingMethodInfo("resetProperty");
            CrossBindingMethodInfo<System.Boolean> msetActive_6 = new CrossBindingMethodInfo<System.Boolean>("setActive");
            CrossBindingMethodInfo<System.Boolean> msetIgnoreTimeScale_7 = new CrossBindingMethodInfo<System.Boolean>("setIgnoreTimeScale");
            CrossBindingMethodInfo<System.Boolean> mnotifyOwnerActive_8 = new CrossBindingMethodInfo<System.Boolean>("notifyOwnerActive");

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

            public override void init(global::ComponentOwner owner)
            {
                if (minit_0.CheckShouldInvokeBase(this.instance))
                    base.init(owner);
                else
                    minit_0.Invoke(this.instance, owner);
            }

            public override void update(System.Single elapsedTime)
            {
                if (mupdate_1.CheckShouldInvokeBase(this.instance))
                    base.update(elapsedTime);
                else
                    mupdate_1.Invoke(this.instance, elapsedTime);
            }

            public override void fixedUpdate(System.Single elapsedTime)
            {
                if (mfixedUpdate_2.CheckShouldInvokeBase(this.instance))
                    base.fixedUpdate(elapsedTime);
                else
                    mfixedUpdate_2.Invoke(this.instance, elapsedTime);
            }

            public override void lateUpdate(System.Single elapsedTime)
            {
                if (mlateUpdate_3.CheckShouldInvokeBase(this.instance))
                    base.lateUpdate(elapsedTime);
                else
                    mlateUpdate_3.Invoke(this.instance, elapsedTime);
            }

            public override void destroy()
            {
                if (mdestroy_4.CheckShouldInvokeBase(this.instance))
                    base.destroy();
                else
                    mdestroy_4.Invoke(this.instance);
            }

            public override void resetProperty()
            {
                if (mresetProperty_5.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_5.Invoke(this.instance);
            }

            public override void setActive(System.Boolean active)
            {
                if (msetActive_6.CheckShouldInvokeBase(this.instance))
                    base.setActive(active);
                else
                    msetActive_6.Invoke(this.instance, active);
            }

            public override void setIgnoreTimeScale(System.Boolean ignore)
            {
                if (msetIgnoreTimeScale_7.CheckShouldInvokeBase(this.instance))
                    base.setIgnoreTimeScale(ignore);
                else
                    msetIgnoreTimeScale_7.Invoke(this.instance, ignore);
            }

            public override void notifyOwnerActive(System.Boolean active)
            {
                if (mnotifyOwnerActive_8.CheckShouldInvokeBase(this.instance))
                    base.notifyOwnerActive(active);
                else
                    mnotifyOwnerActive_8.Invoke(this.instance, active);
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

