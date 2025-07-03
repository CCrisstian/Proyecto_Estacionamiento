using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Tarifas
{
    public partial class Tarifa_Listar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)    // Verifica si es la primera vez que se carga la página
            {
                CargarTarifas(); // Llama al método para cargar las Tarifas
            }
        }

        private void CargarTarifas()
        {
            using (var db = new ProyectoEstacionamientoEntities())  
            {
                var tarifas = db.Tarifa 
                    .Select(t => new
                    {
                        // Selecciona las propiedades necesarias para mostrar en el GridView
                        t.Tarifa_id,
                        Est_nombre = t.Estacionamiento.Est_nombre,
                        Tipos_Tarifa_Descripcion = t.Tipos_Tarifa.Tipos_tarifa_descripcion,
                        Categoria_descripcion = t.Categoria_Vehiculo.Categoria_descripcion,
                        t.Tarifa_Monto,
                        t.Tarifa_Desde
                    })
                    .ToList();  // Obtiene la lista de Tarifas

                gvTarifas.DataSource = tarifas; // Asigna la lista de Tarifas como fuente de datos del GridView
                gvTarifas.DataBind();   // Realiza el enlace de datos al GridView
            }
        }

        protected void btnAgregarTarifa_Click(object sender, EventArgs e)
        {
            Response.Redirect("Tarifa_Crear_Editar.aspx");
        }

        protected void gvTarifas_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Editar")
            {
                string tarifaId = e.CommandArgument.ToString();
                Response.Redirect($"Tarifa_Crear_Editar.aspx?id={tarifaId}");
            }
            
        }
    }
}