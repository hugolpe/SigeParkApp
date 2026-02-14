namespace SigeParkApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            Items.Clear();
            Items.Add(new ShellContent
            {
                Title = "Home",
                Route = "MainPage",
                ContentTemplate = new DataTemplate(typeof(MainPage))
            });
            Items.Add(new ShellContent
            {
                Title = "Vehículos",
                Route = "vehicles",
                ContentTemplate = new DataTemplate(typeof(VehiclesPage))
            });
        }
    }
}
