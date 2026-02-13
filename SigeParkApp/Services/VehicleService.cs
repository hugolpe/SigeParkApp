using System.Text.Json;
using SigeParkApp.Models;

namespace SigeParkApp.Services
{
    /// <summary>
    /// Servicio para gestionar vehículos en el parqueadero
    /// </summary>
    public class VehicleService
    {
        private readonly HttpClient _httpClient;
        private const string ListaDentroEndpoint = "/api/Movimientos/lista-dentro";

        public VehicleService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Obtiene la lista de vehículos dentro del parqueadero
        /// </summary>
        /// <returns>Lista de vehículos dentro</returns>
        public async Task<List<VehiculoDentro>> GetVehiculosDentroAsync()
        {
            try
            {
                Console.WriteLine($"[VehicleService] Iniciando solicitud a {ListaDentroEndpoint}");

                // Crear un mensaje HTTP con el header para evitar la página intermedia de ngrok
                var request = new HttpRequestMessage(HttpMethod.Get, ListaDentroEndpoint);
                request.Headers.Add("ngrok-skip-browser-warning", "true");

                // Realizar la solicitud GET al endpoint
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta como string
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[VehicleService] Respuesta recibida exitosamente. Contenido length: {content.Length}");

                    // Deserializar la respuesta
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var vehiculos = JsonSerializer.Deserialize<List<VehiculoDentro>>(content, options) ?? new List<VehiculoDentro>();

                    Console.WriteLine($"[VehicleService] Deserialized {vehiculos.Count} vehículos");

                    // Procesar cada vehículo para normalizar fechas y calcular tiempo transcurrido
                    var ahora = DateTime.Now;
                    foreach (var vehiculo in vehiculos)
                    {
                        // Normalizar fechas: intentar fechaEntrada, luego cfechaEntradaUtc, luego parsear fechaEntradaUtcIso
                        DateTime? fechaEntradaNormalizada = null;

                        if (vehiculo.FechaEntrada.HasValue)
                        {
                            fechaEntradaNormalizada = vehiculo.FechaEntrada.Value;
                            // Si es UTC, convertir a hora local
                            if (fechaEntradaNormalizada.Value.Kind == DateTimeKind.Utc)
                            {
                                fechaEntradaNormalizada = fechaEntradaNormalizada.Value.ToLocalTime();
                            }
                        }
                        else if (vehiculo.CFechaEntradaUtc.HasValue)
                        {
                            fechaEntradaNormalizada = vehiculo.CFechaEntradaUtc.Value;
                            // Si es UTC, convertir a hora local
                            if (fechaEntradaNormalizada.Value.Kind == DateTimeKind.Utc)
                            {
                                fechaEntradaNormalizada = fechaEntradaNormalizada.Value.ToLocalTime();
                            }
                        }
                        else if (!string.IsNullOrEmpty(vehiculo.FechaEntradaUtcIso))
                        {
                            // Intentar parsear la fecha ISO
                            if (DateTime.TryParse(vehiculo.FechaEntradaUtcIso, out var fechaParseada))
                            {
                                fechaEntradaNormalizada = fechaParseada;
                                // Si es UTC, convertir a hora local
                                if (fechaEntradaNormalizada.Value.Kind == DateTimeKind.Utc)
                                {
                                    fechaEntradaNormalizada = fechaEntradaNormalizada.Value.ToLocalTime();
                                }
                            }
                        }

                        // Calcular tiempo transcurrido
                        if (fechaEntradaNormalizada.HasValue)
                        {
                            var elapsed = ahora - fechaEntradaNormalizada.Value;
                            vehiculo.Horas = (int)elapsed.TotalHours;
                            vehiculo.Minutos = elapsed.Minutes;
                        }

                        // Si no hay tipo, detectarlo usando el método auxiliar
                        if (string.IsNullOrEmpty(vehiculo.Tipo))
                        {
                            vehiculo.Tipo = DetectarTipoPorPlaca(vehiculo.Placa);
                        }
                    }

                    return vehiculos;
                }
                else
                {
                    Console.WriteLine($"[VehicleService] Error en la respuesta: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[VehicleService] Error content: {errorContent}");
                    return new List<VehiculoDentro>();
                }
            }
            catch (HttpRequestException ex)
            {
                // Error de conexión o red
                Console.WriteLine($"[VehicleService] Error de conexión: {ex.Message}");
                throw new Exception("Error de conexión. Verifica tu conexión a internet.", ex);
            }
            catch (TaskCanceledException ex)
            {
                // Timeout
                Console.WriteLine($"[VehicleService] Timeout: {ex.Message}");
                throw new Exception("La solicitud tardó demasiado tiempo. Intenta nuevamente.", ex);
            }
            catch (Exception ex)
            {
                // Error general
                Console.WriteLine($"[VehicleService] Error inesperado: {ex.Message}");
                throw new Exception($"Error al obtener los vehículos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Detecta el tipo de vehículo basándose en el último carácter de la placa
        /// </summary>
        /// <param name="placa">Placa del vehículo</param>
        /// <returns>Tipo de vehículo: "Moto", "Carro" o "Desconocido"</returns>
        public static string DetectarTipoPorPlaca(string placa)
        {
            if (string.IsNullOrEmpty(placa))
            {
                return "Desconocido";
            }

            var ultimoCaracter = placa[^1];

            if (char.IsLetter(ultimoCaracter))
            {
                return "Moto";
            }
            else if (char.IsDigit(ultimoCaracter))
            {
                return "Carro";
            }
            else
            {
                return "Desconocido";
            }
        }
    }
}
