using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameFramework
{
    public partial class AssetTarget : System.IComparable<AssetTarget>
    {
        #region shader pack
        private static AssetTarget shadersAssetTarget;
        bool CheckShaderAndAddToPack(string path)
        {
            if (null == shadersAssetTarget)
                return false;
            if (path.EndsWith(".shader"))
            {
                ProcessPackDepedency(path, shadersAssetTarget);
                return true;
            }
            if (path.StartsWith(@"Assets\Effect\Shaders\") && path.EndsWith(".asset"))
            {
                ProcessPackDepedency(path, shadersAssetTarget);
                return true;
            }
            if (path.StartsWith(@"Assets\PostProcessing") && (
                    path.EndsWith(".compute")
                    || path.EndsWith(".png")
                    || path.EndsWith(".tga")
                    || path.EndsWith(".asset")
                ))
            {
                ProcessPackDepedency(path, shadersAssetTarget);
                return true;
            }
            return false;
        }
        #endregion
        #region root pack
        /// <summary>
        /// UI用到的音效打一个包
        /// <\summary>
        private static AssetTarget musicAssetTarget;
        bool CheckMusicAndAddToPack(string path)
        {
            if (null == musicAssetTarget)
                return false;
            if ((path.EndsWith(".mp3") || path.EndsWith(".wav")) && path.StartsWith(@"Assets\BundleData\Music\UI\"))
            {
                ProcessPackDepedency(path, musicAssetTarget);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 捏脸相关资源打一个包，后续可以考虑删除这部分资源
        /// <\summary>
        /// <param name="path"><\param>
        /// <returns><\returns>

        private static AssetTarget playerBaseAT;
        /// <summary>
        /// 判断是否为捏脸目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool IsKneadRes(string path)
        {
            if (path.StartsWith(@"Assets\BundleData\Texture\Player\")
                    || path.StartsWith(@"Assets\BundleData\Prefab\Model\Player\") && (
                        path.Contains(@"\face\") || path.Contains(@"\hair\")
                    ))
            {
                return true;
            }
            return false;
        }
        bool CheckPlayerBaseAndAddToPack(string path)
        {
            if (null == playerBaseAT)
                return false;
            if (IsKneadRes(path))
            {
                HashSet<AssetTarget> atSet = new HashSet<AssetTarget>();
                GetDependencies(this, atSet);
                foreach (var child in atSet)
                {
                    path = child.assetPath;
                    if (child.exportType == AssetBundleExportType.Asset || IsKneadRes(child.assetPath))
                    { 
                        ProcessPackDepedency(child.assetPath, playerBaseAT);
                    }
                }
                return true;
            }
            return false;
        }
        #endregion
        #region dir pack
        static string[][] dir_packs = new string[][] {
            new string[]{ @"Assets\Effect\Animation\", "effect_animation.all"},
            new string[]{ @"Assets\Effect\Fbx\","effect_fbx.all" },
            new string[]{ @"Assets\Effect\Mesh\","effect_mesh.all"},
            new string[]{ @"Assets\Effect\Tex\", "effect_tex.all"},
            new string[]{ @"Assets\Effect\Textures\Daoguang\", "effect_daoguang.all"},
            new string[]{ @"Assets\Effect\Textures\Dilie\", "effect_texture_dilie.all"},
            new string[]{ @"Assets\Effect\Textures\fazhen\", "effect_texture_fazhen.all"},
            new string[]{ @"Assets\Effect\Textures\Fazheng\", "effect_texture_fazheng.all"},
            new string[]{ @"Assets\Effect\Textures\Glow\", "effect_texture_glow.all"},
            new string[]{ @"Assets\Effect\Textures\Lizi\", "effect_texture_lizi.all"},
            new string[]{ @"Assets\Effect\Textures\Path\", "effect_texture_path.all"},
            new string[]{ @"Assets\Effect\Textures\Tongyong\", "effect_texture_tongyong.all"},
            new string[]{ @"Assets\Effect\Textures\Ui\", "effect_texture_ui.all"},
            new string[]{ @"Assets\Effect\Textures\Wenli\", "effect_texture_wenli.all"},
            new string[]{ @"Assets\Effect\Textures\Wuti\", "effect_texture_wuti.all"},
            new string[]{ @"Assets\Effect\Textures\Xulie\", "effect_texture_xulie.all"},
            new string[]{ @"Assets\Effect\Textures\Yanwu\", "effect_texture_yanwu.all"},

            new string[]{ @"Assets\Scene\Brush\", "scene_bursh.all"},
            new string[]{ @"Assets\Scene\water\", "scene_bursh.all"},
            new string[]{ @"Assets\Scene\delong\", "scene_bursh.all"},

            //new string[]{ @"Assets\BundleData\TPProject\itemicon\", "atlas_item.all"},
            //new string[]{ @"Assets\BundleData\TPProject\itemicon2\", "atlas_item.all"},
            //new string[]{ @"Assets\BundleData\TPProject\itemicon3\", "atlas_item.all"},
            //new string[]{ @"Assets\BundleData\TPProject\itemicon4\", "atlas_item.all"},
            //new string[]{ @"Assets\BundleData\TPProject\cardicon\", "atlas_item.all"},
            //new string[]{ @"Assets\BundleData\TPProject\itemicon6\", "atlas_item.all"},
            //new string[]{ @"Assets\BundleData\TPProject\itemicon7\", "atlas_item.all"},
            //new string[]{ @"Assets\BundleData\TPProject\itemicon10\", "atlas_item.all"},
            //new string[]{ @"Assets\BundleData\TPProject\itemicon5\", "atlas_item.all"},

            //new string[]{ @"Assets\BundleData\TPProject\mainui\", "atlas_other.all"},
            //new string[]{ @"Assets\BundleData\TPProject\mainui2\", "atlas_other.all"},
            //new string[]{ @"Assets\BundleData\TPProject\skillicon\", "atlas_other.all"},
            //new string[]{ @"Assets\BundleData\TPProject\skillicon2\", "atlas_other.all"},
            //new string[]{ @"Assets\BundleData\TPProject\public\", "atlas_other.all"},
            //new string[]{ @"Assets\BundleData\TPProject\peishi\", "atlas_other.all"},
            //new string[]{ @"Assets\BundleData\TPProject\qualityeffect\", "atlas_other.all"},
            //new string[]{ @"Assets\BundleData\TPProject\common\", "atlas_other.all"},
            //new string[]{ @"Assets\BundleData\TPProject\cardicon\", "atlas_other.all"},

            //new string[]{ @"Assets\BundleData\TPProject\vipeffect_1\", "atlas_effect.all"},
            //new string[]{ @"Assets\BundleData\TPProject\vipeffect_3\", "atlas_effect.all"},
            //new string[]{ @"Assets\BundleData\TPProject\vipeffect_5\", "atlas_effect.all"},
            //new string[]{ @"Assets\BundleData\TPProject\vipeffect_6\", "atlas_effect.all"},
            //new string[]{ @"Assets\BundleData\TPProject\vipeffect_7\", "atlas_effect.all"},
            //new string[]{ @"Assets\BundleData\TPProject\vipeffect_8\", "atlas_effect.all"},
            //new string[]{ @"Assets\BundleData\TPProject\vipeffect_9\", "atlas_effect.all"},
            //new string[]{ @"Assets\BundleData\TPProject\vipeffect_10\", "atlas_effect.all"},
            //new string[]{ @"Assets\BundleData\TPProject\vipeffect_11\", "atlas_effect.all"},

            //new string[]{ @"Assets\BundleData\TPProject\fubencommon\", "atlas_main.all"},
            //new string[]{ @"Assets\BundleData\TPProject\CenterMarryUI\", "atlas_main.all"},
            //new string[]{ @"Assets\BundleData\TPProject\CombatFont\", "atlas_main.all"},
            //new string[]{ @"Assets\BundleData\TPProject\AcRedBag\", "atlas_main.all"},
            //new string[]{ @"Assets\BundleData\TPProject\ChickenLittle\", "atlas_main.all"},
            //new string[]{ @"Assets\BundleData\TPProject\emotion\", "atlas_main.all"},
            //new string[]{ @"Assets\BundleData\TPProject\expression\", "atlas_main.all"},
            //new string[]{ @"Assets\BundleData\TPProject\friends\", "atlas_main.all"},
            //new string[]{ @"Assets\BundleData\TPProject\bossFK\", "atlas_main.all"},
            //new string[]{ @"Assets\BundleData\TPProject\laddercommon\", "atlas_main.all"},
            //new string[]{ @"Assets\BundleData\TPProject\bigskillicon\", "atlas_main.all"},
            //new string[]{ @"Assets\BundleData\TPProject\map\", "atlas_main.all"},
            //new string[]{ @"Assets\BundleData\TPProject\public3\", "atlas_main.all"},
            //new string[]{ @"Assets\BundleData\TPProject\public2\", "atlas_main.all"},

        };
        static Dictionary<string, AssetTarget> effectPackDict;
        static AssetTarget effectMatAT;
        public static void CreateDirPack()
        {
            effectPackDict = new Dictionary<string, AssetTarget>();
            foreach (var info in dir_packs)
            {
                AssetTarget target = null;
                CreateCustomPack(info[1], out target);
                effectPackDict[info[1]] = target;
            }
        }
        bool CheckEffectDirAndAddToPack(string path)
        {
            if (null == effectPackDict || effectPackDict.Count <= 0)
                return false;
            foreach (var info in dir_packs)
            {
                if (path.StartsWith(info[0]))
                {
                    if (null != effectMatAT && path.StartsWith(@"Assets\Effect\") && path.EndsWith(".mat"))
                    {
                        HashSet<AssetTarget> atSet = new HashSet<AssetTarget>();
                        GetDependencies(this, atSet);
                        foreach (var at in atSet)
                        { 
                            ProcessPackDepedency(at.assetPath, effectMatAT);
                        }
                        ProcessPackDepedency(path, effectMatAT);
                        continue;
                    }
                    if (path.StartsWith(@"Assets\Effect\") && path.EndsWith(".mat"))
                        continue;
                    ProcessPackDepedency(path, effectPackDict[info[1]]);
                    return true;
                }
            }
            return false;

        }
        #endregion

        #region share pack
        private static AssetTarget pluginsPack;
        bool CheckBaseAndAddToPack(string path)
        {
            if (null == pluginsPack)
                return false;
            if (path.StartsWith(@"Assets\Resources\WaterTextures"))
            {
                ProcessPackDepedency(path, pluginsPack);
                return true;
            }
            if (path.EndsWith(".prefab")
                && (path.StartsWith(@"Assets\Plugins\")
                || path.StartsWith(@"Assets\Resources\Prefab\")))
            {
                ProcessPackDepedency(path, pluginsPack);
                return true;
            }
            if ((path.EndsWith(".ttf") || path.EndsWith(".TTF")) && path.StartsWith(@"Assets\Font\"))
            {
                ProcessPackDepedency(path, pluginsPack);
                return true;
            }
            return false;
        }
        #endregion
        #region 主界面关联的UI打成一个ab
        private static AssetTarget mainuidepAT;
        /// <summary>
        /// 依赖的prefab不依赖图集
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool CheckMainUIDepAndPack(string path)
        {
            if (null == mainuidepAT)
                return false;
            if (path.EndsWith(@"\NewMainUI.prefab"))
            {
                HashSet<AssetTarget> atSet = new HashSet<AssetTarget>();
                GetDependencies(this, atSet);
                foreach (var child in atSet)
                {
                    //有专门的pack处理图集
                    if (child.assetPath.EndsWith(".prefab") && !child.assetPath.StartsWith(@"Assets\BundleData\TPProject"))
                    {
                        ProcessPackDepedency(child.assetPath, mainuidepAT);
                    }
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取依赖的所有资源
        /// </summary>
        /// <param name="at"></param>
        /// <param name="set"></param>
        void GetDependencies(AssetTarget at, HashSet<AssetTarget> set)
        {
            var iter = at._dependParentSet.GetEnumerator();
            while (iter.MoveNext())
            {
                set.Add(iter.Current);
                at.GetDependencies(iter.Current, set);
            }
        }
        #endregion
    }
}
