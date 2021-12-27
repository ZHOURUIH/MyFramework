using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class WindowObjectUIAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo mresetProperty_7 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo<System.Boolean> msetDestroy_8 = new CrossBindingMethodInfo<System.Boolean>("setDestroy");
        static CrossBindingFunctionInfo<System.Boolean> misDestroy_9 = new CrossBindingFunctionInfo<System.Boolean>("isDestroy");
        static CrossBindingMethodInfo<System.Int64> msetAssignID_10 = new CrossBindingMethodInfo<System.Int64>("setAssignID");
        static CrossBindingFunctionInfo<System.Int64> mgetAssignID_11 = new CrossBindingFunctionInfo<System.Int64>("getAssignID");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::WindowObjectUI);
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

        public class Adapter : global::WindowObjectUI, CrossBindingAdaptorType
        {
            protected CrossBindingMethodInfo mdestroy_4 = new CrossBindingMethodInfo("destroy");
            protected CrossBindingMethodInfo mrecycle_6 = new CrossBindingMethodInfo("recycle");
            protected CrossBindingMethodInfo<global::LayoutScript> msetScript_2 = new CrossBindingMethodInfo<global::LayoutScript>("setScript");
            protected CrossBindingMethodInfo mreset_5 = new CrossBindingMethodInfo("reset");
            protected CrossBindingMethodInfo minit_3 = new CrossBindingMethodInfo("init");
            protected CrossBindingMethodInfo<global::myUIObject, global::myUIObject, System.String> massignWindow_1 = new CrossBindingMethodInfo<global::myUIObject, global::myUIObject, System.String>("assignWindow");
            protected CrossBindingMethodInfo<global::myUIObject, System.String> massignWindow_0 = new CrossBindingMethodInfo<global::myUIObject, System.String>("assignWindow");
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

            public override void assignWindow(global::myUIObject parent, System.String name)
            {
                if (massignWindow_0.CheckShouldInvokeBase(this.instance))
                    base.assignWindow(parent, name);
                else
                    massignWindow_0.Invoke(this.instance, parent, name);
            }

            public override void assignWindow(global::myUIObject parent, global::myUIObject template, System.String name)
            {
                if (massignWindow_1.CheckShouldInvokeBase(this.instance))
                    base.assignWindow(parent, template, name);
                else
                    massignWindow_1.Invoke(this.instance, parent, template, name);
            }

            public override void setScript(global::LayoutScript script)
            {
                if (msetScript_2.CheckShouldInvokeBase(this.instance))
                    base.setScript(script);
                else
                    msetScript_2.Invoke(this.instance, script);
            }

            public override void init()
            {
                if (minit_3.CheckShouldInvokeBase(this.instance))
                    base.init();
                else
                    minit_3.Invoke(this.instance);
            }

            public override void destroy()
            {
                if (mdestroy_4.CheckShouldInvokeBase(this.instance))
                    base.destroy();
                else
                    mdestroy_4.Invoke(this.instance);
            }

            public override void reset()
            {
                if (mreset_5.CheckShouldInvokeBase(this.instance))
                    base.reset();
                else
                    mreset_5.Invoke(this.instance);
            }

            public override void recycle()
            {
                if (mrecycle_6.CheckShouldInvokeBase(this.instance))
                    base.recycle();
                else
                    mrecycle_6.Invoke(this.instance);
            }

            public override void resetProperty()
            {
                if (mresetProperty_7.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_7.Invoke(this.instance);
            }

            public override void setDestroy(System.Boolean isDestroy)
            {
                if (msetDestroy_8.CheckShouldInvokeBase(this.instance))
                    base.setDestroy(isDestroy);
                else
                    msetDestroy_8.Invoke(this.instance, isDestroy);
            }

            public override System.Boolean isDestroy()
            {
                if (misDestroy_9.CheckShouldInvokeBase(this.instance))
                    return base.isDestroy();
                else
                    return misDestroy_9.Invoke(this.instance);
            }

            public override void setAssignID(System.Int64 assignID)
            {
                if (msetAssignID_10.CheckShouldInvokeBase(this.instance))
                    base.setAssignID(assignID);
                else
                    msetAssignID_10.Invoke(this.instance, assignID);
            }

            public override System.Int64 getAssignID()
            {
                if (mgetAssignID_11.CheckShouldInvokeBase(this.instance))
                    return base.getAssignID();
                else
                    return mgetAssignID_11.Invoke(this.instance);
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
