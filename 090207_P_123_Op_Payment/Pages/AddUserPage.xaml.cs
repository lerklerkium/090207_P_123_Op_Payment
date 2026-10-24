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
    /// Логика взаимодействия для AddUserPage.xaml
    /// </summary>
    public partial class AddUserPage : Page
    {
        private User _currentUser = new User();
        private bool _isNew = true;

        public AddUserPage(User selectedUser)
        {
            InitializeComponent();
            if (selectedUser != null)
            {
                _currentUser = selectedUser;
                _isNew = false;
            }
            DataContext = _currentUser;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentUser.Login))
                errors.AppendLine("Укажите логин!");
            if (string.IsNullOrWhiteSpace(_currentUser.Password))
                errors.AppendLine("Укажите пароль!");
            if (cmbRole.SelectedValue == null)
                errors.AppendLine("Выберите роль!");
            if (string.IsNullOrWhiteSpace(_currentUser.FIO))
                errors.AppendLine("Укажите ФИО!");

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
                        // Перед сохранением нового пользователя из админки хэшируем его пароль по SHA1
                        _currentUser.Password = AuthPage.GetHash(_currentUser.Password);
                        db.User.Add(_currentUser);
                    }
                    else
                    {
                        // Если редактируем, прикрепляем объект к текущему контексту
                        db.Entry(_currentUser).State = System.Data.Entity.EntityState.Modified;
                    }

                    db.SaveChanges();
                    MessageBox.Show("Данные успешно сохранены!", "Успех");
                    NavigationService?.GoBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBLogin.Text = "";
            TBPass.Text = "";
            cmbRole.SelectedIndex = -1; // Сбрасываем выбор, не удаляя сами элементы
            TBFio.Text = "";
            TBPhoto.Text = "";
        }
    }
}
