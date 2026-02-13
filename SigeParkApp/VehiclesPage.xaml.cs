using SigeParkApp.Models;
using SigeParkApp.Services;

namespace SigeParkApp
{
    public partial class VehiclesPage : ContentPage
    {
        private readonly VehicleService _vehicleService;

        public VehiclesPage(VehicleService vehicleService)
        {
            InitializeComponent();
            _vehicleService = vehicleService;
        }

        /// <summary>
        /// Se llama cuando la página aparece en pantalla
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarVehiculosAsync();
        }

        /// <summary>
        /// Carga los vehículos desde el API y actualiza la UI
        /// </summary>
        private async Task CargarVehiculosAsync()
        {
            try
            {
                Console.WriteLine("[VehiclesPage] Cargando vehículos...");
                
                // Obtener los vehículos del servicio
                var vehiculos = await _vehicleService.GetVehiculosDentroAsync();

                // Actualizar la UI en el hilo principal
                await Dispatcher.DispatchAsync(() =>
                {
                    // Actualizar el CollectionView
                    listaDentro.ItemsSource = vehiculos;

                    // Calcular y mostrar resumen
                    if (vehiculos.Any())
                    {
                        var totalCarros = vehiculos.Count(v => v.Tipo == "Carro");
                        var totalMotos = vehiculos.Count(v => v.Tipo == "Moto");
                        var total = vehiculos.Count;

                        lblResumen.Text = $"Total: {total} — Carro: {totalCarros} Moto: {totalMotos}";
                    }
                    else
                    {
                        lblResumen.Text = "No hay vehículos dentro";
                    }

                    Console.WriteLine($"[VehiclesPage] Cargados {vehiculos.Count} vehículos exitosamente");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VehiclesPage] Error al cargar vehículos: {ex.Message}");
                
                // Mostrar mensaje de error en el hilo principal
                await Dispatcher.DispatchAsync(() =>
                {
                    lblResultado.Text = $"Error al cargar vehículos: {ex.Message}";
                    lblResultado.TextColor = Colors.Red;
                    lblResultado.IsVisible = true;
                });
            }
        }

        /// <summary>
        /// Evento del botón Validar Placa
        /// </summary>
        private void OnValidarClicked(object sender, EventArgs e)
        {
            // Validar que txtPlaca no esté vacío
            if (string.IsNullOrWhiteSpace(txtPlaca.Text))
            {
                lblResultado.Text = "Por favor ingrese una placa";
                lblResultado.TextColor = Colors.Red;
                lblResultado.IsVisible = true;
                return;
            }

            // Convertir placa a mayúsculas
            var placa = txtPlaca.Text.ToUpper();

            // Mostrar mensaje de confirmación con la placa ingresada
            lblResultado.Text = $"Placa ingresada: {placa}";
            lblResultado.TextColor = Colors.Green;
            lblResultado.IsVisible = true;

            Console.WriteLine($"[VehiclesPage] Placa validada: {placa}");
            
            // Preparado para futuras validaciones con el API
        }

        /// <summary>
        /// Evento que se dispara cuando el texto en txtPlaca cambia
        /// Convierte automáticamente el texto a mayúsculas
        /// </summary>
        private void txtPlaca_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPlaca.Text))
            {
                // Convertir automáticamente a mayúsculas
                var cursorPosition = txtPlaca.CursorPosition;
                txtPlaca.Text = txtPlaca.Text.ToUpper();
                txtPlaca.CursorPosition = cursorPosition;
            }
        }
    }
}
