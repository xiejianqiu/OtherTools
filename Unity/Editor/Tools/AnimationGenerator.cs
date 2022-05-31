using UnityEditor;
using UnityEngine;
using System.IO;

public class AnimationGenerator : Editor {

    [MenuItem("Tools/Gen Animation")]
    static void GenerateAnimation()
    {        
        Object[] SelectionAsset = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        Debug.Log(SelectionAsset.Length);
        foreach (Object obj in SelectionAsset)
        {
            GameObject go = obj as GameObject;
            if(go == null)
            {
                continue;
            }
            AnimationClip[] clips = AnimationUtility.GetAnimationClips(go);
            if (clips.Length == 0)
            {
                continue;
            }

            string strAnimPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(go));

            strAnimPath = strAnimPath.Replace("ResMS", "BundleData");

            GfxUtils.CheckTargetPath(strAnimPath);

            for(int i =0; i < clips.Length; i++)
            {
                AnimationClip newClip = new AnimationClip();
                EditorUtility.CopySerialized(clips[i], newClip);
                AssetDatabase.CreateAsset(newClip, strAnimPath+"/" + clips[i].name + ".anim");
            }
        }
        AssetDatabase.Refresh();
    }

}
