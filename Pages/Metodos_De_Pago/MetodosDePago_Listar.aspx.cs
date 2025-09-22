using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Metodos_De_Pago
{
    public partial class MetodosDePago_Listar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string tipoUsuario = Session["Usu_tipo"] as string;
                if (tipoUsuario != "Dueño")
                {
                    // Oculta los elementos si no es Dueño
                    btnAgregar.Visible = false;
                    gvMetodosPago.Columns[0].Visible = false;
                    gvMetodosPago.Columns[4].Visible = false;
                }
                else
                {
                    if (Session["Dueño_EstId"] != null)
                    {
                        gvMetodosPago.Columns[0].Visible = false;
                    }
                }

                string estacionamiento = Session["Usu_estacionamiento"] as string;

                if (!string.IsNullOrEmpty(estacionamiento))
                {
                    TituloMetodosDePago.Text = $"Métodos de Pago del Estacionamiento '<strong>{estacionamiento}</strong>'";
                }         
                else      
                {         
                    TituloMetodosDePago.Text = "Métodos de Pago";
                }

                CargarGrilla();
            }
        }

        private void CargarGrilla()
        {
            string tipoUsuario = Session["Usu_tipo"] as string;
            int legajo = Convert.ToInt32(Session["Usu_legajo"]);

            using (var db = new ProyectoEstacionamientoEntities())
            {
                IQueryable<Acepta_Metodo_De_Pago> query = db.Acepta_Metodo_De_Pago
                    .Include("Estacionamiento")
                    .Include("Metodos_De_Pago");

                if (tipoUsuario == "Dueño")
                {
                    if (Session["Dueño_EstId"] != null)
                    {
                        int estIdSeleccionado = (int)Session["Dueño_EstId"];
                        query = query.Where(amp => amp.Est_id == estIdSeleccionado);
                    }
                    else
                    {
                        var estIdList = db.Estacionamiento
                                           .Where(e => e.Dueño_Legajo == legajo)
                                           .Select(e => e.Est_id);

                        query = query.Where(amp => estIdList.Contains(amp.Est_id));
                    }
                }
                else if (tipoUsuario == "Playero")
                {
                    var estId = db.Playero
                                  .Where(p => p.Playero_legajo == legajo)
                                  .Select(p => p.Est_id)
                                  .FirstOrDefault();

                    if (estId.HasValue)
                    {
                        query = query.Where(amp => amp.Est_id == estId.Value);
                    }
                    else
                    {
                        query = query.Where(amp => false); // no mostrar resultados
                    }
                }

                var metodosDePago = query
                    .Select(a => new
                    {
                        Est_nombre = a.Estacionamiento.Est_nombre,
                        Metodo_pago_descripcion = a.Metodos_De_Pago.Metodo_pago_descripcion,
                        AMP_Desde = a.AMP_Desde,
                        AMP_Hasta = a.AMP_Hasta,
                        Est_id = a.Est_id,
                        Metodo_Pago_id = a.Metodo_Pago_id
                    })
                    .OrderBy(a => a.Est_nombre)
                    .ToList();

                gvMetodosPago.DataSource = metodosDePago;
                gvMetodosPago.DataBind();
            }
        }


        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            Response.Redirect("MetodosDePago_CrearEditar.aspx");
        }

        protected void gvMetodosPago_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Editar")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = gvMetodosPago.Rows[rowIndex];

                int estId = Convert.ToInt32(gvMetodosPago.DataKeys[rowIndex]["Est_id"]);
                int metodoId = Convert.ToInt32(gvMetodosPago.DataKeys[rowIndex]["Metodo_Pago_id"]);

                Response.Redirect($"MetodosDePago_CrearEditar.aspx?estId={estId}&metodoId={metodoId}");
            }
        }

        protected void gvMetodosPago_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string tipoUsuario = Session["Usu_tipo"] as string;

                // Si no es "Dueño", ocultar el botón Editar
                if (tipoUsuario != "Dueño")
                {
                    Button btnEditar = (Button)e.Row.FindControl("btnEditar");
                    if (btnEditar != null)
                    {
                        btnEditar.Visible = false;
                    }
                }
            }
        }

    }
}