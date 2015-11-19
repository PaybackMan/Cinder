using SHS.Sage.Linq.Expressions;
using SHS.Sage.Linq.Expressions.Queryable;
using SHS.Sage.Linq.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Translation
{
    /// <summary>
    /// Splits an expression into two parts
    ///   1) a list of field declarations for sub-expressions that must be evaluated on the server
    ///   2) a expression that describes how to combine/project the fields back together into the correct result
    /// </summary>
    public class FieldProjector : DbExpressionVisitor
    {
        QueryLanguage language;
        Dictionary<FieldExpression, FieldExpression> map;
        List<FieldDeclaration> fields;
        HashSet<string> fieldNames;
        HashSet<Expression> candidates;
        HashSet<IdentifiableAlias> existingAliases;
        IdentifiableAlias newAlias;
        int iField;

        private FieldProjector(QueryLanguage language, Expression expression, IEnumerable<FieldDeclaration> existingFields, IdentifiableAlias newAlias, IEnumerable<IdentifiableAlias> existingAliases)
        {
            this.language = language;
            this.newAlias = newAlias;
            this.existingAliases = new HashSet<IdentifiableAlias>(existingAliases);
            this.map = new Dictionary<FieldExpression, FieldExpression>();
            if (existingFields != null)
            {
                this.fields = new List<FieldDeclaration>(existingFields);
                this.fieldNames = new HashSet<string>(existingFields.Select(c => c.Name));
            }
            else
            {
                this.fields = new List<FieldDeclaration>();
                this.fieldNames = new HashSet<string>();
            }
            this.candidates = Nominator.Nominate(language, expression);
        }

        public static ProjectedFields ProjectFields(QueryLanguage language, Expression expression, IEnumerable<FieldDeclaration> existingFields, IdentifiableAlias newAlias, IEnumerable<IdentifiableAlias> existingAliases)
        {
            FieldProjector projector = new FieldProjector(language, expression, existingFields, newAlias, existingAliases);
            Expression expr = projector.Visit(expression);
            return new ProjectedFields(expr, projector.fields.AsReadOnly());
        }

        public static ProjectedFields ProjectFields(QueryLanguage language, Expression expression, IEnumerable<FieldDeclaration> existingFields, IdentifiableAlias newAlias, params IdentifiableAlias[] existingAliases)
        {
            return ProjectFields(language, expression, existingFields, newAlias, (IEnumerable<IdentifiableAlias>)existingAliases);
        }

        protected override Expression Visit(Expression expression)
        {
            if (this.candidates.Contains(expression))
            {
                if (expression.NodeType == (ExpressionType)DbExpressionType.Field)
                {
                    FieldExpression field = (FieldExpression)expression;
                    FieldExpression mapped;
                    if (this.map.TryGetValue(field, out mapped))
                    {
                        return mapped;
                    }
                    // check for field that already refers to this field
                    foreach (FieldDeclaration existingField in this.fields)
                    {
                        FieldExpression cex = existingField.Expression as FieldExpression;
                        if (cex != null && cex.Alias == field.Alias && cex.Name == field.Name)
                        {
                            // refer to the field already in the field list
                            return new FieldExpression(field.Type, field.QueryType, this.newAlias, existingField.Name);
                        }
                    }
                    if (this.existingAliases.Contains(field.Alias))
                    {
                        int ordinal = this.fields.Count;
                        string fieldName = this.GetUniqueFieldName(field.Name);
                        this.fields.Add(new FieldDeclaration(fieldName, field, field.QueryType));
                        mapped = new FieldExpression(field.Type, field.QueryType, this.newAlias, fieldName);
                        this.map.Add(field, mapped);
                        this.fieldNames.Add(fieldName);
                        return mapped;
                    }
                    // must be referring to outer scope
                    return field;
                }
                else
                {
                    string fieldName = this.GetNextFieldName();
                    var colType = this.language.TypeSystem.GetStorageType(expression.Type);
                    this.fields.Add(new FieldDeclaration(fieldName, expression, colType));
                    return new FieldExpression(expression.Type, colType, this.newAlias, fieldName);
                }
            }
            else
            {
                return base.Visit(expression);
            }
        }

        private bool IsFieldNameInUse(string name)
        {
            return this.fieldNames.Contains(name);
        }

        private string GetUniqueFieldName(string name)
        {
            string baseName = name;
            int suffix = 1;
            while (this.IsFieldNameInUse(name))
            {
                name = baseName + (suffix++);
            }
            return name;
        }

        private string GetNextFieldName()
        {
            return this.GetUniqueFieldName("c" + (iField++));
        }

        /// <summary>
        /// Nominator is a class that walks an expression tree bottom up, determining the set of 
        /// candidate expressions that are possible fields of a select expression
        /// </summary>
        class Nominator : DbExpressionVisitor
        {
            QueryLanguage language;
            bool isBlocked;
            HashSet<Expression> candidates;

            private Nominator(QueryLanguage language)
            {
                this.language = language;
                this.candidates = new HashSet<Expression>();
                this.isBlocked = false;
            }

            internal static HashSet<Expression> Nominate(QueryLanguage language, Expression expression)
            {
                Nominator nominator = new Nominator(language);
                nominator.Visit(expression);
                return nominator.candidates;
            }

            protected override Expression Visit(Expression expression)
            {
                if (expression != null)
                {
                    bool saveIsBlocked = this.isBlocked;
                    this.isBlocked = false;
                    if (this.language.MustBeField(expression))
                    {
                        this.candidates.Add(expression);
                        // don't merge saveIsBlocked
                    }
                    else
                    {
                        base.Visit(expression);
                        if (!this.isBlocked)
                        {
                            if (this.language.CanBeField(expression))
                            {
                                this.candidates.Add(expression);
                            }
                            else
                            {
                                this.isBlocked = true;
                            }
                        }
                        this.isBlocked |= saveIsBlocked;
                    }
                }
                return expression;
            }

            protected override Expression VisitProjection(ProjectionExpression proj)
            {
                this.Visit(proj.Projector);
                return proj;
            }
        }
    }
}
