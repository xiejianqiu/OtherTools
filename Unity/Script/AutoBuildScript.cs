using Shark;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Text;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Threading;
using System.Linq;

/// <summary>
/// 从其他界面切换会unity界面时调用,自动编译Hotfix的代码(代码有改动时调用)
/// </summary>
[ExecuteInEditMode]
public class AutoBuildScript : MonoBehaviour
{
    //[SerializeField]
    private string default_msbuild = "C:/Program Files (x86)/Microsoft Visual Studio/2019/Enterprise/Common7/IDE";
    void Update()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
            return;
        CompilerHotfixDll1();
#endif
    }
    private void CompilerHotfixDll1()
    {
#if UNITY_EDITOR
        if (!CheckChangeFiles())
            return;
        if (Application.isPlaying)
            return;
        Debug.Log(DateTime.Now.ToString("[HH:mm:ss]") +"检测到热更代码有变化,正在编译.....");

        string msbuild = GetMSbuildPath();
        string path = Application.dataPath.Replace("\\", "/").Replace("Assets", "");
        string build = msbuild.Replace("\\", "/") + "/devenv.com";
        string slnPath = path + "hotfix/hotfix.sln";
        string outpath = path.Replace("shengwang", "") + "BuildLog";///hotfix_build_log.txt
        if (!Directory.Exists(outpath))
            Directory.CreateDirectory(outpath);
        outpath += "/hotfix_build_log.txt";
        string parStr = slnPath + " /rebuild  Release  " + " /out  " + outpath;
        Thread delay = new Thread(DelayBuild);
        delay.Start(new string[] { build , parStr, outpath });
#endif
    }
    private void DelayBuild(object obj)
    {
        Thread.Sleep(100);
        string[] para = (string[])obj;
        Process process = new Process();
        ProcessStartInfo info = new ProcessStartInfo(para[0], para[1]);
        info.UseShellExecute = false;
        info.RedirectStandardInput = false;
        info.RedirectStandardOutput = false;
        info.CreateNoWindow = true;
        process.StartInfo = info;
        process.Start();

        while(!process.HasExited)
            process.WaitForExit();

        int exitCode = process.ExitCode;
        process.Close();
        if (exitCode == 0)
            UnityEngine.Debug.Log(DateTime.Now.ToString("[HH:mm:ss]") + "编译完成!!");
        else
            UnityEngine.Debug.LogError(DateTime.Now.ToString("[HH:mm:ss]") +"ExitCode:"+ exitCode + " 编译失败!!!请检查热更代码或请查看日志:" + para[2]);
    }
    public bool CheckChangeFiles()
    {
        string oldTime = PlayerPrefs.GetString("AutoBuildCSFiles", "-1");
        if (!oldTime.Equals("-1"))
        {
            long now = DateTime.Now.Ticks / 10000000;
            long diff = now - Int64.Parse(oldTime);
            if (diff <= 5 && diff >= -5)
                return false;
            else
                PlayerPrefs.SetString("AutoBuildCSFiles", now.ToString());
        }
        else
            PlayerPrefs.SetString("AutoBuildCSFiles", (DateTime.Now.Ticks / 10000000).ToString());

        string path = Application.dataPath.Replace("\\", "/").Replace("Assets", "hotfix/hotfix/iLScript");
        if (!Directory.Exists(path))
            return false;


        if (!CheckBuildTools())
            return false;

        Dictionary<string, string> md5s = GetMD5ByCache();
        List<string> files = GetAllFilesByPath(path);
        string proto = "Client/ProtoMessage/ProtoMessage/ProtoMessage.cs";
        string protoCS = path.Replace("shengwang/hotfix/hotfix/iLScript", proto);
        bool isChangeProto = CheckProtoChange(proto, protoCS, md5s);
        if (isChangeProto)
        {
            Debug.Log(DateTime.Now.ToString("[HH:mm:ss]") + "检测到proto有变化,更新协议");
            ProtoEditor1.Proto2CSEditor.AllProto2CS();
        }

        return CompilreMd5(files, md5s, isChangeProto);
    }

    private List<string> GetAllFilesByPath(string path)
    {
        List<string> files = new List<string>();
        if (!Directory.Exists(path))
            return files;

        string[] allfiles = Directory.GetFiles(path, "*.cs");
        if (allfiles.Length > 0)
            files.AddRange(allfiles);

        string[] dirs = Directory.GetDirectories(path);
        if (dirs.Length == 0)
            return files;
        for (int i = 0; i < dirs.Length; i++)
            files.AddRange(GetAllFilesByPath(dirs[i]));
        return files;
    }
    private Dictionary<string, string> GetMD5ByCache()
    {
        Dictionary<string, string> md5s = new Dictionary<string, string>();
        string path = Application.dataPath.Replace("\\", "/").Replace("Assets", "hotfix/cache.md5");
        if (!File.Exists(path))
        {
            File.Create(path);
            return md5s;
        }

        using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
        {
            while (sr.Peek() != -1)
            {
                string[] line = sr.ReadLine().Trim().Split('\t');
                if (line.Length > 1)
                    md5s.Add(line[0], line[1]);
            }
            sr.Dispose();
            sr.Close();
        }
        return md5s;
    }

    private bool CompilreMd5(List<string> files, Dictionary<string, string> md5Dir,bool isChangeProto = false)
    {
        bool changed = false;
        string path = Application.dataPath.Replace("\\", "/").Replace("Assets", "hotfix/hotfix/iLScript/");
        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i].Replace("\\", "/");
            if (!File.Exists(file))
                continue;
            string md = GetMD5FromStream(file);
            file = file.Replace(path, "");
            if (md5Dir.ContainsKey(file))
            {
                if (!md.Equals(md5Dir[file]))
                {
                    md5Dir[file] = md;
                    changed = true;
                }
            }
            else
            {
                md5Dir.Add(file, md);
                changed = true;
            }
        }
        if ((changed || isChangeProto) && md5Dir.Count > 0)
        {
            string cache = Application.dataPath.Replace("\\", "/").Replace("Assets", "hotfix/cache.md5");
            using (StreamWriter sw = new StreamWriter(cache, false, Encoding.UTF8))
            {
                var e = md5Dir.GetEnumerator();
                while (e.MoveNext())
                {
                    sw.WriteLine(e.Current.Key + "\t" + e.Current.Value);
                }
                e.Dispose();

                sw.Flush();
                sw.Dispose();
                sw.Close();
            }
        }
        return changed;
    }

    public string GetMD5FromStream(string file)
    {
        FileStream fs = File.OpenRead(file);
        System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
        byte[] targetData = md5.ComputeHash(fs);
        StringBuilder strBuilder = new StringBuilder();
        for (int i = 0; i < targetData.Length; i++)
        {
            strBuilder.AppendFormat("{0:x2}", targetData[i]);
        }
        fs.Dispose();
        fs.Close();
        return strBuilder.ToString();
    }

    private string GetMSbuildPath()
    {
        string path = Application.dataPath.Replace("\\", "/").Replace("Assets", "");
        string config = path + "hotfix/msbuild.config";
        string msbuildpath = default_msbuild;
        if (File.Exists(config))
        {
            using(StreamReader sr = new StreamReader(config, Encoding.UTF8))
            {
                msbuildpath = sr.ReadToEnd().Replace("\n", "").Trim();
                sr.Dispose();
                sr.Close();
            }
        }
        else
        {
            using (StreamWriter sw = new StreamWriter(config,false, Encoding.UTF8))
            {
                sw.WriteLine(msbuildpath);
                sw.Flush();
                sw.Dispose();
                sw.Close();
            }
        }
        return msbuildpath;
    }
    private bool CheckBuildTools()
    {
        string msbuild = GetMSbuildPath();
        if (!Directory.Exists(msbuild))
        {
            Debug.Log(DateTime.Now.ToString("[HH:mm:ss]") +"自动编译检测失败\n未找到VisualStudio安装路径, 可在shengwang/hotfix/msbuild.config 文件中配置正确的安装路径,默认路径:\n" + default_msbuild);
            return false;
        }
        return true;
    }
    private bool CheckProtoChange(string key,string path,Dictionary<string,string> md5s)
    {
        bool isChangeProto = false;
        //检测协议 Client/ProtoMessage/ProtoMessage/ProtoMessage.cs
        if (File.Exists(path))
        {
            string md5 = GetMD5FromStream(path);
            if (md5s.ContainsKey(key))
            {
                if (!md5s[key].Equals(md5))
                {
                    isChangeProto = true;
                    md5s[key] = md5;
                }
            }
            else
            {
                isChangeProto = true;
                md5s.Add(key, md5);
            }
        }
        return isChangeProto;
    }
}

