using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaMonitoreoRedes.Models;

namespace SistemaMonitoreoRedes.Services
{
    public class ExportService
    {
        public byte[] ExportarExcel(List<Red> datos)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Monitoreo de Redes");

            // Header
            var headers = new[] { "ID", "Fecha/Hora", "ID Dispositivo", "Nombre", "Tipo", "Marca", "Modelo", "IP", "Ubicación", "Estado", "CPU%", "RAM%", "Descarga", "Subida", "Latencia", "Alerta", "Incidencia", "Riesgo IA", "Recomendación IA" };
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a5f");
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            int row = 2;
            foreach (var d in datos)
            {
                ws.Cell(row, 1).Value = d.IdMonitoreo;
                ws.Cell(row, 2).Value = d.FechaHora?.ToString("dd/MM/yyyy HH:mm") ?? "";
                ws.Cell(row, 3).Value = d.IdDispositivo?.ToString() ?? "";
                ws.Cell(row, 4).Value = d.NombreDispositivo ?? "";
                ws.Cell(row, 5).Value = d.Tipo ?? "";
                ws.Cell(row, 6).Value = d.Marca ?? "";
                ws.Cell(row, 7).Value = d.Modelo ?? "";
                ws.Cell(row, 8).Value = d.Ip ?? "";
                ws.Cell(row, 9).Value = d.Ubicacion ?? "";
                ws.Cell(row, 10).Value = d.Estado ?? "";
                ws.Cell(row, 11).Value = (double?)d.CpuPorcentaje ?? 0;
                ws.Cell(row, 12).Value = (double?)d.RamPorcentaje ?? 0;
                ws.Cell(row, 13).Value = (double?)d.DescargaMbps ?? 0;
                ws.Cell(row, 14).Value = (double?)d.SubidaMbps ?? 0;
                ws.Cell(row, 15).Value = (double?)d.LatenciaMs ?? 0;
                ws.Cell(row, 16).Value = d.Alerta ?? "";
                ws.Cell(row, 17).Value = d.Incidencia ?? "";
                ws.Cell(row, 18).Value = d.RiesgoIa ?? "";
                ws.Cell(row, 19).Value = d.RecomendacionIa ?? "";

                if (row % 2 == 0)
                    ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#f0f4f8");

                row++;
            }

            ws.Columns().AdjustToContents();
            var range = ws.Range(1, 1, row - 1, headers.Length);
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Hair;

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        public byte[] ExportarPdf(List<Red> datos, string titulo = "Reporte de Monitoreo de Redes")
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(c => ComposeContent(c, datos));
                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ").FontSize(8);
                        x.CurrentPageNumber().FontSize(8);
                        x.Span(" de ").FontSize(8);
                        x.TotalPages().FontSize(8);
                    });
                });
            });

            return doc.GeneratePdf();

            void ComposeHeader(IContainer c)
            {
                c.Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(titulo).FontSize(14).Bold().FontColor(Colors.Blue.Darken3);
                        col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Darken1);
                        col.Item().Text($"Total registros: {datos.Count}").FontSize(9);
                    });
                });
                c.PaddingBottom(10);
            }

            void ComposeContent(IContainer c, List<Red> items)
            {
                c.Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(25);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(2);
                        cols.ConstantColumn(70);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(1.5f);
                        cols.ConstantColumn(35);
                        cols.ConstantColumn(35);
                        cols.ConstantColumn(40);
                        cols.RelativeColumn(1.5f);
                    });

                    table.Header(header =>
                    {
                        var heads = new[] { "ID", "Nombre", "Tipo", "IP", "Ubicación", "Estado", "CPU%", "RAM%", "Lat ms", "Riesgo IA" };
                        foreach (var h in heads)
                        {
                            header.Cell().Background(Colors.Blue.Darken3).Padding(4)
                                .Text(h).FontColor(Colors.White).Bold().FontSize(8);
                        }
                    });

                    bool alt = false;
                    foreach (var d in items)
                    {
                        var bg = alt ? Colors.Grey.Lighten4 : Colors.White;
                        alt = !alt;
                        var cells = new[]
                        {
                            d.IdMonitoreo.ToString(),
                            d.NombreDispositivo ?? "",
                            d.Tipo ?? "",
                            d.Ip ?? "",
                            d.Ubicacion ?? "",
                            d.Estado ?? "",
                            $"{d.CpuPorcentaje ?? 0}",
                            $"{d.RamPorcentaje ?? 0}",
                            $"{d.LatenciaMs ?? 0}",
                            d.RiesgoIa ?? ""
                        };
                        foreach (var cell in cells)
                            table.Cell().Background(bg).Padding(3).Text(cell).FontSize(7.5f);
                    }
                });
            }
        }
    }
}
