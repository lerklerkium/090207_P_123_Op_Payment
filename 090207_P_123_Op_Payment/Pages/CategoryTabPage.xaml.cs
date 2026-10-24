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
    /// Логика взаимодействия для CategoryTabPage.xaml
    /// </summary>
    public partial class CategoryTabPage : Page
    {
        public CategoryTabPage()
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
                    DataGridCategory.ItemsSource = db.Category.AsNoTracking().ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}");
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddCategoryPage(null));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var categoryForRemoving = DataGridCategory.SelectedItems.Cast<Category>().ToList();

            if (categoryForRemoving.Count == 0)
            {
                MessageBox.Show("Выберите категории для удаления!", "Предупреждение");
                return;
            }

            if (MessageBox.Show($"Вы точно хотите удалить записи в количестве {categoryForRemoving.Count} элементов?", "Внимание",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new DB_Op_PaymentEntities4())
                    {
                        foreach (var cat in categoryForRemoving)
                        {
                            db.Entry(cat).State = System.Data.Entity.EntityState.Deleted;
                        }
                        db.SaveChanges();
                        MessageBox.Show("Данные успешно удалены!");
                    }
                    RefreshData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении категорий: {ex.Message}\nВозможно, эти категории связаны с существующими платежами.");
                }
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var selectedCategory = (sender as Button).DataContext as Category;
            NavigationService?.Navigate(new AddCategoryPage(selectedCategory));
        }
    }
}
