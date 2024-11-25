using System.Diagnostics;
using System.Text.RegularExpressions;

namespace StockfishServices;

public sealed class StockfishService
{
    #region Public Methods

    public StockfishService()
    {
    }

    public async Task<List<MoveEval>> AnalyzePosition(List<string> moves, int depth)
    {
        using Process _stockfish = await StartStockfish();

        try
        {
            var results = await AnalyzePosition(moves, depth, _stockfish);

            return results;
        }
        finally
        {
            await StopStockfish(_stockfish);
        }
    }

    public async Task<MoveResult> ClassifyMove(List<MoveEval> bestMoves, string playedMove, List<string> moves, int depth)
    {
        var played = GetPlayedMoveEvalFromBest(bestMoves, playedMove);

        if (played is null)
        {
            var newBests = await AnalyzePosition(new List<string>(moves) { playedMove }, depth - 1);
            if (newBests.Count == 0)
                return new(new MoveEval(playedMove, 0), 0, MoveClassification.Erro);

            played = new MoveEval(playedMove, -newBests[0].CentPawns);
        }

        return ClassifyMovePlayed(bestMoves, played);
    }

    public async Task<GameResult> AnalyzeGame(List<string> moves, int depth)
    {
        GameResult ret = new(depth);
        using Process _stockfish = await StartStockfish();

        try
        {
            bool isWhite = true;
            List<string> processedMoves = new();
            var evals = await AnalyzePosition(processedMoves, depth, _stockfish);
            foreach (var move in moves)
            {
                MoveEval? played = GetPlayedMoveEvalFromBest(evals, move);
                processedMoves.Add(move);
                var newEvals = await AnalyzePosition(processedMoves, depth, _stockfish);

                if (played is null)
                {
                    if(newEvals.Count == 0)
                    {
                        throw new InvalidOperationException("Stockfish returned no moves for the current position and last move was not in the best possible moves (checkmate).");
                    }
                    played = new MoveEval(move, -newEvals[0].CentPawns);
                }

                var result = ClassifyMovePlayed(evals, played);

                if (isWhite)
                {
                    ret.White.Moves.Add(move);
                    ret.White.Results.Add(result);
                }
                else
                {
                    ret.Black.Moves.Add(move);
                    ret.Black.Results.Add(result);
                }

                isWhite = !isWhite;
                evals = newEvals;
            }
        }
        finally
        {
            await StopStockfish(_stockfish);
        }

        return ret;
    }

    #endregion

    #region Private Stuff

    private static readonly Regex MovePerformanceMatch = new Regex(@"multipv \d+ score cp (-?\d+).* pv (\w+)", RegexOptions.Compiled);

    private static MoveEval? ParseLineForMove(string line, int depth)
    {
        if (!line.StartsWith($"info depth {depth}") || !line.Contains("multipv"))
            return null;

        var match = MovePerformanceMatch.Match(line);
        if (match.Success)
        {
            return new MoveEval(match.Groups[2].Value, int.Parse(match.Groups[1].Value));
        }
        return null;
    }

    private static async Task<List<MoveEval>> AnalyzePosition(List<string> moves, int depth, Process _stockfish)
    {
        List<MoveEval> results = new();
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

                MoveEval? move = ParseLineForMove(line, depth);

                if (move is not null)
                    results.Add(move);
            }
        }
        catch (OperationCanceledException) { }

        return results.OrderByDescending(r => r.CentPawns).ToList();
    }

    private static async Task<Process> StartStockfish()
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "Assets\\stockfish",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        Process _stockfish = new Process { StartInfo = processStartInfo };
        _stockfish.Start();

        await _stockfish.StandardInput.WriteLineAsync("uci");
        await _stockfish.StandardInput.WriteLineAsync("setoption name MultiPV value 5");
        await _stockfish.StandardInput.WriteLineAsync("setoption name Threads value 8");
        return _stockfish;
    }

    private static async Task StopStockfish(Process _stockfish)
    {
        await _stockfish.StandardInput.WriteLineAsync("quit");
    }

    private static MoveEval? GetPlayedMoveEvalFromBest(List<MoveEval> bestMoves, string playedMove) =>
        bestMoves.Find(move => move.Move == playedMove);

    private static MoveResult ClassifyMovePlayed(List<MoveEval> bestMoves, MoveEval played)
    {
        var bestEval = bestMoves[0];

        if (played.Move == bestEval.Move)
        {
            var otherEvals = bestMoves.Skip(1).Take(4).Select(move => move.CentPawns).ToList();
            var otherLosses = otherEvals.Select(eval => Math.Abs(bestEval.CentPawns) - Math.Abs(eval));
            if (otherLosses.All(loss => loss >= 100))
            {
                return new(played, 0, MoveClassification.Unico);
            }
            return new(played, 0, MoveClassification.Excelente);
        }

        var loss = Math.Abs(bestEval.CentPawns) - Math.Abs(played.CentPawns);
        if (loss <= 50)
        {
            return new(played, loss, MoveClassification.Otimo);
        }
        else if (loss <= 100)
        {
            return new(played, loss, MoveClassification.Bom);
        }
        else if (loss <= 200)
        {
            return new(played, loss, MoveClassification.Ruim);
        }
        else
        {
            return new(played, loss, MoveClassification.Capivarada);
        }
    }

    #endregion
}

