using UnityEngine;
using UnityEditor;

public class CreateMaJiaAssets : EditorWindow
{
    private string m_strAddColor;

    [MenuItem("Tools/ResetTextureColor")]
    static public void OpenSpriteFinderWindow()
    {
        EditorWindow.GetWindow<CreateMaJiaAssets>(false, "CreateMaJiaAssets", true).Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Change Color : ");
        m_strAddColor = EditorGUILayout.TextField(m_strAddColor);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("ResetTextureColor"))
        {
            int color = -1;
            if (int.TryParse(m_strAddColor, out color))
            {
                ResetTextureColor(color);
            }
        }
    }

    public static void ResetTextureColor(int color)
    {
        string[] path = AssetDatabase.GetAllAssetPaths();
        for (int i = 0; i < path.Length; ++i)
        {
            if (!path[i].ToLower().EndsWith(".png"))
            {
                continue;
            }

            if (path[i].ToLower().EndsWith("_n.png"))
            {
                continue;
            }

            if (path[i].ToLower().EndsWith("_c.png"))
            {
                continue;
            }

            if (path[i].ToLower().EndsWith("_s.png"))
            {
                continue;
            }

            if (path[i].ToLower().EndsWith("_alpha.png"))
            {
                continue;
            }

            TextureImporter textureImporter = AssetImporter.GetAtPath(path[i]) as TextureImporter;
            textureImporter.isReadable = true;
            AssetDatabase.ImportAsset(path[i]);

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path[i]);
            if (texture == null)
            {
                continue;
            }

            Debug.LogError(path[i]);

            Texture2D newTex = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            Color[] colors = texture.GetPixels();
            for (int m = 0; m < colors.Length; ++m)
            {
                colors[m].r += color / 255.0f;
                colors[m].g += color / 255.0f;
                colors[m].b += color / 255.0f;
            }
            newTex.SetPixels(colors);
            newTex.Apply();
            byte[] bytes = newTex.EncodeToPNG();
            System.IO.File.WriteAllBytes(Application.dataPath.Replace("\\", "/").Replace("Assets", "") + path[i], bytes);

            textureImporter = AssetImporter.GetAtPath(path[i]) as TextureImporter;
            textureImporter.isReadable = false;
            AssetDatabase.ImportAsset(path[i]);
        }
        // Load the texture we just saved as a Texture2D
        AssetDatabase.SaveAssets();
    }
}