using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Language
{
    public class PrefabLangInfo
    {
        public string name;
        public string assetPath;
        public List<LblInfo> lblLst;
        public PrefabLangInfo()
        { 
        }
        public PrefabLangInfo(string name, string assetPath)
        {
            this.name = name;
            this.assetPath = assetPath;
            lblLst = new List<LblInfo>();
        }
    }
}
