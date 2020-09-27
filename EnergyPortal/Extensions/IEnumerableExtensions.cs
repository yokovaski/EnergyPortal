using System;
using System.Collections.Generic;

namespace EnergyPortal.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TResult> SelectWithPrevious<TSource, TResult>
        (this IEnumerable<TSource> source,
            Func<TSource, TSource, TResult> projection)
        {
            using var iterator = source.GetEnumerator();
            
            if (!iterator.MoveNext())
            {
                yield break;
            }
            
            var previous = iterator.Current;
            
            while (iterator.MoveNext())
            {
                yield return projection(previous, iterator.Current);
                previous = iterator.Current;
            }
        }
        
        public static IEnumerable<TResult> SelectWithNextOrDefault<TSource, TResult>
        (this IEnumerable<TSource> source,
            Func<TSource, TSource, TResult> projection)
        {
            using var iterator = source.GetEnumerator();
            
            if (!iterator.MoveNext())
            {
                yield break;
            }
            
            var current = iterator.Current;
            
            while (iterator.MoveNext())
            {
                var next = iterator.Current;
                yield return projection(current, next);
                current = next;
            }

            yield return projection(current, default);
        }
    }
}