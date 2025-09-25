using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Tarifas
{
    public partial class Tarifa_Crear_Editar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {   // Cargar los datos necesarios para los dropdowns y campos de texto
                CargarEstacionamientos();
                CargarTiposTarifa();
                CargarCategorias();

                if (Request.QueryString["id"] != null)
                {
                    lblTitulo.Text = "Editar Tarifa";
                    btnGuardar.Text = "Actualizar";
                    int id = int.Parse(Request.QueryString["id"]);
                    CargarTarifa(id);
                }
            }
        }

        // Método para cargar los datos de la Tarifa si se está Editando
        private void CargarTarifa(int id)
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                var tarifa = db.Tarifa.FirstOrDefault(t => t.Tarifa_id == id);
                if (tarifa != null)
                {
                    ddlEstacionamientos.SelectedValue = tarifa.Est_id?.ToString() ?? "";
                    ddlTiposTarifa.SelectedValue = tarifa.Tipos_Tarifa_Id?.ToString() ?? "";
                    ddlCategorias.SelectedValue = tarifa.Categoria_id?.ToString() ?? "";
                    txtTarifaMonto.Text = tarifa.Tarifa_Monto.ToString("0.##");
                }
            }
        }

        // Método para cargar Estacionamiento en los dropdowns
        private void CargarEstacionamientos()
        {
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var context = new ProyectoEstacionamientoEntities())
            {
                List<object> estacionamientos;

                if (Session["Dueño_EstId"] != null)
                {
                    // Mostrar solo el estacionamiento seleccionado
                    int estIdSeleccionado = (int)Session["Dueño_EstId"];
                    estacionamientos = context.Estacionamiento
                        .Where(e => e.Est_id == estIdSeleccionado)
                        .Select(e => new { e.Est_id, e.Est_nombre })
                        .ToList<object>();

                    ddlEstacionamientos.Enabled = false;
                    ddlEstacionamientos.DataSource = estacionamientos;
                    ddlEstacionamientos.DataValueField = "Est_id";
                    ddlEstacionamientos.DataTextField = "Est_nombre";
                    ddlEstacionamientos.DataBind();
                }
                else
                {
                    // Mostrar todos los estacionamientos disponibles del Dueño
                    estacionamientos = context.Estacionamiento
                        .Where(e => e.Dueño_Legajo == legajo && e.Est_Disponibilidad == true)
                        .Select(e => new { e.Est_id, e.Est_nombre })
                        .ToList<object>();

                    ddlEstacionamientos.DataSource = estacionamientos;
                    ddlEstacionamientos.DataValueField = "Est_id";
                    ddlEstacionamientos.DataTextField = "Est_nombre";
                    ddlEstacionamientos.DataBind();

                    // Insertar opción por defecto solo si mostramos todos
                    ddlEstacionamientos.Items.Insert(0, new ListItem("-- Seleccione un Estacionamiento --", ""));
                }
            }
        }



        // Método para cargar Tipos de Tarifa en el dropdown
        private void CargarTiposTarifa()
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                ddlTiposTarifa.DataSource = db.Tipos_Tarifa.ToList();
                ddlTiposTarifa.DataTextField = "Tipos_tarifa_descripcion";
                ddlTiposTarifa.DataValueField = "Tipos_tarifa_id";
                ddlTiposTarifa.DataBind();
                ddlTiposTarifa.Items.Insert(0, new ListItem("-- Seleccione una Tarifa--", ""));
            }
        }

        // Método para cargar Categorías de Vehículo en el dropdown
        private void CargarCategorias()
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                ddlCategorias.DataSource = db.Categoria_Vehiculo.ToList();
                ddlCategorias.DataTextField = "Categoria_descripcion";
                ddlCategorias.DataValueField = "Categoria_id";
                ddlCategorias.DataBind();
                ddlCategorias.Items.Insert(0, new ListItem("-- Seleccione una Categoía --", ""));
            }
        }

        // Validación Estacionamiento
        protected void cvEstacionamientos_ServerValidate(object source, ServerValidateEventArgs args)
        {
            // Caso 1: el Dueño ya eligió antes → solo un item en la lista
            if (Session["Dueño_EstId"] != null)
            {
                args.IsValid = true;
                return;
            }

            // Caso 2: no eligió antes → validar que no esté en "-- Seleccione --"
            if (ddlEstacionamientos.SelectedIndex == 0)
            {
                args.IsValid = false;
                cvEstacionamientos.ErrorMessage = "Debe seleccionar un Estacionamiento.";
            }
            else
            {
                args.IsValid = true;
            }
        }

        // Validación Tarifa
        protected void cvTiposTarifa_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (ddlTiposTarifa.SelectedIndex == 0)
            {
                args.IsValid = false;
                cvTiposTarifa.ErrorMessage = "Debe seleccionar un Tipo de Tarifa.";
            }
            else
            {
                args.IsValid = true;
            }
        }

        // Validación Categoria
        protected void cvCategorias_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (ddlCategorias.SelectedIndex == 0)
            {
                args.IsValid = false;
                cvCategorias.ErrorMessage = "Debe seleccionar una Categoría.";
            }
            else
            {
                args.IsValid = true;
            }
        }

        // Validación Monto
        protected void cvMonto_ServerValidate(object source, ServerValidateEventArgs args)
        {
            string valor = txtTarifaMonto.Text; // sin usar args.Value

            // 1. Requerido
            if (string.IsNullOrWhiteSpace(valor))
            {
                args.IsValid = false;
                cvMonto.ErrorMessage = "Debe ingresar un Monto.";
                return;
            }

            // 2. Validar que sea numérico
            if (!decimal.TryParse(valor, out decimal monto))
            {
                args.IsValid = false;
                cvMonto.ErrorMessage = "El monto debe ser un número válido.";
                return;
            }

            // 3. Validar máximo 2 decimales
            if (valor.Contains("."))
            {
                int decimales = valor.Split('.')[1].Length;
                if (decimales > 2)
                {
                    args.IsValid = false;
                    cvMonto.ErrorMessage = "El monto no puede tener más de 2 decimales.";
                    return;
                }
            }

            // 4. Validar positivo
            if (monto < 0)
            {
                args.IsValid = false;
                cvMonto.ErrorMessage = "El monto debe ser un valor positivo.";
                return;
            }

            args.IsValid = true;
        }


        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            Page.Validate();
            if (!Page.IsValid)
            {
                return;
            }

            if (!decimal.TryParse(txtTarifaMonto.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal monto))
            {
                lblTitulo.Text = "El Monto ingresado no es válido.";
                lblTitulo.ForeColor = System.Drawing.Color.Red;
                return;
            }

            using (var db = new ProyectoEstacionamientoEntities())
            {
                int estacionamientoId = int.Parse(ddlEstacionamientos.SelectedValue);
                int tipoTarifaId = int.Parse(ddlTiposTarifa.SelectedValue);
                int categoriaId = int.Parse(ddlCategorias.SelectedValue);

                int? idTarifaEditando = null;
                if (Request.QueryString["id"] != null)
                {
                    idTarifaEditando = int.Parse(Request.QueryString["id"]);
                }

                // Validar duplicados (otra tarifa con mismo Est, Tipo y Categoria)
                bool existeDuplicado;
                if (idTarifaEditando.HasValue)
                {
                    int idEdicion = idTarifaEditando.Value;
                    existeDuplicado = db.Tarifa.Any(t =>
                        t.Est_id == estacionamientoId &&
                        t.Tipos_Tarifa_Id == tipoTarifaId &&
                        t.Categoria_id == categoriaId &&
                        t.Tarifa_id != idEdicion); // excluimos la tarifa que estamos editando
                }
                else
                {
                    existeDuplicado = db.Tarifa.Any(t =>
                        t.Est_id == estacionamientoId &&
                        t.Tipos_Tarifa_Id == tipoTarifaId &&
                        t.Categoria_id == categoriaId);
                }

                if (existeDuplicado)
                {
                    lblTitulo.Text = "Ya existe una tarifa con ese Estacionamiento, Tipo de Tarifa y Categoría.";
                    lblTitulo.ForeColor = System.Drawing.Color.Red;
                    return;
                }

                if (idTarifaEditando != null) // Editar
                {
                    // Traemos la entidad existente
                    Tarifa tarifaExistente = db.Tarifa.Find(idTarifaEditando.Value);
                    if (tarifaExistente != null)
                    {
                        tarifaExistente.Tarifa_Monto = (double)monto;
                        tarifaExistente.Tarifa_Desde = DateTime.Now;

                        db.SaveChanges();
                    }
                }
                else // Agregar
                {
                    Tarifa tarifaNueva = new Tarifa
                    {
                        Est_id = estacionamientoId,
                        Tipos_Tarifa_Id = tipoTarifaId,
                        Categoria_id = categoriaId,
                        Tarifa_Monto = (double)monto,
                        Tarifa_Desde = DateTime.Now
                    };

                    db.Tarifa.Add(tarifaNueva);
                    db.SaveChanges();
                }

                string accion = idTarifaEditando == null ? "agregado" : "editado";
                Response.Redirect($"Tarifa_Listar.aspx?exito=1&accion={accion}");
            }
        }


        protected void btnCancelar_Click(object sender, EventArgs e)
        {

            Response.Redirect("Tarifa_Listar.aspx");
        }

    }
}