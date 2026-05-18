using System;
using System.IO;

namespace Elin.Plugin.Main.PluginHelpers
{
    public class AssetHelper
    {
        public AssetHelper(string rootDirectoryPath)
        {
            RootDirectoryPath = rootDirectoryPath;
        }

        #region property

        /// <summary>
        /// プラグインのルートディレクトリパス。
        /// </summary>
        public string RootDirectoryPath { get; }

        #endregion

        #region function

        public string Combine(string path, params string[] path2)
        {
            if (3 <= path2.Length)
            {
                var paths = new string[2 + path2.Length];
                paths[0] = RootDirectoryPath;
                paths[1] = path;
                Array.Copy(path2, 0, paths, 2, path2.Length);
                return Path.Combine(paths);
            }

            return path2.Length switch
            {
                0 => Path.Combine(RootDirectoryPath, path),
                1 => Path.Combine(RootDirectoryPath, path, path2[0]),
                2 => Path.Combine(RootDirectoryPath, path, path2[0], path2[1]),
                _ => throw new System.NotImplementedException(),
            };
        }

        #endregion
    }
}
