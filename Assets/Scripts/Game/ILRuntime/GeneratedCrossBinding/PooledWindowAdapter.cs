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
    public class PooledWindowAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::PooledWindow);
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

        public class Adapter : global::PooledWindow, CrossBindingAdaptorType
        {
            CrossBindingMethodInfo<global::LayoutScript> msetScript_0 = new CrossBindingMethodInfo<global::LayoutScript>("setScript");
            CrossBindingMethodInfo<global::myUIObject, global::myUIObject, System.String> massignWindow_1 = new CrossBindingMethodInfo<global::myUIObject, global::myUIObject, System.String>("assignWindow");
            CrossBindingMethodInfo minit_2 = new CrossBindingMethodInfo("init");
            CrossBindingMethodInfo mdestroy_3 = new CrossBindingMethodInfo("destroy");
            CrossBindingMethodInfo mreset_4 = new CrossBindingMethodInfo("reset");
            CrossBindingMethodInfo mrecycle_5 = new CrossBindingMethodInfo("recycle");
            CrossBindingMethodInfo<System.Boolean> msetVisible_6 = new CrossBindingMethodInfo<System.Boolean>("setVisible");
            CrossBindingMethodInfo<global::myUIObject, System.Boolean> msetParent_7 = new CrossBindingMethodInfo<global::myUIObject, System.Boolean>("setParent");
            CrossBindingMethodInfo<System.Boolean> msetAsLastSibling_8 = new CrossBindingMethodInfo<System.Boolean>("setAsLastSibling");
            CrossBindingMethodInfo mresetProperty_9 = new CrossBindingMethodInfo("resetProperty");

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

            public override void setScript(global::LayoutScript script)
            {
                if (msetScript_0.CheckShouldInvokeBase(this.instance))
                    base.setScript(script);
                else
                    msetScript_0.Invoke(this.instance, script);
            }

            public override void assignWindow(global::myUIObject parent, global::myUIObject template, System.String name)
            {
                massignWindow_1.Invoke(this.instance, parent, template, name);
            }

            public override void init()
            {
                if (minit_2.CheckShouldInvokeBase(this.instance))
                    base.init();
                else
                    minit_2.Invoke(this.instance);
            }

            public override void destroy()
            {
                if (mdestroy_3.CheckShouldInvokeBase(this.instance))
                    base.destroy();
                else
                    mdestroy_3.Invoke(this.instance);
            }

            public override void reset()
            {
                if (mreset_4.CheckShouldInvokeBase(this.instance))
                    base.reset();
                else
                    mreset_4.Invoke(this.instance);
            }

            public override void recycle()
            {
                if (mrecycle_5.CheckShouldInvokeBase(this.instance))
                    base.recycle();
                else
                    mrecycle_5.Invoke(this.instance);
            }

            public override void setVisible(System.Boolean visible)
            {
                if (msetVisible_6.CheckShouldInvokeBase(this.instance))
                    base.setVisible(visible);
                else
                    msetVisible_6.Invoke(this.instance, visible);
            }

            public override void setParent(global::myUIObject parent, System.Boolean refreshDepth)
            {
                if (msetParent_7.CheckShouldInvokeBase(this.instance))
                    base.setParent(parent, refreshDepth);
                else
                    msetParent_7.Invoke(this.instance, parent, refreshDepth);
            }

            public override void setAsLastSibling(System.Boolean refreshDepth)
            {
                if (msetAsLastSibling_8.CheckShouldInvokeBase(this.instance))
                    base.setAsLastSibling(refreshDepth);
                else
                    msetAsLastSibling_8.Invoke(this.instance, refreshDepth);
            }

            public override void resetProperty()
            {
                if (mresetProperty_9.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_9.Invoke(this.instance);
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

