using System.Text.Json.Serialization;

namespace SigeParkApp.Models
{
    /// <summary>
    /// Modelo para un vehículo dentro del parqueadero
    /// </summary>
    public class VehiculoDentro
    {
        [JsonPropertyName("placa")]
        public string Placa { get; set; } = string.Empty;

        [JsonPropertyName("fechaEntrada")]
        public DateTime? FechaEntrada { get; set; }

        [JsonPropertyName("cfechaEntradaUtc")]
        public DateTime? CFechaEntradaUtc { get; set; }

        [JsonPropertyName("fechaEntradaUtcIso")]
        public string FechaEntradaUtcIso { get; set; } = string.Empty;

        [JsonPropertyName("horas")]
        public int Horas { get; set; }

        [JsonPropertyName("minutos")]
        public int Minutos { get; set; }

        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = string.Empty;
    }
}
