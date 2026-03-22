using Elin.Plugin.Main.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Elin.Plugin.Main.Converters
{
    /// <summary>
    /// <see cref="Color"/> JSON コンバーター。
    /// </summary>
    /// <remarks><see cref="JsonSerializer"/> という大御所を使用して <see cref="Color"/> が簡単にシリアライズ出来ないっていうのは Unity 側の怠慢ではないのか。</remarks>
    public class UnityColorConverter : JsonConverter<Color>
    {
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteValue(UnityUtility.ToHexColor(value));
        }

        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jsonValue = Newtonsoft.Json.Linq.JValue.Load(reader);
            var s = jsonValue.Value<string>();
            return UnityUtility.ConvertFromHexColor(s);
        }
    }
}
