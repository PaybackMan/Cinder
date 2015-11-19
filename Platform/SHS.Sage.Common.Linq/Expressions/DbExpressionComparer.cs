using SHS.Sage.Linq.Expressions.Command;
using SHS.Sage.Linq.Expressions.Queryable;
using SHS.Sage.Linq.Translation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Expressions
{
    /// <summary>
    /// Determines if two expressions are equivalent. Supports DbExpression nodes.
    /// </summary>
    public class DbExpressionComparer : ExpressionComparer
    {
        ScopedDictionary<IdentifiableAlias, IdentifiableAlias> aliasScope;

        protected DbExpressionComparer(
            ScopedDictionary<ParameterExpression, ParameterExpression> parameterScope,
            Func<object, object, bool> fnCompare,
            ScopedDictionary<IdentifiableAlias, IdentifiableAlias> aliasScope)
            : base(parameterScope, fnCompare)
        {
            this.aliasScope = aliasScope;
        }

        public new static bool AreEqual(Expression a, Expression b)
        {
            return AreEqual(null, null, a, b, null);
        }

        public new static bool AreEqual(Expression a, Expression b, Func<object, object, bool> fnCompare)
        {
            return AreEqual(null, null, a, b, fnCompare);
        }

        public static bool AreEqual(ScopedDictionary<ParameterExpression, ParameterExpression> parameterScope, 
            ScopedDictionary<IdentifiableAlias, IdentifiableAlias> aliasScope, Expression a, Expression b)
        {
            return new DbExpressionComparer(parameterScope, null, aliasScope).Compare(a, b);
        }

        public static bool AreEqual(ScopedDictionary<ParameterExpression, ParameterExpression> parameterScope, 
            ScopedDictionary<IdentifiableAlias, IdentifiableAlias> aliasScope, Expression a, Expression b, Func<object, object, bool> fnCompare)
        {
            return new DbExpressionComparer(parameterScope, fnCompare, aliasScope).Compare(a, b);
        }

        protected override bool Compare(Expression a, Expression b)
        {
            if (a == b)
                return true;
            if (a == null || b == null)
                return false;
            if (a.NodeType != b.NodeType)
                return false;
            if (a.Type != b.Type)
                return false;
            switch ((DbExpressionType)a.NodeType)
            {
                case DbExpressionType.Identifiable:
                    return this.CompareIdentifiable((IdentifiableExpression)a, (IdentifiableExpression)b);
                case DbExpressionType.Field:
                    return this.CompareField((FieldExpression)a, (FieldExpression)b);
                case DbExpressionType.Select:
                    return this.CompareSelect((SelectExpression)a, (SelectExpression)b);
                case DbExpressionType.Join:
                    return this.CompareJoin((JoinExpression)a, (JoinExpression)b);
                case DbExpressionType.Aggregate:
                    return this.CompareAggregate((AggregateExpression)a, (AggregateExpression)b);
                case DbExpressionType.Scalar:
                case DbExpressionType.Exists:
                case DbExpressionType.In:
                    return this.CompareSubquery((SubqueryExpression)a, (SubqueryExpression)b);
                case DbExpressionType.AggregateSubquery:
                    return this.CompareAggregateSubquery((AggregateSubqueryExpression)a, (AggregateSubqueryExpression)b);
                case DbExpressionType.IsNull:
                    return this.CompareIsNull((IsNullExpression)a, (IsNullExpression)b);
                case DbExpressionType.Between:
                    return this.CompareBetween((BetweenExpression)a, (BetweenExpression)b);
                case DbExpressionType.RowCount:
                    return this.CompareRowNumber((RowNumberExpression)a, (RowNumberExpression)b);
                case DbExpressionType.Projection:
                    return this.CompareProjection((ProjectionExpression)a, (ProjectionExpression)b);
                case DbExpressionType.NamedValue:
                    return this.CompareNamedValue((NamedValueExpression)a, (NamedValueExpression)b);
                case DbExpressionType.Insert:
                    return this.CompareInsert((InsertCommandExpression)a, (InsertCommandExpression)b);
                case DbExpressionType.Update:
                    return this.CompareUpdate((UpdateCommandExpression)a, (UpdateCommandExpression)b);
                case DbExpressionType.Delete:
                    return this.CompareDelete((DeleteCommandExpression)a, (DeleteCommandExpression)b);
                case DbExpressionType.Batch:
                    return this.CompareBatch((BatchExpression)a, (BatchExpression)b);
                case DbExpressionType.Function:
                    return this.CompareFunction((FunctionExpression)a, (FunctionExpression)b);
                case DbExpressionType.Entity:
                    return this.CompareEntity((EntityExpression)a, (EntityExpression)b);
                case DbExpressionType.If:
                    return this.CompareIf((IfCommandExpression)a, (IfCommandExpression)b);
                case DbExpressionType.Block:
                    return this.CompareBlock((BlockCommandExpression)a, (BlockCommandExpression)b);
                default:
                    return base.Compare(a, b);
            }
        }

        protected virtual bool CompareIdentifiable(IdentifiableExpression a, IdentifiableExpression b)
        {
            return a.Name == b.Name;
        }

        protected virtual bool CompareField(FieldExpression a, FieldExpression b)
        {
            return this.CompareAlias(a.Alias, b.Alias) && a.Name == b.Name;
        }

        protected virtual bool CompareAlias(IdentifiableAlias a, IdentifiableAlias b)
        {
            if (this.aliasScope != null)
            {
                IdentifiableAlias mapped;
                if (this.aliasScope.TryGetValue(a, out mapped))
                    return mapped == b;
            }
            return a == b;
        }

        protected virtual bool CompareSelect(SelectExpression a, SelectExpression b)
        {
            var save = this.aliasScope;
            try
            {
                if (!this.Compare(a.From, b.From))
                    return false;

                this.aliasScope = new ScopedDictionary<IdentifiableAlias, IdentifiableAlias>(save);
                this.MapAliases(a.From, b.From);

                return this.Compare(a.Where, b.Where)
                    && this.CompareOrderList(a.OrderBy, b.OrderBy)
                    && this.CompareExpressionList(a.GroupBy, b.GroupBy)
                    && this.Compare(a.Skip, b.Skip)
                    && this.Compare(a.Take, b.Take)
                    && a.IsDistinct == b.IsDistinct
                    && a.IsReverse == b.IsReverse
                    && this.CompareFieldDeclarations(a.Fields, b.Fields);
            }
            finally
            {
                this.aliasScope = save;
            }
        }

        private void MapAliases(Expression a, Expression b)
        {
            IdentifiableAlias[] prodA = DeclaredAliasGatherer.Gather(a).ToArray();
            IdentifiableAlias[] prodB = DeclaredAliasGatherer.Gather(b).ToArray();
            for (int i = 0, n = prodA.Length; i < n; i++)
            {
                this.aliasScope.Add(prodA[i], prodB[i]);
            }
        }

        protected virtual bool CompareOrderList(ReadOnlyCollection<OrderExpression> a, ReadOnlyCollection<OrderExpression> b)
        {
            if (a == b)
                return true;
            if (a == null || b == null)
                return false;
            if (a.Count != b.Count)
                return false;
            for (int i = 0, n = a.Count; i < n; i++)
            {
                if (a[i].OrderType != b[i].OrderType ||
                    !this.Compare(a[i].Expression, b[i].Expression))
                    return false;
            }
            return true;
        }

        protected virtual bool CompareFieldDeclarations(ReadOnlyCollection<FieldDeclaration> a, ReadOnlyCollection<FieldDeclaration> b)
        {
            if (a == b)
                return true;
            if (a == null || b == null)
                return false;
            if (a.Count != b.Count)
                return false;
            for (int i = 0, n = a.Count; i < n; i++)
            {
                if (!this.CompareFieldDeclaration(a[i], b[i]))
                    return false;
            }
            return true;
        }

        protected virtual bool CompareFieldDeclaration(FieldDeclaration a, FieldDeclaration b)
        {
            return a.Name == b.Name && this.Compare(a.Expression, b.Expression);
        }

        protected virtual bool CompareJoin(JoinExpression a, JoinExpression b)
        {
            if (a.Join != b.Join || !this.Compare(a.Left, b.Left))
                return false;

            if (a.Join == JoinType.CrossApply || a.Join == JoinType.OuterApply)
            {
                var save = this.aliasScope;
                try
                {
                    this.aliasScope = new ScopedDictionary<IdentifiableAlias, IdentifiableAlias>(this.aliasScope);
                    this.MapAliases(a.Left, b.Left);

                    return this.Compare(a.Right, b.Right)
                        && this.Compare(a.Condition, b.Condition);
                }
                finally
                {
                    this.aliasScope = save;
                }
            }
            else
            {
                return this.Compare(a.Right, b.Right)
                    && this.Compare(a.Condition, b.Condition);
            }
        }

        protected virtual bool CompareAggregate(AggregateExpression a, AggregateExpression b)
        {
            return a.AggregateName == b.AggregateName && this.Compare(a.Argument, b.Argument);
        }

        protected virtual bool CompareIsNull(IsNullExpression a, IsNullExpression b)
        {
            return this.Compare(a.Expression, b.Expression);
        }

        protected virtual bool CompareBetween(BetweenExpression a, BetweenExpression b)
        {
            return this.Compare(a.Expression, b.Expression)
                && this.Compare(a.Lower, b.Lower)
                && this.Compare(a.Upper, b.Upper);
        }

        protected virtual bool CompareRowNumber(RowNumberExpression a, RowNumberExpression b)
        {
            return this.CompareOrderList(a.OrderBy, b.OrderBy);
        }

        protected virtual bool CompareNamedValue(NamedValueExpression a, NamedValueExpression b)
        {
            return a.Name == b.Name && this.Compare(a.Value, b.Value);
        }

        protected virtual bool CompareSubquery(SubqueryExpression a, SubqueryExpression b)
        {
            if (a.NodeType != b.NodeType)
                return false;
            switch ((DbExpressionType)a.NodeType)
            {
                case DbExpressionType.Scalar:
                    return this.CompareScalar((ScalarExpression)a, (ScalarExpression)b);
                case DbExpressionType.Exists:
                    return this.CompareExists((ExistsExpression)a, (ExistsExpression)b);
                case DbExpressionType.In:
                    return this.CompareIn((InExpression)a, (InExpression)b);
            }
            return false;
        }

        protected virtual bool CompareScalar(ScalarExpression a, ScalarExpression b)
        {
            return this.Compare(a.Select, b.Select);
        }

        protected virtual bool CompareExists(ExistsExpression a, ExistsExpression b)
        {
            return this.Compare(a.Select, b.Select);
        }

        protected virtual bool CompareIn(InExpression a, InExpression b)
        {
            return this.Compare(a.Expression, b.Expression)
                && this.Compare(a.Select, b.Select)
                && this.CompareExpressionList(a.Values, b.Values);
        }

        protected virtual bool CompareAggregateSubquery(AggregateSubqueryExpression a, AggregateSubqueryExpression b)
        {
            return this.Compare(a.AggregateAsSubquery, b.AggregateAsSubquery)
                && this.Compare(a.AggregateInGroupSelect, b.AggregateInGroupSelect)
                && a.GroupByAlias == b.GroupByAlias;
        }

        protected virtual bool CompareProjection(ProjectionExpression a, ProjectionExpression b)
        {
            if (!this.Compare(a.Select, b.Select))
                return false;

            var save = this.aliasScope;
            try
            {
                this.aliasScope = new ScopedDictionary<IdentifiableAlias, IdentifiableAlias>(this.aliasScope);
                this.aliasScope.Add(a.Select.Alias, b.Select.Alias);

                return this.Compare(a.Projector, b.Projector)
                    && this.Compare(a.Aggregator, b.Aggregator)
                    && a.IsSingleton == b.IsSingleton;
            }
            finally
            {
                this.aliasScope = save;
            }
        }

        protected virtual bool CompareInsert(InsertCommandExpression x, InsertCommandExpression y)
        {
            return this.Compare(x.Identifiable, y.Identifiable)
                && this.CompareFieldAssignments(x.Assignments, y.Assignments);
        }

        protected virtual bool CompareFieldAssignments(ReadOnlyCollection<FieldAssignment> x, ReadOnlyCollection<FieldAssignment> y)
        {
            if (x == y)
                return true;
            if (x.Count != y.Count)
                return false;
            for (int i = 0, n = x.Count; i < n; i++)
            {
                if (!this.Compare(x[i]. Field, y[i]. Field) || !this.Compare(x[i].Expression, y[i].Expression))
                    return false;
            }
            return true;
        }

        protected virtual bool CompareUpdate(UpdateCommandExpression x, UpdateCommandExpression y)
        {
            return this.Compare(x.Identifiable, y.Identifiable) && this.Compare(x.Where, y.Where) && this.CompareFieldAssignments(x.Assignments, y.Assignments);
        }

        protected virtual bool CompareDelete(DeleteCommandExpression x, DeleteCommandExpression y)
        {
            return this.Compare(x.Identifiable, y.Identifiable) && this.Compare(x.Where, y.Where);
        }

        protected virtual bool CompareBatch(BatchExpression x, BatchExpression y)
        {
            return this.Compare(x.Input, y.Input) && this.Compare(x.Operation, y.Operation)
                && this.Compare(x.BatchSize, y.BatchSize) && this.Compare(x.Stream, y.Stream);
        }

        protected virtual bool CompareIf(IfCommandExpression x, IfCommandExpression y)
        {
            return this.Compare(x.Check, y.Check) && this.Compare(x.IfTrue, y.IfTrue) && this.Compare(x.IfFalse, y.IfFalse);
        }

        protected virtual bool CompareBlock(BlockCommandExpression x, BlockCommandExpression y)
        {
            if (x.Commands.Count != y.Commands.Count)
                return false;
            for (int i = 0, n = x.Commands.Count; i < n; i++)
            {
                if (!this.Compare(x.Commands[i], y.Commands[i]))
                    return false;
            }
            return true;
        }

        protected virtual bool CompareFunction(FunctionExpression x, FunctionExpression y)
        {
            return x.Name == y.Name && this.CompareExpressionList(x.Arguments, y.Arguments);
        }

        protected virtual bool CompareEntity(EntityExpression x, EntityExpression y)
        {
            return x.Entity == y.Entity && this.Compare(x.Expression, y.Expression);
        }
    }
}
