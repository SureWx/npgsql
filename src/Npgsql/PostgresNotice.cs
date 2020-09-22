﻿using System;
using JetBrains.Annotations;
using Npgsql.BackendMessages;

namespace Npgsql
{
    /// <summary>
    /// PostgreSQL notices are non-critical messages generated by PostgreSQL, either as a result of a user query
    /// (e.g. as a warning or informational notice), or due to outside activity (e.g. if the database administrator
    /// initiates a "fast" database shutdown).
    /// </summary>
    /// <remarks>
    /// https://www.postgresql.org/docs/current/static/protocol-flow.html#PROTOCOL-ASYNC
    /// </remarks>
    public sealed class PostgresNotice
    {
        #region Message Fields

        /// <summary>
        /// Severity of the error or notice.
        /// Always present.
        /// </summary>
        [PublicAPI]
        public string Severity { get; set; }

        /// <summary>
        /// Severity of the error or notice, not localized.
        /// Always present since PostgreSQL 9.6.
        /// </summary>
        [PublicAPI]
        public string InvariantSeverity { get; }

        /// <summary>
        /// The SQLSTATE code for the error.
        /// </summary>
        /// <remarks>
        /// Always present.
        /// See https://www.postgresql.org/docs/current/static/errcodes-appendix.html
        /// </remarks>
        [PublicAPI]
        public string SqlState { get; set; }

        /// <summary>
        /// The SQLSTATE code for the error.
        /// </summary>
        /// <remarks>
        /// Always present.
        /// See https://www.postgresql.org/docs/current/static/errcodes-appendix.html
        /// </remarks>
        [PublicAPI, Obsolete("Use SqlState instead")]
        public string Code => SqlState;

        /// <summary>
        /// The primary human-readable error message. This should be accurate but terse.
        /// </summary>
        /// <remarks>
        /// Always present.
        /// </remarks>
        [PublicAPI]
        public string MessageText { get; set; }

        /// <summary>
        /// An optional secondary error message carrying more detail about the problem.
        /// May run to multiple lines.
        /// </summary>
        [PublicAPI]
        public string? Detail { get; set; }

        /// <summary>
        /// An optional suggestion what to do about the problem.
        /// This is intended to differ from Detail in that it offers advice (potentially inappropriate) rather than hard facts.
        /// May run to multiple lines.
        /// </summary>
        [PublicAPI]
        public string? Hint { get; set; }

        /// <summary>
        /// The field value is a decimal ASCII integer, indicating an error cursor position as an index into the original query string.
        /// The first character has index 1, and positions are measured in characters not bytes.
        /// 0 means not provided.
        /// </summary>
        [PublicAPI]
        public int Position { get; set; }

        /// <summary>
        /// This is defined the same as the <see cref="Position"/> field, but it is used when the cursor position refers to an internally generated command rather than the one submitted by the client.
        /// The <see cref="InternalQuery" /> field will always appear when this field appears.
        /// 0 means not provided.
        /// </summary>
        [PublicAPI]
        public int InternalPosition { get; set; }

        /// <summary>
        /// The text of a failed internally-generated command.
        /// This could be, for example, a SQL query issued by a PL/pgSQL function.
        /// </summary>
        [PublicAPI]
        public string? InternalQuery { get; set; }

        /// <summary>
        /// An indication of the context in which the error occurred.
        /// Presently this includes a call stack traceback of active PL functions.
        /// The trace is one entry per line, most recent first.
        /// </summary>
        [PublicAPI]
        public string? Where { get; set; }

        /// <summary>
        /// If the error was associated with a specific database object, the name of the schema containing that object, if any.
        /// </summary>
        /// <remarks>PostgreSQL 9.3 and up.</remarks>
        [PublicAPI]
        public string? SchemaName { get; set; }

