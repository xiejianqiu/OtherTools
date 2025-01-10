using Language;
using UnityEngine;
public class LangMono: MonoBehaviour
{
    public string assetPath;
    public bool IsResetTxt = true;
    void Awake()
    {
        if (!IsResetTxt)
            return;
        ResetLblTxt();
    }
    public void ResetLblTxt()
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            if (EnvUtils.IsUnity_Editor())
            {
                Debug.Log($"<color=red>{this.gameObject.name} 该prefab未缓存资源路径，多语言版本无法翻译</color>");
            }
            return;
        }
        PrefabLangInfoMgr.Inst.InitPrefabText(this.gameObject, this.assetPath);
    }
#if UNITY_EDITOR
    [ContextMenu("SaveAssetPath")]
    public bool SaveAssetPath()
    {
        bool IsChange = false;
        var newAssetPath = UnityEditor.AssetDatabase.GetAssetPath(this.gameObject);
        if (string.IsNullOrEmpty(assetPath) || assetPath != newAssetPath)
        {
            assetPath = newAssetPath;
            IsChange = true;
        }
        return IsChange;
    }
    [ContextMenu("Refresh")]
    public void Refresh()
    {
        UnityEditor.AssetDatabase.Refresh();
    }
#endif
}
