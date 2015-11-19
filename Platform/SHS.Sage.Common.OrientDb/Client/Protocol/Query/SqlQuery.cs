﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Orient.Client.Protocol
{
    internal class SqlQuery
    {
        private QueryCompiler _compiler = new QueryCompiler();

        internal void Class(string className)
        {
            _compiler.Unique(Q.Class, ParseClassName(className));
        }

        internal void Property(string propertyName, OType type)
        {
            _compiler.Unique(Q.Property, propertyName, type.ToString());
        }

        internal void LinkedType(OType type)
        {
            _compiler.Unique(Q.LinkedType, type.ToString());
        }

        internal void LinkedClass(string @class)
        {
            _compiler.Unique(Q.LinkedClass, @class);
        }

        #region Cluster

        internal void Cluster(string clusterName)
        {
            _compiler.Unique(Q.Cluster, ParseClassName(clusterName));
        }

        internal void Cluster(string clusterName, OClusterType clustertype)
        {
            _compiler.Unique(Q.Cluster, ParseClassName(clusterName), clustertype.ToString().ToUpper());
        }

        #endregion

        internal void Record(ORID orid)
        {
            _compiler.Unique(Q.Record, orid.ToString());
        }

        internal void Extends(string superClass)
        {
            _compiler.Unique(Q.Extends, ParseClassName(superClass));
        }

        internal void Vertex(string className)
        {
            _compiler.Unique(Q.Vertex, ParseClassName(className));
        }

        internal void Edge(string className)
        {
            _compiler.Unique(Q.Edge, ParseClassName(className));
        }

        internal void Insert<T>(T obj)
        {
            ODocument document;

            if (obj is ODocument)
            {
                document = obj as ODocument;
            }
            else
            {
                document = ODocument.ToDocument(obj);
            }

            if (!string.IsNullOrEmpty(document.OClassName))
            {
                Class(document.OClassName);
            }

            Set(document);
        }

        internal void Update<T>(T obj)
        {
            ODocument document;

            if (obj is ODocument)
            {
                document = obj as ODocument;
            }
            else
            {
                document = ODocument.ToDocument(obj);
            }

            if (!string.IsNullOrEmpty(document.OClassName))
            {
                Class(document.OClassName);
            }

            if (document.ORID != null)
            {
                Record(document.ORID);
            }

            Set(document);
        }

        internal void Delete<T>(T obj)
        {
            ODocument document;

            if (obj is ODocument)
            {
                document = obj as ODocument;
            }
            else
            {
                document = ODocument.ToDocument(obj);
            }

            if (!string.IsNullOrEmpty(document.OClassName))
            {
                Class(document.OClassName);
            }
            else
            {
                throw new OException(OExceptionType.Query, "Document doesn't contain OClassName value.");
            }

            if (document.ORID != null)
            {
                // check if the @rid is correct in real example
                Where("@rid");

                Equals(document.ORID);
            }
        }

        internal void DeleteVertex<T>(T obj)
        {
            ODocument document;

            if (obj is ODocument)
            {
                document = obj as ODocument;
            }
            else
            {
                document = ODocument.ToDocument(obj);
            }

            if (!string.IsNullOrEmpty(document.OClassName))
            {
                Class(document.OClassName);
            }

            if (document.ORID != null)
            {
                Record(document.ORID);
            }
        }

        internal void DeleteEdge<T>(T obj)
        {
            ODocument document;

            if (obj is ODocument)
            {
                document = obj as ODocument;
            }
            else
            {
                document = ODocument.ToDocument(obj);
            }

            if (!string.IsNullOrEmpty(document.OClassName))
            {
                Class(document.OClassName);
            }

            if (document.ORID != null)
            {
                Record(document.ORID);
            }
        }

        #region Select

        internal void Select(params string[] projections)
        {
            for (int i = 0; i < projections.Length; i++)
            {
                _compiler.Append(Q.Select, projections[i]);

                if (i < (projections.Length - 1))
                {
                    _compiler.Append(Q.Select, Q.Comma, "");
                }
            }
        }

        internal void Traverse(params string[] projections)
        {
            for (int i = 0; i < projections.Length; i++)
            {
                _compiler.Append(Q.Traverse, projections[i]);

                if (i < (projections.Length - 1))
                {
                    _compiler.Append(Q.Traverse, Q.Comma, "");
                }
            }
        }

        internal void Also(string projection)
        {
            _compiler.Append(Q.Select, Q.Comma, projection);
        }

        internal void Nth(int index)
        {
            _compiler.Append(Q.Select, "[" + index + "]");
        }

        internal void As(string alias)
        {
            _compiler.Append(Q.Select, "", Q.As, alias);
        }

        #endregion

        #region From

        internal void From(ORID orid)
        {
            _compiler.Unique(Q.From, orid.ToString());
        }

        internal void From(string target)
        {
            _compiler.Unique(Q.From, ParseClassName(target));
        }

        internal void From(ODocument document)
        {
            if (!string.IsNullOrEmpty(document.OClassName))
            {
                From(document.OClassName);
            }

            if (document.ORID != null)
            {
                From(document.ORID);
            }
        }

        #endregion

        internal void To(ORID orid)
        {
            _compiler.Unique(Q.To, orid.ToString());
        }

        #region Set

        internal void Set<T>(string fieldName, T fieldValue)
        {
            string field = "";

            if (_compiler.HasKey(Q.Set))
            {
                field += ", ";
            }

            field += string.Join(" ", fieldName, Q.Equals, "");

            if (fieldValue == null)
            {
                field += "null";
            }
            else if (fieldValue is string)
            {
                field += "'" + fieldValue + "'";
            }
            else if (fieldValue is IList)
            {
                field += "[";
                IList collection = (IList)fieldValue;
                int iteration = 0;

                foreach (object item in collection)
                {
                    field += SqlQuery.ToString(item);

                    iteration++;

                    if (iteration < collection.Count)
                    {
                        field += ", ";
                    }
                }

                field += "]";
            }
            else if (fieldValue is DateTime)
            {
                //DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime value = (DateTime)((object)fieldValue);
                //field += ((long)(value - unixEpoch).TotalMilliseconds);
                field += "'" + value.ToString("s").Replace('T', ' ') + "'";
            }
            else
            {
                field += fieldValue.ToString();
            }

            _compiler.Append(Q.Set, field);
        }

        internal void Set<T>(T obj)
        {
            ODocument document;

            if (obj is ODocument)
            {
                document = obj as ODocument;
            }
            else
            {
                document = ODocument.ToDocument(obj);
            }

            // TODO: go also through embedded fields
            foreach (KeyValuePair<string, object> field in document)
            {
                // set only fields which doesn't start with @ character
                if ((field.Key.Length > 0) && (field.Key[0] != '@'))
                {
                    Set(field.Key, field.Value);
                }
            }
        }

        #endregion

        #region Where with conditions

        internal void Where(string field)
        {
            _compiler.Append(Q.Where, field);
        }

        internal void And(string field)
        {
            _compiler.Append(Q.Where, "", Q.And, field);
        }

        internal void Or(string field)
        {
            _compiler.Append(Q.Where, "", Q.Or, field);
        }

        internal void Equals<T>(T item)
        {
            _compiler.Append(Q.Where, "", Q.Equals, SqlQuery.ToString(item));
        }

        internal void NotEquals<T>(T item)
        {
            _compiler.Append(Q.Where, "", Q.NotEquals, SqlQuery.ToString(item));
        }

        internal void Lesser<T>(T item)
        {
            _compiler.Append(Q.Where, "", Q.Lesser, SqlQuery.ToString(item));
        }

        internal void LesserEqual<T>(T item)
        {
            _compiler.Append(Q.Where, "", Q.LesserEqual, SqlQuery.ToString(item));
        }

        internal void Greater<T>(T item)
        {
            _compiler.Append(Q.Where, "", Q.Greater, SqlQuery.ToString(item));
        }

        internal void GreaterEqual<T>(T item)
        {
            _compiler.Append(Q.Where, "", Q.GreaterEqual, SqlQuery.ToString(item));
        }

        internal void Like<T>(T item)
        {
            _compiler.Append(Q.Where, "", Q.Like, SqlQuery.ToString(item));
        }

        internal void IsNull()
        {
            _compiler.Append(Q.Where, "", Q.Is, Q.Null);
        }

        internal void Contains<T>(T item)
        {
            _compiler.Append(Q.Where, "", Q.Contains, SqlQuery.ToString(item));
        }

        internal void Contains<T>(string field, T value)
        {
            _compiler.Append(Q.Where, "", Q.Contains, "(" + field, Q.Equals, SqlQuery.ToString(value) + ")");
        }

        #endregion

        #region Add

        internal void Add<T>(string fieldName, T fieldValue)
        {
            string field = "";

            if (_compiler.HasKey(Q.Add))
            {
                field += ", ";
            }

            field += string.Join(" ", fieldName, Q.Equals, SqlQuery.ToString(fieldValue));

            _compiler.Append(Q.Add, field);
        }

        #endregion

        #region Remove

        public void Remove(string fieldName)
        {
            if (_compiler.HasKey(Q.Remove))
            {
                fieldName = ", " + fieldName;
            }

            _compiler.Append(Q.Remove, fieldName);
        }

        public void Remove<T>(string fieldName, T collectionValue)
        {
            if (_compiler.HasKey(Q.Remove))
            {
                fieldName = ", " + fieldName;
            }

            _compiler.Append(Q.Remove, fieldName, Q.Equals, SqlQuery.ToString(collectionValue));
        }

        #endregion

        internal void OrderBy(params string[] fields)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                _compiler.Append(Q.OrderBy, fields[i]);

                if (i < (fields.Length - 1))
                {
                    _compiler.Append(Q.OrderBy, Q.Comma, "");
                }
            }
        }

        internal void Ascending()
        {
            _compiler.Unique(Q.Ascending, Q.Ascending);
        }

        internal void Descending()
        {
            _compiler.Unique(Q.Descending, Q.Descending);
        }

        internal void Skip(int skipCount)
        {
            _compiler.Unique(Q.Skip, skipCount.ToString());
        }

        internal void Limit(int maxRecords)
        {
            _compiler.Unique(Q.Limit, maxRecords.ToString());
        }

        #region ToString

        internal static string ToString(object value)
        {
            string sql = "";

            if (value is string)
            {
                sql = string.Join(" ", "'" + value + "'");
            }
            else
            {
                sql = string.Join(" ", value.ToString());
            }

            return sql;
        }

        internal string ToString(QueryType type)
        {
            switch (type)
            {
                case QueryType.CreateClass:
                    return GenerateCreateClassQuery();
                case QueryType.CreateProperty:
                    return GenerateCreatePropertyQuery();
                case QueryType.CreateCluster:
                    return GenerateCreateClusterQuery();
                case QueryType.CreateEdge:
                    return GenerateCreateEdgeQuery();
                case QueryType.CreateVertex:
                    return GenerateCreateVertexQuery();
                case QueryType.DeleteVertex:
                    return GenerateDeleteVertexQuery();
                case QueryType.DeleteEdge:
                    return GenerateDeleteEdgeQuery();
                case QueryType.DeleteDocument:
                    return GenerateDeleteDocumentQuery();
                case QueryType.Insert:
                    return GenerateInsertQuery();
                case QueryType.Update:
                    return GenerateUpdateQuery();
                case QueryType.Select:
                    return GenerateSelectQuery();
                case QueryType.Traverse:
                    return GenerateTraverseQuery();
                default:
                    break;
            }

            return "";
        }

        private string GenerateTraverseQuery()
        {
            string query = "";

            // TRAVERSE [<Projections>]
            if (string.IsNullOrEmpty(_compiler.Value(Q.Traverse)))
            {
                query += string.Join(" ", Q.Traverse);
            }
            else
            {
                query += string.Join(" ", Q.Traverse, _compiler.Value(Q.Traverse));
            }

            // [LET <Assignment>*]) 
            // TODO:

            // FROM <Target>
            query += string.Join(" ", "", Q.From, _compiler.Value(Q.From));

            // [<Condition>*](WHERE) 
            if (_compiler.HasKey(Q.Where))
            {
                query += string.Join(" ", "", Q.Where, _compiler.Value(Q.Where));
            }

            // [BY <Field>](GROUP) 
            // TODO:

            // [BY <Fields>* [ASC|DESC](ORDER)*] 
            if (_compiler.HasKey(Q.OrderBy))
            {
                query += string.Join(" ", "", Q.OrderBy, _compiler.Value(Q.OrderBy));
            }

            if (_compiler.HasKey(Q.Ascending))
            {
                query += string.Join(" ", "", Q.Ascending);
            }

            if (_compiler.HasKey(Q.Descending))
            {
                query += string.Join(" ", "", Q.Descending);
            }

            // [<SkipRecords>](SKIP) 
            if (_compiler.HasKey(Q.Skip))
            {
                query += string.Join(" ", "", Q.Skip, _compiler.Value(Q.Skip));
            }

            // [<MaxRecords>](LIMIT)
            if (_compiler.HasKey(Q.Limit))
            {
                query += string.Join(" ", "", Q.Limit, _compiler.Value(Q.Limit));
            }

            return query;
        }

        #endregion

        private string GenerateCreateClassQuery()
        {
            string query = "";

            // CREATE CLASS <class> 
            query += string.Join(" ", Q.Create, Q.Class, _compiler.Value(Q.Class));

            // [EXTENDS <super-class>] 
            if (_compiler.HasKey(Q.Extends))
            {
                query += string.Join(" ", "", Q.Extends, _compiler.Value(Q.Extends));
            }

            // [CLUSTER <clusterId>*]
            if (_compiler.HasKey(Q.Cluster))
            {
                query += string.Join(" ", "", Q.Cluster, _compiler.Value(Q.Cluster));
            }

            return query;
        }

        private string GenerateCreatePropertyQuery()
        {
            // CREATE PROPERTY <class>.<property> <type> [<linked-type>|<linked-class>]
            string query = "";

            // CREATE PROPERTY <class> 
            query += string.Join(" ", Q.Create, Q.Property, ParsePropertyName(_compiler.Value(Q.Property)));
            // [<linked-type>|<linked-class>]
            query += string.Join(" ", "", _compiler.OrderedValue(Q.LinkedType, Q.LinkedClass));

            return query;
        }

        private string GenerateCreateClusterQuery()
        {
            string query = "";

            // CREATE CLUSTER <name> <type> 
            query += string.Join(" ", Q.Create, Q.Cluster, _compiler.Value(Q.Cluster));

            // [DATASEGMENT <data-segment>|default] 
            // TODO:

            // [LOCATION <path>|default] 
            // TODO:

            // [POSITION <position>|append]
            // TODO:

            return query;
        }

        private string GenerateCreateEdgeQuery()
        {
            string query = "";

            // CREATE EDGE [<class>] 
            query += string.Join(" ", Q.Create, Q.Edge, _compiler.Value(Q.Edge));

            // [CLUSTER <cluster>]
            if (_compiler.HasKey(Q.Cluster))
            {
                query += string.Join(" ", "", Q.Cluster, _compiler.Value(Q.Cluster));
            }

            // FROM <rid>|(<query>)|[<rid>]* 
            query += string.Join(" ", "", Q.From, _compiler.Value(Q.From));

            // TO <rid>|(<query>)|[<rid>]* 
            query += string.Join(" ", "", Q.To, _compiler.Value(Q.To));

            // [SET <field> = <expression>[,]*]
            if (_compiler.HasKey(Q.Set))
            {
                query += string.Join(" ", "", Q.Set, _compiler.Value(Q.Set));
            }

            return query;
        }

        private string GenerateCreateVertexQuery()
        {
            string query = "";

            // CREATE VERTEX [<class>] 
            query += string.Join(" ", Q.Create, Q.Vertex, _compiler.Value(Q.Vertex));

            // [CLUSTER <cluster>]
            if (_compiler.HasKey(Q.Cluster))
            {
                query += string.Join(" ", "", Q.Cluster, _compiler.Value(Q.Cluster));
            }

            // [SET <field> = <expression>[,]*]
            if (_compiler.HasKey(Q.Set))
            {
                query += string.Join(" ", "", Q.Set, _compiler.Value(Q.Set));
            }

            return query;
        }

        private string GenerateInsertQuery()
        {
            string query = "";

            // INSERT INTO <Class>|cluster:<cluster>|index:<index> 
            query += string.Join(" ", Q.Insert, Q.Into, _compiler.Value(Q.Class));

            // [<cluster>](cluster) 
            if (_compiler.HasKey(Q.Cluster))
            {
                query += string.Join(" ", "", Q.Cluster, _compiler.Value(Q.Cluster));
            }

            // [VALUES (<expression>[,]((<field>[,]*))*)]|[<field> = <expression>[,](SET)*]
            if (_compiler.HasKey(Q.Set))
            {
                query += string.Join(" ", "", Q.Set, _compiler.Value(Q.Set));
            }

            return query;
        }

        private string GenerateUpdateQuery()
        {
            string query = "";

            // UPDATE <class>|cluster:<cluster>|<recordID>
            query += string.Join(" ", Q.Update, _compiler.OrderedValue(Q.Class, Q.Cluster, Q.Record));

            // [SET|INCREMENT <field-name> = <field-value>](,)*
            if (_compiler.HasKey(Q.Set))
            {
                query += string.Join(" ", "", Q.Set, _compiler.Value(Q.Set));
            }

            // (ADD|REMOVE])[<field-name> = <field-value>](,)*
            if (_compiler.HasKey(Q.Add))
            {
                query += string.Join(" ", "", Q.Add, _compiler.Value(Q.Add));
            }
            else if (_compiler.HasKey(Q.Remove))
            {
                query += string.Join(" ", "", Q.Remove, _compiler.Value(Q.Remove));
            }

            // [<conditions>](WHERE) 
            if (_compiler.HasKey(Q.Where))
            {
                query += string.Join(" ", "", Q.Where, _compiler.Value(Q.Where));
            }

            // [<max-records>](LIMIT)
            if (_compiler.HasKey(Q.Limit))
            {
                query += string.Join(" ", "", Q.Limit, _compiler.Value(Q.Limit));
            }

            return query;
        }

        private string GenerateDeleteVertexQuery()
        {
            string query = "";

            // DELETE VERTEX <rid>|<[<class>]
            query += string.Join(" ", Q.Delete, Q.Vertex, _compiler.OrderedValue(Q.Class, Q.Record));

            // [WHERE <conditions>] 
            if (_compiler.HasKey(Q.Where))
            {
                query += string.Join(" ", "", Q.Where, _compiler.Value(Q.Where));
            }

            // [LIMIT <MaxRecords>>]
            if (_compiler.HasKey(Q.Limit))
            {
                query += string.Join(" ", "", Q.Limit, _compiler.Value(Q.Limit));
            }

            return query;
        }

        private string GenerateDeleteEdgeQuery()
        {
            string query = "";

            // DELETE EDGE <rid>|FROM <rid>|TO <rid>|<[<class>] 
            if (_compiler.HasKey(Q.From) && _compiler.HasKey(Q.To))
            {
                query += string.Join(" ", Q.Delete, Q.Edge, _compiler.Value(Q.Class), Q.From, _compiler.Value(Q.From), Q.To, _compiler.Value(Q.To));
            }
            else
            {
                query += string.Join(" ", Q.Delete, Q.Edge, _compiler.OrderedValue(Q.Class, Q.Record));
            }

            // [WHERE <conditions>]> 
            if (_compiler.HasKey(Q.Where))
            {
                query += string.Join(" ", "", Q.Where, _compiler.Value(Q.Where));
            }

            // [LIMIT <MaxRecords>]
            if (_compiler.HasKey(Q.Limit))
            {
                query += string.Join(" ", "", Q.Limit, _compiler.Value(Q.Limit));
            }

            return query;
        }

        private string GenerateDeleteDocumentQuery()
        {
            string query = "";

            // DELETE FROM <Class>|cluster:<cluster>|index:<index> 
            query += string.Join(" ", Q.Delete, Q.From, _compiler.OrderedValue(Q.Class, Q.Cluster));

            // [<Condition>*](WHERE) 
            if (_compiler.HasKey(Q.Where))
            {
                query += string.Join(" ", "", Q.Where, _compiler.Value(Q.Where));
            }

            // [BY <Fields>* [ASC|DESC](ORDER)*] 
            // TODO:


            // [<MaxRecords>](LIMIT)
            if (_compiler.HasKey(Q.Limit))
            {
                query += string.Join(" ", "", Q.Limit, _compiler.Value(Q.Limit));
            }

            return query;
        }

        private string GenerateSelectQuery()
        {
            string query = "";

            // SELECT [<Projections>]
            if (string.IsNullOrEmpty(_compiler.Value(Q.Select)))
            {
                query += string.Join(" ", Q.Select);
            }
            else
            {
                query += string.Join(" ", Q.Select, _compiler.Value(Q.Select));
            }

            // [LET <Assignment>*]) 
            // TODO:

            // FROM <Target>
            query += string.Join(" ", "", Q.From, _compiler.Value(Q.From));

            // [<Condition>*](WHERE) 
            if (_compiler.HasKey(Q.Where))
            {
                query += string.Join(" ", "", Q.Where, _compiler.Value(Q.Where));
            }

            // [BY <Field>](GROUP) 
            // TODO:

            // [BY <Fields>* [ASC|DESC](ORDER)*] 
            if (_compiler.HasKey(Q.OrderBy))
            {
                query += string.Join(" ", "", Q.OrderBy, _compiler.Value(Q.OrderBy));
            }

            if (_compiler.HasKey(Q.Ascending))
            {
                query += string.Join(" ", "", Q.Ascending);
            }

            if (_compiler.HasKey(Q.Descending))
            {
                query += string.Join(" ", "", Q.Descending);
            }

            // [<SkipRecords>](SKIP) 
            if (_compiler.HasKey(Q.Skip))
            {
                query += string.Join(" ", "", Q.Skip, _compiler.Value(Q.Skip));
            }

            // [<MaxRecords>](LIMIT)
            if (_compiler.HasKey(Q.Limit))
            {
                query += string.Join(" ", "", Q.Limit, _compiler.Value(Q.Limit));
            }

            return query;
        }

        private string ParseClassName(string className)
        {
            if (className.Equals(typeof(OVertex).Name))
            {
                return "V";
            }

            if (className.Equals(typeof(OEdge).Name))
            {
                return "E";
            }

            return className;
        }

        private string ParsePropertyName(string propertyName)
        {
            if (_compiler.HasKey(Q.Class))
            {
                return _compiler[Q.Class] + "." + propertyName;
            }

            return propertyName;
        }

    }
}
