using System.Text.Json.Serialization;

namespace SigeParkApp.Models
{
    /// <summary>
    /// Modelo para la solicitud de login
    /// </summary>
    public class LoginRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario. No debe ser registrada en logs.
        /// Nota: En una implementación más segura, considerar limpiar de memoria después de usar.
        /// </summary>
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Modelo para el resultado de autenticación
    /// </summary>
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
