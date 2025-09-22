﻿<%@ Page Title="Plaza" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Plaza_Crear_Editar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Plaza.Plaza_Crear_Editar" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <asp:Panel ID="pnlFormulario" runat="server" CssClass="formulario-plaza">

        <h2>
            <asp:Label ID="lblTitulo" runat="server" Text="Agregar un Estacionamiento" />
        </h2>
        
        <br />

        <div class="form-group form-inline">
            <label for="ddlEstacionamiento">Estacionamiento:</label>
            <asp:DropDownList ID="ddlEstacionamiento" runat="server" CssClass="form-control" />
            <asp:CustomValidator ID="cvEstacionamiento" runat="server"
                OnServerValidate="cvEstacionamiento_ServerValidate"
                ErrorMessage="Debe seleccionar un Estacionamiento."
                Display="Dynamic" ForeColor="Red" ValidationGroup="Plaza" />
        </div>

        <div class="form-group form-inline">
            <label for="ddlCategoria">Categoría de Vehículo:</label>
            <asp:DropDownList ID="ddlCategoria" runat="server" CssClass="form-control" />
            <asp:CustomValidator ID="cvCategoria" runat="server"
                OnServerValidate="cvCategoria_ServerValidate"
                ErrorMessage="Debe seleccionar una Categoría."
                Display="Dynamic" ForeColor="Red" ValidationGroup="Plaza" />
        </div>

        <div class="form-group form-inline">
            <label for="txtNombre">Nombre:</label>
            <asp:TextBox ID="txtNombre" runat="server" MaxLength="100" CssClass="form-control" />
            <asp:CustomValidator ID="cvNombre" runat="server"
                OnServerValidate="cvNombre_ServerValidate"
                ErrorMessage="El nombre de la Plaza es obligatorio."
                Display="Dynamic" ForeColor="Red" ValidationGroup="Plaza" />
        </div>

        <div class="form-group form-inline">
            <label for="txtTipo">Tipo:</label>
            <asp:TextBox ID="txtTipo" runat="server" MaxLength="100" CssClass="form-control" />
            <asp:CustomValidator ID="cvTipo" runat="server"
                OnServerValidate="cvTipo_ServerValidate"
                ErrorMessage="El tipo de Plaza es obligatorio."
                Display="Dynamic" ForeColor="Red" ValidationGroup="Plaza" />
        </div>

        <div class="form-group form-inline">
            <asp:CheckBox ID="chkDisponibilidad" runat="server" Text="Disponible" />
        </div>

        <div class="form-group form-inline">
            <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="btnCancelar_Click" CausesValidation="false" CssClass="btn btn-danger" />

            <asp:Button ID="btnGuardar" runat="server" Text="Guardar"
                OnClientClick="return confirmarGuardado();"
                OnClick="btnGuardar_Click" CssClass="btn btn-primary" ValidationGroup="Plaza" />
        </div>

    </asp:Panel>

    <%-- SweetAlert2 para mensajes de confirmación --%>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script>
        function confirmarGuardado() {
            Swal.fire({
                title: "¿Deseás guardar los cambios?",
                showDenyButton: true,
                confirmButtonText: "Guardar",
                denyButtonText: "Cancelar",
                reverseButtons: true // 👈 Esto invierte el orden de los botones
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack('<%= btnGuardar.UniqueID %>', '');
                } else if (result.isDenied) {
                    Swal.fire("Los cambios no se guardaron", "", "info");
                }
            });
            return false;
        }
    </script>
</asp:Content>

