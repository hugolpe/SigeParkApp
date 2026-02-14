using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SigeParkApp.Services;

namespace SigeParkApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddHttpClient<AuthService>(client =>
            {
                client.BaseAddress = new Uri("https://unsecularized-marshlike-lavone.ngrok-free.dev");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            builder.Services.AddHttpClient<VehicleService>(client =>
            {
                client.BaseAddress = new Uri("https://unsecularized-marshlike-lavone.ngrok-free.dev");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            //// Registrar páginas como Transient para evitar problemas de DI circular
            //builder.Services.AddTransient<MainPage>();
            //builder.Services.AddTransient<VehiclesPage>();
            //builder.Services.AddTransient<AppShell>();

            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<VehiclesPage>();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
