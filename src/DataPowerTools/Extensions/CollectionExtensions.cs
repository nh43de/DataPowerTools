using System;
using System.Collections.Generic;
using System.Linq;

namespace DataPowerTools.Extensions;

public static class CollectionExtensions
{
    ///// <summary>
    ///// Take a source and merges it into the target.
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="source"></param>
    ///// <param name="target"></param>
    ///// <param name="matchPredicate">Source, target</param>
    ///// <param name="matchedAction">The merge match condition. Parameters: Source, target </param>
    ///// <param name="addToTargetAction">Parameter: Source item to add to target</param>
    ///// <param name="removeFromTargetAction">Parameter: Target item to remove</param>
    //public static void Merge<T>(
    //    this ICollection<T> target,
    //    ICollection<T> source,
    //    Func<T, T, bool> matchPredicate,
    //    Action<T, T> matchedAction,
    //    Action<T> addToTargetAction = null,
    //    Action<T> removeFromTargetAction = null)
    //{
    //    //for each item in target, find the source item and merge.
    //    foreach (var targetItem in target)
    //    {
    //        var sourceItem = source.FirstOrDefault(p => matchPredicate(p, targetItem));
    //        if (sourceItem != null)
    //        {
    //            matchedAction(sourceItem, targetItem);
    //        }
    //        else if (removeFromTargetAction != null) //no matching item present in source, so notify removal from the target
    //        {
    //            removeFromTargetAction(targetItem);
    //        }
    //    }

    //    //iterate through source and notify items not present in target
    //    if (addToTargetAction != null)
    //    {
    //        foreach (var sourceItem in source)
    //        {
    //            if (!target.Any(item => matchPredicate(sourceItem, item)))
    //            {
    //                addToTargetAction(sourceItem);
    //            }
    //        }
    //    }

    //    //itemsToRemove.Add(sourceItem);

    //    //foreach (var item in itemsToAdd)
    //    //{
    //    //    source.Add(item);
    //    //}

    //    //foreach (var item in itemsToRemove)
    //    //{
    //    //    source.Remove(item);
    //    //}
    //}
}
