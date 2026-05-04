using BepInEx;
using Elin.Plugin.Generated;
using Elin.Plugin.Main.PluginHelpers;
using Elin.Plugin.Main.PluginHelpers.Mods;
using HarmonyLib;
using System;
using System.Threading;

// このファイルはテンプレート管理のため編集しないでください。

namespace Elin.Plugin.Main
{
    public abstract class TemplatePluginBase : BaseUnityPlugin
    {
        #region function

        protected virtual void BuildModOptions(ModOptions modOptions)
        { }

        #endregion
    }

    [BepInPlugin(Package.Id, Mod.Name, Mod.Version)]
    public partial class Plugin : TemplatePluginBase
    {
        #region property

        /// <summary>
        /// <see cref="AwakePlugin"/> 後に <see cref="Harmony.PatchAll()"/> を呼び出すか。
        /// </summary>
        /// <remarks>
        /// <para>デフォルト値の <see langword="true"/> で問題ない。</para>
        /// <para>テンプレートでは <see cref="AwakePlugin"/> による構築を前提としており、<c>Start</c> メソッドまでは面倒を見ない。</para>
        /// </remarks>
        private bool CallPatchAll { get; set; } = true;

        /// <summary>
        /// <see cref="HarmonyLib.Harmony"/> のインスタンス。
        /// </summary>
        /// <remarks>テンプレート側で初期化、破棄される。</remarks>
        private Harmony Harmony { get; } = new Harmony(Package.Id);

        #endregion

        #region function

        /// <summary>
        /// 起動。
        /// </summary>
        /// <remarks>本メソッドではインフラ面の構築も行っているため、プラグインとしての起動処理は <see cref="AwakePlugin()"/> で実施すること。</remarks>
        public void Awake()
        {
            ModHelper.Initialize(this, Logger, SynchronizationContext.Current);

            AwakePlugin();

            if (CallPatchAll)
            {
                Harmony.PatchAll();
            }
        }

        public void Start()
        {
            StartPlugin();

            RegisterModOptions();
        }

        public void OnDestroy()
        {
            OnDestroyPlugin();

            if (Harmony is IDisposable disposable)
            {
                disposable.Dispose();
            }

            ModHelper.Destroy();
        }

        private void RegisterModOptions()
        {
            var modOptionsPlugin = ModHelper.Collaborate.FindModOptions();
            if (modOptionsPlugin is null)
            {
                return;
            }

            var modOptions = new ModOptions(modOptionsPlugin);
            BuildModOptions(modOptions);
        }

        #endregion
    }
}
