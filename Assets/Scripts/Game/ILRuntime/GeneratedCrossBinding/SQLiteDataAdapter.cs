using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class SQLiteDataAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo<Mono.Data.Sqlite.SqliteDataReader> mparse_0 = new CrossBindingMethodInfo<Mono.Data.Sqlite.SqliteDataReader>("parse");
        class insert_1Info : CrossBindingMethodInfo
        {
            static Type[] pTypes = new Type[] {typeof(System.String).MakeByRefType()};

            public insert_1Info()
                : base("insert")
            {

            }

            protected override Type ReturnType { get { return null; } }

            protected override Type[] Parameters { get { return pTypes; } }
            public void Invoke(ILTypeInstance instance, ref System.String valueString)
            {
                EnsureMethod(instance);
                if (method != null)
                {
                    invoking = true;
                    try
                    {
                        using (var ctx = domain.BeginInvoke(method))
                        {
                            ctx.PushObject(valueString);
                            ctx.PushObject(instance);
                            ctx.PushReference(0);
                            ctx.Invoke();
                            valueString = ctx.ReadObject<System.String>(0);
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
        static insert_1Info minsert_1 = new insert_1Info();
        static CrossBindingMethodInfo<global::MyStringBuilder> minsert_2 = new CrossBindingMethodInfo<global::MyStringBuilder>("insert");
        static CrossBindingFunctionInfo<System.Boolean> mcheckData_3 = new CrossBindingFunctionInfo<System.Boolean>("checkData");
        static CrossBindingMethodInfo mnotifyConstructDone_4 = new CrossBindingMethodInfo("notifyConstructDone");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::SQLiteData);
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

        public class Adapter : global::SQLiteData, CrossBindingAdaptorType
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

            public override void parse(Mono.Data.Sqlite.SqliteDataReader reader)
            {
                if (mparse_0.CheckShouldInvokeBase(this.instance))
                    base.parse(reader);
                else
                    mparse_0.Invoke(this.instance, reader);
            }

            public override void insert(ref System.String valueString)
            {
                if (minsert_1.CheckShouldInvokeBase(this.instance))
                    base.insert(ref valueString);
                else
                    minsert_1.Invoke(this.instance, ref valueString);
            }

            public override void insert(global::MyStringBuilder valueString)
            {
                if (minsert_2.CheckShouldInvokeBase(this.instance))
                    base.insert(valueString);
                else
                    minsert_2.Invoke(this.instance, valueString);
            }

            public override System.Boolean checkData()
            {
                if (mcheckData_3.CheckShouldInvokeBase(this.instance))
                    return base.checkData();
                else
                    return mcheckData_3.Invoke(this.instance);
            }

            public override void notifyConstructDone()
            {
                if (mnotifyConstructDone_4.CheckShouldInvokeBase(this.instance))
                    base.notifyConstructDone();
                else
                    mnotifyConstructDone_4.Invoke(this.instance);
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

