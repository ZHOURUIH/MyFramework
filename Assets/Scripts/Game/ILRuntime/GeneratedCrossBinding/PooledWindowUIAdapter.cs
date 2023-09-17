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
    public class PooledWindowUIAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::PooledWindowUI);
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

        public class Adapter : global::PooledWindowUI, CrossBindingAdaptorType
        {
            CrossBindingMethodInfo<System.Boolean> msetVisible_0 = new CrossBindingMethodInfo<System.Boolean>("setVisible");
            CrossBindingMethodInfo<System.Boolean> msetAsLastSibling_1 = new CrossBindingMethodInfo<System.Boolean>("setAsLastSibling");
            CrossBindingMethodInfo<global::myUIObject, System.Boolean> msetParent_2 = new CrossBindingMethodInfo<global::myUIObject, System.Boolean>("setParent");
            CrossBindingMethodInfo<global::LayoutScript> msetScript_3 = new CrossBindingMethodInfo<global::LayoutScript>("setScript");
            CrossBindingMethodInfo<global::myUIObject, global::myUIObject, System.String> massignWindow_4 = new CrossBindingMethodInfo<global::myUIObject, global::myUIObject, System.String>("assignWindow");
            CrossBindingMethodInfo minit_5 = new CrossBindingMethodInfo("init");
            CrossBindingMethodInfo mdestroy_6 = new CrossBindingMethodInfo("destroy");
            CrossBindingMethodInfo mreset_7 = new CrossBindingMethodInfo("reset");
            CrossBindingMethodInfo mrecycle_8 = new CrossBindingMethodInfo("recycle");
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

            public override void setVisible(System.Boolean visible)
            {
                if (msetVisible_0.CheckShouldInvokeBase(this.instance))
                    base.setVisible(visible);
                else
                    msetVisible_0.Invoke(this.instance, visible);
            }

            public override void setAsLastSibling(System.Boolean refreshDepth)
            {
                if (msetAsLastSibling_1.CheckShouldInvokeBase(this.instance))
                    base.setAsLastSibling(refreshDepth);
                else
                    msetAsLastSibling_1.Invoke(this.instance, refreshDepth);
            }

            public override void setParent(global::myUIObject parent, System.Boolean refreshDepth)
            {
                if (msetParent_2.CheckShouldInvokeBase(this.instance))
                    base.setParent(parent, refreshDepth);
                else
                    msetParent_2.Invoke(this.instance, parent, refreshDepth);
            }

            public override void setScript(global::LayoutScript script)
            {
                if (msetScript_3.CheckShouldInvokeBase(this.instance))
                    base.setScript(script);
                else
                    msetScript_3.Invoke(this.instance, script);
            }

            public override void assignWindow(global::myUIObject parent, global::myUIObject template, System.String name)
            {
                massignWindow_4.Invoke(this.instance, parent, template, name);
            }

            public override void init()
            {
                if (minit_5.CheckShouldInvokeBase(this.instance))
                    base.init();
                else
                    minit_5.Invoke(this.instance);
            }

            public override void destroy()
            {
                if (mdestroy_6.CheckShouldInvokeBase(this.instance))
                    base.destroy();
                else
                    mdestroy_6.Invoke(this.instance);
            }

            public override void reset()
            {
                if (mreset_7.CheckShouldInvokeBase(this.instance))
                    base.reset();
                else
                    mreset_7.Invoke(this.instance);
            }

            public override void recycle()
            {
                if (mrecycle_8.CheckShouldInvokeBase(this.instance))
                    base.recycle();
                else
                    mrecycle_8.Invoke(this.instance);
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

