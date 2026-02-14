using System.Text.Json.Serialization;

namespace SigeParkApp.Models
{
    public class StatusResponse
    {
        [JsonPropertyName("existe")]
        public bool Existe { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("fechaEntrada")]
        public DateTime? FechaEntrada { get; set; }

        [JsonPropertyName("fechaSalida")]
        public DateTime? FechaSalida { get; set; }

        [JsonPropertyName("dias")]
        public int Dias { get; set; }

        [JsonPropertyName("horas")]
        public int Horas { get; set; }

        [JsonPropertyName("minutos")]
        public int Minutos { get; set; }

        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = string.Empty;

        [JsonPropertyName("formato")]
        public string Formato { get; set; } = string.Empty;

        [JsonPropertyName("valido")]
        public bool Valido { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; } = string.Empty;

        [JsonPropertyName("tarifas")]
        public List<TarifaItem> Tarifas { get; set; } = new();
    }

    public class TarifaItem
    {
        [JsonPropertyName("parkTarId")]
        public int ParkTarId { get; set; }

        [JsonPropertyName("parkTarNomb")]
        public string ParkTarNomb { get; set; } = string.Empty;
    }

    public class TicketResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("ticket")]
        public int Ticket { get; set; }

        [JsonPropertyName("placa")]
        public string Placa { get; set; } = string.Empty;

        [JsonPropertyName("tipoVehiculo")]
        public string TipoVehiculo { get; set; } = string.Empty;

        [JsonPropertyName("fechaEntrada")]
        public string FechaEntrada { get; set; } = string.Empty;
    }

    public class ListaDentroItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("placa")]
        public string Placa { get; set; } = string.Empty;
    }

    public class CalculoResult
    {
        [JsonPropertyName("monto")]
        public decimal Monto { get; set; }
    }
}
