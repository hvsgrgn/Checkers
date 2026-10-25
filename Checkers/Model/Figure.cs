using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Model;

public struct Position {
    public required int Row { get; init; }
    public required int Col { get; init; }
}
public class Figure
{

    public Position Position { get; set; }

    public required bool IsKing { get; set; }

    public required Colour Owner { get; init; }

}
