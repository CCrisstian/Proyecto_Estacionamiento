using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Security;

namespace Proyecto_Estacionamiento.Pages.Login
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Session.Clear(); // Limpia la sesión previa
            }
        }

        public class EstacionamientoDTO
        {
            public int Est_id { get; set; }
            public string Est_nombre { get; set; }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // Recibe legajo como string (desde TextBox)
            string legajoStr = txtUsuario.Text.Trim();
            string pass = txtClave.Text.Trim();

            // Validación opcional: asegurarse de que sea número
            if (!int.TryParse(legajoStr, out int legajo))
            {
                lblMensaje.Text = "El legajo debe ser numérico.";
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["MiConexionSQL"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = "SELECT Usu_nom, Usu_ap, Usu_tipo FROM Usuarios WHERE Usu_legajo = @legajo AND Usu_pass = @pass";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@legajo", legajo);
                cmd.Parameters.AddWithValue("@pass", pass);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string nombre = reader.GetString(0);
                    string apellido = reader.GetString(1);
                    string tipo = reader.GetString(2);

                    // Guardar en sesión
                    Session["Usu_legajo"] = legajo;
                    Session["Usu_nombre"] = $"{nombre} {apellido}";
                    Session["Usu_tipo"] = tipo;

                    // Autenticación con Forms
                    FormsAuthentication.SetAuthCookie(legajoStr, false);

                    // 🔀 Redirigir según tipo
                    if (tipo.Equals("Dueño", StringComparison.OrdinalIgnoreCase))
                    {
                        reader.Close();

                        string queryEst = @"SELECT Est_id, Est_nombre 
                        FROM Estacionamiento 
                        WHERE Dueño_Legajo = @legajo";

                        SqlCommand cmdEst = new SqlCommand(queryEst, conn);
                        cmdEst.Parameters.AddWithValue("@legajo", legajo);

                        List<EstacionamientoDTO> estacionamientos = new List<EstacionamientoDTO>();

                        using (SqlDataReader readerEst = cmdEst.ExecuteReader())
                        {
                            while (readerEst.Read())
                            {
                                estacionamientos.Add(new EstacionamientoDTO
                                {
                                    Est_id = readerEst.GetInt32(0),
                                    Est_nombre = readerEst.GetString(1)
                                });
                            }
                        }

                        // Guardamos en sesión
                        Session["EstacionamientosDueño"] = estacionamientos;

                        // ✅ Validar si el dueño NO tiene estacionamientos
                        if (estacionamientos.Count == 0)
                        {
                            // Redirigir a la página para crear su primer estacionamiento
                            Response.Redirect("~/Pages/Estacionamiento/Estacionamiento_CrearEditar.aspx");
                        }
                        else
                        {
                            // Caso normal: ya tiene estacionamientos
                            Response.Redirect("~/Pages/Default/Inicio.aspx");
                        }
                    }
                    else
                    {
                        // Cerramos el reader anterior
                        reader.Close();

                        string queryEst = @"SELECT e.Est_id, e.Est_nombre 
                        FROM Playero p 
                        INNER JOIN Estacionamiento e ON p.Est_id = e.Est_id
                        WHERE p.Playero_legajo = @legajo";

                        SqlCommand cmdEst = new SqlCommand(queryEst, conn);
                        cmdEst.Parameters.AddWithValue("@legajo", legajo);

                        using (SqlDataReader readerEst = cmdEst.ExecuteReader())
                        {
                            if (readerEst.Read())
                            {
                                // Guardamos en sesión el ID y el nombre del estacionamiento para el Playero
                                Session["Playero_EstId"] = readerEst.GetInt32(0);
                                Session["Usu_estacionamiento"] = readerEst.GetString(1);
                            }
                        }

                        Response.Redirect("~/Pages/Ingresos/Ingreso_Listar.aspx");
                    }

                }
                else
                {
                    lblMensaje.Text = "Legajo o Contraseña incorrectos.";
                }
            }
        }
    }
}
