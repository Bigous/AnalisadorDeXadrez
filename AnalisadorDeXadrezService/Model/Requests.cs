using System.Text.Json.Serialization;

namespace AnalisadorDeXadrezService.Model;

public record AnalyzeRequest(
    List<string> Moves, 
    int Depth);