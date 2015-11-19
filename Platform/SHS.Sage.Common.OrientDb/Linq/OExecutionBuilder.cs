using SHS.Sage.Linq.Expressions;
using SHS.Sage.Linq.Expressions.Queryable;
using SHS.Sage.Linq.Language;
using SHS.Sage.Linq.Mapping;
using SHS.Sage.Linq.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SHS.Sage.Linq.Expressions.Command;
using System.Collections.ObjectModel;
using SHS.Sage.Linq.Translation;
using SHS.Sage.Mapping;

namespace SHS.Sage.Linq
{
    public class OExecutionBuilder : ExecutionBuilder
    {
        private Type _repoType;
        protected OExecutionBuilder(QueryLinguist linguist, QueryPolicy policy, OTypeBuilder typeBuilder, Expression executor)
            : base (linguist, policy, executor)
        {
            this.TypeBuilder = typeBuilder;
            _repoType = linguist.Translator.Mapper.Mapping.RepositoryType;
        }

        public OTypeBuilder TypeBuilder { get; private set; }

        public new static Expression Build(QueryLinguist linguist, QueryPolicy policy, Expression expression, Expression provider)
        {
            var executor = Expression.Parameter(typeof(QueryExecutor), "executor");
            var builder = new OExecutionBuilder(linguist, policy, new OTypeBuilder(linguist), executor);
            builder.Variables.Add(executor);
            builder.Initializers.Add(
                Expression.Convert(
                    Expression.Call(Expression.Convert(provider, typeof(ICreateExecutor)), "CreateExecutor", null, null),
                    typeof(QueryExecutor)));
            var result = builder.Build(expression);
            return result;
        }

        protected override Expression VisitField(FieldExpression field)
        {
            ParameterExpression fieldReader;
            int iOrdinal;
            if (this.Scope != null && this.Scope.TryGetValue(field, out fieldReader, out iOrdinal))
            {
                // different query types may not be able to guarantee the ordinal position of results
                // in the reader.  for these types, the Scope.UseOrdinalMapping will be set to false
                if (this.Scope.UseOrdinalMapping)
                {
                    MethodInfo method = OFieldReader.GetReaderMethod(field.Type, this.Scope.UseOrdinalMapping);
                    return Expression.Call(fieldReader, method, Expression.Constant(iOrdinal));
                }
                else
                {
                    MethodInfo method = OFieldReader.GetReaderMethod(field.Type, this.Scope.UseOrdinalMapping);
                    return Expression.Call(fieldReader, method, Expression.Constant(field.Name));
                }
            }
            else
            {
                System.Diagnostics.Debug.Fail(string.Format("Field not in scope: {0}", field));
            }
            return field;
        }

        protected override Expression VisitMemberInit(MemberInitExpression init)
        {
            if (!init.Type.IsSealed)
            {
                // replace the native initializer with a proxy which enables lazy loading of complex properties
                init = MakeWrappedEntity(init);
            }

            return base.VisitMemberInit(init);
        }

        protected virtual MemberInitExpression MakeWrappedEntity(MemberInitExpression init)
        {
            if (IsDefaultInit(init))
                return TypeBuilder.GetDefaultInitializer(
                    ((IMapEntities)this.Linguist.Translator.Mapper.Mapping).Single(me => me.EntityType == init.Type), 
                    true,
                    (IMapEntities)this.Linguist.Translator.Mapper.Mapping);
            else
                return TypeBuilder.GetCustomInitializer(init);
        }

        protected virtual bool IsDefaultInit(MemberInitExpression init)
        {
            var publicMembers = GetMembers(init.Type.Implements(typeof(IProxyIdentifiable)) ? init.Type.BaseType : init.Type);
            if (publicMembers.Count() != init.Bindings.Count()) return false;
            foreach (var field in publicMembers)
            {
                if (!init.Bindings.Any(mb => mb.Member.Name.Equals(field.Property.Name)))
                    return false;
            }
            return true;
        }

