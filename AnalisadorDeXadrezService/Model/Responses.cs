namespace AnalisadorDeXadrezService.Model;

// Record para representar a resposta de análise
public record AnalyzeResponse(List<MoveClassificationResponse> Classifications);

// Record para representar a classificação dos movimentos
public record MoveClassificationResponse(string Move, int Classification);
