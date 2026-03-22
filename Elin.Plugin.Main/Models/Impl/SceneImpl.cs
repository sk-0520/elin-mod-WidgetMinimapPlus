using Elin.Plugin.Main.Models.Settings;

namespace Elin.Plugin.Main.Models.Impl
{
    public static class SceneImpl
    {
        #region Scene

        public static void InitPostfix(Scene __instance, Scene.Mode newMode, Setting setting)
        {
            if (newMode == Scene.Mode.StartGame)
            {
                setting.Build(true);
            }
        }

        #endregion
    }
}
