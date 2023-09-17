using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Language
{
    /// <summary>
    /// 用于提取PrefabLangInfo.txt中的中文
    /// </summary>
    public class GrabJsonChinese
    {
        /// <summary>
        /// 将提取json文件中的中文
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        static public HashSet<string> GrabChinese(string filePath)
        {
            HashSet<string> cnSet = new HashSet<string>();
            if (File.Exists(filePath))
            {
                var utfEndoding = new UTF8Encoding(false);
                var content = File.ReadAllText(filePath, utfEndoding);
                var prefabInfos = JsonMapper.ToObject<PrefabLangInfo[]>(content);
                foreach (var info in prefabInfos) 
                {
                    foreach (var lblInfo in info.lblLst)
                    {
                        cnSet.Add(lblInfo.text);
                    }
                }
            }
            else
            {
                Console.WriteLine($"{filePath}  不存在");
            }
            return cnSet;
        }
        /// <summary>
        /// 翻译Prefab中的文本
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="lgInfo"></param>
        /// <param name="lang"></param>
        /// <returns></returns>

        static public void FanYiChinese(string filePath, Dictionary<string, LgInfo> lgInfo, LANGUAGE lang)
        {
            HashSet<string> cnSet = new HashSet<string>();
            if (File.Exists(filePath))
            {
                var utfEndoding = new UTF8Encoding(false);
                var content = File.ReadAllText(filePath, utfEndoding);
                var prefabInfos = JsonMapper.ToObject<PrefabLangInfo[]>(content);
                foreach (var info in prefabInfos)
                {
                    foreach (var lblInfo in info.lblLst)
                    {
                        if (lgInfo.TryGetValue(lblInfo.text, out var tmpInfo))
                        {
                            var dstTxt = tmpInfo.GetLang(lang);
                            if (!string.IsNullOrEmpty(dstTxt))
                            {
                                lblInfo.text = tmpInfo.GetLang(lang);
                            }
                        }
                    }
                }
                List<PrefabLangInfo> infoLst = new List<PrefabLangInfo>();
                infoLst.AddRange(prefabInfos);
                var json = JsonMapper.ToJson(infoLst);
                File.WriteAllText(filePath, json, utfEndoding);
            }
            else
            {
                Console.WriteLine($"{filePath}  不存在");
            }
        }
    }
}
