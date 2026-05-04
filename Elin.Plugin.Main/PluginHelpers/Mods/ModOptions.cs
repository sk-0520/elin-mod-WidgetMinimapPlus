using BepInEx;
using Elin.Plugin.Generated;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Elin.Plugin.Main.PluginHelpers.Mods
{
    public class ModOptionsController : ILanguageSystem
    {
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

        private Action<string, string, string> SetTranslationMethod
        {
            get
            {
                if (field is null)
                {
                    var methodInfo = AccessTools.Method(RawType, "SetTranslation");

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
            SetTranslationMethod(langCode, id, trans);
        }

        private void ApplyTranslations(string sectionName, PropertyInfo propertyInfo, string langCode, PluginLocalization localization, HashSet<string> setIds)
        {
            var generatePluginConfigDescriptionAttribute = propertyInfo.GetCustomAttribute<GeneratePluginConfigDescriptionAttribute>();
            var configId = $"{sectionName}.{propertyInfo.Name}";
            var propertyId = $"{sectionName}{propertyInfo.Name}";

            // null ってる場合は子クラスか未設定
            if (generatePluginConfigDescriptionAttribute is null)
            {
                if (propertyInfo.PropertyType.IsClass)
                {
                    var nestedPropertyInfos = propertyInfo.PropertyType.GetProperties();
                    foreach (var nestedPropertyInfo in nestedPropertyInfos)
                    {
                        ApplyTranslations(configId, nestedPropertyInfo, langCode, localization, setIds);
                    }
                }
            }
            else
            {
                var propertyName = generatePluginConfigDescriptionAttribute.PropertyName;
                var localizationItem = generatePluginConfigDescriptionAttribute.Target switch
                {
                    PluginConfigDescriptionTarget.Config => localization.Config.Items[propertyName],
                    PluginConfigDescriptionTarget.General => localization.General.Items[propertyName],
                    _ => throw new NotImplementedException(),
                };

                if (setIds.Contains(propertyId))
                {
                    ModHelper.LogDev((langCode, propertyId, localizationItem.GetText(this)));
                    SetTranslation(langCode, propertyId, localizationItem.GetText(this));
                    setIds.Add(propertyId);
                }
                else
                {
                    ModHelper.LogDev((langCode, propertyId, localizationItem.GetText(this)));
                }
            }
        }

        // 現状の SG では無理。実行時に構築する
        internal void ApplyTranslations<TConfig>(string langCode, PluginLocalization localization)
            where TConfig : class
        {
            var configType = typeof(TConfig);
            var sectionName = configType.Name;
            var setIds = new HashSet<string>();

            var properties = configType.GetProperties();
            foreach (var property in properties)
            {
                ApplyTranslations(sectionName, property, langCode, localization, setIds);
            }
        }

        #endregion

        #region ILanguageSystem

        public string LangCode => Lang.langCode;

        public bool IsJP => Lang.isJP;

        public bool IsEN => Lang.isEN;


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
