using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Proyecto_Estacionamiento.Servicios
{
    public class ServicioGeocodificacion
    {
        public class NominatimResultado
        {
            [JsonProperty("lat")]
            public string Lat { get; set; }

            [JsonProperty("lon")]
            public string Lon { get; set; }
        }
        public class Coordenadas
        {
            public double? Latitud { get; set; }
            public double? Longitud { get; set; }

            public bool EsValida => Latitud.HasValue && Longitud.HasValue;
        }

        private readonly HttpClient _httpClient;

        public ServicioGeocodificacion()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Proyecto_Estacionamiento/1.0");
        }

        public async Task<Coordenadas> ObtenerCoordenadasAsync(string direccion, string localidad, string provincia)
        {
            try
            {
                // Armar dirección completa para búsqueda
                string direccionCompleta = $"{direccion}, {localidad}, {provincia}, Argentina";
                // URL de la API de Nominatim para geocodificación
                string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(direccionCompleta)}&format=json&limit=1";

                // Realizar solicitud HTTP
                var response = await _httpClient.GetStringAsync(url);

                // Parsear respuesta
                var resultados = JsonConvert.DeserializeObject<List<NominatimResultado>>(response);

                // Validar que haya al menos un resultado
                if (resultados != null && resultados.Count > 0)
                {
                    double lat = double.Parse(resultados[0].Lat, System.Globalization.CultureInfo.InvariantCulture);
                    double lon = double.Parse(resultados[0].Lon, System.Globalization.CultureInfo.InvariantCulture);
                    return new Coordenadas { Latitud = lat, Longitud = lon };
                }
            }
            catch
            {
                return new Coordenadas(); // Devuelve lat y lon como null
            }
            return new Coordenadas(); // Ruta alternativa si no hay resultados
        }
    }
}
