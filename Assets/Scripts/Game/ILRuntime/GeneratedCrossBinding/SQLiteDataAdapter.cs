using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class SQLiteDataAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo mresetProperty_0 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo<global::Mono.Data.Sqlite.SqliteDataReader> mparse_1 = new CrossBindingMethodInfo<global::Mono.Data.Sqlite.SqliteDataReader>("parse");
        class insert_2Info : CrossBindingMethodInfo
        {
            static Type[] pTypes = new Type[] {typeof(System.String).MakeByRefType()};

            public insert_2Info()
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
        static insert_2Info minsert_2 = new insert_2Info();
        static CrossBindingMethodInfo<global::MyStringBuilder> minsert_3 = new CrossBindingMethodInfo<global::MyStringBuilder>("insert");
        static CrossBindingFunctionInfo<System.Boolean> mcheckData_4 = new CrossBindingFunctionInfo<System.Boolean>("checkData");
        static CrossBindingMethodInfo<System.Boolean> msetDestroy_5 = new CrossBindingMethodInfo<System.Boolean>("setDestroy");
        static CrossBindingFunctionInfo<System.Boolean> misDestroy_6 = new CrossBindingFunctionInfo<System.Boolean>("isDestroy");
        static CrossBindingMethodInfo<System.Int64> msetAssignID_7 = new CrossBindingMethodInfo<System.Int64>("setAssignID");
        static CrossBindingFunctionInfo<System.Int64> mgetAssignID_8 = new CrossBindingFunctionInfo<System.Int64>("getAssignID");
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

            public override void resetProperty()
            {
                if (mresetProperty_0.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_0.Invoke(this.instance);
            }

            public override void parse(global::Mono.Data.Sqlite.SqliteDataReader reader)
            {
                if (mparse_1.CheckShouldInvokeBase(this.instance))
                    base.parse(reader);
                else
                    mparse_1.Invoke(this.instance, reader);
            }

            public override void insert(ref System.String valueString)
            {
                if (minsert_2.CheckShouldInvokeBase(this.instance))
                    base.insert(ref valueString);
                else
                    minsert_2.Invoke(this.instance, ref valueString);
            }

            public override void insert(global::MyStringBuilder valueString)
            {
                if (minsert_3.CheckShouldInvokeBase(this.instance))
                    base.insert(valueString);
                else
                    minsert_3.Invoke(this.instance, valueString);
            }

            public override System.Boolean checkData()
            {
                if (mcheckData_4.CheckShouldInvokeBase(this.instance))
                    return base.checkData();
                else
                    return mcheckData_4.Invoke(this.instance);
            }

            public override void setDestroy(System.Boolean isDestroy)
            {
                if (msetDestroy_5.CheckShouldInvokeBase(this.instance))
                    base.setDestroy(isDestroy);
                else
                    msetDestroy_5.Invoke(this.instance, isDestroy);
            }

            public override System.Boolean isDestroy()
            {
                if (misDestroy_6.CheckShouldInvokeBase(this.instance))
                    return base.isDestroy();
                else
                    return misDestroy_6.Invoke(this.instance);
            }

            public override void setAssignID(System.Int64 assignID)
            {
                if (msetAssignID_7.CheckShouldInvokeBase(this.instance))
                    base.setAssignID(assignID);
                else
                    msetAssignID_7.Invoke(this.instance, assignID);
            }

            public override System.Int64 getAssignID()
            {
                if (mgetAssignID_8.CheckShouldInvokeBase(this.instance))
                    return base.getAssignID();
                else
                    return mgetAssignID_8.Invoke(this.instance);
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

