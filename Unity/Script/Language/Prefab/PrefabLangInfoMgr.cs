using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static NGUIText;
using static UILabel;
using static UIWidget;

namespace Language
{
    /// <summary>
    /// 负责多语言预制物体文本替换
    /// </summary>
    public class PrefabLangInfoMgr:IDisposable
    {
        private static PrefabLangInfoMgr _Inst;
        public static PrefabLangInfoMgr Inst {
            get {
                if (null == _Inst)
                {
                    _Inst = new PrefabLangInfoMgr();
                }
                return _Inst;
            }
        }
        public bool IsLoad { get; set; } = false;
        private Dictionary<string,PrefabLangInfo> mPrefabInfoDict;
        /// <summary>
        /// 加载多语言prefab信息
        /// </summary>
        public void LoadPrefabLangInfo()
        {
            if (EnvUtils.IsUnity_Editor() && !EnvUtils.IsEDITOR_ABMOD())
            {
                var textStr = ReadTxtTable(LanguageMgr.Inst.GetPrefabLangPath());
                Load(textStr);
            }
            else
            {
                TablesLoadTask._actExecute = OnLoadTable;
                GfxLogicBrige.Instance.ABManager.AddLoadTask(new TablesLoadTask(LanguageMgr.Inst.GetDataTbABPath()));
            }
        }
        public void Load(string content)
        {
            try
            {
                mPrefabInfoDict = new Dictionary<string, PrefabLangInfo>();
                var prefabInfos = JsonMapper.ToObject<PrefabLangInfo[]>(content);
                foreach (var info in prefabInfos)
                {
                    var key = info.assetPath;
                    if (mPrefabInfoDict.ContainsKey(key))
                    {
                        Debug.LogError($"<color=green>{info.assetPath} Prefab名称重复了</color>");
                        continue;
                    }
                    mPrefabInfoDict.Add(key, info);
                }
                IsLoad = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"PrefabLangInfoMgr.Load {e}");
            }
        }
        private void OnLoadTable(TablesLoadTask vTask)
        {
            var textStr = vTask.ReadTxtTable(LanguageMgr.Inst.GetPrefabLangPath());
            Load(textStr);
            GfxLogicBrige.Instance.ABManager.RemoveLoader(TablesLoadTask.m_abLoader, true);
            TablesLoadTask.m_abLoader = null;
            TablesLoadTask._actExecute = null;
        }
        private string ReadTxtTable(string strPath)
        {
            string localPath = Application.dataPath + "/BundleData/" + strPath;
            string strData = null;
            if (File.Exists(localPath))
            {
                StreamReader sr = null;
                sr = File.OpenText(localPath);
                strData = sr.ReadToEnd();
                sr.Close();
            }
            return strData;
        }
        /// <summary>
        /// 修改prefab中文
        /// </summary>
        /// <param name="Go"></param>
        /// <param name="path">相对于BundleData的路径</param>
        public void InitPrefabText(GameObject Go, string path)
        {
            if (null == Go)
            {
                Debug.LogError($"PrefabLangInfoMgr.InitPrefabText Go Is Null, {path}");
                return;
            }
            var tran = Go.transform;
            if (null != mPrefabInfoDict && mPrefabInfoDict.TryGetValue(path, out var info))
            {
                foreach (var lblInfo in info.lblLst)
                {
                    var lblTran = tran.Find(lblInfo.relativePath);
                    if (null != lblTran)
                    {
                        var label = lblTran.GetComponent<UILabel>();
                        if (null != label)
                        {
                            label.text = lblInfo.text;
                            //label.mTextID = lblInfo.mTextID;
                            ////label.fontSize = lblInfo.fontSize;
                            //label.depth = lblInfo.depth;
                            //label.supportEncoding = lblInfo.supportEncoding;
                            //if (Enum.TryParse<Alignment>(lblInfo.alignment, out var alignment))
                            //{
                            //    label.alignment = alignment;
                            //}
                            //if (Enum.TryParse<Overflow>(lblInfo.alignment, out var overflowMethod))
                            //{
                            //    label.overflowMethod = overflowMethod;
                            //}
                            //if (Enum.TryParse<Pivot>(lblInfo.alignment, out var pivot))
                            //{
                            //    label.pivot = pivot;
                            //}
                        }
                    }
                }
            }
        }
        public void Dispose()
        {
            this.mPrefabInfoDict.Clear();
        }
    }
}
