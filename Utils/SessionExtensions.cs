using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace TauManager.Utils
{
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            return value == null ? default(T) : 
                JsonConvert.DeserializeObject<T>(value);
        }

        public static T Get<T>(this ISession session, string key, T defaultValue)
        {
            var value = session.GetString(key);

            return value == null ? defaultValue : 
                JsonConvert.DeserializeObject<T>(value);
        }

        public static bool TryGet<T>(this ISession session, string key, out T resultObject)
        {
            var result = session.TryGetValue(key, out var outObject);
            resultObject = outObject == null ? default(T) :
                JsonConvert.DeserializeObject<T>(outObject.ToString());
            return result;
        }
    }
}