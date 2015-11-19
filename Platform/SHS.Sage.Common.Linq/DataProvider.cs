using SHS.Sage.Linq.Expressions;
using SHS.Sage.Linq.Language;
using SHS.Sage.Linq.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public abstract class DataProvider : QueryProvider, ICreateExecutor, IProvideRepository
    {
        QueryLanguage _language;
        QueryMapping _mapping;
        QueryPolicy _policy;
        TextWriter _log;
        ICacheQueries _cache;

        public DataProvider(IRepository repository, QueryLanguage language, QueryMapping mapping, QueryPolicy policy)
        {
            if (language == null)
                throw new InvalidOperationException("Language not specified");
            if (mapping == null)
                throw new InvalidOperationException("Mapping not specified");
            if (policy == null)
                throw new InvalidOperationException("Policy not specified");
            this._language = language;
            this._mapping = mapping;
            this._policy = policy;
            this.Repository = repository;
        }

        public QueryMapping Mapping
        {
            get { return this._mapping; }
        }

        public QueryLanguage Language
        {
            get { return this._language; }
        }

        public QueryPolicy Policy
        {
            get { return this._policy; }

            set
            {
                if (value == null)
                {
                    this._policy = new QueryPolicy();
                }
                else
                {
                    this._policy = value;
                }
            }
        }

        public TextWriter Log
        {
            get { return this._log; }
            set { this._log = value; }
        }

        public ICacheQueries Cache
        {
            get { return this._cache; }
            set { this._cache = value; }
        }

        public IRepository Repository { get; protected set; }

        public bool CanEvaluateLocally(Expression expression)
        {
            return this.Mapping.CanEvaluateLocally(expression);
        }

        public bool CanBeParameter(Expression expression)
        {
            Type type = TypeHelper.GetNonNullableType(expression.Type);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Object:
                    if (expression.Type == typeof(Byte[]) ||
                        expression.Type == typeof(Char[]))
                        return true;
                    return false;
                default:
                    return true;
            }
        }

        protected abstract QueryExecutor CreateExecutor();

        QueryExecutor ICreateExecutor.CreateExecutor()
        {
            return this.CreateExecutor();
        }

        public override string ToString(Expression queryExpression)
        {
            Expression plan = this.GetExecutionPlan(queryExpression);
            var commands = CommandGatherer.Gather(plan).Select(c => c.CommandText).ToArray();
            return string.Join("\n\n", commands);
        }

        /// <summary>
        /// Convert the query expression into an execution plan
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public virtual Expression GetExecutionPlan(Expression expression)
        {
            // strip off lambda for now
            LambdaExpression lambda = expression as LambdaExpression;
            if (lambda != null)
                expression = lambda.Body;

            QueryTranslator translator = this.CreateTranslator();

            // translate query into client & server parts
            Expression translation = translator.Translate(expression);

            var parameters = lambda != null ? lambda.Parameters : null;
            Expression provider = this.Find(expression, parameters, typeof(DataProvider));
            if (provider == null)
            {
                Expression rootQueryable = this.Find(expression, parameters, typeof(IQueryable));
                provider = Expression.Property(rootQueryable, typeof(IQueryable).GetProperty("Provider"));
            }
            // pass in the IQueryProvider parameter to the builder
            provider = Expression.Parameter(typeof(IQueryProvider), "provider");
            return translator.Police.BuildExecutionPlan(translation, provider);
        }

        private Expression Find(Expression expression, IList<ParameterExpression> parameters, Type type)
        {
            if (parameters != null)
            {
                Expression found = parameters.FirstOrDefault(p => type.IsAssignableFrom(p.Type));
                if (found != null)
                    return found;
            }
            return TypedSubtreeFinder.Find(expression, type);
        }

        protected virtual QueryTranslator CreateTranslator()
        {
            return new QueryTranslator(this._language, this._mapping, this._policy);
        }

        class CommandGatherer : DbExpressionVisitor
        {
            List<QueryCommand> commands = new List<QueryCommand>();

            public static ReadOnlyCollection<QueryCommand> Gather(Expression expression)
            {
                var gatherer = new CommandGatherer();
                gatherer.Visit(expression);
                return gatherer.commands.AsReadOnly();
            }

            protected override Expression VisitConstant(ConstantExpression c)
            {
                QueryCommand qc = c.Value as QueryCommand;
                if (qc != null)
                {
                    this.commands.Add(qc);
                }
                return c;
            }
        }

        public override object Execute(Expression expression)
        {
            LambdaExpression lambda = expression as LambdaExpression;

            if (this.Policy.UseQueryCache 
                && lambda == null 
                && this._cache != null 
                && expression.NodeType != ExpressionType.Constant)
            {
                return this._cache.Execute(expression);
            }

            Expression plan = this.GetExecutionPlan(expression);

            if (lambda != null)
            {
                // we'll only enter here when being called from cache invoker
                var parms = lambda.Parameters
                    .Union(new ParameterExpression[] 
                    {
                        (ParameterExpression)this.Find(plan, null, typeof(IQueryProvider))
                    });

                // make lambda type that extends current lambda to include IQueryProvider parameter
                var lambdaType = MakeLambdaType(parms, lambda.ReturnType);

                // this lambda will also require input of IQueryProvider instance as final parameter
                LambdaExpression fn = Expression.Lambda(lambdaType, plan, parms);
#if NOREFEMIT
                    return ExpressionEvaluator.CreateDelegate(fn);
#else
                // compile & return the execution plan so it can be used multiple times
                return fn.Compile();
#endif
            }
            else
            {
                // we'll enter here if caching is not enabled
                // compile the execution plan and invoke it
                var parms = new ParameterExpression[]
                {
                    (ParameterExpression)this.Find(plan, null, typeof(IQueryProvider))
                };
                // make lambda type that extends current lambda to include IQueryProvider parameter
                var lambdaType = MakeLambdaType(parms, typeof(object));
                var efn = Expression.Lambda(lambdaType, Expression.Convert(plan, typeof(object)), parms);
#if NOREFEMIT
                    return ExpressionEvaluator.Eval(efn, new object[] { this });
#else
                var fn = efn.Compile();
                try
                {
                    return fn.DynamicInvoke(this);
                }
                catch(TargetInvocationException tie)
                {
                    throw tie.InnerException;
                }
#endif
            }
        }

        /// <summary>
        /// Creates a new Func<> type to allow the passing of the IQueryProvider as the last input parameter
        /// </summary>
        /// <param name="parms"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private Type MakeLambdaType(IEnumerable<ParameterExpression> parms, Type type)
        {
            var parmsCount = parms.Count() + 1;
            var genType = typeof(Func<>);
            switch(parmsCount)
            {
                case 2:
                    {
                        genType = typeof(Func<,>);
                        break;
                    }
                case 3:
                    {
                        genType = typeof(Func<,,>);
                        break;
                    }
                case 4:
                    {
                        genType = typeof(Func<,,,>);
                        break;
                    }
                case 5:
                    {
                        genType = typeof(Func<,,,,>);
                        break;
                    }
                case 6:
                    {
                        genType = typeof(Func<,,,,,>);
                        break;
                    }
                case 7:
                    {
                        genType = typeof(Func<,,,,,,>);
                        break;
                    }
                case 8:
                    {
                        genType = typeof(Func<,,,,,,,>);
                        break;
                    }
                case 9:
                    {
                        genType = typeof(Func<,,,,,,,,>);
                        break;
                    }
                case 10:
                    {
                        genType = typeof(Func<,,,,,,,,,>);
                        break;
                    }
                case 11:
                    {
                        genType = typeof(Func<,,,,,,,,,,>);
                        break;
                    }
                case 12:
                    {
                        genType = typeof(Func<,,,,,,,,,,,>);
                        break;
                    }
                case 13:
                    {
                        genType = typeof(Func<,,,,,,,,,,,,>);
                        break;
                    }
                case 14:
                    {
                        genType = typeof(Func<,,,,,,,,,,,,,>);
                        break;
                    }
                case 15:
                    {
                        genType = typeof(Func<,,,,,,,,,,,,,,>);
                        break;
                    }
                default: throw new InvalidOperationException("Too many parameters");
            }
            var genTypes = parms.Select(p => p.Type).ToList();
            genTypes.Add(type);
            return genType.MakeGenericType(genTypes.ToArray());
        }

        public override IQueryable CreateQuery(Expression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(DataSet<>).MakeGenericType(elementType), new object[] { this, this.Repository, expression });
            }
            catch (System.Reflection.TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }
    }
}
