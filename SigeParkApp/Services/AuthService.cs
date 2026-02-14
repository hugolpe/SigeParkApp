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

                var options = new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var jsonContent = JsonSerializer.Serialize(loginRequest, options);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, LoginEndpoint)
                {
                    Content = content
                };
                
                // Headers necesarios para ngrok
                request.Headers.Add("ngrok-skip-browser-warning", "true");
                request.Headers.Add("Accept", "application/json");

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
