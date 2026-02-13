namespace SigeParkApp
{
    public partial class AppShell : Shell
    {
        public AppShell(MainPage mainPage, VehiclesPage vehiclesPage)
        {
            InitializeComponent();
            
            // Configurar las páginas usando DI
            Items.Clear();
            Items.Add(new ShellContent
            {
                Title = "Home",
                Route = "MainPage",
                Content = mainPage
            });
            Items.Add(new ShellContent
            {
                Title = "Vehículos",
                Route = "vehicles",
                Content = vehiclesPage
            });
        }
    }
}
