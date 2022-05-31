using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System;

public class UIEditor : Editor
{
    [MenuItem("UIEditor/SetAllChildrenPanelSortingLayer")]
    public static void SetAllChildrenPanelToParent()
    {
        GameObject go = Selection.activeGameObject;
        UIPanel panel = go.GetComponent<UIPanel>();
        if (panel != null)
        {
            SetSortingName(go, panel.sortingLayerName);
        }
    }

    public static void SetSortingName(GameObject go, string strSortingName)
    {
        UIPanel uipanel = go.GetComponent<UIPanel>();
        if (uipanel != null)
        {
            uipanel.sortingLayerName = strSortingName;
        }

        for (int i = 0; i < go.transform.childCount; ++i)
        {
            SetSortingName(go.transform.GetChild(i).gameObject, strSortingName);
        }
    }

    [MenuItem("UIEditor/SetAllChildrenPanelDepth")]
    public static void SetAllChildrenPanelDepth()
    {
        GameObject go = Selection.activeGameObject;
        UIPanel panel = go.GetComponent<UIPanel>();
        if (panel != null)
        {
            SetPanelDepth(go, panel.depth);
        }
    }

    public static void SetPanelDepth(GameObject go, int nDepth)
    {
        UIPanel uipanel = go.GetComponent<UIPanel>();
        if (uipanel != null)
        {
            uipanel.depth -= nDepth;
        }

        for (int i = 0; i < go.transform.childCount; ++i)
        {
            SetPanelDepth(go.transform.GetChild(i).gameObject, nDepth);
        }
    }

    [MenuItem("UIEditor/ResetAllChildrenPanel")]
    public static void ResetAllChildrenPanel()
    {
        GameObject go = Selection.activeGameObject;
        UIPanel panel = go.GetComponent<UIPanel>();
        if (panel != null)
        {
            SetPanelDepth(go, panel.depth);
            SetSortingName(go, "Default");
        }
    }
    
    [MenuItem("UIEditor/ResetUIButton")]
    public static void ResetUIButton()
    {
        NGUIMenu.AddCollider();


        GameObject[] list = Selection.gameObjects;
        for (int i = 0; i < list.Length; ++i)
        {
            GameObject go = list[i];

            UIButton btn = go.GetComponent<UIButton>();
            if (btn == null)
            {
                btn = go.AddComponent<UIButton>();
            }

            if (btn != null)
            {
                btn.defaultColor = Color.white;
                btn.pressed = Color.white;
                btn.hover = Color.white;
                btn.disabledColor = Color.white;
                btn.ResetDefaultColor();
            }

            UIButtonScale scale = go.GetComponent<UIButtonScale>();
            if (scale == null)
            {
                scale = go.AddComponent<UIButtonScale>();
            }
            if (scale != null)
            {
                scale.hover = Vector3.one;
                //scale.pressed = new Vector3(0.9f, 0.9f, 0.9f);
            }

            //UIPlaySound sound = go.GetComponent<UIPlaySound>();
            //if (sound == null)
            //{
            //    go.AddComponent<UIPlaySound>();
            //}
        }
    }

    [MenuItem("UIEditor/RemoveProUIButton")]
    public static void RemoveProUIButton()
    {
        GameObject[] list = Selection.gameObjects;

        for (int i = 0; i < list.Length; ++i)
        {
            ChangeProUIButton(list[i]);
        }
    }

    private static void ChangeProUIButton(GameObject go)
    {
        Transform t = go.transform;

        ProUIButton button = go.GetComponent<ProUIButton>();
        if (button != null)
        {
            List<EventDelegate> list = button.onClick;
            GameObject target = button.tweenTarget;
            string normal = button.normalSprite;
            string hover = button.hoverSprite;
            string pressed = button.pressedSprite;
            string disable = button.disabledSprite;

            DestroyImmediate(button);

            UIButton btn = go.GetComponent<UIButton>();
            if (btn == null)
            {
                btn = go.AddComponent<UIButton>();
            }
            btn.defaultColor = Color.white;
            btn.pressed = Color.white;
            btn.hover = Color.white;
            btn.disabledColor = Color.white;
            btn.ResetDefaultColor();

            btn.tweenTarget = target;
            btn.normalSprite = normal;
            btn.hoverSprite = hover;
            btn.pressedSprite = pressed;
            btn.disabledSprite = disable;

            btn.onClick.AddRange(list);
        }

        for (int i = 0; i < t.childCount; ++i)
        {
            ChangeProUIButton(t.GetChild(i).gameObject);
        }
    }

