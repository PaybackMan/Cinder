using SHS.Sage.Linq.Expressions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHS.Sage.Linq.Translation
{
    /// <summary>
    /// Result from calling FieldProjector.ProjectFields
    /// </summary>
    public sealed class ProjectedFields
    {
        Expression projector;
        ReadOnlyCollection<FieldDeclaration> fields;

        public ProjectedFields(Expression projector, ReadOnlyCollection<FieldDeclaration> fields)
        {
            this.projector = projector;
            this.fields = fields;
        }

        public Expression Projector
        {
            get { return this.projector; }
        }

        public ReadOnlyCollection<FieldDeclaration> Fields
        {
            get { return this.fields; }
        }
    }
}
