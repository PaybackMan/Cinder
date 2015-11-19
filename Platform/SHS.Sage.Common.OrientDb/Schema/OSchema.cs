using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Schema
{
    public class OSchema<TRepository> : ISchema<TRepository> where TRepository : ORepository
    {
        private List<IClass<TRepository>> _classes = new List<IClass<TRepository>>();

        public IEnumerable<IClass<TRepository>> Classes { get { return _classes.AsReadOnly(); } }

        IEnumerable<IClass> ISchema.Classes
        {
            get
            {
                return Classes;
            }
        }

        public void AddClass(IClass<TRepository> classSchema)
        {
            _classes.Add(classSchema);
        }

        public void AlterClass(IClass<TRepository> classSchema)
        {
            var oClass = _classes.SingleOrDefault(c => c.Name == classSchema.Name);
            if (oClass != null)
            {
                RemoveClass(oClass);
            }
            AddClass(classSchema);
        }

        public IClass<TRepository> GetClass(IIdentifiable identifiable)
        {
            return GetClass(identifiable.Id);
        }

        public IClass<TRepository> GetClass(string id)
        {
            return _classes.SingleOrDefault(c => c.IsMaskedId(id));
        }

        public IClass<TRepository> GetClass<T>(T identifiable) where T : IIdentifiable
        {
            return GetClass(identifiable.Id);
        }

        public void RemoveClass(IClass<TRepository> classSchema)
        {
            _classes.Remove(classSchema);
        }

        IClass ISchema.GetClass(IIdentifiable identifiable)
        {
            return GetClass(identifiable);
        }

        IClass ISchema.GetClass(string id)
        {
            return GetClass(id);
        }

        IClass ISchema.GetClass<T>(T identifiable)
        {
            return GetClass(identifiable);
        }
    }
}
