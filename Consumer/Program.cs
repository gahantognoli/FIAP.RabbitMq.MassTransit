using Consumer;
using Consumer.Eventos;
using Core;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

var fila = builder.Configuration.GetSection("MassTransit")["NomeFila"] ?? "";
var servidor = builder.Configuration.GetSection("MassTransit")["Servidor"] ?? "";
var usuario = builder.Configuration.GetSection("MassTransit")["Usuario"] ?? "";
var senha = builder.Configuration.GetSection("MassTransit")["Senha"] ?? "";

builder.Services.AddMassTransit(cfgBus =>
{
    cfgBus.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(servidor, "/", hostCfg =>
        {
            hostCfg.Username(usuario);
            hostCfg.Password(senha);
        });
        
        cfg.ReceiveEndpoint(fila, cfgEndpoint =>
        {
            cfgEndpoint.Consumer<PedidoCriadoConsumidor>();
        });
        
        cfg.ConfigureEndpoints(ctx);
    });
    
    cfgBus.AddConsumer<PedidoCriadoConsumidor>();
});

var host = builder.Build();
host.Run();