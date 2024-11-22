using System.Diagnostics;
using System.Text.RegularExpressions;

namespace StockfishServices;

public record MoveResult(string Move, int CentPawns);

public sealed class StockfishService
{
    private static readonly Regex MovePerformanceMatch = new Regex(@"multipv \d+ score cp (-?\d+).* pv (\w+)", RegexOptions.Compiled);

    public StockfishService()
    {
    }

    private static MoveResult? ParseLineForMove(string line, int depth)
    {
        if (!line.StartsWith($"info depth {depth}") || !line.Contains("multipv"))
            return null;

        var match = MovePerformanceMatch.Match(line);
        if (match.Success)
        {
            return new MoveResult(match.Groups[2].Value, int.Parse(match.Groups[1].Value));
        }
        return null;
    }

    public async Task<List<MoveResult>> AnalyzePositionAsync(List<string> moves, int depth)
    {
        var results = new List<MoveResult>();

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "Assets\\stockfish",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process _stockfish = new Process { StartInfo = processStartInfo };
        _stockfish.Start();

        await _stockfish.StandardInput.WriteLineAsync("uci");
        await _stockfish.StandardInput.WriteLineAsync("setoption name MultiPV value 5");
        await _stockfish.StandardInput.WriteLineAsync("setoption name Threads value 8");
        await _stockfish.StandardInput.WriteLineAsync($"position startpos moves {string.Join(' ', moves)}");
        await _stockfish.StandardInput.WriteLineAsync($"go depth {depth} multipv 5");

        using CancellationTokenSource cts = new CancellationTokenSource(5000);
        string? line;
        try
        {
            while ((line = await _stockfish.StandardOutput.ReadLineAsync(cts.Token)) != null)
            {
                if (line.Contains("bestmove"))
                {
                    break;
                }

                MoveResult? move = ParseLineForMove(line, depth);

                if (move is not null)
                    results.Add(move);
            }
        }
        catch (OperationCanceledException) { }

        await _stockfish.StandardInput.WriteLineAsync("quit");

        return results.OrderByDescending(r => r.CentPawns).ToList();
    }

    public async Task<MoveClassification> ClassifyMove(List<MoveResult> bestMoves, string playedMove, List<string> moves, int depth)
    {
        if (bestMoves.Count == 0)
            return MoveClassification.Erro;

        var bestEval = bestMoves[0];
        var played = bestMoves.Find(move => move.Move == playedMove);

        if (played is null)
        {
            var newBests = await AnalyzePositionAsync(new List<string>(moves) { playedMove }, depth - 1);
            if (newBests.Count == 0)
                return MoveClassification.Erro;

            played = new MoveResult(playedMove, -newBests[0].CentPawns);
        }

        if (played == bestEval)
        {
            var otherEvals = bestMoves.Skip(1).Take(4).Select(move => move.CentPawns).ToList();
            var otherLosses = otherEvals.Select(eval => Math.Abs(bestEval.CentPawns) - Math.Abs(eval));
            if (otherLosses.All(loss => loss >= 100))
            {
                return MoveClassification.Unico;
            }
            return MoveClassification.Excelente;
        }

        var loss = Math.Abs(Math.Abs(bestEval.CentPawns) - Math.Abs(played.CentPawns));
        if (loss <= 50)
        {
            return MoveClassification.Otimo;
        }
        else if (loss <= 100)
        {
            return MoveClassification.Bom;
        }
        else if (loss <= 200)
        {
            return MoveClassification.Ruim;
        }
        else
        {
            return MoveClassification.Capivarada;
        }
    }
}

