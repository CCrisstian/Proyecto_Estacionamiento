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


        // VALIDACION - DNI
        protected void cvDNI_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var validator = (CustomValidator)source;
            string dni = TextDNI.Text.Trim(); // Usamos TextDNI, que es el ID de tu TextBox

            // 1. No debe aceptar vacío
            if (string.IsNullOrWhiteSpace(dni))
            {
                validator.ErrorMessage = "El DNI es obligatorio.";
                args.IsValid = false;
                return; // Detenemos la validación aquí
            }

            // 2. Usamos una Expresión Regular para validar el resto de las reglas de una sola vez:
            //    - Solo números enteros
            //    - Exactamente 8 dígitos
            //    - Sin letras ni símbolos
            var regex = new Regex(@"^\d{8}$");

            if (!regex.IsMatch(dni))
            {
                validator.ErrorMessage = "El DNI debe contener exactamente 8 números.";
                args.IsValid = false;
                return;
            }

            // Si pasó todas las validaciones, es válido
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


        protected void cvDesde_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var validator = (CustomValidator)source;
            string valor = txtDesde.Text.Trim();

            // 1️ No puede estar vacío
            if (string.IsNullOrWhiteSpace(valor))
            {
                validator.ErrorMessage = "La fecha y hora de inicio son obligatorias.";
                args.IsValid = false;
                return;
            }

            // 2️ Intentar parsear fecha y hora usando el formato ISO correcto
            DateTime fechaDesde;
            string formatoISO = "yyyy-MM-dd'T'HH:mm";
            if (!DateTime.TryParseExact(valor, formatoISO, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out fechaDesde))
            {
                validator.ErrorMessage = "Debe ingresar una fecha y hora válidas.";
                args.IsValid = false;
                return;
            }

            // 3️ Debe ser igual o posterior a la fecha y hora actual (IGNORANDO SEGUNDOS)

            // Obtenemos la hora actual del servidor
            DateTime ahora = DateTime.Now;

            // Creamos una nueva fecha actual pero con los segundos en cero
            DateTime ahoraSinSegundos = new DateTime(ahora.Year, ahora.Month, ahora.Day, ahora.Hour, ahora.Minute, 0);

            // Comparamos la fecha del usuario con nuestra fecha "truncada"
            if (fechaDesde < ahoraSinSegundos)
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
            txtHasta.Text = fechaHasta.ToString("yyyy-MM-dd'T'HH:mm");
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

            // Limpiamos cualquier error anterior
            lblError.Text = "";

            try
            {
                // ==========================================================
                // 0. VALIDACIÓN DE DATOS PREVIA (FORMATO CORRECTO)
                // ==========================================================
                Page.Validate("Abonado");
                if (!Page.IsValid)
                {
                    lblError.Text = "Por favor, corrija los errores marcados en el formulario.";
                    return;
                }

                DateTime fechaDesde, fechaHasta;
                var culture = System.Globalization.CultureInfo.InvariantCulture;

                // Define el formato ISO que envía el control datetime-local
                string formatoISO = "yyyy-MM-dd'T'HH:mm";

                // 1. Validamos la fecha "Desde"
                if (!DateTime.TryParseExact(txtDesde.Text, formatoISO, culture, System.Globalization.DateTimeStyles.None, out fechaDesde))
                {
                    lblError.Text = $"Error en 'Fecha Desde'. El valor '{txtDesde.Text}' no es una fecha y hora válida.";
                    return;
                }

                // 2. Validamos la fecha "Hasta"
                if (!DateTime.TryParseExact(txtHasta.Text, formatoISO, culture, System.Globalization.DateTimeStyles.None, out fechaHasta))
                {
                    lblError.Text = $"Error en 'Fecha Hasta'. El valor '{txtHasta.Text}' no es una fecha y hora válida.";
                    return;
                }


                // Si llegamos aquí, las fechas son válidas y están cargadas en las variables.

                // ==========================================================
                // Comienza la transacción de la base de datos
                // ==========================================================
                using (var db = new ProyectoEstacionamientoEntities())
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        // ==========================================================
                        // 1. INSERTAR "Titular_Abono"
                        // ==========================================================
                        var nuevoTitular = new Titular_Abono();
                        // ... (asignación de campos de nuevoTitular)
                        nuevoTitular.TAB_Cuil_Cuit = Convert.ToInt64(txtCuilCuit.Text.Trim());
                        nuevoTitular.TAB_DNI = Convert.ToInt32(TextDNI.Text.Trim());
                        nuevoTitular.TAB_Telefono = Convert.ToInt32(txtTelefono.Text.Trim());
                        nuevoTitular.TAB_Nombre = txtNombre.Text.Trim();
                        nuevoTitular.TAB_Apellido = string.IsNullOrWhiteSpace(txtApellido.Text) ? null : txtApellido.Text.Trim();
                        nuevoTitular.TAB_Fecha_Desde = fechaDesde;
                        nuevoTitular.TAB_Fecha_Vto = fechaHasta;
                        db.Titular_Abono.Add(nuevoTitular);
                        db.SaveChanges();

                        // ==========================================================
                        // 2. INSERTAR "Abono"
                        // ==========================================================
                        int? estId = ObtenerEstacionamientoId();
                        if (estId == null) throw new InvalidOperationException("No se pudo determinar el estacionamiento actual.");
                        int plazaId = Convert.ToInt32(ddlPlaza.SelectedValue);

                        var nuevoAbono = new Abono();
                        // ... (asignación de campos de nuevoAbono)
                        nuevoAbono.Est_id = estId.Value;
                        nuevoAbono.Plaza_id = plazaId;
                        nuevoAbono.TAB_Cuil_Cuit = nuevoTitular.TAB_Cuil_Cuit;
                        nuevoAbono.TAB_Fecha_Desde = nuevoTitular.TAB_Fecha_Desde;
                        nuevoAbono.TAB_DNI = nuevoTitular.TAB_DNI;
                        db.Abono.Add(nuevoAbono);
                        db.SaveChanges();

                        // ==========================================================
                        // 3. ACTUALIZAR "Plaza"
                        // ==========================================================
                        var plaza = db.Plaza.FirstOrDefault(p => p.Est_id == estId && p.Plaza_id == plazaId);
                        if (plaza != null)
                        {
                            plaza.Plaza_Disponibilidad = false;
                        }
                        else
                        {
                            throw new InvalidOperationException($"La plaza con ID {plazaId} no fue encontrada.");
                        }
                        db.SaveChanges();

                        // ==========================================================
                        // 4. INSERTAR "Pagos_Abonados"
                        // ==========================================================
                        var nuevoPago = new Pagos_Abonados();
                        // ... (asignación de campos de nuevoPago)
                        nuevoPago.Est_id = nuevoAbono.Est_id;
                        nuevoPago.Plaza_id = nuevoAbono.Plaza_id;
                        nuevoPago.TAB_Cuil_Cuit = nuevoAbono.TAB_Cuil_Cuit;
                        nuevoPago.TAB_Fecha_Desde = nuevoAbono.TAB_Fecha_Desde;
                        nuevoPago.TAB_DNI = nuevoAbono.TAB_DNI;
                        nuevoPago.Metodo_Pago_id = Convert.ToInt32(ddlMetodoPago.SelectedValue);
                        nuevoPago.PA_Monto = Convert.ToDouble(lblTotal.Text);
                        db.Pagos_Abonados.Add(nuevoPago);
                        db.SaveChanges();

                        // ==========================================================
                        // 5 & 6. INSERTAR "Vehiculo" Y "Vehiculo_Abonado" (CON MANEJO DE ERROR ESPECÍFICO)
                        // ==========================================================
                        try
                        {
                            int categoriaId = Convert.ToInt32(ddlCategoriaVehiculo.SelectedValue);
                            int tarifaId = Convert.ToInt32(ddlTipoAbono.SelectedValue);

                            var patentesUnicas = txtPatente.Text
                                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(p => p.Trim().ToUpper())
                                .Distinct();

                            // Verificamos si hay al menos una patente para procesar
                            if (!patentesUnicas.Any())
                            {
                                throw new InvalidOperationException("No se ingresó ninguna patente válida.");
                            }

                            foreach (var patente in patentesUnicas)
                            {
                                // Paso 1: Verificamos si el vehículo ya existe en la base de datos.
                                var vehiculoExistente = db.Vehiculo.Find(patente);
                                if (vehiculoExistente == null)
                                {
                                    // Si no existe, lo agregamos al contexto. Aún no se guarda en la BD.
                                    var nuevoVehiculo = new Vehiculo { Vehiculo_Patente = patente, Categoria_id = categoriaId };
                                    db.Vehiculo.Add(nuevoVehiculo);
                                }

                                // Paso 2: Creamos la relación con el abono y la agregamos al contexto.
                                var nuevaRelacion = new Vehiculo_Abonado
                                {
                                    Vehiculo_Patente = patente,
                                    Tarifa_id = tarifaId,
                                    Est_id = nuevoAbono.Est_id,
                                    Plaza_id = nuevoAbono.Plaza_id,
                                    TAB_Cuil_Cuit = nuevoAbono.TAB_Cuil_Cuit,
                                    TAB_Fecha_Desde = nuevoAbono.TAB_Fecha_Desde,
                                    TAB_DNI = nuevoAbono.TAB_DNI
                                };
                                db.Vehiculo_Abonado.Add(nuevaRelacion);
                            }

                            // Guardamos TODOS los vehículos nuevos y TODAS las relaciones en una sola operación.
                            db.SaveChanges();
                        }
                        catch (Exception ex2)
                        {
                            System.Diagnostics.Debug.WriteLine(ex2.ToString());
                            lblError.Text = "Error al procesar las patentes. Verifique que todas tengan un formato válido y no excedan el largo permitido.";

                            throw;
                        }

                        // ==========================================================
                        // 7. CONFIRMACIÓN FINAL
                        // ==========================================================
                        transaction.Commit(); // Confirma TODAS las operaciones.

                        // ==========================================================
                        // 8. Redirigir después de confirmar
                        // ==========================================================
                        Response.Redirect($"~/Pages/Abonados/Abonados_Listar.aspx?exito=1");
                    }
                }
            }
            catch (Exception ex)
            {
                // El catch ahora atrapará otros errores (de base de datos, conversiones numéricas, etc.)
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                lblError.Text = "Ocurrió un error inesperado al guardar los datos. Verifique la información.";
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("Abonados_Listar.aspx");
        }
    }
}
