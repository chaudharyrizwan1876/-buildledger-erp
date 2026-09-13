using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using BuildLedger.Application.DTOs;

namespace BuildLedger.App.Services;

/// <summary>Builds and prints a simple, professional invoice/report layout using FlowDocument + PrintDialog.</summary>
public static class InvoicePrintService
{
    public static void PrintSaleInvoice(SaleDetailDto sale, string businessName)
    {
        var doc = new FlowDocument
        {
            PagePadding = new Thickness(40),
            FontFamily = new FontFamily("Segoe UI"),
            ColumnWidth = 700
        };

        doc.Blocks.Add(new Paragraph(new Run(businessName)) { FontSize = 20, FontWeight = FontWeights.Bold });
        doc.Blocks.Add(new Paragraph(new Run("Sales Invoice")) { FontSize = 13, Foreground = Brushes.Gray, Margin = new Thickness(0, 0, 0, 16) });

        var header = new Table();
        header.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        header.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        var headerGroup = new TableRowGroup();
        header.RowGroups.Add(headerGroup);
        var headerRow = new TableRow();
        headerGroup.Rows.Add(headerRow);
        headerRow.Cells.Add(InfoCell($"Invoice #: {sale.InvoiceNumber}\nDate: {sale.SaleDate:dd MMM yyyy}"));
        headerRow.Cells.Add(InfoCell($"Customer: {sale.CustomerName}\nSite: {sale.SiteName ?? "-"}"));
        doc.Blocks.Add(header);

        var itemsTable = new Table { CellSpacing = 0, Margin = new Thickness(0, 20, 0, 0) };
        itemsTable.Columns.Add(new TableColumn { Width = new GridLength(3, GridUnitType.Star) });
        itemsTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        itemsTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        itemsTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        itemsTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

        var group = new TableRowGroup();
        itemsTable.RowGroups.Add(group);

        var headRow = new TableRow { Background = Brushes.LightGray };
        headRow.Cells.Add(HeadCell("Product"));
        headRow.Cells.Add(HeadCell("Unit"));
        headRow.Cells.Add(HeadCell("Qty"));
        headRow.Cells.Add(HeadCell("Rate"));
        headRow.Cells.Add(HeadCell("Amount"));
        group.Rows.Add(headRow);

        foreach (var line in sale.Lines)
        {
            var row = new TableRow();
            row.Cells.Add(Cell(line.ProductName));
            row.Cells.Add(Cell(line.Unit));
            row.Cells.Add(Cell(line.Quantity.ToString("N2")));
            row.Cells.Add(Cell(line.UnitRate.ToString("N2")));
            row.Cells.Add(Cell(line.LineTotal.ToString("N2")));
            group.Rows.Add(row);
        }
        doc.Blocks.Add(itemsTable);

        var totals = new Paragraph { TextAlignment = TextAlignment.Right, Margin = new Thickness(0, 16, 0, 0) };
        totals.Inlines.Add(new Run($"Subtotal: {sale.SubTotal:N2}\n"));
        totals.Inlines.Add(new Run($"Discount: {sale.DiscountAmount:N2}\n"));
        totals.Inlines.Add(new Run($"Total: {sale.Total:N2}\n") { FontWeight = FontWeights.Bold, FontSize = 15 });
        totals.Inlines.Add(new Run($"Paid: {sale.AmountPaid:N2}\n"));
        totals.Inlines.Add(new Run($"Balance Due: {sale.AmountDue:N2}") { FontWeight = FontWeights.Bold });
        doc.Blocks.Add(totals);

        Print(doc);
    }

    public static void PrintTable(string title, IEnumerable<string> columnHeaders, IEnumerable<IEnumerable<string>> rows, string businessName)
    {
        var doc = new FlowDocument { PagePadding = new Thickness(40), FontFamily = new FontFamily("Segoe UI"), ColumnWidth = 750 };
        doc.Blocks.Add(new Paragraph(new Run(businessName)) { FontSize = 20, FontWeight = FontWeights.Bold });
        doc.Blocks.Add(new Paragraph(new Run(title)) { FontSize = 13, Foreground = Brushes.Gray, Margin = new Thickness(0, 0, 0, 16) });
        doc.Blocks.Add(new Paragraph(new Run($"Generated: {DateTime.Now:dd MMM yyyy HH:mm}")) { FontSize = 10, Foreground = Brushes.Gray });

        var headers = columnHeaders.ToList();
        var table = new Table { CellSpacing = 0, Margin = new Thickness(0, 16, 0, 0) };
        foreach (var _ in headers) table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

        var group = new TableRowGroup();
        table.RowGroups.Add(group);

        var headRow = new TableRow { Background = Brushes.LightGray };
        foreach (var h in headers) headRow.Cells.Add(HeadCell(h));
        group.Rows.Add(headRow);

        foreach (var row in rows)
        {
            var r = new TableRow();
            foreach (var cell in row) r.Cells.Add(Cell(cell));
            group.Rows.Add(r);
        }
        doc.Blocks.Add(table);

        Print(doc);
    }

    private static TableCell InfoCell(string text) => new(new Paragraph(new Run(text))) { Padding = new Thickness(4) };
    private static TableCell HeadCell(string text) => new(new Paragraph(new Run(text)) { FontWeight = FontWeights.Bold }) { Padding = new Thickness(6, 4, 6, 4), BorderBrush = Brushes.DarkGray, BorderThickness = new Thickness(0, 0, 0, 1) };
    private static TableCell Cell(string text) => new(new Paragraph(new Run(text))) { Padding = new Thickness(6, 4, 6, 4), BorderBrush = Brushes.LightGray, BorderThickness = new Thickness(0, 0, 0, 1) };

    private static void Print(FlowDocument doc)
    {
        var dialog = new PrintDialog();
        if (dialog.ShowDialog() != true) return;

        doc.PageHeight = dialog.PrintableAreaHeight;
        doc.PageWidth = dialog.PrintableAreaWidth;

        var paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
        dialog.PrintDocument(paginator, "BuildLedger ERP");
    }
}
