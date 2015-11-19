using SHS.Sage.Linq.Expressions;
using SHS.Sage.Linq.Expressions.Queryable;
using SHS.Sage.Linq.Language;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Translation
{
    /// <summary>
    /// Removes duplicate field declarations that refer to the same underlying field
    /// </summary>
    public class RedundantFieldRemover : DbExpressionVisitor
    {
        Dictionary<FieldExpression, FieldExpression> map;

        protected RedundantFieldRemover()
        {
            this.map = new Dictionary<FieldExpression, FieldExpression>();
        }

        public static Expression Remove(Expression expression)
        {
            return new RedundantFieldRemover().Visit(expression);
        }

        protected override Expression VisitField(FieldExpression field)
        {
            FieldExpression mapped;
            if (this.map.TryGetValue(field, out mapped))
            {
                return mapped;
            }
            return field;
        }

        protected override Expression VisitSelect(SelectExpression select)
        {
            select = (SelectExpression)base.VisitSelect(select);

            // look for redundant field declarations
            List<FieldDeclaration> cols = select.Fields.OrderBy(c => c.Name).ToList();
            BitArray removed = new BitArray(select.Fields.Count);
            bool anyRemoved = false;
            for (int i = 0, n = cols.Count; i < n - 1; i++)
            {
                FieldDeclaration ci = cols[i];
                FieldExpression cix = ci.Expression as FieldExpression;
                StorageType qt = cix != null ? cix.QueryType : ci.QueryType;
                FieldExpression cxi = new FieldExpression(ci.Expression.Type, qt, select.Alias, ci.Name);
                for (int j = i + 1; j < n; j++)
                {
                    if (!removed.Get(j))
                    {
                        FieldDeclaration cj = cols[j];
                        if (SameExpression(ci.Expression, cj.Expression))
                        {
                            // any reference to 'j' should now just be a reference to 'i'
                            FieldExpression cxj = new FieldExpression(cj.Expression.Type, qt, select.Alias, cj.Name);
                            this.map.Add(cxj, cxi);
                            removed.Set(j, true);
                            anyRemoved = true;
                        }
                    }
                }
            }
            if (anyRemoved)
            {
                List<FieldDeclaration> newDecls = new List<FieldDeclaration>();
                for (int i = 0, n = cols.Count; i < n; i++)
                {
                    if (!removed.Get(i))
                    {
                        newDecls.Add(cols[i]);
                    }
                }
                select = select.SetFields(newDecls);
            }
            return select;
        }

        bool SameExpression(Expression a, Expression b)
        {
            if (a == b) return true;
            FieldExpression ca = a as FieldExpression;
            FieldExpression cb = b as FieldExpression;
            return (ca != null && cb != null && ca.Alias == cb.Alias && ca.Name == cb.Name);
        }
    }
}