namespace ProtoEditor1
{
    class OpcodeInfo
    {
        public string Name;
        public int Opcode;
    }

    [Flags]
    public enum HeadFlag
    {
        None = 0,
        Bson = 1,
        Proto = 2,
    }

    public class Proto2CSEditor
    {
        private static string msgName = "";
        private static string protoPath = "../Tools/Message";  // "F:/game pro/project/Tools/Message";  UnityEngine.Application.dataPath + "/ScriptEx_HotFix/GenMsg"; //
        private static string clientMessagePath = "../shengwang/hotfix/hotfix/iLScript/GenMsg/"; // UnityEngine.Application.dataPath + "/ScriptEx_HotFix/GenMsg"; // "../Tools/Message/GenMsg";
        private static readonly char[] splitChars = { '=', ' ', '\t' };
        private static List<OpcodeInfo> msgOpcode = new List<OpcodeInfo>();
        private static MultiMap<string, string> parentMsg = new MultiMap<string, string>();
        private static Dictionary<string, string> publicMembers = new Dictionary<string, string>();
        static List<string> enumlist = new List<string>();
        static List<string> enumNameList = new List<string>();
        public static void AllProto2CS()
        {
            msgOpcode.Clear();
            string[] paths = { "SharkMsg.proto", "SharkDB.proto" };
            Proto2CS("Shark", paths, clientMessagePath, "OuterOpcode", 0, HeadFlag.Proto);
            GenerateOpcode("Shark", "OuterOpcode", clientMessagePath);
			#if UNITY_EDITOR
            AssetDatabase.Refresh();
			#endif
            UnityEngine.Debug.Log(" 生成protocs 结束");
        }

