using System;
using System.Web.UI.WebControls;
using System.Threading.Tasks;
using Proyecto_Estacionamiento.Servicios;
using System.Linq;

namespace Proyecto_Estacionamiento.Pages.Estacionamiento
{
    public partial class Estacionamiento_CrearEditar : System.Web.UI.Page
    {
        // Servicio para obtener Provincias y Localidades desde la API de Datos Abiertos del Gobierno Argentino
        private Provincias_Localidades servicioProvinciasLocalidades = new Provincias_Localidades();

        protected async void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Cargar provincias
                var provincias = await servicioProvinciasLocalidades.ObtenerProvinciasAsync();
                ddlProvincia.DataSource = provincias;
                ddlProvincia.DataBind();
                ddlProvincia.Items.Insert(0, new ListItem("- Seleccione una Provincia -", ""));

                // Cargar horas
                for (int hora = 0; hora < 24; hora++)
                {
                    string horaTexto = hora.ToString("D2") + ":00";
                    ddlHoraInicio.Items.Add(horaTexto);
                    ddlHoraFin.Items.Add(horaTexto);
                    ddlHoraInicio_Domingo.Items.Add(horaTexto);
                    ddlHoraFin_Domingo.Items.Add(horaTexto);
                }

                ToggleHoraControls(false);

