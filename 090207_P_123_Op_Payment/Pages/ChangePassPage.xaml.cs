using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// Логика взаимодействия для ChangePassPage.xaml
    /// </summary>
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }

        // Локальный статический метод хэширования паролей SHA1
        private static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        // ОБРАБОТЧИК НАЖАТИЯ КНОПКИ СОХРАНЕНИЯ (Пункты 2, 3, 4, 5, 6 задания)
        private void ButtonSavePassword_Click(object sender, RoutedEventArgs e)
        {
            // 3. Проверка заполнения ВСЕХ полей формы
            if (string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
                string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password) ||
                string.IsNullOrEmpty(TbLogin.Text))
            {
                MessageBox.Show("Все поля обязательны к заполнению!", "Предупреждение");
                return;
            }

            string hashedPass = GetHash(CurrentPasswordBox.Password);

            try
            {
                // Открываем подключение к вашей БД с использованием корректного имени контекста
                using (var db = new DB_Op_PaymentEntities4())
                {
                    // 4. Проверка на правильно введенные текущие данные аккаунта (таблица .User)
                    var user = db.User.FirstOrDefault(u => u.Login == TbLogin.Text && u.Password == hashedPass);

                    if (user == null)
                    {
                        MessageBox.Show("Текущий пароль или Логин введены неверно!", "Ошибка доступа");
                        return;
                    }

                    // 5. Проверка корректности формата нового пароля (по аналогии с RegPage)
                    if (NewPasswordBox.Password.Length < 6)
                    {
                        MessageBox.Show("Новый пароль слишком короткий, должно быть минимум 6 символов!", "Ошибка безопасности");
                        return;
                    }

                    bool en = true;
                    bool number = false;

                    for (int i = 0; i < NewPasswordBox.Password.Length; i++)
                    {
                        char ch = NewPasswordBox.Password[i];
                        if (ch >= '0' && ch <= '9')
                            number = true;
                        else if (!((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z')))
                            en = false;
                    }

                    if (!en)
                    {
                        MessageBox.Show("Используйте только английскую раскладку для нового пароля!", "Ошибка ввода");
                        return;
                    }

                    if (!number)
                    {
                        MessageBox.Show("Новый пароль должен содержать хотя бы одну цифру!", "Ошибка ввода");
                        return;
                    }

                    // Проверка на совпадение нового пароля и подтверждения
                    if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
                    {
                        MessageBox.Show("Новый пароль и подтверждение не совпадают!", "Ошибка валидации");
                        return;
                    }

                    // 6. Сохранение изменений в базе данных при успешном прохождении проверок
                    if (en && number)
                    {
                        user.Password = GetHash(NewPasswordBox.Password); // Хэшируем и присваиваем новый пароль
                        db.SaveChanges(); // Сохраняем изменения в SQL Server

                        MessageBox.Show("Пароль успешно изменен!", "Успех");

                        // Перенаправляем пользователя обратно на страницу авторизации
                        NavigationService?.Navigate(new AuthPage());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при работе с базой данных: {ex.Message}", "Критический сбой");
            }
        }

        // МЕТОДЫ УПРАВЛЕНИЯ ВИДИМОСТЬЮ ПОДСКАЗОК-ПЛЕЙСХОЛДЕРОВ
        private void TbLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblLogHint != null)
                lblLogHint.Visibility = TbLogin.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (lblCurrentPassHint != null)
                lblCurrentPassHint.Visibility = CurrentPasswordBox.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (lblNewPassHint != null)
                lblNewPassHint.Visibility = NewPasswordBox.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (lblConfirmPassHint != null)
                lblConfirmPassHint.Visibility = ConfirmPasswordBox.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
