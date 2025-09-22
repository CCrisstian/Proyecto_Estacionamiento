using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Plaza
{
    public partial class Plaza_Crear_Editar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarEstacionamientos();
                CargarCategorias();

                if (Request.QueryString["id"] != null)  // verificamos si es una Plaza existente mediante el parámetro id en la URL
                {
                    lblTitulo.Text = "Editar Plaza";
                    btnGuardar.Text = "Actualizar";
                    int plazaId = int.Parse(Request.QueryString["id"]); // Guardamos el id de la Plaza desde la URL
                    ddlEstacionamiento.Enabled = false; // Deshabilita el campo Estacionamiento
                    CargarPlaza(plazaId);   // Cargamos el id de la Plaza para poder Editarla
                }
                else
                {
                    lblTitulo.Text = "Agregar Plaza";
                    // Modo Alta
                    chkDisponibilidad.Checked = true;  // marcado por defecto
                    chkDisponibilidad.Visible = false; // no se muestra
                }
            }
        }

        // Cargamos los Estacionamientos al cargar la página para poder seleccionar uno por su Nombre
        private void CargarEstacionamientos()
        {
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var context = new ProyectoEstacionamientoEntities())
            {
                List<object> estacionamientos;

                if (Session["Dueño_EstId"] != null)
                {
                    // Dueño eligió un estacionamiento → solo ese
                    int estIdSeleccionado = (int)Session["Dueño_EstId"];
                    estacionamientos = context.Estacionamiento
                        .Where(e => e.Est_id == estIdSeleccionado && e.Est_Disponibilidad == true)
                        .Select(e => new { e.Est_id, e.Est_nombre })
                        .ToList<object>();
                    ddlEstacionamiento.Enabled = false; // Deshabilita el campo Estacionamiento
                }
                else
                {
                    // No eligió → mostramos todos sus estacionamientos disponibles
                    estacionamientos = context.Estacionamiento
                        .Where(e => e.Dueño_Legajo == legajo && e.Est_Disponibilidad == true)
                        .Select(e => new { e.Est_id, e.Est_nombre })
                        .ToList<object>();
                }

                ddlEstacionamiento.DataSource = estacionamientos;
                ddlEstacionamiento.DataValueField = "Est_id";
                ddlEstacionamiento.DataTextField = "Est_nombre";
                ddlEstacionamiento.DataBind();
            }
        }

        // Cargamos las Categorías de Vehículos al cargar la página para poder seleccionar una por su Descripción
        private void CargarCategorias()
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                ddlCategoria.DataSource = db.Categoria_Vehiculo.ToList();
                ddlCategoria.DataTextField = "Categoria_descripcion";
                ddlCategoria.DataValueField = "Categoria_id";
                ddlCategoria.DataBind();
            }
        }

        // Cargamos los datos de la Plaza seleccionada para poder Editarla
        private void CargarPlaza(int plazaId)
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                // Busca una Plaza por su ID y, si la encuentra, carga sus datos en los campos de la página para Edición.
                // Si no encuentra la Plaza, muestra un mensaje de error.
                var plaza = db.Plaza.FirstOrDefault(p => p.Plaza_id == plazaId);
                if (plaza != null)
                {
                    ddlEstacionamiento.SelectedValue = plaza.Est_id.ToString();
                    ddlCategoria.SelectedValue = plaza.Categoria_id.ToString();
                    txtNombre.Text = plaza.Plaza_Nombre;
                    txtTipo.Text = plaza.Plaza_Tipo;
                    chkDisponibilidad.Checked = plaza.Plaza_Disponibilidad;
                }
            }
        }

        // ------------------ VALIDADORES ------------------
        protected void cvEstacionamiento_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !string.IsNullOrEmpty(ddlEstacionamiento.SelectedValue);
        }

        protected void cvCategoria_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !string.IsNullOrEmpty(ddlCategoria.SelectedValue);
        }

        protected void cvNombre_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !string.IsNullOrWhiteSpace(txtNombre.Text);
        }

        protected void cvTipo_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = !string.IsNullOrWhiteSpace(txtTipo.Text);
        }

        // ------------------ GUARDAR ------------------
        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            using (var db = new ProyectoEstacionamientoEntities())
            {
                Proyecto_Estacionamiento.Plaza plaza;

                if (Request.QueryString["id"] != null)
                {
                    // Editar
                    int plazaId = int.Parse(Request.QueryString["id"]);
                    plaza = db.Plaza.FirstOrDefault(p => p.Plaza_id == plazaId);
                    if (plaza == null)
                    {
                        // mostramos error en un validador existente en vez de lblMensaje
                        cvNombre.IsValid = false;
                        cvNombre.ErrorMessage = "Plaza no encontrada.";
                        return;
                    }
                }
                else
                {
                    // Agregar
                    plaza = new Proyecto_Estacionamiento.Plaza();
                    plaza.Est_id = int.Parse(ddlEstacionamiento.SelectedValue);
                    db.Plaza.Add(plaza);
                }

                // Asignación de valores
                plaza.Categoria_id = int.Parse(ddlCategoria.SelectedValue);
                plaza.Plaza_Nombre = txtNombre.Text.Trim();
                plaza.Plaza_Tipo = txtTipo.Text.Trim();
                plaza.Plaza_Disponibilidad = chkDisponibilidad.Checked;

                db.SaveChanges();

                string accion = Request.QueryString["id"] == null ? "agregado" : "editado";
                Response.Redirect($"Plaza_Listar.aspx?exito=1&accion={accion}");
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Plaza/Plaza_Listar.aspx");
        }
    }
}