using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Default
{
    public partial class Ingreso_Registrar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // VALIDACIÓN DE SEGURIDAD
            // Si no hay usuario logueado, lo mandamos al Login inmediatamente.
            if (Session["Usu_legajo"] == null || Session["Usu_tipo"] == null)
            {
                Response.Redirect("~/Pages/Login/Login.aspx");
                return; // Detenemos la ejecución para que no siga cargando y falle
            }

            if (!IsPostBack)
            {
                string estacionamiento = Session["Usu_estacionamiento"] as string;

                if (!string.IsNullOrEmpty(estacionamiento))
                {
                    TituloRegistroIngresos.Text = $"Registrar Ingreso en el Estacionamiento '<strong>{estacionamiento}</strong>'";
                }
                else
                {
                    TituloRegistroIngresos.Text = "Registrar Ingreso";
                }

                CargarCategoriasFiltradas();
                ddlPlaza.Enabled = false;
                ddlTarifa.Enabled = false;
            }
        }

        private int? ObtenerEstacionamientoId()
        {
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                // Buscamos al Playero
                var playero = db.Playero.FirstOrDefault(p => p.Usuarios.Usu_legajo == legajo);

                // Obtenemos el Estacionamiento donde esta asignado el Playero
                int estacionamientoId = (int)playero.Est_id;

                return estacionamientoId;
            }
        }

        // Metodo para autocompletar los campos del Vehículo si se ingresa una patente existente

        protected void TxtPatente_TextChanged(object sender, EventArgs e)
        {
            string patenteIngresada = txtPatente.Text.Trim().Replace(" ", "").ToUpper();
            ResetearCampos();

            if (string.IsNullOrWhiteSpace(patenteIngresada)) return;

            using (var db = new ProyectoEstacionamientoEntities())
            {
                int? estId = ObtenerEstacionamientoId();

                // 2. Buscar si la patente tiene un ABONO VIGENTE
                var abonoVehiculo = db.Vehiculo_Abonado
                    .Include("Abono")
                    .Include("Abono.Plaza")
                    .Include("Abono.Pagos_Abonados")              
                    .Include("Abono.Pagos_Abonados.Tarifa")       
                    .Include("Abono.Pagos_Abonados.Tarifa.Tipos_Tarifa") 
                    .Include("Vehiculo")
                    .Include("Vehiculo.Categoria_Vehiculo")
                    .FirstOrDefault(va =>
                        va.Vehiculo_Patente == patenteIngresada &&
                        va.Abono.Est_id == estId &&
                        va.Abono.Fecha_Vto >= DateTime.Now
                    );

                if (abonoVehiculo != null)
                {
                    var plazaDelAbono = abonoVehiculo.Abono.Plaza;
                    var categoriaDelAbono = abonoVehiculo.Vehiculo.Categoria_Vehiculo;

                    
                    var ultimoPago = abonoVehiculo.Abono.Pagos_Abonados
                                        .OrderByDescending(p => p.Fecha_Pago)
                                        .FirstOrDefault();

                    // Si por alguna razón no hay pagos (raro), no podemos determinar la tarifa
                    if (ultimoPago == null) return;

                    var tarifaDelAbono = ultimoPago.Tarifa;
                    

                    if (!plazaDelAbono.Plaza_Disponibilidad)
                    {
                        string script = $"Swal.fire({{icon: 'error', title: 'Plaza Ocupada', text: 'La plaza {plazaDelAbono.Plaza_Nombre} asignada a este abono ya se encuentra ocupada. Se registrará como un ingreso normal.'}});";
                        ScriptManager.RegisterStartupScript(this, GetType(), "alertPlazaOcupada", script, true);
                        ddlCategoria.SelectedValue = categoriaDelAbono.Categoria_id.ToString();
                        ddlCategoria_SelectedIndexChanged(null, null);
                        return;
                    }

                    // Autocompletar
                    ddlCategoria.SelectedValue = categoriaDelAbono.Categoria_id.ToString();
                    ddlCategoria_SelectedIndexChanged(null, null);

                    // Plaza
                    string plazaIdDelAbonoStr = plazaDelAbono.Plaza_id.ToString();
                    if (ddlPlaza.Items.FindByValue(plazaIdDelAbonoStr) == null)
                    {
                        ddlPlaza.Items.Add(new ListItem(plazaDelAbono.Plaza_Nombre, plazaIdDelAbonoStr));
                    }
                    ddlPlaza.SelectedValue = plazaIdDelAbonoStr;

                    // Tarifa (Usando tarifaDelAbono recuperada del pago)
                    string tarifaIdAbonoStr = tarifaDelAbono.Tarifa_id.ToString();
                    string tarifaDescAbono = tarifaDelAbono.Tipos_Tarifa.Tipos_tarifa_descripcion;

                    if (ddlTarifa.Items.FindByValue(tarifaIdAbonoStr) == null)
                    {
                        ddlTarifa.Items.Add(new ListItem(tarifaDescAbono, tarifaIdAbonoStr));
                    }
                    ddlTarifa.SelectedValue = tarifaIdAbonoStr;

                    // Visual
                    cvTarifa.Enabled = false;
                    pnlDetalleTarifa.Visible = true;
                    litDescripcionTarifa.Text = $"Este vehículo es beneficiario de un Abono <b>'{tarifaDescAbono}'</b>";
                    litMontoTarifa.Text = "Abonado";
                    imgCategoria.ImageUrl = ObtenerIconoPorCategoria(categoriaDelAbono.Categoria_descripcion);

                    ddlCategoria.Enabled = false;
                    ddlPlaza.Enabled = false;
                    ddlTarifa.Enabled = false;
                }
                else
                {
                    var vehiculo = db.Vehiculo.FirstOrDefault(v => v.Vehiculo_Patente == patenteIngresada);
                    if (vehiculo != null)
                    {
                        ddlCategoria.SelectedValue = vehiculo.Categoria_id.ToString();
                        ddlCategoria.Enabled = false;
                        ddlCategoria_SelectedIndexChanged(null, null);
                    }
                }
            }
        }

        private void ResetearCampos()
        {
            // Habilitar controles
            ddlCategoria.Enabled = true;
            ddlPlaza.Enabled = true;
            ddlTarifa.Enabled = true;
            ddlTarifa.Visible = true;
            cvTarifa.Enabled = true;

            // Limpiar selecciones
            ddlCategoria.SelectedValue = "0";
            LimpiarDropDown(ddlPlaza, "--Seleccione Plaza--");
            LimpiarDropDown(ddlTarifa, "-- Seleccione Tarifa --");

            // Ocultar panel de detalles
            pnlDetalleTarifa.Visible = false;
            litDescripcionTarifa.Text = "";
            litMontoTarifa.Text = "";
        }


        private void CargarCategoriasFiltradas()
        {

            using (var db = new ProyectoEstacionamientoEntities())
            {
                // Obtenemos el Estacionamiento
                int? estacionamientoId = ObtenerEstacionamientoId();

                // Obtener Plazas del Estacionamiento
                var plazas = db.Plaza.Where(plaza => plaza.Est_id == estacionamientoId);

                // Obtener ids de Categorías que admiten esas Plazas
                var categoriasIds = plazas
                                    .Select(plaza => plaza.Categoria_id)
                                    .Distinct()
                                    .ToList();

                // Obtener Categorías
                var categorias = db.Categoria_Vehiculo
                                   .Where(cat => categoriasIds.Contains(cat.Categoria_id))
                                   .OrderBy(cat => cat.Categoria_descripcion)
                                   .ToList();

                // Asignar al DropDownList
                ddlCategoria.DataSource = categorias;
                ddlCategoria.DataTextField = "Categoria_descripcion";
                ddlCategoria.DataValueField = "Categoria_id";
                ddlCategoria.DataBind();
                ddlCategoria.Items.Insert(0, new System.Web.UI.WebControls.ListItem("--Seleccione Categoría--", "0"));
            }
        }

        private void LimpiarDropDown(DropDownList ddl, string textoDefault)
        {
            ddl.Items.Clear();
            ddl.Items.Insert(0, new ListItem(textoDefault, "0"));
            ddl.Enabled = false;
        }

        // Usamos el id del Estacionamiento y el de la Categoría seleccionada para filtrar las Plazas y Tarifas
        protected void ddlCategoria_SelectedIndexChanged(object sender, EventArgs e)
        {
            int? estacionamientoId = ObtenerEstacionamientoId();
            int categoriaSeleccionadaId = Convert.ToInt32(ddlCategoria.SelectedValue);

            // Filtrar Plazas
            CargarPlazasFiltradas(estacionamientoId, categoriaSeleccionadaId);
            // Filtrar Tarifas
            CargarTarifasFiltradas(estacionamientoId, categoriaSeleccionadaId);
        }


        protected void CargarPlazasFiltradas(int? estacionamientoId, int categoriaSeleccionadaId)
        {
            if (categoriaSeleccionadaId == 0)   // Si no se seleccionó Categoría no se habilitan las Plazas
            {
                LimpiarDropDown(ddlPlaza, "--Seleccione Plaza--");
                return;
            }

            using (var db = new ProyectoEstacionamientoEntities())
            {
                var ahora = DateTime.Now;

                var plazas = db.Plaza
                               .Where(p => p.Est_id == estacionamientoId
                                    && p.Categoria_id == categoriaSeleccionadaId
                                    && p.Plaza_Disponibilidad == true
                                    && !p.Abono.Any(a => a.Fecha_Vto >= ahora)
                                    )
                               .OrderBy(p => p.Plaza_id)
                               .ToList();

                if (plazas.Any())
                {
                    ddlPlaza.DataSource = plazas;
                    ddlPlaza.DataTextField = "Plaza_Nombre";
                    ddlPlaza.DataValueField = "Plaza_id";
                    ddlPlaza.DataBind();
                    ddlPlaza.Items.Insert(0, new ListItem("--Seleccione Plaza--", "0"));
                    ddlPlaza.Enabled = true;
                }
                else
                {
                    LimpiarDropDown(ddlPlaza, "--Seleccione Plaza--");
                }
            }
        }

        protected void CargarTarifasFiltradas(int? estacionamientoId, int categoriaSeleccionadaId)
        {
            if (categoriaSeleccionadaId == 0)
            {
                LimpiarDropDown(ddlTarifa, "-- Seleccione Tarifa --");
                return;
            }

            using (var db = new ProyectoEstacionamientoEntities())
            {
                // 1. Definimos los únicos IDs de Tipos_Tarifa que queremos mostrar
                var idsPermitidos = new List<int> { 1, 2, 6 }; // 1:Por hora, 2:Por día, 6:Quincenal

                // 2. Traemos la lista de la BD, filtrando por los IDs permitidos
                var tarifas = db.Tarifa
                    .Where(t => t.Est_id == estacionamientoId &&
                                t.Categoria_id == categoriaSeleccionadaId &&
                                idsPermitidos.Contains(t.Tipos_Tarifa.Tipos_tarifa_id))
                    .Select(t => new
                    {
                        t.Tarifa_id,
                        Descripcion = t.Tipos_Tarifa != null ? t.Tipos_Tarifa.Tipos_tarifa_descripcion : "(Sin descripción)",
                        t.Tarifa_Monto,
                        t.Tipos_Tarifa.Tipos_tarifa_id
                    })
                    .ToList(); // Traemos la lista filtrada a memoria

                // 3. Definimos el orden lógico (ahora solo con los IDs permitidos)
                var ordenLogicoIDs = new List<int> { 1, 2, 6 };

                // 4. Reordenamos la lista en memoria
                var tarifasOrdenadas = tarifas
                    .OrderBy(t => ordenLogicoIDs.IndexOf(t.Tipos_tarifa_id))
                    .ToList();

                // 5. Continuamos con la lógica para poblar el dropdown
                if (tarifasOrdenadas.Any())
                {
                    ddlTarifa.DataSource = tarifasOrdenadas;
                    ddlTarifa.DataTextField = "Descripcion";
                    ddlTarifa.DataValueField = "Tarifa_id";
                    ddlTarifa.DataBind();
                    ddlTarifa.Items.Insert(0, new ListItem("-- Seleccione Tarifa --", "0"));
                    ddlTarifa.Enabled = true;
                }
                else
                {
                    LimpiarDropDown(ddlTarifa, "-- Seleccione Tarifa --");
                }
            }
        }

        protected void ddlTarifa_SelectedIndexChanged(object sender, EventArgs e)
        {
            int tarifaId = int.Parse(ddlTarifa.SelectedValue);
            int categoriaId = int.Parse(ddlCategoria.SelectedValue);

            int estacionamientoId = 0;
            if (Session["Playero_EstId"] != null)
                estacionamientoId = (int)Session["Playero_EstId"];
            else if (Session["Dueño_EstId"] != null)
                estacionamientoId = (int)Session["Dueño_EstId"];

            if (tarifaId > 0)
            {
                using (var db = new ProyectoEstacionamientoEntities())
                {
                    var tarifa = db.Tarifa
                        .Where(t => t.Tarifa_id == tarifaId)
                        .Select(t => new
                        {
                            t.Tarifa_id,
                            Categoria = t.Categoria_Vehiculo.Categoria_descripcion,
                            TipoTarifa = t.Tipos_Tarifa.Tipos_tarifa_descripcion,
                            t.Tarifa_Monto
                        })
                        .FirstOrDefault();

                    if (tarifa != null)
                    {
                        // Texto dinámico
                        litDescripcionTarifa.Text = $"La tarifa para <strong>{tarifa.Categoria}</strong> - {tarifa.TipoTarifa}";

                        // Monto
                        litMontoTarifa.Text = tarifa.Tarifa_Monto.ToString("C");

                        // Imagen según categoría (usamos el helper)
                        imgCategoria.ImageUrl = ObtenerIconoPorCategoria(tarifa.Categoria);

                        pnlDetalleTarifa.Visible = true;
                    }
                }
            }
            else
            {
                pnlDetalleTarifa.Visible = false;
            }
        }

        private string ObtenerIconoPorCategoria(string categoria)
        {
            var mapa = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Automóviles y Camionetas", "Automóviles y Camionetas.png" },
                { "Motocicletas", "Motocicletas.png" },
                { "Camiones", "Camiones.png" },
                { "Ómnibus y Minibuses", "Ómnibus y Minibuses.png" },
                { "Vehículos agrícolas o especiales", "Vehículos agrícolas.png" }, // diferencia aquí
                { "Casas rodantes y Motorhomes", "Casas rodantes y Motorhomes.png" },
                { "Remolques y acoplados", "Remolques y acoplados.png" },
                { "Vehículo para personas con discapacidad", "Vehículo para personas con discapacidad.png" }
            };

            if (mapa.ContainsKey(categoria))
                return "~/Images/" + mapa[categoria];
            else
                return "~/Images/EnConstruccion.png"; // opcional: imagen por defecto
        }


        // Validaciones
        protected void cvPatente_ServerValidate(object source, ServerValidateEventArgs args)
        {
            string patenteIngresada = txtPatente.Text?.Trim().ToUpper().Replace(" ", "");

            // Validar que se haya ingresado una Patente
            if (string.IsNullOrWhiteSpace(patenteIngresada))
            {
                args.IsValid = false;
                cvPatente.ErrorMessage = "La Patente no puede estar vacía.";
                return;
            }

            using (var db = new ProyectoEstacionamientoEntities())
            {
                // Validar ocupación activa
                var ocupacionActiva = db.Ocupacion
                    .FirstOrDefault(o => o.Vehiculo_Patente.Replace(" ", "").ToUpper() == patenteIngresada
                                       && o.Ocu_fecha_Hora_Fin == null);

                if (ocupacionActiva != null)
                {
                    args.IsValid = false;
                    cvPatente.ErrorMessage = $"El vehículo con patente {txtPatente.Text} ya se encuentra dentro del estacionamiento.";
                    return; // Detener ejecución antes de modificar la BD
                }
            }

            args.IsValid = true;
        }


        protected void cvCategoria_ServerValidate(object source, ServerValidateEventArgs args)
        {
            // Validar que se haya seleccionado una Categoría
            if (ddlCategoria.SelectedValue == "0")
            {
                args.IsValid = false;
                cvCategoria.ErrorMessage = "Debe seleccionar una Categoría.";
                return;
            }
        }

        protected void cvPlaza_ServerValidate(object source, ServerValidateEventArgs args)
        {
            // Validar que se haya seleccionado una plaza
            if (ddlPlaza.SelectedValue == "0")
            {
                args.IsValid = false;
                cvPlaza.ErrorMessage = "Debe seleccionar una Plaza.";
                return;
            }
        }


        protected void cvTarifa_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = ddlTarifa.SelectedValue != "0";
            if (!args.IsValid) cvTarifa.ErrorMessage = "Debe seleccionar una Tarifa.";
        }


        // Guardar
        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            // Validar todos los CustomValidators del grupo "Ingreso"
            Page.Validate("Ingreso");
            if (!Page.IsValid) return;

            var patenteIngresada = txtPatente.Text.Trim().Replace(" ", "").ToUpper();

            using (var db = new ProyectoEstacionamientoEntities())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Validar existencia de Vehículo
                        var vehiculoExistente = db.Vehiculo
                            .FirstOrDefault(v => v.Vehiculo_Patente.Replace(" ", "").ToUpper() == patenteIngresada);

                        if (vehiculoExistente == null)
                        {
                            var nuevoVehiculo = new Vehiculo
                            {
                                Vehiculo_Patente = patenteIngresada,
                                Categoria_id = int.Parse(ddlCategoria.SelectedValue),
                            };
                            db.Vehiculo.Add(nuevoVehiculo);
                            db.SaveChanges();
                        }

                        //  Obtener datos necesarios para Pago y Ocupación
                        int? estId = ObtenerEstacionamientoId();

                        int tarifaId = int.Parse(ddlTarifa.SelectedValue);
                        var tarifa = db.Tarifa.FirstOrDefault(t => t.Tarifa_id == tarifaId);

                        int plazaIdSeleccionada = int.Parse(ddlPlaza.SelectedValue);
                        var plaza = db.Plaza.FirstOrDefault(p => p.Est_id == estId && p.Plaza_id == plazaIdSeleccionada);

                        // Cambiar disponibilidad de Plaza
                        plaza.Plaza_Disponibilidad = false;
                        db.SaveChanges();

                        // Crear Ocupacion
                        var nuevaOcupacion = new Ocupacion
                        {
                            Est_id = (int)estId,
                            Plaza_id = plazaIdSeleccionada,
                            Ocu_fecha_Hora_Inicio = DateTime.Now,
                            Vehiculo_Patente = patenteIngresada,
                            Tarifa_id = tarifaId,
                            Pago_id = null   // Se asignará en el Egreso
                        };
                        db.Ocupacion.Add(nuevaOcupacion);
                        db.SaveChanges();

                        // Confirmar transacción
                        transaction.Commit();

                        // Redirigir después de confirmar
                        Response.Redirect($"~/Pages/Ingresos/Ingreso_Listar.aspx?exito=1&accion=ingreso");
                    }
                    catch (Exception ex)
                    {
                        // Solo rollback si la transacción está activa y la conexión válida
                        if (db.Database.CurrentTransaction != null)
                            db.Database.CurrentTransaction.Rollback();

                        // Opcional: mostrar error en algún CustomValidator o log
                        cvPlaza.ErrorMessage = "Error al guardar el ingreso: " + ex.Message;
                        cvPlaza.IsValid = false;
                    }
                }
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Ingresos/Ingreso_Listar.aspx");
        }

    }
}