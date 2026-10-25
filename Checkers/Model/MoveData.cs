using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Model;

public struct MoveData
{
    public required Position OldPosition { get; init; }
    public required Position NewPosition { get; init; }

    public Position? Target { get; set; }
}
