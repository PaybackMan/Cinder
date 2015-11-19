using SHS.Sage.Common.Linq;
using SHS.Sage.Common.OrientDb.Tests.Things;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Common.OrientDb.Tests
{
    public class Mapping : IMapGraphTypes
    {
        static bool _registered = false;

        public Mapping(bool handleMapping = true)
        {
            IsRegistered = !handleMapping;
        }
        public bool IsRegistered
        {
            get { return _registered; }
            private set { _registered = value; }
        }

        public bool CanBuildType(Type identifiableType, out IBuildGraphTypes builder)
        {
            builder = null;
            if ( identifiableType.Equals(typeof(Patient)))
            {
                builder = Builder;
                return true;
            }
            return false;
        }

        public void Register(IBuildGraphTypes builder)
        {
            lock(this)
            {
                if (IsRegistered) return;

                Builder = builder;

                builder.Map<Patient>()
                    .Class(map => map.Name = "Patient")
                    .Property(map => 
                    {
                        map.TargetProperty = () => map.Target.Name;
                        map.Name = "NAME";
                    })
                    .Property(map => 
                    {
                        map.TargetProperty = () => map.Target.Age;
                        map.Name = "Age";
                    });
            }
        }

        private IBuildGraphTypes Builder { get; set; }
    }
}
