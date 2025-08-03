using System;
using System.Linq;

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
                    int plazaId = int.Parse(Request.QueryString["id"]); // Guardamos el id de la Plaza desde la URL
                    ddlEstacionamiento.Enabled = false; // Deshabilita el campo Estacionamiento
                    CargarPlaza(plazaId);   // Cargamos el id de la Plaza para poder Editarla
                }
                else
                {
                    lblTitulo.Text = "Agregar Plaza";
                }
            }
        }

        // Cargamos los Estacionamientos al cargar la página para poder seleccionar uno por su Nombre
        private void CargarEstacionamientos()   
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                ddlEstacionamiento.DataSource = db.Estacionamiento.ToList();
                ddlEstacionamiento.DataTextField = "Est_nombre";
                ddlEstacionamiento.DataValueField = "Est_id";
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
                    ddlDisponible.SelectedValue = plaza.Plaza_Disponibilidad ? "true" : "false";
                }
            }
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            using (var db = new ProyectoEstacionamientoEntities()) // Conexión a la Base de Datos
            {
                Proyecto_Estacionamiento.Plaza plaza;   // Creamos una nueva Plaza
                if (Request.QueryString["id"] != null)  // Si existe el parámetro id, significa que se está Editando.
                {
                    // Editar
                    int plazaId = int.Parse(Request.QueryString["id"]);
                    plaza = db.Plaza.FirstOrDefault(p => p.Plaza_id == plazaId);
                    if (plaza == null)
                    {
                        lblMensaje.Text = "Plaza no encontrada.";
                        return;
                    }
                }
                else
                {
                    // Agregar
                    plaza = new Proyecto_Estacionamiento.Plaza();
                    plaza.Est_id = int.Parse(ddlEstacionamiento.SelectedValue); // Solo en Agregar
                    db.Plaza.Add(plaza);    // Se agrega al contexto de la Base de Datos, pero aún no se guarda nada.
                }

                // Toma los valores seleccionados por el usuario o ya presentes en los campos de la página y los asigna a la entidad Plaza.
                plaza.Categoria_id = int.Parse(ddlCategoria.SelectedValue);
                plaza.Plaza_Nombre = txtNombre.Text;
                plaza.Plaza_Tipo = txtTipo.Text;
                plaza.Plaza_Disponibilidad = ddlDisponible.SelectedValue == "true";

                db.SaveChanges(); // Guardado en la Base de Datos
                Response.Redirect("~/Pages/Plaza/Plaza_Listar.aspx");
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/Plaza/Plaza_Listar.aspx");
        }
    }
}