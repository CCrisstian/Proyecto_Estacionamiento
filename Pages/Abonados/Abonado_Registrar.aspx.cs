using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        // VALIDACION - Numero_Identificacion
        protected void CvNumero_Identificacion_ServerValidate(object source, ServerValidateEventArgs args)
        {
            var validator = (CustomValidator)source;
            string numeroStr = txtNumero_Identificacion.Text.Trim();
            string tipoId = ddlTipoIdentificacion.SelectedValue; // Leemos el tipo seleccionado

            // 1. Validar que el número no esté vacío
            if (string.IsNullOrWhiteSpace(numeroStr))
            {
                validator.ErrorMessage = "El Número de Identificación es obligatorio.";
                args.IsValid = false;
                return;
            }

            // 2. Validar que sean solo números
            // Usamos Regex para permitir cualquier cantidad de dígitos (luego validamos la longitud)
            if (!Regex.IsMatch(numeroStr, @"^\d+$"))
            {
                validator.ErrorMessage = "El Número de Identificación debe contener solo números.";
                args.IsValid = false;
                return;
            }

            // 3. Validar longitud y formato según el tipo seleccionado
            args.IsValid = false; // Asumir inválido hasta que un caso lo valide

            switch (tipoId)
            {
                case "CUIL":
                case "CUIT":
                    if (numeroStr.Length == 11)
                    {
                        args.IsValid = true;
                    }
                    else
                    {
                        validator.ErrorMessage = "El CUIT/CUIL debe tener 11 dígitos.";
                    }
                    break;

                case "DNI":
                    if (numeroStr.Length != 8)
                    {
                        validator.ErrorMessage = "El DNI debe tener 8 dígitos.";
                    }
                    else if (numeroStr.StartsWith("9"))
                    {
                        validator.ErrorMessage = "Un DNI de 8 dígitos no puede comenzar con 9. Si es DNI Extranjero, selecciónelo.";
                    }
                    else
                    {
                        args.IsValid = true;
                    }
                    break;

                case "DNI-Extranjero":
                    if (numeroStr.Length != 8)
                    {
                        validator.ErrorMessage = "El DNI Extranjero debe tener 8 dígitos.";
                    }
                    else if (!numeroStr.StartsWith("9"))
                    {
                        validator.ErrorMessage = "El DNI Extranjero debe comenzar con el dígito 9.";
                    }
                    else
                    {
                        args.IsValid = true;
                    }
                    break;

                case "": // El usuario no seleccionó un tipo
                    validator.ErrorMessage = "Debe seleccionar un Tipo de Identificación válido.";
                    break;

                default: // Un valor inesperado
                    validator.ErrorMessage = "Tipo de Identificación no reconocido.";
                    break;
            }
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
                // 1. Definimos los únicos IDs de Tipos_Tarifa que queremos mostrar
                var idsAbonos = new List<int> { 3, 4, 5 }; // 3:Semanal, 4:Mensual, 5:Anual

                // 2. Traemos la lista de la BD, filtrando por los IDs permitidos
                var tarifas = db.Tarifa
                    .Where(t => t.Est_id == estacionamientoId &&
                                t.Categoria_id == categoriaSeleccionadaId &&
                                idsAbonos.Contains(t.Tipos_Tarifa.Tipos_tarifa_id)) // <-- FILTRO AÑADIDO
                    .Select(t => new
                    {
                        t.Tarifa_id,
                        Descripcion = t.Tipos_Tarifa != null ? t.Tipos_Tarifa.Tipos_tarifa_descripcion : "(Sin descripción)",
                        t.Tarifa_Monto,
                        t.Tipos_Tarifa.Tipos_tarifa_id // <-- Necesario para ordenar
                    })
                    .ToList(); // Traemos la lista a memoria

                // 3. Definimos el orden lógico (Semanal, Mensual, Anual)
                var ordenLogicoIDs = new List<int> { 3, 4, 5 };

                // 4. Ordenamos la lista en memoria
                var tarifasOrdenadas = tarifas
                    .OrderBy(t => ordenLogicoIDs.IndexOf(t.Tipos_tarifa_id))
                    .ToList();

                // 5. Continuamos con la lógica para poblar el dropdown
                if (tarifasOrdenadas.Any())
                {
                    ddlTipoAbono.DataSource = tarifasOrdenadas; // Usamos la lista ordenada
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
            // Limpiamos el label de tipo de plaza CADA VEZ que se recargan las plazas
            lblPlazaTipo.Text = string.Empty;

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
                                    && !p.Abono.Any(a => a.Fecha_Vto >= ahora))
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

        protected void ddlPlaza_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Si el usuario selecciona "--Seleccione Plaza--" (valor "0")
            if (ddlPlaza.SelectedValue == "0" || string.IsNullOrEmpty(ddlPlaza.SelectedValue))
            {
                lblPlazaTipo.Text = string.Empty;
                return;
            }

            // Obtenemos los IDs necesarios para buscar la plaza
            int plazaId = int.Parse(ddlPlaza.SelectedValue);
            int? estacionamientoId = ObtenerEstacionamientoId();

            using (var db = new ProyectoEstacionamientoEntities())
            {
                // Buscamos la plaza específica usando la clave primaria (Est_id y Plaza_id)
                var plaza = db.Plaza.FirstOrDefault(p => p.Est_id == estacionamientoId && p.Plaza_id == plazaId);

                if (plaza != null)
                {
                    // Asignamos el valor de Plaza_Tipo al Label
                    lblPlazaTipo.Text = $"{plaza.Plaza_Tipo}";
                }
                else
                {
                    lblPlazaTipo.Text = string.Empty; // No debería pasar, pero por seguridad
                }
            }
        }

        // VALIDACION - Plaza
        protected void CvPlaza_ServerValidate(object source, ServerValidateEventArgs args)
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

        protected void TxtNumero_Identificacion_TextChanged(object sender, EventArgs e)
        {
            string numeroStr = txtNumero_Identificacion.Text.Trim();
            string tipoId = ddlTipoIdentificacion.SelectedValue;

            // Solo buscar si tenemos ambos datos y el número parece válido (11 dígitos)
            if (!string.IsNullOrEmpty(numeroStr) && numeroStr.Length == 11 && long.TryParse(numeroStr, out long numeroParsed) && !string.IsNullOrEmpty(tipoId))
            {
                using (var db = new ProyectoEstacionamientoEntities()) // Usa el nombre correcto de tu contexto
                {
                    // Buscamos el último registro de titular con esa identificación (podría tener varios abonos históricos)
                    var titularExistente = db.Titular_Abono
                                            .Where(t => t.Numero_Identificacion == numeroParsed && t.Tipo_Identificacion == tipoId)
                                            .OrderByDescending(t => t.TAB_Fecha_Alta) // Tomamos el más reciente
                                            .FirstOrDefault();

                    if (titularExistente != null)
                    {
                        // Autocompletar campos
                        txtNombre.Text = titularExistente.TAB_Nombre;
                        txtApellido.Text = titularExistente.TAB_Apellido;
                        txtTelefono.Text = titularExistente.TAB_Telefono.ToString();

                        // Deshabilitar campos para evitar edición accidental
                        txtNombre.Enabled = false;
                        txtApellido.Enabled = false;
                        txtTelefono.Enabled = false;
                    }
                    else
                    {
                        // Si no se encuentra, limpiar campos (por si cambió el número)
                        LimpiarCamposTitular();
                        // Habilitar campos si estaban deshabilitados
                        HabilitarCamposTitular(true);
                    }
                }
            }
            else
            {
                // Si el número/tipo no es válido, limpiar campos
                LimpiarCamposTitular();
                // Habilitar campos si estaban deshabilitados
                HabilitarCamposTitular(true);
            }
        }

        // Método auxiliar para limpiar campos
        private void LimpiarCamposTitular()
        {
            txtNombre.Text = string.Empty;
            txtApellido.Text = string.Empty;
            txtTelefono.Text = string.Empty;
        }

        private void HabilitarCamposTitular(bool habilitar)
        {
            txtNombre.Enabled = habilitar;
            txtApellido.Enabled = habilitar;
            txtTelefono.Enabled = habilitar;
        }

        // GUARDAR
        protected void BtnGuardar_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            try
            {
                // 0. VALIDACIÓN DE DATOS PREVIA
                Page.Validate("Abonado");
                if (!Page.IsValid)
                {
                    return;
                }

                DateTime fechaDesde, fechaHasta;
                var culture = System.Globalization.CultureInfo.InvariantCulture;
                string formatoISO = "yyyy-MM-dd'T'HH:mm";

                if (!DateTime.TryParseExact(txtDesde.Text, formatoISO, culture, System.Globalization.DateTimeStyles.None, out fechaDesde))
                {
                    lblError.Text = $"Error en 'Fecha Desde'. El valor '{txtDesde.Text}' no es una fecha y hora válida.";
                    return;
                }
                if (!DateTime.TryParseExact(txtHasta.Text, formatoISO, culture, System.Globalization.DateTimeStyles.None, out fechaHasta))
                {
                    lblError.Text = $"Error en 'Fecha Hasta'. El valor '{txtHasta.Text}' no es una fecha y hora válida.";
                    return;
                }

                // ==========================================================
                // Comienza la transacción de la base de datos
                // ==========================================================
                using (var db = new ProyectoEstacionamientoEntities())
                {
                    using (var transaction = db.Database.BeginTransaction())
                    {
                        // ==========================================================
                        // 1. BUSCAR O INSERTAR "Titular_Abono"
                        // ==========================================================
                        long numeroIdParsed = Convert.ToInt64(txtNumero_Identificacion.Text.Trim());
                        string tipoIdSeleccionado = ddlTipoIdentificacion.SelectedValue;

                        Titular_Abono titularParaAbono = db.Titular_Abono
                                                          .FirstOrDefault(t => t.Numero_Identificacion == numeroIdParsed && t.Tipo_Identificacion == tipoIdSeleccionado);

                        if (titularParaAbono == null) // Si NO existe, lo creamos
                        {
                            titularParaAbono = new Titular_Abono();
                            titularParaAbono.Numero_Identificacion = numeroIdParsed;
                            titularParaAbono.Tipo_Identificacion = tipoIdSeleccionado;
                            titularParaAbono.TAB_Telefono = Convert.ToInt64(txtTelefono.Text.Trim());
                            titularParaAbono.TAB_Nombre = txtNombre.Text.Trim();
                            titularParaAbono.TAB_Apellido = string.IsNullOrWhiteSpace(txtApellido.Text) ? null : txtApellido.Text.Trim();
                            titularParaAbono.TAB_Fecha_Alta = fechaDesde;
                            db.Titular_Abono.Add(titularParaAbono);
                        }

                        // ==========================================================
                        // 2. INSERTAR "Abono"
                        // ==========================================================
                        int? estId = ObtenerEstacionamientoId();
                        if (estId == null) throw new InvalidOperationException("No se pudo determinar el estacionamiento actual.");
                        int plazaId = Convert.ToInt32(ddlPlaza.SelectedValue);

                        var nuevoAbono = new Abono();
                        nuevoAbono.Est_id = estId.Value;
                        nuevoAbono.Plaza_id = plazaId;
                        nuevoAbono.Numero_Identificacion = titularParaAbono.Numero_Identificacion;
                        nuevoAbono.Tipo_Identificacion = titularParaAbono.Tipo_Identificacion;
                        nuevoAbono.Fecha_Desde = fechaDesde;
                        nuevoAbono.Fecha_Vto = fechaHasta;
                        db.Abono.Add(nuevoAbono);

                        // Esto es necesario para que la BD genere el nuevo 'Id_Abono' (IDENTITY).
                        db.SaveChanges();

                        // ==========================================================
                        // 3. INSERTAR "Pagos_Abonados"
                        // ==========================================================

                        // Validar turno
                        if (Session["Turno_Id_Actual"] == null)
                        {
                            lblError.Text = "Error: Debe iniciar un turno de caja para registrar el cobro del abono.";
                            return;
                        }
                        int turnoIdActual = (int)Session["Turno_Id_Actual"];

                        var nuevoPago = new Pagos_Abonados();
                        nuevoPago.Id_Abono = nuevoAbono.Id_Abono;
                        nuevoPago.Est_id = nuevoAbono.Est_id;
                        nuevoPago.Fecha_Pago = fechaDesde;
                        nuevoPago.Metodo_Pago_id = Convert.ToInt32(ddlMetodoPago.SelectedValue);
                        nuevoPago.PA_Monto = Convert.ToDouble(lblTotal.Text);
                        int tarifaId = Convert.ToInt32(ddlTipoAbono.SelectedValue);
                        nuevoPago.Tarifa_id = tarifaId;
                        nuevoPago.Turno_id = turnoIdActual; // <-- Asignacion del TURNO
                        db.Pagos_Abonados.Add(nuevoPago);
                        db.SaveChanges();

                        // ==========================================================
                        // 4 & 5. INSERTAR "Vehiculo" Y "Vehiculo_Abonado"
                        // ==========================================================
                        try
                        {
                            int categoriaId = Convert.ToInt32(ddlCategoriaVehiculo.SelectedValue);
                            var patentesUnicas = txtPatente.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim().ToUpper()).Distinct();

                            if (!patentesUnicas.Any())
                            {
                                throw new InvalidOperationException("No se ingresó ninguna patente válida.");
                            }

                            foreach (var patente in patentesUnicas)
                            {
                                var vehiculoExistente = db.Vehiculo.Find(patente);
                                if (vehiculoExistente == null)
                                {
                                    var nuevoVehiculo = new Vehiculo { Vehiculo_Patente = patente, Categoria_id = categoriaId };
                                    db.Vehiculo.Add(nuevoVehiculo);
                                }

                                var nuevaRelacion = new Vehiculo_Abonado
                                {
                                    Vehiculo_Patente = patente,
                                    Id_Abono = nuevoAbono.Id_Abono
                                };
                                db.Vehiculo_Abonado.Add(nuevaRelacion);
                            }

                            // Guardamos la actualización de Plaza, el nuevo Pago, los nuevos Vehículos y las nuevas Relaciones
                            db.SaveChanges();
                        }
                        catch (Exception ex2)
                        {
                            System.Diagnostics.Debug.WriteLine(ex2.ToString());
                            lblError.Text = "Error al procesar las patentes. Verifique formato y largo.";
                            throw; // Re-lanza para revertir la transacción
                        }

                        // ==========================================================
                        // 6. CONFIRMACIÓN FINAL
                        // ==========================================================
                        transaction.Commit();
                        Response.Redirect($"~/Pages/Abonados/Abonados_Listar.aspx?exito=1");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                lblError.Text = "Error: " + ex.Message;
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("Abonados_Listar.aspx");
        }
    }
}
