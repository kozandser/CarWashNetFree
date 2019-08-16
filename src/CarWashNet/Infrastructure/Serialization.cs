using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarWashNet.Infrastructure
{
    public interface IKLibSerializer
    {
        string Serialize<T>(T obj);
        T Deserialize<T>(string input);
    }
    public class JsonKLibSerializer : IKLibSerializer
    {
        public string Serialize<T>(T obj)
        {
            string jsonString = fastJSON.JSON.ToNiceJSON(obj);
            return jsonString;
        }
        public T Deserialize<T>(string jsonString)
        {
            T returnValue = default(T);
            returnValue = fastJSON.JSON.ToObject<T>(jsonString);
            return returnValue;
        }
    }
    public static class JsonKLibSerializerHelper
    {
        public static string SerializeJson<T>(this T obj)
        {
            return new JsonKLibSerializer().Serialize(obj);
        }
        public static T DeserializeJson<T>(this string jsonString)
        {
            return new JsonKLibSerializer().Deserialize<T>(jsonString);
        }
    }

}
