using System;
using System.Data.SqlClient;
using System.Configuration;
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

                    // Redirigir a la página por defecto
                    FormsAuthentication.RedirectFromLoginPage(legajoStr, false);
                }
                else
                {
                    lblMensaje.Text = "Legajo o Contraseña incorrectos.";
                }
            }
        }
    }
}
