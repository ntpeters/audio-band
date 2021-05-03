using System.Collections.Generic;
using System.Linq;

namespace WindowsAudioSource.Extensions
{
    public static class EnumerableExtensions
    {
        public static TGroupElement FirstInGroupOrDefault<TGroupKey, TGroupElement>(this IEnumerable<IGrouping<TGroupKey, TGroupElement>> groupedElements, TGroupKey key)
            where TGroupKey : struct
        {
            var keyGroup = groupedElements.FirstOrDefault(group => group.Key.Equals(key));
            if (keyGroup == null)
            {
                return default;
            }
            return keyGroup.FirstOrDefault();
        }
    }
}
