namespace Venflow
{
    /// <inheritdoc cref="NpgsqlTypes.NpgsqlDbType"/>
    public static class VenflowDbType
    {
        // Note that it's important to never change the numeric values of this enum, since user applications
        // compile them in.

        #region Numeric Types

        /// <summary>
        /// Corresponds to the PostgreSQL 8-byte "bigint" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-numeric.html</remarks>

        public const int Bigint = 1;

        /// <summary>
        /// Corresponds to the PostgreSQL 8-byte floating-point "double" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-numeric.html</remarks>

        public const int Double = 8;

        /// <summary>
        /// Corresponds to the PostgreSQL 4-byte "integer" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-numeric.html</remarks>

        public const int Integer = 9;

        /// <summary>
        /// Corresponds to the PostgreSQL arbitrary-precision "numeric" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-numeric.html</remarks>

        public const int Numeric = 13;

        /// <summary>
        /// Corresponds to the PostgreSQL floating-point "real" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-numeric.html</remarks>

        public const int Real = 17;

        /// <summary>
        /// Corresponds to the PostgreSQL 2-byte "smallint" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-numeric.html</remarks>

        public const int Smallint = 18;

        /// <summary>
        /// Corresponds to the PostgreSQL "money" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-money.html</remarks>

        public const int Money = 12;

        #endregion

        #region Boolean Type

        /// <summary>
        /// Corresponds to the PostgreSQL "boolean" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-boolean.html</remarks>

        public const int Boolean = 2;

        #endregion

        #region Geometric types

        /// <summary>
        /// Corresponds to the PostgreSQL geometric "box" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>

        public const int Box = 3;

        /// <summary>
        /// Corresponds to the PostgreSQL geometric "circle" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>

        public const int Circle = 5;

        /// <summary>
        /// Corresponds to the PostgreSQL geometric "line" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>

        public const int Line = 10;

        /// <summary>
        /// Corresponds to the PostgreSQL geometric "lseg" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>

        public const int LSeg = 11;

        /// <summary>
        /// Corresponds to the PostgreSQL geometric "path" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>

        public const int Path = 14;

        /// <summary>
        /// Corresponds to the PostgreSQL geometric "point" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>

        public const int Point = 15;

        /// <summary>
        /// Corresponds to the PostgreSQL geometric "polygon" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-geometric.html</remarks>

        public const int Polygon = 16;

        #endregion

        #region Character Types

        /// <summary>
        /// Corresponds to the PostgreSQL "char(n)" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-character.html</remarks>

        public const int Char = 6;

        /// <summary>
        /// Corresponds to the PostgreSQL "text" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-character.html</remarks>

        public const int Text = 19;

        /// <summary>
        /// Corresponds to the PostgreSQL "varchar" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-character.html</remarks>

        public const int Varchar = 22;

        /// <summary>
        /// Corresponds to the PostgreSQL internal "name" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-character.html</remarks>

        public const int Name = 32;

        /// <summary>
        /// Corresponds to the PostgreSQL "citext" type for the citext module.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/citext.html</remarks>
        public const int Citext = 51;   // Extension type

        /// <summary>
        /// Corresponds to the PostgreSQL "char" type.
        /// </summary>
        /// <remarks>
        /// This is an internal field and should normally not be used for regular applications.
        ///
        /// See https://www.postgresql.org/docs/current/static/datatype-text.html
        /// </remarks>

        public const int InternalChar = 38;

        #endregion

        #region Binary Data Types

        /// <summary>
        /// Corresponds to the PostgreSQL "bytea" type, holding a raw byte string.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-binary.html</remarks>

        public const int Bytea = 4;

        #endregion

        #region Date/Time Types

        /// <summary>
        /// Corresponds to the PostgreSQL "date" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>

        public const int Date = 7;

        /// <summary>
        /// Corresponds to the PostgreSQL "time" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>

        public const int Time = 20;

        /// <summary>
        /// Corresponds to the PostgreSQL "timestamp" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>

        public const int Timestamp = 21;

        /// <summary>
        /// Corresponds to the PostgreSQL "timestamp with time zone" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>
        // NOTE: Don't remove this (see #1694)
        public const int TimestampTZ = 26;

        /// <summary>
        /// Corresponds to the PostgreSQL "timestamp with time zone" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>

        public const int TimestampTz = 26;

        /// <summary>
        /// Corresponds to the PostgreSQL "interval" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>

