using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockfishServices;

public record PlayerResult(List<string> Moves, int Depth, List<MoveResult> Results)
{
    public PlayerResult(List<string> moves, int depth) : this(moves, depth, new List<MoveResult>())
    {
    }
    public PlayerResult() : this(new List<string>(), 0, new List<MoveResult>())
    {
    }

    public double GetAvgLossCentPawns() => Results.Average(x => x.LostCentPawns);
}
