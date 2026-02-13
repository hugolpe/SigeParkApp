using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using SigeParkApp.Models;

namespace SigeParkApp.Services
{
    /// <summary>
    /// Servicio de autenticación que se comunica con la API de UserApi
    /// </summary>
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private const string LoginEndpoint = "/api/accounts/login";

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Realiza el login del usuario con la API
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <param name="password">Contraseña del usuario</param>
        /// <returns>Resultado de la autenticación</returns>
        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                // Crear el objeto de solicitud
                var loginRequest = new LoginRequest
                {
                    Email = email,
                    Password = password
                };

                // Crear un mensaje HTTP con el header para evitar la página intermedia de ngrok
                var request = new HttpRequestMessage(HttpMethod.Post, LoginEndpoint)
                {
                    Content = JsonContent.Create(loginRequest)
                };
                request.Headers.Add("ngrok-skip-browser-warning", "true");

                // Realizar la solicitud POST al endpoint de login
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta como string
                    var message = await response.Content.ReadAsStringAsync();
                    
                    // Deserializar correctamente si es un string JSON
                    // Si falla, usar el string original (respuesta no estándar)
                    try
                    {
                        message = JsonSerializer.Deserialize<string>(message) ?? message;
                    }
                    catch (JsonException)
                    {
                        // La respuesta no es un JSON válido, usar el contenido tal cual
                    }

                    return new AuthResult
                    {
                        Success = true,
                        Message = message
                    };
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Error 401: Credenciales incorrectas
                    var message = await response.Content.ReadAsStringAsync();
                    
                    // Deserializar correctamente si es un string JSON
                    // Si falla, usar el string original (respuesta no estándar)
                    try
                    {
                        message = JsonSerializer.Deserialize<string>(message) ?? message;
                    }
                    catch (JsonException)
                    {
                        // La respuesta no es un JSON válido, usar el contenido tal cual
                    }
                    
                    return new AuthResult
                    {
                        Success = false,
                        Message = message
                    };
                }
                else
                {
                    // Otro error HTTP
                    return new AuthResult
                    {
                        Success = false,
                        Message = $"Error del servidor: {response.StatusCode}"
                    };
                }
            }
            catch (HttpRequestException)
            {
                // Error de conexión o red
                return new AuthResult
                {
                    Success = false,
                    Message = "Error de conexión. Verifica tu conexión a internet."
                };
            }
            catch (TaskCanceledException)
            {
                // Timeout
                return new AuthResult
                {
                    Success = false,
                    Message = "La solicitud tardó demasiado tiempo. Intenta nuevamente."
                };
            }
            catch (Exception ex)
            {
                // Error general - en producción, esto debería registrarse para debugging
                return new AuthResult
                {
                    Success = false,
                    Message = $"Error inesperado: {ex.Message}"
                };
            }
        }
    }
}
