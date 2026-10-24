using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _090207_P_123_Op_Payment.Pages
{
    /// <summary>
    /// Логика взаимодействия для UserPage.xaml
    /// </summary>
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();
        }

        // Событие загрузки страницы — заполняем фильтр категорий из БД
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DB_Op_PaymentEntities4())
                {
                    // Подгружаем список категорий из базы
                    var categories = db.Category.AsNoTracking().ToList();

                    // Добавляем пустой элемент для сброса фильтрации
                    categories.Insert(0, new Category { ID = 0, Name = "Все категории" });

                    categoryComboBox.ItemsSource = categories;
                    categoryComboBox.SelectedIndex = 0; // Выбираем "Все категории" по умолчанию
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка категорий: {ex.Message}", "Ошибка");
            }

            // Принудительно вызываем первоначальный вывод таблицы
            UpdateData();
        }

        // Обработчики смены фильтров
        private void FilterChanged(object sender, SelectionChangedEventArgs e) => UpdateData();
        private void FilterChanged(object sender, TextChangedEventArgs e) => UpdateData();

        // Кнопка полной очистки параметров поиска
        private void clearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            fioFilterTextBox.Text = "";
            categoryComboBox.SelectedIndex = 0;
            sortComboBox.SelectedIndex = 0;
            UpdateData();
        }

        // Единый метод многокритериального поиска и вывода в DataGrid
        private void UpdateData()
        {
            // Блокируем вызов метода, если визуальные компоненты еще не созданы в памяти
            if (!IsInitialized) return;

            try
            {
                using (var db = new DB_Op_PaymentEntities4())
                {
                    // Подгружаем таблицу платежей вместе со связанными записями User и Category
                    var currentPayments = db.Payment
                        .Include("User")
                        .Include("Category")
                        .AsNoTracking()
                        .ToList();

                    // 1. Поиск по ФИО пользователя (без учета регистра букв)
                    if (!string.IsNullOrWhiteSpace(fioFilterTextBox.Text))
                    {
                        currentPayments = currentPayments
                            .Where(x => x.User != null && x.User.FIO != null &&
                                   x.User.FIO.ToLower().Contains(fioFilterTextBox.Text.ToLower()))
                            .ToList();
                    }

                    // 2. Фильтрация по категории (срабатывает, если выбран конкретный элемент с ID > 0)
                    if (categoryComboBox.SelectedValue is int selectedCategoryId && selectedCategoryId > 0)
                    {
                        currentPayments = currentPayments
                            .Where(x => x.CategoryID == selectedCategoryId)
                            .ToList();
                    }

                    // 3. Сортировка по датам и алфавитному порядку
                    switch (sortComboBox.SelectedIndex)
                    {
                        case 0: // По дате (сначала новые)
                            currentPayments = currentPayments.OrderByDescending(x => x.Date).ToList();
                            break;
                        case 1: // По дате (сначала старые)
                            currentPayments = currentPayments.OrderBy(x => x.Date).ToList();
                            break;
                        case 2: // По алфавиту ФИО (А-Я)
                            currentPayments = currentPayments.OrderBy(x => x.User?.FIO).ToList();
                            break;
                        case 3: // По алфавиту ФИО (Я-А)
                            currentPayments = currentPayments.OrderByDescending(x => x.User?.FIO).ToList();
                            break;
                    }

                    // Отправляем готовый и отсортированный массив данных в стандартную таблицу DataGrid
                    DataGridUserPayments.ItemsSource = currentPayments;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления таблицы платежей: {ex.Message}", "Ошибка");
            }
        }
    }
}
