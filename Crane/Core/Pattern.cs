using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crane.Core
{
    public abstract class Pattern
    {
        public CMesh CMesh { get; protected set; }
        public List<Constraint> Constraints { get; protected set; }

        protected abstract void SetCMesh();
        protected abstract void SetConstraints();
    }
}
