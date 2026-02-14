using System.Globalization;
using System.Text;
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
        private const string StatusEndpoint = "/api/Movimientos/status";
        private const string NuevoEndpoint = "/api/Movimientos/nuevo";
        private const string SalidaEndpoint = "/api/Movimientos/salida";
        private const string CalcularEndpoint = "/api/Movimientos/calcular";

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

        /// <summary>
        /// Obtiene el estado de un vehículo (si está dentro o fuera del parqueadero)
        /// </summary>
        /// <param name="placa">Placa del vehículo</param>
        /// <returns>Respuesta con el estado y detalles del vehículo</returns>
        public async Task<StatusResponse> GetStatusAsync(string placa)
        {
            try
            {
                Console.WriteLine($"[VehicleService] Consultando estado de la placa: {placa}");

                var request = new HttpRequestMessage(HttpMethod.Get, $"{StatusEndpoint}/{placa}");
                request.Headers.Add("ngrok-skip-browser-warning", "true");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[VehicleService] Respuesta de status recibida: {content}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var statusResponse = JsonSerializer.Deserialize<StatusResponse>(content, options);
                    return statusResponse ?? new StatusResponse();
                }
                else
                {
                    Console.WriteLine($"[VehicleService] Error en status: {response.StatusCode}");
                    return new StatusResponse { Error = $"Error: {response.StatusCode}" };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VehicleService] Error al obtener status: {ex.Message}");
                return new StatusResponse { Error = ex.Message };
            }
        }

        /// <summary>
        /// Registra la entrada de un vehículo al parqueadero
        /// </summary>
        /// <param name="placa">Placa del vehículo</param>
        /// <param name="tarifaId">ID de la tarifa seleccionada</param>
        /// <returns>Respuesta con el ticket de entrada</returns>
        public async Task<TicketResponse> RegistrarEntradaAsync(string placa, int tarifaId)
        {
            try
            {
                Console.WriteLine($"[VehicleService] Registrando entrada de {placa} con tarifa {tarifaId}");

                var requestBody = new
                {
                    placa = placa,
                    tarifaId = tarifaId
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, NuevoEndpoint)
                {
                    Content = content
                };
                request.Headers.Add("ngrok-skip-browser-warning", "true");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[VehicleService] Entrada registrada: {responseContent}");

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var ticketResponse = JsonSerializer.Deserialize<TicketResponse>(responseContent, options);
                    return ticketResponse ?? new TicketResponse { Success = false };
                }
                else
                {
                    Console.WriteLine($"[VehicleService] Error al registrar entrada: {response.StatusCode}");
                    return new TicketResponse { Success = false };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VehicleService] Error al registrar entrada: {ex.Message}");
                return new TicketResponse { Success = false };
            }
        }

        /// <summary>
        /// Registra la salida y cobro de un vehículo
        /// </summary>
        /// <param name="placa">Placa del vehículo</param>
        /// <param name="pago">Monto pagado por el cliente</param>
        /// <param name="monto">Monto total a cobrar</param>
        /// <returns>True si se registró exitosamente</returns>
        public async Task<bool> RegistrarSalidaAsync(string placa, decimal pago, decimal monto)
        {
            try
            {
                Console.WriteLine($"[VehicleService] Registrando salida de {placa}, pago: {pago}, monto: {monto}");

                var requestBody = new
                {
                    placa = placa,
                    pago = pago,
                    monto = monto
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, SalidaEndpoint)
                {
                    Content = content
                };
                request.Headers.Add("ngrok-skip-browser-warning", "true");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[VehicleService] Salida registrada exitosamente");
                    return true;
                }
                else
                {
                    Console.WriteLine($"[VehicleService] Error al registrar salida: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VehicleService] Error al registrar salida: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Calcula el monto a cobrar por un vehículo
        /// </summary>
        /// <param name="placa">Placa del vehículo</param>
        /// <returns>Monto a cobrar o null si no se pudo calcular</returns>
        public async Task<decimal?> CalcularMontoAsync(string placa)
        {
            try
            {
                Console.WriteLine($"[VehicleService] Calculando monto para {placa}");

                // Primero obtener el ID del movimiento desde lista-dentro
                var request = new HttpRequestMessage(HttpMethod.Get, ListaDentroEndpoint);
                request.Headers.Add("ngrok-skip-browser-warning", "true");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    var vehiculos = JsonSerializer.Deserialize<List<ListaDentroItem>>(content, options);
                    var vehiculo = vehiculos?.FirstOrDefault(v => v.Placa.Equals(placa, StringComparison.OrdinalIgnoreCase));

                    if (vehiculo != null)
                    {
                        // Calcular el monto usando el ID
                        var calcRequest = new HttpRequestMessage(HttpMethod.Get, $"{CalcularEndpoint}/{vehiculo.Id}");
                        calcRequest.Headers.Add("ngrok-skip-browser-warning", "true");

                        var calcResponse = await _httpClient.SendAsync(calcRequest);

                        if (calcResponse.IsSuccessStatusCode)
                        {
                            var calcContent = await calcResponse.Content.ReadAsStringAsync();
                            var calcResult = JsonSerializer.Deserialize<CalculoResult>(calcContent, options);
                            Console.WriteLine($"[VehicleService] Monto calculado: {calcResult?.Monto}");
                            return calcResult?.Monto;
                        }
                    }
                }

                Console.WriteLine($"[VehicleService] No se pudo calcular el monto");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VehicleService] Error al calcular monto: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene la fecha de entrada de un vehículo como fallback
        /// </summary>
        /// <param name="placa">Placa del vehículo</param>
        /// <returns>Fecha de entrada o null si no se encuentra</returns>
        public async Task<DateTime?> ObtenerFechaEntradaAsync(string placa)
        {
            try
            {
                Console.WriteLine($"[VehicleService] Obteniendo fecha de entrada para {placa}");

                var vehiculos = await GetVehiculosDentroAsync();
                var vehiculo = vehiculos.FirstOrDefault(v => v.Placa.Equals(placa, StringComparison.OrdinalIgnoreCase));

                return vehiculo?.FechaEntrada ?? vehiculo?.CFechaEntradaUtc;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VehicleService] Error al obtener fecha de entrada: {ex.Message}");
                return null;
            }
        }
    }
}
