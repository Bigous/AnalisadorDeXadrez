using System.Collections.Concurrent;

namespace AnalisadorDeXadrezService.Controllers;

public static class Controllers
{
    private static ConcurrentBag<IController> _controllers = [];

    public static void Register(WebApplication app)
    {
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

        _controllers.Add(new ChessAnalyzerController(loggerFactory.CreateLogger<ChessAnalyzerController>()));

        foreach (var controller in _controllers)
        {
            controller.Register(app);
        }
    }
}
