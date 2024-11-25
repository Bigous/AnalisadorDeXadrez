
using AnalisadorDeXadrezService.Model;
using StockfishServices;

namespace AnalisadorDeXadrezService.Controllers;

public class ChessAnalyzerController : IController
{
    private readonly ILogger<ChessAnalyzerController> _logger;

    public ChessAnalyzerController(ILogger<ChessAnalyzerController> logger)
    {
        _logger = logger;
    }

    public void Register(WebApplication app)
    {
        app.MapPost("/api/chess/analyze", Analyze);
    }

    private async Task<IResult> Analyze(AnalyzeRequest analyzeRequest, StockfishService stockFishService)
    {
        if (analyzeRequest == null || analyzeRequest.Moves == null || analyzeRequest.Depth == 0)
        {
            _logger.LogWarning("Movimentos e profundidade são obrigatórios.");
            return Results.BadRequest("Movimentos e profundidade são obrigatórios.");
        }

        _logger.LogInformation("Iniciando análise {Depth} para os movimentos: {Movimentos}", analyzeRequest.Depth, string.Join(", ", analyzeRequest.Moves));

        var classifications = new List<MoveClassificationResponse>();
        var currentFen = "startpos";

        try
        {
            List<string> moves = new();
            foreach (var move in analyzeRequest.Moves)
            {
                var bestMoves = await stockFishService.AnalyzePosition(moves, analyzeRequest.Depth);
                var classification = await stockFishService.ClassifyMove(bestMoves, move, moves, analyzeRequest.Depth);
                classifications.Add(new MoveClassificationResponse(move, (int)classification.Classification));
                currentFen += $" {move}"; // Atualiza a FEN com o movimento atual
                moves.Add(move);
            }
            _logger.LogInformation("Análise concluída com sucesso.");
            return Results.Ok(new AnalyzeResponse(classifications));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao analisar a partida.");
            return Results.Problem("Erro ao analisar a partida: " + ex.Message);
        }
    }
}