        /// <summary>
        /// Table name: if the error was associated with a specific table, the name of the table.
        /// (Refer to the schema name field for the name of the table's schema.)
        /// </summary>
        /// <remarks>PostgreSQL 9.3 and up.</remarks>
        [PublicAPI]
        public string? TableName { get; set; }

        /// <summary>
        /// If the error was associated with a specific table column, the name of the column.
        /// (Refer to the schema and table name fields to identify the table.)
        /// </summary>
        /// <remarks>PostgreSQL 9.3 and up.</remarks>
        [PublicAPI]
        public string? ColumnName { get; set; }

        /// <summary>
        /// If the error was associated with a specific data type, the name of the data type.
        /// (Refer to the schema name field for the name of the data type's schema.)
        /// </summary>
        /// <remarks>PostgreSQL 9.3 and up.</remarks>
        [PublicAPI]
        public string? DataTypeName { get; set; }

        /// <summary>
        /// If the error was associated with a specific constraint, the name of the constraint.
        /// Refer to fields listed above for the associated table or domain.
        /// (For this purpose, indexes are treated as constraints, even if they weren't created with constraint syntax.)
        /// </summary>
        /// <remarks>PostgreSQL 9.3 and up.</remarks>
        [PublicAPI]
        public string? ConstraintName { get; set; }

        /// <summary>
        /// The file name of the source-code location where the error was reported.
        /// </summary>
        /// <remarks>PostgreSQL 9.3 and up.</remarks>
        [PublicAPI]
        public string? File { get; set; }

        /// <summary>
        /// The line number of the source-code location where the error was reported.
        /// </summary>
        [PublicAPI]
        public string? Line { get; set; }

        /// <summary>
        /// The name of the source-code routine reporting the error.
        /// </summary>
        [PublicAPI]
        public string? Routine { get; set; }

        #endregion

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public PostgresNotice(string severity, string invariantSeverity, string sqlState, string messageText)
            : this(messageText, severity, invariantSeverity, sqlState, detail: null) {}

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public PostgresNotice(
            string messageText, string severity, string invariantSeverity, string sqlState,
            string? detail = null, string? hint = null, int position = 0, int internalPosition = 0,
            string? internalQuery = null, string? where = null, string? schemaName = null, string? tableName = null,
            string? columnName = null, string? dataTypeName = null, string? constraintName = null, string? file = null,
            string? line = null, string? routine = null)
        {
            MessageText = messageText;
            Severity = severity;
            InvariantSeverity = invariantSeverity;
            SqlState = sqlState;

            Detail = detail;
            Hint = hint;
            Position = position;
            InternalPosition = internalPosition;
            InternalQuery = internalQuery;
            Where = where;
            SchemaName = schemaName;
            TableName = tableName;
            ColumnName = columnName;
            DataTypeName = dataTypeName;
            ConstraintName = constraintName;
            File = file;
            Line = line;
            Routine = routine;
        }

        PostgresNotice(ErrorOrNoticeMessage msg)
            : this(
                msg.Message, msg.Severity, msg.InvariantSeverity, msg.SqlState,
                msg.Detail, msg.Hint, msg.Position, msg.InternalPosition, msg.InternalQuery,
                msg.Where, msg.SchemaName, msg.TableName, msg.ColumnName, msg.DataTypeName,
                msg.ConstraintName, msg.File, msg.Line, msg.Routine) {}

        internal static PostgresNotice Load(NpgsqlReadBuffer buf, bool includeDetail)
            => new PostgresNotice(ErrorOrNoticeMessage.Load(buf, includeDetail));
    }

    /// <summary>
    /// Provides data for a PostgreSQL notice event.
    /// </summary>
    public sealed class NpgsqlNoticeEventArgs : EventArgs
    {
        /// <summary>
        /// The Notice that was sent from the database.
        /// </summary>
        public PostgresNotice Notice { get; }

        internal NpgsqlNoticeEventArgs(PostgresNotice notice)
        {
            Notice = notice;
        }
    }
}