        //#region
        public static void Proto2CS(string ns, string[] paths, string outputPath, string opcodeClassName, int startOpcode, HeadFlag flag, bool isClient = true)
        {
            msgOpcode.Clear();
            enumlist.Clear();
            bool isexist = Directory.Exists(outputPath);
            if (!isexist)
                Directory.CreateDirectory(outputPath);
            parentMsg = new MultiMap<string, string>();
            string s = "";
            string proto = Path.Combine(protoPath, paths[0]);
            string csPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(proto) + ".cs");
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i];
                if (!string.IsNullOrEmpty(path))
                {
                    string proto11 = Path.Combine(protoPath, path);
                    string ss = File.ReadAllText(proto11) + "\n";
                    s += ss;
                }
            }
            StringBuilder sb = new StringBuilder();
            sb.Append("using ProtoBuf;\n");
            sb.Append("using System.Collections.Generic;\n");
            sb.Append(string.Format("namespace {0}\n", ns));
            sb.Append("{\n");

            bool isMsgStart = false;
            string parentClass = "";
            bool hasEnum = false;
            foreach (string line in s.Split('\n'))
            {
                string newline = line.Trim();
                if (newline == "")
                {
                    continue;
                }

                if (newline.StartsWith("//")) // 添加注释的
                {
                    sb.Append(string.Format("{0}\n", newline));
                }
                if (newline.StartsWith("message"))
                {
                    parentClass = "";
                    isMsgStart = true;
                    msgName = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)[1];

                    msgOpcode.Add(new OpcodeInfo() { Name = msgName, Opcode = ++startOpcode });

                    sb.Append(string.Format("\t[Message({0}.{1})]\n", opcodeClassName, msgName));

                    sb.Append("\t[ProtoContract]\n");
                    sb.Append(string.Format("\tpublic partial class {0}", msgName));

                    //清除成员
                    publicMembers.Clear();

                    if (parentClass != "")
                    {
                        sb.Append(string.Format(": {0}\n", parentClass));
                        //if (!newline.Contains("{"))
                        {
                            sb.Append("{");
                        }
                    }
                    else
                    {
                        sb.Append("{\n");
                    }
                }
                if (newline.Contains("enum"))
                {
                    hasEnum = true;
                    enumlist.Add(newline);
                    continue;
                }
                if (hasEnum)
                {
                    if (newline.Contains("}"))
                    {
                        hasEnum = false;
                    }
                    enumlist.Add(newline);
                    continue;
                }
                if (isMsgStart && newline.Contains("{"))
                {
                    if (!newline.Contains("{"))
                        sb.Append("\t{\n");

                    if (parentClass == "IRequest")
                    {
                        sb.Append("\t\t[ProtoMember(90, IsRequired = true)]\n");
                        sb.Append("\t\tpublic int RpcId { get; set; }\n");
                    }
                    if (parentClass == "IResponse")
                    {
                        sb.Append("\t\t[ProtoMember(90, IsRequired = true)]\n");
                        sb.Append("\t\tpublic int RpcId { get; set; }\n");
                        //sb.Append("\t\t[ProtoMember(91, IsRequired = true)]\n");
                        //sb.Append("\t\tpublic int Error { get; set; }\n");
                        //sb.Append("\t\t[ProtoMember(92, IsRequired = true)]\n");
                        //sb.Append("\t\tpublic string Message { get; set; }\n");
                    }
                    if (parentClass == "IMessage")
                    {
                        sb.Append("\t\t[ProtoMember(90, IsRequired = true)]\n");
                        sb.Append("\t\tpublic int RpcId { get; set; }\n");
                    }
                }

                bool hasMember = false;
                //// 成员
                if (newline.StartsWith("required"))
                {
                    hasMember = true;
                    //UnityEngine.Debug.LogError(" new line is " + newline);
                    Members(sb, newline, true);
                }

                if (newline.StartsWith("optional"))
                {
                    hasMember = true;
                    Members(sb, newline, false);
                }

                if (newline.StartsWith("repeated"))
                {
                    hasMember = true;
                    Repeated(sb, ns, newline, isClient);
                }
                if (isMsgStart && newline == "}")
                {
                    //if (!msgName.Equals(""))
                    //    DefalutConstructor(sb, msgName);
                    isMsgStart = false;
                    sb.Append("\t}\n\n");
                    msgName = "";
                }
            }
            // 处理enum的
            ProcessEnum(enumlist, sb);
            sb.Append("}\n");

            //if (!isClient)
            //{
            //GenerateHead(sb, ns, flag, opcodeClassName);
            //}

            File.WriteAllText(csPath, sb.ToString());
        }



        private static void GenerateHead(StringBuilder sb, string ns, HeadFlag flag, string opcodeClassName)
        {
            if ((flag & HeadFlag.Bson) != 0)
            {
                if (parentMsg.Count > 0)
                {
                    sb.AppendLine(string.Format("namespace {0}", ns));
                    sb.AppendLine("{");
                    foreach (string parentClass in parentMsg.GetDictionary().Keys)
                    {
                        foreach (string s in parentMsg.GetAll(parentClass))
                        {
                            sb.Append(string.Format("\t[BsonKnownTypes(typeof({0}))]\n", s));
                        }

                        sb.Append(string.Format("\tpublic partial class {0} {{}}\n\n", parentClass));
                    }

                    sb.AppendLine("}");
                }
            }

            if ((flag & HeadFlag.Proto) != 0)
            {
                if (parentMsg.Count > 0)
                {
                    sb.AppendLine(string.Format("namespace {0}", ns));
                    sb.AppendLine("{");
                    foreach (string parentClass in parentMsg.GetDictionary().Keys)
                    {

                        foreach (string s in parentMsg.GetAll(parentClass))
                        {
                            sb.Append(string.Format("\t[ProtoInclude({0}.{1}, typeof({2}))]\n", opcodeClassName, s, s));
                        }

                        sb.Append(string.Format("\tpublic partial class {0} {{}}\n\n", parentClass));
                    }

                    sb.AppendLine("}");
                }
            }
        }

        private static void GenerateOpcode(string ns, string outputFileName, string outputPath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("namespace {0}", ns));
            sb.AppendLine("{");
            sb.AppendLine(string.Format("\tpublic static partial class {0}", outputFileName));
            sb.AppendLine("\t{");
            foreach (OpcodeInfo info in msgOpcode)
            {
                sb.AppendLine(string.Format("\t\t public const ushort {0} = {1};", info.Name, info.Opcode));
            }
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            string csPath = Path.Combine(outputPath, outputFileName + ".cs");
            File.WriteAllText(csPath, sb.ToString());
        }

        private static void Repeated(StringBuilder sb, string ns, string newline, bool isClient)
        {
            try
            {
                int index = newline.IndexOf(";");
                newline = newline.Remove(index);
                string[] ss = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                string type = ss[1];
                type = ConvertType(type);
                string name = ss[2];
                int order = int.Parse(ss[3]);
                if (isClient)
                {
                    sb.Append(string.Format("\t\t[ProtoMember({0}, TypeName = \"{1}.{2}\")]\n", order, ns, type));
                }
                else
                {
                    sb.Append(string.Format("\t\t[ProtoMember({0})]\n", order));
                }

                sb.Append(string.Format("\t\tpublic List<{0}> {1} = new List<{2}>();\n", type, name, type));
                publicMembers.Add(name, string.Format("List<{0}>", type));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogFormat(string.Format("{0}\n {1}", newline, e));
            }

        }

        //初始化参数
        public static void DefalutConstructor(StringBuilder sb, string msgName)
        {
            //if (msgName.Equals(""))
            //    return;

            sb.Append(string.Format("\t\tpublic  {0}()", msgName));// msgName
            sb.Append("{");
            foreach (string name in publicMembers.Keys)
            {
                if (!publicMembers[name].StartsWith("List"))
                    sb.Append(string.Format("{0} t_{1} = {2}; ", publicMembers[name], name, name));//, name, name)); {3} = t_{4};
                //sb.Append(string.Format("\t\t\t{0} = {1};\n", name, name));
            }
            sb.Append("}\n");
            publicMembers.Clear();
            msgName = "";
        }

        private static string ConvertType(string type)
        {
            string typeCs = "";
            switch (type)
            {
                case "int16":
                    typeCs = "short";
                    break;
                case "int32":
                    typeCs = "int";
                    break;
                case "bytes":
                    typeCs = "byte[]";
                    break;
                case "uint32":
                    typeCs = "uint";
                    break;
                case "long":
                    typeCs = "long";
                    break;
                case "int64":
                    typeCs = "long";
                    break;
                case "uint64":
                    typeCs = "ulong";
                    break;
                case "uint16":
                    typeCs = "ushort";
                    break;
                case "sint32":
                    typeCs = "int";
                    break;
                case "sint64":
                    typeCs = "long";
                    break;

                default:
                    typeCs = type;
                    break;
            }
            return typeCs;
        }

        private static void Members(StringBuilder sb, string newline, bool isRequired)
        {
            try
            {
                int index = newline.IndexOf(";");
                newline = newline.Remove(index);
                string[] ss = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                string type = ss[1];
                string name = ss[2];
                //UnityEngine.Debug.LogError(" type is "+ type +" name is "+ name);
                int order = int.Parse(ss[3]);
                //UnityEngine.Debug.LogError(" order is " + order);

                //UnityEngine.Debug.LogFormat("\t\t[ProtoMember({0}, IsRequired = {1})]\n", order, isRequired.ToString().ToLower());

                string typeCs = ConvertType(type);
                sb.Append(string.Format("\t\tprivate bool {0}{1} = false;\n", name, "HasVal"));
                sb.Append(string.Format("\t\tpublic  {0} {1}{2} ;\n", typeCs, "_", name));
                sb.Append(string.Format("\t\t[ProtoMember({0}, IsRequired = {1})]\n", order, isRequired.ToString().ToLower()));
                sb.Append(string.Format("\t\tpublic {0} {1}  ", typeCs, name));
                string ecode = "{ get {return _" + name + ";} set { _" + name + "= value; " + name + "HasVal = true;}}";
                sb.Append(ecode);
                sb.Append(ProcessCheck(typeCs, name));

                publicMembers.Add(name, typeCs);
                //sb.Append("\n");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogFormat(string.Format("{0}\n {1}", newline, e));
            }
        }

        private static string ProcessCheck(string stype, string para)
        {
            StringBuilder sb = new StringBuilder();
            string firstStr = para.Substring(0, 1).ToUpper();
            string secondStr = para.Substring(1, para.Length - 1);
            //UnityEngine.Debug.LogError(" first str is "+ firstStr +" second str is "+secondStr);
            string checkStr = string.Format("\t\t public bool {0}{1}", para, "Specified");
            sb.Append(checkStr);
            sb.Append("{");
            string tempStr = "";
            tempStr = "get{ return this." + para + "!= null && " + para + "HasVal == true;}";
            //if (tempStr == "")
            //{
            //    return "";
            //}
            sb.Append(tempStr);
            sb.Append("}");
            sb.Append("\n");
            return sb.ToString();
        }

        private static void EnumMember(StringBuilder sb, string newline)
        {
            try
            {
                if (newline.Contains("{"))
                {
                    return;
                }
                int index = newline.IndexOf(";");
                newline = newline.Remove(index);
                string[] ss = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                string name = ss[0];
                int order = int.Parse(ss[1]);
                sb.Append(string.Format("\t\t[ProtoEnum(Value={0})]\n", order));
                sb.Append(string.Format("\t\t{0} = {1},\n", name, order));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogFormat(string.Format("{0}\n {1}", newline, e));
            }
        }

        private static void ProcessEnum(List<string> enumlist, StringBuilder sb)
        {
            bool doSkip = false;
            bool isMsgStart = false;
            enumNameList.Clear();
            for (int i = 0; i < enumlist.Count; i++)
            {
                string newline = enumlist[i];
                if (newline == "")
                {
                    continue;
                }
                if (newline.StartsWith("}"))
                {
                    isMsgStart = false;
                    if (!doSkip)
                        sb.Append("}\n");
                    doSkip = false;
                    continue;
                }
                if (newline.StartsWith("//")) // 添加注释的
                {
                    if (!doSkip)
                        sb.Append(string.Format("{0}\n", newline));
                }
                else if (newline.StartsWith("enum"))
                {
                    isMsgStart = true;
                    string msgName = newline.Split(splitChars, StringSplitOptions.RemoveEmptyEntries)[1];
                    if (enumNameList.Contains(msgName))
                    {
                        doSkip = true;
                        continue;
                    }
                    doSkip = false;
                    sb.Append("\t[ProtoContract]\n");
                    sb.Append(string.Format("\tpublic  enum {0}", msgName));
                    enumNameList.Add(msgName);
                    if (!newline.Contains("{"))
                        sb.Append("{\n");
                }
                else
                {
                    if (!doSkip)
                        EnumMember(sb, newline);
                }
            }
        }

    }
    public class MultiMap<T, K>
    {
        private readonly SortedDictionary<T, List<K>> dictionary = new SortedDictionary<T, List<K>>();

        // 重用list
        private readonly Queue<List<K>> queue = new Queue<List<K>>();

        private T firstKey;

        public SortedDictionary<T, List<K>> GetDictionary()
        {
            return this.dictionary;
        }

        public void Add(T t, K k)
        {
            List<K> list;
            this.dictionary.TryGetValue(t, out list);
            if (list == null)
            {
                list = this.FetchList();
            }
            list.Add(k);
            this.dictionary[t] = list;
        }

        public KeyValuePair<T, List<K>> First()
        {
            return this.dictionary.First();
        }

        public T FirstKey()
        {
            return this.dictionary.Keys.First();
        }

        public int Count
        {
            get
            {
                return this.dictionary.Count;
            }
        }

        private List<K> FetchList()
        {
            if (this.queue.Count > 0)
            {
                List<K> list = this.queue.Dequeue();
                list.Clear();
                return list;
            }
            return new List<K>();
        }

        private void RecycleList(List<K> list)
        {
            // 防止暴涨
            if (this.queue.Count > 100)
            {
                return;
            }
            list.Clear();
            this.queue.Enqueue(list);
        }

        public bool Remove(T t, K k)
        {
            List<K> list;
            this.dictionary.TryGetValue(t, out list);
            if (list == null)
            {
                return false;
            }
            if (!list.Remove(k))
            {
                return false;
            }
            if (list.Count == 0)
            {
                this.RecycleList(list);
                this.dictionary.Remove(t);
            }
            return true;
        }

        public bool Remove(T t)
        {
            List<K> list = null;
            this.dictionary.TryGetValue(t, out list);
            if (list != null)
            {
                this.RecycleList(list);
            }
            return this.dictionary.Remove(t);
        }

        /// <summary>
        /// 不返回内部的list,copy一份出来
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public K[] GetAll(T t)
        {
            List<K> list;
            this.dictionary.TryGetValue(t, out list);
            if (list == null)
            {
                return new K[0];
            }
            return list.ToArray();
        }

        /// <summary>
        /// 返回内部的list
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public List<K> this[T t]
        {
            get
            {
                List<K> list;
                this.dictionary.TryGetValue(t, out list);
                return list;
            }
        }

        public K GetOne(T t)
        {
            List<K> list;
            this.dictionary.TryGetValue(t, out list);
            if (list != null && list.Count > 0)
            {
                return list[0];
            }
            return default(K);
        }

        public bool Contains(T t, K k)
        {
            List<K> list;
            this.dictionary.TryGetValue(t, out list);
            if (list == null)
            {
                return false;
            }
            return list.Contains(k);
        }

        public bool ContainsKey(T t)
        {
            return this.dictionary.ContainsKey(t);
        }
    }
}