<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Turno_Listar.aspx.cs" Inherits="Proyecto_Estacionamiento.Pages.Turnos.Turno_Listar" %>



<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Lista de Turnos</h2>

    <asp:GridView ID="GridViewTurnos" runat="server" AutoGenerateColumns="False" Width="100%" CssClass="table table-bordered" CellPadding="4" ForeColor="#333333" GridLines="None">
        <AlternatingRowStyle BackColor="White" />
        <Columns>

            <asp:BoundField DataField="Estacionamiento"
                HeaderText="Estacionamiento" />

            <asp:TemplateField HeaderText="Playero">
                <ItemTemplate>
                    <%# Eval("ApellidoPlayero") %>, <%# Eval("NombrePlayero") %>
                </ItemTemplate>
            </asp:TemplateField>

            <asp:BoundField DataField="Turno_FechaHora_Inicio"
                HeaderText="Inicio de Turno"
                DataFormatString="{0:dd/MM/yyyy HH:mm}"
                HtmlEncode="false" />

            <asp:BoundField DataField="Turno_FechaHora_fin"
                HeaderText="Fin de Turno"
                DataFormatString="{0:dd/MM/yyyy HH:mm}"
                HtmlEncode="false" />

            <asp:BoundField DataField="Caja_Monto_Inicio"
                HeaderText="Monto Inicio"
                DataFormatString="{0:C}"
                HtmlEncode="false" />

            <asp:BoundField DataField="Caja_Monto_fin"
                HeaderText="Monto Fin"
                DataFormatString="{0:C}"
                HtmlEncode="false" />

            <asp:BoundField DataField="Caja_Monto_total"
                HeaderText="Total"
                DataFormatString="{0:C}"
                HtmlEncode="false" />

        </Columns>
        <EditRowStyle BackColor="#7C6F57" />
        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#009900" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center" />
        <RowStyle BackColor="#E3EAEB" />
        <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333" />
        <SortedAscendingCellStyle BackColor="#F8FAFA" />
        <SortedAscendingHeaderStyle BackColor="#246B61" />
        <SortedDescendingCellStyle BackColor="#D4DFE1" />
        <SortedDescendingHeaderStyle BackColor="#15524A" />
    </asp:GridView>

            <br />
            <br />
    <asp:Button ID="btnInicioTurno" runat="server" Text="Inicio de Turno" OnClick="btnInicioTurno_Click" />
    <asp:Button ID="btnFinTurno" runat="server" Text="Fin de Turno" OnClick="btnFinTurno_Click" />

</asp:Content>
