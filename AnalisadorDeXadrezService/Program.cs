using AnalisadorDeXadrezService.Controllers;
using AnalisadorDeXadrezService.Serialization;
using Microsoft.AspNetCore.Mvc;
using StockfishServices;


var builder = WebApplication.CreateSlimBuilder(args);

//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    serverOptions.ListenAnyIP(5001, listenOptions =>
//    {
//        listenOptions.UseHttps();
//    });
//});
//builder.WebHost.ConfigureKestrel((context, options) =>
//{
//    options.Configure(context.Configuration.GetSection("Kestrel"));
//});


builder.Services.AddScoped<StockfishService>();

// Adicione o contexto JSON ao JsonSerializerOptions
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.TypeInfoResolver = JsonContext.Default;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

Controllers.Register(app);

app.Run();
