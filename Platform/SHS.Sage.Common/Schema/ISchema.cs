using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Schema
{
    public interface ISchema
    {
        IEnumerable<IClass> Classes { get; }
        IClass GetClass(string id);
        IClass GetClass(IIdentifiable identifiable);
        IClass GetClass<T>(T identifiable) where T : IIdentifiable;
    }

    public interface ISchema<TRepository> : ISchema
        where TRepository : IRepository
    {
        new IEnumerable<IClass<TRepository>> Classes { get; }
        void AddClass(IClass<TRepository> classSchema);
        void RemoveClass(IClass<TRepository> classSchema);
        void AlterClass(IClass<TRepository> classSchema);

        new IClass<TRepository> GetClass(string id);
        new IClass<TRepository> GetClass(IIdentifiable identifiable);
        new IClass<TRepository> GetClass<T>(T identifiable) where T : IIdentifiable;
    }
}
