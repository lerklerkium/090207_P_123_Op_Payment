using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Drawing;
using _090207_P_123_Op_Payment.Pages;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;

namespace _090207_P_123_Op_Payment.Pages
{
    /// <summary>
    /// Логика взаимодействия для DiagrammPage.xaml
    /// </summary>
    public partial class DiagrammPage : Page
    {
        // Создаем контекст базы данных
        private DB_Op_PaymentEntities4 _context = new DB_Op_PaymentEntities4();

        // Класс для структуры данных графика
        public class ChartItem
        {
            public string CategoryName { get; set; }
            public double Value { get; set; }
            public double Height { get; set; }
        }

        public DiagrammPage()
        {
            InitializeComponent();

            // ЖЕЛЕЗНЫЙ ФИКС: Загружаем пользователей сразу при создании страницы
            LoadUsersData();
        }

        // Вынесли загрузку в отдельный метод для надежности
        private void LoadUsersData()
        {
            try
            {
                // Загружаем список пользователей из базы данных
                var usersList = _context.User.AsNoTracking().ToList();

                if (usersList.Count == 0)
                {
                    MessageBox.Show("В базе данных нет пользователей! Диаграмма не может быть построена.", "Внимание");
                    return;
                }

                CmbUser.ItemsSource = usersList;

                // Автоматически выбираем первого пользователя в списке, чтобы график построился сам
                CmbUser.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критическая ошибка загрузки пользователей из БД: {ex.Message}\nПроверьте строку подключения в App.config!", "Ошибка БД");
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Если при открытии страницы список почему-то пуст — пробуем загрузить еще раз
            if (CmbUser.ItemsSource == null)
            {
                LoadUsersData();
            }
        }

        // Метод расчета и принудительного рендеринга столбцов диаграммы
        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            // Проверяем, что страница инициализирована и пользователь действительно выбран
            if (!IsInitialized || CmbUser.SelectedItem == null) return;

            if (CmbUser.SelectedItem is User currentUser)
            {
                try
                {
                    var categoriesList = _context.Category.AsNoTracking().ToList();
                    var paymentsList = _context.Payment.AsNoTracking().ToList();
                    List<ChartItem> chartData = new List<ChartItem>();

                    foreach (var category in categoriesList)
                    {
                        // Считаем сумму платежей по конкретной категории для выбранного пользователя
                        double totalSum = paymentsList
                            .Where(u => u.UserID == currentUser.ID && u.CategoryID == category.ID)
                            .Sum(u => (double)(u.Price * u.Num));

                        if (totalSum > 0)
                        {
                            chartData.Add(new ChartItem
                            {
                                CategoryName = category.Name,
                                Value = totalSum
                            });
                        }
                    }

                    // Если у пользователя вообще нет платежей в базе данных
                    if (chartData.Count == 0)
                    {
                        WpfChart.ItemsSource = null;
                        return;
                    }

                    // Вычисляем высоту столбцов (максимальный столбец равен 180 пикселей)
                    double maxValue = chartData.Max(x => x.Value);
                    if (maxValue > 0)
                    {
                        foreach (var item in chartData)
                        {
                            item.Height = (item.Value / maxValue) * 180;
                        }
                    }

                    // Принудительно сбрасываем и обновляем визуальное отображение столбцов WPF
                    WpfChart.ItemsSource = null;
                    WpfChart.ItemsSource = chartData;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка расчета диаграммы: {ex.Message}", "Ошибка");
                }
            }
        }

        // ШАГ 13. ИСПРАВЛЕННАЯ ВЫГРУЗКА ДАННЫХ В MICROSOFT WORD
        private void BtnExportWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allUsers = _context.User.ToList();
                var allCategories = _context.Category.ToList();

                var application = new Word.Application();
                Word.Document document = application.Documents.Add();

