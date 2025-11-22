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

                    // (Aquí va toda tu lógica de iTextSharp para crear el PDF)
                    var fontTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                    var fontSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                    var fontBody = FontFactory.GetFont(FontFactory.HELVETICA, 12);
                    var fontInfo = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 10);

                    document.Add(new Paragraph("Incidencia", fontTitulo) { Alignment = Element.ALIGN_CENTER });
                    document.Add(Chunk.NEWLINE);
                    document.Add(new Paragraph($"Estacionamiento: { incidencia.Playero.Estacionamiento.Est_nombre}", fontBody));
                    document.Add(new Paragraph($"Fecha y Hora: {incidencia.Inci_fecha_Hora:dd/MM/yyyy HH:mm}", fontBody));
                    document.Add(new Paragraph($"Playero a cargo: {incidencia.Playero.Usuarios.Usu_ap}, {incidencia.Playero.Usuarios.Usu_nom}", fontBody));
                    document.Add(new Paragraph($"Estado: {(incidencia.Inci_Estado ? "Resuelto" : "Pendiente")}", fontBody));
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