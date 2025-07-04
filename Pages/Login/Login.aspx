<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Login.Login" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Iniciar Sesión</title>
    <style>
        body {
            font-family: Arial;
            background-color: #f2f2f2;
            padding: 50px;
        }
        .login-container {
            width: 400px;
            margin: auto;
            background-color: white;
            padding: 25px;
            border-radius: 10px;
            box-shadow: 0 0 10px #ccc;
        }
        .login-container h2 {
            text-align: center;
        }
        .form-control {
            padding: 10px;
            margin-top: 8px;
            margin-bottom: 16px;
        }
        .btn {
            border-style: none;
            border-color: inherit;
            border-width: medium;
            padding: 10px;
            background-color: #007acc;
            color: white;
            cursor: pointer;
            margin-left: 110px;
        }
        .btn:hover {
            background-color: #005f99;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-container">
            <h2>Iniciar Sesión</h2>
            <label for="txtUsuario">Usuario</label>
            <br />
            <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" Width="318px" />

            <label for="txtClave">Contraseña</label>
            <br />
            <asp:TextBox ID="txtClave" runat="server" TextMode="Password" CssClass="form-control" Width="317px" />
            <br />
            <asp:Button ID="btnLogin" runat="server" Text="Ingresar" CssClass="btn" OnClick="btnLogin_Click" Width="166px" />
            <br />
            <asp:Label ID="lblMensaje" runat="server" ForeColor="Red"></asp:Label>
        </div>
    </form>
</body>
</html>