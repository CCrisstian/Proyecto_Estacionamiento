using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Data.Entity;

namespace Proyecto_Estacionamiento.Pages.Incidencia
{
    public partial class Incidencia_Descargar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // 1. Obtenemos los parámetros de la URL
            if (!int.TryParse(Request.QueryString["legajo"], out int legajo) ||
                !long.TryParse(Request.QueryString["fechaTicks"], out long fechaTicks))
            {
                Response.Write("Error: Parámetros de reporte inválidos.");
                Response.End();
                return;
            }

            DateTime fechaHora = new DateTime(fechaTicks);

            // 2. Llamamos la lógica de generación de PDF
            GenerarPdfIncidencia(legajo, fechaHora);
        }

        private void GenerarPdfIncidencia(int legajo, DateTime fechaHora)
        {
            try
            {
                // 3. OBTENER DATOS COMPLETOS
                Incidencias incidencia;
                using (var db = new ProyectoEstacionamientoEntities())
                {
                    incidencia = db.Incidencias
                        .Include("Playero")
                        .Include("Playero.Usuarios")
                        .Include("Playero.Estacionamiento")
                        .FirstOrDefault(i =>
                            i.Playero_legajo == legajo &&
                            DbFunctions.TruncateTime(i.Inci_fecha_Hora) == fechaHora.Date &&
                            i.Inci_fecha_Hora.Hour == fechaHora.Hour &&
                            i.Inci_fecha_Hora.Minute == fechaHora.Minute &&
                            i.Inci_fecha_Hora.Second == fechaHora.Second
                        );
                }

                if (incidencia == null)
                {
                    Response.Write("Error: Incidencia no encontrada.");
                    Response.End();
                    return;
                }

                // 4. CREAR DOCUMENTO PDF EN MEMORIA
                using (MemoryStream ms = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    document.Open();

                    // --- FUENTES ---
                    var fTituloBlanco = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24, BaseColor.WHITE);
                    var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                    var fontSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                    var fontBody = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                    var fontBodyBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                    var fontInfo = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 10);

                    // --- ENCABEZADO ---
                    PdfPTable headerTable = new PdfPTable(2);
                    headerTable.WidthPercentage = 100;
                    // Ajustamos anchos: columna logo pequeña, columna título grande
                    headerTable.SetWidths(new float[] { 1f, 4f });

                    // 1. Celda del LOGO
                    string imagePath = Server.MapPath("~/Images/LogoACE_SinFondo.PNG"); // Ruta relativa del servidor
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(imagePath);
                    logo.ScaleToFit(60f, 60f); // Ajustar tamaño

                    PdfPCell cellLogo = new PdfPCell(logo);
                    cellLogo.Border = PdfPCell.NO_BORDER;
                    cellLogo.BackgroundColor = new BaseColor(50, 160, 65); // Verde
                    cellLogo.HorizontalAlignment = Element.ALIGN_CENTER;
                    cellLogo.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellLogo.Padding = 10f;
                    headerTable.AddCell(cellLogo);

                    // 2. Celda del TÍTULO
                    PdfPCell cellTitulo = new PdfPCell(new Phrase("Reporte de Incidencia", fTituloBlanco));
                    cellTitulo.Border = PdfPCell.NO_BORDER;
                    cellTitulo.BackgroundColor = new BaseColor(50, 160, 65); // Verde
                    cellTitulo.HorizontalAlignment = Element.ALIGN_CENTER;
                    cellTitulo.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellTitulo.Padding = 20f; // Espacio vertical
                    headerTable.AddCell(cellTitulo);
                    headerTable.SpacingAfter = 20f;

                    document.Add(headerTable);

                    void AgregarLinea(string etiqueta, string valor)
                    {
                        var p = new Paragraph();
                        p.Add(new Chunk(etiqueta + ": ", fontBodyBold)); // Etiqueta en Negrita
                        p.Add(new Chunk(valor, fontBody));             // Valor Normal
                        document.Add(p);
                    }
                    // --------------------------------------

                    // Usamos la función para agregar las líneas
                    AgregarLinea("Estacionamiento", incidencia.Playero.Estacionamiento.Est_nombre);
                    AgregarLinea("Fecha y Hora", $"{incidencia.Inci_fecha_Hora:dd/MM/yyyy HH:mm}");
                    AgregarLinea("Playero", $"{incidencia.Playero.Usuarios.Usu_ap}, {incidencia.Playero.Usuarios.Usu_nom}");

                    document.Add(Chunk.NEWLINE);
                    
                    document.Add(new Paragraph("Motivo de la Incidencia:", fontSubtitulo));
                    document.Add(new Paragraph(incidencia.Inci_Motivo, fontBody));
                    
                    document.Add(Chunk.NEWLINE);
                    
                    document.Add(new Paragraph("Descripción Detallada:", fontSubtitulo));
                    document.Add(new Paragraph(incidencia.Inci_descripcion, fontBody));

                    document.Close();
                    writer.Close();

                    // 5. ENVIAR EL PDF AL NAVEGADOR
                    Response.Clear();
                    Response.ContentType = "application/pdf";
                    string nombreArchivo = $"Incidencia_{incidencia.Playero.Usuarios.Usu_ap}_{fechaHora:yyyyMMddHHmm}.pdf";
                    Response.AddHeader("content-disposition", "attachment;filename=" + nombreArchivo);
                    Response.Buffer = true;
                    Response.BinaryWrite(ms.ToArray());
                    Response.Flush();
                    Response.SuppressContent = true;
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al generar PDF: " + ex.Message);
                Response.Write("Error al generar el PDF: " + ex.Message);
                Response.Write(fechaHora);
                Response.End();
            }
        }
    }
}