        protected IEnumerable<IMappedProperty> GetMembers(Type entityType)
        {
            foreach(var prop in this.Linguist.Translator.Mapper.Mapping.GetEntity(entityType, this._repoType).Properties)
            {
                yield return prop;
            }
        }

        protected override Expression VisitCommand(CommandExpression command)
        {
            return base.VisitCommand(command);
        }

        protected override Expression VisitInsert(InsertCommandExpression insert)
        {
            return base.VisitInsert(insert);
        }

        protected override bool IsMultipleCommands(CommandExpression command)
        {
            return base.IsMultipleCommands(command);
        }

        protected override Expression BuildExecuteCommand(CommandExpression command)
        {
            // parameterize query
            var expression = this.Parameterize(command);

            string commandText = this.Linguist.Format(expression);
            ReadOnlyCollection<NamedValueExpression> namedValues = NamedValueGatherer.Gather(expression);
            QueryCommand qc = new QueryCommand(commandText, namedValues.Select(v => new QueryParameter(v.Name, v.Type, v.QueryType)), false);
            Expression[] values = namedValues.Select(v => Expression.Convert(this.Visit(v.Value), typeof(object))).ToArray();

            if (command is OInsertCommandExpression)
            {
                // insert will directly return a populated instance without the need for a BlockCommandExpression
                // to execute first an INSERT followed by a SELECT - so here we utilize the same Execute method
                // as a projection in order to map the returned values into an entity instance in a single call
                return ExecuteInsert((OInsertCommandExpression)command, qc, values);
            }
            else if (command is OUpdateCommandExpression)
            {
                // update will directly return a populated instance without the need for a BlockCommandExpression
                // to execute first an INSERT followed by a SELECT - so here we utilize the same Execute method
                // as a projection in order to map the returned values into an entity instance in a single call
                return ExecuteUpdate((OUpdateCommandExpression)command, qc, values);
            }
            else
            {
                ProjectionExpression projection = ProjectionFinder.FindProjection(expression);
                if (projection != null)
                {
                    return this.ExecuteProjection(projection, qc, values);
                }
            }

            Expression plan = Expression.Call(this.Executor, "ExecuteCommand", null,
                Expression.Constant(qc),
                Expression.NewArrayInit(typeof(object), values)
                );

            return plan;
        }

        private Expression ExecuteUpdate(OUpdateCommandExpression update, QueryCommand command, Expression[] values)
        {
            var reader = Expression.Parameter(typeof(FieldReader), "r" + nReaders++);
            var saveScope = this.scope;

            var projection = this.Linguist.Translator.Mapper.GetQueryExpression(new OMappingEntity(update.Projection.Projector.Type, _repoType));
            var fields = new OFieldFinder().Find(update.Projection.Projector).Select(fe => new FieldDeclaration(fe.Name, fe, fe.QueryType)).ToArray();
            var alias = ((FieldExpression)fields[0].Expression).Alias;
            this.scope = new BuilderScope(
                this.scope,
                reader,
                alias,
                fields)
            {
                UseOrdinalMapping = false // we can't guarantee the order that properties will be returned, so we need to read by name
            };
            //var projector = Expression.Lambda(this.Visit(update.Projection.Projector), reader);
            var projector = RedirectProjector(update.Projection.Projector, reader, false);
            this.scope = saveScope;

            var entity = EntityFinder.Find(update.Projection.Projector);

            // call low-level execute directly on supplied DbQueryProvider
            Expression result = Expression.Call(this.Executor, "Execute", new Type[] { update.Identifiable.Entity.EntityType },
                Expression.Constant(command),
                projector,
                Expression.Constant(entity, typeof(MappingEntity)),
                Expression.NewArrayInit(typeof(object), values)
                );

            if (update.Projection.Aggregator != null)
            {
                // apply aggregator
                result = DbExpressionReplacer.Replace(update.Projection.Aggregator.Body, update.Projection.Aggregator.Parameters[0], result);
            }
            return result;
        }

