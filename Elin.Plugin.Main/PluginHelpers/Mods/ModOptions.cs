using BepInEx;
using Elin.Plugin.Generated;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;

namespace Elin.Plugin.Main.PluginHelpers.Mods
{
    [Obsolete("未完成")]
    public class ModOptionsController : ILanguageSystem
    {
        #region define

        private struct Id
        {
            public Id(string config, string option)
            {
                Config = config;
                Option = option;
            }

            #region property

            public string Config { get; }
            public string Option { get; }

            #endregion
        }

        #endregion

        public ModOptionsController(object rawController)
        {
            Raw = rawController;
            RawType = rawController.GetType();
        }

        #region property

        private object Raw { get; }
        private Type RawType { get; }

        private Action<string> SetPreBuildWithXmlMethod
        {
            // これに関してはキャッシュにせず MethodInfo を直接呼び出す形でいいかも。そんな頻繁に呼び出されないし
            get
            {
                if (field is null)
                {
                    var methodInfo = AccessTools.Method(RawType, "SetPreBuildWithXml");

                    var instanceExpr = Expression.Constant(Raw);
                    var argExpr = Expression.Parameter(typeof(string), "xml");
                    var callExpr = Expression.Call(instanceExpr, methodInfo, argExpr);
                    var lambda = Expression.Lambda<Action<string>>(callExpr, argExpr);
                    var action = lambda.Compile();

                    field = action;
                }
                return field;
            }
        }

        private Action<string, string, string> SetTranslationArgsStringStringStringMethod
        {
            get
            {
                if (field is null)
                {
                    var methodInfo = AccessTools.Method(RawType, "SetTranslation", new[] { typeof(string), typeof(string), typeof(string) });

                    var instanceExpr = Expression.Constant(Raw);
                    var argExprs = (
                       langCode: Expression.Parameter(typeof(string), "langCode"),
                       id: Expression.Parameter(typeof(string), "id"),
                       trans: Expression.Parameter(typeof(string), "trans")
                    );
                    var callExpr = Expression.Call(instanceExpr, methodInfo, argExprs.langCode, argExprs.id, argExprs.trans);
                    var lambda = Expression.Lambda<Action<string, string, string>>(callExpr, argExprs.langCode, argExprs.id, argExprs.trans);
                    var action = lambda.Compile();

                    field = action;
                }
                return field;
            }
        }

        #endregion

        #region function

        public void SetPreBuildXml(string xml)
        {
            SetPreBuildWithXmlMethod(xml);
        }

        public void SetTranslation(string langCode, string id, string trans)
        {
            SetTranslationArgsStringStringStringMethod(langCode, id, trans);
        }

        private Id ToId(string sectionName, PropertyInfo propertyInfo)
        {
            var configId = $"{sectionName}.{propertyInfo.Name}";
            var optionId = $"{sectionName}{propertyInfo.Name}";

            return new Id(configId, optionId);
        }

        private void ApplyTranslations(string sectionName, PropertyInfo propertyInfo, string langCode, PluginLocalization localization, HashSet<string> setIds)
        {
            var generatePluginConfigDescriptionAttribute = propertyInfo.GetCustomAttribute<GeneratePluginConfigDescriptionAttribute>();
            var id = ToId(sectionName, propertyInfo);

            // null ってる場合は子クラスか未設定
            if (generatePluginConfigDescriptionAttribute is null)
            {
                if (propertyInfo.PropertyType.IsClass)
                {
                    var nestedPropertyInfos = propertyInfo.PropertyType.GetProperties();
                    foreach (var nestedPropertyInfo in nestedPropertyInfos)
                    {
                        ApplyTranslations(id.Config, nestedPropertyInfo, langCode, localization, setIds);
                    }
                }
            }
            else
            {
                // 定義から翻訳文言を設定
                var propertyName = generatePluginConfigDescriptionAttribute.PropertyName;
                var localizationItem = generatePluginConfigDescriptionAttribute.Target switch
                {
                    PluginConfigDescriptionTarget.Config => localization.Config.Items[propertyName],
                    PluginConfigDescriptionTarget.General => localization.General.Items[propertyName],
                    _ => throw new NotImplementedException(),
                };

                var langValue = localizationItem.GetText(langCode, this);
                //ModHelper.LogDev(("set", langCode, id.Option, langValue));
                SetTranslation(langCode, id.Option, langValue);
                setIds.Add(id.Option);
            }
        }

        // 現状の SG では無理。実行時に構築する
        internal void ApplyTranslations<TConfig>(string langCode, PluginLocalization localization)
            where TConfig : class
        {
            var configType = typeof(TConfig);
            var sectionName = configType.Name;
            var setIds = new HashSet<string>();

            SetTranslation(langCode, Package.Id, Package.Title);
            var properties = configType.GetProperties();
            foreach (var property in properties)
            {
                ApplyTranslations(sectionName, property, langCode, localization, setIds);
            }

            // むりやりの流し込みで何とかしてみる
            foreach (var pair in localization.Config.Items.Where(a => !setIds.Contains(a.Key)))
            {
                var langValue = pair.Value.GetText(langCode, this);
                SetTranslation(langCode, pair.Key, langValue);
            }
        }

        internal void OnBuildUI(Action<OptionUIBuilder> action)
        {
            var onBuildUIProperty = AccessTools.Property(RawType, "OnBuildUI");
            var onBuildUI = (Delegate)onBuildUIProperty.GetValue(Raw);
            Action<object> x = (b) => action(new OptionUIBuilder(b));
            Delegate.Combine(onBuildUI, x);
        }

        // ここで全部やる！ これ以外の構築は一旦考慮しない！
        [Obsolete]
        internal void ApplyPreBuildXml<TConfig>(string xml, string langCode, PluginLocalization localization)
        {
            XmlDocument xmlDoc = new();
            xmlDoc.LoadXml(xml);

            var root = xmlDoc.DocumentElement;
            ModHelper.LogDev(root);
        }


        #endregion

        #region ILanguageSystem

        public string LangCode => Lang.langCode;

        public bool IsJP => Lang.isJP;

        public bool IsEN => Lang.isEN;


        #endregion
    }

    [Obsolete("未完成")]
    public class OptionUIBuilder
    {
        public OptionUIBuilder(object raw)
        {
            Raw = raw;
            RawType = raw.GetType();
        }

        #region property

        private object Raw { get; }
        private Type RawType { get; }

        #endregion
    }

    [Obsolete("未完成")]
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

    [Obsolete("未完成")]
    public static class ModOptionsExtensions
    {
        #region function

        public static ModOptionsController Register(this ModOptions modOptions, string guid)
        {
            return modOptions.Register(guid, null);
        }

        public static ModOptionsController Register(this ModOptions modOptions)
        {
            return modOptions.Register(Package.Id, null);
        }

        #endregion
    }
}
