using System;
using Newtonsoft.Json;

namespace MyLab.Task.Runtime
{
    class TaskEnumJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteValue(Enum.GetName(value!.GetType(), value)?.ToLower());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum;
        }
    }
}
