using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GameFramework
{
    public class HashUtil
    {
        public static string Get(Stream fs)
        {
            HashAlgorithm ha = HashAlgorithm.Create();
            byte[] bytes = ha.ComputeHash(fs);
            fs.Close();
            return ToHexString(bytes);
        }
        static public string ConvertToABName(string assetPath)
        {
            string bn = (assetPath)
                .Replace('\\', '_')
                .Replace('/', '_')
                .Replace(" ", "_")
                .ToLower()
                .Replace("assets_bundledata_packs_", "pack_")
                .Replace("assets_bundledata_", "aroot_")
                .Replace("assets_scene_scene_", "asroot_")
                .Replace("assets_scene_", "asother_")
                .Replace("assets_resms_", "ares_")
                .Replace("assets_resources_", "ares2_")
                .Replace("assets_(temporary)_", "atmp_")
                .Replace("assets_", "u_");
            return bn;
        }
        public static string Get(string s)
        {
            return s;
            //return Get(Encoding.UTF8.GetBytes(s));
        }

        public static string Get(byte[] data)
        {
            HashAlgorithm ha = HashAlgorithm.Create();
            byte[] bytes = ha.ComputeHash(data);
            return ToHexString(bytes);
        }

        public static string ToHexString(byte[] bytes)
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString("X2"));
                }
                hexString = strB.ToString().ToLower();
            }
            return hexString;
        }
    }
}