    [MenuItem("Terrain/ResetTerrainScale")]
    public static void ResetTerrainScale()
    {
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        ResetTerrainScaleChild(Selection.activeGameObject);
        //EditorSceneManager.SaveOpenScenes();
    }

    public static void ResetTerrainScaleChild(GameObject go)
    {
        Transform t = go.transform;
        t.localScale = new Vector3(Mathf.Abs(t.localScale.x), Mathf.Abs(t.localScale.y), Mathf.Abs(t.localScale.z));
        for (int i = 0; i < t.childCount; ++i)
        {
            ResetTerrainScaleChild(t.GetChild(i).gameObject);
        }
    }

    [MenuItem("UIEditor/ResetSelectIconSpriteWidth")]
    public static void ResetSelectIconSpriteWidth()
    {
        UISprite[] list = Selection.activeGameObject.GetComponentsInChildren<UISprite>(true);
        for (int i = 0; i < list.Length; ++i)
        {
            UISprite sp = list[i];
            if (sp.spriteName == "common_border_7")
            {
                sp.width = sp.GetAtlasSprite().width;
                sp.height = sp.GetAtlasSprite().height;
            }
        }
    }

    [MenuItem("UIEditor/ResetRedPoint")]
    public static void ResetRedPoint()
    {
        GameObject[] list = Selection.gameObjects;
        for (int i = 0; i < list.Length; i++)
        {
            GameObject go = list[i];
            //TweenAlpha vTweenScale = go.GetComponent<TweenAlpha>();
            ////如果需要换曲线 打印曲线 改一下最下边的代码
            //for (int j = 0; j < vTweenScale.animationCurve.keys.Length; j++)
            //{
            //    Keyframe vKeyframe = vTweenScale.animationCurve.keys[j];
            //    string str = string.Format("new Keyframe({0},{1},{2},{3})", vKeyframe.time, vKeyframe.value, vKeyframe.inTangent,
            //          vKeyframe.outTangent);
            //    Debug.LogError(str);

            //}

            TweenScale vTweenScale = go.GetComponent<TweenScale>();
            //如果需要换曲线 打印曲线 改一下最下边的代码
            //for (int j = 0; j < vTweenScale.animationCurve.keys.Length; j++)
            //{
            //    Keyframe vKeyframe = vTweenScale.animationCurve.keys[j];
            //    string str = string.Format("new Keyframe({0},{1},{2},{3})", vKeyframe.time, vKeyframe.value, vKeyframe.inTangent,
            //          vKeyframe.outTangent);
            //    Debug.LogError(str);

            //}

            if (vTweenScale == null)
            {
                vTweenScale = go.AddComponent<TweenScale>();
            }
            if (vTweenScale)
            {
                vTweenScale.from = Vector3.one;
                vTweenScale.to = Vector3.one * 0.8f;
                vTweenScale.updateTable = false;
                vTweenScale.duration = 3;
                vTweenScale.style = UITweener.Style.Loop;
                vTweenScale.delay = 0;
                vTweenScale.ignoreTimeScale = true;
                vTweenScale.animationCurve = new AnimationCurve(
                    new Keyframe(0, 0, 0, 0),
                    new Keyframe(0.0575f, 1, 1.000017f, 1.000017f),
                    new Keyframe(0.115f, 0, 0.3916025f, 0.3916025f),
                    new Keyframe(0.1725f, 1, 0.7933006f, 0.7933006f),
                    new Keyframe(0.23f, 0, -0.0974769f, 1 / 0.0f),
                    new Keyframe(1.047363f, 0.0375061f, 1, 1));
            }
        }
    }

    [MenuItem("UIEditor/ResetArrowTweenAlpha")]
    public static void ResetArrowTweenAlpha()
    {
        GameObject[] list = Selection.gameObjects;
        for (int i = 0; i < list.Length; i++)
        {
            GameObject go = list[i];
            TweenAlpha vTweenAlpha = go.GetComponent<TweenAlpha>();
            ////如果需要换曲线 打印曲线 改一下最下边的代码
            //for (int j = 0; j < vTweenAlpha.animationCurve.keys.Length; j++)
            //{
            //    Keyframe vKeyframe = vTweenAlpha.animationCurve.keys[j];
            //    string str = string.Format("new Keyframe({0},{1},{2},{3})", vKeyframe.time, vKeyframe.value, vKeyframe.inTangent,
            //          vKeyframe.outTangent);
            //    Debug.LogError(str);

            //}

            if (vTweenAlpha == null)
            {
                vTweenAlpha = go.AddComponent<TweenAlpha>();
            }
            if (vTweenAlpha)
            {
                vTweenAlpha.from = 1;
                vTweenAlpha.to = 0;
                //vTweenAlpha.updateTable = false;
                vTweenAlpha.duration = 1.5f;
                vTweenAlpha.style = UITweener.Style.Loop;
                vTweenAlpha.delay = 0;
                vTweenAlpha.ignoreTimeScale = true;
                vTweenAlpha.animationCurve = new AnimationCurve(
                    new Keyframe(0, 0, 0, 0),
                    new Keyframe(0.5f, 0.25f, 0, 0),
                    new Keyframe(1, 0, 0, 0));
            }
        }
    }
    
