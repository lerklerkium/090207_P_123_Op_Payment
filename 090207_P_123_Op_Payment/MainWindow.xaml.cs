using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;

namespace _090207_P_123_Op_Payment
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.IsEnabled = true;
            timer.Tick += (o, t) => { DateTimeNow.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"); };
            timer.Start();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы действительно хотите выйти из приложения?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No) e.Cancel = true;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack) MainFrame.GoBack();
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeComboBox == null) return;
            string themeFile = ThemeComboBox.SelectedIndex == 1 ? "DictionaryCustom.xaml" : "Dictionary.xaml";
            try
            {
                var uri = new Uri(themeFile, UriKind.Relative);
                ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
                if (resourceDict != null)
                {
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(resourceDict);
                }
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка тем: {ex.Message}"); }
        }

        // АВТОМАТИЧЕСКАЯ СМЕНА ЗАГОЛОВКА ОКНА ПРИ ПЕРЕХОДЕ МЕЖДУ СТРАНИЦАМИ
        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Проверяем, что текущий контент во фрейме — это WPF страница
            if (MainFrame.Content is Page currentPage)
            {
                // Присваиваем окну заголовок текущей открытой страницы
                this.Title = currentPage.Title;
            }
        }
    }
}
