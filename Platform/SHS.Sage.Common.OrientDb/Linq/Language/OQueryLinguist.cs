using SHS.Sage.Linq.Expressions;
using SHS.Sage.Linq.Expressions.Command;
using SHS.Sage.Linq.Expressions.Queryable;
using SHS.Sage.Linq.Policy;
using SHS.Sage.Linq.Translation;
using SHS.Sage.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Language
{
    public class OQueryLinguist : QueryLinguist
    {
        public OQueryLinguist(QueryLanguage language, QueryTranslator translator) : base(language, translator) { }
        public override string Format(System.Linq.Expressions.Expression expression)
        {
            using(var ms = new MemoryStream())
            {
                using(var sw = new StreamWriter(ms, ASCIIEncoding.ASCII, 1024, true))
                {
                    var writer = new OQueryWriter(this);
                    writer.Write(expression, sw);
                }
                ms.Position = 0;
                return ASCIIEncoding.ASCII.GetString(ms.ToArray());
            }
        }

        public override Expression Translate(Expression expression)
        {
            // remove redundant layers again before cross apply rewrite
            expression = UnusedFieldRemover.Remove(expression);
            expression = RedundantFieldRemover.Remove(expression);
            expression = RedundantSubqueryRemover.Remove(expression);

            // convert cross-apply and outer-apply joins into inner & left-outer-joins if possible
            var rewritten = CrossApplyRewriter.Rewrite(this.Language, expression);

            // convert cross joins into inner joins
            rewritten = CrossJoinRewriter.Rewrite(rewritten);

            if (rewritten != expression)
            {
                expression = rewritten;
                // do final reduction
                expression = UnusedFieldRemover.Remove(expression);
                expression = RedundantSubqueryRemover.Remove(expression);
                expression = RedundantJoinRemover.Remove(expression);
                expression = RedundantFieldRemover.Remove(expression);
            }

            // add policy.DefaultLimit if LIMIT is not specified
            expression = MissingLimitAdder.Evaluate(expression, this.Translator.Police.Policy as OQueryPolicy);

            return expression;
        }

        public override Expression Parameterize(Expression expression)
        {
            return OParameterizer.Parameterize(this.Language, expression);
        }

        private class OQueryWriter : DbExpressionVisitor
        {
            Expression _query;
            StreamWriter _sw;
            OQueryLinguist _linguist;
            int _depth = 0;
            int _indent = 3;
            Dictionary<IdentifiableAlias, string> _aliases;
            bool _forDebug = false;
            bool _hideFieldAliases = true;
            bool _hideIdentifiableAliases = true;
            bool _isNested;

            protected enum Indentation
            {
                Same,
                Inner,
                Outer
            }

            public OQueryWriter(OQueryLinguist linguist) 
            {
                _linguist = linguist;
                _aliases = new Dictionary<IdentifiableAlias, string>();
            }

            public OQueryLinguist Linguist { get { return _linguist; } }
            public int IndentationWidth { get { return _indent; } }
            public int Depth { get { return _depth; } }

            protected bool HideFieldAliases
            {
                get { return this._hideFieldAliases; }
                set { this._hideFieldAliases = value; }
            }

            protected bool HideIdentifiableAliases
            {
                get { return this._hideIdentifiableAliases; }
                set { this._hideIdentifiableAliases = value; }
            }


            protected bool IsNested
            {
                get { return this._isNested; }
                set { this._isNested = value; }
            }

            protected bool ForDebug
            {
                get { return this._forDebug; }
            }

            public void Write(Expression query, StreamWriter writer)
            {
                this._query = query;
                this._sw = writer;
                this.Visit(query);
            }

            protected void Write(object value)
            {
                _sw.Write(value);
            }

            protected virtual void WriteParameterName(string name)
            {
                this.Write("{@" + name + "}");
            }

            protected virtual void WriteVariableName(string name)
            {
                this.WriteParameterName(name);
            }

            protected virtual void WriteAsAliasName(string aliasName)
            {
                this.Write("AS ");
                this.WriteAliasName(aliasName);
            }

            protected virtual void WriteAliasName(string aliasName)
            {
                this.Write(aliasName);
            }

            protected virtual void WriteAsFieldName(string fieldName)
            {
                this.Write("AS ");
                this.WriteFieldName(fieldName);
            }

            protected virtual void WriteFieldName(string fieldName)
            {
                string name = (this.Linguist.Language != null) ? this.Linguist.Language.Quote(fieldName) : fieldName;
                
                this.Write(name);
            }

            protected virtual void WriteIdentifiableName(string tableName)
            {
                string name = (this.Linguist.Language != null) ? this.Linguist.Language.Quote(tableName) : tableName;
                this.Write(name);
            }

            protected void WriteLine(Indentation style)
            {
                _sw.WriteLine();
                this.Indent(style);
                for (int i = 0, n = this._depth * this._indent; i < n; i++)
                {
                    this.Write(" ");
                }
            }

            protected void Indent(Indentation style)
            {
                if (style == Indentation.Inner)
                {
                    this._depth++;
                }
                else if (style == Indentation.Outer)
                {
                    this._depth--;
                    System.Diagnostics.Debug.Assert(this._depth >= 0);
                }
            }

            protected virtual string GetAliasName(IdentifiableAlias alias)
            {
                string name;
                if (!this._aliases.TryGetValue(alias, out name))
                {
                    name = "A" + alias.GetHashCode() + "?";
                    this._aliases.Add(alias, name);
                }
                return name;
            }

            protected void AddAlias(IdentifiableAlias alias)
            {
                string name;
                if (!this._aliases.TryGetValue(alias, out name))
                {
                    name = "t" + this._aliases.Count;
                    this._aliases.Add(alias, name);
                }
            }

            protected virtual void AddAliases(Expression expr)
            {
                AliasedExpression ax = expr as AliasedExpression;
                if (ax != null)
                {
                    this.AddAlias(ax.Alias);
                }
                else
                {
                    JoinExpression jx = expr as JoinExpression;
                    if (jx != null)
                    {
                        this.AddAliases(jx.Left);
                        this.AddAliases(jx.Right);
                    }
                }
            }

            protected override Expression Visit(Expression exp)
            {
                if (exp == null) return null;

                // check for supported node types first 
                // non-supported ones should not be visited (as they would produce bad SQL)
                switch (exp.NodeType)
                {
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                    case ExpressionType.Not:
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.UnaryPlus:
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.Divide:
                    case ExpressionType.Modulo:
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.Coalesce:
                    case ExpressionType.RightShift:
                    case ExpressionType.LeftShift:
                    case ExpressionType.ExclusiveOr:
                    case ExpressionType.Power:
                    case ExpressionType.Conditional:
                    case ExpressionType.Constant:
                    case ExpressionType.MemberAccess:
                    case ExpressionType.Call:
                    case ExpressionType.New:
                    case (ExpressionType)DbExpressionType.Identifiable:
                    case (ExpressionType)DbExpressionType.Field:
                    case (ExpressionType)DbExpressionType.Select:
                    case (ExpressionType)DbExpressionType.Join:
                    case (ExpressionType)DbExpressionType.Aggregate:
                    case (ExpressionType)DbExpressionType.Scalar:
                    case (ExpressionType)DbExpressionType.Exists:
                    case (ExpressionType)DbExpressionType.In:
                    case (ExpressionType)DbExpressionType.AggregateSubquery:
                    case (ExpressionType)DbExpressionType.IsNull:
                    case (ExpressionType)DbExpressionType.Between:
                    case (ExpressionType)DbExpressionType.RowCount:
                    case (ExpressionType)DbExpressionType.Projection:
                    case (ExpressionType)DbExpressionType.NamedValue:
                    case (ExpressionType)DbExpressionType.Insert:
                    case (ExpressionType)DbExpressionType.Update:
                    case (ExpressionType)DbExpressionType.Delete:
                    case (ExpressionType)DbExpressionType.Block:
                    case (ExpressionType)DbExpressionType.If:
                    case (ExpressionType)DbExpressionType.Declaration:
                    case (ExpressionType)DbExpressionType.Variable:
                    case (ExpressionType)DbExpressionType.Function:
                        return base.Visit(exp);

                    case ExpressionType.ArrayLength:
                    case ExpressionType.Quote:
                    case ExpressionType.TypeAs:
                    case ExpressionType.ArrayIndex:
                    case ExpressionType.TypeIs:
                    case ExpressionType.Parameter:
                    case ExpressionType.Lambda:
                    case ExpressionType.NewArrayInit:
                    case ExpressionType.NewArrayBounds:
                    case ExpressionType.Invoke:
                    case ExpressionType.MemberInit:
                    case ExpressionType.ListInit:
                    default:
                        if (!_forDebug)
                        {
                            throw new NotSupportedException(string.Format("The LINQ expression node of type {0} is not supported", exp.NodeType));
                        }
                        else
                        {
                            this.Write(string.Format("?{0}?(", exp.NodeType));
                            base.Visit(exp);
                            this.Write(")");
                            return exp;
                        }
                }
            }

            protected override Expression VisitMemberAccess(MemberExpression m)
            {
                //if (this._forDebug)
                //{
                    this.Visit(m.Expression);
                    this.Write(".");
                    var mapped = this.Linguist.Translator.Mapper.Mapping.GetEntity(m.Member, this.Linguist.Translator.Mapper.Mapping.RepositoryType);
                    var name = mapped.Properties.Single(p => p.Property == m.Member || p.Property.Name.Equals(m.Member.Name)).StorageProperty;
                    this.Write(name);
                    return m;
                //}
                //else
                //{
                //    throw new NotSupportedException(string.Format("The member access '{0}' is not supported", m.Member));
                //}
            }

            protected override Expression VisitMethodCall(MethodCallExpression m)
            {
                if (m.Method.DeclaringType == typeof(Decimal))
                {
                    switch (m.Method.Name)
                    {
                        case "Add":
                        case "Subtract":
                        case "Multiply":
                        case "Divide":
                        case "Remainder":
                            this.Write("(");
                            this.VisitValue(m.Arguments[0]);
                            this.Write(" ");
                            this.Write(GetOperator(m.Method.Name));
                            this.Write(" ");
                            this.VisitValue(m.Arguments[1]);
                            this.Write(")");
                            return m;
                        case "Negate":
                            this.Write("-");
                            this.Visit(m.Arguments[0]);
                            this.Write("");
                            return m;
                        case "Compare":
                            this.Visit(Expression.Condition(
                                Expression.Equal(m.Arguments[0], m.Arguments[1]),
                                Expression.Constant(0),
                                Expression.Condition(
                                    Expression.LessThan(m.Arguments[0], m.Arguments[1]),
                                    Expression.Constant(-1),
                                    Expression.Constant(1)
                                    )));
                            return m;
                    }
                }
                else if (m.Method.Name == "ToString" && m.Object.Type == typeof(string))
                {
                    return this.Visit(m.Object);  // no op
                }
                else if (m.Method.Name == "Equals")
                {
                    if (m.Method.IsStatic && m.Method.DeclaringType == typeof(object))
                    {
                        this.Write("(");
                        this.Visit(m.Arguments[0]);
                        this.Write(" = ");
                        this.Visit(m.Arguments[1]);
                        this.Write(")");
                        return m;
                    }
                    else if (!m.Method.IsStatic && m.Arguments.Count == 1 && m.Arguments[0].Type == m.Object.Type)
                    {
                        this.Write("(");
                        this.Visit(m.Object);
                        this.Write(" = ");
                        this.Visit(m.Arguments[0]);
                        this.Write(")");
                        return m;
                    }
                }
                if (this._forDebug)
                {
                    if (m.Object != null)
                    {
                        this.Visit(m.Object);
                        this.Write(".");
                    }
                    this.Write(string.Format("?{0}?", m.Method.Name));
                    this.Write("(");
                    for (int i = 0; i < m.Arguments.Count; i++)
                    {
                        if (i > 0)
                            this.Write(", ");
                        this.Visit(m.Arguments[i]);
                    }
                    this.Write(")");
                    return m;
                }
                else
                {
                    throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
                }
            }

            protected virtual bool IsInteger(Type type)
            {
                return TypeHelper.IsInteger(type);
            }

            protected override NewExpression VisitNew(NewExpression nex)
            {
                if (this._forDebug)
                {
                    this.Write("?new?");
                    this.Write(nex.Type.Name);
                    this.Write("(");
                    for (int i = 0; i < nex.Arguments.Count; i++)
                    {
                        if (i > 0)
                            this.Write(", ");
                        this.Visit(nex.Arguments[i]);
                    }
                    this.Write(")");
                    return nex;
                }
                else
                {
                    throw new NotSupportedException(string.Format("The construtor for '{0}' is not supported", nex.Constructor.DeclaringType));
                }
            }

            protected override Expression VisitUnary(UnaryExpression u)
            {
                string op = this.GetOperator(u);
                switch (u.NodeType)
                {
                    case ExpressionType.Not:
                        if (IsBoolean(u.Operand.Type) || op.Length > 1)
                        {
                            this.Write(op);
                            this.Write(" ");
                            this.VisitPredicate(u.Operand);
                        }
                        else
                        {
                            this.Write(op);
                            this.VisitValue(u.Operand);
                        }
                        break;
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                        this.Write(op);
                        this.VisitValue(u.Operand);
                        break;
                    case ExpressionType.UnaryPlus:
                        this.VisitValue(u.Operand);
                        break;
                    case ExpressionType.Convert:
                        // ignore conversions for now
                        this.Visit(u.Operand);
                        break;
                    default:
                        if (this._forDebug)
                        {
                            this.Write(string.Format("?{0}?", u.NodeType));
                            this.Write("(");
                            this.Visit(u.Operand);
                            this.Write(")");
                            return u;
                        }
                        else
                        {
                            throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
                        }
                }
                return u;
            }

            protected override Expression VisitBinary(BinaryExpression b)
            {
                string op = this.GetOperator(b);
                Expression left = b.Left;
                Expression right = b.Right;

                this.Write("(");
                switch (b.NodeType)
                {
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                        if (this.IsBoolean(left.Type))
                        {
                            this.VisitPredicate(left);
                            this.Write(" ");
                            this.Write(op);
                            this.Write(" ");
                            this.VisitPredicate(right);
                        }
                        else
                        {
                            this.VisitValue(left);
                            this.Write(" ");
                            this.Write(op);
                            this.Write(" ");
                            this.VisitValue(right);
                        }
                        break;
                    case ExpressionType.Equal:
                        if (right.NodeType == ExpressionType.Constant)
                        {
                            ConstantExpression ce = (ConstantExpression)right;
                            if (ce.Value == null)
                            {
                                this.Visit(left);
                                this.Write(" IS NULL");
                                break;
                            }
                        }
                        else if (left.NodeType == ExpressionType.Constant)
                        {
                            ConstantExpression ce = (ConstantExpression)left;
                            if (ce.Value == null)
                            {
                                this.Visit(right);
                                this.Write(" IS NULL");
                                break;
                            }
                        }
                        goto case ExpressionType.LessThan;
                    case ExpressionType.NotEqual:
                        if (right.NodeType == ExpressionType.Constant)
                        {
                            ConstantExpression ce = (ConstantExpression)right;
                            if (ce.Value == null)
                            {
                                this.Visit(left);
                                this.Write(" IS NOT NULL");
                                break;
                            }
                        }
                        else if (left.NodeType == ExpressionType.Constant)
                        {
                            ConstantExpression ce = (ConstantExpression)left;
                            if (ce.Value == null)
                            {
                                this.Visit(right);
                                this.Write(" IS NOT NULL");
                                break;
                            }
                        }
                        goto case ExpressionType.LessThan;
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                        // check for special x.CompareTo(y) && type.Compare(x,y)
                        if (left.NodeType == ExpressionType.Call && right.NodeType == ExpressionType.Constant)
                        {
                            MethodCallExpression mc = (MethodCallExpression)left;
                            ConstantExpression ce = (ConstantExpression)right;
                            if (ce.Value != null && ce.Value.GetType() == typeof(int) && ((int)ce.Value) == 0)
                            {
                                if (mc.Method.Name == "CompareTo" && !mc.Method.IsStatic && mc.Arguments.Count == 1)
                                {
                                    left = mc.Object;
                                    right = mc.Arguments[0];
                                }
                                else if (
                                    (mc.Method.DeclaringType == typeof(string) || mc.Method.DeclaringType == typeof(decimal))
                                      && mc.Method.Name == "Compare" && mc.Method.IsStatic && mc.Arguments.Count == 2)
                                {
                                    left = mc.Arguments[0];
                                    right = mc.Arguments[1];
                                }
                            }
                        }
                        goto case ExpressionType.Add;
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.Divide:
                    case ExpressionType.Modulo:
                    case ExpressionType.ExclusiveOr:
                    case ExpressionType.LeftShift:
                    case ExpressionType.RightShift:
                        this.VisitValue(left);
                        this.Write(" ");
                        this.Write(op);
                        this.Write(" ");
                        this.VisitValue(right);
                        break;
                    default:
                        if (this._forDebug)
                        {
                            this.Write(string.Format("?{0}?", b.NodeType));
                            this.Write("(");
                            this.Visit(b.Left);
                            this.Write(", ");
                            this.Visit(b.Right);
                            this.Write(")");
                            return b;
                        }
                        else
                        {
                            throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
                        }
                }
                this.Write(")");
                return b;
            }

            protected virtual string GetOperator(string methodName)
            {
                switch (methodName)
                {
                    case "Add": return "+";
                    case "Subtract": return "-";
                    case "Multiply": return "*";
                    case "Divide": return "/";
                    case "Negate": return "-";
                    case "Remainder": return "%";
                    default: return null;
                }
            }

            protected virtual string GetOperator(UnaryExpression u)
            {
                switch (u.NodeType)
                {
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                        return "-";
                    case ExpressionType.UnaryPlus:
                        return "+";
                    case ExpressionType.Not:
                        return IsBoolean(u.Operand.Type) ? "NOT" : "~";
                    default:
                        return "";
                }
            }

            protected virtual string GetOperator(BinaryExpression b)
            {
                switch (b.NodeType)
                {
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                        return (IsBoolean(b.Left.Type)) ? "AND" : "&";
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                        return (IsBoolean(b.Left.Type) ? "OR" : "|");
                    case ExpressionType.Equal:
                        return "=";
                    case ExpressionType.NotEqual:
                        return "<>";
                    case ExpressionType.LessThan:
                        return "<";
                    case ExpressionType.LessThanOrEqual:
                        return "<=";
                    case ExpressionType.GreaterThan:
                        return ">";
                    case ExpressionType.GreaterThanOrEqual:
                        return ">=";
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                        return "+";
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                        return "-";
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                        return "*";
                    case ExpressionType.Divide:
                        return "/";
                    case ExpressionType.Modulo:
                        return "%";
                    case ExpressionType.ExclusiveOr:
                        return "^";
                    case ExpressionType.LeftShift:
                        return "<<";
                    case ExpressionType.RightShift:
                        return ">>";
                    default:
                        return "";
                }
            }

            protected virtual bool IsBoolean(Type type)
            {
                return type == typeof(bool) || type == typeof(bool?);
            }

            protected virtual bool IsPredicate(Expression expr)
            {
                switch (expr.NodeType)
                {
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                        return IsBoolean(((BinaryExpression)expr).Type);
                    case ExpressionType.Not:
                        return IsBoolean(((UnaryExpression)expr).Type);
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case (ExpressionType)DbExpressionType.IsNull:
                    case (ExpressionType)DbExpressionType.Between:
                    case (ExpressionType)DbExpressionType.Exists:
                    case (ExpressionType)DbExpressionType.In:
                        return true;
                    case ExpressionType.Call:
                        return IsBoolean(((MethodCallExpression)expr).Type);
                    default:
                        return false;
                }
            }

            protected virtual Expression VisitPredicate(Expression expr)
            {
                this.Visit(expr);
                if (!IsPredicate(expr))
                {
                    this.Write(" <> 0");
                }
                return expr;
            }

            protected virtual Expression VisitValue(Expression expr)
            {
                return this.Visit(expr);
            }

            protected override Expression VisitConditional(ConditionalExpression c)
            {
                if (this._forDebug)
                {
                    this.Write("?iff?(");
                    this.Visit(c.Test);
                    this.Write(", ");
                    this.Visit(c.IfTrue);
                    this.Write(", ");
                    this.Visit(c.IfFalse);
                    this.Write(")");
                    return c;
                }
                else
                {
                    throw new NotSupportedException(string.Format("Conditional expressions not supported"));
                }
            }

            protected override Expression VisitConstant(ConstantExpression c)
            {
                this.WriteValue(c.Value);
                return c;
            }

            protected virtual void WriteValue(object value)
            {
                if (value == null)
                {
                    this.Write("NULL");
                }
                else if (value.GetType().IsEnum)
                {
                    this.Write(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())));
                }
                else
                {
                    //switch (Type.GetTypeCode(value.GetType()))
                    //{
                    //    case TypeCode.Boolean:
                    //        this.Write(((bool)value) ? 1 : 0);
                    //        break;
                    //    case TypeCode.String:
                    //        this.Write("'");
                    //        this.Write(value);
                    //        this.Write("'");
                    //        break;
                    //    case TypeCode.Object:
                    //        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", value));
                    //    case TypeCode.Single:
                    //    case TypeCode.Double:
                    //        string str = value.ToString();
                    //        if (!str.Contains('.'))
                    //        {
                    //            str += ".0";
                    //        }
                    //        this.Write(str);
                    //        break;
                    //    default:
                    //        this.Write(value);
                    //        break;
                    //}
                    this.Write(value.ToOrientValueString());
                }
            }

            protected override Expression VisitField(FieldExpression field)
            {
                //if (field.Alias != null && !this.HideFieldAliases)
                //{
                //    this.WriteAliasName(GetAliasName(field.Alias));
                //    this.Write(".");
                //}
                if (field.Name.Equals("Id"))
                {
                    this.WriteFieldName("@rid");
                }
                //else if (field.Type.Implements<IIdentifiable>())
                //{
                //    var mapping = this.Linguist.Translator.Mapper.Mapping.GetEntity(field.Type, this.Linguist.Translator.Mapper.Mapping.RepositoryType);
                //    this.Write(mapping.StorageClass);
                //}
                else
                {
                    this.WriteFieldName(field.Name);
                    //WriteIfNullField(field);
                }
                return field;
            }

            //private void WriteIfNullField(FieldExpression field)
            //{
            //    this.Write(string.Format("IfNull({0}, {1}) as {0}", field.Name, FieldDefaultValue(field)));
            //}

            private string FieldDefaultValue(FieldExpression field)
            {

                if (field.Type.Equals(typeof(bool))
                    || field.Type.Equals(typeof(byte))
                    || field.Type.Equals(typeof(sbyte))
                    || field.Type.Equals(typeof(char))
                    || field.Type.Equals(typeof(short))
                    || field.Type.Equals(typeof(ushort))
                    || field.Type.Equals(typeof(int))
                    || field.Type.Equals(typeof(uint))
                    || field.Type.Equals(typeof(long))
                    || field.Type.Equals(typeof(ulong)))
                {
                    return 0.ToOrientValueString();
                }
                else if (field.Type.Equals(typeof(float))
                    || field.Type.Equals(typeof(double)))
                {
                    return (0.0d).ToOrientValueString();
                }
                else if (field.Type.Equals(typeof(decimal)))
                {
                    return (0.0m).ToOrientValueString();
                }
                else if (field.Type.Equals(typeof(DateTime)))
                {
                    return (new DateTime(1970, 1, 1, 0, 0, 0)).ToOrientValueString();
                }
                else if (field.Type.Equals(typeof(string)))
                {
                    return "".ToOrientValueString();
                }
                else return "null"; // not sure how to handle defaults for arrays...
            }

            protected override Expression VisitProjection(ProjectionExpression proj)
            {
                // treat these like scalar subqueries
                if ((proj.Projector is FieldExpression) || this._forDebug)
                {
                    this.Write("(");
                    this.WriteLine(Indentation.Inner);
                    this.Visit(proj.Select);
                    this.Write(")");
                    this.Indent(Indentation.Outer);
                }
                else
                {
                    throw new NotSupportedException("Non-scalar projections cannot be translated to SQL.");
                }
                return proj;
            }

            protected override Expression VisitSelect(SelectExpression select)
            {
                this.AddAliases(select.From);
                this.Write("SELECT ");
                if (select.IsDistinct)
                {
                    this.Write("DISTINCT ");
                }
                if (!IsDefaultProjection(select))
                {
                    // we only explicitly emit field names if the projection declares a 
                    // custom list of properties
                    this.WriteFields(select.Fields, ((IdentifiableExpression)select.From).Entity);
                }
                else
                {
                    this.Write("*");
                }
                this.Write(", @class as +class, @rid as +rid, @version as +version ");
                if (select.From != null)
                {
                    this.WriteLine(Indentation.Same);
                    this.Write("FROM ");
                    this.VisitSource(select.From);
                }
                if (select.Where != null)
                {
                    this.WriteLine(Indentation.Same);
                    this.Write("WHERE ");
                    this.VisitPredicate(select.Where);
                }
                if (select.GroupBy != null && select.GroupBy.Count > 0)
                {
                    this.WriteLine(Indentation.Same);
                    this.Write("GROUP BY ");
                    for (int i = 0, n = select.GroupBy.Count; i < n; i++)
                    {
                        if (i > 0)
                        {
                            this.Write(", ");
                        }
                        this.VisitValue(select.GroupBy[i]);
                    }
                }
                if (select.OrderBy != null && select.OrderBy.Count > 0)
                {
                    this.WriteLine(Indentation.Same);
                    this.Write("ORDER BY ");
                    for (int i = 0, n = select.OrderBy.Count; i < n; i++)
                    {
                        OrderExpression exp = select.OrderBy[i];
                        if (i > 0)
                        {
                            this.Write(", ");
                        }
                        this.VisitValue(exp.Expression);
                        if (exp.OrderType != OrderType.Ascending)
                        {
                            this.Write(" DESC");
                        }
                    }
                }
                if (select.Take != null)
                {
                    this.WriteLimitClause(select.Take);
                }
                return select;
            }

            protected virtual bool IsDefaultProjection(SelectExpression select)
            {
                var publicMembers = GetMembers(((IdentifiableExpression)select.From).Entity);
                if (publicMembers.Count() != select.Fields.Count()) return false;
                foreach(var field in publicMembers)
                {
                    if (!select.Fields.Any(fd => fd.Name.Equals(field.Name)))
                        return false;
                }
                return true;
            }

            protected IEnumerable<FieldDeclaration> GetMembers(IMappedEntity entity)
            {
                var members = this.Linguist.Translator.Mapper.Mapping.GetMappedMembers(entity);
                foreach(var mi in members.Where(m => m.MemberType == System.Reflection.MemberTypes.Property))
                {
                    var memberType = TypeHelper.GetMemberType(mi);
                    var storageType = new OStorageType(memberType);
                    yield return new FieldDeclaration(mi.Name, new FieldExpression(memberType, storageType, null, mi.Name), storageType);
                }
            }

            protected virtual void WriteLimitClause(Expression expression)
            {
                this._sw.WriteLine();
                this.Write("LIMIT ");
                this.Visit(expression);
            }

            protected virtual void WriteFields(ReadOnlyCollection<FieldDeclaration> fields, IMappedEntity entity)
            {
                if (fields.Count > 0)
                {
                    for (int i = 0, n = fields.Count; i < n; i++)
                    {
                        FieldDeclaration field = fields[i];
                        if (i > 0)
                        {
                            this.Write(", ");
                        }
                        if (entity.EntityType.Implements<IAssociation>()
                            && (field.Name.Equals("out") || field.Name.Equals("in")))
                        {
                            this.Write(field.Name);
                        }
                        else
                        {
                            FieldExpression c = this.VisitValue(field.Expression) as FieldExpression;
                            if (!string.IsNullOrEmpty(field.Name) && (c == null || c.Name != field.Name))
                            {
                                this.Write(" ");
                                this.WriteAsFieldName(field.Name);
                            }
                        }
                    }
                }
                else
                {
                    this.Write("NULL ");
                    if (this._isNested)
                    {
                        this.WriteAsFieldName("tmp");
                        this.Write(" ");
                    }
                }
            }

            protected override Expression VisitSource(Expression source)
            {
                bool saveIsNested = this._isNested;
                this._isNested = true;
                switch ((DbExpressionType)source.NodeType)
                {
                    case DbExpressionType.Identifiable:
                        IdentifiableExpression table = (IdentifiableExpression)source;
                        this.WriteIdentifiableName(table.Entity.StorageClass);
                        if (!this.HideIdentifiableAliases)
                        {
                            this.Write(" ");
                            this.WriteAsAliasName(GetAliasName(table.Alias));
                        }
                        break;
                    case DbExpressionType.Select:
                        SelectExpression select = (SelectExpression)source;
                        this.Write("(");
                        this.WriteLine(Indentation.Inner);
                        this.Visit(select);
                        this.WriteLine(Indentation.Same);
                        this.Write(") ");
                        this.WriteAsAliasName(GetAliasName(select.Alias));
                        this.Indent(Indentation.Outer);
                        break;
                    case DbExpressionType.Join:
                        this.VisitJoin((JoinExpression)source);
                        break;
                    default:
                        throw new InvalidOperationException("Select source is not valid type");
                }
                this._isNested = saveIsNested;
                return source;
            }

            protected override Expression VisitJoin(JoinExpression join)
            {
                this.VisitJoinLeft(join.Left);
                this.WriteLine(Indentation.Same);
                switch (join.Join)
                {
                    case JoinType.CrossJoin:
                        this.Write("CROSS JOIN ");
                        break;
                    case JoinType.InnerJoin:
                        this.Write("INNER JOIN ");
                        break;
                    case JoinType.CrossApply:
                        this.Write("CROSS APPLY ");
                        break;
                    case JoinType.OuterApply:
                        this.Write("OUTER APPLY ");
                        break;
                    case JoinType.LeftOuter:
                    case JoinType.SingletonLeftOuter:
                        this.Write("LEFT OUTER JOIN ");
                        break;
                }
                this.VisitJoinRight(join.Right);
                if (join.Condition != null)
                {
                    this.WriteLine(Indentation.Inner);
                    this.Write("ON ");
                    this.VisitPredicate(join.Condition);
                    this.Indent(Indentation.Outer);
                }
                return join;
            }

            protected virtual Expression VisitJoinLeft(Expression source)
            {
                return this.VisitSource(source);
            }

            protected virtual Expression VisitJoinRight(Expression source)
            {
                return this.VisitSource(source);
            }

            protected virtual void WriteAggregateName(string aggregateName)
            {
                switch (aggregateName)
                {
                    case "Average":
                        this.Write("AVG");
                        break;
                    case "LongCount":
                        this.Write("COUNT");
                        break;
                    default:
                        this.Write(aggregateName.ToUpper());
                        break;
                }
            }

            protected virtual bool RequiresAsteriskWhenNoArgument(string aggregateName)
            {
                return aggregateName == "Count" || aggregateName == "LongCount";
            }

            protected override Expression VisitAggregate(AggregateExpression aggregate)
            {
                this.WriteAggregateName(aggregate.AggregateName);
                this.Write("(");
                if (aggregate.IsDistinct)
                {
                    this.Write("DISTINCT ");
                }
                if (aggregate.Argument != null)
                {
                    this.VisitValue(aggregate.Argument);
                }
                else if (RequiresAsteriskWhenNoArgument(aggregate.AggregateName))
                {
                    this.Write("*");
                }
                this.Write(")");
                return aggregate;
            }

            protected override Expression VisitIsNull(IsNullExpression isnull)
            {
                this.VisitValue(isnull.Expression);
                this.Write(" IS NULL");
                return isnull;
            }

            protected override Expression VisitBetween(BetweenExpression between)
            {
                this.VisitValue(between.Expression);
                this.Write(" BETWEEN ");
                this.VisitValue(between.Lower);
                this.Write(" AND ");
                this.VisitValue(between.Upper);
                return between;
            }

            protected override Expression VisitRowNumber(RowNumberExpression rowNumber)
            {
                throw new NotSupportedException();
            }

            protected override Expression VisitScalar(ScalarExpression subquery)
            {
                this.Write("(");
                this.WriteLine(Indentation.Inner);
                this.Visit(subquery.Select);
                this.WriteLine(Indentation.Same);
                this.Write(")");
                this.Indent(Indentation.Outer);
                return subquery;
            }

            protected override Expression VisitExists(ExistsExpression exists)
            {
                this.Write("EXISTS(");
                this.WriteLine(Indentation.Inner);
                this.Visit(exists.Select);
                this.WriteLine(Indentation.Same);
                this.Write(")");
                this.Indent(Indentation.Outer);
                return exists;
            }

            protected override Expression VisitIn(InExpression @in)
            {
                if (@in.Values != null)
                {
                    if (@in.Values.Count == 0)
                    {
                        this.Write("0 <> 0");
                    }
                    else
                    {
                        this.VisitValue(@in.Expression);
                        this.Write(" IN (");
                        for (int i = 0, n = @in.Values.Count; i < n; i++)
                        {
                            if (i > 0) this.Write(", ");
                            this.VisitValue(@in.Values[i]);
                        }
                        this.Write(")");
                    }
                }
                else
                {
                    this.VisitValue(@in.Expression);
                    this.Write(" IN (");
                    this.WriteLine(Indentation.Inner);
                    this.Visit(@in.Select);
                    this.WriteLine(Indentation.Same);
                    this.Write(")");
                    this.Indent(Indentation.Outer);
                }
                return @in;
            }

            protected override Expression VisitNamedValue(NamedValueExpression value)
            {
                this.WriteParameterName(value.Name);
                return value;
            }

            protected override Expression VisitInsert(InsertCommandExpression insert)
            {
                if (insert.Identifiable.Entity.EntityType.Implements<IAssociation>())
                {
                    VisitInsertAssociation(insert);
                }
                else if (insert.Identifiable.Entity.EntityType.Implements<IThing>())
                {
                    VisitInsertThing(insert);
                }
                return insert;
            }

            protected virtual void VisitInsertThing(InsertCommandExpression insert)
            {
                this.Write("INSERT INTO ");
                this.WriteIdentifiableName(insert.Identifiable.Entity.StorageClass);

                this.Write("(");
                for (int i = 0, n = insert.Assignments.Count; i < n; i++)
                {
                    FieldAssignment ca = insert.Assignments[i];
                    if (i > 0) this.Write(", ");
                    this.WriteFieldName(ca.Field.Name);
                }
                this.Write(")");
                this.WriteLine(Indentation.Same);
                this.Write("VALUES (");
                for (int i = 0, n = insert.Assignments.Count; i < n; i++)
                {
                    FieldAssignment ca = insert.Assignments[i];
                    if (i > 0) this.Write(", ");
                    this.Visit(ca.Expression);
                }
                this.Write(") RETURN @this");
            }

            protected virtual void VisitInsertAssociation(InsertCommandExpression insert)
            {
                this.Write("CREATE EDGE ");
                this.WriteIdentifiableName(insert.Identifiable.Entity.StorageClass);
                this.Write(" FROM ");
                this.Visit(insert.Assignments.Single(fa => fa.Field.Name.Equals("out")).Expression);
                this.Write(" TO ");
                this.Visit(insert.Assignments.Single(fa => fa.Field.Name.Equals("in")).Expression);
                if (insert.Assignments.Count > 2)
                {
                    this.Write(" SET ");
                    bool comma = false;
                    foreach (var assignment in insert.Assignments.Where(fa => fa.Field.Name != "out" && fa.Field.Name != "in"))
                    {
                        if (comma) this.Write(", ");
                        this.Visit(assignment.Field);
                        this.Write(" = ");
                        if (assignment.Expression is ConstantExpression)
                        {
                            this.Write(((ConstantExpression)assignment.Expression).Value.ToOrientValueString(assignment.Field.Type));
                        }
                        else
                        {
                            this.Visit(assignment.Expression);
                        }
                        comma = true;
                    }
                }
            }

            protected override Expression VisitUpdate(UpdateCommandExpression update)
            {
                this.Write("UPDATE ");
                this.WriteIdentifiableName(update.Identifiable.Entity.StorageClass);
                this.WriteLine(Indentation.Same);
                bool saveHide = this.HideFieldAliases;
                this.HideFieldAliases = true;
                this.Write("SET ");
                for (int i = 0, n = update.Assignments.Count; i < n; i++)
                {
                    FieldAssignment ca = update.Assignments[i];
                    if (i > 0) this.Write(", ");
                    this.Visit(ca.Field);
                    this.Write(" = ");
                    this.Visit(ca.Expression);
                }
                this.WriteLine(Indentation.Same);
                this.Write("RETURN AFTER ");
                if (update.Where != null)
                {
                    //this.WriteLine(Indentation.Same);
                    this.Write("WHERE ");
                    this.VisitPredicate(update.Where);
                }
                this.HideFieldAliases = saveHide;
                return update;
            }

            protected override Expression VisitDelete(DeleteCommandExpression delete)
            {
                if (delete.Identifiable.Entity.EntityType.Implements<IThing>())
                {
                    this.Write("DELETE Vertex ");
                }
                else if (delete.Identifiable.Entity.EntityType.Implements<IAssociation>())
                {
                    this.Write("DELETE Edge ");
                }
                else throw new NotSupportedException("Only Association and Thing types are supported.");

                bool saveHideIdentifiable = this.HideIdentifiableAliases;
                bool saveHideField = this.HideFieldAliases;
                this.HideIdentifiableAliases = true;
                this.HideFieldAliases = true;
                this.VisitSource(delete.Identifiable);
                if (delete.Where != null)
                {
                    this.WriteLine(Indentation.Same);
                    this.Write("WHERE ");
                    this.VisitPredicate(delete.Where);
                }
                this.HideIdentifiableAliases = saveHideIdentifiable;
                this.HideFieldAliases = saveHideField;
                return delete;
            }

            protected override Expression VisitIf(IfCommandExpression ifx)
            {
                throw new NotSupportedException();
            }

            protected override Expression VisitBlock(BlockCommandExpression block)
            {
                throw new NotSupportedException();
            }

            protected override Expression VisitDeclaration(DeclarationCommand decl)
            {
                throw new NotSupportedException();
            }

            protected override Expression VisitVariable(VariableExpression vex)
            {
                this.WriteVariableName(vex.Name);
                return vex;
            }

            protected virtual void VisitStatement(Expression expression)
            {
                var p = expression as ProjectionExpression;
                if (p != null)
                {
                    this.Visit(p.Select);
                }
                else
                {
                    this.Visit(expression);
                }
            }

            protected override Expression VisitFunction(FunctionExpression func)
            {
                this.Write(func.Name);
                if (func.Arguments.Count > 0)
                {
                    this.Write("(");
                    for (int i = 0, n = func.Arguments.Count; i < n; i++)
                    {
                        if (i > 0) this.Write(", ");
                        this.Visit(func.Arguments[i]);
                    }
                    this.Write(")");
                }
                return func;
            }
        }
    }
}
