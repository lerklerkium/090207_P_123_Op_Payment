using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using _090207_P_123_Op_Payment;

namespace _090207_P_123_Op_Payment.Pages
{
    /// <summary>
    /// Логика взаимодействия для RegPage.xaml
    /// </summary>
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
            // Выбор роли по умолчанию (пункт 3 задания)
            comboBxRole.SelectedIndex = 0;
        }

        // Обработчик нажатия на кнопку «Зарегистрироваться»
        private void regButton_Click(object sender, RoutedEventArgs e)
        {
            // 6.1. Проверка на заполнение полей
            if (string.IsNullOrEmpty(txtbxLog.Text) ||
                string.IsNullOrEmpty(txtbxFIO.Text) ||
                string.IsNullOrEmpty(passBxFrst.Password) ||
                string.IsNullOrEmpty(passBxScnd.Password))
            {
                MessageBox.Show("Заполните все поля!", "Предупреждение");
                return;
            }

            try
            {
                using (DB_Op_PaymentEntities4 db = new DB_Op_PaymentEntities4())
                {
                    // 6.2. Проверка наличия логина в базе данных
                    var userExists = db.User.AsNoTracking().FirstOrDefault(u => u.Login == txtbxLog.Text);
                    if (userExists != null)
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка уникальности");
                        return;
                    }

                    // 6.3. Проверка формата введенного пароля (длина не менее 6 символов)
                    if (passBxFrst.Password.Length < 6)
                    {
                        MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!", "Ошибка безопасности");
                        return;
                    }

                    bool en = true;
                    bool number = false;

                    for (int i = 0; i < passBxFrst.Password.Length; i++)
                    {
                        char ch = passBxFrst.Password[i];
                        if (ch >= '0' && ch <= '9')
                            number = true;
                        else if (!((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z')))
                            en = false;
                    }

                    if (!en)
                    {
                        MessageBox.Show("Используйте только английскую раскладку для пароля!", "Ошибка ввода");
                        return;
                    }

                    if (!number)
                    {
                        MessageBox.Show("Добавьте в пароль хотя бы одну цифру!", "Ошибка ввода");
                        return;
                    }

                    // 7.4. Проверка на совпадение двух введенных паролей
                    if (passBxFrst.Password != passBxScnd.Password)
                    {
                        MessageBox.Show("Пароли не совпадают!", "Ошибка валидации");
                        return;
                    }

                    // 7.5. Если все проверки пройдены — хэшируем пароль и сохраняем сущность в БД
                    string hashedPassword = AuthPage.GetHash(passBxFrst.Password);

                    User userObject = new User
                    {
                        FIO = txtbxFIO.Text,
                        Login = txtbxLog.Text,
                        Password = hashedPassword, // Запись защищенного хэша
                        Role = (comboBxRole.SelectedItem as ComboBoxItem)?.Content.ToString()
                    };

                    db.User.Add(userObject);
                    db.SaveChanges();

                    MessageBox.Show("Пользователь успешно зарегистрирован!", "Успех");

                    // Очистка полей формы после успешной работы
                    txtbxLog.Clear();
                    passBxFrst.Clear();
                    passBxScnd.Clear();
                    txtbxFIO.Clear();
                    comboBxRole.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить пользователя в БД: {ex.Message}", "Критический сбой");
            }
        }

        // Передача фокуса текстовым полям при клике мышкой по плейсхолдерам (Пункт 4 задания)
        private void lblLogHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => txtbxLog.Focus();
        private void lblPassHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => passBxFrst.Focus();
        private void lblPassSecHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => passBxScnd.Focus();
        private void lblFioHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => txtbxFIO.Focus();

        // Динамическое скрытие или отображение заполнителей при изменении текста
        private void txtbxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblLogHitn != null)
                lblLogHitn.Visibility = txtbxLog.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void txtbxFIO_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblFioHitn != null)
                lblFioHitn.Visibility = txtbxFIO.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void passBxFrst_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (lblPassHitn != null)
                lblPassHitn.Visibility = passBxFrst.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void passBxScnd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (lblPassSecHitn != null)
                lblPassSecHitn.Visibility = passBxScnd.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
