using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Estacionamiento
{
    public partial class Estacionamiento_Crear : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                for (int hora = 0; hora < 24; hora++)
                {
                    string horaTexto = hora.ToString("D2") + ":00";
                    ddlHoraInicio.Items.Add(horaTexto);
                    ddlHoraFin.Items.Add(horaTexto);
                }
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            string horario = ddlHoraInicio.SelectedValue + "-" + ddlHoraFin.SelectedValue;
        }
    }
}