namespace SigeParkApp
{
    public partial class AppShell : Shell
    {
        public AppShell(MainPage mainPage)
        {
            InitializeComponent();
            
            // Configurar la página principal usando DI
            Items.Clear();
            Items.Add(new ShellContent
            {
                Title = "Home",
                Route = "MainPage",
                Content = mainPage
            });
        }
    }
}
