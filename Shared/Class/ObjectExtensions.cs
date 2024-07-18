using System.Text.Json;

namespace GEORGE.Shared.Class
{
    public static class ObjectExtensions
    {
        public static T DeepCopy<T>(this T self)
        {
            var json = JsonSerializer.Serialize(self);
            return JsonSerializer.Deserialize<T>(json);
        }

        public static List<T> DeepCopyList<T>(this List<T> self)
        {
            var json = JsonSerializer.Serialize(self);
            return JsonSerializer.Deserialize<List<T>>(json);
        }
    }
}
