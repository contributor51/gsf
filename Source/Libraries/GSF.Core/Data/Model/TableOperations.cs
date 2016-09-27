﻿//******************************************************************************************************
//  TableOperations.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/01/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using GSF.Collections;
using GSF.Reflection;

// ReSharper disable StaticMemberInGenericType
namespace GSF.Data.Model
{
    /// <summary>
    /// Defines database operations for a modeled table.
    /// </summary>
    /// <typeparam name="T">Modeled table.</typeparam>
    public class TableOperations<T> : ITableOperations where T : class, new()
    {
        #region [ Members ]

        // Constants
        private const string CountSqlFormat = "SELECT COUNT(*) FROM {0}";
        private const string OrderBySqlFormat = "SELECT {0} FROM {1} ORDER BY {{0}}";
        private const string OrderByWhereSqlFormat = "SELECT {0} FROM {1} WHERE {{0}} ORDER BY {{1}}";
        private const string SelectSqlFormat = "SELECT * FROM {0} WHERE {1}";
        private const string AddNewSqlFormat = "INSERT INTO {0}({1}) VALUES ({2})";
        private const string UpdateSqlFormat = "UPDATE {0} SET {1} WHERE {2}";
        private const string DeleteSqlFormat = "DELETE FROM {0} WHERE {1}";