        protected virtual Expression ExecuteInsert(OInsertCommandExpression insert, QueryCommand command, Expression[] values)
        {
            var reader = Expression.Parameter(typeof(FieldReader), "r" + nReaders++);
            var saveScope = this.scope;
            
            var projection = this.Linguist.Translator.Mapper.GetQueryExpression(new OMappingEntity(insert.Projection.Projector.Type, _repoType));
            var fields = new OFieldFinder().Find(insert.Projection.Projector).Select(fe => new FieldDeclaration(fe.Name, fe, fe.QueryType)).ToArray();
            var alias = ((FieldExpression)fields[0].Expression).Alias;
            this.scope = new BuilderScope(
                this.scope,
                reader,
                alias,
                fields)
            {
                UseOrdinalMapping = false // we can't guarantee the order that properties will be returned, so we need to read by name
            };
            //var projector = Expression.Lambda(this.Visit(insert.Projection.Projector), reader);
            var projector = RedirectProjector(insert.Projection.Projector, reader, false);
            this.scope = saveScope;

            var entity = EntityFinder.Find(insert.Projection.Projector);

            // call low-level execute directly on supplied DbQueryProvider
            Expression result = Expression.Call(this.Executor, "Execute", new Type[] { insert.Identifiable.Entity.EntityType },
                Expression.Constant(command),
                projector,
                Expression.Constant(entity, typeof(MappingEntity)),
                Expression.NewArrayInit(typeof(object), values)
                );

            if (insert.Projection.Aggregator != null)
            {
                // apply aggregator
                result = DbExpressionReplacer.Replace(insert.Projection.Aggregator.Body, insert.Projection.Aggregator.Parameters[0], result);
            }
            return result;
        }

        protected override Expression ExecuteProjection(ProjectionExpression projection, QueryCommand command, Expression[] values)
        {
            var saveScope = this.Scope;
            ParameterExpression reader = Expression.Parameter(typeof(FieldReader), "r" + nReaders++);
            this.Scope = new BuilderScope(this.Scope, reader, projection.Select.Alias, projection.Select.Fields)
            {
                UseOrdinalMapping = false // orient cannot guarantee ordinally mapped results - must map by name
            };

            var projector = RedirectProjector(projection.Projector, reader, true);

            this.Scope = saveScope;

            var entity = EntityFinder.Find(projection.Projector);

            if (entity != null && entity.EntityType == typeof(_Thing))
            {
                entity = new OMappingEntity(typeof(IIdentifiable),_repoType);
            }

            string methExecute = "Execute";

            // call low-level execute directly on supplied DbQueryProvider
            Expression result = Expression.Call(this.Executor, methExecute, new Type[] { projector.Body.Type },
                Expression.Constant(command),
                projector,
                Expression.Constant(entity, typeof(MappingEntity)),
                Expression.NewArrayInit(typeof(object), values)
                );

            if (projection.Aggregator != null)
            {
                if (projector.ReturnType != projection.Aggregator.Parameters[0].Type.GetGenericArguments()[0])
                {
                    // we probably swaped out an Identifiable type for IIdentifiable, so we need to rebuild the projection 
                    // using the interface, otherwise we'll get cast failures downstream
                    projection = new ProjectionExpression(
                        projection.Select, 
                        projector, 
                        Aggregator.GetAggregator(projector.ReturnType, 
                        projection.Aggregator.Parameters[0].Type.GetGenericTypeDefinition().MakeGenericType(projector.ReturnType)));
                }
                // apply aggregator
                result = DbExpressionReplacer.Replace(projection.Aggregator.Body, projection.Aggregator.Parameters[0], result);
            }
            return result;
        }

