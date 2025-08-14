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
                CargarCategoriasFiltradas();
                ddlPlaza.Enabled = false;
                ddlTarifa.Enabled = false;
                CargarMetodosDePagoFiltrados();
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
                    txtMarca.Text = vehiculo.Vehiculo_Marca;
                    txtModelo.Text = vehiculo.Vehiculo_Modelo.ToString();
                    ddlColor.SelectedValue = vehiculo.Vehiculo_Color;

                    // Bloqueamos edición de los campos
                    ddlCategoria.Enabled = false;
                    txtMarca.ReadOnly = true;
                    txtModelo.ReadOnly = true;
                    ddlColor.Enabled = false;

                    // Filtrar automáticamente Plazas y Tarifas según categoría
                    ddlCategoria_SelectedIndexChanged(null, null);
                }
                else
                {
                    // Si la patente se borra o es nueva, desbloqueamos campos y limpiamos
                    ddlCategoria.Enabled = true;
                    txtMarca.ReadOnly = false;
                    txtModelo.ReadOnly = false;
                    ddlColor.Enabled = true;

                    ddlCategoria.SelectedValue = "0";
                    txtMarca.Text = "";
                    txtModelo.Text = "";
                    ddlColor.SelectedValue = "0";
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


        private void CargarMetodosDePagoFiltrados()
        {

            using (var db = new ProyectoEstacionamientoEntities())
            {

                // Obtenemos el Estacionamiento
                int? estacionamientoId = ObtenerEstacionamientoId();

                // Filtramos Métodos de Pago aceptados por ese estacionamiento
                var metodosPago = db.Acepta_Metodo_De_Pago
                    .Where(a => a.Est_id == estacionamientoId)
                    .Select(a => a.Metodos_De_Pago)
                    .Distinct()
                    .ToList();
                if (metodosPago.Any())
                {
                    // Vinculamos al DropDownList
                    ddlMetodoDePago.DataSource = metodosPago;
                    ddlMetodoDePago.DataTextField = "Metodo_pago_descripcion";
                    ddlMetodoDePago.DataValueField = "Metodo_pago_id";
                    ddlMetodoDePago.DataBind();
                    ddlMetodoDePago.Items.Insert(0, new ListItem("--Seleccione Método de Pago--", "0"));
                }
                else
                {
                    LimpiarDropDown(ddlMetodoDePago, "--Seleccione Método de Pago--");
                }
            }
        }


        // Validaciones
        private bool ValidarFormulario()
        {
            // Validar que los campos de texto no estén vacíos
            if (string.IsNullOrWhiteSpace(txtPatente.Text))
            {
                lblMensaje.Text = "La Patente no puede estar vacía.";
                lblMensaje.ForeColor = System.Drawing.Color.Red;
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtMarca.Text))
            {
                lblMensaje.Text = "La Marca no puede estar vacía.";
                lblMensaje.ForeColor = System.Drawing.Color.Red;
                return false;
            }

            if (!int.TryParse(txtModelo.Text.Trim(), out int modelo) || modelo < 1900 || modelo > DateTime.Now.Year)
            {
                lblMensaje.Text = "El Modelo debe ser un número válido entre 1900 y el año actual.";
                lblMensaje.ForeColor = System.Drawing.Color.Red;
                return false;
            }

            if (ddlCategoria.SelectedValue == "0")
            {
                lblMensaje.Text = "Debe seleccionar una Categoría.";
                lblMensaje.ForeColor = System.Drawing.Color.Red;
                return false;
            }

            if (ddlColor.SelectedValue == "0")
            {
                lblMensaje.Text = "Debe seleccionar un Color.";
                lblMensaje.ForeColor = System.Drawing.Color.Red;
                return false;
            }

            if (ddlPlaza.SelectedValue == "0")
            {
                lblMensaje.Text = "Debe seleccionar una Plaza.";
                lblMensaje.ForeColor = System.Drawing.Color.Red;
                return false;
            }

            if (ddlTarifa.SelectedValue == "0")
            {
                lblMensaje.Text = "Debe seleccionar una Tarifa.";
                lblMensaje.ForeColor = System.Drawing.Color.Red;
                return false;
            }

            if (ddlMetodoDePago.SelectedValue == "0")
            {
                lblMensaje.Text = "Debe seleccionar un Método de Pago.";
                lblMensaje.ForeColor = System.Drawing.Color.Red;
                return false;
            }

            // Si todo está correcto
            lblMensaje.Text = ""; // Limpia el mensaje
            return true;
        }


        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario())
                return;

            try
            {
                using (var db = new ProyectoEstacionamientoEntities())
                {
                    // 🔍 Normalizamos la patente ingresada ingreso "a" y la convertimos en "A"
                    var patenteIngresada = txtPatente.Text.Trim().Replace(" ", "").ToUpper();

                    var ocupacionActiva = db.Ocupacion
                        .FirstOrDefault(o => o.Vehiculo_Patente.Replace(" ", "").ToUpper() == patenteIngresada
                                           && o.Ocu_fecha_Hora_Fin == null);

                    if (ocupacionActiva != null)
                    {
                        lblMensaje.Text = $"El vehículo con patente {patenteIngresada} ya se encuentra dentro del estacionamiento.";
                        return;
                    }

                    using (var transaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            // Verificar si el vehículo ya existe
                            var vehiculoExistente = db.Vehiculo
                                .FirstOrDefault(v => v.Vehiculo_Patente.Replace(" ", "").ToUpper() == patenteIngresada);

                            if (vehiculoExistente == null)
                            {
                                var nuevoVehiculo = new Vehiculo
                                {
                                    Vehiculo_Patente = patenteIngresada,
                                    Categoria_id = int.Parse(ddlCategoria.SelectedValue),
                                    Vehiculo_Marca = txtMarca.Text.Trim(),
                                    Vehiculo_Modelo = int.Parse(txtModelo.Text.Trim()),
                                    Vehiculo_Color = ddlColor.SelectedValue
                                };

                                db.Vehiculo.Add(nuevoVehiculo);
                                db.SaveChanges();
                            }

                            // Crear Pago_Ocupacion
                            int? estId = ObtenerEstacionamientoId();
                            int metodoPagoId = Convert.ToInt32(ddlMetodoDePago.SelectedValue);
                            int tarifaId = Convert.ToInt32(ddlTarifa.SelectedValue);
                            var tarifa = db.Tarifa.FirstOrDefault(t => t.Tarifa_id == tarifaId);
                            double importe = tarifa.Tarifa_Monto;

                            var nuevoPago = new Pago_Ocupacion
                            {
                                Est_id = (int)estId,
                                Metodo_Pago_id = metodoPagoId,
                                Pago_Importe = importe,
                                Pago_Fecha = null
                            };

                            db.Pago_Ocupacion.Add(nuevoPago);
                            db.SaveChanges();
                            int nuevoPago_id = nuevoPago.Pago_id;

                            // Cambiar disponibilidad de Plaza
                            int plazaIdSeleccionada = int.Parse(ddlPlaza.SelectedValue);
                            var plaza = db.Plaza.FirstOrDefault(p => p.Est_id == estId && p.Plaza_id == plazaIdSeleccionada);
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
                                Pago_id = nuevoPago_id
                            };

                            db.Ocupacion.Add(nuevaOcupacion);
                            db.SaveChanges();

                            // Confirmar
                            transaction.Commit();
                            Response.Redirect("~/Pages/Default/Ingreso_Listar.aspx");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            lblMensaje.Text = "Error al guardar: " + ex.Message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "Error de conexión o transacción: " + ex.Message;
            }
        }


        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Default/Ingreso_Listar.aspx");
        }

    }
}