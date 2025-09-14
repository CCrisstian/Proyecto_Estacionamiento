using Proyecto_Estacionamiento.Servicios;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;

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
                else {
                    // Modo Alta
                    chkDisponibilidad.Checked = true;  // marcado por defecto
                    chkDisponibilidad.Visible = false; // no se muestra
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

        // Campos obligatorios
        protected void cvNombre_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !string.IsNullOrWhiteSpace(txtNombre.Text);
        }
        protected void cvProvincia_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = ddlProvincia.SelectedIndex != 0;
        }

        protected void cvLocalidad_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = ddlLocalidad.SelectedIndex != 0;
        }
        protected void cvDireccion_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !string.IsNullOrWhiteSpace(txtDireccion.Text);
        }

        // Horarios
        protected void cvHorario_ServerValidate(object source, ServerValidateEventArgs args)
        {
            int horaInicio = int.Parse(ddlHoraInicio.SelectedValue.Substring(0, 2));
            int horaFin = int.Parse(ddlHoraFin.SelectedValue.Substring(0, 2));
            args.IsValid = horaInicio < horaFin;
        }

        protected void cvHorarioDomingo_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (!chkDomingo.Checked)
            {
                args.IsValid = true;
                return;
            }
            int horaInicio = int.Parse(ddlHoraInicio_Domingo.SelectedValue.Substring(0, 2));
            int horaFin = int.Parse(ddlHoraFin_Domingo.SelectedValue.Substring(0, 2));
            args.IsValid = horaInicio < horaFin;
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

            // 1. Validaciones
            Page.Validate("Estacionamiento");
            if (!Page.IsValid) return;

            // 2. Leer datos del formulario
            string nombre = txtNombre.Text.Trim();
            string direccion = txtDireccion.Text.Trim();
            string provincia = ddlProvincia.SelectedValue;
            string localidad = ddlLocalidad.SelectedValue;
            string diasAtencion = ddlDiaInicio.SelectedValue + " a " + ddlDiaFin.SelectedValue;
            string horario = ddlHoraInicio.SelectedValue + " - " + ddlHoraFin.SelectedValue;
            bool diasFeriado = chkDiasFeriado.Checked;
            bool disponibilidad = chkDisponibilidad.Checked;
            bool domingo = chkDomingo.Checked;
            string horaDomingo = domingo ? ddlHoraInicio_Domingo.SelectedValue + " - " + ddlHoraFin_Domingo.SelectedValue : null;

            // 3. Validar sesión
            if (Session["Usu_legajo"] == null)
            {
                Response.Redirect("~/Pages/Login/Login.aspx");
                return;
            }
            int legajoDueño = (int)Session["Usu_legajo"];

            try
            {
                // 4. Geocodificación
                var servicioGeo = new ServicioGeocodificacion();
                var coordenadas = await servicioGeo.ObtenerCoordenadasAsync(direccion, localidad, provincia);

                if (!coordenadas.EsValida)
                {
                    cvDireccion.ErrorMessage = "No se pudo obtener la ubicación geográfica. Verifica la Dirección.";
                    cvDireccion.IsValid = false;
                    return;
                }

                using (var db = new ProyectoEstacionamientoEntities())
                {
                    // 5. Validar duplicados por dirección o coordenadas
                    int? estId = string.IsNullOrEmpty(hdnEstId.Value) ? (int?)null : int.Parse(hdnEstId.Value);
                    bool duplicado = ExisteDuplicado(db, estId, provincia, localidad, direccion, coordenadas.Latitud, coordenadas.Longitud);
                    if (duplicado)
                    {
                        MostrarError("Ya existe un Estacionamiento registrado en esa dirección o ubicación.");
                        return;
                    }

                    // 6. Verificar si es edición o alta nueva
                    int? id = string.IsNullOrEmpty(hdnEstId.Value) ? (int?)null : int.Parse(hdnEstId.Value);
                    string accion = "";
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

                    // 🔍 Verificar si es el primer estacionamiento del Dueño
                    int cantEstacionamientos = db.Estacionamiento
                        .Count(est => est.Dueño_Legajo == legajoDueño);

                    if (cantEstacionamientos == 1)
                    {
                        // Si es el primero, mandarlo a Inicio.aspx con el nombre del Estacionamiento
                        Response.Redirect($"~/Pages/Default/Inicio.aspx?exito=1&accion={accion}&nombre={HttpUtility.UrlEncode(nombre)}");
                    }
                    else
                    {
                        // Caso normal: ir a la lista
                        Response.Redirect($"Estacionamiento_Listar.aspx?exito=1&accion={accion}");
                    }

                }
            }
            catch (Exception ex)
            {
                string mensaje = ex.Message;
                Exception inner = ex.InnerException;

                while (inner != null)
                {
                    mensaje += " | Inner: " + inner.Message;
                    inner = inner.InnerException;
                }

                MostrarError("Error al guardar: " + mensaje);
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
                    var diasSplit = est.Est_Dias_Atencion.Split(new[] { " a " }, StringSplitOptions.None);
                    if (diasSplit.Length == 2)
                    {
                        ddlDiaInicio.SelectedValue = diasSplit[0].Trim();
                        ddlDiaFin.SelectedValue = diasSplit[1].Trim();
                    }

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