using System;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
#if USE_HYBRID_CLR
using HybridCLR.Editor.Settings;
#endif
#if USE_OBFUZ
using Obfuz.Settings;
#endif
using static EditorDefine;

public class MenuInit
{
    public const string MENU_NAME = MENU_ROOT_NAME + "初始化/";                                     // 初始化菜单根路径
    private const string PACKAGE_NAME = "com.zhourui.myframework";                                  // 包名
    private const string PROJECT_TEMPLATE_PATH = "Packages/" + PACKAGE_NAME + "/ProjectTemplate~/"; // 包内项目模板目录
    private const string START_SCENE = "Assets/Resources/Scene/start.unity";                        // 启动场景路径
    private const string GAME_ENTRY_NODE_NAME = "GameEntry";                                        // 启动节点名称
    private const string GAME_ENTRY_TYPE_NAME = "GameEntry";                                        // 启动脚本类型名,如果有命名空间需要改成完整名
    private const string SESSION_PENDING_ATTACH_GAMEENTRY = "MyFramework.PendingAttachGameEntry";   // 等待挂接GameEntry的Session标记
    [MenuItem(MENU_NAME + "初始化框架", false, 0)]
    public static void initFramework()
    {
        doInit(false);
    }
    [MenuItem(MENU_NAME + "初始化框架+HybridCLR+Obfuz", false, 0)]
    public static void initFrameworkHybridCLRObfuz()
    {
        doInit(true);
    }
    //---------------------------------------------------------------------------------------------------------------------------
    // Unity重新编译并DomainReload以后,继续执行初始化中断的挂接流程
    [InitializeOnLoadMethod]
    private static void initOnLoad()
    {
        EditorApplication.delayCall += continueAttachGameEntryIfNeed;
    }
    // 初始化框架
    protected static void doInit(bool withHybridCLRObfuz)
    {
        if (!copyProjectTemplateFiles(out bool needCompile))
        {
            clearPendingAttachGameEntry();
            return;
        }

        FrameEditorSettings.save();
        MenuSetting.createRuntimeSettinsFile();

        if (withHybridCLRObfuz)
        {
            initHybridCLRObfuz();
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        if (needCompile)
        {
            requestAttachGameEntryAfterCompile();
            Debug.Log("MyFramework模板脚本已复制,等待Unity编译完成后自动挂接GameEntry");
            return;
        }

        AttachGameEntryResult attachResult = attachGameEntryToStartScene(true);
        if (attachResult == AttachGameEntryResult.Success)
        {
            addStartSceneToBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("MyFramework初始化完成");
            return;
        }

        if (attachResult == AttachGameEntryResult.MissingGameEntryType)
        {
            requestAttachGameEntryAfterCompile();
            Debug.Log("未找到GameEntry类型,等待Unity重新编译后自动挂接GameEntry");
            return;
        }

        clearPendingAttachGameEntry();
        Debug.LogError("MyFramework初始化失败,请检查模板文件是否完整");
    }
    // 拷贝ProjectTemplate~中的所有文件到工程根目录,保持原有相对路径
    protected static bool copyProjectTemplateFiles(out bool needCompile)
    {
        needCompile = false;
        string projectRootPath = getProjectRootPath();
        string templateFullPath = projectRootPath + PROJECT_TEMPLATE_PATH;
        if (!Directory.Exists(templateFullPath))
        {
            Debug.LogError("未找到项目模板目录: " + PROJECT_TEMPLATE_PATH);
            return false;
        }

        foreach (string sourceFullPathOrigin in Directory.GetFiles(templateFullPath, "*", SearchOption.AllDirectories))
        {
            string sourceFullPath = sourceFullPathOrigin.Replace("\\", "/");
            if (sourceFullPath.endWith(".meta"))
            {
                continue;
            }
            string relativePath = sourceFullPath[templateFullPath.Length..];
            string targetFullPath = (projectRootPath + relativePath).Replace("\\", "/");
            string targetAssetPath = relativePath.Replace("\\", "/");
            if (File.Exists(targetFullPath))
            {
                Debug.Log("模板文件已存在,跳过: " + targetAssetPath);
                continue;
            }

            string targetFolder = Path.GetDirectoryName(targetFullPath);
            if (!targetFolder.isEmpty())
            {
                Directory.CreateDirectory(targetFolder);
            }
            File.Copy(sourceFullPath, targetFullPath, false);
            Debug.Log("复制模板文件: " + targetAssetPath);
            if (isScriptCompileFile(targetFullPath))
            {
                needCompile = true;
            }
        }
        return true;
    }
    // 请求编译完成后挂接GameEntry
    protected static void requestAttachGameEntryAfterCompile()
    {
        SessionState.SetBool(SESSION_PENDING_ATTACH_GAMEENTRY, true);
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        CompilationPipeline.RequestScriptCompilation();
    }
    // 清理等待挂接GameEntry的Session标记
    protected static void clearPendingAttachGameEntry()
    {
        SessionState.SetBool(SESSION_PENDING_ATTACH_GAMEENTRY, false);
    }
    // 编译完成后继续挂接GameEntry
    protected static void continueAttachGameEntryIfNeed()
    {
        if (!SessionState.GetBool(SESSION_PENDING_ATTACH_GAMEENTRY, false))
        {
            return;
        }

        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += continueAttachGameEntryIfNeed;
            return;
        }

        // 插件反复移除/重新添加时,SessionState可能残留,但用户工程中还没有执行初始化,此时不应该报错
        if (!hasStartScene())
        {
            clearPendingAttachGameEntry();
            Debug.Log("检测到残留的MyFramework初始化挂接标记,但启动场景不存在,已自动清理");
            return;
        }

        AttachGameEntryResult attachResult = attachGameEntryToStartScene(false);
        if (attachResult == AttachGameEntryResult.Success)
        {
            clearPendingAttachGameEntry();
            addStartSceneToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("MyFramework初始化完成,已自动挂接GameEntry");
            return;
        }

        clearPendingAttachGameEntry();

        if (attachResult == AttachGameEntryResult.MissingGameEntryType)
        {
            Debug.LogError("GameEntry挂接失败,未找到GameEntry类型,请确认模板脚本已经复制并且编译成功");
            return;
        }
        if (attachResult == AttachGameEntryResult.MissingGameEntryNode)
        {
            Debug.LogError("GameEntry挂接失败,启动场景中未找到根节点: " + GAME_ENTRY_NODE_NAME);
            return;
        }
        Debug.LogError("GameEntry挂接失败");
    }
    // 打开启动场景,并将用户工程中的GameEntry脚本挂到GameEntry根节点
    protected static AttachGameEntryResult attachGameEntryToStartScene(bool logError)
    {
        if (!hasStartScene())
        {
            if (logError)
            {
                Debug.LogError("未找到启动场景: " + START_SCENE);
            }
            return AttachGameEntryResult.MissingStartScene;
        }

        Type gameEntryType = findMonoBehaviourType(GAME_ENTRY_TYPE_NAME);
        if (gameEntryType == null)
        {
            if (logError)
            {
                Debug.LogWarning("未找到GameEntry类型: " + GAME_ENTRY_TYPE_NAME);
            }
            return AttachGameEntryResult.MissingGameEntryType;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return AttachGameEntryResult.Cancelled;
        }

        Scene scene = EditorSceneManager.OpenScene(START_SCENE, OpenSceneMode.Single);
        GameObject gameEntryObject = scene.GetRootGameObjects().find(item => item.name == GAME_ENTRY_NODE_NAME);
        if (gameEntryObject == null)
        {
            if (logError)
            {
                Debug.LogError("启动场景中未找到根节点: " + GAME_ENTRY_NODE_NAME);
            }
            return AttachGameEntryResult.MissingGameEntryNode;
        }

        int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameEntryObject);
        if (count > 0)
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameEntryObject);
            EditorUtility.SetDirty(gameEntryObject);
            Debug.Log("已移除Missing Script数量: " + count + ", 节点: " + gameEntryObject.name);
        }

        if (gameEntryObject.GetComponent(gameEntryType) == null)
        {
            gameEntryObject.AddComponent(gameEntryType);
            EditorUtility.SetDirty(gameEntryObject);
            Debug.Log("已挂接GameEntry脚本: " + gameEntryType.FullName);
        }
        else
        {
            Debug.Log("GameEntry脚本已经存在: " + gameEntryType.FullName);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return AttachGameEntryResult.Success;
    }
    // 判断启动场景是否存在,同时兼容刚复制但AssetDatabase还没完全导入的情况
    protected static bool hasStartScene()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(START_SCENE) != null ||
            !File.Exists(getProjectRootPath() + START_SCENE))
        {
            return true;
        }
        AssetDatabase.ImportAsset(START_SCENE, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<SceneAsset>(START_SCENE) != null;
    }
    // 将启动场景添加到Build Settings第一位
    protected static void addStartSceneToBuildSettings()
    {
        if (!hasStartScene())
        {
            Debug.LogWarning("启动场景不存在,无法添加到Build Settings: " + START_SCENE);
            return;
        }

        EditorBuildSettingsScene[] oldScenes = EditorBuildSettings.scenes;
        for (int i = 0; i < oldScenes.Length; ++i)
        {
            if (oldScenes[i].path == START_SCENE)
            {
                if (!oldScenes[i].enabled)
                {
                    oldScenes[i].enabled = true;
                    EditorBuildSettings.scenes = oldScenes;
                }
                Debug.Log("启动场景已在Build Settings中: " + START_SCENE);
                return;
            }
        }

        EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[oldScenes.Length + 1];
        newScenes[0] = new EditorBuildSettingsScene(START_SCENE, true);
        for (int i = 0; i < oldScenes.Length; ++i)
        {
            newScenes[i + 1] = oldScenes[i];
        }
        EditorBuildSettings.scenes = newScenes;
        Debug.Log("已添加启动场景到Build Settings: " + START_SCENE);
    }
    // 初始化HybridCLR和Obfuz,实际上就只是改一下生成文件的路径,使之都生成到Scripts/AutoGenerated目录中
    protected static void initHybridCLRObfuz()
    {
#if USE_OBFUZ
        ObfuzSettings.Instance.encryptionVMSettings.codeOutputPath = "Assets/Scripts/AutoGenerated/Obfuz/GeneratedEncryptionVirtualMachine.cs";
        ObfuzSettings.Save();
#endif
#if USE_HYBRID_CLR
        HybridCLRSettings.Instance.outputAOTGenericReferenceFile = "Scripts/AutoGenerated/HybridCLRGenerate/AOTGenericReferences.cs";
        HybridCLRSettings.Save();
#endif
    }
    // 通过类型名查找MonoBehaviour类型,避免Editor_Frame直接引用Game程序集
    protected static Type findMonoBehaviourType(string typeName)
    {
        foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(typeName);
            if (type != null && typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                return type;
            }
        }

        foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch
            {
                continue;
            }
            foreach (Type type in types)
            {
                if (type.Name == typeName && typeof(MonoBehaviour).IsAssignableFrom(type))
                {
                    return type;
                }
            }
        }
        return null;
    }
    // 判断复制的文件是否会触发脚本编译
    protected static bool isScriptCompileFile(string filePath)
    {
        string extension = Path.GetExtension(filePath);
        return extension == ".cs" ||
               extension == ".asmdef" ||
               extension == ".asmref" ||
               extension == ".rsp";
    }
    // 获取Unity工程根目录
    protected static string getProjectRootPath()
    {
        string dataPath = Application.dataPath.Replace("\\", "/");
        return Directory.GetParent(dataPath)?.FullName.Replace("\\", "/") + "/";
    }
    // 挂接GameEntry的结果
    protected enum AttachGameEntryResult
    {
        Success,               // 挂接成功
        MissingStartScene,     // 缺少启动场景
        MissingGameEntryType,  // 缺少GameEntry类型
        MissingGameEntryNode,  // 缺少GameEntry节点
        Cancelled,             // 用户取消保存场景
    }
}