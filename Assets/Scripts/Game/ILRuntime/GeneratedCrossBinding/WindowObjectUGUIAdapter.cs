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
    public class WindowObjectUGUIAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::WindowObjectUGUI);
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

        public class Adapter : global::WindowObjectUGUI, CrossBindingAdaptorType
        {
            CrossBindingMethodInfo<global::myUGUIObject> massignWindow_0 = new CrossBindingMethodInfo<global::myUGUIObject>("assignWindow");
            CrossBindingMethodInfo<global::myUIObject, System.String> massignWindow_1 = new CrossBindingMethodInfo<global::myUIObject, System.String>("assignWindow");
            CrossBindingMethodInfo<global::myUIObject, global::myUIObject, System.String> massignWindow_2 = new CrossBindingMethodInfo<global::myUIObject, global::myUIObject, System.String>("assignWindow");
            CrossBindingMethodInfo<global::LayoutScript> msetScript_3 = new CrossBindingMethodInfo<global::LayoutScript>("setScript");
            CrossBindingMethodInfo minit_4 = new CrossBindingMethodInfo("init");
            CrossBindingMethodInfo mdestroy_5 = new CrossBindingMethodInfo("destroy");
            CrossBindingMethodInfo mreset_6 = new CrossBindingMethodInfo("reset");
            CrossBindingMethodInfo mrecycle_7 = new CrossBindingMethodInfo("recycle");
            CrossBindingMethodInfo mresetProperty_8 = new CrossBindingMethodInfo("resetProperty");

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

            public override void assignWindow(global::myUGUIObject itemRoot)
            {
                if (massignWindow_0.CheckShouldInvokeBase(this.instance))
                    base.assignWindow(itemRoot);
                else
                    massignWindow_0.Invoke(this.instance, itemRoot);
            }

            public override void assignWindow(global::myUIObject parent, System.String name)
            {
                if (massignWindow_1.CheckShouldInvokeBase(this.instance))
                    base.assignWindow(parent, name);
                else
                    massignWindow_1.Invoke(this.instance, parent, name);
            }

            public override void assignWindow(global::myUIObject parent, global::myUIObject template, System.String name)
            {
                if (massignWindow_2.CheckShouldInvokeBase(this.instance))
                    base.assignWindow(parent, template, name);
                else
                    massignWindow_2.Invoke(this.instance, parent, template, name);
            }

            public override void setScript(global::LayoutScript script)
            {
                if (msetScript_3.CheckShouldInvokeBase(this.instance))
                    base.setScript(script);
                else
                    msetScript_3.Invoke(this.instance, script);
            }

            public override void init()
            {
                if (minit_4.CheckShouldInvokeBase(this.instance))
                    base.init();
                else
                    minit_4.Invoke(this.instance);
            }

            public override void destroy()
            {
                if (mdestroy_5.CheckShouldInvokeBase(this.instance))
                    base.destroy();
                else
                    mdestroy_5.Invoke(this.instance);
            }

            public override void reset()
            {
                if (mreset_6.CheckShouldInvokeBase(this.instance))
                    base.reset();
                else
                    mreset_6.Invoke(this.instance);
            }

            public override void recycle()
            {
                if (mrecycle_7.CheckShouldInvokeBase(this.instance))
                    base.recycle();
                else
                    mrecycle_7.Invoke(this.instance);
            }

            public override void resetProperty()
            {
                if (mresetProperty_8.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_8.Invoke(this.instance);
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
