using AnalisadorDeXadrezService.Model;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace AnalisadorDeXadrezService.Serialization;

[JsonSerializable(typeof(AnalyzeRequest))]
[JsonSerializable(typeof(MoveClassificationResponse))]
[JsonSerializable(typeof(AnalyzeResponse))]
public partial class JsonContext : JsonSerializerContext
{
}
