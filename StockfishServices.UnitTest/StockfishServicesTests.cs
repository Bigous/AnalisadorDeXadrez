using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockfishServices.UnitTest;

[TestClass]
public class StockfishServicesTests
{
    private List<string> _moves01;
    private StockfishService _service;
    [TestInitialize]
    public void Setup()
    {
        _moves01 = "e2e4 e7e6 d2d4 c7c5 d4d5 e6d5 e4d5 f8d6 b1c3 a7a6 c3e4 d8c7 g2g3 g8e7 f1g2 e8g8 g1h3 h7h6 e1g1 d6e5 d5d6 e5d6 e4d6 b8c6 c1f4 c7a5 d6c8 a8c8 d1d7 b7b5 d7g4 e7g6 f4d6 f8d8 d6c5 c6e5 g4b4 a5b4 c5b4 e5c4 a1d1 a6a5 d1d8 c8d8 b4c3 b5b4 b2b3 c4a3 c3a1 a3c2 f1c1 c2d4 a1d4 d8d4 h3f4 g6f4 g3f4 d4f4 c1c5 f4g4 c5a5 g7g5 a2a3 b4a3 a5a3 g8g7 a3a8 g7g6 h2h3 g4b4 g2d5 f7f5 a8g8 g6f6 g8h8 f6g7 h8g8 g7f6 d5c4 b4b7 g8f8 f6g6 g1g2 g6h5 g2g3 f5f4 g3f3 b7e7 f3g2 h5h4 f8h8 h6h5 h8g8 e7e5 b3b4 e5e4 g8c8 g5g4 h3g4 h5g4 b4b5 f4f3 g2h2 e4e7 c8h8 h4g5 h2g3 e7c7 c4d5 c7c5 d5c6 c5c4 h8g8 g5f6 g8g4".Split(' ').ToList();
        _service = new();
    }

    [TestMethod]
    public async Task AnalysePositionAsync_ShouldReturn5BestMoves()
    {
        // Arrange
        var stockfishService = new StockfishService();
        var depth = 18;

        // Act
        var result = await stockfishService.AnalyzePositionAsync(["e2e4"], depth);

        // Assert
        Assert.AreEqual(5, result.Count);
    }
}
