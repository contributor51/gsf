//******************************************************************************************************
//  Outage.cs - Gbtc
//
//  Copyright � 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/24/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace GSF.IO
{
    /// <summary>
    /// Represents an outage as a start time and an end time.
    /// </summary>
    public class Outage : Range<DateTimeOffset>
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="Outage"/> with the same start and end time as the given range.
        /// </summary>
        public Outage(Range<DateTimeOffset> range)
            : this(range.Start, range.End)
        {
        }

        /// <summary>
        /// Creates a new <see cref="Outage"/> with the specified start and end time.
        /// </summary>
        /// <param name="startTime">Start time for outage.</param>
        /// <param name="endTime">End time for outage.</param>
        public Outage(DateTimeOffset startTime, DateTimeOffset endTime)
            : base(startTime, endTime)
        {
            if (startTime > endTime)
                throw new ArgumentException("Outage start time is past end time");
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Merges all consecutive groups of overlapping ranges in a
        /// collection and returns the resulting collection of outages.
        /// </summary>
        /// <param name="outages">The collection of outages.</param>
        /// <returns>The collection of merged outages.</returns>
        /// <remarks>This method does not preserve the order of the source collection.</remarks>
        public static IEnumerable<Outage> MergeOverlapping(IEnumerable<Outage> outages)
        {
            return MergeAllOverlapping(outages).Select(range => new Outage(range));
        }

        #endregion
    }
}