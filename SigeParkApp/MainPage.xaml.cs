namespace SigeParkApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
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

            // Aquí puedes agregar tu lógica de autenticación
            // Por ejemplo, llamar a un servicio web, validar credenciales, etc.
            
            lblResultado.Text = "Iniciando sesión...";
            lblResultado.TextColor = Colors.Blue;
            
            // Simulación de login - REEMPLAZAR esto con autenticación real
            // En una implementación real, validar contra API/base de datos
            await Task.Delay(1000);
            
            // Si el login es exitoso, navega a la siguiente página
            // await Navigation.PushAsync(new HomePage());
            
            lblResultado.Text = "Credenciales correctas";
            lblResultado.TextColor = Colors.Green;
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
