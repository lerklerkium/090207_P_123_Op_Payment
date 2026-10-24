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
    /// Логика взаимодействия для AddPaymentPage.xaml
    /// </summary>
    public partial class AddPaymentPage : Page
    {
        private Payment _currentPayment = new Payment();
        private bool _isNew = true;

        public AddPaymentPage(Payment selectedPayment)
        {
            InitializeComponent();

            try
            {
                // Подключаемся к вашей БД для заполнения выпадающих списков
                using (var db = new DB_Op_PaymentEntities4())
                {
                    CBCategory.ItemsSource = db.Category.AsNoTracking().ToList();
                    CBCategory.DisplayMemberPath = "Name";

                    CBUser.ItemsSource = db.User.AsNoTracking().ToList();
                    CBUser.DisplayMemberPath = "FIO";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации списков: {ex.Message}");
            }

            if (selectedPayment != null)
            {
                _currentPayment = selectedPayment;
                _isNew = false;
            }
            else
            {
                // Устанавливаем текущую дату для нового платежа по умолчанию
                _currentPayment.Date = DateTime.Now;
            }

            DataContext = _currentPayment;
        }

        // ОБРАБОТЧИК КНОПКИ "СОХРАНИТЬ" (Исправленная валидация из пункта 15)
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            // 1. Проверка названия платежа
            if (string.IsNullOrWhiteSpace(_currentPayment.Name))
                errors.AppendLine("Укажите название платежа!");

            // 2. Проверка даты
            if (_currentPayment.Date == DateTime.MinValue)
                errors.AppendLine("Укажите корректную дату!");

            // 3. Проверка количества (математическое сравнение вместо ToString)
            if (_currentPayment.Num <= 0)
                errors.AppendLine("Укажите количество (больше 0)!");

            // 4. Проверка цены / суммы
            if (_currentPayment.Price <= 0)
                errors.AppendLine("Укажите цену (больше 0)!");

            // 5. Проверка выбора клиента (внешний ключ UserID)
            if (_currentPayment.UserID <= 0)
                errors.AppendLine("Укажите клиента!");

            // 6. Проверка выбора категории (внешний ключ CategoryID)
            if (_currentPayment.CategoryID <= 0)
                errors.AppendLine("Укажите категорию!");

            // Если есть ошибки — выводим и прерываем сохранение
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Предупреждение");
                return;
            }

            try
            {
                using (var db = new DB_Op_PaymentEntities4())
                {
                    if (_isNew)
                    {
                        db.Payment.Add(_currentPayment);
                    }
                    else
                    {
                        // Если редактируем существующий, уведомляем EF об изменениях
                        db.Entry(_currentPayment).State = System.Data.Entity.EntityState.Modified;
                    }

                    db.SaveChanges();
                    MessageBox.Show("Данные успешно сохранены!", "Успех");
                    NavigationService?.GoBack(); // Возвращаемся обратно к таблице платежей
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка");
            }
        }

        // ОБРАБОТЧИК КНОПКИ "ОЧИСТИТЬ" (Исправленные имена элементов из пункта 15)
        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBPaymentName.Text = "";
            TBAmount.Text = "";
            TBCount.Text = "";
            TBDate.Text = DateTime.Now.ToString("dd.MM.yyyy");

            // Безопасный сброс ComboBox (CBCategory и CBUser вместо TBCategory и TBUser)
            CBCategory.SelectedIndex = -1;
            CBUser.SelectedIndex = -1;
        }
    }
}
