using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace System.Net.TMDb.Internal
{
    internal class ResourceCreationConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Resource).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject
            object target;
            if (!TryCreateByMediaType(jObject, out target))
                target = Activator.CreateInstance(objectType);

            // Populate the object properties
            serializer.Populate(jObject.CreateReader(), target);
            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private static bool TryCreateByMediaType(JObject jObject, out object target)
        {
            switch ((string)jObject["media_type"])
            {
                case "movie":
                    target = new Movie();
                    break;
                case "person":
                    target = new Person();
                    break;
                case "tv":
                    target = new Show();
                    break;
                default:
                    target = null;
                    return false;
            }
            return true;
        }
    }
}