                foreach (var user in allUsers)
                {
                    // Абзац для ФИО пользователя (Заголовок)
                    Word.Paragraph userParagraph = document.Paragraphs.Add();
                    Word.Range userRange = userParagraph.Range;
                    userRange.Text = user.FIO;
                    userRange.Font.Name = "Times New Roman";
                    userRange.Font.Size = 18;
                    userRange.Font.Bold = 1;
                    userRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    userRange.InsertParagraphAfter(); // Метод вызывается СТРОГО у Range

                    document.Paragraphs.Add();

                    // Создание и форматирование таблицы
                    Word.Paragraph tableParagraph = document.Paragraphs.Add();
                    Word.Range tableRange = tableParagraph.Range;
                    Word.Table paymentsTable = document.Tables.Add(tableRange, allCategories.Count + 1, 2);

                    paymentsTable.Borders.InsideLineStyle = paymentsTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    paymentsTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                    // Названия колонок таблицы
                    Word.Range cellRange;
                    cellRange = paymentsTable.Cell(1, 1).Range;
                    cellRange.Text = "Категория";
                    cellRange = paymentsTable.Cell(1, 2).Range;
                    cellRange.Text = "Сумма расходов";

                    // ИСПРАВЛЕНО: Свойства шрифта применяются к общему Range таблицы, чтобы избежать ошибок с коллекцией Rows
                    paymentsTable.Range.Font.Name = "Times New Roman";
                    paymentsTable.Range.Font.Size = 12;

                    // Выделяем жирным шрифтом и центруем строго первую строку (заголовки)
                    paymentsTable.Rows[1].Range.Font.Bold = 1;
                    paymentsTable.Rows[1].Range.Font.Size = 14;
                    paymentsTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    // Заполнение таблицы платежами (цикл по строкам)
                    for (int i = 0; i < allCategories.Count; i++)
                    {
                        var currentCategory = allCategories[i];

                        cellRange = paymentsTable.Cell(i + 2, 1).Range;
                        cellRange.Text = currentCategory.Name;

                        cellRange = paymentsTable.Cell(i + 2, 2).Range;
                        double sum = user.Payment.Where(u => u.CategoryID == currentCategory.ID).Sum(u => (double)(u.Num * u.Price));
                        cellRange.Text = sum.ToString("N2") + " руб.";
                    }

                    document.Paragraphs.Add();

                    // Расчет максимального платежа
                    var maxPayment = user.Payment.OrderByDescending(u => u.Price * u.Num).FirstOrDefault();
                    if (maxPayment != null)
                    {
                        Word.Paragraph maxPaymentParagraph = document.Paragraphs.Add();
                        Word.Range maxPaymentRange = maxPaymentParagraph.Range;
                        maxPaymentRange.Text = $"Самый дорогостоящий платеж - {maxPayment.Name} за {(maxPayment.Price * maxPayment.Num).ToString("N2")} руб. от {maxPayment.Date.ToString("dd.MM.yyyy")}";
                        maxPaymentRange.Font.Name = "Times New Roman";
                        maxPaymentRange.Font.Size = 14;
                        maxPaymentRange.Font.Bold = 1;
                        maxPaymentRange.Font.Color = Word.WdColor.wdColorDarkRed;
                        maxPaymentRange.InsertParagraphAfter(); // Вызывается у Range
                    }

                    document.Paragraphs.Add();

                    // Расчет минимального платежа
                    var minPayment = user.Payment.OrderBy(u => u.Price * u.Num).FirstOrDefault();
                    if (minPayment != null)
                    {
                        Word.Paragraph minPaymentParagraph = document.Paragraphs.Add();
                        Word.Range minPaymentRange = minPaymentParagraph.Range;
                        minPaymentRange.Text = $"Самый дешевый платеж - {minPayment.Name} за {(minPayment.Price * minPayment.Num).ToString("N2")} руб. от {minPayment.Date.ToString("dd.MM.yyyy")}";
                        minPaymentRange.Font.Name = "Times New Roman";
                        minPaymentRange.Font.Size = 14;
                        minPaymentRange.Font.Bold = 1;
                        minPaymentRange.Font.Color = Word.WdColor.wdColorDarkGreen;
                        minPaymentRange.InsertParagraphAfter(); // Вызывается у Range
                    }

                    if (user != allUsers.LastOrDefault())
                    {
                        document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                    }
                }

                // Нижний колонтитул с номером страницы
                foreach (Word.Section section in document.Sections)
                {
                    Word.HeaderFooter footer = section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                    footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter);
                }

                // Верхний колонтитул с текущей датой
                foreach (Word.Section section in document.Sections)
                {
                    Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.Text = $"Отчет сформирован: {DateTime.Now.ToString("dd.MM.yyyy")}";
                    headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    headerRange.Font.ColorIndex = Word.WdColorIndex.wdBlack;
                    headerRange.Font.Size = 10;
                    headerRange.Font.Name = "Times New Roman";
                }

                application.Visible = true;

