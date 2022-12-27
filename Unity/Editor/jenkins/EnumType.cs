using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NSBuildGame
{
    public enum PargmaType {
        Platform,//Android,iOS
        BRes,//构建打包所有资源
        IsDebug,//是否为debug包
        IsAAB,//是否为AAB包
        IsMin,//是否为小包
    }
    public class PargmaTypeCheck {
        private Dictionary<PargmaType, string> argsDict;

        public PargmaTypeCheck()
        {
            this.argsDict = new Dictionary<PargmaType, string>();
        }
        public void Init(string[] args) {
            for (int index = 1; index < args.Length; index++)
            {
                try
                {
                    Debug.Log($"============= {index}:{args[index]} ========");
                    if (Enum.TryParse<PargmaType>(args[index], out var val))
                    {
                        argsDict[val] = "";
                        if (index + 1 < args.Length)
                        {
                            if (int.TryParse(args[index + 1], out var iVal) || !Enum.TryParse<PargmaType>(args[index + 1], out var val2))
                            {
                                argsDict[val] = args[index + 1];
                                index += 1;
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            //=============打印参数信息========================
            StringBuilder logBuilder = new StringBuilder();
            var iter = argsDict.GetEnumerator();
            while (iter.MoveNext()) {
                logBuilder.AppendLine($"{iter.Current.Key} {iter.Current.Value}");
            }
            Debug.Log($"透传参数信息:{argsDict.Count}\n{logBuilder.ToString()}");
        }
        /// <summary>
        /// 该函数只对布尔型参数有效
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsTrue(PargmaType type) {

            return argsDict.ContainsKey(type) && "true" == argsDict[type];
        }
        /// <summary>
        /// 获取参数值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public string GetValue(PargmaType type) {
            if (argsDict.ContainsKey(type))
            {
                return argsDict[type];
            }
            return string.Empty;
        }
        public bool HasKey(PargmaType type) {
            return argsDict.ContainsKey(type);
        }
    }
}
