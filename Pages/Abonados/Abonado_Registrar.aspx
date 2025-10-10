<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Abonado_Registrar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Abonados.Abonado_Registrar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Panel ID="pnlFormulario" runat="server" CssClass="formulario-abonado">
        <h2>
            <asp:Label ID="lblTitulo" runat="server" Text="Registrar un Abonado" />
        </h2>

        <hr />

        <!-- CUIL / CUIT -->
        <div class="form-group form-inline">
            <label for="txtCuilCuit">CUIL / CUIT:</label>
            <asp:TextBox ID="txtCuilCuit" runat="server" CssClass="form-control" />
            <asp:CustomValidator ID="cvCuilCuit" runat="server"
                OnServerValidate="cvCuilCuit_ServerValidate"
                ErrorMessage=""
                Display="Dynamic"
                ForeColor="Red"
                ValidationGroup="Abonado" />
        </div>

        <!-- Nombre -->
        <div class="form-group form-inline">
            <label for="txtNombre">Nombre:</label>
            <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" />
            <asp:CustomValidator ID="cvNombre" runat="server"
                OnServerValidate="cvNombre_ServerValidate"
                ErrorMessage=""
                Display="Dynamic"
                ForeColor="Red"
                ValidationGroup="Abonado" />

            <!-- Apellido -->
            <label for="txtApellido">Apellido:</label>
            <asp:TextBox ID="txtApellido" runat="server" CssClass="form-control" />
            <asp:CustomValidator ID="cvApellido" runat="server"
                OnServerValidate="cvApellido_ServerValidate"
                ErrorMessage=""
                Display="Dynamic"
                ForeColor="Red"
                ValidationGroup="Abonado" />
        </div>

        <!-- Teléfono -->
        <div class="form-group form-inline">
            <label for="txtTelefono">Teléfono:</label>
            <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control" />
            <asp:CustomValidator ID="cvTelefono" runat="server"
                OnServerValidate="cvTelefono_ServerValidate"
                ErrorMessage=""
                Display="Dynamic"
                ForeColor="Red"
                ValidationGroup="Abonado" />
        </div>

        <hr />

        <!-- ====================== DATOS DEL VEHÍCULO ====================== -->

        <div class="form-group form-inline">
            <!-- Patente -->
            <label for="txtPatente">Patente:</label>
            <asp:TextBox ID="txtPatente" runat="server" CssClass="form-control" />
            <asp:CustomValidator ID="cvPatente" runat="server"
                OnServerValidate="cvPatente_ServerValidate"
                ErrorMessage=""
                Display="Dynamic"
                ForeColor="Red"
                ValidationGroup="Abonado" />

            <!-- Categoría del Vehículo -->
            <label for="ddlCategoriaVehiculo">Categoría del Vehículo:</label>
            <asp:DropDownList ID="ddlCategoriaVehiculo" runat="server" CssClass="form-control"
                AutoPostBack="true"
                OnSelectedIndexChanged="ddlCategoriaVehiculo_SelectedIndexChanged" />
            <asp:CustomValidator ID="cvCategoriaVehiculo" runat="server"
                OnServerValidate="cvCategoriaVehiculo_ServerValidate"
                ErrorMessage=""
                Display="Dynamic"
                ForeColor="Red"
                ValidationGroup="Abonado" />
        </div>

        <hr />

        <!-- ====================== TARIFA DEL ABONO ====================== -->

        <div class="form-group form-inline">
            <!-- Tipo de Abono -->
            <label for="ddlTipoAbono">Tipo de Abono:</label>
            <asp:DropDownList ID="ddlTipoAbono" runat="server" CssClass="form-control"
                AutoPostBack="true" OnSelectedIndexChanged="ddlTipoAbono_SelectedIndexChanged" />
            <asp:CustomValidator ID="cvTipoAbono" runat="server"
                OnServerValidate="cvTipoAbono_ServerValidate"
                Display="Dynamic"
                ForeColor="Red"
                ValidationGroup="Abonado" />

            <!-- Precio (calculado, solo lectura) -->
            <label for="lblPrecio" class="ml-1">Precio:</label>
            <asp:Label ID="lblPrecio" runat="server"
                CssClass="font-weight-bold text-primary d-inline-block"
                Style="font-size: 1.8rem; min-width: 60px;" />
        </div>

        <!-- Cantidad de tiempo -->
        <div class="form-group form-inline">
            <label for="txtCantidadTiempo">Cantidad de tiempo:</label>
            <asp:TextBox ID="txtCantidadTiempo" runat="server" CssClass="form-control"
                AutoPostBack="true"
                OnTextChanged="txtCantidadTiempo_TextChanged" />
            <asp:CustomValidator ID="cvCantidadTiempo" runat="server"
                OnServerValidate="cvCantidadTiempo_ServerValidate"
                Display="Dynamic"
                ForeColor="Red"
                ValidationGroup="Abonado" />
        </div>

        <hr />

        <!-- ====================== PLAZA ====================== -->

        <div class="form-group form-inline">
            <label for="ddlPlaza">Plaza:</label>
            <asp:DropDownList ID="ddlPlaza" runat="server" CssClass="form-control" />
            <asp:CustomValidator ID="cvPlaza" runat="server"
                OnServerValidate="cvPlaza_ServerValidate"
                Display="Dynamic"
                ForeColor="Red"
                ValidationGroup="Abonado" />
        </div>

        <hr />

        <!-- ====================== FECHA DE INICIO Y FIN ====================== -->

        <div class="form-group form-inline">
            <!-- Desde -->
            <label for="txtDesde">Desde:</label>
            <asp:TextBox ID="txtDesde" runat="server" CssClass="form-control"
                TextMode="DateTimeLocal"
                AutoPostBack="true"
                OnTextChanged="txtDesde_TextChanged" />

            <asp:CustomValidator ID="cvDesde" runat="server"
                OnServerValidate="cvDesde_ServerValidate"
                ErrorMessage="Debe seleccionar una fecha y hora válidas y mayor o igual a ahora."
                Display="Dynamic"
                ForeColor="Red"
                ValidationGroup="Abonado" />


            <!-- Hasta visible al usuario -->
            <label>Hasta:</label>
            <asp:Label ID="lblHasta" runat="server" CssClass="font-weight-bold text-primary d-inline-block"
                Style="font-size: 1.8rem; min-width: 60px;" />

            <!-- Hasta oculto para enviar al servidor -->
            <asp:TextBox ID="txtHasta" runat="server" CssClass="d-none" />
        </div>

        <hr />

        <!-- ====================== MONTO A PAGAR ====================== -->

        <!-- Total a pagar -->
        <div class="total-container">
            <span class="total-text">Total a pagar: $</span>
            <asp:Label ID="lblTotal" runat="server" CssClass="total-valor" />
        </div>

        <br />
        <!-- Método de Pago -->
        <div class="form-group form-inline">
            <label for="ddlMetodoPago">Método de Pago:</label>
            <asp:DropDownList ID="ddlMetodoPago" runat="server" CssClass="form-control" />
            <asp:CustomValidator ID="cvMetodoPago" runat="server"
                OnServerValidate="cvMetodoPago_ServerValidate"
                Display="Dynamic"
                ForeColor="Red"
                ValidationGroup="Abonado" />
        </div>

        <br />

        <!-- ====================== BOTONES ====================== -->
        <div class="form-group">
            <asp:Button ID="btnCancelar" runat="server" Text="Cancelar"
                OnClick="btnCancelar_Click"
                CausesValidation="False" CssClass="btn btn-danger" />

            <asp:Button ID="btnGuardar" runat="server" Text="Guardar"
                CssClass="btn btn-primary"
                ValidationGroup="Abonado"
                OnClick="btnGuardar_Click" />
        </div>
    </asp:Panel>

</asp:Content>