        protected virtual LambdaExpression RedirectProjector(Expression projector, ParameterExpression reader, bool isIdempotent)
        {
            var newProjector = ReplaceReaderParameter(this.Visit(projector), reader);
            var newEntity = newProjector is MemberInitExpression ? ((MemberInitExpression)newProjector).NewExpression : (NewExpression)newProjector;
            var repoParam = newEntity.Arguments.SingleOrDefault(e => e.Type.Equals(typeof(IRepository))) as ParameterExpression;

            if (!isIdempotent || newEntity.Type.Name.Contains("<"))
            {
                // this is either an update/insert or a custom projection
                // in either case, we won't be enumerating several different record types in the result
                // so just build either a native type, or a native type proxy
                if (repoParam == null)
                {
                    // this not a defer-loaded wrapped entity, so just new it with the reader param only
                    projector = Expression.Lambda(newProjector, reader);
                }
                else
                {
                    // this projector will need both the reader and the repository 
                    // downstream invokers will need to understand that they might receive two possible 
                    // types of lambdas
                    projector = Expression.Lambda(newProjector, reader, repoParam);
                }
            }
            else
            {
                // this call can result in mixed data types in the response, so we need to delegate type
                // construction to a type that knows how to build and select multiple projectors, based on the
                // the class of data returned
                var projectionType = newProjector.Type.Implements(typeof(IProxyIdentifiable)) 
                    ? newProjector.Type.BaseType 
                    : newProjector.Type;

                if (projectionType == typeof(_Thing))
                {
                    projectionType = typeof(IThing);
                }
                else if (projectionType == typeof(_Association))
                {
                    projectionType = typeof(IAssociation);
                }

                var typeBuilder = Expression.Constant(this.TypeBuilder);
                MethodInfo buildTypeMethod;
                if (repoParam == null)
                {
                    // this not a defer-loaded wrapped entity, so just new it with the reader param only
                    buildTypeMethod = typeof(OTypeBuilder).GetMethod("BuildNonDeferredType", new Type[] { typeof(FieldReader), typeof(IRepository) });
                }
                else
                {
                    // this projector will need both the reader and the repository 
                    // downstream invokers will need to understand that they might receive two possible 
                    // types of lambdas
                    buildTypeMethod = typeof(OTypeBuilder).GetMethod("BuildDeferredType", new Type[] { typeof(FieldReader), typeof(IRepository) });
                }
                buildTypeMethod = buildTypeMethod.MakeGenericMethod(projectionType);
                var buildTypeCall = Expression.Call(typeBuilder, buildTypeMethod, reader, repoParam);
                projector = Expression.Lambda(buildTypeCall, reader, repoParam);
            }
            return projector as LambdaExpression;
        }

        private Expression ReplaceReaderParameter(Expression projector, ParameterExpression reader)
        {
            if (projector is NewExpression)
            {
                projector = ReplaceReaderParameterInCtor((NewExpression)projector, reader);
            }
            else if (projector is MemberInitExpression)
            {
                List<MemberBinding> newBindings = new List<MemberBinding>();
                foreach(var binding in ((MemberInitExpression)projector).Bindings)
                {
                    if (((MemberAssignment)binding).Expression is MethodCallExpression
                        && ((MethodCallExpression)((MemberAssignment)binding).Expression).Object is ParameterExpression
                        && ((ParameterExpression)((MethodCallExpression)((MemberAssignment)binding).Expression).Object).Type.Equals(typeof(FieldReader)))
                    {
                        newBindings.Add(Expression.Bind(
                            binding.Member, 
                            Expression.Call(
                                reader, 
                                ((MethodCallExpression)((MemberAssignment)binding).Expression).Method,
                                ((MethodCallExpression)((MemberAssignment)binding).Expression).Arguments)));
                    }
                    else newBindings.Add(binding);
                }
                projector = Expression.MemberInit(ReplaceReaderParameterInCtor(((MemberInitExpression)projector).NewExpression, reader), newBindings.ToArray());
            }
            return projector;
        }

        private NewExpression ReplaceReaderParameterInCtor(NewExpression newEx, ParameterExpression reader)
        {
            List<Expression> newArgs = new List<Expression>();
            foreach(var arg in newEx.Arguments)
            {
                if (arg is MethodCallExpression
                        && ((MethodCallExpression)arg).Object is ParameterExpression
                        && ((ParameterExpression)((MethodCallExpression)arg).Object).Type.Equals(typeof(FieldReader)))
                {
                    newArgs.Add(Expression.Call(reader, ((MethodCallExpression)arg).Method, ((MethodCallExpression)arg).Arguments));
                }
                else newArgs.Add(arg);
            }
            return newEx;
        }
    }
}
