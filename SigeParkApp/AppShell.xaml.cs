namespace SigeParkApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Registrar rutas para navegación
            Routing.RegisterRoute("vehicles", typeof(VehiclesPage));
        }
    }
}
