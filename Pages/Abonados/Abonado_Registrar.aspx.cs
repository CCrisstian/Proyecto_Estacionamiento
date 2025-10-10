using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Abonados
{
    public partial class Abonado_Registrar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarCategoriasFiltradas();
                ddlTipoAbono.Enabled = false;
                ddlPlaza.Enabled = false;
                CargarMetodosDePagoEnDropDown();
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

        // VALIDACION - CUIL/CUIT
        protected void cvCuilCuit_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var validator = (CustomValidator)source;

            if (string.IsNullOrWhiteSpace(txtCuilCuit.Text))
            {
                validator.ErrorMessage = "El CUIL / CUIT es obligatorio.";
                args.IsValid = false;
                return;
            }

            if (!long.TryParse(txtCuilCuit.Text, out _))
            {
                validator.ErrorMessage = "El CUIL / CUIT debe contener solo números.";
                args.IsValid = false;
                return;
            }

            if (txtCuilCuit.Text.Length != 11)
            {
                validator.ErrorMessage = "El CUIL / CUIT debe tener 11 dígitos.";
                args.IsValid = false;
                return;
            }

            args.IsValid = true;
        }

        // VALIDACION - Nombre
        protected void cvNombre_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var validator = (CustomValidator)source;
            string valor = txtNombre.Text.Trim();

            if (string.IsNullOrWhiteSpace(valor))
            {
                validator.ErrorMessage = "El Nombre es obligatorio.";
                args.IsValid = false;
                return;
            }

            // Acepta letras (mayúsculas/minúsculas) y espacios
            if (!Regex.IsMatch(valor, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                validator.ErrorMessage = "El Nombre solo puede contener letras.";
                args.IsValid = false;
                return;
            }

            args.IsValid = true;
        }

        // VALIDACION - Apellido
        protected void cvApellido_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var validator = (CustomValidator)source;
            string valor = txtApellido.Text.Trim();

            if (string.IsNullOrWhiteSpace(valor))
            {
                validator.ErrorMessage = "El Apellido es obligatorio.";
                args.IsValid = false;
                return;
            }

            if (!Regex.IsMatch(valor, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                validator.ErrorMessage = "El Apellido solo puede contener letras.";
                args.IsValid = false;
                return;
            }

            args.IsValid = true;
        }

        // VALIDACION - Telefono
        protected void cvTelefono_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var validator = (CustomValidator)source;
            string valor = txtTelefono.Text.Trim();

            // No puede estar vacío
            if (string.IsNullOrWhiteSpace(valor))
            {
                validator.ErrorMessage = "El Teléfono es obligatorio.";
                args.IsValid = false;
                return;
            }

            // Debe contener solo números enteros (sin letras, espacios ni símbolos)
            if (!Regex.IsMatch(valor, @"^\d+$"))
            {
                validator.ErrorMessage = "El Teléfono solo puede contener números enteros.";
                args.IsValid = false;
                return;
            }

            args.IsValid = true;
        }

        // VALIDACION - Patente
        protected void cvPatente_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var validator = (CustomValidator)source;
            string valor = txtPatente.Text.Trim();

            if (string.IsNullOrWhiteSpace(valor))
            {
                validator.ErrorMessage = "La Patente es obligatoria.";
                args.IsValid = false;
                return;
            }

            args.IsValid = true;
        }

        // Campo - Categoría del Vehículo
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
                ddlCategoriaVehiculo.DataSource = categorias;
                ddlCategoriaVehiculo.DataTextField = "Categoria_descripcion";
                ddlCategoriaVehiculo.DataValueField = "Categoria_id";
                ddlCategoriaVehiculo.DataBind();
                ddlCategoriaVehiculo.Items.Insert(0, new System.Web.UI.WebControls.ListItem("-- Seleccionar Categoría --", "0"));
            }
        }

        // VALIDACION - Categoría del Vehículo
        protected void cvCategoriaVehiculo_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var validator = (CustomValidator)source;

            if (ddlCategoriaVehiculo.SelectedValue == "0")
            {
                validator.ErrorMessage = "Debe seleccionar una Categoría de Vehículo.";
                args.IsValid = false;
                return;
            }

            args.IsValid = true;
        }

        private void LimpiarDropDown(DropDownList ddl, string textoDefault)
        {
            ddl.Items.Clear();
            ddl.Items.Insert(0, new ListItem(textoDefault, "0"));
            ddl.Enabled = false;
        }


        // Usamos el id del Estacionamiento y el de la Categoría seleccionada para filtrar las Plazas y Abono
        protected void ddlCategoriaVehiculo_SelectedIndexChanged(object sender, EventArgs e)
        {
            int? estacionamientoId = ObtenerEstacionamientoId();
            int categoriaSeleccionadaId = Convert.ToInt32(ddlCategoriaVehiculo.SelectedValue);

            // Filtrar Abono
            CargarTipoAbonoFiltrado(estacionamientoId, categoriaSeleccionadaId);

            // Filtrar Plazas
            CargarPlazasFiltradas(estacionamientoId, categoriaSeleccionadaId);
        }


        // Tipo de Abono
        protected void CargarTipoAbonoFiltrado(int? estacionamientoId, int categoriaSeleccionadaId)
        {
            if (categoriaSeleccionadaId == 0) // Si no se seleccionó Categoría no se habilita Abono
            {
                LimpiarDropDown(ddlTipoAbono, "--Seleccione Abono--");
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
                    ddlTipoAbono.DataSource = tarifas;
                    ddlTipoAbono.DataTextField = "Descripcion";
                    ddlTipoAbono.DataValueField = "Tarifa_id";
                    ddlTipoAbono.DataBind();
                    ddlTipoAbono.Items.Insert(0, new ListItem("--Seleccione Abono--", "0"));
                    ddlTipoAbono.Enabled = true;
                }
                else
                {
                    LimpiarDropDown(ddlTipoAbono, "--Seleccione Abono--");
                }
            }
        }

        // VALIDACION - Abono
        protected void cvTipoAbono_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var validator = (CustomValidator)source;

            if (ddlTipoAbono.SelectedValue == "0")
            {
                validator.ErrorMessage = "Debe seleccionar un tipo de Abono.";
                args.IsValid = false;
                return;
            }

            args.IsValid = true;
        }

        // Campo - Precio
        protected void ddlTipoAbono_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlTipoAbono.SelectedValue == "0")
            {
                lblPrecio.Text = string.Empty;
                return;
            }

            int tarifaId = int.Parse(ddlTipoAbono.SelectedValue);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                var tarifa = db.Tarifa.FirstOrDefault(t => t.Tarifa_id == tarifaId);
                if (tarifa != null)
                {
                    lblPrecio.Text = tarifa.Tarifa_Monto.ToString("0.00"); // Ejemplo: $1.500,00
                }
                else
                {
                    lblPrecio.Text = "0.00";
                }
            }
            CalcularFechaHasta();
            ActualizarTotal();
        }


        // Plaza
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

        // VALIDACION - Plaza
        protected void cvPlaza_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var validator = (CustomValidator)source;

            if (ddlPlaza.SelectedValue == "0")
            {
                validator.ErrorMessage = "Debe seleccionar una plaza.";
                args.IsValid = false;
                return;
            }

            args.IsValid = true;
        }

        // VALIDACION - Cantidad de tiempo
        protected void cvCantidadTiempo_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var validator = (CustomValidator)source;
            string valor = txtCantidadTiempo.Text.Trim();

            // No puede estar vacío
            if (string.IsNullOrWhiteSpace(valor))
            {
                validator.ErrorMessage = "La cantidad de tiempo es obligatoria.";
                args.IsValid = false;
                return;
            }

            // Solo se aceptan números enteros positivos
            if (!Regex.IsMatch(valor, @"^\d+$"))
            {
                validator.ErrorMessage = "La cantidad de tiempo debe ser un número entero.";
                args.IsValid = false;
                return;
            }

            args.IsValid = true;
        }


        // VALIDACION - Desde
        protected void cvDesde_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var validator = (CustomValidator)source;
            string valor = txtDesde.Text.Trim();

            // 1️ No puede estar vacío
            if (string.IsNullOrWhiteSpace(valor))
            {
                validator.ErrorMessage = "La fecha y hora de inicio (Desde) son obligatorias.";
                args.IsValid = false;
                return;
            }

            // 2️ Intentar parsear fecha y hora (formato ISO de datetime-local)
            DateTime fechaDesde;
            if (!DateTime.TryParse(valor, out fechaDesde))
            {
                validator.ErrorMessage = "Debe ingresar una fecha y hora válidas.";
                args.IsValid = false;
                return;
            }

            // 3️ Debe ser igual o posterior a la fecha y hora actual
            if (fechaDesde < DateTime.Now)
            {
                validator.ErrorMessage = "La fecha y hora de inicio deben ser iguales o posteriores al momento actual.";
                args.IsValid = false;
                return;
            }

            args.IsValid = true;
        }

        // Campo - Hasta
        private void CalcularFechaHasta()
        {
            // Validar que los campos requeridos tengan valor
            if (string.IsNullOrWhiteSpace(txtDesde.Text) ||
                ddlTipoAbono.SelectedValue == "0" ||
                string.IsNullOrWhiteSpace(txtCantidadTiempo.Text))
            {
                lblHasta.Text = "";
                return;
            }

            // Intentar parsear la fecha/hora Desde
            DateTime fechaDesde;
            string valorDesde = txtDesde.Text.Trim();

            // Formato esperado de un input type="datetime-local" → "yyyy-MM-ddTHH:mm"
            string[] formatosValidos = { "yyyy-MM-ddTHH:mm", "dd-MM-yyyy HH:mm", "dd-MM-yyyy" };

            if (!DateTime.TryParseExact(valorDesde, formatosValidos,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out fechaDesde))
            {
                lblHasta.Text = string.Empty;
                return;
            }

            // Parsear cantidad de tiempo
            if (!int.TryParse(txtCantidadTiempo.Text, out int cantidad) || cantidad <= 0)
            {
                lblHasta.Text = string.Empty;
                return;
            }

            // Obtener tipo de abono
            string tipoAbono = ddlTipoAbono.SelectedItem.Text; // "Por hora", "Por día", etc.
            DateTime fechaHasta = fechaDesde;

            switch (tipoAbono)
            {
                case "Por hora":
                    fechaHasta = fechaDesde.AddHours(cantidad);
                    break;
                case "Por día":
                    fechaHasta = fechaDesde.AddDays(cantidad);
                    break;
                case "Semanal":
                    fechaHasta = fechaDesde.AddDays(7 * cantidad);
                    break;
                case "Mensual":
                    fechaHasta = fechaDesde.AddMonths(cantidad);
                    break;
                case "Anual":
                    fechaHasta = fechaDesde.AddYears(cantidad);
                    break;
            }

            // Mostrar fecha y hora
            lblHasta.Text = fechaHasta.ToString("dd-MM-yyyy HH:mm");
        }


        // Llamadas automáticas al método CalcularFechaHasta
        protected void txtDesde_TextChanged(object sender, EventArgs e)
        {
            CalcularFechaHasta();
        }

        protected void txtCantidadTiempo_TextChanged(object sender, EventArgs e)
        {
            CalcularFechaHasta();
            ActualizarTotal();
        }


        // Metodos de Pago
        private void CargarMetodosDePagoEnDropDown()
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                int? estacionamientoId = (int)Session["Playero_EstId"];

                var metodosPago = db.Acepta_Metodo_De_Pago
                    .Where(a => a.Est_id == estacionamientoId)
                    .Select(a => a.Metodos_De_Pago)
                    .Distinct()
                    .ToList();

                ddlMetodoPago.Items.Clear();

                if (metodosPago.Any())
                {
                    ddlMetodoPago.DataSource = metodosPago;
                    ddlMetodoPago.DataTextField = "Metodo_pago_descripcion";
                    ddlMetodoPago.DataValueField = "Metodo_pago_id";
                    ddlMetodoPago.DataBind();
                }

                // Opción por defecto al principio
                ddlMetodoPago.Items.Insert(0, new ListItem("-- Seleccione Método de Pago --", "0"));
            }
        }


        // VALIDAR - Metodo de Pago
        protected void cvMetodoPago_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (ddlMetodoPago.SelectedValue == "0")
            {
                args.IsValid = false;
                ((CustomValidator)source).ErrorMessage = "Debe Seleccionar un Método de Pago.";
            }
            else
            {
                args.IsValid = true;
            }
        }


        // TOTAL
        private void ActualizarTotal()
        {
            if (double.TryParse(lblPrecio.Text, out double precio) &&
                int.TryParse(txtCantidadTiempo.Text, out int cantidad))
            {
                double total = precio * cantidad;
                lblTotal.Text = total.ToString("0.00");
            }
            else
            {
                lblTotal.Text = "0.00";
            }
        }


        // GUARDAR
        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            // Por ahora vacío
            // Aquí se implementará la lógica para guardar los datos en la base de datos
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("Abonados_Listar.aspx");
        }
    }
}
