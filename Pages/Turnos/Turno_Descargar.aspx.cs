using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Xml.Linq;

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
                    var turno = db.Turno.Include("Playero.Usuarios").Include("Playero.Estacionamiento")
                                    .FirstOrDefault(t => t.Turno_id == turnoId);

                    if (turno == null) { Response.Write("Turno no encontrado."); Response.End(); return; }

                    // --- OBTENCIÓN DE DATOS (OCUPACIÓN) ---
                    var rawOcupacion = db.Pago_Ocupacion
                        .Where(p => p.Turno_id == turnoId)
                        .Select(p => new {
                            Ingreso = p.Ocupacion.FirstOrDefault().Ocu_fecha_Hora_Inicio,
                            Egreso = p.Ocupacion.FirstOrDefault().Ocu_fecha_Hora_Fin,
                            Plaza = p.Ocupacion.FirstOrDefault().Plaza.Plaza_Nombre,
                            Metodo = p.Metodos_De_Pago.Metodo_pago_descripcion,
                            Monto = p.Pago_Importe
                        }).ToList(); // Traer a memoria para formatear

                    var listaOcupacion = rawOcupacion.Select(x => new {
                        Ingreso = x.Ingreso.ToString("HH:mm"),
                        Egreso = x.Egreso?.ToString("HH:mm") ?? "-",
                        Plaza = x.Plaza ?? "-",
                        FormaPago = x.Metodo,
                        MontoVal = x.Monto,
                        MontoStr = x.Monto.ToString("C")
                    }).ToList();

                    // --- OBTENCIÓN DE DATOS (ABONOS) ---
                    var rawAbonos = db.Pagos_Abonados
                        .Where(p => p.Turno_id == turnoId)
                        .Select(p => new {
                            FechaPago = p.Fecha_Pago,
                            Plaza = p.Abono.Plaza.Plaza_Nombre,
                            Metodo = p.Acepta_Metodo_De_Pago.Metodos_De_Pago.Metodo_pago_descripcion,
                            Monto = p.PA_Monto
                        }).ToList();

                    var listaAbonos = rawAbonos.Select(x => new {
                        Fecha = x.FechaPago.ToString("dd/MM HH:mm"),
                        Plaza = x.Plaza,
                        FormaPago = x.Metodo,
                        MontoVal = x.Monto,
                        MontoStr = x.Monto.ToString("C")
                    }).ToList();

                    // --- CÁLCULOS DE TOTALES ---
                    double totalOcupacion = listaOcupacion.Sum(x => x.MontoVal);
                    double totalAbono = listaAbonos.Sum(x => x.MontoVal);

                    // Totales en Efectivo
                    double efectivoOcupacion = listaOcupacion.Where(x => x.FormaPago == "Efectivo").Sum(x => x.MontoVal);
                    double efectivoAbono = listaAbonos.Where(x => x.FormaPago == "Efectivo").Sum(x => x.MontoVal);
                    double totalEfectivoRecaudado = efectivoOcupacion + efectivoAbono;

                    // Totales Agrupados por Método (Para la nueva tabla)
                    // Unimos ambas listas en una sola proyección genérica para agrupar
                    var todosLosPagos = listaOcupacion.Select(x => new { Metodo = x.FormaPago, Monto = x.MontoVal })
                        .Concat(listaAbonos.Select(x => new { Metodo = x.FormaPago, Monto = x.MontoVal }))
                        .GroupBy(x => x.Metodo)
                        .Select(g => new { Metodo = g.Key, Total = g.Sum(x => x.Monto) })
                        .Where(x => x.Metodo != "Efectivo") // Opcional: Excluir efectivo si ya se muestra en caja
                        .ToList();


                    // --- GENERAR PDF ---
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
                        PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                        doc.Open();

                        // --- FUENTES ---
                        var fTituloBlanco = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24, BaseColor.WHITE);
                        var fTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                        var fSubtitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                        var fNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                        var fBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

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
                        PdfPCell cellTitulo = new PdfPCell(new Phrase("Reporte de Cierre de Turno", fTituloBlanco));
                        cellTitulo.Border = PdfPCell.NO_BORDER;
                        cellTitulo.BackgroundColor = new BaseColor(50, 160, 65); // Verde
                        cellTitulo.HorizontalAlignment = Element.ALIGN_CENTER;
                        cellTitulo.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cellTitulo.Padding = 20f; // Espacio vertical
                        headerTable.AddCell(cellTitulo);
                        headerTable.SpacingAfter = 20f;

                        doc.Add(headerTable);

                        void AgregarLinea(string etiqueta, string valor)
                        {
                            var p = new Paragraph();
                            p.Add(new Chunk(etiqueta + ": ", fBold));
                            p.Add(new Chunk(valor, fNormal));
                            doc.Add(p);
                        }
                        
                        AgregarLinea("Estacionamiento", turno.Playero.Estacionamiento.Est_nombre);
                        AgregarLinea("Playero", $"{turno.Playero.Usuarios.Usu_nom} {turno.Playero.Usuarios.Usu_ap}");
                        AgregarLinea("Inicio", turno.Turno_FechaHora_Inicio.ToString("dd/MM/yyyy HH:mm"));
                        AgregarLinea("Fin", turno.Turno_FechaHora_fin.HasValue ? turno.Turno_FechaHora_fin.Value.ToString("dd/MM/yyyy HH:mm") : "En curso");
                        doc.Add(Chunk.NEWLINE);

                        // --- TABLA 1: OCUPACIÓN ---
                        doc.Add(new Paragraph("Detalle de Pagos por: Ocupación", fSubtitulo));
                        PdfPTable tOcup = new PdfPTable(5) { WidthPercentage = 100, SpacingBefore = 10f, SpacingAfter = 10f };
                        string[] hOcup = { "Ingreso", "Egreso", "Plaza", "Pago", "Monto" };
                        foreach (var h in hOcup) tOcup.AddCell(new PdfPCell(new Phrase(h, fBold)) { BackgroundColor = BaseColor.LIGHT_GRAY });

                        foreach (var item in listaOcupacion)
                        {
                            tOcup.AddCell(new PdfPCell(new Phrase(item.Ingreso, fNormal)));
                            tOcup.AddCell(new PdfPCell(new Phrase(item.Egreso, fNormal)));
                            tOcup.AddCell(new PdfPCell(new Phrase(item.Plaza, fNormal)));
                            tOcup.AddCell(new PdfPCell(new Phrase(item.FormaPago, fNormal)));
                            tOcup.AddCell(new PdfPCell(new Phrase(item.MontoStr, fNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                        }
                        doc.Add(tOcup);

                        // Totales Ocupación
                        doc.Add(new Paragraph($"Total Efectivo: {efectivoOcupacion:C}", fNormal) { Alignment = Element.ALIGN_RIGHT });
                        doc.Add(new Paragraph($"Total Ocupación: {totalOcupacion:C}", fBold) { Alignment = Element.ALIGN_RIGHT });
                        doc.Add(Chunk.NEWLINE);

                        // --- TABLA 2: ABONOS ---
                        doc.Add(new Paragraph("Detalle de Pagos por: Abono", fSubtitulo));
                        PdfPTable tAbono = new PdfPTable(4) { WidthPercentage = 100, SpacingBefore = 10f, SpacingAfter = 10f };
                        string[] hAbono = { "Fecha", "Plaza", "Pago", "Monto" };
                        foreach (var h in hAbono) tAbono.AddCell(new PdfPCell(new Phrase(h, fBold)) { BackgroundColor = BaseColor.LIGHT_GRAY });

                        foreach (var item in listaAbonos)
                        {
                            tAbono.AddCell(new PdfPCell(new Phrase(item.Fecha, fNormal)));
                            tAbono.AddCell(new PdfPCell(new Phrase(item.Plaza, fNormal)));
                            tAbono.AddCell(new PdfPCell(new Phrase(item.FormaPago, fNormal)));
                            tAbono.AddCell(new PdfPCell(new Phrase(item.MontoStr, fNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                        }
                        doc.Add(tAbono);

                        // Totales Abono
                        doc.Add(new Paragraph($"Total Efectivo: {efectivoAbono:C}", fNormal) { Alignment = Element.ALIGN_RIGHT });
                        doc.Add(new Paragraph($"Total Abono: {totalAbono:C}", fBold) { Alignment = Element.ALIGN_RIGHT });
                        doc.Add(Chunk.NEWLINE);
                        doc.Add(Chunk.NEWLINE);

                        // --- TABLA 3: CAJA EFECTIVO ---
                        doc.Add(new Paragraph("Detalle de Caja: Efectivo", fSubtitulo));
                        PdfPTable tCaja = new PdfPTable(2) { WidthPercentage = 60, HorizontalAlignment = Element.ALIGN_LEFT, SpacingBefore = 10f };

                        // Monto Inicio
                        double mInicio = turno.Caja_Monto_Inicio ?? 0;
                        tCaja.AddCell(new PdfPCell(new Phrase("Monto Inicio (Efectivo en Caja)", fNormal)));
                        tCaja.AddCell(new PdfPCell(new Phrase(mInicio.ToString("C"), fNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });

                        // Total Recaudado (Solo Efectivo) - Ahora llamado "Monto Fin"
                        tCaja.AddCell(new PdfPCell(new Phrase("Recaudado en Efectivo", fNormal)));
                        tCaja.AddCell(new PdfPCell(new Phrase(totalEfectivoRecaudado.ToString("C"), fNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });

                        // Total Caja (Inicio + Efectivo Recaudado)
                        double totalCajaEfectivo = mInicio + totalEfectivoRecaudado;
                        tCaja.AddCell(new PdfPCell(new Phrase("Monto Fin (Efectivo en Caja)", fBold)));
                        tCaja.AddCell(new PdfPCell(new Phrase(totalCajaEfectivo.ToString("C"), fBold)) { HorizontalAlignment = Element.ALIGN_RIGHT });

                        doc.Add(tCaja);
                        doc.Add(Chunk.NEWLINE);

                        // --- TABLA 4: OTRAS FORMAS DE PAGO ---
                        if (todosLosPagos.Any())
                        {
                            doc.Add(new Paragraph("Detalle de Caja: Otras formas de Pago", fSubtitulo));
                            PdfPTable tOtros = new PdfPTable(2) { WidthPercentage = 50, HorizontalAlignment = Element.ALIGN_LEFT, SpacingBefore = 10f, SpacingAfter = 10f };
                            tOtros.AddCell(new PdfPCell(new Phrase("Forma de Pago", fBold)) { BackgroundColor = BaseColor.LIGHT_GRAY });
                            tOtros.AddCell(new PdfPCell(new Phrase("Total", fBold)) { BackgroundColor = BaseColor.LIGHT_GRAY, HorizontalAlignment = Element.ALIGN_RIGHT });

                            foreach (var p in todosLosPagos)
                            {
                                tOtros.AddCell(new PdfPCell(new Phrase(p.Metodo, fNormal)));
                                tOtros.AddCell(new PdfPCell(new Phrase(p.Total.ToString("C"), fNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                            }
                            doc.Add(tOtros);
                        }

                        // --- TOTAL GENERAL ---
                        doc.Add(Chunk.NEWLINE);
                        double granTotal = (turno.Caja_Monto_total ?? (totalOcupacion + totalAbono)); // O la suma calculada
                        doc.Add(new Paragraph($"Total Recaudado: {granTotal:C}", fTitulo) { Alignment = Element.ALIGN_RIGHT });


                        doc.Close();
                        Response.Clear();
                        Response.ContentType = "application/pdf";
                        
                        // 1. Preparamos las fechas en un formato válido para nombres de archivo
                        string formatoArchivo = "yyyy-MM-dd_HH-mm";
                        string fInicio = turno.Turno_FechaHora_Inicio.ToString(formatoArchivo);
                        string fFin = turno.Turno_FechaHora_fin.HasValue
                                      ? turno.Turno_FechaHora_fin.Value.ToString(formatoArchivo)
                                      : "En_curso";

                        // 2. Construimos el nombre 
                        string nombreArchivo = $"TurnoPlayero_{turno.Playero.Usuarios.Usu_nom}_{turno.Playero.Usuarios.Usu_ap}_{fInicio}_{fFin}.pdf";

                        // 3. Enviamos el header
                        Response.AddHeader("content-disposition", $"attachment;filename={nombreArchivo}");
                        Response.BinaryWrite(ms.ToArray());
                        
                        Response.End();
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Write("Error: " + ex.Message);
            }
        }
    }
}