        // Fields
        private readonly AdoDataConnection m_connection;
        private Action<Exception> m_exceptionHandler;
        private IEnumerable<DataRow> m_primaryKeyCache;
        private string m_lastSortField;
        private bool m_useCaseSensitiveFieldNames;
        private readonly string m_countSql;
        private readonly string m_orderBySql;
        private readonly string m_orderByWhereSql;
        private readonly string m_selectSql;
        private readonly string m_addNewSql;
        private readonly string m_updateSql;
        private readonly string m_updateWhereSql;
        private readonly string m_deleteSql;
        private readonly string m_deleteWhereSql;
        private readonly string m_searchFilterSql;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="TableOperations{T}"/>.
        /// </summary>
        /// <param name="connection"><see cref="AdoDataConnection"/> instance to use for database operations.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> cannot be <c>null</c>.</exception>
        public TableOperations(AdoDataConnection connection)
        {
            if ((object)connection == null)
                throw new ArgumentNullException(nameof(connection));

            m_connection = connection;
            m_countSql = s_countSql;
            m_orderBySql = s_orderBySql;
            m_orderByWhereSql = s_orderByWhereSql;
            m_selectSql = s_selectSql;
            m_addNewSql = s_addNewSql;
            m_updateSql = s_updateSql;
            m_updateWhereSql = s_updateWhereSql;
            m_deleteSql = s_deleteSql;
            m_deleteWhereSql = s_deleteWhereSql;
            m_searchFilterSql = s_searchFilterSql;

            // When any escape targets are defined for the modeled identifiers, i.e., table or field names,
            // the static SQL statements are defined with ANSI standard escape delimiters. We check if the
            // user model has opted to instead use common database escape delimiters, or no delimiters, that
            // will apply to the active database type and make any needed adjustments. As a result, it will
            // be slightly faster to construct this class when ANSI standard escape delimiters are used.
            if ((object)s_escapedTableNameTargets != null)
            {
                string derivedTableName = GetEscapedTableName();
                string ansiEscapedTableName = $"\"{s_tableName}\"";

                if (!derivedTableName.Equals(ansiEscapedTableName))
                {
                    m_countSql = m_countSql.Replace(ansiEscapedTableName, derivedTableName);
                    m_orderBySql = m_orderBySql.Replace(ansiEscapedTableName, derivedTableName);
                    m_orderByWhereSql = m_orderByWhereSql.Replace(ansiEscapedTableName, derivedTableName);
                    m_selectSql = m_selectSql.Replace(ansiEscapedTableName, derivedTableName);
                    m_addNewSql = m_addNewSql.Replace(ansiEscapedTableName, derivedTableName);
                    m_updateSql = m_updateSql.Replace(ansiEscapedTableName, derivedTableName);
                    m_updateWhereSql = m_updateWhereSql.Replace(ansiEscapedTableName, derivedTableName);
                    m_deleteSql = m_deleteSql.Replace(ansiEscapedTableName, derivedTableName);
                    m_deleteWhereSql = m_deleteWhereSql.Replace(ansiEscapedTableName, derivedTableName);
                }
            }

            if ((object)s_escapedFieldNameTargets != null)
            {
                foreach (KeyValuePair<string, Dictionary<DatabaseType, bool>> escapedFieldNameTarget in s_escapedFieldNameTargets)
                {
                    string fieldName = escapedFieldNameTarget.Key;
                    string derivedFieldName = GetEscapedFieldName(fieldName, escapedFieldNameTarget.Value);
                    string ansiEscapedFieldName = $"\"{fieldName}\"";

                    if (!derivedFieldName.Equals(ansiEscapedFieldName))
                    {
                        m_orderBySql = m_orderBySql.Replace(ansiEscapedFieldName, derivedFieldName);
                        m_orderByWhereSql = m_orderByWhereSql.Replace(ansiEscapedFieldName, derivedFieldName);
                        m_selectSql = m_selectSql.Replace(ansiEscapedFieldName, derivedFieldName);
                        m_addNewSql = m_addNewSql.Replace(ansiEscapedFieldName, derivedFieldName);
                        m_updateSql = m_updateSql.Replace(ansiEscapedFieldName, derivedFieldName);
                        m_updateWhereSql = m_updateWhereSql.Replace(ansiEscapedFieldName, derivedFieldName);
                        m_deleteSql = m_deleteSql.Replace(ansiEscapedFieldName, derivedFieldName);
                        m_deleteWhereSql = m_deleteWhereSql.Replace(ansiEscapedFieldName, derivedFieldName);
                        m_searchFilterSql = m_searchFilterSql.Replace(ansiEscapedFieldName, derivedFieldName);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="TableOperations{T}"/>.
        /// </summary>
        /// <param name="connection"><see cref="AdoDataConnection"/> instance to use for database operations.</param>
        /// <param name="exceptionHandler">Delegate to handle table operation exceptions.</param>
        /// <remarks>
        /// When exception handler is provided, table operations will not throw exceptions for database calls, any
        /// encountered exceptions will be passed to handler for processing.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> cannot be <c>null</c>.</exception>
        public TableOperations(AdoDataConnection connection, Action<Exception> exceptionHandler) : this(connection)
        {
            m_exceptionHandler = exceptionHandler;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the table name defined for the modeled table, includes any escaping as defined in model.
        /// </summary>
        public string TableName => GetEscapedTableName();

        /// <summary>
        /// Gets the table name defined for the modeled table without any escape characters.
        /// </summary>
        /// <remarks>
        /// A table name will only be escaped if the model requested escaping with the <see cref="UseEscapedNameAttribute"/>.
        /// </remarks>
        public string UnescapedTableName => s_tableName;

        /// <summary>
        /// Gets flag that determines if modeled table has a primary key that is an identity field.
        /// </summary>
        public bool HasPrimaryKeyIdentityField => s_hasPrimaryKeyIdentityField;

        /// <summary>
        /// Gets or sets delegate used to handle table operation exceptions.
        /// </summary>
        /// <remarks>
        /// When exception handler is provided, table operations will not throw exceptions for database calls, any
        /// encountered exceptions will be passed to handler for processing. Otherwise, exceptions will be thrown
        /// on the call stack.
        /// </remarks>
        public Action<Exception> ExceptionHandler
        {
            get
            {
                return m_exceptionHandler;
            }
            set
            {
                m_exceptionHandler = value;
            }
        }

        /// <summary>
        /// Gets or sets flag that determines if field names should be treated as case sensitive. Defaults to <c>false</c>.
        /// </summary>
        /// <remarks>
        /// In cases where modeled table fields have applied <see cref="UseEscapedNameAttribute"/>, this flag will be used
        /// to properly update escaped field names that may be case sensitive.
        /// </remarks>
        public bool UseCaseSensitiveFieldNames
        {
            get
            {
                return m_useCaseSensitiveFieldNames;
            }
            set
            {
                m_useCaseSensitiveFieldNames = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Queries database and returns modeled table records for the specified parameters.
        /// </summary>
        /// <param name="orderByExpression">Field name expression used for sort order, include ASC or DESC as needed - does not include ORDER BY; defaults to primary keys.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <param name="limit">Limit of number of record to return.</param>
        /// <returns>An enumerable of modeled table row instances for queried records.</returns>
        /// <remarks>
        /// If no record <paramref name="restriction"/> or <paramref name="limit"/> is provided, all rows will be returned.
        /// </remarks>
        public IEnumerable<T> QueryRecords(string orderByExpression = null, RecordRestriction restriction = null, int limit = -1)
        {
            if (string.IsNullOrWhiteSpace(orderByExpression))
                orderByExpression = s_primaryKeyFields;

            string sqlExpression = null;

            try
            {
                if (limit < 1)
                {
                    // No record limit specified
                    if ((object)restriction == null)
                    {
                        sqlExpression = string.Format(m_orderBySql, orderByExpression);
                        return m_connection.RetrieveData(sqlExpression).AsEnumerable().Select(row => LoadRecord(GetPrimaryKeys(row)));
                    }

                    sqlExpression = string.Format(m_orderByWhereSql, UpdateFieldNames(restriction.FilterExpression), orderByExpression);
                    return m_connection.RetrieveData(sqlExpression, restriction.Parameters).AsEnumerable().Select(row => LoadRecord(GetPrimaryKeys(row)));
                }

                if ((object)restriction == null)
                {
                    sqlExpression = string.Format(m_orderBySql, orderByExpression);
                    return m_connection.RetrieveData(sqlExpression).AsEnumerable().Take(limit).Select(row => LoadRecord(GetPrimaryKeys(row)));
                }

                sqlExpression = string.Format(m_orderByWhereSql, UpdateFieldNames(restriction.FilterExpression), orderByExpression);
                return m_connection.RetrieveData(sqlExpression, restriction.Parameters).AsEnumerable().Take(limit).Select(row => LoadRecord(GetPrimaryKeys(row)));
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception during record query for {typeof(T).Name} \"{sqlExpression ?? "undefined"}, {ValueList(restriction?.Parameters)}\": {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return Enumerable.Empty<T>();
            }
        }

        IEnumerable ITableOperations.QueryRecords(string orderByExpression, RecordRestriction restriction, int limit)
        {
            return QueryRecords(orderByExpression, restriction, limit);
        }

        /// <summary>
        /// Queries database and returns modeled table records for the specified sorting, paging and search parameters.
        /// </summary>
        /// <param name="sortField">Field name to order-by.</param>
        /// <param name="ascending">Sort ascending flag; set to <c>false</c> for descending.</param>
        /// <param name="page">Page number of records to return (1-based).</param>
        /// <param name="pageSize">Current page size.</param>
        /// <param name="searchText">Text to search.</param>
        /// <returns>An enumerable of modeled table row instances for queried records.</returns>
        /// <remarks>
        /// This function is used for record paging. Primary keys are cached server-side, typically per user session, to maintain desired per-page sort order.
        /// </remarks>
        public IEnumerable<T> QueryRecords(string sortField, bool ascending, int page, int pageSize, string searchText)
        {
            return QueryRecords(sortField, ascending, page, pageSize, GetSearchRestriction(searchText));
        }

        IEnumerable ITableOperations.QueryRecords(string sortField, bool ascending, int page, int pageSize, string searchText)
        {
            return QueryRecords(sortField, ascending, page, pageSize, searchText);
        }

        /// <summary>
        /// Queries database and returns modeled table records for the specified sorting and paging parameters.
        /// </summary>
        /// <param name="sortField">Field name to order-by.</param>
        /// <param name="ascending">Sort ascending flag; set to <c>false</c> for descending.</param>
        /// <param name="page">Page number of records to return (1-based).</param>
        /// <param name="pageSize">Current page size.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <returns>An enumerable of modeled table row instances for queried records.</returns>
        /// <remarks>
        /// This function is used for record paging. Primary keys are cached server-side, typically per user session, to maintain desired per-page sort order.
        /// </remarks>
        public IEnumerable<T> QueryRecords(string sortField, bool ascending, int page, int pageSize, RecordRestriction restriction = null)
        {
            if (string.IsNullOrWhiteSpace(sortField))
                sortField = s_fieldNames[s_primaryKeyProperties[0].Name];

            if ((object)m_primaryKeyCache == null || string.Compare(sortField, m_lastSortField, StringComparison.OrdinalIgnoreCase) != 0)
            {
                string orderByExpression = $"{sortField}{(ascending ? "" : " DESC")}";
                string sqlExpression = null;

                try
                {
                    if ((object)restriction == null)
                    {
                        sqlExpression = string.Format(m_orderBySql, orderByExpression);
                        m_primaryKeyCache = m_connection.RetrieveData(sqlExpression).AsEnumerable();
                    }
                    else
                    {
                        sqlExpression = string.Format(m_orderByWhereSql, UpdateFieldNames(restriction.FilterExpression), orderByExpression);
                        m_primaryKeyCache = m_connection.RetrieveData(sqlExpression, restriction.Parameters).AsEnumerable();
                    }

                }
                catch (Exception ex)
                {
                    InvalidOperationException opex = new InvalidOperationException($"Exception during record query for {typeof(T).Name} \"{sqlExpression ?? "undefined"}, {ValueList(restriction?.Parameters)}\": {ex.Message}", ex);

                    if ((object)m_exceptionHandler == null)
                        throw opex;

                    m_exceptionHandler(opex);
                    return Enumerable.Empty<T>();
                }

                m_lastSortField = sortField;
            }

            return m_primaryKeyCache.ToPagedList(page, pageSize).Select(row => LoadRecord(row.ItemArray)).Where(record => record != null);
        }

        IEnumerable ITableOperations.QueryRecords(string sortField, bool ascending, int page, int pageSize, RecordRestriction restriction)
        {
            return QueryRecords(sortField, ascending, page, pageSize, restriction);
        }

        /// <summary>
        /// Gets the total record count for the modeled table based on search parameter.
        /// </summary>
        /// <param name="searchText">Text to search.</param>
        /// <returns>Total record count for the modeled table.</returns>
        public int QueryRecordCount(string searchText)
        {
            return QueryRecordCount(GetSearchRestriction(searchText));
        }

        /// <summary>
        /// Gets the total record count for the modeled table.
        /// </summary>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <returns>Total record count for the modeled table.</returns>
        public int QueryRecordCount(RecordRestriction restriction = null)
        {
            string sqlExpression = null;

            try
            {
                if ((object)restriction == null)
                {
                    sqlExpression = m_countSql;
                    return m_connection.ExecuteScalar<int>(sqlExpression);
                }

                sqlExpression = $"{m_countSql} WHERE {UpdateFieldNames(restriction.FilterExpression)}";
                return m_connection.ExecuteScalar<int>(sqlExpression, restriction.Parameters);
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception during record count query for {typeof(T).Name} \"{sqlExpression ?? "undefined"}, {ValueList(restriction?.Parameters)}\": {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return -1;
            }
        }

        /// <summary>
        /// Creates a new modeled table record queried from the specified <paramref name="primaryKeys"/>.
        /// </summary>
        /// <param name="primaryKeys">Primary keys values of the record to load.</param>
        /// <returns>New modeled table record queried from the specified <paramref name="primaryKeys"/>.</returns>
        public T LoadRecord(params object[] primaryKeys)
        {
            try
            {
                return LoadRecord(m_connection.RetrieveRow(m_selectSql, primaryKeys));
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception during record load for {typeof(T).Name} \"{m_selectSql}, {ValueList(primaryKeys)}\": {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return null;
            }
        }

        object ITableOperations.LoadRecord(params object[] primaryKeys)
        {
            return LoadRecord(primaryKeys);
        }

        /// <summary>
        /// Creates a new modeled table record queried from the specified <paramref name="row"/>.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> of queried data to be loaded.</param>
        /// <returns>New modeled table record queried from the specified <paramref name="row"/>.</returns>
        public T LoadRecord(DataRow row)
        {
            try
            {
                T record = new T();

                // Make sure record exists, if not return null instead of a blank record
                if (s_hasPrimaryKeyIdentityField && GetPrimaryKeys(row).All(Common.IsDefaultValue))
                    return null;

                foreach (PropertyInfo property in s_properties.Values)
                {
                    try
                    {
                        property.SetValue(record, row.ConvertField(s_fieldNames[property.Name], property.PropertyType), null);
                    }
                    catch (Exception ex)
                    {
                        InvalidOperationException opex = new InvalidOperationException($"Exception during record load field assignment for \"{typeof(T).Name}.{property.Name} = {row[s_fieldNames[property.Name]]}\": {ex.Message}", ex);

                        if ((object)m_exceptionHandler == null)
                            throw opex;

                        m_exceptionHandler(opex);
                    }
                }

                return record;
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception during record load for {typeof(T).Name} from data row: {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return null;
            }
        }

        object ITableOperations.LoadRecord(DataRow row)
        {
            return LoadRecord(row);
        }

        /// <summary>
        /// Deletes the record referenced by the specified <paramref name="primaryKeys"/>.
        /// </summary>
        /// <param name="primaryKeys">Primary keys values of the record to load.</param>
        /// <returns>Number of rows affected.</returns>
        public int DeleteRecord(params object[] primaryKeys)
        {
            try
            {
                int affectedRecords = m_connection.ExecuteNonQuery(m_deleteSql, primaryKeys);

                if (affectedRecords > 0)
                    m_primaryKeyCache = null;

                return affectedRecords;
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception during record delete for {typeof(T).Name} \"{m_deleteSql}, {ValueList(primaryKeys)}\": {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return 0;
            }
        }

        /// <summary>
        /// Deletes the specified modeled table <paramref name="record"/> from the database.
        /// </summary>
        /// <param name="record">Record to delete.</param>
        /// <returns>Number of rows affected.</returns>
        public int DeleteRecord(T record)
        {
            return DeleteRecord(GetPrimaryKeys(record));
        }

        int ITableOperations.DeleteRecord(object value)
        {
            T record = value as T;

            if (record == null)
                throw new ArgumentException($"Cannot delete record of type \"{value?.GetType().Name ?? "null"}\", expected \"{typeof(T).Name}\"", nameof(value));

            return DeleteRecord(record);
        }

        /// <summary>
        /// Deletes the record referenced by the specified <paramref name="row"/>.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> of queried data to be deleted.</param>
        /// <returns>Number of rows affected.</returns>
        public int DeleteRecord(DataRow row)
        {
            return DeleteRecord(GetPrimaryKeys(row));
        }

        /// <summary>
        /// Deletes the records referenced by the specified <paramref name="restriction"/>.
        /// </summary>
        /// <param name="restriction">Record restriction to apply</param>
        /// <returns>Number of rows affected.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="restriction"/> cannot be <c>null</c>.</exception>
        public int DeleteRecord(RecordRestriction restriction)
        {
            if ((object)restriction == null)
                throw new ArgumentNullException(nameof(restriction));

            string sqlExpression = null;

            try
            {
                sqlExpression = $"{m_deleteWhereSql}{UpdateFieldNames(restriction.FilterExpression)}";
                int affectedRecords = m_connection.ExecuteNonQuery(sqlExpression, restriction.Parameters);

                if (affectedRecords > 0)
                    m_primaryKeyCache = null;

                return affectedRecords;
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception during record delete for {typeof(T).Name} \"{sqlExpression ?? "undefined"}, {ValueList(restriction.Parameters)}\": {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return 0;
            }
        }

        /// <summary>
        /// Updates the database with the specified modeled table <paramref name="record"/>.
        /// </summary>
        /// <param name="record">Record to update.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <returns>Number of rows affected.</returns>
        /// <remarks>
        /// Record restriction is only used for custom update expressions or in cases where modeled
        /// table has no defined primary keys.
        /// </remarks>
        public int UpdateRecord(T record, RecordRestriction restriction = null)
        {
            List<object> values = new List<object>();

            if ((object)restriction == null)
            {
                try
                {
                    foreach (PropertyInfo property in s_updateProperties)
                        values.Add(property.GetValue(record));

                    foreach (PropertyInfo property in s_primaryKeyProperties)
                        values.Add(property.GetValue(record));

                    return m_connection.ExecuteNonQuery(m_updateSql, values.ToArray());
                }
                catch (Exception ex)
                {
                    InvalidOperationException opex = new InvalidOperationException($"Exception during record update for {typeof(T).Name} \"{m_updateSql}, {ValueList(values)}\": {ex.Message}", ex);

                    if ((object)m_exceptionHandler == null)
                        throw opex;

                    m_exceptionHandler(opex);
                    return 0;
                }
            }

            string sqlExpression = null;

            try
            {
                foreach (PropertyInfo property in s_updateProperties)
                    values.Add(property.GetValue(record));

                values.AddRange(restriction.Parameters);

                List<object> updateWhereOffsets = new List<object>();
                int updateFieldIndex = s_updateProperties.Length;

                for (int i = 0; i < restriction.Parameters.Length; i++)
                    updateWhereOffsets.Add($"{{{updateFieldIndex + i}}}");

                sqlExpression = $"{m_updateWhereSql}{string.Format(UpdateFieldNames(restriction.FilterExpression), updateWhereOffsets.ToArray())}";
                return m_connection.ExecuteNonQuery(sqlExpression, values.ToArray());
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception during record update for {typeof(T).Name} \"{sqlExpression}, {ValueList(values)}\": {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return 0;
            }
        }

        int ITableOperations.UpdateRecord(object value, RecordRestriction restriction)
        {
            T record = value as T;

            if (record == null)
                throw new ArgumentException($"Cannot update record of type \"{value?.GetType().Name ?? "null"}\", expected \"{typeof(T).Name}\"", nameof(value));

            return UpdateRecord(record);
        }

        /// <summary>
        /// Updates the database with the specified <paramref name="row"/>.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> of queried data to be updated.</param>
        /// <param name="restriction">Record restriction to apply, if any.</param>
        /// <returns>Number of rows affected.</returns>
        /// <remarks>
        /// Record restriction is only used for custom update expressions or in cases where modeled
        /// table has no defined primary keys.
        /// </remarks>
        public int UpdateRecord(DataRow row, RecordRestriction restriction = null)
        {
            return UpdateRecord(LoadRecord(row), restriction);
        }

        /// <summary>
        /// Adds the specified modeled table <paramref name="record"/> to the database.
        /// </summary>
        /// <param name="record">Record to add.</param>
        /// <returns>Number of rows affected.</returns>
        public int AddNewRecord(T record)
        {
            List<object> values = new List<object>();

            try
            {
                foreach (PropertyInfo property in s_addNewProperties)
                    values.Add(property.GetValue(record));

                int affectedRecords = m_connection.ExecuteNonQuery(m_addNewSql, values.ToArray());

                if (affectedRecords > 0)
                    m_primaryKeyCache = null;

                return affectedRecords;
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception during record insert for {typeof(T).Name} \"{m_addNewSql}, {ValueList(values)}\": {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return 0;
            }
        }

        int ITableOperations.AddNewRecord(object value)
        {
            T record = value as T;

            if (record == null)
                throw new ArgumentException($"Cannot add new record of type \"{value?.GetType().Name ?? "null"}\", expected \"{typeof(T).Name}\"", nameof(value));

            return AddNewRecord(record);
        }

        /// <summary>
        /// Adds the specified <paramref name="row"/> to the database.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> of queried data to be added.</param>
        /// <returns>Number of rows affected.</returns>
        public int AddNewRecord(DataRow row)
        {
            return AddNewRecord(LoadRecord(row));
        }

        /// <summary>
        /// Gets the primary key values from the specified <paramref name="record"/>.
        /// </summary>
        /// <param name="record">Record of data to retrieve primary keys from.</param>
        /// <returns>Primary key values from the specified <paramref name="record"/>.</returns>
        public object[] GetPrimaryKeys(T record)
        {
            try
            {
                List<object> values = new List<object>();

                foreach (PropertyInfo property in s_primaryKeyProperties)
                    values.Add(property.GetValue(record));

                return values.ToArray();
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception loading primary key fields for {typeof(T).Name} \"{s_primaryKeyProperties.Select(property => property.Name).ToDelimitedString(", ")}\": {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return new object[0];
            }
        }

        object[] ITableOperations.GetPrimaryKeys(object value)
        {
            T record = value as T;

            if (record == null)
                throw new ArgumentException($"Cannot get primary keys for record of type \"{value?.GetType().Name ?? "null"}\", expected \"{typeof(T).Name}\"", nameof(value));

            return GetPrimaryKeys(record);
        }

        /// <summary>
        /// Gets the primary key values from the specified <paramref name="row"/>.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> of queried data.</param>
        /// <returns>Primary key values from the specified <paramref name="row"/>.</returns>
        public object[] GetPrimaryKeys(DataRow row)
        {
            try
            {
                List<object> values = new List<object>();

                foreach (PropertyInfo property in s_primaryKeyProperties)
                    values.Add(row[s_fieldNames[property.Name]]);

                return values.ToArray();
            }
            catch (Exception ex)
            {
                InvalidOperationException opex = new InvalidOperationException($"Exception loading primary key fields for {typeof(T).Name} \"{s_primaryKeyProperties.Select(property => property.Name).ToDelimitedString(", ")}\": {ex.Message}", ex);

                if ((object)m_exceptionHandler == null)
                    throw opex;

                m_exceptionHandler(opex);
                return new object[0];
            }
        }

        /// <summary>
        /// Gets the field names for the table; if <paramref name="escaped"/> is <c>true</c>, also includes any escaping as defined in model.
        /// </summary>
        /// <param name="escaped">Flag that determines if field names should include any escaping as defined in the model; defaults to <c>true</c>.</param>
        /// <returns>Array of field names.</returns>
        /// <remarks>
        /// A field name will only be escaped if the model requested escaping with the <see cref="UseEscapedNameAttribute"/>.
        /// </remarks>
        public string[] GetFieldNames(bool escaped = true)
        {
            if (escaped)
                return s_fieldNames.Values.Select(fieldName => GetEscapedFieldName(fieldName)).ToArray();

            // Fields in the field names dictionary are stored in unescaped format
            return s_fieldNames.Values.ToArray();
        }

        /// <summary>
        /// Get the primary key field names for the table; if <paramref name="escaped"/> is <c>true</c>, also includes any escaping as defined in model.
        /// </summary>
        /// <param name="escaped">Flag that determines if field names should include any escaping as defined in the model; defaults to <c>true</c>.</param>
        /// <returns>Array of primary key field names.</returns>
        /// <remarks>
        /// A field name will only be escaped if the model requested escaping with the <see cref="UseEscapedNameAttribute"/>.
        /// </remarks>
        public string[] GetPrimaryKeyFieldNames(bool escaped = true)
        {
            if (escaped)
                return s_primaryKeyFields.Split(',').Select(fieldName => GetEscapedFieldName(fieldName.Trim())).ToArray();

            return s_primaryKeyFields.Split(',').Select(fieldName => GetUnescapedFieldName(fieldName.Trim())).ToArray();
        }

        /// <summary>
        /// Attempts to get the specified <paramref name="attribute"/> for a field.
        /// </summary>
        /// <typeparam name="TAttribute">Type of attribute to attempt to get.</typeparam>
        /// <param name="fieldName">Name of field to use for attribute lookup.</param>
        /// <param name="attribute">Attribute that was found, if any.</param>
        /// <returns><c>true</c> if attribute was found; otherwise, <c>false</c>.</returns>
        public bool TryGetFieldAttribute<TAttribute>(string fieldName, out TAttribute attribute) where TAttribute : Attribute
        {
            string propertyName;
            PropertyInfo property;

            if (s_propertyNames.TryGetValue(fieldName, out propertyName) && s_properties.TryGetValue(propertyName, out property) && property.TryGetAttribute(out attribute))
                return true;

            attribute = default(TAttribute);
            return false;
        }

        /// <summary>
        /// Attempts to get the specified <paramref name="attributeType"/> for a field.
        /// </summary>
        /// <param name="fieldName">Name of field to use for attribute lookup.</param>
        /// <param name="attributeType">Type of attribute to attempt to get.</param>
        /// <param name="attribute">Attribute that was found, if any.</param>
        /// <returns><c>true</c> if attribute was found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException"><paramref name="attributeType"/> is not an <see cref="Attribute"/>.</exception>
        public bool TryGetFieldAttribute(string fieldName, Type attributeType, out Attribute attribute)
        {
            string propertyName;
            PropertyInfo property;

            if (!attributeType.IsInstanceOfType(typeof(Attribute)))
                throw new ArgumentException($"The specified type \"{attributeType.Name}\" is not an Attribute.", nameof(attributeType));

            if (s_propertyNames.TryGetValue(fieldName, out propertyName) && s_properties.TryGetValue(propertyName, out property) && property.TryGetAttribute(attributeType, out attribute))
                return true;

            attribute = null;
            return false;
        }

        /// <summary>
        /// Determines if the specified field has an associated attribute.
        /// </summary>
        /// <typeparam name="TAttribute">Type of attribute to search for.</typeparam>
        /// <param name="fieldName">Name of field to use for attribute lookup.</param>
        /// <returns><c>true</c> if field has attribute; otherwise, <c>false</c>.</returns>
        public bool FieldHasAttribute<TAttribute>(string fieldName) where TAttribute : Attribute
        {
            return FieldHasAttribute(fieldName, typeof(TAttribute));
        }

        /// <summary>
        /// Determines if the specified field has an associated attribute.
        /// </summary>
        /// <param name="fieldName">Name of field to use for attribute lookup.</param>
        /// <param name="attributeType">Type of attribute to search for.</param>
        /// <returns><c>true</c> if field has attribute; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException"><paramref name="attributeType"/> is not an <see cref="Attribute"/>.</exception>
        public bool FieldHasAttribute(string fieldName, Type attributeType)
        {
            string propertyName;
            PropertyInfo property;
            HashSet<Type> attributes;

            if (!attributeType.IsSubclassOf(typeof(Attribute)))
                throw new ArgumentException($"The specified type \"{attributeType.Name}\" is not an Attribute.", nameof(attributeType));

            if (s_propertyNames.TryGetValue(fieldName, out propertyName) && s_properties.TryGetValue(propertyName, out property) && s_attributes.TryGetValue(property, out attributes))
                return attributes.Contains(attributeType);

            return false;
        }

        /// <summary>
        /// Gets the value for the specified field.
        /// </summary>
        /// <param name="record">Modeled table record.</param>
        /// <param name="fieldName">Field name to retrieve.</param>
        /// <returns>Field value or <c>null</c> if field is not found.</returns>
        public object GetFieldValue(T record, string fieldName)
        {
            string propertyName;
            PropertyInfo property;

            if (s_propertyNames.TryGetValue(fieldName, out propertyName) && s_properties.TryGetValue(propertyName, out property))
                return property.GetValue(record);

            return null;
        }

        object ITableOperations.GetFieldValue(object value, string fieldName)
        {
            T record = value as T;

            if (record == null)
                throw new ArgumentException($"Cannot get \"{fieldName}\" field value for record of type \"{value?.GetType().Name ?? "null"}\", expected \"{typeof(T).Name}\"", nameof(value));

            return GetFieldValue(record, fieldName);
        }

        /// <summary>
        /// Gets the <see cref="Type"/> for the specified field.
        /// </summary>
        /// <param name="fieldName">Field name to retrieve.</param>
        /// <returns>Field <see cref="Type"/> or <c>null</c> if field is not found.</returns>
        public Type GetFieldType(string fieldName)
        {
            string propertyName;
            PropertyInfo property;

            if (s_propertyNames.TryGetValue(fieldName, out propertyName) && s_properties.TryGetValue(propertyName, out property))
                return property.PropertyType;

            return null;
        }

        /// <summary>
        /// Generates a <see cref="RecordRestriction"/> based on fields marked with <see cref="SearchableAttribute"/> and specified <paramref name="searchText"/>.
        /// </summary>
        /// <param name="searchText">Text to search.</param>
        /// <returns><see cref="RecordRestriction"/> based on fields marked with <see cref="SearchableAttribute"/> and specified <paramref name="searchText"/>.</returns>
        public RecordRestriction GetSearchRestriction(string searchText)
        {
            if (string.IsNullOrWhiteSpace(m_searchFilterSql) || string.IsNullOrWhiteSpace(searchText))
                return null;

            searchText = searchText.Trim();

            string[] keyWords = searchText.RemoveDuplicateWhiteSpace().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (keyWords.Length == 1)
                return new RecordRestriction(m_searchFilterSql, $"%{searchText}%", searchText);

            StringBuilder multiKeyWordFilter = new StringBuilder();

            for (int i = 0; i < keyWords.Length * 2; i+=2)
            {
                if (i > 0)
                    multiKeyWordFilter.Append(" AND ");

                multiKeyWordFilter.Append('(');
                multiKeyWordFilter.AppendFormat(m_searchFilterSql, $"{{{i}}}", $"{{{i + 1}}}");
                multiKeyWordFilter.Append(')');
            }

            return new RecordRestriction(multiKeyWordFilter.ToString(), keyWords.SelectMany(keyWord => new object[] { $"%{keyWord}%", keyWord }).ToArray());
        }

        // Derive table name, escaping it if requested by model
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetEscapedTableName()
        {
            if ((object)s_escapedTableNameTargets == null)
                return s_tableName;

            bool useAnsiQuotes;

            if (s_escapedTableNameTargets.TryGetValue(m_connection.DatabaseType, out useAnsiQuotes))
                return m_connection.EscapeIdentifier(s_tableName, useAnsiQuotes);

            return s_tableName;
        }

        // Derive field name, escaping it if requested by model
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetEscapedFieldName(string fieldName, Dictionary<DatabaseType, bool> escapedFieldNameTargets = null)
        {
            if ((object)s_escapedFieldNameTargets == null)
                return fieldName;

            if ((object)escapedFieldNameTargets == null && !s_escapedFieldNameTargets.TryGetValue(fieldName, out escapedFieldNameTargets))
                return fieldName;

            bool useAnsiQuotes;

            if (escapedFieldNameTargets.TryGetValue(m_connection.DatabaseType, out useAnsiQuotes))
                return m_connection.EscapeIdentifier(fieldName, useAnsiQuotes);

            return fieldName;
        }

        // Derive field name, unescaping it if it was escaped by the model
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetUnescapedFieldName(string fieldName)
        {
            if ((object)s_escapedFieldNameTargets == null)
                return fieldName;

            Dictionary<DatabaseType, bool> escapedFieldNameTargets;

            if (!s_escapedFieldNameTargets.TryGetValue(fieldName, out escapedFieldNameTargets))
                return fieldName;

            return fieldName.Substring(1, fieldName.Length - 2);
        }

        // Update field names in expression, escaping or unescaping as needed as defined by model
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string UpdateFieldNames(string filterExpression)
        {
            if ((object)s_escapedFieldNameTargets != null)
            {
                foreach (KeyValuePair<string, Dictionary<DatabaseType, bool>> escapedFieldNameTarget in s_escapedFieldNameTargets)
                {
                    string fieldName = escapedFieldNameTarget.Key;
                    string derivedFieldName = GetEscapedFieldName(fieldName, escapedFieldNameTarget.Value);
                    string ansiEscapedFieldName = $"\"{fieldName}\"";

                    if (m_useCaseSensitiveFieldNames)
                    {
                        if (!derivedFieldName.Equals(ansiEscapedFieldName))
                            filterExpression = filterExpression.Replace(ansiEscapedFieldName, derivedFieldName);
                    }
                    else
                    {
                        if (!derivedFieldName.Equals(ansiEscapedFieldName, StringComparison.OrdinalIgnoreCase))
                            filterExpression = filterExpression.ReplaceCaseInsensitive(ansiEscapedFieldName, derivedFieldName);
                    }
                }
            }

            return filterExpression;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly string s_tableName;
        private static readonly Dictionary<string, PropertyInfo> s_properties;
        private static readonly Dictionary<string, string> s_fieldNames;
        private static readonly Dictionary<string, string> s_propertyNames;
        private static readonly Dictionary<PropertyInfo, HashSet<Type>> s_attributes;
        private static readonly PropertyInfo[] s_addNewProperties;
        private static readonly PropertyInfo[] s_updateProperties;
        private static readonly PropertyInfo[] s_primaryKeyProperties;
        private static readonly Dictionary<DatabaseType, bool> s_escapedTableNameTargets;
        private static readonly Dictionary<string, Dictionary<DatabaseType, bool>> s_escapedFieldNameTargets;
        private static readonly string s_countSql;
        private static readonly string s_orderBySql;
        private static readonly string s_orderByWhereSql;
        private static readonly string s_selectSql;
        private static readonly string s_addNewSql;
        private static readonly string s_updateSql;
        private static readonly string s_updateWhereSql;
        private static readonly string s_deleteSql;
        private static readonly string s_deleteWhereSql;
        private static readonly string s_primaryKeyFields;
        private static readonly string s_searchFilterSql;
        private static readonly bool s_hasPrimaryKeyIdentityField;

        // Static Constructor
        static TableOperations()
        {
            StringBuilder addNewFields = new StringBuilder();
            StringBuilder addNewFormat = new StringBuilder();
            StringBuilder updateFormat = new StringBuilder();
            StringBuilder whereFormat = new StringBuilder();
            StringBuilder primaryKeyFields = new StringBuilder();
            StringBuilder searchFilterSql = new StringBuilder();
            List<PropertyInfo> addNewProperties = new List<PropertyInfo>();
            List<PropertyInfo> updateProperties = new List<PropertyInfo>();
            List<PropertyInfo> primaryKeyProperties = new List<PropertyInfo>();
            int primaryKeyIndex = 0;
            int addNewFieldIndex = 0;
            int updateFieldIndex = 0;

            // Table name will default to class name of modeled table
            s_tableName = typeof(T).Name;

            // Check for overridden table name
            TableNameAttribute tableNameAttribute;

            if (typeof(T).TryGetAttribute(out tableNameAttribute) && !string.IsNullOrWhiteSpace(tableNameAttribute.TableName))
                s_tableName = tableNameAttribute.TableName;

            // Check for escaped table name targets
            UseEscapedNameAttribute[] useEscapedNameAttributes;

            if (typeof(T).TryGetAttributes(out useEscapedNameAttributes))
                s_escapedTableNameTargets = DeriveEscapedNameTargets(useEscapedNameAttributes);

            s_properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.CanRead && property.CanWrite)
                .ToDictionary(property => property.Name, StringComparer.OrdinalIgnoreCase);

            s_fieldNames = s_properties.ToDictionary(kvp => kvp.Key, kvp => GetFieldName(kvp.Value), StringComparer.OrdinalIgnoreCase);
            s_propertyNames = s_fieldNames.ToDictionary(kvp => kvp.Value, kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);
            s_attributes = new Dictionary<PropertyInfo, HashSet<Type>>();
            s_hasPrimaryKeyIdentityField = false;

            foreach (PropertyInfo property in s_properties.Values)
            {
                string fieldName = s_fieldNames[property.Name];
                PrimaryKeyAttribute primaryKeyAttribute;
                SearchableAttribute searchableAttribute;

                property.TryGetAttribute(out primaryKeyAttribute);
                property.TryGetAttribute(out searchableAttribute);

                if (property.TryGetAttributes(out useEscapedNameAttributes))
                {
                    if ((object)s_escapedFieldNameTargets == null)
                        s_escapedFieldNameTargets = new Dictionary<string, Dictionary<DatabaseType, bool>>(StringComparer.OrdinalIgnoreCase);

                    s_escapedFieldNameTargets[fieldName] = DeriveEscapedNameTargets(useEscapedNameAttributes);

                    // If any database has been targeted for escaping the field name, pre-apply the standard ANSI escaped
                    // field name in the static SQL expressions. This will provide a unique replaceable identifier should
                    // the common database delimiters, or no delimiters, be applicable for an active database connection
                    fieldName = $"\"{fieldName}\"";
                }

                if ((object)primaryKeyAttribute != null)
                {
                    if (primaryKeyAttribute.IsIdentity)
                    {
                        s_hasPrimaryKeyIdentityField = true;
                    }
                    else
                    {
                        addNewFields.Append($"{(addNewFields.Length > 0 ? ", " : "")}{fieldName}");
                        addNewFormat.Append($"{(addNewFormat.Length > 0 ? ", " : "")}{{{addNewFieldIndex++}}}");
                        addNewProperties.Add(property);
                    }

                    whereFormat.Append($"{(whereFormat.Length > 0 ? " AND " : "")}{fieldName}={{{primaryKeyIndex++}}}");
                    primaryKeyFields.Append($"{(primaryKeyFields.Length > 0 ? ", " : "")}{fieldName}");
                    primaryKeyProperties.Add(property);
                }
                else
                {
                    addNewFields.Append($"{(addNewFields.Length > 0 ? ", " : "")}{fieldName}");
                    addNewFormat.Append($"{(addNewFormat.Length > 0 ? ", " : "")}{{{addNewFieldIndex++}}}");
                    updateFormat.Append($"{(updateFormat.Length > 0 ? ", " : "")}{fieldName}={{{updateFieldIndex++}}}");
                    addNewProperties.Add(property);
                    updateProperties.Add(property);
                }

                if ((object)searchableAttribute != null)
                {
                    if (searchFilterSql.Length > 0)
                        searchFilterSql.Append(" OR ");

                    if (searchableAttribute.SearchType == SearchType.Default)
                    {
                        if (property.PropertyType == typeof(string))
                            searchFilterSql.Append($"{fieldName} LIKE {{0}}");
                        else
                            searchFilterSql.Append($"{fieldName}={{1}}");
                    }
                    else
                    {
                        if (searchableAttribute.SearchType == SearchType.LikeExpression)
                            searchFilterSql.Append($"{fieldName} LIKE {{0}}");
                        else
                            searchFilterSql.Append($"{fieldName}={{1}}");
                    }
                }

                s_attributes.Add(property, new HashSet<Type>(property.CustomAttributes.Select(attributeData => attributeData.AttributeType)));
            }

            // Have to assume all fields are primary when none are specified
            if (primaryKeyProperties.Count == 0)
            {
                foreach (PropertyInfo property in s_properties.Values)
                {
                    string fieldName = s_fieldNames[property.Name];

                    if (s_escapedFieldNameTargets?.ContainsKey(fieldName) ?? false)
                        fieldName = $"\"{fieldName}\"";

                    whereFormat.Append($"{(whereFormat.Length > 0 ? " AND " : "")}{fieldName}={{{primaryKeyIndex++}}}");
                    primaryKeyFields.Append($"{(primaryKeyFields.Length > 0 ? ", " : "")}{fieldName}");
                    primaryKeyProperties.Add(property);
                }

                s_primaryKeyFields = primaryKeyFields.ToString();

                // Default to all
                primaryKeyFields.Clear();
                primaryKeyFields.Append("*");
            }
            else
            {
                s_primaryKeyFields = primaryKeyFields.ToString();
            }

            List<object> updateWhereOffsets = new List<object>();

            for (int i = 0; i < primaryKeyIndex; i++)
                updateWhereOffsets.Add($"{{{updateFieldIndex + i}}}");

            // If any database has been targeted for escaping the table name, pre-apply the standard ANSI escaped
            // table name in the static SQL expressions. This will provide a unique replaceable identifier should
            // the common database delimiters, or no delimiters, be applicable for an active database connection
            string tableName = s_tableName;

            if ((object)s_escapedTableNameTargets != null)
                tableName = $"\"{tableName}\"";

            s_countSql = string.Format(CountSqlFormat, tableName);
            s_orderBySql = string.Format(OrderBySqlFormat, primaryKeyFields, tableName);
            s_orderByWhereSql = string.Format(OrderByWhereSqlFormat, primaryKeyFields, tableName);
            s_selectSql = string.Format(SelectSqlFormat, tableName, whereFormat);
            s_addNewSql = string.Format(AddNewSqlFormat, tableName, addNewFields, addNewFormat);
            s_updateSql = string.Format(UpdateSqlFormat, tableName, updateFormat, string.Format(whereFormat.ToString(), updateWhereOffsets.ToArray()));
            s_deleteSql = string.Format(DeleteSqlFormat, tableName, whereFormat);
            s_updateWhereSql = s_updateSql.Substring(0, s_updateSql.IndexOf(" WHERE ", StringComparison.Ordinal) + 7);
            s_deleteWhereSql = s_deleteSql.Substring(0, s_deleteSql.IndexOf(" WHERE ", StringComparison.Ordinal) + 7);
            s_searchFilterSql = searchFilterSql.ToString();

            s_addNewProperties = addNewProperties.ToArray();
            s_updateProperties = updateProperties.ToArray();
            s_primaryKeyProperties = primaryKeyProperties.ToArray();
        }

        // Static Methods
        private static string GetFieldName(PropertyInfo property)
        {
            FieldNameAttribute fieldNameAttribute;

            if (property.TryGetAttribute(out fieldNameAttribute) && !string.IsNullOrEmpty(fieldNameAttribute?.FieldName))
                return fieldNameAttribute.FieldName;

            return property.Name;
        }

        private static Dictionary<DatabaseType, bool> DeriveEscapedNameTargets(UseEscapedNameAttribute[] useEscapedNameAttributes)
        {
            if (useEscapedNameAttributes == null || useEscapedNameAttributes.Length == 0)
                return null;

            DatabaseType[] databaseTypes;
            bool allDatabasesTargeted = false;

            // If any attribute has no database target type specified, then all database types are assumed
            if (useEscapedNameAttributes.Any(attribute => attribute.TargetDatabaseType == null))
            {
                allDatabasesTargeted = true;
                databaseTypes = Enum.GetValues(typeof(DatabaseType)).Cast<DatabaseType>().ToArray();
            }
            else
            {
                databaseTypes = useEscapedNameAttributes.Select(attribute => attribute.TargetDatabaseType.GetValueOrDefault()).Distinct().ToArray();
            }

            Dictionary<DatabaseType, bool> escapedNameTargets = new Dictionary<DatabaseType, bool>(databaseTypes.Length);

            foreach (DatabaseType databaseType in databaseTypes)
            {
                UseEscapedNameAttribute useEscapedNameAttribute = useEscapedNameAttributes.FirstOrDefault(attribute => attribute.TargetDatabaseType == databaseType);
                bool useAnsiQuotes = ((object)useEscapedNameAttribute != null && useEscapedNameAttribute.UseAnsiQuotes) || (allDatabasesTargeted && databaseType != DatabaseType.MySQL);
                escapedNameTargets[databaseType] = useAnsiQuotes;
            }

            return escapedNameTargets;
        }

        private static string ValueList(IReadOnlyList<object> values)
        {
            if (values == null)
                return "";

            StringBuilder delimitedString = new StringBuilder();


            for (int i = 0; i < values.Count; i++)
            {
                if (delimitedString.Length > 0)
                    delimitedString.Append(", ");

                delimitedString.AppendFormat("{0}:{1}", i, values[i]);
            }

            return delimitedString.ToString();
        }

        #endregion
    }
}
