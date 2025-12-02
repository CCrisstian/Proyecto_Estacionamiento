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
                        .Select(p => new
                        {
                            Ocup = p.Ocupacion.FirstOrDefault(),
                            Metodo = p.Metodos_De_Pago.Metodo_pago_descripcion,
                            Monto = p.Pago_Importe
                        }).ToList();

                    var listaOcupacion = rawOcupacion.Select(x => new
                    {
                        Ingreso = x.Ocup?.Ocu_fecha_Hora_Inicio,
                        Egreso = x.Ocup?.Ocu_fecha_Hora_Fin,
                        Plaza = x.Ocup?.Plaza?.Plaza_Nombre ?? "-",
                        Patente = x.Ocup?.Vehiculo_Patente ?? "-",
                        TipoVehiculo = x.Ocup?.Vehiculo?.Categoria_Vehiculo?.Categoria_descripcion ?? "-",
                        Tarifa = x.Ocup?.Tarifa?.Tipos_Tarifa?.Tipos_tarifa_descripcion ?? "-",
                        FormaPago = x.Metodo,
                        MontoVal = x.Monto,
                        MontoStr = x.Monto.ToString("C")
                    }).ToList();

                    // --- OBTENCIÓN DE DATOS (ABONOS) ---
                    var rawAbonos = db.Pagos_Abonados
                        .Where(p => p.Turno_id == turnoId)
                        .Select(p => new
                        {
                            FechaPago = p.Fecha_Pago,
                            Plaza = p.Abono.Plaza.Plaza_Nombre,
                            Tarifa = p.Tarifa.Tipos_Tarifa.Tipos_tarifa_descripcion,
                            Metodo = p.Acepta_Metodo_De_Pago.Metodos_De_Pago.Metodo_pago_descripcion,
                            Monto = p.PA_Monto,
                            Nombre = p.Abono.Titular_Abono.TAB_Nombre,
                            Apellido = p.Abono.Titular_Abono.TAB_Apellido,
                            PatentesList = p.Abono.Vehiculo_Abonado.Select(va => va.Vehiculo_Patente)
                        }).ToList();

                    var listaAbonos = rawAbonos.Select(x => new
                    {
                        Fecha = x.FechaPago.ToString("dd/MM HH:mm"),
                        Plaza = x.Plaza,
                        FormaPago = x.Metodo,
                        Titular = $"{x.Nombre} {x.Apellido}",
                        Patentes = string.Join(", ", x.PatentesList),
                        Tarifa = x.Tarifa,
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

                        var colorVerdeHeader = new BaseColor(50, 160, 65); // #32a041
                        var fHeaderBlanco = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE); // Fuente Blanca

                        // --- FUNCIÓN LOCAL PARA REPETIR EL ENCABEZADO ---
                        Action AgregarEncabezado = () =>
                        {
                            PdfPTable headerTable = new PdfPTable(2);
                            headerTable.WidthPercentage = 100;
                            headerTable.SetWidths(new float[] { 1f, 4f });

                            // 1. Logo
                            string imagePath = Server.MapPath("~/Images/LogoACE_SinFondo.PNG");
                            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(imagePath);
                            logo.ScaleToFit(60f, 60f);

                            PdfPCell cellLogo = new PdfPCell(logo);
                            cellLogo.Border = PdfPCell.NO_BORDER;
                            cellLogo.BackgroundColor = new BaseColor(50, 160, 65);
                            cellLogo.HorizontalAlignment = Element.ALIGN_CENTER;
                            cellLogo.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellLogo.Padding = 10f;
                            headerTable.AddCell(cellLogo);

                            // 2. Título
                            PdfPCell cellTitulo = new PdfPCell(new Phrase("Reporte de Cierre de Turno", fTituloBlanco));
                            cellTitulo.Border = PdfPCell.NO_BORDER;
                            cellTitulo.BackgroundColor = new BaseColor(50, 160, 65);
                            cellTitulo.HorizontalAlignment = Element.ALIGN_CENTER;
                            cellTitulo.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellTitulo.Padding = 20f;
                            headerTable.AddCell(cellTitulo);
                            headerTable.SpacingAfter = 20f;

                            doc.Add(headerTable);
                        };

                        // Función auxiliar para líneas de texto
                        void AgregarLinea(string etiqueta, string valor)
                        {
                            var p = new Paragraph();
                            p.Add(new Chunk(etiqueta + ": ", fBold));
                            p.Add(new Chunk(valor, fNormal));
                            doc.Add(p);
                        }

                        // =========================================================
                        // PAGINA 1: DATOS GENERALES + TABLA OCUPACIÓN
                        // =========================================================
                        AgregarEncabezado(); // Inserta Header

                        // Parámetros
                        AgregarLinea("Estacionamiento", turno.Playero.Estacionamiento.Est_nombre);
                        AgregarLinea("Playero", $"{turno.Playero.Usuarios.Usu_nom} {turno.Playero.Usuarios.Usu_ap}");
                        AgregarLinea("Inicio del Turno", turno.Turno_FechaHora_Inicio.ToString("dd/MM/yyyy HH:mm"));
                        AgregarLinea("Fin del Turno", turno.Turno_FechaHora_fin.HasValue ? turno.Turno_FechaHora_fin.Value.ToString("dd/MM/yyyy HH:mm") : "En curso");
                        AgregarLinea("Fecha y Hora de la impresión", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                        doc.Add(Chunk.NEWLINE);


                        // Tabla Ocupación
                        doc.Add(new Paragraph("Detalle de Cobros por: Ocupación", fSubtitulo));
                        PdfPTable tOcup = new PdfPTable(8) { WidthPercentage = 100, SpacingBefore = 10f, SpacingAfter = 10f };

                        // Cabeceras con Estilo Verde
                        string[] hOcup = { "Ingreso", "Egreso", "Plaza", "Patente", "Vehículo", "Tarifa", "Cobro", "Monto" };
                        foreach (var h in hOcup)
                        {
                            tOcup.AddCell(new PdfPCell(new Phrase(h, fHeaderBlanco))
                            {
                                BackgroundColor = colorVerdeHeader,
                                PaddingTop = 6f,     // Aumenta espacio arriba
                                PaddingBottom = 6f,  // Aumenta espacio abajo
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                VerticalAlignment = Element.ALIGN_MIDDLE
                            });
                        }

                        foreach (var item in listaOcupacion)
                        {
                            tOcup.AddCell(new PdfPCell(new Phrase(item.Ingreso?.ToString("HH:mm") ?? "-", fNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            tOcup.AddCell(new PdfPCell(new Phrase(item.Egreso?.ToString("HH:mm") ?? "-", fNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            tOcup.AddCell(new PdfPCell(new Phrase(item.Plaza, fNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            tOcup.AddCell(new PdfPCell(new Phrase(item.Patente, fNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            tOcup.AddCell(new PdfPCell(new Phrase(item.TipoVehiculo, fNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            tOcup.AddCell(new PdfPCell(new Phrase(item.Tarifa, fNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            tOcup.AddCell(new PdfPCell(new Phrase(item.FormaPago, fNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            tOcup.AddCell(new PdfPCell(new Phrase(item.MontoStr, fNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                        }

                        doc.Add(tOcup);

                        // Totales Ocupación
                        doc.Add(new Paragraph($"Total Efectivo: {efectivoOcupacion:C}", fNormal) { Alignment = Element.ALIGN_RIGHT });
                        doc.Add(new Paragraph($"Total Ocupación: {totalOcupacion:C}", fBold) { Alignment = Element.ALIGN_RIGHT });

                        // *** SALTO DE PÁGINA ***
                        doc.NewPage();

                        // =========================================================
                        // PAGINA 2: TABLA ABONOS
                        // =========================================================
                        AgregarEncabezado(); // Repite Header

                        // Parámetros
                        AgregarLinea("Estacionamiento", turno.Playero.Estacionamiento.Est_nombre);
                        AgregarLinea("Playero", $"{turno.Playero.Usuarios.Usu_nom} {turno.Playero.Usuarios.Usu_ap}");
                        AgregarLinea("Inicio del Turno", turno.Turno_FechaHora_Inicio.ToString("dd/MM/yyyy HH:mm"));
                        AgregarLinea("Fin del Turno", turno.Turno_FechaHora_fin.HasValue ? turno.Turno_FechaHora_fin.Value.ToString("dd/MM/yyyy HH:mm") : "En curso");
                        AgregarLinea("Fecha y Hora de la impresión", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                        doc.Add(Chunk.NEWLINE);

                        doc.Add(new Paragraph("Detalle de Cobros por: Abono", fSubtitulo));
                        PdfPTable tAbono = new PdfPTable(7) { WidthPercentage = 100, SpacingBefore = 10f, SpacingAfter = 10f };

                        // Cabeceras con Estilo Verde
                        string[] hAbono = { "Fecha", "Plaza", "Titular", "Patentes", "Tarifa", "Cobro", "Monto" };
                        foreach (var h in hAbono)
                        {
                            tAbono.AddCell(new PdfPCell(new Phrase(h, fHeaderBlanco))
                            {
                                BackgroundColor = colorVerdeHeader,
                                PaddingTop = 6f,     // Aumenta espacio arriba
                                PaddingBottom = 6f,  // Aumenta espacio abajo
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                VerticalAlignment = Element.ALIGN_MIDDLE
                            });
                        }

                        foreach (var item in listaAbonos)
                        {
                            tAbono.AddCell(new PdfPCell(new Phrase(item.Fecha, fNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            tAbono.AddCell(new PdfPCell(new Phrase(item.Plaza, fNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            tAbono.AddCell(new PdfPCell(new Phrase(item.Titular, fNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            tAbono.AddCell(new PdfPCell(new Phrase(item.Patentes, fNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            tAbono.AddCell(new PdfPCell(new Phrase(item.Tarifa, fNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            tAbono.AddCell(new PdfPCell(new Phrase(item.FormaPago, fNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                            tAbono.AddCell(new PdfPCell(new Phrase(item.MontoStr, fNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                        }
                        doc.Add(tAbono);

                        // Totales Abono
                        doc.Add(new Paragraph($"Total Efectivo: {efectivoAbono:C}", fNormal) { Alignment = Element.ALIGN_RIGHT });
                        doc.Add(new Paragraph($"Total Abono: {totalAbono:C}", fBold) { Alignment = Element.ALIGN_RIGHT });

                        // *** SALTO DE PÁGINA ***
                        doc.NewPage();


                        // =========================================================
                        // PAGINA 3: CAJA EFECTIVO
                        // =========================================================
                        AgregarEncabezado(); // Repite Header

                        // Parámetros
                        AgregarLinea("Estacionamiento", turno.Playero.Estacionamiento.Est_nombre);
                        AgregarLinea("Playero", $"{turno.Playero.Usuarios.Usu_nom} {turno.Playero.Usuarios.Usu_ap}");
                        AgregarLinea("Inicio del Turno", turno.Turno_FechaHora_Inicio.ToString("dd/MM/yyyy HH:mm"));
                        AgregarLinea("Fin del Turno", turno.Turno_FechaHora_fin.HasValue ? turno.Turno_FechaHora_fin.Value.ToString("dd/MM/yyyy HH:mm") : "En curso");
                        AgregarLinea("Fecha y Hora de la impresión", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                        doc.Add(Chunk.NEWLINE);

                        doc.Add(new Paragraph("Detalle de Caja: Efectivo", fSubtitulo));
                        PdfPTable tCaja = new PdfPTable(2) { WidthPercentage = 60, HorizontalAlignment = Element.ALIGN_LEFT, SpacingBefore = 10f };

                        double mInicio = turno.Caja_Monto_Inicio ?? 0;
                        tCaja.AddCell(new PdfPCell(new Phrase("Monto Inicio (Efectivo en Caja)", fNormal)));
                        tCaja.AddCell(new PdfPCell(new Phrase(mInicio.ToString("C"), fNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });

                        tCaja.AddCell(new PdfPCell(new Phrase("Recaudado en Efectivo", fNormal)));
                        tCaja.AddCell(new PdfPCell(new Phrase(totalEfectivoRecaudado.ToString("C"), fNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });

                        double totalCajaEfectivo = mInicio + totalEfectivoRecaudado;
                        tCaja.AddCell(new PdfPCell(new Phrase("Monto Fin (Efectivo en Caja)", fBold)));
                        tCaja.AddCell(new PdfPCell(new Phrase(totalCajaEfectivo.ToString("C"), fBold)) { HorizontalAlignment = Element.ALIGN_RIGHT });

                        doc.Add(tCaja);

                        doc.Add(Chunk.NEWLINE);


                        // =========================================================
                        // PAGINA 3: OTRAS FORMAS DE PAGO
                        // =========================================================

                        doc.Add(new Paragraph("Detalle de Caja: Otras formas de Pago", fSubtitulo));

                        // Validamos si hay datos para no mostrar tabla vacía, pero mantenemos la página
                        if (todosLosPagos.Any())
                        {
                            PdfPTable tOtros = new PdfPTable(2) { WidthPercentage = 50, HorizontalAlignment = Element.ALIGN_LEFT, SpacingBefore = 10f, SpacingAfter = 10f };

                            // Cabeceras Manuales con Estilo Verde
                            tOtros.AddCell(new PdfPCell(new Phrase("Forma de Pago", fHeaderBlanco)) { BackgroundColor = colorVerdeHeader, PaddingTop = 6f, PaddingBottom = 6f, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
                            tOtros.AddCell(new PdfPCell(new Phrase("Total", fHeaderBlanco)) { BackgroundColor = colorVerdeHeader, PaddingTop = 6f, PaddingBottom = 6f, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });

                            foreach (var p in todosLosPagos)
                            {
                                tOtros.AddCell(new PdfPCell(new Phrase(p.Metodo, fNormal)));
                                tOtros.AddCell(new PdfPCell(new Phrase(p.Total.ToString("C"), fNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                            }
                            doc.Add(tOtros);
                        }
                        else
                        {
                            doc.Add(new Paragraph("No se registraron movimientos en otras formas de pago.", fNormal));
                        }


                        // =========================================================
                        // PAGINA 3: TOTAL GENERAL
                        // =========================================================

                        doc.Add(Chunk.NEWLINE);

                        // Cuadro grande para el Total
                        PdfPTable tTotalGen = new PdfPTable(1);
                        tTotalGen.WidthPercentage = 100;

                        double granTotal = (turno.Caja_Monto_total ?? (totalOcupacion + totalAbono));

                        PdfPCell cellTotal = new PdfPCell(new Phrase($"Total Recaudado: {granTotal:C}", fTitulo));
                        cellTotal.HorizontalAlignment = Element.ALIGN_CENTER;
                        cellTotal.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cellTotal.Padding = 20f;
                        cellTotal.BackgroundColor = BaseColor.LIGHT_GRAY;

                        tTotalGen.AddCell(cellTotal);
                        doc.Add(tTotalGen);

                        // --- FIN DEL DOCUMENTO ---
                        doc.Close();

                        // --- DESCARGA ---
                        Response.Clear();
                        Response.ContentType = "application/pdf";
                        string formatoArchivo = "yyyy-MM-dd_HH-mm";
                        string fInicio = turno.Turno_FechaHora_Inicio.ToString(formatoArchivo);
                        string fFin = turno.Turno_FechaHora_fin.HasValue ? turno.Turno_FechaHora_fin.Value.ToString(formatoArchivo) : "En_curso";
                        string nombreArchivo = $"TurnoPlayero_{turno.Playero.Usuarios.Usu_nom}_{turno.Playero.Usuarios.Usu_ap}_{fInicio}_{fFin}.pdf";
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