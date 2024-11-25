using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockfishServices;

public record GameResult(PlayerResult White, PlayerResult Black, int Depth)
{
    public List<string> Moves => White.Moves.Concat(Black.Moves).ToList();

    public GameResult(int depth) : this(new PlayerResult(), new PlayerResult(), depth)
    {
    }

    public double GetAvgLossCentPawns() => (White.GetAvgLossCentPawns() + Black.GetAvgLossCentPawns()) / 2;
}