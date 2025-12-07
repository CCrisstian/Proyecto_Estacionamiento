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

                    var fHeaderBlanco = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE); // Fuente Blanca


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


                    // -----------------------------------------------------------------
                    // 2. SEGUNDA CELDA: CONTENEDOR (FECHA + TÍTULO JUNTOS)
                    // -----------------------------------------------------------------
                    PdfPCell cellContenido = new PdfPCell();
                    cellContenido.Border = PdfPCell.NO_BORDER;
                    cellContenido.BackgroundColor = new BaseColor(50, 160, 65); // Mismo Fondo Verde
                    cellContenido.VerticalAlignment = Element.ALIGN_MIDDLE;

                    // A. Agregamos la Fecha como un Párrafo dentro de esta celda
                    var pFecha = new Paragraph($"Fecha - Hora de impresión: {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}", fHeaderBlanco);
                    
                    // Asegúrate que fHeaderBlanco sea pequeña (ej. tamaño 9 o 10)
                    pFecha.Alignment = Element.ALIGN_RIGHT;
                    pFecha.SpacingAfter = 5f; // Un poco de espacio antes del título
                    cellContenido.AddElement(pFecha);

                    // B. Agregamos el Título como otro Párrafo dentro de la misma celda
                    var pTitulo = new Paragraph("Reporte de Cierre de Turno", fTituloBlanco);
                    pTitulo.Alignment = Element.ALIGN_CENTER;
                    cellContenido.AddElement(pTitulo);

                    // C. (Opcional) Ajustar padding para que no quede pegado a los bordes
                    cellContenido.PaddingRight = 10f;
                    cellContenido.PaddingBottom = 15f;

                    headerTable.AddCell(cellContenido); // Agregamos la celda 2

                    // -----------------------------------------------------------------
                    // FINALIZAR
                    // -----------------------------------------------------------------
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
                    AgregarLinea("Playero", $"{incidencia.Playero.Usuarios.Usu_ap}, {incidencia.Playero.Usuarios.Usu_nom}");
                    AgregarLinea("Fecha y Hora de la Incidencia", $"{incidencia.Inci_fecha_Hora:dd/MM/yyyy HH:mm}");

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