    [MenuItem("UIEditor/RestRightTabBtns")]
    public static void RestRightTabBtns()
    {
        GameObject go = Selection.activeGameObject;
        go.transform.localPosition = new Vector3(442, 188, 0);

        UIPanel vPanel = go.GetComponent<UIPanel>();
        if (vPanel == null)
        {
            vPanel = go.AddComponent<UIPanel>();
        }
        vPanel.clipping = UIDrawCall.Clipping.SoftClip;
        vPanel.SetRect(34, -205, 60, 512);
        vPanel.clipSoftness = new Vector2(0, 0);
        vPanel.sortingOrder = 5;

        for (int i = 0; i < go.transform.childCount; i++)
        {
            Transform t = go.transform.GetChild(i);
            t.localPosition = new Vector3(-60, -82*i, 0);

            TweenPosition vTweenPosition = t.GetComponent<TweenPosition>();
            if (vTweenPosition == null)
            {
                vTweenPosition = t.gameObject.AddComponent<TweenPosition>();
            }
            if (vTweenPosition)
            {
                vTweenPosition.from = t.localPosition;
                vTweenPosition.to = new Vector3(0, t.localPosition.y, 0);
                vTweenPosition.duration = 0.1f;
                vTweenPosition.style = UITweener.Style.Once;
                vTweenPosition.delay = 0.2f + 0.02f * i;
                vTweenPosition.ignoreTimeScale = true;
                vTweenPosition.animationCurve = new AnimationCurve(
                    new Keyframe(0, 0, 0, 1),
                    new Keyframe(1, 1, 1, 0));
            }
        }
    }

    [MenuItem("UIEditor/ResetItemOnClickEffect")]
    public static void ResetItemOnClickEffect()
    {
        GameObject go = Selection.activeGameObject;
        UIButtonScale scale = go.GetComponent<UIButtonScale>();
        if (scale == null)
        {
            scale = go.AddComponent<UIButtonScale>();
        }
        if (scale != null)
        {
            scale.hover = Vector3.one;
            //scale.pressed = new Vector3(0.9f, 0.9f, 0.9f);
        }
    }


    [MenuItem("UIEditor/ResetUITexture")]
    public static void ResetUITexture()
    {
        var textures = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
        for (int i = 0; i < textures.Length; ++i)
        {
            string path = AssetDatabase.GetAssetPath(textures[i]);
            TextureImporter texImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (texImporter != null && texImporter.textureType == TextureImporterType.Default)
            {
                TextureImporterPlatformSettings setting = texImporter.GetPlatformTextureSettings("Android");
                if (setting.overridden)
                {
                    continue;
                }
                bool isETC2 = textures[i].name.EndsWith("_alpha") || textures[i].name.EndsWith("_color");
                setting.name = "Android";
                setting.overridden = true;
                setting.maxTextureSize = 2048;
                setting.format = isETC2 ? TextureImporterFormat.ETC2_RGB4 : TextureImporterFormat.ETC2_RGBA8;
                setting.compressionQuality = (int)UnityEditor.TextureCompressionQuality.Normal;
                texImporter.SetPlatformTextureSettings(setting);
                AssetDatabase.ImportAsset(path);
            }
        }
    }

    [MenuItem("UIEditor/ResetItemIconSize")]
    public static void ResetItemIconSize()
    {
        GameObject go = Selection.activeGameObject;
        ResetIconSize(go.transform);
    }

    private static void ResetIconSize(Transform t)
    {
        if (t.name == "ItemIcon")
        {
            UISprite sp = t.GetComponent<UISprite>();
            if (sp != null)
            {
                sp.width = 64;
                sp.height = 64;
            }
        }

        for (int i = 0; i < t.childCount; ++i)
        {
            ResetIconSize(t.GetChild(i));
        }
    }
}
