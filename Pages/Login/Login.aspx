<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Login.Login" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Iniciar Sesión</title>
    <style>
        /* Fondo animado del body */
        body {
            animation: fondoAnimado 12s infinite alternate ease-in-out;
            font-family: 'Poppins', sans-serif;
            margin: 0;
            padding: 0;
        }

        @keyframes fondoAnimado {
            0% {
                background-color: #2e8b57;
            }
            /* verde seco (SeaGreen) */
            25% {
                background-color: #2f4f4f;
            }
            /* gris verdoso (DarkSlateGray) */
            50% {
                background-color: #006400;
            }
            /* verde oscuro */
            75% {
                background-color: #013220;
            }
            /* verde muy oscuro */
            100% {
                background-color: #000000;
            }
            /* negro */
        }

        .login-container {
            height: 520px;
            width: 400px;
            background-color: rgba(255, 255, 255, 0.13);
            position: absolute;
            transform: translate(-50%, -50%);
            top: 50%;
            left: 50%;
            border-radius: 10px;
            backdrop-filter: blur(30px);
            border: 2px solid rgba(255, 255, 255, 0.1);
            box-shadow: 0 0 40px rgba(8, 7, 16, 0.6);
            padding: 50px 35px;
        }

            .login-container * {
                color: #ffffff;
                letter-spacing: 0.5px;
                outline: none;
                border: none;
            }

            .login-container h2 {
                text-align: center;
                font-size: 42px;
                font-weight: 500;
                line-height: 42px;
            }

        label {
            display: block;
            margin-top: 30px;
            font-size: 26px;
            font-weight: 500;
        }

        .form-control {
            display: block;
            height: 50px;
            width: 100%;
            background-color: rgba(255, 255, 255, 0.07);
            border-radius: 3px;
            padding: 0 10px;
            margin-top: 8px;
            font-size: 22px;
            font-weight: 300;
            color: #ffffff;
        }

        ::placeholder {
            color: #e5e5e5;
        }

        .btn {
            margin-top: 50px;
            width: 100%;
            background-color: #ffffff;
            color: #080710;
            padding: 15px 0;
            font-size: 26px;
            font-weight: 600;
            border-radius: 5px;
            cursor: pointer;
        }

            .btn:hover {
                background-color: #dddddd;
            }

        .error-label {
            margin-top: 10px;
            color: red;
            font-weight: bold;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-container">
            <h2>Iniciar Sesión</h2>

            <label for="txtUsuario">Usuario</label>
            <asp:TextBox ID="txtUsuario" runat="server" CssClass="form-control" placeholder="Ingrese su usuario" />

            <label for="txtClave">Contraseña</label>
            <asp:TextBox ID="txtClave" runat="server" TextMode="Password" CssClass="form-control" placeholder="Ingrese su contraseña" />

            <asp:Label ID="lblMensaje" runat="server" CssClass="error-label"></asp:Label>

            <asp:Button ID="btnLogin" runat="server" Text="Ingresar" CssClass="btn" OnClick="btnLogin_Click" />
        </div>
    </form>
</body>
</html>