                string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                document.SaveAs2(Path.Combine(myDocumentsPath, "Payments.docx"));
                document.SaveAs2(Path.Combine(myDocumentsPath, "Payments.pdf"), Word.WdExportFormat.wdExportFormatPDF);
                MessageBox.Show("Экспорт в Word успешно завершен!", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Word: {ex.Message}", "Критический сбой");
            }
        }
        // ШАГ 14. ЭКСПОРТ В EXCEL
        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allUsers = _context.User.ToList().OrderBy(u => u.FIO).ToList();
                double grandTotal = 0;
                var application = new Excel.Application();
                application.SheetsInNewWorkbook = allUsers.Count;
                Excel.Workbook workbook = application.Workbooks.Add(Type.Missing);
                for (int i = 0; i < allUsers.Count; i++)
                {
                    int startRowIndex = 1;
                    Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Worksheets[i + 1];
                    worksheet.Name = allUsers[i].FIO.Length > 30 ? allUsers[i].FIO.Substring(0, 30) : allUsers[i].FIO;
                    worksheet.Cells[startRowIndex, 1] = "Дата платежа";
                    worksheet.Cells[startRowIndex, 2] = "Название";
                    worksheet.Cells[startRowIndex, 3] = "Стоимость";
                    worksheet.Cells[startRowIndex, 4] = "Количество";
                    worksheet.Cells[startRowIndex, 5] = "Сумма";
                    Excel.Range columnHeaderRange = worksheet.Range[worksheet.Cells[startRowIndex, 1], worksheet.Cells[startRowIndex, 5]];
                    columnHeaderRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    columnHeaderRange.Font.Bold = true;
                    startRowIndex++;
                    var userCategories = allUsers[i].Payment.OrderBy(u => u.Date).GroupBy(u => u.Category).OrderBy(u => u.Key.Name);
                    foreach (var groupCategory in userCategories)
                    {
                        Excel.Range headerRange = worksheet.Range[worksheet.Cells[startRowIndex, 1], worksheet.Cells[startRowIndex, 5]];
                        headerRange.Merge();
                        headerRange.Value = groupCategory.Key.Name;
                        headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        headerRange.Font.Italic = true;
                        startRowIndex++;
                        int firstPaymentRow = startRowIndex;
                        foreach (var payment in groupCategory)
                        {
                            worksheet.Cells[startRowIndex, 1] = payment.Date.ToString("dd.MM.yyyy");
                            worksheet.Cells[startRowIndex, 2] = payment.Name;
                            worksheet.Cells[startRowIndex, 3] = payment.Price;
                            (worksheet.Cells[startRowIndex, 3] as Excel.Range).NumberFormat = "0.00";
                            worksheet.Cells[startRowIndex, 4] = payment.Num;
                            worksheet.Cells[startRowIndex, 5].Formula = $"=C{startRowIndex}*D{startRowIndex}";
                            (worksheet.Cells[startRowIndex, 5] as Excel.Range).NumberFormat = "0.00";
                            grandTotal += (double)(payment.Price * payment.Num);
                            startRowIndex++;
                        }
                        Excel.Range sumRange = worksheet.Range[worksheet.Cells[startRowIndex, 1], worksheet.Cells[startRowIndex, 4]];
                        sumRange.Merge();
                        sumRange.Value = "ИТОГО:";
                        sumRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                        worksheet.Cells[startRowIndex, 5].FormulaLocal = $"=СУММ(E{firstPaymentRow}:E{startRowIndex - 1})";
                        sumRange.Font.Bold = true;
                        (worksheet.Cells[startRowIndex, 5] as Excel.Range).Font.Bold = true;
                        (worksheet.Cells[startRowIndex, 5] as Excel.Range).NumberFormat = "0.00";
                        startRowIndex++;
                    }
                    Excel.Range rangeBorders = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[startRowIndex - 1, 5]];
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlInsideHorizontal].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlInsideVertical].LineStyle =
                    Excel.XlLineStyle.xlContinuous;
                    worksheet.Columns.AutoFit();
                }
                // Создание итогового листа «Общий итог»
                Excel.Worksheet summarySheet = (Excel.Worksheet)workbook.Worksheets.Add(Type.Missing, workbook.Worksheets[workbook.Worksheets.Count], Type.Missing, Type.Missing);
                summarySheet.Name = "Общий итог";
                summarySheet.Cells[1, 1] = "Общий итог:";
                summarySheet.Cells[1, 2] = grandTotal;
                ((Excel.Range)summarySheet.Cells[1, 2]).NumberFormat = "0.00 руб.";
                // Успешный перевод цвета System.Drawing.Color в OLE формат для Excel
                Excel.Range summaryRange = summarySheet.Range[summarySheet.Cells[1, 1], summarySheet.Cells[1, 2]];
                summaryRange.Font.Color = ColorTranslator.ToOle(Color.Red); // Сработает благодаря ссылке на System.Drawing
                summaryRange.Font.Bold = true;
                summarySheet.Columns.AutoFit();
                application.Visible = true;
                MessageBox.Show("Экспорт в Excel успешно завершен!", "Успех");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}", "Критический сбой");
            }
        }
    }
}
