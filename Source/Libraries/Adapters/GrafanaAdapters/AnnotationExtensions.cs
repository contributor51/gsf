//******************************************************************************************************
//  AnnotationExtensions.cs - Gbtc
//
//  Copyright � 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  09/22/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GSF.TimeSeries;
using GSF.TimeSeries.Adapters;

namespace GrafanaAdapters
{
    /// <summary>
    /// Grafana <see cref="Annotation"/> extensions class.
    /// </summary>
    public static class AnnotationExtensions
    {
        /// <summary>
        /// Gets table name for specified annotation <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Annotation type.</param>
        /// <returns>Table name for specified annotation <paramref name="type"/>.</returns>
        public static string TableName(this AnnotationType type)
        {
            switch (type)
            {
                case AnnotationType.RaisedAlarms:
                    return "RaisedAlarms";
                case AnnotationType.ClearedAlarms:
                    return "ClearedAlarms";
                default:
                    return "Undefined";
            }
        }

        private static string Translate(this string tableName)
        {
            // Source metadata for raised and cleared alarms is the same
            if (tableName.Equals("RaisedAlarms", StringComparison.OrdinalIgnoreCase))
                return "Alarms";

            if (tableName.Equals("ClearedAlarms", StringComparison.OrdinalIgnoreCase))
                return "Alarms";

            return tableName;
        }

        /// <summary>
        /// Gets the target field name for Guid based point IDs for table used with specified annotation <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Annotation type.</param>
        /// <returns>Target field name for Guid based point IDs for specified annotation <paramref name="type"/>.</returns>
        public static string TargetFieldName(this AnnotationType type)
        {
            switch (type)
            {
                case AnnotationType.RaisedAlarms:
                case AnnotationType.ClearedAlarms:
                    return "AssociatedMeasurementID";
                default:
                    throw new InvalidOperationException("Cannot extract target for specified annotation type.");
            }
        }

        /// <summary>
        /// Determines if the data point is applicable for specified annotation <paramref name="type"/>. 
        /// </summary>
        /// <param name="type">Annotation type.</param>
        /// <param name="datapoint">Time series values data point.</param>
        /// <returns><c>true</c> if the data point is applicable for specified annotation <paramref name="type"/>; otherwise, <c>false</c>.</returns>
        public static bool IsApplicable(this AnnotationType type, double[] datapoint)
        {
            if ((object)datapoint == null)
                throw new ArgumentNullException(nameof(datapoint));

            double value = datapoint[TimeSeriesValues.Value];

            switch (type)
            {
                case AnnotationType.RaisedAlarms:
                    return value != 0.0D;
                case AnnotationType.ClearedAlarms:
                    return value == 0.0D;
                default:
                    throw new InvalidOperationException("Cannot determine data point applicability for specified annotation type.");
            }
        }
        /// <summary>
        /// Populates an annotation response title, text and tags for specified annotation <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Annotation type.</param>
        /// <param name="response">Annotation response.</param>
        /// <param name="target">Target of annotation response.</param>
        /// <param name="definition">Associated metadata definition for response.</param>
        /// <param name="datapoint">Time series values data point for response.</param>
        /// <returns>Populates an annotation response title, text and tags for specified annotation <paramref name="type"/>.</returns>
        public static void PopulateResponse(this AnnotationType type, AnnotationResponse response, string target, DataRow definition, double[] datapoint)
        {
            if ((object)response == null)
                throw new ArgumentNullException(nameof(response));

            if ((object)target == null)
                throw new ArgumentNullException(nameof(target));

            if ((object)definition == null)
                throw new ArgumentNullException(nameof(definition));

            if ((object)datapoint == null)
                throw new ArgumentNullException(nameof(datapoint));

            switch (type)
            {
                case AnnotationType.RaisedAlarms:
                case AnnotationType.ClearedAlarms:
                    response.title = $"Alarm {(type == AnnotationType.RaisedAlarms ? "Raised" : "Cleared")}";
                    response.text = $"{definition["Description"]}<br>Severity = {definition["Severity"]}<br>{definition["TagName"]}";
                    response.tags = $"{GetTargetFromGuid(definition["SignalID"].ToString())}, {target}";
                    break;
                default:
                    throw new InvalidOperationException("Cannot populate response information for specified annotation type.");
            }
        }

        /// <summary>
        /// Extracts a Grafana <see cref="QueryRequest"/> from an <see cref="AnnotationRequest"/>.
        /// </summary>
        /// <param name="request">Annotation request.</param>
        /// <param name="targets">List of desired targets.</param>
        /// <param name="maxDataPoints">Maximum points to return.</param>
        /// <returns>Grafana query request object from an annotation <paramref name="request"/>.</returns>
        public static QueryRequest ExtractQueryRequest(this AnnotationRequest request, IEnumerable<string> targets, int maxDataPoints)
        {
            if ((object)targets == null)
                throw new ArgumentNullException(nameof(targets));

            return new QueryRequest
            {
                range = request.range,
                rangeRaw = request.rangeRaw,
                interval = "*",
                targets = targets.Select((target, index) => new Target { refId = $"ID{index}", target = target }).ToList(),
                format = "json",
                maxDataPoints = maxDataPoints
            };
        }

