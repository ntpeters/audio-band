using System.Collections.Generic;
using System.Linq;

namespace WindowsAudioSource.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Returns the first element in the first group of a sequence matching the given <paramref name="key"/>, or a default value if the sequence is empty, no matching group is found, or the matching group is empty.
        /// </summary>
        /// <typeparam name="TGroupKey">Type of the key of the <see cref="IGrouping{TKey, TElement}"/> items in the sequence.</typeparam>
        /// <typeparam name="TGroupElement">Type of the values of the <see cref="IGrouping{TKey, TElement}"/> items in the sequence.</typeparam>
        /// <param name="groupedElements">An <see cref="IEnumerable{T}"/> of <see cref="IGrouping{TKey, TElement}"/></param>
        /// <param name="key">Key of the <see cref="IGrouping{TKey, TElement}"/> to get the first element of in the sequence.</param>
        /// <returns>
        /// <see langword="default"/>(<typeparamref name="TGroupElement"/>) if <paramref name="groupedElements"/> is empty, does not contain a group matching the <paramref name="key"/>, or the matching group is empty; otherwise, the first element in the first group matching the <paramref name="key"/>.
        /// </returns>
        public static TGroupElement FirstInGroupOrDefault<TGroupKey, TGroupElement>(this IEnumerable<IGrouping<TGroupKey, TGroupElement>> groupedElements, TGroupKey key)
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
