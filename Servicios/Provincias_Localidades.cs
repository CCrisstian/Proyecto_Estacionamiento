using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Proyecto_Estacionamiento.Servicios
{
    public class Provincias_Localidades
    {
        public class Provincia
        {
            public string nombre { get; set; }
        }

        public class ProvinciasResponse
        {
            public List<Provincia> provincias { get; set; }
        }

        public class Localidad
        {
            public string nombre { get; set; }
        }

        public class LocalidadesResponse
        {
            public List<Localidad> localidades { get; set; }
        }

        // Obtener lista de Provincias desde la API de Datos Abiertos del Gobierno Argentino
        public async Task<List<string>> ObtenerProvinciasAsync()
        {
            using (HttpClient client = new HttpClient()) // HttpClient se usa para hacer solicitudes HTTP
            {
                // URL de la API Gobierno Argentino para obtener provincias
                string url = "https://apis.datos.gob.ar/georef/api/provincias";
                
                // GET request a la API para obtener las provincias
                var response = await client.GetStringAsync(url);
                
                // convertir el JSON recibido a un objeto de tipo ProvinciasResponse
                var data = JsonConvert.DeserializeObject<ProvinciasResponse>(response);
                // devolver una lista de nombres de provincias
                return data.provincias.Select(p => p.nombre).OrderBy(p => p).ToList();
            }
        }

        // Obtener lista de Localidades por Provincia desde la API de Datos Abiertos del Gobierno Argentino
        public async Task<List<string>> ObtenerLocalidadesAsync(string provincia)
        {
            using (HttpClient client = new HttpClient())
            {
                // Máximo de 5000 resultados para asegurarte que incluye todas las localidades
                string url = $"https://apis.datos.gob.ar/georef/api/localidades?provincia={provincia}&max=5000";
                var response = await client.GetStringAsync(url);
                var data = JsonConvert.DeserializeObject<LocalidadesResponse>(response);
                return data.localidades.Select(l => l.nombre).OrderBy(l => l).ToList();
            }
        }
    }
}