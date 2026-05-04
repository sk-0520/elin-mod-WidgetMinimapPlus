using Elin.Plugin.Main.Models.Settings;
using Elin.Plugin.Main.PluginHelpers;
using Elin.Plugin.Main.PluginHelpers.Mods;

// Mod 用テンプレート組み込み想定

namespace Elin.Plugin.Main
{
    partial class Plugin
    {
        #region function

        private Setting LoadConfig()
        {
            var setting = Setting.Bind(Config, new Setting());
            return setting;
        }

        /// <summary>
        /// 起動時のプラグイン独自処理。
        /// </summary>
        private void AwakePlugin()
        {
            var setting = LoadConfig();
            Setting.Instance = setting;
            // 設定のビルド自体はプロファイル読み込み時に行う
        }

        /// <summary>
        /// 初期化時のプラグイン独自処理。
        /// </summary>
        /// <remarks>
        /// <para>通常の初期化は基本的に <see cref="AwakePlugin"/> で行う想定。</para>
        /// <para>ModHelp 用に <see cref="Start"/> を生やしたので本メソッドが追加されただけ。</para>
        /// </remarks>
        private void StartPlugin()
        {
            //NOP
        }

        /// <summary>
        /// 終了時のプラグイン独自処理。
        /// </summary>
        private void OnDestroyPlugin()
        {
            //NOP
        }

#if DEBUG
        public void PHL()
        {
            ModHelper.LogDev("PHL!");
            Setting.Instance.Build(MessageHelper.CanOutputMessage);
        }
#endif

        #endregion

        #region TemplatePluginBase

        protected override void BuildModOptions(ModOptions modOptions)
        {
            var xml = //lang=xml
"""
<?xml version="1.0" encoding="UTF-8"?>
<config xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="ConfigLayoutSchema.xsd">
    <hlayout>
        <vlayout border="true">
            <slider>MySlider</slider>
            <t_group min_count="1" max_count="1" value="1">
                <title>MySelectionGroup</title>
                <choice>
                    <contentId>item01</contentId>
                </choice>
                <choice>
                    <contentId>item02</contentId>
                </choice>
                <choice>
                    <contentId>item03</contentId>
                </choice>
                <choice>
                    <contentId>item04</contentId>
                </choice>
            </t_group>
            <button id = "btn01">
                <contentId>button.name</contentId>
                <tooltip>button.tooltip</tooltip>
            </button>
        </vlayout>
        <vlayout border="true">
            <one_choice type="lr_select" value="1">
                <choice>
                    <contentId>buttonLR.item01</contentId>
                    <tooltip>tootipId01</tooltip>
                </choice>
                <choice>
                    <contentId>buttonLR.item02</contentId>
                </choice>
                <choice>
                    <contentId>buttonLR.item03</contentId>
                </choice>
            </one_choice>
            <one_choice type="dropdown" value="1">
                <choice>
                    <contentId>dropdown.item01</contentId>
                    <tooltip>dropdown.tootip01</tooltip>
                </choice>
                <choice>
                    <contentId>dropdown.item02</contentId>
                </choice>
                <choice>
                    <contentId>dropdown.item03</contentId>
                </choice>
            </one_choice>
            <t_group id = "group02" min_count="0" max_count="2" value="1,2">
                <title>MySelectionGroup2</title>
                <choice>
                    <contentId>item01</contentId>
                </choice>
                <choice>
                    <contentId>item02</contentId>
                </choice>
                <choice>
                    <contentId>item03</contentId>
                </choice>
                <choice>
                    <contentId>item04</contentId>
                </choice>
            </t_group>
        </vlayout>
    </hlayout>
    <text>exampleText</text>
    <slider id = "slider02" min="0" max="100" value="30" buttons="true">MySlider</slider>
    <input>
        <placeholderID>input.placeholder</placeholderID>
    </input>
</config>
"""
            ;

            var controller = modOptions.Register();
            controller.SetPreBuildXml(xml);
        }

        #endregion
    }
}
