using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using System.Windows.Shapes;

namespace uchetishee
{
    /// <summary>
    /// Логика взаимодействия для dobavlenie.xaml
    /// </summary>
    public partial class dobavlenie : Window
    {

        private const string connectionString = "Data Source=DESKTOP-HDQ72AK\\SQLEXPRESS;Initial Catalog=prodaji;Integrated Security=True";

        public dobavlenie()
        {
            InitializeComponent();
            // Инициализация ComboBox данными из таблиц status_klient и spos_opl
            InitializeComboBoxes();
        }

        private void InitializeComboBoxes()
        {
            // Заполнение ComboBox данными из таблицы status_klient
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT naim_status_kli FROM status_klient";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    status.Items.Add(reader.GetString(0));
                }
                reader.Close();
            }

            // Заполнение ComboBox данными из таблицы spos_opl
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT spos_opl FROM spos_opl";
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    spos.Items.Add(reader.GetString(0));
                }
                reader.Close();
            }
        }

        private void vihod_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void tov_Click(object sender, RoutedEventArgs e)
        {
            tovari tov = new tovari();
            tov.Show();
            this.Close();
        }

        private void zaka_Click(object sender, RoutedEventArgs e)
        {
            zak zak = new zak();
            zak.Show();
            this.Close();
        }

        private void dob_Click(object sender, RoutedEventArgs e)
        {
            AddDataToDatabase();
        }

        private void AddDataToDatabase()
        {
            // Получение выбранных элементов из ComboBox
            string selectedStatus = status.SelectedItem.ToString();
            string selectedSpos = spos.SelectedItem.ToString();

            // Получение id выбранных элементов из таблиц status_klient и spos_opl
            int statusId = GetIdFromTable("status_klient", "naim_status_kli", selectedStatus);
            int sposId = GetIdFromTable("spos_opl", "spos_opl", selectedSpos);

            // Получение данных из текстовых полей
            string firstName = imya.Text;
            string lastName = familiya.Text;
            string middleName = otch.Text;
            string company = komp.Text;
            string phone = tel.Text;
            string emailText = email.Text;
            string innText = inn.Text;
            string managerIdText = manag.Text; // Это строка, потому что id менеджера в базе может быть числом или текстом, в зависимости от вашей реализации

            // Вставка данных в таблицу zakaz
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO zakaz (id_kli, id_status_zakaz, id_spos_opl, id_valuta, id_manager) VALUES (@id_kli, @id_status_zakaz, @id_spos_opl, @id_valuta, @id_manager)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id_kli", DBNull.Value); // Значение id_kli должно быть добавлено из другого места
                command.Parameters.AddWithValue("@id_status_zakaz", statusId);
                command.Parameters.AddWithValue("@id_spos_opl", sposId);
                command.Parameters.AddWithValue("@id_valuta", DBNull.Value); // Аналогично id_kli
                command.Parameters.AddWithValue("@id_manager", managerIdText); // Аналогично id_kli
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private int GetIdFromTable(string tableName, string columnName, string value)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = $"SELECT id_{tableName} FROM {tableName} WHERE {columnName} = @value";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@value", value);
                connection.Open();
                object result = command.ExecuteScalar();
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
                return -1; // В случае, если значение не найдено
            }
        }
    }
}