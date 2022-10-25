using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using UnityEngine;
using System.Text;

public class GfxUtils
{
    public static Vector3 GetAdapterScale()
    {
        Vector3 scale = Vector3.one;
        float a = (float)Screen.width / Screen.height;
        a = Mathf.Min(a, 21f / 9f);
        float b = 16f / 9f;
        if (a > b)
        {
            scale.x = a / b;
        }
        else
        {
            scale.y = b / a;
        }
        return scale;
    }
    public static float GetRotateAngle(float x1, float y1, float x2, float y2)
    {
        const float epsilon = 1.0e-6f;
        float nyPI = (float)Math.Acos(-1.0);
        float dist, dot, angle;

        // normalize
        dist = (float)Math.Sqrt(x1 * x1 + y1 * y1);
        x1 /= dist;
        y1 /= dist;
        dist = (float)Math.Sqrt(x2 * x2 + y2 * y2);
        x2 /= dist;
        y2 /= dist;
        // dot product
        dot = x1 * x2 + y1 * y2;
        if (Math.Abs(dot - 1.0) <= epsilon)
            angle = 0.0f;
        else if (Math.Abs(dot + 1.0) <= epsilon)
            angle = nyPI;
        else
        {
            float cross;

            angle = (float)Math.Acos(dot);
            //cross product
            cross = x1 * y2 - x2 * y1;
            // vector p2 is clockwise from vector p1 
            // with respect to the origin (0.0)
            if (cross < 0)
            {
                angle = 2 * nyPI - angle;
            }
        }
        //degree = angle *  180.0 / nyPI;
        return angle;
    }
    public static HttpWebRequest CreatePostHttpRequest(string url, IDictionary<string, string> httpHeaders, IDictionary<string, string> parameters, int timeout)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentNullException("url");
        }
        HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
        request.Method = "POST";
        request.ContentType = "application/x-www-form-urlencoded";
        request.Timeout = timeout;
        request.ReadWriteTimeout = timeout;

        request.SendChunked = false;
        request.TransferEncoding = null;

        request.KeepAlive = false;

        if (!(httpHeaders == null || httpHeaders.Count == 0))
        {
            foreach (var item in httpHeaders)
            {
                request.Headers.Set(item.Key, item.Value);
            }
        }

        if (!(parameters == null || parameters.Count == 0))
        {
            StringBuilder buffer = new StringBuilder();
            int i = 0;
            foreach (string key in parameters.Keys)
            {
                if (i > 0)
                {
                    buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                }
                else
                {
                    buffer.AppendFormat("{0}={1}", key, parameters[key]);
                }
                i++;
            }
            byte[] data = Encoding.UTF8.GetBytes(buffer.ToString());
            //LogSystem.Login.Debug("url={0}", url);
            //LogSystem.Login.Debug("buffer={0}", buffer.ToString());
            request.ContentLength = data.Length;
            //LogSystem.Login.Debug("CreatePostHttpRequest data{0}, dataLength={1}", data, data.Length);

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }
            //LogSystem.Login.Debug("CreatePostHttpRequest after stream.Write");
        }
        return request;
    }
    
    public static bool IsIphoneX()
    {
        bool bRet = false;

#if UNITY_IOS
        if (Screen.width == 2688 && Screen.height == 1242)
        {
            bRet = true;
        }
        if (Screen.width == 2436 && Screen.height == 1125)
        {
            bRet = true;
        }
        if (Screen.width == 1792 && Screen.height == 828)
        {
            bRet = true;
        }
        if (Screen.width == 2778 && Screen.height == 1284)
        {
            bRet = true;
        }
        if (Screen.width == 2352 && Screen.height == 1170)
        {
            bRet = true;
        }
        if (Screen.width == 2340 && Screen.height == 1080)
        {
            bRet = true;
        }
#endif
        return bRet;
    }
    public static Color getColorByString(string str)
    {
        if (str.Length == 10)
        {
            str = str.Remove(0, 1).Remove(8, 1);
        }

        if (str.Length != 8)
        {
            return Color.white;
        }

        int r = 0, g = 0, b = 0, a = 0;
        int num = 0;
        char c;
        for (int i = 0; i < str.Length; ++i)
        {
            c = str[i];
            if (c >= 'a' && c <= 'f')
            {
                c = (char)(c - 32);
            }
            if (c >= '0' && c <= '9')
            {
                num = num * 16 + (c - '0');
            }
            else if (c >= 'A' && c <= 'F')
            {
                num = num * 16 + (c - 'A' + 10);
            }
            else
            {
                num = 0;
            }

            switch (i)
            {
                case 1: r = num; num = 0; break;
                case 3: g = num; num = 0; break;
                case 5: b = num; num = 0; break;
                case 7: a = num; num = 0; break;
            }
        }

        return new Color((float)r / 255f, (float)g / 255f, (float)b / 255f, (float)a / 255f);
    }
    public static string GetMD5Hash(string pathName)
    {
        string result = string.Empty;
        string text = string.Empty;
        MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
        try
        {
            FileStream fileStream = new FileStream(pathName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            byte[] value = mD5CryptoServiceProvider.ComputeHash(fileStream);
            fileStream.Close();
            text = BitConverter.ToString(value);
            text = text.Replace("-", string.Empty);
            result = text;
        }
        catch
        {
            
        }
        return result;
    }
    public static void CheckTargetPath(string targetPath)
    {
        targetPath = targetPath.Replace('\\', '/');
        int num = targetPath.LastIndexOf('.');
        int num2 = targetPath.LastIndexOf('/');
        if (num > 0 && num2 < num)
        {
            targetPath = targetPath.Substring(0, num2);
        }
        if (Directory.Exists(targetPath))
        {
            return;
        }
        string[] array = targetPath.Split(new char[]
        {
            '/'
        });
        string text = string.Empty;
        int num3 = array.Length;
        for (int i = 0; i < num3; i++)
        {
            text = text + array[i] + '/';
            if (!Directory.Exists(text))
            {
                Directory.CreateDirectory(text);
            }
        }
    }
    public static void DeleteFolder(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }
        
        string[] array = Directory.GetFiles(path);
        string[] array2 = array;
        for (int i = 0; i < array2.Length; i++)
        {
            string path2 = array2[i];
            File.Delete(path2);
        }
        array = Directory.GetDirectories(path);
        string[] array3 = array;
        for (int j = 0; j < array3.Length; j++)
        {
            string path3 = array3[j];
            DeleteFolder(path3);
        }
        Directory.Delete(path);
    }
    public static string GetFileString(string path)
    {
        string strRet = "";

        try
        {
            if (File.Exists(path))
            {
                FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                StreamReader streamReader = new StreamReader(fileStream);
                strRet = streamReader.ReadToEnd();
                streamReader.Close();
                fileStream.Close();
            }
        }
        catch
        {
            
        }
        return strRet;
    }
    public static UnityEngine.Object LoadResource(string resPath, Type systemTypeInstance = null)
    {
        UnityEngine.Object @object;
        if (systemTypeInstance == null)
        {
            @object = Resources.Load(resPath);
        }
        else
        {
            @object = Resources.Load(resPath, systemTypeInstance);
        }
        return @object;
    }
    public static bool IsHitVisibal(Vector3 worldPoint, GameObject go)
    {
        
        UIWidget widght = go.GetComponent<UIWidget>();
        if (widght == null)
        {
            return true;
        }

        if (!widght.isVisible)
        {
            return false;
        }

        UIPanel panel = NGUITools.FindInParents<UIPanel>(go);

        while (panel != null)
        {
            if (!panel.IsVisible(worldPoint)) return false;
            panel = panel.parentPanel;
        }
        return true;
    }
    public static Vector3 GetNavSamplePosition(Vector3 pos)
    {
        UnityEngine.AI.NavMeshHit navMeshHit;
        if (UnityEngine.AI.NavMesh.SamplePosition(pos, out navMeshHit, 200f, -1))
        {
            return navMeshHit.position;
        }
        return pos;
    }
    public static void GetComponentInChilds<T>(Transform transform, List<T> list) where T : Component
    {
        T component = null;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            component = child.GetComponent<T>();
            if (component)
            {
                list.Add(component);
            }
            GetComponentInChilds<T>(child, list);
        }
    }
    public static void DltAllFiles(string path)
    {
        if(!Directory.Exists(path))
        {
            return;
        }
        string[] files = Directory.GetFiles(path);
        for(int i = 0; i < files.Length;i++)
        {
            File.Delete(files[i]);
        }
    }
    public static float DirClientToServer(Quaternion rotate)
    {
        return 1.57079637f - rotate.eulerAngles.y * 3.14159274f / 180f;
    }
    public static Quaternion DirServerToClient(float rad)
    {
        return Quaternion.Euler(0f, 90f - rad * 180f / 3.14159274f, 0f);
    }
    public static float GetNavSampleHeight(Vector3 pos)
    {
        UnityEngine.AI.NavMeshHit navMeshHit;
        if (UnityEngine.AI.NavMesh.SamplePosition(pos, out navMeshHit, 200f, -1))
        {
            return navMeshHit.position.y;
        }
        return pos.y;
    }
    public static void SetScrollViewOffset(UIScrollView sc, float offsetY, bool isNeedResetPosition = true, float offsetX=0)
    {
        if (sc == null)
        {
            return;
        }

        if (isNeedResetPosition)
        {
            sc.ResetPosition();
        }

        UIPanel panel = sc.GetComponent<UIPanel>();
        if (panel == null)
        {
            return;
        }

        panel.clipOffset += new Vector2(-offsetX, -offsetY);
        panel.transform.localPosition += new Vector3(offsetX, offsetY, 0);
    }
    public static string BinToHex(byte[] bytes)
    {
        return BinToHex(bytes, 0);
    }
    public static string BinToHex(byte[] bytes, int start)
    {
        return BinToHex(bytes, start, bytes.Length - start);
    }
    public static string BinToHex(byte[] bytes, int start, int count)
    {
        if (start < 0 || count <= 0 || start + count > bytes.Length)
            return "";
        StringBuilder sb = new StringBuilder(count * 4);
        for (int ix = 0; ix < count; ++ix) {
            sb.AppendFormat("{0,2:X2}", bytes[ix+start]);
            if ((ix + 1) % 16 == 0)
                sb.AppendLine();
            else
                sb.Append(' ');
        }
        return sb.ToString();
    }
    
}
