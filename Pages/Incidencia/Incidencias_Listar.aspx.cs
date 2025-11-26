using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO; // Necesario para MemoryStream
using System.Data.Entity;

namespace Proyecto_Estacionamiento.Pages.Incidencia
{
    public partial class Incidencias_Listar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string estacionamiento = Session["Usu_estacionamiento"] as string;

                if (!string.IsNullOrEmpty(estacionamiento))
                {
                    Estacionamiento_Nombre.Text = $"Estacionamiento: '<strong>{estacionamiento}</strong>'";
                }
                else
                {
                    Estacionamiento_Nombre.Visible = false;
                }

                string tipoUsuario = Session["Usu_tipo"] as string;
                int legajo = Convert.ToInt32(Session["Usu_legajo"]);

                if (tipoUsuario != "Playero")
                {
                    btnIncidencia.Visible = false;
                }
                else
                {
                    gvIncidencias.Columns[0].Visible = false;
                    ButtonVolver.Visible = false;
                }

                CargarLogicaEstacionamiento();

                // Fechas por defecto
                txtDesde.Text = DateTime.Now.ToString("dd/MM/yyyy");
                txtHasta.Text = DateTime.Now.ToString("dd/MM/yyyy");

                CargarIncidencias();
            }
        }

        private void CargarLogicaEstacionamiento()
        {
            string tipoUsuario = Session["Usu_tipo"] as string;

            // --- CASO A: PLAYERO ---
            if (tipoUsuario == "Playero")
            {
                // El playero ya tiene su estacionamiento asignado en sesión (desde el Login)
                // No debe ver el selector.
                lblEstacionamiento.Visible = false;
                ddlEstacionamiento.Visible = false;

                // El nombre del estacionamiento ya se carga en el Page_Load principal
                // con Estacionamiento_Nombre.Text, así que no necesitamos hacer nada más aquí.
                return;
            }

            // --- CASO B: DUEÑO ---
            if (Session["Dueño_EstId"] != null)
            {
                // CASO B.1: Estacionamiento YA seleccionado (vino desde el menú principal)
                string nombreEst = Session["Usu_estacionamiento"] as string;
                Estacionamiento_Nombre.Text = $"Estacionamiento: '<strong>{nombreEst}</strong>'";
                Estacionamiento_Nombre.Visible = true;

                // Ocultar selección manual
                lblEstacionamiento.Visible = false;
                ddlEstacionamiento.Visible = false;
            }
            else
            {
                // CASO B.2: NO hay estacionamiento seleccionado -> Permitir elegir de la lista
                Estacionamiento_Nombre.Visible = false;

                lblEstacionamiento.Visible = true;
                ddlEstacionamiento.Visible = true;

                CargarComboEstacionamientos(); // Este método ya funciona bien para Dueños
            }
        }

        private void CargarComboEstacionamientos()
        {
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                var lista = db.Estacionamiento
                              .Where(e => e.Dueño_Legajo == legajo)
                              .Select(e => new { e.Est_id, e.Est_nombre })
                              .ToList();

                ddlEstacionamiento.DataSource = lista;
                ddlEstacionamiento.DataTextField = "Est_nombre";
                ddlEstacionamiento.DataValueField = "Est_id";
                ddlEstacionamiento.DataBind();

                // Agregar opción por defecto
                ddlEstacionamiento.Items.Insert(0, new System.Web.UI.WebControls.ListItem("-- Seleccione --", "0"));
            }
        }

        // Evento al cambiar la selección (opcional, por si quieres actualizar algo en pantalla)
        protected void ddlEstacionamiento_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Si necesitas hacer algo cuando cambia, ej. limpiar el reporte
        }


        // VALIDACIÓN - Estacionamiento
        protected void CvEstacionamiento_ServerValidate(object source, ServerValidateEventArgs args)
        {
            // Solo validamos si el control está visible (es decir, si el dueño no tenía uno preseleccionado)
            if (ddlEstacionamiento.Visible)
            {
                if (ddlEstacionamiento.SelectedValue == "0")
                {
                    ((CustomValidator)source).ErrorMessage = "Debe seleccionar un Estacionamiento.";
                    args.IsValid = false;
                    return;
                }
            }
            args.IsValid = true;
        }

        // VALIDACIÓN - Fecha Desde
        protected void CvDesde_ServerValidate(object source, ServerValidateEventArgs args)
        {
            string valor = txtDesde.Text.Trim();

            if (string.IsNullOrWhiteSpace(valor))
            {
                ((CustomValidator)source).ErrorMessage = "La fecha 'Desde' es obligatoria.";
                args.IsValid = false;
                return;
            }

            if (!DateTime.TryParse(valor, out _))
            {
                ((CustomValidator)source).ErrorMessage = "Formato de fecha inválido.";
                args.IsValid = false;
                return;
            }

            args.IsValid = true;
        }

        // VALIDACIÓN - Desde > Hasta
        protected void CvHasta_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var validator = (CustomValidator)source;
            string valorHasta = txtHasta.Text.Trim();
            string valorDesde = txtDesde.Text.Trim();

            // A. Validar vacío
            if (string.IsNullOrWhiteSpace(valorHasta))
            {
                validator.ErrorMessage = "La fecha 'Hasta' es obligatoria.";
                args.IsValid = false;
                return;
            }

            // B. Validar formato
            if (!DateTime.TryParse(valorHasta, out DateTime fechaHasta))
            {
                validator.ErrorMessage = "Formato de fecha inválido.";
                args.IsValid = false;
                return;
            }

            // C. Validar Rango (Desde > Hasta)
            // Solo comparamos si la fecha 'Desde' también es válida
            if (DateTime.TryParse(valorDesde, out DateTime fechaDesde))
            {
                if (fechaDesde > fechaHasta)
                {
                    validator.ErrorMessage = "La fecha 'Desde' no puede ser mayor que 'Hasta'.";
                    args.IsValid = false;
                    return;
                }
            }

            args.IsValid = true;
        }

        // DTO para poblar la grilla
        public class IncidenciaDTO
        {
            public int Playero_legajo { get; set; }
            public DateTime Inci_fecha_Hora { get; set; }
            public string PlayeroNombre { get; set; }
            public string FechaHoraStr { get; set; }
            public string Inci_Motivo { get; set; }
            public string Inci_descripcion { get; set; }
            public string DownloadUrl { get; set; }
        }

        protected void btnFiltrarIncidencia_Click(object sender, EventArgs e)
        {
            // Verificar que la página antes de consultar
            Page.Validate();

            if (Page.IsValid)
            {
                CargarIncidencias();
            }
        }

        private void CargarIncidencias()
        {
            string tipoUsuario = Session["Usu_tipo"] as string;
            int legajo = Session["Usu_legajo"] != null ? Convert.ToInt32(Session["Usu_legajo"]) : 0;

            using (var db = new ProyectoEstacionamientoEntities())
            {
                // 1. Consulta base con Eager Loading
                IQueryable<Incidencias> query = db.Incidencias
                                                  .Include("Playero")
                                                  .Include("Playero.Usuarios");

                // 2. Filtros por ROL y ESTACIONAMIENTO
                if (tipoUsuario == "Dueño")
                {
                    if (Session["Dueño_EstId"] != null)
                    {
                        // Dueño gestionando un estacionamiento específico
                        int estIdSeleccionado = (int)Session["Dueño_EstId"];
                        query = query.Where(i => i.Playero.Est_id == estIdSeleccionado);
                    }
                    else
                    {
                        // Dueño viendo reporte global

                        // A. Filtro base de seguridad (solo sus estacionamientos)
                        var estIdsDelDueño = db.Estacionamiento
                                               .Where(e => e.Dueño_Legajo == legajo)
                                               .Select(e => e.Est_id);

                        query = query.Where(i => i.Playero.Est_id.HasValue && estIdsDelDueño.Contains(i.Playero.Est_id.Value));

                        // B. Filtro del DropDown (ddlEstacionamiento) - NUEVO
                        if (ddlEstacionamiento.Visible &&
                            !string.IsNullOrEmpty(ddlEstacionamiento.SelectedValue) &&
                            ddlEstacionamiento.SelectedValue != "0" && // Ojo: Verifica si usas "0" o "-1" en tu insert
                            ddlEstacionamiento.SelectedValue != "-1")
                        {
                            int idEstacionamientoFiltro = int.Parse(ddlEstacionamiento.SelectedValue);
                            query = query.Where(i => i.Playero.Est_id == idEstacionamientoFiltro);
                        }
                    }
                }
                else if (tipoUsuario == "Playero")
                {
                    // El playero solo ve sus propias incidencias
                    query = query.Where(i => i.Playero_legajo == legajo);
                }

                // 3. FILTROS DE FECHAS (NUEVO BLOQUE)
                // ---------------------------------------------------------
                DateTime fechaDesde;
                DateTime fechaHasta;

                // Fecha Desde
                if (!string.IsNullOrEmpty(txtDesde.Text) &&
                    DateTime.TryParseExact(txtDesde.Text, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out fechaDesde))
                {
                    query = query.Where(i => i.Inci_fecha_Hora >= fechaDesde);
                }

                // Fecha Hasta
                if (!string.IsNullOrEmpty(txtHasta.Text) &&
                    DateTime.TryParseExact(txtHasta.Text, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out fechaHasta))
                {
                    // Ajustamos al final del día (23:59:59)
                    DateTime finDelDia = fechaHasta.AddDays(1).AddTicks(-1);
                    query = query.Where(i => i.Inci_fecha_Hora <= finDelDia);
                }

                // 4. Ejecución y Proyección
                var listaIncidencias = query
                    .OrderBy(i => i.Playero.Usuarios.Usu_ap)
                    .ThenByDescending(i => i.Inci_fecha_Hora)
                    .ToList(); // Ejecutamos SQL aquí

                var incidenciasDTO = listaIncidencias.Select(i => new IncidenciaDTO
                {
                    Playero_legajo = i.Playero_legajo,
                    Inci_fecha_Hora = i.Inci_fecha_Hora,
                    PlayeroNombre = (i.Playero != null && i.Playero.Usuarios != null)
                                    ? $"{i.Playero.Usuarios.Usu_nom}, {i.Playero.Usuarios.Usu_ap}"
                                    : "Desconocido",
                    FechaHoraStr = i.Inci_fecha_Hora.ToString("dd/MM/yyyy HH:mm"),
                    Inci_Motivo = i.Inci_Motivo,
                    Inci_descripcion = i.Inci_descripcion,
                    DownloadUrl = $"Incidencia_Descargar.aspx?legajo={i.Playero_legajo}&fechaTicks={i.Inci_fecha_Hora.Ticks}"
                })
                .ToList();

                gvIncidencias.DataSource = incidenciasDTO;
                gvIncidencias.DataKeyNames = new string[] { "Playero_legajo", "Inci_fecha_Hora" };
                gvIncidencias.DataBind();
            }
        }

        protected void btnVolver(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Reporte/Reportes_Listar.aspx");
        }

        protected void btnIncidencia_Click(object sender, EventArgs e)
        {
            Response.Redirect("Incidencia_Registrar.aspx");
        }


        protected void gvIncidencias_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Encontrar el LinkButton de descarga
                LinkButton btnDescargar = e.Row.FindControl("btnDescargar") as LinkButton;

                if (btnDescargar != null)
                {
                    // Obtener el ScriptManager (que está en el Site.Master)
                    ScriptManager sm = ScriptManager.GetCurrent(this.Page);

                    if (sm != null)
                    {
                        // Registrar el botón para que haga un PostBack completo (no AJAX)
                        sm.RegisterPostBackControl(btnDescargar);
                    }
                }
            }
        }
    }
}