        /// <summary>
        /// Parses query expression from annotation for annotation type.
        /// </summary>
        /// <param name="annotation">Grafana annotation.</param>
        /// <param name="useFilterExpression">Determines if query is using a filter expression.</param>
        /// <returns>Parsed annotation type for query expression from <paramref name="annotation"/>.</returns>
        public static AnnotationType ParseQueryType(this Annotation annotation, out bool useFilterExpression)
        {
            if ((object)annotation == null)
                throw new ArgumentNullException(nameof(annotation));

            useFilterExpression = false;

            AnnotationType type = AnnotationType.Undefined;
            string tableName, expression, sortField, query = annotation.query ?? "";
            int takeCount;

            if (AdapterBase.ParseFilterExpression(query, out tableName, out expression, out sortField, out takeCount))
            {
                useFilterExpression = true;

                switch (tableName.ToUpperInvariant())
                {
                    case "RAISEDALARMS":
                        type = AnnotationType.RaisedAlarms;
                        break;
                    case "CLEAREDALARMS":
                        type = AnnotationType.ClearedAlarms;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid FILTER table for annotation query expression.");
                }
            }
            else if (query.StartsWith("#RaisedAlarms", StringComparison.OrdinalIgnoreCase))
            {
                type = AnnotationType.RaisedAlarms;
            }
            else if (query.StartsWith("#ClearedAlarms", StringComparison.OrdinalIgnoreCase))
            {
                type = AnnotationType.ClearedAlarms;
            }

            if (type == AnnotationType.Undefined)
                throw new InvalidOperationException("Unrecognized type or syntax for annotation query expression.");

            return type;
        }

        /// <summary>
        /// Parses query expression from annotation request for annotation type.
        /// </summary>
        /// <param name="request">Grafana annotation request.</param>
        /// <param name="useFilterExpression">Determines if query is using a filter expression.</param>
        /// <returns>Parsed annotation type for query expression from annotation <paramref name="request"/>.</returns>
        public static AnnotationType ParseQueryType(this AnnotationRequest request, out bool useFilterExpression)
        {
            if ((object)request == null)
                throw new ArgumentNullException(nameof(request));

            return request.annotation.ParseQueryType(out useFilterExpression);
        }

        /// <summary>
        /// Parses source definitions for an annotation query.
        /// </summary>
        /// <param name="annotation">Grafana annotation.</param>
        /// <param name="type">Annotation type.</param>
        /// <param name="source">Metadata of source definitions.</param>
        /// <param name="useFilterExpression">Determines if query is using a filter expression.</param>
        /// <returns>Parsed source definitions from <paramref name="annotation"/>.</returns>
        public static Dictionary<string, DataRow> ParseSourceDefinitions(this Annotation annotation, AnnotationType type, DataSet source, bool useFilterExpression)
        {
            if ((object)annotation == null)
                throw new ArgumentNullException(nameof(annotation));

            if ((object)source == null)
                throw new ArgumentNullException(nameof(source));

            if (type == AnnotationType.Undefined)
                throw new InvalidOperationException("Unrecognized type or syntax for annotation query expression.");

            string query = annotation.query ?? "";
            DataRow[] rows;

            if (useFilterExpression)
            {
                string tableName, expression, sortField;
                int takeCount;

                if (AdapterBase.ParseFilterExpression(query, out tableName, out expression, out sortField, out takeCount))
                    rows = source.Tables[tableName.Translate()].Select(expression, sortField).Take(takeCount).ToArray();
                else
                    throw new InvalidOperationException("Invalid FILTER syntax for annotation query expression.");
            }
            else
            {
                // Assume all records if no filter expression was provided
                rows = source.Tables[type.TableName().Translate()].Rows.Cast<DataRow>().ToArray();
            }

            Dictionary<string, DataRow> definitions = new Dictionary<string, DataRow>(StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in rows)
            {
                MeasurementKey key = GetTargetFromGuid(row[type.TargetFieldName()].ToString());

                if (key != MeasurementKey.Undefined)
                    definitions[key.ID.ToString()] = row;
            }

            return definitions;
        }

        /// <summary>
        /// Parses source definitions for an annotation query.
        /// </summary>
        /// <param name="request">Grafana annotation request.</param>
        /// <param name="type">Annotation type.</param>
        /// <param name="source">Metadata of source definitions.</param>
        /// <param name="useFilterExpression">Determines if query is using a filter expression.</param>
        /// <returns>Parsed source definitions from annotation <paramref name="request"/>.</returns>
        public static Dictionary<string, DataRow> ParseSourceDefinitions(this AnnotationRequest request, AnnotationType type, DataSet source, bool useFilterExpression)
        {
            if ((object)request == null)
                throw new ArgumentNullException(nameof(request));

            return request.annotation.ParseSourceDefinitions(type, source, useFilterExpression);
        }

        private static MeasurementKey GetTargetFromGuid(string guidID) => MeasurementKey.LookUpBySignalID(Guid.Parse(guidID));
    }
}