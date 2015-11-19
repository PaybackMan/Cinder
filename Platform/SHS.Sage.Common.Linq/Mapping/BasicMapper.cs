using SHS.Sage.Linq.Expressions;
using SHS.Sage.Linq.Expressions.Command;
using SHS.Sage.Linq.Expressions.Queryable;
using SHS.Sage.Linq.Language;
using SHS.Sage.Linq.Translation;
using SHS.Sage.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Mapping
{
    public class BasicMapper : QueryMapper
    {
        BasicMapping mapping;
        QueryTranslator translator;

        public BasicMapper(BasicMapping mapping, QueryTranslator translator)
        {
            this.mapping = mapping;
            this.translator = translator;
        }

        public override QueryMapping Mapping
        {
            get { return this.mapping; }
        }

        public override QueryTranslator Translator
        {
            get { return this.translator; }
        }

        /// <summary>
        /// The query language specific type for the Field
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        public virtual StorageType GetFieldType(IMappedEntity entity, MemberInfo member)
        {
            string dbType = this.mapping.GetFieldDbType(entity, member);
            if (dbType != null)
            {
                return this.translator.Linguist.Language.TypeSystem.Parse(dbType);
            }
            return this.translator.Linguist.Language.TypeSystem.GetStorageType(TypeHelper.GetMemberType(member));
        }

        public override ProjectionExpression GetQueryExpression(IMappedEntity entity)
        {
            var IdentifiableAlias = new IdentifiableAlias();
            var selectAlias = new IdentifiableAlias();
            var table = new IdentifiableExpression(IdentifiableAlias, entity, this.mapping.GetTableName(entity));

            Expression projector = this.GetEntityExpression(table, entity);
            var pc = FieldProjector.ProjectFields(this.translator.Linguist.Language, projector, null, selectAlias, IdentifiableAlias);

            var proj = new ProjectionExpression(
                new SelectExpression(selectAlias, pc.Fields, table, null),
                pc.Projector
                );

            return (ProjectionExpression)this.Translator.Police.ApplyPolicy(proj, entity.EntityType);
        }

        public override EntityExpression GetEntityExpression(Expression root, IMappedEntity entity)
        {
            // must be some complex type constructed from multiple Fields
            var assignments = new List<EntityAssignment>();
            foreach (MemberInfo mi in this.mapping.GetMappedMembers(entity))
            {
                if (!this.mapping.IsAssociationRelationship(entity, mi))
                {
                    Expression me = this.GetMemberExpression(root, entity, mi);
                    if (me != null)
                    {
                        assignments.Add(new EntityAssignment(mi, me));
                    }
                }
            }

            return new EntityExpression(entity, BuildEntityExpression(entity, assignments));
        }

        public class EntityAssignment
        {
            public MemberInfo Member { get; private set; }
            public Expression Expression { get; private set; }
            public EntityAssignment(MemberInfo member, Expression expression)
            {
                this.Member = member;
                System.Diagnostics.Debug.Assert(expression != null);
                this.Expression = expression;
            }
        }

        protected virtual Expression BuildEntityExpression(IMappedEntity entity, IList<EntityAssignment> assignments)
        {
            NewExpression newExpression;

            // handle cases where members are not directly assignable
            EntityAssignment[] readonlyMembers = assignments.Where(b => TypeHelper.IsReadOnly(b.Member)).ToArray();
            ConstructorInfo[] cons = entity.EntityType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            bool hasNoArgConstructor = cons.Any(c => c.GetParameters().Length == 0);

            if (readonlyMembers.Length > 0 || !hasNoArgConstructor)
            {
                // find all the constructors that bind all the read-only members
                var consThatApply = cons.Select(c => this.BindConstructor(c, readonlyMembers))
                                        .Where(cbr => cbr != null && cbr.Remaining.Count == 0).ToList();
                if (consThatApply.Count == 0)
                {
                    throw new InvalidOperationException(string.Format("Cannot construct type '{0}' with all mapped includedMembers.", entity.EntityType));
                }
                // just use the first one... (Note: need better algorithm. :-)
                if (readonlyMembers.Length == assignments.Count)
                {
                    return consThatApply[0].Expression;
                }
                var r = this.BindConstructor(consThatApply[0].Expression.Constructor, assignments);

                newExpression = r.Expression;
                assignments = r.Remaining;
            }
            else
            {
                newExpression = Expression.New(entity.EntityType);
            }

            Expression result;
            if (assignments.Count > 0)
            {
                if (entity.EntityType.IsInterface)
                {
                    assignments = this.MapAssignments(assignments, entity.EntityType).ToList();
                }
                 result = Expression.MemberInit(newExpression, (MemberBinding[])assignments.Select(a => Expression.Bind(a.Member, a.Expression)).ToArray());
            }
            else
            {
                result = newExpression;
            }

            return result;
        }

        private IEnumerable<EntityAssignment> MapAssignments(IEnumerable<EntityAssignment> assignments, Type entityType)
        {
            foreach (var assign in assignments)
            {
                MemberInfo[] members = entityType.GetMember(assign.Member.Name, BindingFlags.Instance | BindingFlags.Public);
                if (members != null && members.Length > 0)
                {
                    yield return new EntityAssignment(members[0], assign.Expression);
                }
                else
                {
                    yield return assign;
                }
            }
        }

        protected virtual ConstructorBindResult BindConstructor(ConstructorInfo cons, IList<EntityAssignment> assignments)
        {
            var ps = cons.GetParameters();
            var args = new Expression[ps.Length];
            var mis = new MemberInfo[ps.Length];
            HashSet<EntityAssignment> members = new HashSet<EntityAssignment>(assignments);
            HashSet<EntityAssignment> used = new HashSet<EntityAssignment>();

            for (int i = 0, n = ps.Length; i < n; i++)
            {
                ParameterInfo p = ps[i];
                var assignment = members.FirstOrDefault(a =>
                    p.Name == a.Member.Name
                    && p.ParameterType.IsAssignableFrom(a.Expression.Type));
                if (assignment == null)
                {
                    assignment = members.FirstOrDefault(a =>
                        string.Compare(p.Name, a.Member.Name, true) == 0
                        && p.ParameterType.IsAssignableFrom(a.Expression.Type));
                }
                if (assignment != null)
                {
                    args[i] = assignment.Expression;
                    if (mis != null)
                        mis[i] = assignment.Member;
                    used.Add(assignment);
                }
                else
                {
                    MemberInfo[] mems = cons.DeclaringType.GetMember(p.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                    if (mems != null && mems.Length > 0)
                    {
                        args[i] = Expression.Constant(TypeHelper.GetDefault(p.ParameterType), p.ParameterType);
                        mis[i] = mems[0];
                    }
                    else
                    {
                        // unknown parameter, does not match any member
                        return null;
                    }
                }
            }

            members.ExceptWith(used);

            return new ConstructorBindResult(Expression.New(cons, args, mis), members);
        }

        protected class ConstructorBindResult
        {
            public NewExpression Expression { get; private set; }
            public ReadOnlyCollection<EntityAssignment> Remaining { get; private set; }
            public ConstructorBindResult(NewExpression expression, IEnumerable<EntityAssignment> remaining)
            {
                this.Expression = expression;
                this.Remaining = remaining.ToReadOnly();
            }
        }

        public override bool HasIncludedMembers(EntityExpression entity)
        {
            var policy = this.translator.Police.Policy;
            foreach (var mi in this.mapping.GetMappedMembers(entity.Entity))
            {
                if (policy.IsIncluded(mi))
                    return true;
            }
            return false;
        }

        public override EntityExpression IncludeMembers(EntityExpression entity, Func<MemberInfo, bool> fnIsIncluded)
        {
            var assignments = this.GetAssignments(entity.Expression).ToDictionary(ma => ma.Member.Name);
            bool anyAdded = false;
            foreach (var mi in this.mapping.GetMappedMembers(entity.Entity))
            {
                EntityAssignment ea;
                bool okayToInclude = !assignments.TryGetValue(mi.Name, out ea) || IsNullRelationshipAssignment(entity.Entity, ea);
                if (okayToInclude && fnIsIncluded(mi))
                {
                    ea = new EntityAssignment(mi, this.GetMemberExpression(entity.Expression, entity.Entity, mi));
                    assignments[mi.Name] = ea;
                    anyAdded = true;
                }
            }
            if (anyAdded)
            {
                return new EntityExpression(entity.Entity, this.BuildEntityExpression(entity.Entity, assignments.Values.ToList()));
            }
            return entity;
        }

        private bool IsNullRelationshipAssignment(IMappedEntity entity, EntityAssignment assignment)
        {
            if (this.mapping.IsRelationship(entity, assignment.Member))
            {
                var cex = assignment.Expression as ConstantExpression;
                if (cex != null && cex.Value == null)
                    return true;
            }
            return false;
        }


        private IEnumerable<EntityAssignment> GetAssignments(Expression newOrMemberInit)
        {
            var assignments = new List<EntityAssignment>();
            var minit = newOrMemberInit as MemberInitExpression;
            if (minit != null)
            {
                assignments.AddRange(minit.Bindings.OfType<MemberAssignment>().Select(a => new EntityAssignment(a.Member, a.Expression)));
                newOrMemberInit = minit.NewExpression;
            }
            var nex = newOrMemberInit as NewExpression;
            if (nex != null && nex.Members != null)
            {
                assignments.AddRange(
                    Enumerable.Range(0, nex.Arguments.Count)
                              .Where(i => nex.Members[i] != null)
                              .Select(i => new EntityAssignment(nex.Members[i], nex.Arguments[i]))
                              );
            }
            return assignments;
        }


        public override Expression GetMemberExpression(Expression root, IMappedEntity entity, MemberInfo member)
        {
            if (this.mapping.IsAssociationRelationship(entity, member))
            {
                var relatedEntity = this.mapping.GetRelatedEntity(entity, member);
                ProjectionExpression projection = this.GetQueryExpression(relatedEntity);

                // make where clause for joining back to 'root'
                var declaredTypeMembers = this.mapping.GetAssociationKeyMembers(entity, member).ToList();
                var associatedMembers = this.mapping.GetAssociationRelatedKeyMembers(entity, member).ToList();

                Expression where = null;
                for (int i = 0, n = associatedMembers.Count; i < n; i++)
                {
                    Expression equal =
                        this.GetMemberExpression(projection.Projector, relatedEntity, associatedMembers[i]).Equal(
                            this.GetMemberExpression(root, entity, declaredTypeMembers[i])
                        );
                    where = (where != null) ? where.And(equal) : equal;
                }

                IdentifiableAlias newAlias = new IdentifiableAlias();
                var pc = FieldProjector.ProjectFields(this.translator.Linguist.Language, projection.Projector, null, newAlias, projection.Select.Alias);

                LambdaExpression aggregator = Aggregator.GetAggregator(TypeHelper.GetMemberType(member), typeof(IEnumerable<>).MakeGenericType(pc.Projector.Type));
                var result = new ProjectionExpression(
                    new SelectExpression(newAlias, pc.Fields, projection.Select, where),
                    pc.Projector, aggregator
                    );

                return this.translator.Police.ApplyPolicy(result, member);
            }
            else
            {
                AliasedExpression aliasedRoot = root as AliasedExpression;
                if (aliasedRoot != null && this.mapping.IsField(entity, member))
                {
                    return new FieldExpression(TypeHelper.GetMemberType(member), 
                        this.GetFieldType(entity, member), 
                        aliasedRoot.Alias, 
                        this.mapping.GetFieldName(entity, member));
                }
                return QueryBinder.BindMember(root, member);
            }
        }

        public override Expression GetInsertExpression(IMappedEntity entity, Expression instance, LambdaExpression selector)
        {
            var IdentifiableAlias = new IdentifiableAlias();
            var table = new IdentifiableExpression(IdentifiableAlias, entity, this.mapping.GetTableName(entity));
            var assignments = this.GetFieldAssignments(table, instance, entity, (e, m) => !this.mapping.IsGenerated(e, m));

            if (selector != null)
            {
                return new BlockCommandExpression(
                    new InsertCommandExpression(table, assignments),
                    this.GetInsertResult(entity, instance, selector, null)
                    );
            }

            return new InsertCommandExpression(table, assignments);
        }

        protected virtual IEnumerable<FieldAssignment> GetFieldAssignments(Expression table, Expression instance, IMappedEntity entity, Func<IMappedEntity, MemberInfo, bool> fnIncludeField)
        {
            foreach (var m in this.mapping.GetMappedMembers(entity))
            {
                if (this.mapping.IsField(entity, m) && fnIncludeField(entity, m))
                {
                    yield return new FieldAssignment(
                        (FieldExpression)this.GetMemberExpression(table, entity, m),
                        Expression.MakeMemberAccess(instance, m)
                        );
                }
            }
        }

        protected virtual Expression GetInsertResult(IMappedEntity entity, Expression instance, LambdaExpression selector, Dictionary<MemberInfo, Expression> map)
        {
            var IdentifiableAlias = new IdentifiableAlias();
            var tex = new IdentifiableExpression(IdentifiableAlias, entity, this.mapping.GetTableName(entity));
            var aggregator = Aggregator.GetAggregator(selector.Body.Type, typeof(IEnumerable<>).MakeGenericType(selector.Body.Type));

            Expression where;
            DeclarationCommand genIdCommand = null;
            var generatedIds = this.mapping.GetMappedMembers(entity).Where(m => this.mapping.IsPrimaryKey(entity, m) && this.mapping.IsGenerated(entity, m)).ToList();
            if (generatedIds.Count > 0)
            {
                if (map == null || !generatedIds.Any(m => map.ContainsKey(m)))
                {
                    var localMap = new Dictionary<MemberInfo, Expression>();
                    genIdCommand = this.GetGeneratedIdCommand(entity, generatedIds.ToList(), localMap);
                    map = localMap;
                }

                // is this just a retrieval of one generated id member?
                var mex = selector.Body as MemberExpression;
                if (mex != null && this.mapping.IsPrimaryKey(entity, mex.Member) && this.mapping.IsGenerated(entity, mex.Member))
                {
                    if (genIdCommand != null)
                    {
                        // just use the select from the genIdCommand
                        return new ProjectionExpression(
                            genIdCommand.Source,
                            new FieldExpression(mex.Type, genIdCommand.Variables[0].QueryType, genIdCommand.Source.Alias, genIdCommand.Source.Fields[0].Name),
                            aggregator
                            );
                    }
                    else
                    {
                        IdentifiableAlias alias = new IdentifiableAlias();
                        var colType = this.GetFieldType(entity, mex.Member);
                        return new ProjectionExpression(
                            new SelectExpression(alias, new[] { new FieldDeclaration("", map[mex.Member], colType) }, null, null),
                            new FieldExpression(TypeHelper.GetMemberType(mex.Member), colType, alias, ""),
                            aggregator
                            );
                    }
                }

                where = generatedIds.Select((m, i) =>
                    this.GetMemberExpression(tex, entity, m).Equal(map[m])
                    ).Aggregate((x, y) => x.And(y));
            }
            else
            {
                where = this.GetIdentityCheck(tex, entity, instance);
            }

            Expression typeProjector = this.GetEntityExpression(tex, entity);
            Expression selection = DbExpressionReplacer.Replace(selector.Body, selector.Parameters[0], typeProjector);
            IdentifiableAlias newAlias = new IdentifiableAlias();
            var pc = FieldProjector.ProjectFields(this.translator.Linguist.Language, selection, null, newAlias, IdentifiableAlias);
            var pe = new ProjectionExpression(
                new SelectExpression(newAlias, pc.Fields, tex, where),
                pc.Projector,
                aggregator
                );

            if (genIdCommand != null)
            {
                return new BlockCommandExpression(genIdCommand, pe);
            }
            return pe;
        }

        protected virtual DeclarationCommand GetGeneratedIdCommand(IMappedEntity entity, List<MemberInfo> members, Dictionary<MemberInfo, Expression> map)
        {
            var Fields = new List<FieldDeclaration>();
            var decls = new List<VariableDeclaration>();
            var alias = new IdentifiableAlias();
            foreach (var member in members)
            {
                Expression genId = this.translator.Linguist.Language.GetGeneratedIdExpression(member);
                var name = member.Name;
                var colType = this.GetFieldType(entity, member);
                Fields.Add(new FieldDeclaration(member.Name, genId, colType));
                decls.Add(new VariableDeclaration(member.Name, colType, new FieldExpression(genId.Type, colType, alias, member.Name)));
                if (map != null)
                {
                    var vex = new VariableExpression(member.Name, TypeHelper.GetMemberType(member), colType);
                    map.Add(member, vex);
                }
            }
            var select = new SelectExpression(alias, Fields, null, null);
            return new DeclarationCommand(decls, select);
        }

        protected virtual Expression GetIdentityCheck(Expression root, IMappedEntity entity, Expression instance)
        {
            return this.mapping.GetMappedMembers(entity)
            .Where(m => this.mapping.IsPrimaryKey(entity, m))
            .Select(m => this.GetMemberExpression(root, entity, m).Equal(Expression.MakeMemberAccess(instance, m)))
            .Aggregate((x, y) => x.And(y));
        }

        protected virtual Expression GetEntityExistsTest(IMappedEntity entity, Expression instance)
        {
            ProjectionExpression tq = this.GetQueryExpression(entity);
            Expression where = this.GetIdentityCheck(tq.Select, entity, instance);
            return new ExistsExpression(new SelectExpression(new IdentifiableAlias(), null, tq.Select, where));
        }

        protected virtual Expression GetEntityStateTest(IMappedEntity entity, Expression instance, LambdaExpression updateCheck)
        {
            ProjectionExpression tq = this.GetQueryExpression(entity);
            Expression where = this.GetIdentityCheck(tq.Select, entity, instance);
            Expression check = DbExpressionReplacer.Replace(updateCheck.Body, updateCheck.Parameters[0], tq.Projector);
            where = where.And(check);
            return new ExistsExpression(new SelectExpression(new IdentifiableAlias(), null, tq.Select, where));
        }

        public override Expression GetUpdateExpression(IMappedEntity entity, Expression instance, LambdaExpression updateCheck, LambdaExpression selector)
        {
            var IdentifiableAlias = new IdentifiableAlias();
            var table = new IdentifiableExpression(IdentifiableAlias, entity, this.mapping.GetTableName(entity));

            var where = this.GetIdentityCheck(table, entity, instance);
            if (updateCheck != null)
            {
                Expression typeProjector = this.GetEntityExpression(table, entity);
                Expression pred = DbExpressionReplacer.Replace(updateCheck.Body, updateCheck.Parameters[0], typeProjector);
                where = where.And(pred);
            }

            var assignments = this.GetFieldAssignments(table, instance, entity, (e, m) => this.mapping.IsUpdatable(e, m));

            Expression update = new UpdateCommandExpression(table, where, assignments);

            if (selector != null)
            {
                return new BlockCommandExpression(
                    update,
                    new IfCommandExpression(
                        this.translator.Linguist.Language.GetRowsAffectedExpression(update).GreaterThan(Expression.Constant(0)),
                        this.GetUpdateResult(entity, instance, selector),
                        null
                        )
                    );
            }
            else
            {
                return update;
            }
        }

        protected virtual Expression GetUpdateResult(IMappedEntity entity, Expression instance, LambdaExpression selector)
        {
            var tq = this.GetQueryExpression(entity);
            Expression where = this.GetIdentityCheck(tq.Select, entity, instance);
            Expression selection = DbExpressionReplacer.Replace(selector.Body, selector.Parameters[0], tq.Projector);
            IdentifiableAlias newAlias = new IdentifiableAlias();
            var pc = FieldProjector.ProjectFields(this.translator.Linguist.Language, selection, null, newAlias, tq.Select.Alias);
            return new ProjectionExpression(
                new SelectExpression(newAlias, pc.Fields, tq.Select, where),
                pc.Projector,
                Aggregator.GetAggregator(selector.Body.Type, typeof(IEnumerable<>).MakeGenericType(selector.Body.Type))
                );
        }

        public override Expression GetDeleteExpression(IMappedEntity entity, Expression instance, LambdaExpression deleteCheck)
        {
            IdentifiableExpression table = new IdentifiableExpression(new IdentifiableAlias(), entity, this.mapping.GetTableName(entity));
            Expression where = null;

            if (instance != null)
            {
                where = this.GetIdentityCheck(table, entity, instance);
            }

            if (deleteCheck != null)
            {
                Expression row = this.GetEntityExpression(table, entity);
                Expression pred = DbExpressionReplacer.Replace(deleteCheck.Body, deleteCheck.Parameters[0], row);
                where = (where != null) ? where.And(pred) : pred;
            }

            return new DeleteCommandExpression(table, where);
        }
    }
}
