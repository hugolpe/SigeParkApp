using SigeParkApp.Models;
using SigeParkApp.Services;

namespace SigeParkApp
{
    public partial class VehiclesPage : ContentPage
    {
        private readonly VehicleService _vehicleService;
        private StatusResponse? _ultimoResultado;

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
                    // Actualizar el BindableLayout
                    BindableLayout.SetItemsSource(listaDentro, vehiculos);

                    // Mostrar/ocultar mensaje de lista vacía
                    if (vehiculos.Any())
                    {
                        lblEmpty.IsVisible = false;
                        // Calcular y mostrar resumen
                        var totalCarros = vehiculos.Count(v => v.Tipo == "Carro");
                        var totalMotos = vehiculos.Count(v => v.Tipo == "Moto");
                        var total = vehiculos.Count;

                        lblResumen.Text = $"Total: {total} — Carro: {totalCarros} Moto: {totalMotos}";
                    }
                    else
                    {
                        lblEmpty.IsVisible = true;
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
        private async void OnValidarClicked(object sender, EventArgs e)
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

            try
            {
                Console.WriteLine($"[VehiclesPage] Validando placa: {placa}");
                
                // Llamar al API para obtener el estado del vehículo
                _ultimoResultado = await _vehicleService.GetStatusAsync(placa);

                if (!string.IsNullOrEmpty(_ultimoResultado.Error))
                {
                    lblResultado.Text = $"Error: {_ultimoResultado.Error}";
                    lblResultado.TextColor = Colors.Red;
                    lblResultado.IsVisible = true;
                    return;
                }

                // Mostrar información del vehículo
                lblTipo.Text = $"Tipo: {_ultimoResultado.Tipo}";
                lblTipo.IsVisible = true;
                
                lblFormato.Text = $"Formato: {_ultimoResultado.Formato}";
                lblFormato.IsVisible = true;

                // Verificar si el vehículo está dentro del parqueadero
                if (_ultimoResultado.Status == "Dentro")
                {
                    // Vehículo está dentro - mostrar botón de cobro
                    lblResultado.Text = "🚗 Vehículo en parqueadero";
                    lblResultado.TextColor = Colors.Orange;
                    lblResultado.IsVisible = true;

                    // Ocultar controles de entrada
                    pickerTarifa.IsVisible = false;
                    btnRegistrar.IsVisible = false;

                    // Mostrar botón de cobro de salida
                    btnCobrarSalida.IsVisible = true;

                    Console.WriteLine($"[VehiclesPage] Vehículo {placa} está dentro del parqueadero");
                }
                else
                {
                    // Vehículo está fuera - mostrar opción de registrar entrada
                    lblResultado.Text = "✅ Listo para registrar";
                    lblResultado.TextColor = Colors.Green;
                    lblResultado.IsVisible = true;

                    // Poblar el picker de tarifas
                    if (_ultimoResultado.Tarifas != null && _ultimoResultado.Tarifas.Any())
                    {
                        pickerTarifa.ItemsSource = _ultimoResultado.Tarifas
                            .Select(t => $"{t.ParkTarNomb} (ID: {t.ParkTarId})")
                            .ToList();
                        pickerTarifa.SelectedIndex = 0;
                        pickerTarifa.IsVisible = true;
                    }

                    // Mostrar botón de registrar entrada
                    btnRegistrar.IsVisible = true;

                    // Ocultar botón de cobro
                    btnCobrarSalida.IsVisible = false;

                    Console.WriteLine($"[VehiclesPage] Vehículo {placa} está fuera del parqueadero");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VehiclesPage] Error al validar placa: {ex.Message}");
                lblResultado.Text = $"Error: {ex.Message}";
                lblResultado.TextColor = Colors.Red;
                lblResultado.IsVisible = true;
            }
        }

