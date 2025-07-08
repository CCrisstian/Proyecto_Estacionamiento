using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Turnos
{
    public partial class Turno_Listar : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarTurnos();
                string tipoUsuario = Session["Usu_tipo"] as string;
                if (tipoUsuario != "Playero")
                {
                    // Oculta los elementos si no es Playero
                    btnInicioTurno.Visible = false;
                    btnFinTurno.Visible = false;
                }
            }
        }

        private void CargarTurnos()
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                var lista = db.Turno
                    .Select(t => new
                    {
                        t.Turno_FechaHora_Inicio,
                        t.Turno_FechaHora_fin,
                        t.Caja_Monto_Inicio,
                        t.Caja_Monto_fin,
                        t.Caja_Monto_total,
                        NombrePlayero = t.Playero.Usuarios.Usu_nom,
                        ApellidoPlayero = t.Playero.Usuarios.Usu_ap,
                        Estacionamiento = t.Playero.Estacionamiento.Est_nombre,
                    })
                    .OrderByDescending(t => t.Turno_FechaHora_Inicio)
                    .ToList();

                GridViewTurnos.DataSource = lista;
                GridViewTurnos.DataBind();
            }
        }

        protected void btnInicioTurno_Click(object sender, EventArgs e)
        {
            try
            {
                using (var db = new ProyectoEstacionamientoEntities())
                {
                    if (Session["Usu_legajo"] == null)
                    {
                        Response.Redirect("~/Pages/Login/Login.aspx");
                        return;
                    }

                    int legajoPlayero = (int)Session["Usu_legajo"];

                    bool hayTurnoAbierto = db.Turno.Any(t =>
                        t.Playero_Legajo == legajoPlayero &&
                        t.Turno_FechaHora_fin == null
                    );

                    if (hayTurnoAbierto)
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Ya hay un turno abierto.');", true);
                        return;
                    }

                    // Asignar fecha con precisión truncada a segundos
                    DateTime fechaInicio = DateTime.Now;
                    fechaInicio = new DateTime(fechaInicio.Year, fechaInicio.Month, fechaInicio.Day,
                                               fechaInicio.Hour, fechaInicio.Minute, fechaInicio.Second);

                    var nuevoTurno = new Turno
                    {
                        Playero_Legajo = legajoPlayero,
                        Turno_FechaHora_Inicio = fechaInicio,
                        Caja_Monto_Inicio = 0   // Por ahora, el monto de inicio es 0
                    };

                    db.Turno.Add(nuevoTurno);
                    db.SaveChanges();
                }

                CargarTurnos();
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", $"alert('Error: {ex.Message}');", true);
            }
        }

        protected void btnFinTurno_Click(object sender, EventArgs e)
        {
            try
            {
                using (var db = new ProyectoEstacionamientoEntities())
                {
                    if (Session["Usu_legajo"] == null)
                    {
                        Response.Redirect("~/Pages/Login/Login.aspx");
                        return;
                    }

                    int legajoPlayero = (int)Session["Usu_legajo"];

                    var turnoAbierto = db.Turno
                                         .Where(t => t.Playero_Legajo == legajoPlayero && t.Turno_FechaHora_fin == null)
                                         .OrderByDescending(t => t.Turno_FechaHora_Inicio)
                                         .FirstOrDefault();

                    if (turnoAbierto == null)
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('No hay Turno abierto para finalizar.');", true);
                        return;
                    }

                    DateTime fechaFin = DateTime.Now;
                    fechaFin = new DateTime(fechaFin.Year, fechaFin.Month, fechaFin.Day, fechaFin.Hour, fechaFin.Minute, fechaFin.Second);

                    turnoAbierto.Turno_FechaHora_fin = fechaFin;

                    turnoAbierto.Caja_Monto_fin = 0;    // Por ahora, el Monto Fin es 0

                    // Aquí se realizará el cálculo del Monto Total
                    turnoAbierto.Caja_Monto_total = turnoAbierto.Caja_Monto_fin - (turnoAbierto.Caja_Monto_Inicio ?? 0);

                    db.SaveChanges();
                }

                CargarTurnos();
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", $"alert('Error al finalizar turno: {ex.Message}');", true);
            }
        }


    }
}