                // Modo Editar
                if (Request.QueryString["Est_id"] != null)
                {
                    int id = int.Parse(Request.QueryString["Est_id"]);

                    // Guardamos el id en el hidden field
                    hdnEstId.Value = id.ToString();

                    CargarEstacionamiento(id);
                    btnGuardar.Text = "Actualizar";
                }
            }
        }

        protected async void ddlProvincia_SelectedIndexChanged(object sender, EventArgs e)
        {
            string provincia = ddlProvincia.SelectedValue;
            if (!string.IsNullOrEmpty(provincia))
            {
                var localidades = await servicioProvinciasLocalidades.ObtenerLocalidadesAsync(provincia);
                ddlLocalidad.DataSource = localidades;
                ddlLocalidad.DataBind();
                ddlLocalidad.Items.Insert(0, new ListItem("- Seleccione una Localidad -", ""));
            }
        }

        protected void chkDomingo_CheckedChanged(object sender, EventArgs e)
        {
            ToggleHoraControls(chkDomingo.Checked);
        }

        private void ToggleHoraControls(bool visible)
        {
            ddlHoraInicio_Domingo.Visible = visible;
            ddlHoraFin_Domingo.Visible = visible;
        }

        private void LimpiarMensaje()
        {
            lblMensaje.Text = "";
            lblMensaje.ForeColor = System.Drawing.Color.Red;
        }

        private void MostrarError(string mensaje)
        {
            lblMensaje.Text = mensaje;
            lblMensaje.ForeColor = System.Drawing.Color.Red;
        }

        private bool ValidarCamposObligatorios()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtDireccion.Text))
            {
                MostrarError("Los campos Nombre y Dirección son obligatorios.");
                return false;
            }

            if (ddlProvincia.SelectedIndex == 0 || ddlLocalidad.SelectedIndex == 0)
            {
                MostrarError("Los campos Provincia y Localidad son obligatorios.");
                return false;
            }

            return true;
        }
        private bool ValidarHorario(DropDownList ddlInicio, DropDownList ddlFin, string mensajeError)
        {
            int horaInicio = int.Parse(ddlInicio.SelectedValue.Substring(0, 2));
            int horaFin = int.Parse(ddlFin.SelectedValue.Substring(0, 2));

            if (horaInicio >= horaFin)
            {
                MostrarError(mensajeError);
                return false;
            }
            return true;
        }

        private bool ExisteDuplicado(
            ProyectoEstacionamientoEntities db,
            int? id,
            string provincia,
            string localidad,
            string direccion,
            double? latitud,
            double? longitud)
        {
            // Buscamos candidatos con mismos datos clave
            var coincidencias = db.Estacionamiento.Where(est =>
                (est.Est_provincia == provincia &&
                 est.Est_localidad == localidad &&
                 est.Est_direccion == direccion)
                ||
                (est.Est_Latitud == latitud &&
                 est.Est_Longitud == longitud)
            ).ToList();

            foreach (var est in coincidencias)
            {
                // ---------------------------
                // Caso 1: mismo ID y mismos datos -> no es duplicado
                // ---------------------------
                if (id.HasValue && est.Est_id == id.Value &&
                    est.Est_provincia == provincia &&
                    est.Est_localidad == localidad &&
                    est.Est_direccion == direccion &&
                    est.Est_Latitud == latitud &&
                    est.Est_Longitud == longitud)
                {
                    return false;
                }

                // ---------------------------
                // Caso 2: mismo ID pero datos distintos -> verificar duplicados
                // Si existe OTRO estacionamiento con mismos datos, es duplicado.
                // ---------------------------
                if (id.HasValue && est.Est_id == id.Value)
                {
                    bool existeDuplicado = db.Estacionamiento.Any(e =>
                        e.Est_id != id.Value &&
                        ((e.Est_provincia == provincia &&
                          e.Est_localidad == localidad &&
                          e.Est_direccion == direccion)
                         || (e.Est_Latitud == latitud && e.Est_Longitud == longitud))
                    );

                    if (existeDuplicado)
                        return true;
                    else
                        return false;
                }

                // ---------------------------
                // Caso 3: distinto ID con mismos datos -> es duplicado
                // ---------------------------
                if (!id.HasValue || est.Est_id != id.Value)
                {
                    bool existeDuplicado = db.Estacionamiento.Any(e =>
                        (e.Est_provincia == provincia &&
                        e.Est_localidad == localidad &&
                        e.Est_direccion == direccion)
                        || (e.Est_Latitud == latitud && e.Est_Longitud == longitud)
                    );

                    if (existeDuplicado)
                        return true;
                    else
                        return false;
                }
            }

            // No hubo duplicados
            return false;
        }


        protected async void btnGuardar_Click(object sender, EventArgs e)
        {
            LimpiarMensaje();

            // 1. Validar campos obligatorios
            if (!ValidarCamposObligatorios()) return;

            // 2. Validar horario semanal
            if (!ValidarHorario(ddlHoraInicio, ddlHoraFin, "La hora de inicio debe ser menor que la hora de fin.")) return;

            // 3. Validar horario de fin de semana (si corresponde)
            string horaDomingo = null;
            bool domingo = chkDomingo.Checked;
            if (domingo)
            {
                if (!ValidarHorario(ddlHoraInicio_Domingo, ddlHoraFin_Domingo, "La hora de inicio de fin de semana debe ser menor que la hora de fin.")) return;

                horaDomingo = ddlHoraInicio_Domingo.SelectedValue + " - " + ddlHoraFin_Domingo.SelectedValue;
            }

            // 4. Leer datos del formulario
            string nombre = txtNombre.Text.Trim();
            string direccion = txtDireccion.Text.Trim();
            string provincia = ddlProvincia.SelectedValue;
            string localidad = ddlLocalidad.SelectedValue;
            string diasAtencion = ddlDiaInicio.SelectedValue + " a " + ddlDiaFin.SelectedValue;
            string horario = ddlHoraInicio.SelectedValue + " - " + ddlHoraFin.SelectedValue;
            bool diasFeriado = chkDiasFeriado.Checked;
            bool disponibilidad = chkDisponibilidad.Checked;

            // 5. Validar sesión
            if (Session["Usu_legajo"] == null)
            {
                Response.Redirect("~/Pages/Login/Login.aspx");
                return;
            }
            int legajoDueño = (int)Session["Usu_legajo"];

            try
            {
                // 6. Geocodificación
                var servicioGeo = new ServicioGeocodificacion();
                var coordenadas = await servicioGeo.ObtenerCoordenadasAsync(direccion, localidad, provincia);

                if (!coordenadas.EsValida)
                {
                    MostrarError("No se pudo obtener la ubicación geográfica. Verifica la dirección.");
                    return;
                }

                using (var db = new ProyectoEstacionamientoEntities())
                {
                    // 7. Validar duplicados por dirección o coordenadas
                    int? estId = string.IsNullOrEmpty(hdnEstId.Value) ? (int?)null : int.Parse(hdnEstId.Value);
                    bool duplicado = ExisteDuplicado(db, estId, provincia, localidad, direccion, coordenadas.Latitud, coordenadas.Longitud);
                    if (duplicado)
                    {
                        MostrarError("Ya existe un Estacionamiento registrado en esa dirección o ubicación.");
                        return;
                    }

                    // 8. Verificar si es edición o alta nueva
                    int? id = string.IsNullOrEmpty(hdnEstId.Value) ? (int?)null : int.Parse(hdnEstId.Value);
                    string accion;
                    if (id.HasValue)
                    {
                        var est = db.Estacionamiento.Find(id.Value);
                        if (est != null)
                        {
                            est.Est_nombre = nombre;
                            est.Est_provincia = provincia;
                            est.Est_localidad = localidad;
                            est.Est_direccion = direccion;
                            est.Est_Latitud = coordenadas.Latitud;
                            est.Est_Longitud = coordenadas.Longitud;
                            est.Est_Hra_Atencion = horario;
                            est.Est_Dias_Atencion = diasAtencion;
                            est.Est_Dias_Feriado_Atencion = diasFeriado;
                            est.Est_Fin_de_semana_Atencion = domingo;
                            est.Est_Hora_Fin_de_semana = horaDomingo;
                            est.Est_Disponibilidad = disponibilidad;

                            accion = "editado";
                        }
                        else
                        {
                            MostrarError("No se encontró el estacionamiento a actualizar.");
                            return;
                        }
                    }
                    else
                    {
                        var nuevoEstacionamiento = new Proyecto_Estacionamiento.Estacionamiento
                        {
                            Dueño_Legajo = legajoDueño,
                            Est_nombre = nombre,
                            Est_provincia = provincia,
                            Est_localidad = localidad,
                            Est_direccion = direccion,
                            Est_Latitud = coordenadas.Latitud,
                            Est_Longitud = coordenadas.Longitud,
                            Est_Hra_Atencion = horario,
                            Est_puntaje = 0,
                            Est_Dias_Atencion = diasAtencion,
                            Est_Fin_de_semana_Atencion = domingo,
                            Est_Hora_Fin_de_semana = horaDomingo,
                            Est_Dias_Feriado_Atencion = diasFeriado,
                            Est_Disponibilidad = disponibilidad
                        };
                        db.Estacionamiento.Add(nuevoEstacionamiento);
                        accion = "agregado";
                    }

                    db.SaveChanges();
                    Response.Redirect($"Estacionamiento_Listar.aspx?exito=1&accion={accion}");
                }
            }
            catch (Exception ex)
            {
                MostrarError("Error al guardar: " + ex.Message);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Estacionamiento/Estacionamiento_Listar.aspx");
        }

        private void CargarEstacionamiento(int id)
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                var est = db.Estacionamiento.Find(id);
                if (est != null)
                {

                    txtNombre.Text = est.Est_nombre;
                    txtDireccion.Text = est.Est_direccion;
                    ddlProvincia.SelectedValue = est.Est_provincia;

                    // Cargar las localidades
                    var localidades = Task.Run(() => servicioProvinciasLocalidades.ObtenerLocalidadesAsync(est.Est_provincia)).Result;
                    ddlLocalidad.DataSource = localidades;
                    ddlLocalidad.DataBind();
                    ddlLocalidad.Items.Insert(0, new ListItem("- Seleccione una localidad -", ""));
                    ddlLocalidad.SelectedValue = est.Est_localidad;

                    // Horario (Inicio - Fin)
                    var horarioSplit = est.Est_Hra_Atencion.Split('-');
                    ddlHoraInicio.SelectedValue = horarioSplit[0].Trim();
                    ddlHoraFin.SelectedValue = horarioSplit[1].Trim();

                    // Días de Atención (Inicio - Fin)
                    var diasSplit = est.Est_Dias_Atencion.Split('a');
                    ddlDiaInicio.SelectedValue = diasSplit[0].Trim();
                    ddlDiaFin.SelectedValue = diasSplit[1].Trim();

                    // Booleanos
                    chkDiasFeriado.Checked = est.Est_Dias_Feriado_Atencion ?? false;
                    chkDomingo.Checked = est.Est_Fin_de_semana_Atencion ?? false;
                    chkDisponibilidad.Checked = est.Est_Disponibilidad;

                    // Horario Domingo (Inicio - Fin)
                    if (!string.IsNullOrEmpty(est.Est_Hora_Fin_de_semana))
                    {
                        var findeSplit = est.Est_Hora_Fin_de_semana.Split('-');
                        if (findeSplit.Length == 2)
                        {
                            ddlHoraInicio_Domingo.SelectedValue = findeSplit[0].Trim();
                            ddlHoraFin_Domingo.SelectedValue = findeSplit[1].Trim();
                        }
                    }

                    ViewState["Est_id"] = est.Est_id; // Guardamos el ID
                    ToggleHoraControls(chkDomingo.Checked);
                }
            }
        }

    }
}