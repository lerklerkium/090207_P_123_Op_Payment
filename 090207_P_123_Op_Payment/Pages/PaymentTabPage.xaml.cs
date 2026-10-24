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
    /// Логика взаимодействия для PaymentTabPage.xaml
    /// </summary>
    public partial class PaymentTabPage : Page
    {
        public PaymentTabPage()
        {
            InitializeComponent();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                RefreshData();
            }
        }

        private void RefreshData()
        {
            try
            {
                using (var db = new DB_Op_PaymentEntities4())
                {
                    foreach (var entry in db.ChangeTracker.Entries())
                    {
                        entry.Reload();
                    }
                    // Подгружаем связанные таблицы User и Category для корректного отображения ФИО и Категории
                    DataGridPayment.ItemsSource = db.Payment
                        .Include("User")
                        .Include("Category")
                        .AsNoTracking()
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке платежей: {ex.Message}");
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddPaymentPage(null));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var paymentForRemoving = DataGridPayment.SelectedItems.Cast<Payment>().ToList();

            if (paymentForRemoving.Count == 0)
            {
                MessageBox.Show("Выберите платежи для удаления!", "Предупреждение");
                return;
            }

            if (MessageBox.Show($"Вы точно хотите удалить записи в количестве {paymentForRemoving.Count} элементов?", "Внимание",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new DB_Op_PaymentEntities4())
                    {
                        foreach (var pay in paymentForRemoving)
                        {
                            db.Entry(pay).State = System.Data.Entity.EntityState.Deleted;
                        }
                        db.SaveChanges();
                        MessageBox.Show("Данные успешно удалены!");
                    }
                    RefreshData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении платежей: {ex.Message}");
                }
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var selectedPayment = (sender as Button).DataContext as Payment;
            NavigationService?.Navigate(new AddPaymentPage(selectedPayment));
        }
    }
}
