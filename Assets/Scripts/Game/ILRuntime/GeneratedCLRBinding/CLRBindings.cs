using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {

        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Quaternion> s_UnityEngine_Quaternion_Binding_Binder = null;
        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector2> s_UnityEngine_Vector2_Binding_Binder = null;
        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector2Int> s_UnityEngine_Vector2Int_Binding_Binder = null;
        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector3> s_UnityEngine_Vector3_Binding_Binder = null;
        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector3Int> s_UnityEngine_Vector3Int_Binding_Binder = null;

        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            System_Type_Binding.Register(app);
            ComponentOwner_Binding.Register(app);
            FrameBase_Binding.Register(app);
            CommandSystem_Binding.Register(app);
            UnityEngine_Vector3_Binding.Register(app);
            MathUtility_Binding.Register(app);
            Transformable_Binding.Register(app);
            GameScene_Binding.Register(app);
            LT_Binding.Register(app);
            UnityUtility_Binding.Register(app);
            CommandCharacterManagerCreateCharacter_Binding.Register(app);
            CharacterManager_Binding.Register(app);
            Character_Binding.Register(app);
            CommandCharacterManagerDestroy_Binding.Register(app);
            CommandGameSceneManagerEnter_Binding.Register(app);
            GameBase_Binding.Register(app);
            System_Object_Binding.Register(app);
            MovableObject_Binding.Register(app);
            CommandReceiver_Binding.Register(app);
            GameBasePooledObject_Binding.Register(app);
            Command_Binding.Register(app);
            FrameSystem_Binding.Register(app);
            GameComponent_Binding.Register(app);
            PlayerState_Binding.Register(app);
            PooledWindow_Binding.Register(app);
            SceneInstance_Binding.Register(app);
            SceneProcedure_Binding.Register(app);
            LayoutScript_Binding.Register(app);
            SQLiteData_Binding.Register(app);
            SQLiteTable_Binding.Register(app);
            GameLayout_Binding.Register(app);
            LayoutManager_Binding.Register(app);
            FT_Binding.Register(app);
            StringUtility_Binding.Register(app);
            System_String_Binding.Register(app);
            myUGUIText_Binding.Register(app);

            ILRuntime.CLR.TypeSystem.CLRType __clrType = null;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Quaternion));
            s_UnityEngine_Quaternion_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Quaternion>;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Vector2));
            s_UnityEngine_Vector2_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector2>;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Vector2Int));
            s_UnityEngine_Vector2Int_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector2Int>;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Vector3));
            s_UnityEngine_Vector3_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector3>;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Vector3Int));
            s_UnityEngine_Vector3Int_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector3Int>;
        }

        /// <summary>
        /// Release the CLR binding, please invoke this BEFORE ILRuntime Appdomain destroy
        /// </summary>
        public static void Shutdown(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            s_UnityEngine_Quaternion_Binding_Binder = null;
            s_UnityEngine_Vector2_Binding_Binder = null;
            s_UnityEngine_Vector2Int_Binding_Binder = null;
            s_UnityEngine_Vector3_Binding_Binder = null;
            s_UnityEngine_Vector3Int_Binding_Binder = null;
        }
    }
}
