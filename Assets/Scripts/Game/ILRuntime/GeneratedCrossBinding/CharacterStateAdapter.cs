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
    public class CharacterStateAdapter : CrossBindingAdaptor
    {
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
            CrossBindingMethodInfo mdestroy_0 = new CrossBindingMethodInfo("destroy");
            CrossBindingMethodInfo<global::Character> msetCharacter_1 = new CrossBindingMethodInfo<global::Character>("setCharacter");
            CrossBindingFunctionInfo<System.Boolean> mcanEnter_2 = new CrossBindingFunctionInfo<System.Boolean>("canEnter");
            CrossBindingMethodInfo menter_3 = new CrossBindingMethodInfo("enter");
            CrossBindingMethodInfo<System.Single> mupdate_4 = new CrossBindingMethodInfo<System.Single>("update");
            CrossBindingMethodInfo<System.Single> mfixedUpdate_5 = new CrossBindingMethodInfo<System.Single>("fixedUpdate");
            CrossBindingMethodInfo<System.Boolean, System.String> mleave_6 = new CrossBindingMethodInfo<System.Boolean, System.String>("leave");
            CrossBindingMethodInfo<System.Single> mkeyProcess_7 = new CrossBindingMethodInfo<System.Single>("keyProcess");
            CrossBindingFunctionInfo<System.Int32> mgetPriority_8 = new CrossBindingFunctionInfo<System.Int32>("getPriority");
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