        public const int Interval = 30;

        /// <summary>
        /// Corresponds to the PostgreSQL "time with time zone" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>
        // NOTE: Don't remove this (see #1694)
        public const int TimeTZ = 31;

        /// <summary>
        /// Corresponds to the PostgreSQL "time with time zone" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>

        public const int TimeTz = 31;

        /// <summary>
        /// Corresponds to the obsolete PostgreSQL "abstime" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-datetime.html</remarks>


        public const int Abstime = 33;

        #endregion

        #region Network Address Types

        /// <summary>
        /// Corresponds to the PostgreSQL "inet" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-net-types.html</remarks>

        public const int Inet = 24;

        /// <summary>
        /// Corresponds to the PostgreSQL "cidr" type, a field storing an IPv4 or IPv6 network.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-net-types.html</remarks>

        public const int Cidr = 44;

        /// <summary>
        /// Corresponds to the PostgreSQL "macaddr" type, a field storing a 6-byte physical address.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-net-types.html</remarks>

        public const int MacAddr = 34;

        /// <summary>
        /// Corresponds to the PostgreSQL "macaddr8" type, a field storing a 6-byte or 8-byte physical address.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-net-types.html</remarks>

        public const int MacAddr8 = 54;

        #endregion

        #region Bit String Types

        /// <summary>
        /// Corresponds to the PostgreSQL "bit" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-bit.html</remarks>

        public const int Bit = 25;

        /// <summary>
        /// Corresponds to the PostgreSQL "varbit" type, a field storing a variable-length string of bits.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-boolean.html</remarks>

        public const int Varbit = 39;

        #endregion

        #region Text Search Types

        /// <summary>
        /// Corresponds to the PostgreSQL "tsvector" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-textsearch.html</remarks>

        public const int TsVector = 45;

        /// <summary>
        /// Corresponds to the PostgreSQL "tsquery" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-textsearch.html</remarks>

        public const int TsQuery = 46;

        /// <summary>
        /// Corresponds to the PostgreSQL "regconfig" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-textsearch.html</remarks>

        public const int Regconfig = 56;

        #endregion

        #region UUID Type

        /// <summary>
        /// Corresponds to the PostgreSQL "uuid" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-uuid.html</remarks>

        public const int Uuid = 27;

        #endregion

        #region XML Type

        /// <summary>
        /// Corresponds to the PostgreSQL "xml" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-xml.html</remarks>

        public const int Xml = 28;

        #endregion

        #region JSON Types

        /// <summary>
        /// Corresponds to the PostgreSQL "json" type, a field storing JSON in text format.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-json.html</remarks>
        /// <seealso cref="Jsonb"/>

        public const int Json = 35;

        /// <summary>
        /// Corresponds to the PostgreSQL "jsonb" type, a field storing JSON in an optimized binary.
        /// format.
        /// </summary>
        /// <remarks>
        /// Supported since PostgreSQL 9.4.
        /// See https://www.postgresql.org/docs/current/static/datatype-json.html
        /// </remarks>

        public const int Jsonb = 36;

        /// <summary>
        /// Corresponds to the PostgreSQL "jsonpath" type, a field storing JSON path in text format.
        /// format.
        /// </summary>
        /// <remarks>
        /// Supported since PostgreSQL 12.
        /// See https://www.postgresql.org/docs/current/datatype-json.html#DATATYPE-JSONPATH
        /// </remarks>

        public const int JsonPath = 57;

        #endregion

        #region HSTORE Type

        /// <summary>
        /// Corresponds to the PostgreSQL "hstore" type, a dictionary of string key-value pairs.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/hstore.html</remarks>
        public const int Hstore = 37; // Extension type

        #endregion

        #region Internal Types

        /// <summary>
        /// Corresponds to the PostgreSQL "refcursor" type.
        /// </summary>

        public const int Refcursor = 23;

        /// <summary>
        /// Corresponds to the PostgreSQL internal "oidvector" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-oid.html</remarks>

        public const int Oidvector = 29;

        /// <summary>
        /// Corresponds to the PostgreSQL internal "int2vector" type.
        /// </summary>

        public const int Int2Vector = 52;

        /// <summary>
        /// Corresponds to the PostgreSQL "oid" type.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-oid.html</remarks>

        public const int Oid = 41;

        /// <summary>
        /// Corresponds to the PostgreSQL "xid" type, an internal transaction identifier.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-oid.html</remarks>

