using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class CharacterStateAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo mdestroy_0 = new CrossBindingMethodInfo("destroy");
        static CrossBindingMethodInfo<global::Character> msetCharacter_1 = new CrossBindingMethodInfo<global::Character>("setCharacter");
        static CrossBindingFunctionInfo<System.Boolean> mcanEnter_2 = new CrossBindingFunctionInfo<System.Boolean>("canEnter");
        static CrossBindingMethodInfo menter_3 = new CrossBindingMethodInfo("enter");
        static CrossBindingMethodInfo<System.Single> mupdate_4 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo<System.Single> mfixedUpdate_5 = new CrossBindingMethodInfo<System.Single>("fixedUpdate");
        static CrossBindingMethodInfo<System.Boolean, System.String> mleave_6 = new CrossBindingMethodInfo<System.Boolean, System.String>("leave");
        static CrossBindingMethodInfo<System.Single> mkeyProcess_7 = new CrossBindingMethodInfo<System.Single>("keyProcess");
        static CrossBindingFunctionInfo<System.Int32> mgetPriority_8 = new CrossBindingFunctionInfo<System.Int32>("getPriority");
        static CrossBindingMethodInfo mresetProperty_9 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo<System.Boolean> msetDestroy_10 = new CrossBindingMethodInfo<System.Boolean>("setDestroy");
        static CrossBindingFunctionInfo<System.Boolean> misDestroy_11 = new CrossBindingFunctionInfo<System.Boolean>("isDestroy");
        static CrossBindingMethodInfo<System.Int64> msetAssignID_12 = new CrossBindingMethodInfo<System.Int64>("setAssignID");
        static CrossBindingFunctionInfo<System.Int64> mgetAssignID_13 = new CrossBindingFunctionInfo<System.Int64>("getAssignID");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::CharacterState);
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

        public class Adapter : global::CharacterState, CrossBindingAdaptorType
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

            public override void destroy()
            {
                if (mdestroy_0.CheckShouldInvokeBase(this.instance))
                    base.destroy();
                else
                    mdestroy_0.Invoke(this.instance);
            }

            public override void setCharacter(global::Character character)
            {
                if (msetCharacter_1.CheckShouldInvokeBase(this.instance))
                    base.setCharacter(character);
                else
                    msetCharacter_1.Invoke(this.instance, character);
            }

            public override System.Boolean canEnter()
            {
                if (mcanEnter_2.CheckShouldInvokeBase(this.instance))
                    return base.canEnter();
                else
                    return mcanEnter_2.Invoke(this.instance);
            }

            public override void enter()
            {
                if (menter_3.CheckShouldInvokeBase(this.instance))
                    base.enter();
                else
                    menter_3.Invoke(this.instance);
            }

            public override void update(System.Single elapsedTime)
            {
                if (mupdate_4.CheckShouldInvokeBase(this.instance))
                    base.update(elapsedTime);
                else
                    mupdate_4.Invoke(this.instance, elapsedTime);
            }

            public override void fixedUpdate(System.Single elapsedTime)
            {
                if (mfixedUpdate_5.CheckShouldInvokeBase(this.instance))
                    base.fixedUpdate(elapsedTime);
                else
                    mfixedUpdate_5.Invoke(this.instance, elapsedTime);
            }

            public override void leave(System.Boolean isBreak, System.String param)
            {
                if (mleave_6.CheckShouldInvokeBase(this.instance))
                    base.leave(isBreak, param);
                else
                    mleave_6.Invoke(this.instance, isBreak, param);
            }

            public override void keyProcess(System.Single elapsedTime)
            {
                if (mkeyProcess_7.CheckShouldInvokeBase(this.instance))
                    base.keyProcess(elapsedTime);
                else
                    mkeyProcess_7.Invoke(this.instance, elapsedTime);
            }

            public override System.Int32 getPriority()
            {
                if (mgetPriority_8.CheckShouldInvokeBase(this.instance))
                    return base.getPriority();
                else
                    return mgetPriority_8.Invoke(this.instance);
            }

            public override void resetProperty()
            {
                if (mresetProperty_9.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_9.Invoke(this.instance);
            }

            public override void setDestroy(System.Boolean isDestroy)
            {
                if (msetDestroy_10.CheckShouldInvokeBase(this.instance))
                    base.setDestroy(isDestroy);
                else
                    msetDestroy_10.Invoke(this.instance, isDestroy);
            }

            public override System.Boolean isDestroy()
            {
                if (misDestroy_11.CheckShouldInvokeBase(this.instance))
                    return base.isDestroy();
                else
                    return misDestroy_11.Invoke(this.instance);
            }

            public override void setAssignID(System.Int64 assignID)
            {
                if (msetAssignID_12.CheckShouldInvokeBase(this.instance))
                    base.setAssignID(assignID);
                else
                    msetAssignID_12.Invoke(this.instance, assignID);
            }

            public override System.Int64 getAssignID()
            {
                if (mgetAssignID_13.CheckShouldInvokeBase(this.instance))
                    return base.getAssignID();
                else
                    return mgetAssignID_13.Invoke(this.instance);
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

