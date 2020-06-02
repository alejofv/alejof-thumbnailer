using AlejoF.Thumbnailer.Settings;
using MediatR;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Alejof.Thumbnailer.Startup))]
namespace Alejof.Thumbnailer
{
     public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddFunctionSettings();
            builder.Services.AddMediatR(typeof(Startup));
        }
    }
}
