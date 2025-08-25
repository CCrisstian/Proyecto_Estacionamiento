using System;
using System.Linq;
using System.Web;
using System.Web.UI;

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
                    btnInicioTurno.Visible = false;
                    btnFinTurno.Visible = false;
                    lblMontoInicio.Visible = false;
                    lblMontoFin.Visible = false;
                    txtMontoInicio.Visible = false;
                    txtMontoFin.Visible = false;
                }
                else
                {
                    GridViewTurnos.Columns[0].Visible = false;
                }
            }
        }

        private void CargarTurnos()
        {
            string tipoUsuario = Session["Usu_tipo"] as string;
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                IQueryable<Turno> query = db.Turno;

                if (tipoUsuario == "Dueño")
                {
                    // Estacionamientos del dueño
                    var estIds = db.Estacionamiento
                                   .Where(e => e.Dueño_Legajo == legajo)
                                   .Select(e => e.Est_id);

                    query = query.Where(t => t.Playero.Est_id.HasValue && estIds.Contains(t.Playero.Est_id.Value));
                }
                else if (tipoUsuario == "Playero")
                {
                    // Estacionamiento asignado al playero
                    var estId = db.Playero
                                  .Where(p => p.Playero_legajo == legajo)
                                  .Select(p => p.Est_id)
                                  .FirstOrDefault();

                    if (estId.HasValue)
                    {
                        query = query.Where(t => t.Playero.Est_id == estId.Value);
                    }
                    else
                    {
                        // No mostrar nada si no tiene estacionamiento asignado
                        query = query.Where(t => false);
                    }
                }

                var turnos = query
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

                GridViewTurnos.DataSource = turnos;
                GridViewTurnos.DataBind();
            }
        }

        protected void btnInicioTurno_Click(object sender, EventArgs e)
        {
            try
            {
                if (!double.TryParse(txtMontoInicio.Text, out double montoInicio) || montoInicio < 0)
                {
                    ScriptManager.RegisterStartupScript(
                        this, this.GetType(), "alertaMontoInicio",
                        "mostrarAlerta('Atención', 'Ingrese un monto de inicio válido y no negativo.', 'warning');",
                        true
                    );
                    return;
                }

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
                        ScriptManager.RegisterStartupScript(
                            this, this.GetType(), "alertaTurnoAbierto",
                            "mostrarAlerta('Atención', 'Ya hay un turno abierto.', 'warning');",
                            true
                        );
                        return;
                    }

                    DateTime fechaInicio = DateTime.Now;
                    fechaInicio = new DateTime(fechaInicio.Year, fechaInicio.Month, fechaInicio.Day,
                                               fechaInicio.Hour, fechaInicio.Minute, fechaInicio.Second);

                    var nuevoTurno = new Turno
                    {
                        Playero_Legajo = legajoPlayero,
                        Turno_FechaHora_Inicio = fechaInicio,
                        Caja_Monto_Inicio = montoInicio
                    };

                    db.Turno.Add(nuevoTurno);
                    db.SaveChanges();
                }

                txtMontoInicio.Text = "";
                string accion = "inicio";
                Response.Redirect($"Turno_Listar.aspx?exito=1&accion={accion}");
            }
            catch (Exception ex)
            {
                var safe = HttpUtility.JavaScriptStringEncode(ex.Message);
                ScriptManager.RegisterStartupScript(
                    this, this.GetType(), "alertaErrorInicio",
                    $"mostrarAlerta('Error', 'Error al iniciar turno: {safe}', 'error');",
                    true
                );
            }
        }

        protected void btnFinTurno_Click(object sender, EventArgs e)
        {
            try
            {
                if (!double.TryParse(txtMontoFin.Text, out double montoFin) || montoFin < 0)
                {
                    ScriptManager.RegisterStartupScript(
                        this, this.GetType(), "alertaMontoFin",
                        "mostrarAlerta('Atención', 'Ingrese un monto de fin válido y no negativo.', 'warning');",
                        true
                    );
                    return;
                }

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
                        ScriptManager.RegisterStartupScript(
                            this, this.GetType(), "alertaSinTurno",
                            "mostrarAlerta('Atención', 'No hay Turno abierto para finalizar.', 'warning');",
                            true
                        );
                        return;
                    }

                    DateTime fechaFin = DateTime.Now;
                    fechaFin = new DateTime(fechaFin.Year, fechaFin.Month, fechaFin.Day, fechaFin.Hour, fechaFin.Minute, fechaFin.Second);

                    turnoAbierto.Turno_FechaHora_fin = fechaFin;
                    turnoAbierto.Caja_Monto_fin = montoFin;

                    turnoAbierto.Caja_Monto_total = montoFin - (turnoAbierto.Caja_Monto_Inicio ?? 0);

                    db.SaveChanges();
                }

                txtMontoFin.Text = "";
                string accion = "fin";
                Response.Redirect($"Turno_Listar.aspx?exito=1&accion={accion}");
            }
            catch (Exception ex)
            {
                var safe = HttpUtility.JavaScriptStringEncode(ex.Message);
                ScriptManager.RegisterStartupScript(
                    this, this.GetType(), "alertaErrorFin",
                    $"mostrarAlerta('Error', 'Error al finalizar turno: {safe}', 'error');",
                    true
                );
            }
        }
    }
}
