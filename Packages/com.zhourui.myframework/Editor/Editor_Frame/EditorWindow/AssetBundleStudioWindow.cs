using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using static FrameBaseDefine;

// 查看一个AssetBundle文件中的所有资源
public class AssetBundleStudioWindow : GameEditorWindow
{
    private class AssetItem
    {
        public string mAssetName;
        public string mTypeName;
        public Object mAsset;
    }

    private AssetBundle mAssetBundle;
    private string mAssetBundlePath;
    private readonly List<AssetItem> mAssetList = new();
    private Vector2 mAssetScroll;
    private Vector2 mPreviewScroll;
    private int mSelectIndex = -1;
    private string mSearchText = string.Empty;
    private void OnDisable()
    {
        unloadAssetBundle();
    }
    protected override void onGUI()
    {
        base.onGUI();
        drawToolbar();
        EditorGUILayout.Space(4.0f);
        EditorGUILayout.BeginHorizontal();
        drawAssetList();
        GUILayout.Space(4.0f);
        drawPreview();
        EditorGUILayout.EndHorizontal();
    }
    private void drawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("打开AB", EditorStyles.toolbarButton, GUILayout.Width(70.0f)))
        {
            openAssetBundle();
        }
        if (GUILayout.Button("卸载", EditorStyles.toolbarButton, GUILayout.Width(60.0f)))
        {
            unloadAssetBundle();
        }
        space(8);
        EditorGUILayout.LabelField("搜索", GUILayout.Width(30.0f));
        mSearchText = EditorGUILayout.TextField(mSearchText, EditorStyles.toolbarSearchField, GUILayout.Width(260.0f));
        GUILayout.FlexibleSpace();
        if (!mAssetBundlePath.isEmpty())
        {
            EditorGUILayout.LabelField(mAssetBundlePath, EditorStyles.miniLabel);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void drawAssetList()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(430.0f));
        EditorGUILayout.LabelField("Assets", EditorStyles.boldLabel);
        mAssetScroll = EditorGUILayout.BeginScrollView(mAssetScroll, "box");
        for (int i = 0; i < mAssetList.Count; ++i)
        {
            AssetItem item = mAssetList[i];
            if (!isMatchSearch(item))
            {
                continue;
            }

            Rect rect = EditorGUILayout.BeginHorizontal();
            if (mSelectIndex == i)
            {
                EditorGUI.DrawRect(rect, new Color(0.25f, 0.45f, 0.75f, 0.35f));
            }

            Texture icon = AssetPreview.GetMiniThumbnail(item.mAsset);
            GUILayout.Label(icon, GUILayout.Width(18.0f), GUILayout.Height(18.0f));
            if (GUILayout.Button(item.mAssetName, EditorStyles.label))
            {
                mSelectIndex = i;
                GUI.FocusControl(null);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(item.mTypeName, EditorStyles.miniLabel, GUILayout.Width(90.0f));
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.LabelField("资源数量: " + mAssetList.Count, EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
    }
    private void drawPreview()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
        mPreviewScroll = EditorGUILayout.BeginScrollView(mPreviewScroll, "box");

        AssetItem item = getSelectedItem();
        if (item == null || item.mAsset == null)
        {
            EditorGUILayout.HelpBox("未选择资源", MessageType.Info);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.LabelField("Name", item.mAssetName);
        EditorGUILayout.LabelField("Type", item.mTypeName);
        EditorGUILayout.ObjectField("Object", item.mAsset, typeof(Object), false);
        space(8);
        drawAssetPreview(item.mAsset);
        EditorGUILayout.EndScrollView();
        drawExportButtons(item);
        EditorGUILayout.EndVertical();
    }
    private void drawAssetPreview(Object asset)
    {
        if (asset is Texture2D texture)
        {
            drawTexturePreview(texture);
            return;
        }
        if (asset is Sprite sprite)
        {
            drawSpritePreview(sprite);
            return;
        }
        if (asset is TextAsset textAsset)
        {
            drawTextAssetPreview(textAsset);
            return;
        }
        if (asset is Material material)
        {
            drawMaterialPreview(material);
            return;
        }
        if (asset is GameObject gameObject)
        {
            drawGameObjectPreview(gameObject);
            return;
        }
        Texture preview = AssetPreview.GetAssetPreview(asset);
        if (preview != null)
        {
            drawPreviewTexture(preview, 512.0f);
            return;
        }
        EditorGUILayout.HelpBox("当前类型暂不支持预览:" + asset.GetType().Name, MessageType.Info);
    }
    private void drawTexturePreview(Texture2D texture)
    {
        EditorGUILayout.LabelField("Size", texture.width + " x " + texture.height);
        drawPreviewTexture(texture, 512.0f);
    }
    private void drawSpritePreview(Sprite sprite)
    {
        EditorGUILayout.LabelField("Texture", sprite.texture != null ? sprite.texture.name : "null");
        EditorGUILayout.LabelField("Rect", sprite.rect.ToString());
        EditorGUILayout.LabelField("Pivot", sprite.pivot.ToString());
        Texture2D preview = AssetPreview.GetAssetPreview(sprite);
        if (preview != null)
        {
            drawPreviewTexture(preview, 512.0f);
        }
        else if (sprite.texture != null)
        {
            drawPreviewTexture(sprite.texture, 512.0f);
        }
    }
    private void drawTextAssetPreview(TextAsset textAsset)
    {
        string text = textAsset.text;
        if (text == null)
        {
            EditorGUILayout.HelpBox("TextAsset没有可显示文本", MessageType.Info);
            return;
        }
        if (text.Length > 12000)
        {
            text = text.Substring(0, 12000) + "\n\n...... 文本过长,这里只显示前12000字符";
        }
        EditorGUILayout.TextArea(text, GUILayout.MinHeight(360.0f));
    }
    private void drawMaterialPreview(Material material)
    {
        Texture preview = AssetPreview.GetAssetPreview(material);
        if (preview != null)
        {
            drawPreviewTexture(preview, 512.0f);
        }
        Shader shader = material.shader;
        EditorGUILayout.LabelField("Shader", shader != null ? shader.name : "null");
    }
    private void drawGameObjectPreview(GameObject gameObject)
    {
        Texture preview = AssetPreview.GetAssetPreview(gameObject);
        if (preview != null)
        {
            drawPreviewTexture(preview, 512.0f);
        }
        EditorGUILayout.LabelField("GameObject", gameObject.name);
        EditorGUILayout.LabelField("Components", gameObject.GetComponents<Component>().Length.ToString());
    }
    private void drawPreviewTexture(Texture texture, float maxSize)
    {
        if (texture == null)
        {
            return;
        }

        float width = texture.width;
        float height = texture.height;
        float scale = Mathf.Min(maxSize / width, maxSize / height, 1.0f);
        Rect rect = GUILayoutUtility.GetRect(width * scale, height * scale, GUILayout.ExpandWidth(false));
        EditorGUI.DrawPreviewTexture(rect, texture, null, ScaleMode.ScaleToFit);
    }
    private void drawExportButtons(AssetItem item)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("导出选中资源", EditorStyles.toolbarButton, GUILayout.Width(110.0f)))
        {
            exportSelectedAsset(item);
        }
        if (GUILayout.Button("导出所有图片", EditorStyles.toolbarButton, GUILayout.Width(110.0f)))
        {
            exportAllImages();
        }
        EditorGUILayout.EndHorizontal();
    }
    private void openAssetBundle()
    {
        string path = EditorUtility.OpenFilePanel("选择AssetBundle", F_ASSET_BUNDLE_PATH, "");
        if (path.isEmpty())
        {
            return;
        }
        unloadAssetBundle();
        mAssetBundle = AssetBundle.LoadFromFile(path);
        if (mAssetBundle == null)
        {
            Debug.LogError("AssetBundle加载失败:" + path);
            return;
        }
        mAssetBundlePath = path;
        loadAssetList();
        Debug.Log("AssetBundle加载成功:" + path + ",资源数量:" + mAssetList.Count);
    }
    private void unloadAssetBundle()
    {
        if (mAssetBundle != null)
        {
            mAssetBundle.Unload(true);
            mAssetBundle = null;
        }
        mAssetBundlePath = string.Empty;
        mAssetList.Clear();
        mSelectIndex = -1;
    }
    private void loadAssetList()
    {
        mAssetList.Clear();
        mSelectIndex = -1;
        if (mAssetBundle == null)
        {
            return;
        }

        string[] assetNames = mAssetBundle.GetAllAssetNames();
        Array.Sort(assetNames, StringComparer.OrdinalIgnoreCase);
        foreach (string assetName in assetNames)
        {
            Object asset = null;
            string typeName = "Unknown";

            try
            {
                asset = mAssetBundle.LoadAsset<Object>(assetName);
                if (asset != null)
                {
                    typeName = asset.GetType().Name;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("加载资源失败:" + assetName + "\n" + e.Message);
            }

            mAssetList.Add(new AssetItem
            {
                mAssetName = assetName,
                mTypeName = typeName,
                mAsset = asset,
            });
        }

        string[] scenePaths = mAssetBundle.GetAllScenePaths();
        foreach (string scenePath in scenePaths)
        {
            mAssetList.Add(new AssetItem
            {
                mAssetName = scenePath,
                mTypeName = "Scene",
                mAsset = null,
            });
        }
    }
    private bool isMatchSearch(AssetItem item)
    {
        if (mSearchText.isEmpty())
        {
            return true;
        }
        return item.mAssetName.IndexOf(mSearchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
               item.mTypeName.IndexOf(mSearchText, StringComparison.OrdinalIgnoreCase) >= 0;
    }
    private AssetItem getSelectedItem()
    {
        if (mSelectIndex < 0 || mSelectIndex >= mAssetList.Count)
        {
            return null;
        }
        return mAssetList[mSelectIndex];
    }
    private void exportSelectedAsset(AssetItem item)
    {
        if (item == null || item.mAsset == null)
        {
            Debug.LogError("未选择可导出的资源");
            return;
        }
        string folder = EditorUtility.SaveFolderPanel("选择导出目录", "", "");
        if (folder.isEmpty())
        {
            return;
        }
        if (item.mAsset is Texture2D texture)
        {
            exportTexture(texture, folder, getSafeFileName(item.mAssetName));
            return;
        }
        if (item.mAsset is Sprite sprite)
        {
            exportSprite(sprite, folder, getSafeFileName(item.mAssetName));
            return;
        }
        if (item.mAsset is TextAsset textAsset)
        {
            exportTextAsset(textAsset, folder, getSafeFileName(item.mAssetName));
            return;
        }
        Debug.LogWarning("当前资源类型暂不支持导出:" + item.mAsset.GetType().Name);
    }
    private void exportAllImages()
    {
        if (mAssetList.Count == 0)
        {
            return;
        }
        string folder = EditorUtility.SaveFolderPanel("选择导出目录", "", "");
        if (folder.isEmpty())
        {
            return;
        }
        int count = 0;
        foreach (AssetItem item in mAssetList)
        {
            if (item.mAsset is Texture2D texture)
            {
                exportTexture(texture, folder, getSafeFileName(item.mAssetName));
                ++count;
            }
            else if (item.mAsset is Sprite sprite)
            {
                exportSprite(sprite, folder, getSafeFileName(item.mAssetName));
                ++count;
            }
        }
        Debug.Log("导出图片完成,数量:" + count + ",目录:" + folder);
    }
    private void exportTexture(Texture2D texture, string folder, string fileName)
    {
        Texture2D readable = copyTextureToReadable(texture);
        if (readable == null)
        {
            Debug.LogError("Texture导出失败:" + texture.name);
            return;
        }
        string path = Path.Combine(folder, fileName + ".png");
        File.WriteAllBytes(path, readable.EncodeToPNG());
        DestroyImmediate(readable);
        Debug.Log("导出Texture:" + path);
    }
    private void exportSprite(Sprite sprite, string folder, string fileName)
    {
        Texture2D readable = copySpriteToReadable(sprite);
        if (readable == null)
        {
            Debug.LogError("Sprite导出失败:" + sprite.name);
            return;
        }
        string path = Path.Combine(folder, fileName + ".png");
        File.WriteAllBytes(path, readable.EncodeToPNG());
        DestroyImmediate(readable);
        Debug.Log("导出Sprite:" + path);
    }
    private void exportTextAsset(TextAsset textAsset, string folder, string fileName)
    {
        string path = Path.Combine(folder, fileName + ".bytes");
        File.WriteAllBytes(path, textAsset.bytes);
        Debug.Log("导出TextAsset:" + path);
    }
    private Texture2D copyTextureToReadable(Texture texture)
    {
        if (texture == null)
        {
            return null;
        }
        RenderTexture oldActive = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(texture, renderTexture);
        RenderTexture.active = renderTexture;
        Texture2D readable = new(texture.width, texture.height, TextureFormat.RGBA32, false);
        readable.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        readable.Apply();
        RenderTexture.active = oldActive;
        RenderTexture.ReleaseTemporary(renderTexture);
        return readable;
    }
    private Texture2D copySpriteToReadable(Sprite sprite)
    {
        if (sprite == null || sprite.texture == null)
        {
            return null;
        }
        Rect rect = sprite.textureRect;
        RenderTexture oldActive = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(sprite.texture.width, sprite.texture.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(sprite.texture, renderTexture);
        RenderTexture.active = renderTexture;
        Texture2D readable = new((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
        readable.ReadPixels(rect, 0, 0);
        readable.Apply();
        RenderTexture.active = oldActive;
        RenderTexture.ReleaseTemporary(renderTexture);
        return readable;
    }
    private string getSafeFileName(string assetName)
    {
        string fileName = assetName.Replace("\\", "/");
        int index = fileName.LastIndexOf('/');
        if (index >= 0)
        {
            fileName = fileName[(index + 1)..];
        }
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }
        if (fileName.isEmpty())
        {
            fileName = "asset";
        }
        return fileName;
    }
}