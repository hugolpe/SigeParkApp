using SigeParkApp.Services;

namespace SigeParkApp
{
    public partial class MainPage : ContentPage
    {
        private readonly AuthService _authService;

        public MainPage(AuthService authService)
        {
            InitializeComponent();
            _authService = authService;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Validación básica
            if (string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                lblResultado.Text = "Por favor ingrese usuario y contraseña";
                lblResultado.TextColor = Colors.Red;
                return;
            }

            // Mostrar mensaje de carga
            lblResultado.Text = "Iniciando sesión...";
            lblResultado.TextColor = Colors.Blue;
            
            // Llamar al servicio de autenticación real
            var result = await _authService.LoginAsync(txtEmail.Text, txtPassword.Text);
            
            // Mostrar el resultado
            if (result.Success)
            {
                lblResultado.Text = result.Message;
                lblResultado.TextColor = Colors.Green;
                
                // Si el login es exitoso, navega a la siguiente página
                // await Navigation.PushAsync(new HomePage());
            }
            else
            {
                lblResultado.Text = result.Message;
                lblResultado.TextColor = Colors.Red;
            }
        }

        // Descomenta esto si necesitas el botón de vista previa
        /*
        private async void OnTicketPreviewClicked(object sender, EventArgs e)
        {
            // Navegar a la vista previa del ticket
            // await Navigation.PushAsync(new TicketPreviewPage());
        }
        */
    }
}
