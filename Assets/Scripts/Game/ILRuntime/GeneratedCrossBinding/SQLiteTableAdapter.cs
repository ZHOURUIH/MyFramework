using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class SQLiteTableAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo mlinkTable_0 = new CrossBindingMethodInfo("linkTable");
        static CrossBindingMethodInfo mnotifyConstructDone_1 = new CrossBindingMethodInfo("notifyConstructDone");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::SQLiteTable);
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

        public class Adapter : global::SQLiteTable, CrossBindingAdaptorType
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

            public override void linkTable()
            {
                if (mlinkTable_0.CheckShouldInvokeBase(this.instance))
                    base.linkTable();
                else
                    mlinkTable_0.Invoke(this.instance);
            }

            public override void notifyConstructDone()
            {
                if (mnotifyConstructDone_1.CheckShouldInvokeBase(this.instance))
                    base.notifyConstructDone();
                else
                    mnotifyConstructDone_1.Invoke(this.instance);
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

