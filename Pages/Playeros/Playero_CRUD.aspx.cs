using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Playeros
{
    public partial class Playero_CRUD : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarGrilla();
            }
        }

        private void CargarGrilla()
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                var datos = db.Playero
                    .Select(p => new
                    {
                        Est_nombre = p.Estacionamiento.Est_nombre,
                        Playero_legajo = p.Playero_legajo,
                        Usu_dni = p.Usuarios.Usu_dni,
                        Usu_pass = p.Usuarios.Usu_pass,
                        Usu_ap = p.Usuarios.Usu_ap,
                        Usu_nom = p.Usuarios.Usu_nom,
                        Usu_tipo = "Playero" // Valor por defecto
                    })
                    .ToList();
                gvPlayeros.DataSource = datos;
                gvPlayeros.DataBind();
            }
        }

        protected void btnAgregarPlayero_Click(object sender, EventArgs e)
        {
            // Redirige al formulario de creación sin parámetro (alta)
            Response.Redirect("Playero_Crear_Editar.aspx");
        }

        protected void gvPlayeros_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            
            if (e.CommandName == "Editar")
            {
                int index = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = gvPlayeros.Rows[index];
                int playeroLegajo = Convert.ToInt32(row.Cells[1].Text); // Ajusta el índice si cambias el orden de columnas

                // Redirige al formulario de edición con el legajo como parámetro
                Response.Redirect($"Playero_Crear_Editar.aspx?legajo={playeroLegajo}");
            }
        }

        protected void gvPlayeros_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                foreach (TableCell cell in e.Row.Cells)
                {
                    foreach (Control control in cell.Controls)
                    {
                        if (control is LinkButton btn && btn.CommandName == "Delete")
                        {
                            btn.OnClientClick = "return confirm('¿Está seguro que desea eliminar este playero?');";
                        }
                    }
                }
            }
        }

        protected void gvPlayeros_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            // Obtén el legajo del playero desde la fila seleccionada
            int playeroLegajo = Convert.ToInt32(gvPlayeros.DataKeys[e.RowIndex].Value);

            using (var context = new ProyectoEstacionamientoEntities()) // Reemplaza por tu contexto real
            {
                // Elimina primero el usuario relacionado
                var usuario = context.Usuarios.FirstOrDefault(u => u.Usu_legajo == playeroLegajo);
                if (usuario != null)
                    context.Usuarios.Remove(usuario);

                // Luego elimina el playero
                var playero = context.Playero.FirstOrDefault(p => p.Playero_legajo == playeroLegajo);
                if (playero != null)
                    context.Playero.Remove(playero);

                context.SaveChanges();
            }

            // Recarga la grilla
            CargarGrilla();
        }
    }
}