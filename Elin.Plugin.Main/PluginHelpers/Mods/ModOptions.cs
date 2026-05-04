using BepInEx;
using Elin.Plugin.Generated;
using HarmonyLib;
using System;

namespace Elin.Plugin.Main.PluginHelpers.Mods
{
    public class ModOptionsController
    {
        public ModOptionsController(object rawController)
        {
            Raw = rawController;
            RawType = rawController.GetType();
        }

        #region property

        private object Raw { get; }
        private Type RawType { get; }

        #endregion

        #region function

        public void SetPreBuildXml(string xml)
        {
            var setPreBuildWithXmlMethod = AccessTools.Method(RawType, "SetPreBuildWithXml");
            setPreBuildWithXmlMethod.Invoke(Raw, new object[] { xml });
        }

        #endregion
    }

    public class ModOptions
    {
        public ModOptions(BaseUnityPlugin plugin)
        {
            Plugin = plugin;
            PluginType = Plugin.GetType();
        }

        #region property

        private BaseUnityPlugin Plugin { get; }
        private Type PluginType { get; }

        #endregion

        #region function

        public ModOptionsController Register(string guid, string? tooltipId, params object[] configs)
        {
            var controllerType = PluginType.Assembly.GetType("EvilMask.Elin.ModOptions.ModOptionController");

            var registerMethod = AccessTools.Method(controllerType, "Register", new[] { typeof(string), typeof(string), typeof(object[]) });
            var rawController = registerMethod.Invoke(null, new object?[] { guid, tooltipId, configs });
            if (rawController is null)
            {
                throw new InvalidOperationException($"{PluginType}.Register");
            }

            return new ModOptionsController(rawController);
        }

        #endregion
    }

    public static class ModOptionsExtensions
    {
        #region function

        public static ModOptionsController Register(this ModOptions modOptions, string guid)
        {
            return modOptions.Register($"{Package.Title}({Package.Id})", guid);
        }

        public static ModOptionsController Register(this ModOptions modOptions)
        {
            return modOptions.Register($"{Package.Title}({Package.Id})", null!);
        }

        #endregion
    }
}
