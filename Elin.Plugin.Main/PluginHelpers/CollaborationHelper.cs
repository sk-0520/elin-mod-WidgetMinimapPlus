using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elin.Plugin.Main.PluginHelpers
{
    [Obsolete("未完成")]
    public class CollaborationHelper
    {
        #region define

        /// <summary>
        /// 各種連携 Mod の ID。
        /// </summary>
        public static class ModId
        {
            #region define

            /// <summary>
            /// Mod Options
            /// </summary>
            /// <seealso href="https://steamcommunity.com/sharedfiles/filedetails/?id=3381182341" />
            public const string ModOptions = "evilmask.elinplugins.modoptions";

            #endregion
        }

        #endregion

        #region function

        private static IEnumerable<BaseUnityPlugin> FilterPlugins(List<object> plugins)
        {
            return plugins
                .OfType<BaseUnityPlugin>()
                .Where(a => a is not null)
            ;
        }

        public BaseUnityPlugin? FindPluginOrDefault(string pluginId)
        {
            return FilterPlugins(ModManager.ListPluginObject)
                .FirstOrDefault(a => a.Info.Metadata.GUID == pluginId)
            ;
        }

        public BaseUnityPlugin FindPlugin(string pluginId)
        {
            return FilterPlugins(ModManager.ListPluginObject)
                .First(a => a.Info.Metadata.GUID == pluginId)
            ;
        }

        #endregion
    }

    [Obsolete("未完成")]
    public static class CollaborationHelperExtensions
    {
        #region function

        public static BaseUnityPlugin? FindModOptions(this CollaborationHelper collaborate)
        {
            return collaborate.FindPluginOrDefault(CollaborationHelper.ModId.ModOptions);
        }

        #endregion
    }
}