        /// <summary>
        /// Evento del botón Registrar Entrada
        /// </summary>
        private async void OnRegistrarClicked(object sender, EventArgs e)
        {
            if (_ultimoResultado == null || string.IsNullOrWhiteSpace(txtPlaca.Text))
            {
                await DisplayAlert("Error", "Primero debe validar la placa", "OK");
                return;
            }

            if (pickerTarifa.SelectedIndex < 0)
            {
                await DisplayAlert("Error", "Debe seleccionar una tarifa", "OK");
                return;
            }

            try
            {
                // Obtener el ID de la tarifa seleccionada
                var tarifaSeleccionada = _ultimoResultado.Tarifas[pickerTarifa.SelectedIndex];
                var placa = txtPlaca.Text.ToUpper();

                Console.WriteLine($"[VehiclesPage] Registrando entrada de {placa} con tarifa {tarifaSeleccionada.ParkTarId}");

                // Llamar al API para registrar la entrada
                var resultado = await _vehicleService.RegistrarEntradaAsync(placa, tarifaSeleccionada.ParkTarId);

                if (resultado.Success)
                {
                    await DisplayAlert("Éxito", 
                        $"Entrada registrada exitosamente\n\nTicket: {resultado.Ticket}\nPlaca: {resultado.Placa}\nTipo: {resultado.TipoVehiculo}\nFecha: {resultado.FechaEntrada}", 
                        "OK");

                    // Limpiar formulario y actualizar lista
                    LimpiarFormulario();
                    await CargarVehiculosAsync();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo registrar la entrada", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VehiclesPage] Error al registrar entrada: {ex.Message}");
                await DisplayAlert("Error", $"Error al registrar entrada: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Evento del botón Cobrar Salida
        /// </summary>
        private async void OnCobrarClicked(object sender, EventArgs e)
        {
            if (_ultimoResultado == null || string.IsNullOrWhiteSpace(txtPlaca.Text))
            {
                await DisplayAlert("Error", "Primero debe validar la placa", "OK");
                return;
            }

            if (_ultimoResultado.Status != "Dentro")
            {
                await DisplayAlert("Error", "El vehículo no está en el parqueadero", "OK");
                return;
            }

            try
            {
                var placa = txtPlaca.Text.ToUpper();
                Console.WriteLine($"[VehiclesPage] Procesando cobro de salida para {placa}");

                // Obtener fecha de entrada
                DateTime? fechaEntrada = _ultimoResultado.FechaEntrada;
                if (!fechaEntrada.HasValue)
                {
                    // Fallback: intentar obtenerla desde lista-dentro
                    fechaEntrada = await _vehicleService.ObtenerFechaEntradaAsync(placa);
                }

                if (!fechaEntrada.HasValue)
                {
                    await DisplayAlert("Error", "No se pudo obtener la fecha de entrada", "OK");
                    return;
                }

                // Calcular horas a cobrar
                var ahora = DateTime.Now;
                var tiempoTranscurrido = ahora - fechaEntrada.Value;
                var horasACobrar = (int)Math.Ceiling(tiempoTranscurrido.TotalHours);

                // Intentar obtener el monto del API
                decimal monto;
                var montoApi = await _vehicleService.CalcularMontoAsync(placa);
                
                if (montoApi.HasValue)
                {
                    monto = montoApi.Value;
                    Console.WriteLine($"[VehiclesPage] Monto obtenido del API: {monto}");
                }
                else
                {
                    // Cálculo local como fallback: primera hora 2000, adicionales 1000 cada una
                    monto = 2000 + ((horasACobrar - 1) * 1000);
                    Console.WriteLine($"[VehiclesPage] Monto calculado localmente: {monto}");
                }

                // Mostrar confirmación con detalles
                var confirmar = await DisplayAlert("Confirmar Cobro",
                    $"Placa: {placa}\n" +
                    $"Tipo: {_ultimoResultado.Tipo}\n" +
                    $"Entrada: {fechaEntrada.Value:dd/MM/yyyy HH:mm}\n" +
                    $"Horas: {horasACobrar}\n" +
                    $"Total a cobrar: ${monto:N0}\n\n" +
                    $"¿Desea proceder con el cobro?",
                    "Sí", "No");

                if (!confirmar)
                {
                    return;
                }

                // Solicitar monto pagado
                var pagoStr = await DisplayPromptAsync("Pago",
                    $"Total: ${monto:N0}\n\nIngrese el monto pagado:",
                    placeholder: "0",
                    keyboard: Keyboard.Numeric);

                if (string.IsNullOrWhiteSpace(pagoStr))
                {
                    return;
                }

                // Validar el monto pagado
                if (!decimal.TryParse(pagoStr, out var pago))
                {
                    await DisplayAlert("Error", "Monto inválido", "OK");
                    return;
                }

                if (pago < monto)
                {
                    await DisplayAlert("Error", $"El pago debe ser al menos ${monto:N0}", "OK");
                    return;
                }

                // Registrar la salida
                var exitoso = await _vehicleService.RegistrarSalidaAsync(placa, pago, monto);

                if (exitoso)
                {
                    var cambio = pago - monto;
                    await DisplayAlert("Salida Registrada",
                        $"Salida registrada exitosamente\n\n" +
                        $"Total: ${monto:N0}\n" +
                        $"Pagado: ${pago:N0}\n" +
                        $"Cambio: ${cambio:N0}",
                        "OK");

                    // Limpiar formulario y actualizar lista
                    LimpiarFormulario();
                    await CargarVehiculosAsync();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo registrar la salida", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VehiclesPage] Error al cobrar salida: {ex.Message}");
                await DisplayAlert("Error", $"Error al procesar salida: {ex.Message}", "OK");
            }
        }

        /// <summary>
        /// Limpia el formulario de validación
        /// </summary>
        private void LimpiarFormulario()
        {
            txtPlaca.Text = string.Empty;
            lblResultado.IsVisible = false;
            lblTipo.IsVisible = false;
            lblFormato.IsVisible = false;
            pickerTarifa.IsVisible = false;
            btnRegistrar.IsVisible = false;
            btnCobrarSalida.IsVisible = false;
            _ultimoResultado = null;
        }
    }
}
