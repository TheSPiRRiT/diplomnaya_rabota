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

namespace uchetishee
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        prodajiEntities bd = new prodajiEntities();
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void click_Click(object sender, RoutedEventArgs e)
        {

            prodajiEntities db = new prodajiEntities();
            string log = login.Text;
            string password = pass.Password;


            try
            {
                var manager = db.UZ_manager.Where((u) => u.login_manager == log && u.password_manager == password).Single();

                zak zak = new zak();
                zak.Show();
                this.Close();

            }
            catch
            {
                trigg.Visibility = Visibility.Visible;
            }
        }
    }
}
