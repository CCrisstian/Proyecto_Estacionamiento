﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Proyecto_Estacionamiento.Pages.Tarifas
{
    public partial class Tarifa_Crear_Editar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {   // Cargar los datos necesarios para los dropdowns y campos de texto
                CargarEstacionamientos();
                CargarTiposTarifa();
                CargarCategorias();

                if (Request.QueryString["id"] != null)
                {
                    lblTitulo.Text = "Editar Tarifa";
                    int id = int.Parse(Request.QueryString["id"]);
                    CargarTarifa(id);
                }
                else
                {
                    lblTitulo.Text = "Agregar Tarifa";
                    txtTarifaDesde.Text = "";
                }
            }
        }

        // Método para cargar los datos de la Tarifa si se está Editando
        private void CargarTarifa(int id)
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                var tarifa = db.Tarifa.FirstOrDefault(t => t.Tarifa_id == id);
                if (tarifa != null)
                {
                    ddlEstacionamientos.SelectedValue = tarifa.Est_id?.ToString() ?? "";
                    ddlTiposTarifa.SelectedValue = tarifa.Tipos_Tarifa_Id?.ToString() ?? "";
                    ddlCategorias.SelectedValue = tarifa.Categoria_id?.ToString() ?? "";
                    txtTarifaMonto.Text = tarifa.Tarifa_Monto.ToString("0.##");
                    txtTarifaDesde.Text = tarifa.Tarifa_Desde.ToString("yyyy-MM-dd");
                }
            }
        }

        // Método para cargar Estacionamiento en los dropdowns
        private void CargarEstacionamientos()
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                ddlEstacionamientos.DataSource = db.Estacionamiento.ToList();               // Obtener la lista de Estacionamientos
                ddlEstacionamientos.DataTextField = "Est_nombre";                           // Campo que se mostrará en el dropdown
                ddlEstacionamientos.DataValueField = "Est_id";                              // Campo que se usará como valor del dropdown
                ddlEstacionamientos.DataBind();                                             // Vincular la lista al dropdown
                ddlEstacionamientos.Items.Insert(0, new ListItem("-- Seleccione --", ""));  // Insertar un item por defecto
            }
        }

        // Método para cargar Tipos de Tarifa en el dropdown
        private void CargarTiposTarifa()
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                ddlTiposTarifa.DataSource = db.Tipos_Tarifa.ToList();
                ddlTiposTarifa.DataTextField = "Tipos_tarifa_descripcion";
                ddlTiposTarifa.DataValueField = "Tipos_tarifa_id";
                ddlTiposTarifa.DataBind();
                ddlTiposTarifa.Items.Insert(0, new ListItem("-- Seleccione --", ""));
            }
        }

        // Método para cargar Categorías de Vehículo en el dropdown
        private void CargarCategorias()
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                ddlCategorias.DataSource = db.Categoria_Vehiculo.ToList();
                ddlCategorias.DataTextField = "Categoria_descripcion";
                ddlCategorias.DataValueField = "Categoria_id";
                ddlCategorias.DataBind();
                ddlCategorias.Items.Insert(0, new ListItem("-- Seleccione --", ""));
            }
        }

        // Evento para mostrar/ocultar el calendario al hacer clic en el botón
        protected void btnMostrarCalendario_Click(object sender, EventArgs e)
        {
            calTarifaDesde.Visible = !calTarifaDesde.Visible;
        }

        // Evento para manejar la selección de una fecha en el calendario
        protected void calTarifaDesde_SelectionChanged(object sender, EventArgs e)
        {
            txtTarifaDesde.Text = calTarifaDesde.SelectedDate.ToString("yyyy-MM-dd");
            calTarifaDesde.Visible = false;
        }


        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            using (var db = new ProyectoEstacionamientoEntities())
            {
                Tarifa tarifa = new Tarifa  // Crear una nueva instancia de Tarifa
                {
                    // Asignar los valores de los campos del formulario a la nueva Tarifa
                    Est_id = int.Parse(ddlEstacionamientos.SelectedValue),
                    Tipos_Tarifa_Id = int.Parse(ddlTiposTarifa.SelectedValue),
                    Categoria_id = int.Parse(ddlCategorias.SelectedValue),
                    Tarifa_Monto = (double)decimal.Parse(txtTarifaMonto.Text),
                    Tarifa_Desde = DateTime.Parse(txtTarifaDesde.Text)
                };
                if (Request.QueryString["id"] != null)  // Verificar si se está Editando una Tarifa existente
                {
                    int id = int.Parse(Request.QueryString["id"]);
                    tarifa.Tarifa_id = id;
                    db.Entry(tarifa).State = System.Data.Entity.EntityState.Modified;   // Actualizar la Tarifa existente
                }
                else
                {
                    db.Tarifa.Add(tarifa);  // Agregar una nueva Tarifa
                }
                db.SaveChanges();   // Guardar los cambios en la Base de Datos
            }
            Response.Redirect("Tarifa_Listar.aspx");
        }
        protected void btnCancelar_Click(object sender, EventArgs e)
        {

            Response.Redirect("Tarifa_Listar.aspx");
        }

    }
}