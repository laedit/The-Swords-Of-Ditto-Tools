using Newtonsoft.Json;
using System;

namespace FormatSave
{
    class DoubleJsonConverter : JsonConverter
    {
        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(double);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(((double)value).ToString("0.000000", System.Globalization.CultureInfo.InvariantCulture));
        }
    }
}
