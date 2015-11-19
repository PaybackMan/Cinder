using SHS.Sage.Linq.Expressions;
using SHS.Sage.Linq.Expressions.Queryable;
using SHS.Sage.Linq.Translation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Language
{
    /// <summary>
    /// Defines the language rules for the query provider
    /// </summary>
    public abstract class QueryLanguage
    {
        public abstract QueryTypeSystem TypeSystem { get; }
        public abstract Expression GetGeneratedIdExpression(MemberInfo member);

        public virtual string Quote(string name)
        {
            return name;
        }

        public virtual bool AllowsMultipleCommands
        {
            get { return false; }
        }

        public virtual bool AllowSubqueryInSelectWithoutFrom
        {
            get { return false; }
        }

        public virtual bool AllowDistinctInAggregates
        {
            get { return false; }
        }

        public virtual Expression GetRowsAffectedExpression(Expression command)
        {
            return new FunctionExpression(typeof(int), "@@ROWCOUNT", null);
        }

        public virtual bool IsRowsAffectedExpressions(Expression expression)
        {
            FunctionExpression fex = expression as FunctionExpression;
            return fex != null && fex.Name == "@@ROWCOUNT";
        }

        public virtual Expression GetOuterJoinTest(SelectExpression select)
        {
            // if the field is used in the join condition (equality test)
            // if it is null in the database then the join test won't match (null != null) so the row won't appear
            // we can safely use this existing field as our test to determine if the outer join produced a row

            // find a field that is used in equality test
            var aliases = DeclaredAliasGatherer.Gather(select.From);
            var joinFields = JoinFieldGatherer.Gather(aliases, select).ToList();
            if (joinFields.Count > 0)
            {
                // prefer one that is already in the projection list.
                foreach (var jc in joinFields)
                {
                    foreach (var col in select.Fields)
                    {
                        if (jc.Equals(col.Expression))
                        {
                            return jc;
                        }
                    }
                }
                return joinFields[0];
            }

            // fall back to introducing a constant
            return Expression.Constant(1, typeof(int?));
        }

        public virtual ProjectionExpression AddOuterJoinTest(ProjectionExpression proj)
        {
            var test = this.GetOuterJoinTest(proj.Select);
            var select = proj.Select;
            FieldExpression testCol = null;
            // look to see if test expression exists in fields already
            foreach (var col in select.Fields)
            {
                if (test.Equals(col.Expression))
                {
                    var colType = this.TypeSystem.GetStorageType(test.Type);
                    testCol = new FieldExpression(test.Type, colType, select.Alias, col.Name);
                    break;
                }
            }
            if (testCol == null)
            {
                // add expression to projection
                testCol = test as FieldExpression;
                string colName = (testCol != null) ? testCol.Name : "Test";
                colName = proj.Select.Fields.GetAvailableFieldName(colName);
                var colType = this.TypeSystem.GetStorageType(test.Type);
                select = select.AddField(new FieldDeclaration(colName, test, colType));
                testCol = new FieldExpression(test.Type, colType, select.Alias, colName);
            }
            var newProjector = new OuterJoinedExpression(testCol, proj.Projector);
            return new ProjectionExpression(select, newProjector, proj.Aggregator);
        }

        class JoinFieldGatherer
        {
            HashSet<IdentifiableAlias> aliases;
            HashSet<FieldExpression> fields = new HashSet<FieldExpression>();

            private JoinFieldGatherer(HashSet<IdentifiableAlias> aliases)
            {
                this.aliases = aliases;
            }

            public static HashSet<FieldExpression> Gather(HashSet<IdentifiableAlias> aliases, SelectExpression select)
            {
                var gatherer = new JoinFieldGatherer(aliases);
                gatherer.Gather(select.Where);
                return gatherer.fields;
            }

            private void Gather(Expression expression)
            {
                BinaryExpression b = expression as BinaryExpression;
                if (b != null)
                {
                    switch (b.NodeType)
                    {
                        case ExpressionType.Equal:
                        case ExpressionType.NotEqual:
                            if (IsExternalField(b.Left) && GetField(b.Right) != null)
                            {
                                this.fields.Add(GetField(b.Right));
                            }
                            else if (IsExternalField(b.Right) && GetField(b.Left) != null)
                            {
                                this.fields.Add(GetField(b.Left));
                            }
                            break;
                        case ExpressionType.And:
                        case ExpressionType.AndAlso:
                            if (b.Type == typeof(bool) || b.Type == typeof(bool?))
                            {
                                this.Gather(b.Left);
                                this.Gather(b.Right);
                            }
                            break;
                    }
                }
            }

            private FieldExpression GetField(Expression exp)
            {
                while (exp.NodeType == ExpressionType.Convert)
                    exp = ((UnaryExpression)exp).Operand;
                return exp as FieldExpression;
            }

            private bool IsExternalField(Expression exp)
            {
                var col = GetField(exp);
                if (col != null && !this.aliases.Contains(col.Alias))
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Determines whether the CLR type corresponds to a scalar data type in the query language
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual bool IsScalar(Type type)
        {
            type = TypeHelper.GetNonNullableType(type);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    return false;
                case TypeCode.Object:
                    return
                        type == typeof(DateTimeOffset) ||
                        type == typeof(TimeSpan) ||
                        type == typeof(Guid) ||
                        type == typeof(byte[]);
                default:
                    return true;
            }
        }

        public virtual bool IsAggregate(MemberInfo member)
        {
            var method = member as MethodInfo;
            if (method != null)
            {
                if (method.DeclaringType == typeof(Queryable)
                    || method.DeclaringType == typeof(Enumerable))
                {
                    switch (method.Name)
                    {
                        case "Count":
                        case "LongCount":
                        case "Sum":
                        case "Min":
                        case "Max":
                        case "Average":
                            return true;
                    }
                }
            }
            var property = member as PropertyInfo;
            if (property != null
                && property.Name == "Count"
                && typeof(IEnumerable).IsAssignableFrom(property.DeclaringType))
            {
                return true;
            }
            return false;
        }

        public virtual bool AggregateArgumentIsPredicate(string aggregateName)
        {
            return aggregateName == "Count" || aggregateName == "LongCount";
        }

        /// <summary>
        /// Determines whether the given expression can be represented as a field in a select expressionss
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual bool CanBeField(Expression expression)
        {
            // by default, push all work in projection to client
            return this.MustBeField(expression);
        }

        public virtual bool MustBeField(Expression expression)
        {
            switch (expression.NodeType)
            {
                case (ExpressionType)DbExpressionType.Field:
                case (ExpressionType)DbExpressionType.Scalar:
                case (ExpressionType)DbExpressionType.Exists:
                case (ExpressionType)DbExpressionType.AggregateSubquery:
                case (ExpressionType)DbExpressionType.Aggregate:
                    return true;
                default:
                    return false;
            }
        }

        public abstract QueryLinguist CreateLinguist(QueryTranslator translator);
    }
}