        public const int Xid = 42;

        /// <summary>
        /// Corresponds to the PostgreSQL "xid8" type, an internal transaction identifier.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-oid.html</remarks>

        public const int Xid8 = 64;

        /// <summary>
        /// Corresponds to the PostgreSQL "cid" type, an internal command identifier.
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/datatype-oid.html</remarks>

        public const int Cid = 43;

        /// <summary>
        /// Corresponds to the PostgreSQL "regtype" type, a numeric (OID) ID of a type in the pg_type table.
        /// </summary>

        public const int Regtype = 49;

        /// <summary>
        /// Corresponds to the PostgreSQL "tid" type, a tuple id identifying the physical location of a row within its table.
        /// </summary>

        public const int Tid = 53;

        /// <summary>
        /// Corresponds to the PostgreSQL "pg_lsn" type, which can be used to store LSN (Log Sequence Number) data which
        /// is a pointer to a location in the WAL.
        /// </summary>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/datatype-pg-lsn.html and
        /// https://git.postgresql.org/gitweb/?p=postgresql.git;a=commit;h=7d03a83f4d0736ba869fa6f93973f7623a27038a
        /// </remarks>

        public const int PgLsn = 59;

        #endregion

        #region Special

        /// <summary>
        /// A special value that can be used to send parameter values to the database without
        /// specifying their type, allowing the database to cast them to another value based on context.
        /// The value will be converted to a string and send as text.
        /// </summary>
        /// <remarks>
        /// This value shouldn't ordinarily be used, and makes sense only when sending a data type
        /// unsupported by Npgsql.
        /// </remarks>

        public const int Unknown = 40;

        #endregion

        #region PostGIS

        /// <summary>
        /// The geometry type for PostgreSQL spatial extension PostGIS.
        /// </summary>
        public const int Geometry = 50;  // Extension type

        /// <summary>
        /// The geography (geodetic) type for PostgreSQL spatial extension PostGIS.
        /// </summary>
        public const int Geography = 55; // Extension type

        #endregion

        #region Label tree types

        /// <summary>
        /// The PostgreSQL ltree type, each value is a label path "a.label.tree.value", forming a tree in a set.
        /// </summary>
        /// <remarks>See http://www.postgresql.org/docs/current/static/ltree.html</remarks>
        public const int LTree = 60; // Extension type

        /// <summary>
        /// The PostgreSQL lquery type for PostgreSQL extension ltree
        /// </summary>
        /// <remarks>See http://www.postgresql.org/docs/current/static/ltree.html</remarks>
        public const int LQuery = 61; // Extension type

        /// <summary>
        /// The PostgreSQL ltxtquery type for PostgreSQL extension ltree
        /// </summary>
        /// <remarks>See http://www.postgresql.org/docs/current/static/ltree.html</remarks>
        public const int LTxtQuery = 62; // Extension type

        #endregion

        #region Composables

        /// <summary>
        /// Corresponds to the PostgreSQL "array" type, a variable-length multidimensional array of
        /// another type. This value must be combined with another value from <see cref="NpgsqlDbType"/>
        /// via a bit OR (e.g. NpgsqlDbType.Array | NpgsqlDbType.Integer)
        /// </summary>
        /// <remarks>See https://www.postgresql.org/docs/current/static/arrays.html</remarks>
        public const int Array = int.MinValue;

        /// <summary>
        /// Corresponds to the PostgreSQL "range" type, continuous range of values of specific type.
        /// This value must be combined with another value from <see cref="NpgsqlDbType"/>
        /// via a bit OR (e.g. NpgsqlDbType.Range | NpgsqlDbType.Integer)
        /// </summary>
        /// <remarks>
        /// Supported since PostgreSQL 9.2.
        /// See https://www.postgresql.org/docs/current/static/rangetypes.html
        /// </remarks>
        public const int Range = 0x40000000;

        /// <summary>
        /// Corresponds to the PostgreSQL "multirange" type, continuous range of values of specific type.
        /// This value must be combined with another value from <see cref="NpgsqlDbType"/>
        /// via a bit OR (e.g. NpgsqlDbType.Multirange | NpgsqlDbType.Integer)
        /// </summary>
        /// <remarks>
        /// Supported since PostgreSQL 14.
        /// See https://www.postgresql.org/docs/current/static/rangetypes.html
        /// </remarks>
        public const int Multirange = 0x20000000;

        #endregion
    }
}
