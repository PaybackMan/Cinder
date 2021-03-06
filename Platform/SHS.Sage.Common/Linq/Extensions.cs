﻿using SHS.Sage.Linq.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public static class Extensions
    {
        ///<summary>Finds the index of the first item matching an expression in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="predicate">The expression to test the items against.</param>
        ///<returns>The index of the first matching item, or -1 if no items match.</returns>
        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            int retVal = 0;
            foreach (var item in items)
            {
                if (predicate(item)) return retVal;
                retVal++;
            }
            return -1;
        }
        ///<summary>Finds the index of the first occurence of an item in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="item">The item to find.</param>
        ///<returns>The index of the first matching item, or -1 if the item was not found.</returns>
        public static int IndexOf<T>(this IEnumerable<T> items, T item)
        {
            return items.FindIndex(i => EqualityComparer<T>.Default.Equals(item, i));
        }
        /// <summary>
        /// Finds the index of the first occurence of an item in an enumerable after an initial offset.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="item"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int IndexOf<T>(this IEnumerable<T> items, T item, int startIndex)
        {
            var foundIndex = items.Skip(startIndex).FindIndex(i => EqualityComparer<T>.Default.Equals(item, i));
            if (foundIndex == -1) return -1;
            else return foundIndex + startIndex;
        }
    }
}
