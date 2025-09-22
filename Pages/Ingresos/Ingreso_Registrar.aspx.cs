using Antlr.Runtime.Misc;
using System;
using System.Linq;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Default
{
    public partial class Ingreso_Registrar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
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
                ddlTarifa.Enabled = false;            }
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
        protected void txtPatente_TextChanged(object sender, EventArgs e)
        {
            string patenteIngresada = txtPatente.Text.Trim().Replace(" ", "").ToUpper();

            using (var db = new ProyectoEstacionamientoEntities())
            {
                var vehiculo = db.Vehiculo
                                 .FirstOrDefault(v => v.Vehiculo_Patente.Replace(" ", "").ToUpper() == patenteIngresada);

                if (vehiculo != null)
                {
                    // Autocompletamos los campos
                    ddlCategoria.SelectedValue = vehiculo.Categoria_id.ToString();

                    // Bloqueamos edición de los campos
                    ddlCategoria.Enabled = false;

                    // Filtrar automáticamente Plazas y Tarifas según categoría
                    ddlCategoria_SelectedIndexChanged(null, null);
                }
                else
                {
                    // Si la patente se borra o es nueva, desbloqueamos campos y limpiamos
                    ddlCategoria.Enabled = true;

                    ddlCategoria.SelectedValue = "0";
                }
            }
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
                var plazas = db.Plaza
                               .Where(p => p.Est_id == estacionamientoId
                                    && p.Categoria_id == categoriaSeleccionadaId
                                    && p.Plaza_Disponibilidad == true)
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
                LimpiarDropDown(ddlTarifa, "--Seleccione Tarifa--");
                return;
            }

            using (var db = new ProyectoEstacionamientoEntities())
            {
                var tarifas = db.Tarifa
                    .Where(t => t.Est_id == estacionamientoId && t.Categoria_id == categoriaSeleccionadaId)
                    .Select(t => new
                    {
                        t.Tarifa_id,
                        Descripcion = t.Tipos_Tarifa != null ? t.Tipos_Tarifa.Tipos_tarifa_descripcion : "(Sin descripción)",
                        t.Tarifa_Monto
                    })
                    .OrderBy(t => t.Descripcion)
                    .ToList();

                if (tarifas.Any())
                {
                    ddlTarifa.DataSource = tarifas;
                    ddlTarifa.DataTextField = "Descripcion";
                    ddlTarifa.DataValueField = "Tarifa_id";
                    ddlTarifa.DataBind();
                    ddlTarifa.Items.Insert(0, new ListItem("--Seleccione Tarifa--", "0"));
                    ddlTarifa.Enabled = true;
                }
                else
                {
                    LimpiarDropDown(ddlTarifa, "--Seleccione Tarifa--");
                }
            }
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
                // 🔍 Validar ocupación activa
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
            // 1️⃣ Validar todos los CustomValidators del grupo "Ingreso"
            Page.Validate("Ingreso");
            if (!Page.IsValid) return;

            var patenteIngresada = txtPatente.Text.Trim().Replace(" ", "").ToUpper();

            using (var db = new ProyectoEstacionamientoEntities())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // 🔍 Validar existencia de Vehículo
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

                        // 🔍 Obtener datos necesarios para Pago y Ocupación
                        int? estId = ObtenerEstacionamientoId();

                        int tarifaId = int.Parse(ddlTarifa.SelectedValue);
                        var tarifa = db.Tarifa.FirstOrDefault(t => t.Tarifa_id == tarifaId);

                        int plazaIdSeleccionada = int.Parse(ddlPlaza.SelectedValue);
                        var plaza = db.Plaza.FirstOrDefault(p => p.Est_id == estId && p.Plaza_id == plazaIdSeleccionada);

                        // 🔹 Cambiar disponibilidad de Plaza
                        plaza.Plaza_Disponibilidad = false;
                        db.SaveChanges();

                        // 🔹 Crear Ocupacion
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

                        // 🔹 Confirmar transacción
                        transaction.Commit();

                        // 🔹 Redirigir después de confirmar
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