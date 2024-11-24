using Core;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMassTransit(config =>
{
    config.UsingRabbitMq((ctx, cfg) =>
    {
        var servidor = builder.Configuration.GetSection("MassTransit")["Servidor"] ?? "";
        cfg.Host(servidor, "/", hostCfg =>
        {
            var usuario = builder.Configuration.GetSection("MassTransit")["Usuario"] ?? "";
            var senha = builder.Configuration.GetSection("MassTransit")["Senha"] ?? "";
            hostCfg.Username(usuario);
            hostCfg.Password(senha);
        });
        cfg.ConfigureEndpoints(ctx);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/Pedidos", async (IBus bus, IConfiguration configuration) =>
    {
        var fila = configuration.GetSection("MassTransit")["NomeFila"] ?? "";
        
        var endpoint = await bus.GetSendEndpoint(new Uri($"queue:{fila}"));

        var pedidoId = new Random().Next();

        var usuarioId = new Random().Next();
        
        await endpoint.Send(new Pedido(pedidoId, new Usuario(usuarioId, "Gabriel", "gabriel@gmail.com")));
        
        return Results.Ok();
    })
    .WithName("PostPedidos")
    .WithOpenApi();

app.Run();