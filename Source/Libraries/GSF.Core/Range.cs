﻿//******************************************************************************************************
//  Range.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/30/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace GSF
{
    /// <summary>
    /// Represents a range of values with a start and end value.
    /// </summary>
    public class Range<T>
    {
        #region [ Members ]

        // Fields
        private readonly T m_start;
        private readonly T m_end;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="Range{T}"/> class using the default comparer.
        /// </summary>
        /// <param name="start">The start value of the range.</param>
        /// <param name="end">The end value of the range.</param>
        public Range(T start, T end)
        {
            m_start = start;
            m_end = end;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the start value of the range.
        /// </summary>
        public T Start => m_start;

        /// <summary>
        /// Gets the end value of the range.
        /// </summary>
        public T End => m_end;

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines whether the range overlaps with the given range.
        /// </summary>
        /// <param name="range">The range to be compared with.</param>
        /// <returns>True if the ranges overlap; false otherwise.</returns>
        public bool Overlaps(Range<T> range)
        {
            return Overlaps(range, Comparer<T>.Default);
        }

        /// <summary>
        /// Determines whether the range overlaps with the given range.
        /// </summary>
        /// <param name="range">The range to be compared with.</param>
        /// <param name="comparer">The comparer used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>True if the ranges overlap; false otherwise.</returns>
        public bool Overlaps(Range<T> range, IComparer<T> comparer)
        {
            return Overlaps(range, comparer.Compare);
        }

        /// <summary>
        /// Determines whether the range overlaps with the given range.
        /// </summary>
        /// <param name="range">The range to be compared with.</param>
        /// <param name="comparison">The comparison used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>True if the ranges overlap; false otherwise.</returns>
        public bool Overlaps(Range<T> range, Comparison<T> comparison)
        {
            return comparison(m_start, range.m_end) <= 0 && comparison(range.m_start, m_end) <= 0;
        }

        /// <summary>
        /// Merges two ranges into one range that fully encompasses both ranges.
        /// </summary>
        /// <param name="range">The range to be merged with this one.</param>
        /// <returns>The range that fully encompasses the merged ranges.</returns>
        public Range<T> Merge(Range<T> range)
        {
            return Merge(range, Comparer<T>.Default);
        }

        /// <summary>
        /// Merges two ranges into one range that fully encompasses both ranges.
        /// </summary>
        /// <param name="range">The range to be merged with this one.</param>
        /// <param name="comparer">The comparer used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>The range that fully encompasses the merged ranges.</returns>
        public Range<T> Merge(Range<T> range, IComparer<T> comparer)
        {
            return Merge(range, comparer.Compare);
        }

        /// <summary>
        /// Merges two ranges into one range that fully encompasses both ranges.
        /// </summary>
        /// <param name="range">The range to be merged with this one.</param>
        /// <param name="comparison">The comparison used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>The range that fully encompasses the merged ranges.</returns>
        public Range<T> Merge(Range<T> range, Comparison<T> comparison)
        {
            T start = comparison(m_start, range.m_start) <= 0 ? m_start : range.m_start;
            T end = comparison(m_end, range.m_end) >= 0 ? m_end : range.m_end;
            return new Range<T>(start, end);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Merges all ranges in a collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges to be merged.</param>
        /// <returns>A range that is the result of merging all ranges in the collection.</returns>
        public static Range<T> Merge(IEnumerable<Range<T>> ranges)
        {
            return Merge(ranges, Comparer<T>.Default);
        }

        /// <summary>
        /// Merges all ranges in a collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges to be merged.</param>
        /// <param name="comparer">The comparer used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>A range that is the result of merging all ranges in the collection.</returns>
        public static Range<T> Merge(IEnumerable<Range<T>> ranges, Comparer<T> comparer)
        {
            return Merge(ranges, comparer.Compare);
        }

        /// <summary>
        /// Merges all ranges in a collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges to be merged.</param>
        /// <param name="comparison">The comparison used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>A range that is the result of merging all ranges in the collection.</returns>
        public static Range<T> Merge(IEnumerable<Range<T>> ranges, Comparison<T> comparison)
        {
            return ranges.Aggregate((range1, range2) => range1.Merge(range2, comparison));
        }

        /// <summary>
        /// Merges all consecutive groups of overlapping ranges in a
        /// collection and returns the resulting collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges.</param>
        /// <returns>The collection of merged ranges.</returns>
        public static IEnumerable<Range<T>> MergeConsecutiveOverlapping(IEnumerable<Range<T>> ranges)
        {
            return MergeConsecutiveOverlapping(ranges, Comparer<T>.Default);
        }

        /// <summary>
        /// Merges all consecutive groups of overlapping ranges in a
        /// collection and returns the resulting collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges.</param>
        /// <param name="comparer">The comparer used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>The collection of merged ranges.</returns>
        public static IEnumerable<Range<T>> MergeConsecutiveOverlapping(IEnumerable<Range<T>> ranges, IComparer<T> comparer)
        {
            return MergeConsecutiveOverlapping(ranges, comparer.Compare);
        }

        /// <summary>
        /// Merges all consecutive groups of overlapping ranges in a
        /// collection and returns the resulting collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges.</param>
        /// <param name="comparison">The comparison used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>The collection of merged ranges.</returns>
        public static IEnumerable<Range<T>> MergeConsecutiveOverlapping(IEnumerable<Range<T>> ranges, Comparison<T> comparison)
        {
            Range<T> currentRange = null;

            foreach (Range<T> range in ranges)
            {
                if ((object)currentRange == null)
                {
                    currentRange = range;
                }
                else if (currentRange.Overlaps(range, comparison))
                {
                    currentRange = currentRange.Merge(range, comparison);
                }
                else
                {
                    yield return currentRange;
                    currentRange = range;
                }
            }

            if ((object)currentRange != null)
                yield return currentRange;
        }

        /// <summary>
        /// Merges all consecutive groups of overlapping ranges in a
        /// collection and returns the resulting collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges.</param>
        /// <returns>The collection of merged ranges.</returns>
        /// <remarks>This method does not preserve the order of the source collection.</remarks>
        public static IEnumerable<Range<T>> MergeAllOverlapping(IEnumerable<Range<T>> ranges)
        {
            return MergeAllOverlapping(ranges, Comparer<T>.Default);
        }

        /// <summary>
        /// Merges all consecutive groups of overlapping ranges in a
        /// collection and returns the resulting collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges.</param>
        /// <param name="comparison">The comparison used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>The collection of merged ranges.</returns>
        /// <remarks>This method does not preserve the order of the source collection.</remarks>
        public static IEnumerable<Range<T>> MergeAllOverlapping(IEnumerable<Range<T>> ranges, Comparison<T> comparison)
        {
            return MergeAllOverlapping(ranges, Comparer<T>.Create(comparison));
        }

        /// <summary>
        /// Merges all consecutive groups of overlapping ranges in a
        /// collection and returns the resulting collection of ranges.
        /// </summary>
        /// <param name="ranges">The collection of ranges.</param>
        /// <param name="comparer">The comparer used to compare objects of type <typeparamref name="T"/>.</param>
        /// <returns>The collection of merged ranges.</returns>
        /// <remarks>This method does not preserve the order of the source collection.</remarks>
        public static IEnumerable<Range<T>> MergeAllOverlapping(IEnumerable<Range<T>> ranges, IComparer<T> comparer)
        {
            return MergeConsecutiveOverlapping(ranges.OrderBy(range => range.Start, comparer));
        }

        #endregion
    }
}
