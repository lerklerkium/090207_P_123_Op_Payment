using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using _090207_P_123_Op_Payment;

namespace _090207_P_123_Op_Payment.Pages
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;

        public AuthPage()
        {
            InitializeComponent();
        }

        // Общий статический метод хэширования SHA1
        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        // Обработчик кнопки "Вход"
        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxLogin.Text) || string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин или пароль!", "Предупреждение");
                return;
            }

            string hashedPassword = GetHash(PasswordBox.Password);

            try
            {
                using (var db = new DB_Op_PaymentEntities4())
                {
                    // Поиск пользователя по логину и совпадению хэшированного пароля
                    var user = db.User
                        .AsNoTracking()
                        .FirstOrDefault(u => u.Login == TextBoxLogin.Text && u.Password == hashedPassword);

                    if (user == null)
                    {
                        MessageBox.Show("Пользователь с такими данными не найден!", "Ошибка авторизации");
                        failedAttempts++;

                        if (failedAttempts >= 3)
                        {
                            if (captcha.Visibility != Visibility.Visible)
                            {
                                CaptchaSwitch();
                            }
                            CaptchaChange();
                        }
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Пользователь успешно найден!", "Успех");
                        failedAttempts = 0;

                        // Переходы по ролям (строго соответствуют записям в БД)
                        switch (user.Role)
                        {
                            case "Пользователь":
                            case "User":
                                NavigationService?.Navigate(new UserPage());
                                break;
                            case "Администратор":
                            case "Admin":
                                NavigationService?.Navigate(new AdminPage());
                                break;
                            default:
                                MessageBox.Show("Роль пользователя не распознана системой.", "Ошибка ролей");
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Критический сбой");
            }
        }

        // Переключение видимости интерфейса при блокировке капчей
        public void CaptchaSwitch()
        {
            if (captcha.Visibility == Visibility.Visible)
            {
                TextBoxLogin.Clear();
                PasswordBox.Clear();

                captcha.Visibility = Visibility.Hidden;
                captchaInput.Visibility = Visibility.Hidden;
                labelCaptcha.Visibility = Visibility.Hidden;
                submitCaptcha.Visibility = Visibility.Hidden;

                labelLogin.Visibility = Visibility.Visible;
                labelPass.Visibility = Visibility.Visible;
                TextBoxLogin.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Visible;
                ButtonChangePassword.Visibility = Visibility.Visible;
                ButtonEnter.Visibility = Visibility.Visible;
                ButtonReg.Visibility = Visibility.Visible;
            }
            else
            {
                captcha.Visibility = Visibility.Visible;
                captchaInput.Visibility = Visibility.Visible;
                labelCaptcha.Visibility = Visibility.Visible;
                submitCaptcha.Visibility = Visibility.Visible;

                labelLogin.Visibility = Visibility.Hidden;
                labelPass.Visibility = Visibility.Hidden;
                TextBoxLogin.Visibility = Visibility.Hidden;
                txtHintLogin.Visibility = Visibility.Hidden;
                PasswordBox.Visibility = Visibility.Hidden;
                txtHintPass.Visibility = Visibility.Hidden;

                ButtonChangePassword.Visibility = Visibility.Hidden;
                ButtonEnter.Visibility = Visibility.Hidden;
                ButtonReg.Visibility = Visibility.Hidden;
            }
        }

        // Генератор случайной текстовой капчи
        public void CaptchaChange()
        {
            string allowchar = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
            allowchar += ",a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,y,z";
            allowchar += ",1,2,3,4,5,6,7,8,9,0";

            string[] ar = allowchar.Split(',');
            string pwd = "";
            Random r = new Random();

            for (int i = 0; i < 6; i++)
            {
                pwd += ar[r.Next(0, ar.Length)];
            }
            captcha.Text = pwd;
        }

        // Кнопка проверки введенной капчи
        private void submitCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (captchaInput.Text != captcha.Text)
            {
                MessageBox.Show("Неверно введена капча!", "Ошибка");
                CaptchaChange();
            }
            else
            {
                MessageBox.Show("Капча введена успешно, можете продолжить авторизацию.", "Успех");
                CaptchaSwitch();
                failedAttempts = 0;
            }
        }

        // Блокировка команд копирования, вставки и вырезания текста капчи
        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        // Логика отображения водяных знаков (подсказок)
        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtHintLogin != null)
                txtHintLogin.Visibility = TextBoxLogin.Text.Length > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (txtHintPass != null)
                txtHintPass.Visibility = PasswordBox.Password.Length > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        // Переходы по кнопкам
        private void ButtonReg_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new RegPage());
        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e) => NavigationService?.Navigate(new ChangePassPage());
    }
}
