using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using static Proyecto_Estacionamiento.Pages.Metodos_De_Pago.MetodosDePago_Listar;

namespace Proyecto_Estacionamiento.Pages.Playeros
{
    public partial class Playero_Listar : System.Web.UI.Page
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
            string tipoUsuario = Session["Usu_tipo"] as string;
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                IQueryable<Playero> query = db.Playero;

                var estIdList = db.Estacionamiento
                                   .Where(e => e.Dueño_Legajo == legajo)
                                   .Select(e => e.Est_id);

                query = query.Where(pl => pl.Est_id.HasValue && estIdList.Contains(pl.Est_id.Value));

                var playeros = query
                    .Select(p => new
                    {
                        Est_nombre = p.Estacionamiento.Est_nombre,
                        Playero_legajo = p.Playero_legajo,
                        Usu_dni = p.Usuarios.Usu_dni ?? 0,
                        Usu_pass = p.Usuarios.Usu_pass,
                        Usu_ap = p.Usuarios.Usu_ap,
                        Usu_nom = p.Usuarios.Usu_nom,
                        Playero_activo = p.Playero_activo
                    })
                    .OrderBy(p => p.Est_nombre)
                    .ToList();

                gvPlayeros.DataSource = playeros;
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
    }
}