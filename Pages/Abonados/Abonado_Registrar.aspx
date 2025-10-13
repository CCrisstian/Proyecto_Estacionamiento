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

            <label for="txtDNI">DNI:</label>
            <asp:TextBox ID="TextDNI" runat="server" CssClass="form-control" />
            <asp:CustomValidator ID="cvDNI" runat="server"
                OnServerValidate="cvDNI_ServerValidate"
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
            <!-- Patentes -->
            <label for="txtPatente">Patente(s):</label>
            <asp:TextBox ID="txtPatente" runat="server" CssClass="form-control" placeholder="Clic para agregar patentes" />

            <button id="btnAbrirModalPatentes" type="button" class="btn btn-info btn-sm ml-2">Administrar Patentes</button>

            <asp:CustomValidator ID="cvPatente" runat="server"
                OnServerValidate="cvPatente_ServerValidate"
                ErrorMessage="Debe ingresar al menos una patente."
                Display="Dynamic"
                ForeColor="Red"
                ValidationGroup="Abonado" />

            <!-- Modal para ingresar varias patentes -->
            <div class="modal fade" id="modalPatentes" tabindex="-1" role="dialog" aria-labelledby="modalPatentesLabel" aria-hidden="true">
                <div class="modal-dialog" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="modalPatentesLabel">Ingresar Patentes</h5>
                            <button type="button" class="close" data-dismiss="modal" aria-label="Cerrar">
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>
                        <div class="modal-body">
                            <table class="table" id="tablaPatentes">
                                <thead>
                                    <tr>
                                        <th>Patente</th>
                                        <th style="width: 50px;">
                                            <button type="button" class="btn btn-primary btn-sm" onclick="agregarFilaPatente()">+</button>
                                        </th>
                                    </tr>
                                </thead>
                                <tbody>
                                </tbody>
                            </table>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-primary" onclick="guardarPatentes()">Guardar</button>
                            <button type="button" class="btn btn-danger" data-dismiss="modal">Cancelar</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
        <script src="https://cdn.jsdelivr.net/npm/bootstrap@4.6.2/dist/js/bootstrap.bundle.min.js"></script>

        <script>
            // Variable auxiliar que usa la sintaxis ASP.NET para obtener el ID real del TextBox
            var patenteTextBoxID = '<%= txtPatente.ClientID %>';

            // Función auxiliar para crear una fila de patente (con botón de eliminar y valor opcional)
            function crearFilaPatente(valor = '') {
                return '<tr>' +
                    '<td><input type="text" class="form-control patente-input" value="' + valor + '" /></td>' +
                    '<td><button type="button" class="btn btn-danger btn-sm" onclick="eliminarFilaPatente(this)">X</button></td>' +
                    '</tr>';
            }

            // 1. Añade una fila nueva y vacía
            function agregarFilaPatente() {
                $('#tablaPatentes tbody').append(crearFilaPatente());
            }

            // 2. Elimina una fila específica
            function eliminarFilaPatente(elementoBoton) {
                $(elementoBoton).closest('tr').remove();
            }

            // 3. Carga las patentes existentes en el modal al abrir (Precarga/Edición)
            function cargarPatentesModal() {
                var patenteTextBox = $('#' + patenteTextBoxID);
                var patentesString = patenteTextBox.val();
                var tbody = $('#tablaPatentes tbody');
                tbody.empty(); // Limpia la tabla antes de cargar

                if (patentesString) {
                    var patentesArray = patentesString.split(',').map(item => item.trim()).filter(item => item.length > 0);
                    patentesArray.forEach(function (patente) {
                        tbody.append(crearFilaPatente(patente));
                    });
                }

                // Si no hay patentes, agrega una fila vacía para empezar
                if (tbody.children().length === 0) {
                    agregarFilaPatente();
                }
            }

            // 4. Guarda los datos en el TextBox principal
            function guardarPatentes() {
                var patentes = [];
                $('.patente-input').each(function () {
                    var val = $(this).val().trim();
                    if (val) patentes.push(val);
                });

                $('#' + patenteTextBoxID).val(patentes.join(', '));
                $('#modalPatentes').modal('hide');
            }

            // 5. Inicialización de Eventos (Se ejecuta al cargar la página)
            $(document).ready(function () {
                // Asigna el evento de apertura del modal a ambos elementos (TextBox y Botón)
                $('#' + patenteTextBoxID + ', #btnAbrirModalPatentes').on('click', function (e) {
                    e.preventDefault(); // Previene el comportamiento por defecto
                    $('#modalPatentes').modal('show');
                });

                // Evento de Bootstrap: se ejecuta ANTES de que el modal se muestre
                $('#modalPatentes').on('show.bs.modal', function () {
                    cargarPatentesModal();
                });
            });
        </script>

        <!-- Categoría del Vehículo -->
        <div class="form-group form-inline">
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

        <asp:Label ID="lblError" runat="server" ForeColor="Red" Font-Bold="true" EnableViewState="false"></asp:Label>
    </asp:Panel>

</asp:Content>
