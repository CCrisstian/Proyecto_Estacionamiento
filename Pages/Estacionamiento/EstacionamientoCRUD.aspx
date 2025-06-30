<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EstacionamientoCRUD.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Estacionamiento.EstacionamientoCRUD" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Gestión de Estacionamientos</title>
    <style>
        .formulario {
            background-color: #f7f7f7;
            border: 1px solid #ccc;
            padding: 15px;
            margin-bottom: 20px;
            width: 500px;
        }

        label {
            display: block;
            margin-top: 10px;
        }

        input, select {
            width: 100%;
            padding: 6px;
        }

        .botones {
            margin-top: 15px;
        }

            .botones input {
                margin-right: 10px;
            }
        .grid {}
    </style>
</head>
<body>

    <form id="form1" runat="server">
        <div class="botones">
            <asp:Button ID="btnCrear" runat="server" Text="Crear nuevo estacionamiento" OnClick="btnCrear_Click" CssClass="btn" Width="189px" />
        </div>

        <asp:GridView ID="gvEstacionamientos" runat="server" AutoGenerateColumns="False" DataKeyNames="Est_id"
            CssClass="grid" Width="800px">
            <Columns>
                
                <asp:BoundField DataField="Est_nombre" HeaderText="Nombre" />
                <asp:BoundField DataField="Est_provincia" HeaderText="Provincia" />
                <asp:BoundField DataField="Est_localidad" HeaderText="Localidad" />
                <asp:BoundField DataField="Est_direccion" HeaderText="Dirección" />
                <asp:BoundField DataField="Est_horario" HeaderText="Horario" />
                <asp:BoundField DataField="Est_puntaje" HeaderText="Puntaje" />

                <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />

            </Columns>
        </asp:GridView>
    </form>

</body>
</html>
