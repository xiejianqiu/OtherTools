using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace CoreLib
{
    public sealed class IniFile
    {
   
        private string m_FileName = null;
        internal string FileName
        {
            get
            {
                return m_FileName;
            }
        }
            
        private Dictionary<string, Dictionary<string, string>> m_Sections = new Dictionary<string,Dictionary<string, string>>(); 


        public IniFile(string FileName)
        {
            Initialize(FileName, false);
        }
            
        private void Initialize (string FileName, bool Lazy)
        {
            m_FileName = FileName;
            LoadData();
        }

        private void LoadData()
        {
            StreamReader sr = null;
            try
            {
                m_Sections.Clear();
            
                try
                {
                    sr = new StreamReader(m_FileName);
                }
                catch (FileNotFoundException)
                {
                    return;
                }
                    
                Dictionary<string, string> CurrentSection = null;
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    s = s.Trim();

                    if (s.StartsWith("[") && s.EndsWith("]"))
                    {
                        if (s.Length > 2)
                        {
                            string SectionName = s.Substring(1,s.Length-2);

                            if (m_Sections.ContainsKey(SectionName))
                            {
                                CurrentSection = null;
                            }
                            else
                            {
                                CurrentSection = new Dictionary<string, string>();
                                m_Sections.Add(SectionName,CurrentSection);
                            }
                        }
                    }
                    else if(s.StartsWith("/"))
                    {
                        continue;
                    }
                    else if (CurrentSection != null)
                    {
                        int i;
                        if ((i=s.IndexOf('=')) > 0)
                        {
                            int j = s.Length - i - 1;
                            string Key = s.Substring(0,i).Trim();
                            if (Key.Length  > 0)
                            {
                                if (!CurrentSection.ContainsKey(Key))
                                {
                                    string Value = (j > 0) ? (s.Substring(i+1,j).Trim()) : ("");
                                    CurrentSection.Add(Key,Value);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                if (sr != null) sr.Close();
                sr = null;
            }
        }

        public string GetValue(string SectionName, string Key, string DefaultValue)
        {
            Dictionary<string, string> Section;
            if (!m_Sections.TryGetValue(SectionName, out Section)) return DefaultValue;
            string Value;
            if (!Section.TryGetValue(Key, out Value)) return DefaultValue;
            return Value;
        }

        public bool GetValue(string SectionName, string Key, bool DefaultValue)
        {
            string StringValue=GetValue(SectionName, Key, DefaultValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
            int Value;
            if (int.TryParse(StringValue, out Value)) return (Value != 0);
            return DefaultValue;
        }

        public int GetValue(string SectionName, string Key, int DefaultValue)
        {
            string StringValue=GetValue(SectionName, Key, DefaultValue.ToString(CultureInfo.InvariantCulture));
            int Value;
            if (int.TryParse(StringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out Value)) return Value;
            return DefaultValue;
        }

        public double GetValue(string SectionName, string Key, double DefaultValue)
        {
            string StringValue=GetValue(SectionName, Key, DefaultValue.ToString(CultureInfo.InvariantCulture));
            double Value;
            if (double.TryParse(StringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out Value)) return Value;
            return DefaultValue;
        }
    }
}

