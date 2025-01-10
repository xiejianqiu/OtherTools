using Language;
using System;

public class TBPathAttribute : Attribute
{
    public string tbPathForAb;
    public string tbPathForSrc;
    public string UpdateTIps;
    public string PrefabLangInfo;
    public TBPathAttribute(string tbPathForAb, string tbPathForSrc, string updateTIps)
    {
        this.tbPathForAb = tbPathForAb;
        this.tbPathForSrc = tbPathForSrc;
        this.UpdateTIps = updateTIps;
    }

    public TBPathAttribute(string tbPathForAb, string tbPathForSrc, string updateTIps, string tPrefabLangInfo)
    {
        this.tbPathForAb = tbPathForAb;
        this.tbPathForSrc = tbPathForSrc;
        UpdateTIps = updateTIps;
        this.PrefabLangInfo = tPrefabLangInfo;
    }
}
