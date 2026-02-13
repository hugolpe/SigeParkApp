namespace SigeParkApp.Models
{
    /// <summary>
    /// Modelo para la solicitud de login
    /// </summary>
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
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
