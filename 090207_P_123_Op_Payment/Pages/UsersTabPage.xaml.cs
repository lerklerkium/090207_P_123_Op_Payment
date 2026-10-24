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
    /// Логика взаимодействия для UsersTabPage.xaml
    /// </summary>
    public partial class UsersTabPage : Page
    {
        public UsersTabPage()
        {
            InitializeComponent();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        // Обновление данных при открытии страницы
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
                    // Обновляем кэш Entity Framework, чтобы видеть актуальные изменения из БД
                    foreach (var entry in db.ChangeTracker.Entries())
                    {
                        entry.Reload();
                    }
                    DataGridUser.ItemsSource = db.User.AsNoTracking().ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddUserPage(null)); //adduserpage
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var usersForRemoving = DataGridUser.SelectedItems.Cast<User>().ToList();

            if (usersForRemoving.Count == 0)
            {
                MessageBox.Show("Выберите строки для удаления!", "Предупреждение");
                return;
            }

            if (MessageBox.Show($"Вы точно хотите удалить записи в количестве {usersForRemoving.Count} элементов?", "Внимание",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new DB_Op_PaymentEntities4())
                    {
                        // Привязываем сущности к текущему контексту для успешного удаления
                        foreach (var user in usersForRemoving)
                        {
                            db.Entry(user).State = System.Data.Entity.EntityState.Deleted;
                        }
                        db.SaveChanges();
                        MessageBox.Show("Данные успешно удалены!");
                    }
                    RefreshData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}");
                }
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = (sender as Button).DataContext as User;
            NavigationService?.Navigate(new AddUserPage(selectedUser));
        }
    }
}
