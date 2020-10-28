using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace HotFix
{   
    public class PlayerStateAdapter : CrossBindingAdaptor
    {
        static CrossBindingMethodInfo<global::Character> msetPlayer_0 = new CrossBindingMethodInfo<global::Character>("setPlayer");
        static CrossBindingFunctionInfo<System.Boolean> mcanEnter_1 = new CrossBindingFunctionInfo<System.Boolean>("canEnter");
        static CrossBindingMethodInfo menter_2 = new CrossBindingMethodInfo("enter");
        static CrossBindingMethodInfo<System.Single> mupdate_3 = new CrossBindingMethodInfo<System.Single>("update");
        static CrossBindingMethodInfo<System.Single> mfixedUpdate_4 = new CrossBindingMethodInfo<System.Single>("fixedUpdate");
        static CrossBindingMethodInfo<System.Boolean, System.String> mleave_5 = new CrossBindingMethodInfo<System.Boolean, System.String>("leave");
        static CrossBindingMethodInfo<System.Single> mkeyProcess_6 = new CrossBindingMethodInfo<System.Single>("keyProcess");
        static CrossBindingFunctionInfo<System.Int32> mgetPriority_7 = new CrossBindingFunctionInfo<System.Int32>("getPriority");
        static CrossBindingMethodInfo mresetProperty_8 = new CrossBindingMethodInfo("resetProperty");
        static CrossBindingMethodInfo mnotifyConstructDone_9 = new CrossBindingMethodInfo("notifyConstructDone");
        public override Type BaseCLRType
        {
            get
            {
                return typeof(global::PlayerState);
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

        public class Adapter : global::PlayerState, CrossBindingAdaptorType
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

            public override void setPlayer(global::Character player)
            {
                if (msetPlayer_0.CheckShouldInvokeBase(this.instance))
                    base.setPlayer(player);
                else
                    msetPlayer_0.Invoke(this.instance, player);
            }

            public override System.Boolean canEnter()
            {
                if (mcanEnter_1.CheckShouldInvokeBase(this.instance))
                    return base.canEnter();
                else
                    return mcanEnter_1.Invoke(this.instance);
            }

            public override void enter()
            {
                if (menter_2.CheckShouldInvokeBase(this.instance))
                    base.enter();
                else
                    menter_2.Invoke(this.instance);
            }

            public override void update(System.Single elapsedTime)
            {
                if (mupdate_3.CheckShouldInvokeBase(this.instance))
                    base.update(elapsedTime);
                else
                    mupdate_3.Invoke(this.instance, elapsedTime);
            }

            public override void fixedUpdate(System.Single elapsedTime)
            {
                if (mfixedUpdate_4.CheckShouldInvokeBase(this.instance))
                    base.fixedUpdate(elapsedTime);
                else
                    mfixedUpdate_4.Invoke(this.instance, elapsedTime);
            }

            public override void leave(System.Boolean isBreak, System.String param)
            {
                if (mleave_5.CheckShouldInvokeBase(this.instance))
                    base.leave(isBreak, param);
                else
                    mleave_5.Invoke(this.instance, isBreak, param);
            }

            public override void keyProcess(System.Single elapsedTime)
            {
                if (mkeyProcess_6.CheckShouldInvokeBase(this.instance))
                    base.keyProcess(elapsedTime);
                else
                    mkeyProcess_6.Invoke(this.instance, elapsedTime);
            }

            public override System.Int32 getPriority()
            {
                if (mgetPriority_7.CheckShouldInvokeBase(this.instance))
                    return base.getPriority();
                else
                    return mgetPriority_7.Invoke(this.instance);
            }

            public override void resetProperty()
            {
                if (mresetProperty_8.CheckShouldInvokeBase(this.instance))
                    base.resetProperty();
                else
                    mresetProperty_8.Invoke(this.instance);
            }

            public override void notifyConstructDone()
            {
                if (mnotifyConstructDone_9.CheckShouldInvokeBase(this.instance))
                    base.notifyConstructDone();
                else
                    mnotifyConstructDone_9.Invoke(this.instance);
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

