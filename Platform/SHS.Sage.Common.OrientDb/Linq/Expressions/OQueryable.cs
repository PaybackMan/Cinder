using SHS.Sage.Linq.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq
{
    public static class OQueryable
    {
        /// <summary>
        /// Gets all associations of all types where the specified thing is a Source of the Association
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<IAssociation> SourceOf(this IThing source)
        {
            return SourceOf<IAssociation>(source);
        }

        /// <summary>
        /// Gets all associations of all types where the specified thing is a Source of the Association
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<IAssociation> SourceOf(this IThing source, Expression<Func<IAssociation, bool>> predicate)
        {
            return SourceOf<IAssociation>(source, predicate);
        }

        /// <summary>
        /// Gets all associations of TAssociation where the specified thing is a Source of the Association
        /// </summary>
        /// <typeparam name="TAssociation"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IQueryable<TAssociation> SourceOf<TAssociation>(this IThing source)
            where TAssociation : IAssociation
        {
            return SourceOf<TAssociation>(source, (a) => true);
        }

        /// <summary>
        /// Gets all associations of TAssociation where the specified Thing is a Source of the Association, which matches 
        /// the supplied predicate
        /// </summary>
        /// <typeparam name="TAssociation"></typeparam>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IQueryable<TAssociation> SourceOf<TAssociation>(this IThing source, Expression<Func<TAssociation, bool>> predicate)
            where TAssociation : IAssociation
        {
            if (!(source is IProxyIdentifiable))
                throw new InvalidOperationException("The source must first be attached to a repository.");

            var proxy = (IProxyIdentifiable)source;
            if (!proxy.IsValid)
                throw new InvalidOperationException("The repository is not in a valid state for executing this method");

            if (string.IsNullOrEmpty(source.Id))
            {
                // the item isn't identified, so just return an empty set
                return new TAssociation[0].AsQueryable();
            }

            var repo = proxy.Repository;

            var sourceMember = Expression.MakeMemberAccess(
                predicate.Parameters[0],
                typeof(TAssociation).GetTypeInfo().DeclaredProperties.Single(p => p.Name.Equals("Source")));
            var sourceEquals = Expression.Equal(Expression.Constant(source), sourceMember);
            var sourcePredicate = Expression.Lambda<Func<TAssociation, bool>>(sourceEquals, predicate.Parameters);

            var completePredicate = Expression.Lambda<Func<TAssociation, bool>>(
                Expression.And(sourcePredicate.Body, predicate.Body),
                predicate.Parameters);

            return repo.AssociationSet<TAssociation>().Where(completePredicate);
        }
    }
}
