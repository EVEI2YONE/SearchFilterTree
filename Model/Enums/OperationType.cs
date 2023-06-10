using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Enums
{
    public enum OperationType
    {
        LessThan,
        LessThanOrEqual,
        NotLessThan,
        NotLessThanOrEqual,
        Equals,
        NotEquals,
        GreaterThan,
        greaterThanOrEqual,
        NotGreaterThan,
        NotGreaterThanOrEqual,

        Contains,
        NotContains,
        Within,
        NotWithin,
        IsNull,
        IsNotNull
    }
}
