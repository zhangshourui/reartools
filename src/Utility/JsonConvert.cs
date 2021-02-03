using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility
{
  
    public class EmptyStringToDefault : JsonConverter
    {
        private readonly Type[] _types;
        public EmptyStringToDefault()
        {

        }
        public EmptyStringToDefault(params Type[] types)
        {
            _types = types;
        }
        public override bool CanConvert(Type objectType)
        {
            if (_types != null)
                return _types.Any(t => t == objectType);
            else
                return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value != null)
            {
                if (reader.ValueType == typeof(string) && objectType != typeof(string))
                {
                    var str = reader.Value.ToString();
                    if (string.IsNullOrEmpty(str))
                    {
                        return objectType.IsValueType ? Activator.CreateInstance(objectType) : null;
                        //return existingValue;
                    }
                }
            }
            return serializer.Deserialize(reader, objectType);

        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //throw new NotImplementedException();
            writer.WriteValue(value);
        }
    }
}
