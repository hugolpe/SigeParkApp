using Microsoft.Extensions.Logging;
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

            // Configuración de HttpClient con la URL base de la API
            builder.Services.AddHttpClient<AuthService>(client =>
            {
                client.BaseAddress = new Uri("https://subpar-overidly-meta.ngrok-free.dev");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Configuración de HttpClient para VehicleService
            builder.Services.AddHttpClient<VehicleService>(client =>
            {
                client.BaseAddress = new Uri("https://subpar-overidly-meta.ngrok-free.dev");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Registrar servicios en el contenedor de DI
            // AuthService ya está registrado por AddHttpClient<AuthService>
            // VehicleService ya está registrado por AddHttpClient<VehicleService>
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<VehiclesPage>();
            builder.Services.AddSingleton<AppShell>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
