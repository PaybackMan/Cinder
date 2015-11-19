using SHS.Core.Domain.MedicalRecords;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SHS.Sage.Services.Services
{
    public interface ITransform
    {
        T Transform<T>(HttpRequest request);


    }
}
