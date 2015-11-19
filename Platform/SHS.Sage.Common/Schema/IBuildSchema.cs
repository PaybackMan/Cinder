using SHS.Sage.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Schema
{
    public interface IBuildSchema<TRepository> : IBuildSchema 
        where TRepository : IRepository
    {
        void Build(TRepository repository, Type identifiableType);
        void Build<T>(TRepository repository) where T : IIdentifiable;
        ISchema<TRepository> GetSchema(TRepository repository);
        void BuildSchema(TRepository oRepository);
    }

    public interface IBuildSchema
    {
        void Build(IRepository repository, Type identifiableType);
        void Build<T>(IRepository repository) where T : IIdentifiable;
        ISchema<IRepository> GetSchema(IRepository repository);
        void BuildSchema(IRepository oRepository);
        bool IsInitialized { get; }

        ISchema Schema { get; }
    }
}
