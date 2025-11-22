using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Xml.Linq;
// Asegúrate de tener los usings de tu modelo de datos

namespace Proyecto_Estacionamiento.Pages.Turnos
{
    public partial class Turno_Descargar : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (int.TryParse(Request.QueryString["turnoId"], out int turnoId))
            {
                GenerarPdfTurno(turnoId);
            }
            else
            {
                Response.Write("ID de turno inválido.");
                Response.End();
            }
        }

        private void GenerarPdfTurno(int turnoId)
        {
            try
            {
                using (var db = new ProyectoEstacionamientoEntities())
                {
                    // 1. Obtener Datos del Turno
                    var turno = db.Turno.Include("Playero.Usuarios").Include("Playero.Estacionamiento")
                                  .FirstOrDefault(t => t.Turno_id == turnoId);

                    if (turno == null) { Response.Write("Turno no encontrado."); Response.End(); return; }

                    // 2. OBTENER DATOS DE OCUPACIÓN (Proyección para PDF)
                    var listaOcupacion = db.Pago_Ocupacion
                        .Where(p => p.Turno_id == turnoId)
                        .Select(p => new
                        {
                            // Navegamos a Ocupacion para obtener los detalles
                            // Nota: Usamos el pago_id para encontrar la ocupación relacionada
                            DatosOcupacion = db.Ocupacion.FirstOrDefault(o => o.Pago_id == p.Pago_id),
                            Metodo = p.Metodos_De_Pago.Metodo_pago_descripcion,
                            Monto = p.Pago_Importe
                        })
                        .ToList() // Traemos a memoria
                        .Select(x => new
                        {
                            Ingreso = x.DatosOcupacion?.Ocu_fecha_Hora_Inicio.ToString("HH:mm") ?? "-",
                            Egreso = x.DatosOcupacion?.Ocu_fecha_Hora_Fin?.ToString("HH:mm") ?? "-",
                            Plaza = x.DatosOcupacion?.Plaza.Plaza_Nombre ?? "-",
                            FormaPago = x.Metodo,
                            MontoStr = x.Monto.ToString("C") // Formato moneda
                        })
                        .ToList();

                    // 3. OBTENER DATOS DE ABONOS (Proyección para PDF)
                    var listaAbonos = db.Pagos_Abonados
                        .Where(p => p.Turno_id == turnoId)
                        .Select(p => new
                        {
                            FechaPago = p.Fecha_Pago,
                            Plaza = p.Abono.Plaza.Plaza_Nombre,
                            Metodo = p.Acepta_Metodo_De_Pago.Metodos_De_Pago.Metodo_pago_descripcion,
                            Monto = p.PA_Monto
                        })
                        .ToList() // Traemos a memoria
                        .Select(x => new
                        {
                            Fecha = x.FechaPago.ToString("dd/MM HH:mm"),
                            Plaza = x.Plaza,
                            FormaPago = x.Metodo,
                            MontoStr = x.Monto.ToString("C")
                        })
                        .ToList();

                    // 3. Generar PDF
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
                        PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                        doc.Open();

                        // Fuentes
                        var fTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                        var fSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                        var fNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                        var fBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

                        // --- ENCABEZADO ---
                        doc.Add(new Paragraph("Reporte de Cierre de Turno", fTitulo) { Alignment = Element.ALIGN_CENTER });
                        doc.Add(Chunk.NEWLINE);

                        // Función auxiliar para líneas con etiqueta en negrita
                        void AgregarLinea(string etiqueta, string valor)
                        {
                            var p = new Paragraph();
                            p.Add(new Chunk(etiqueta + ": ", fBold)); // Etiqueta en negrita
                            p.Add(new Chunk(valor, fNormal));       // Valor normal
                            doc.Add(p);
                        }

                        AgregarLinea("Estacionamiento", turno.Playero.Estacionamiento.Est_nombre);
                        AgregarLinea("Playero", $"{turno.Playero.Usuarios.Usu_nom} {turno.Playero.Usuarios.Usu_ap}");
                        AgregarLinea("Inicio de Turno", turno.Turno_FechaHora_Inicio.ToString("dd/MM/yyyy HH:mm"));
                        AgregarLinea("Fin de Turno", turno.Turno_FechaHora_fin.HasValue ? turno.Turno_FechaHora_fin.Value.ToString("dd/MM/yyyy HH:mm") : "En curso");
                        doc.Add(Chunk.NEWLINE);

                        // --- TABLA OCUPACIÓN ---
                        doc.Add(new Paragraph("Detalle de Pagos por: Ocupación", fSubtitulo));

                        // --- TABLA OCUPACIÓN ---
                        PdfPTable tablaOcup = new PdfPTable(5); // 5 columnas
                        tablaOcup.WidthPercentage = 100;

                        // espacio ANTES de la tabla (para separarla del título anterior)
                        tablaOcup.SpacingBefore = 15f;

                        // espacio DESPUÉS de la tabla (para separarla de lo que sigue)
                        tablaOcup.SpacingAfter = 15f;

                        // Cabeceras
                        string[] headersOcup = { "Ingreso", "Egreso", "Plaza", "Pago", "Monto" };
                        foreach (var h in headersOcup) tablaOcup.AddCell(new PdfPCell(new Phrase(h, fBold)) { BackgroundColor = BaseColor.LIGHT_GRAY });

                        // Filas
                        foreach (var item in listaOcupacion)
                        {
                            tablaOcup.AddCell(new PdfPCell(new Phrase(item.Ingreso, fNormal)));
                            tablaOcup.AddCell(new PdfPCell(new Phrase(item.Egreso, fNormal)));
                            tablaOcup.AddCell(new PdfPCell(new Phrase(item.Plaza, fNormal)));
                            tablaOcup.AddCell(new PdfPCell(new Phrase(item.FormaPago, fNormal)));
                            // Alineamos el monto a la derecha
                            tablaOcup.AddCell(new PdfPCell(new Phrase(item.MontoStr, fNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                        }
                        doc.Add(tablaOcup);

                        // Total Ocupación
                        double totalOcupacionVal = listaOcupacion.Sum(x => double.Parse(x.MontoStr, System.Globalization.NumberStyles.Currency));
                        double totalOcupacionReal = db.Pago_Ocupacion.Where(p => p.Turno_id == turnoId).Sum(p => (double?)p.Pago_Importe) ?? 0;
                        doc.Add(new Paragraph($"Total Ocupación: {totalOcupacionReal:C}", fBold) { Alignment = Element.ALIGN_RIGHT });
                        doc.Add(Chunk.NEWLINE);

                        // --- TABLA ABONOS ---
                        doc.Add(new Paragraph("Detalle de Pagos por: Abono", fSubtitulo));
                        
                        PdfPTable tablaAbono = new PdfPTable(4); // 4 columnas: Fecha, Plaza, Pago, Monto

                        // espacio ANTES de la tabla (para separarla del título anterior)
                        tablaAbono.SpacingBefore = 15f;

                        // espacio DESPUÉS de la tabla (para separarla de lo que sigue)
                        tablaAbono.SpacingAfter = 15f;

                        tablaAbono.WidthPercentage = 100;

                        // Cabeceras
                        string[] headersAbono = { "Fecha", "Plaza", "Pago", "Monto" };
                        foreach (var h in headersAbono) tablaAbono.AddCell(new PdfPCell(new Phrase(h, fBold)) { BackgroundColor = BaseColor.LIGHT_GRAY });

                        // Filas (Lógica similar)
                        foreach (var item in listaAbonos)
                        {
                            tablaAbono.AddCell(new PdfPCell(new Phrase(item.Fecha, fNormal)));
                            tablaAbono.AddCell(new PdfPCell(new Phrase(item.Plaza, fNormal)));
                            tablaAbono.AddCell(new PdfPCell(new Phrase(item.FormaPago, fNormal)));
                            tablaAbono.AddCell(new PdfPCell(new Phrase(item.MontoStr, fNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                        }
                        doc.Add(tablaAbono);

                        // Total Abonos
                        double totalAbonoReal = db.Pagos_Abonados.Where(p => p.Turno_id == turnoId).Sum(p => (double?)p.PA_Monto) ?? 0;
                        doc.Add(new Paragraph($"Total Abono: {totalAbonoReal:C}", fBold) { Alignment = Element.ALIGN_RIGHT });
                        doc.Add(Chunk.NEWLINE);

                        // --- DETALLE CAJA (TABULAR) ---
                        doc.Add(new Paragraph("Detalle de Caja", fSubtitulo));

                        PdfPTable tablaCaja = new PdfPTable(2); // 2 columnas
                        
                        // espacio ANTES de la tabla (para separarla del título anterior)
                        tablaCaja.SpacingBefore = 15f;

                        // espacio DESPUÉS de la tabla (para separarla de lo que sigue)
                        tablaCaja.SpacingAfter = 15f;

                        tablaCaja.WidthPercentage = 50; // Más angosta, centrada o a la izquierda
                        tablaCaja.HorizontalAlignment = Element.ALIGN_LEFT;

                        // Cabeceras Caja
                        tablaCaja.AddCell(new PdfPCell(new Phrase("Concepto", fBold)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                        tablaCaja.AddCell(new PdfPCell(new Phrase("Monto", fBold)) { BackgroundColor = BaseColor.LIGHT_GRAY, HorizontalAlignment = Element.ALIGN_RIGHT });

                        // Filas Caja
                        tablaCaja.AddCell(new PdfPCell(new Phrase("Monto Inicio", fNormal)));
                        tablaCaja.AddCell(new PdfPCell(new Phrase(turno.Caja_Monto_Inicio.Value.ToString("C"), fNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });

                        tablaCaja.AddCell(new PdfPCell(new Phrase("Total Recaudado", fNormal)));
                        tablaCaja.AddCell(new PdfPCell(new Phrase(turno.Caja_Monto_total.Value.ToString("C"), fNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });

                        // Fila Final (Total Caja)
                        tablaCaja.AddCell(new PdfPCell(new Phrase("Monto Fin (Caja)", fBold)));
                        tablaCaja.AddCell(new PdfPCell(new Phrase(turno.Caja_Monto_fin.Value.ToString("C"), fBold)) { HorizontalAlignment = Element.ALIGN_RIGHT });

                        doc.Add(tablaCaja);

                        doc.Close();

                        // Enviar al navegador
                        Response.Clear();
                        Response.ContentType = "application/pdf";
                        Response.AddHeader("content-disposition", $"attachment;filename=TurnoPlayero_{turno.Playero.Usuarios.Usu_nom}_{turno.Playero.Usuarios.Usu_ap}.pdf");
                        Response.BinaryWrite(ms.ToArray());
                        Response.End();
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Write("Error al generar PDF: " + ex.Message);
            }
        }
    }
}