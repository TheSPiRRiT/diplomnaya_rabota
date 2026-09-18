using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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
    /// Логика взаимодействия для tovari.xaml
    /// </summary>
    public partial class tovari : Window
    {
        private const string connectionString = "Data Source=DESKTOP-HDQ72AK\\SQLEXPRESS;Initial Catalog=prodaji;Integrated Security=True";

        public tovari()
        {
            InitializeComponent();
        }

        private void vihod_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT t.id_tovar, t.artikul, k.kategoria,v.naim_vid,s.seriya, t.nomenklatur, t.stoimost, va.valuta, sk.naim_sklad, t.kolichestvo, e.ed_izm\r\nFROM tovar t\r\nJOIN kategor k ON k.id_kategor = t.id_kategor\r\nJOIN seriya s on s.id_serii= t.id_serii\r\njoin ed_izm e on e.id_ed_izm=t.id_ed_izm\r\njoin vid_tov v on v.id_vid_tov= t.id_vid_tov\r\njoin valuta va on va.id_valuta= t.id_valuta\r\njoin sklad sk on sk.id_sklad= t.id_sklad;";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                dtgridzak.ItemsSource = dataTable.DefaultView;
            }
        }

        private void ser_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(search.Text.Trim(), out int orderId))
            {
                SearchData(orderId);
            }
            else if (string.IsNullOrWhiteSpace(search.Text.Trim()))
            {
                SearchData();
            }
            else
            {
                MessageBox.Show("Введите корректный ID заказа");
            }
        }

        private void SearchData(int? orderId = null)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"SELECT t.id_tovar, t.artikul, k.kategoria,v.naim_vid,s.seriya, t.nomenklatur, t.stoimost, va.valuta, sk.naim_sklad, t.kolichestvo, e.ed_izm
                        FROM tovar t
                        JOIN kategor k ON k.id_kategor = t.id_kategor
                        JOIN seriya s ON s.id_serii = t.id_serii
                        JOIN ed_izm e ON e.id_ed_izm = t.id_ed_izm
                        JOIN vid_tov v ON v.id_vid_tov = t.id_vid_tov
                        JOIN valuta va ON va.id_valuta = t.id_valuta
                        JOIN sklad sk ON sk.id_sklad = t.id_sklad";

                if (orderId != null)
                {
                    query += " WHERE t.id_tovar = @OrderId";
                }

                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);

                if (orderId != null)
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@OrderId", orderId);
                }

                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                dtgridzak.ItemsSource = dataTable.DefaultView;
            }
        }

        private void dob_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void search_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void zaka_Click(object sender, RoutedEventArgs e)
        {
            zak zak = new zak();
            zak.Show();
            this.Close();
        }

        private void udal_Click(object sender, RoutedEventArgs e)
        {
            if (dtgridzak.SelectedItem != null)
            {
                DataRowView row = (DataRowView)dtgridzak.SelectedItem;
                int idToDelete = Convert.ToInt32(row["id_tovar"]); // Предполагается, что у вас есть столбец "id_tovar"

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string deleteDetailsQuery = "DELETE FROM detali_zakaza WHERE id_tovara = @Id";
                    string deleteOrderQuery = "DELETE FROM tovar WHERE id_tovar = @Id";

                    using (SqlCommand deleteDetailsCommand = new SqlCommand(deleteDetailsQuery, connection))
                    using (SqlCommand deleteOrderCommand = new SqlCommand(deleteOrderQuery, connection))
                    {
                        deleteDetailsCommand.Parameters.AddWithValue("@Id", idToDelete);
                        deleteOrderCommand.Parameters.AddWithValue("@Id", idToDelete);

                        // Начать транзакцию
                        SqlTransaction transaction = connection.BeginTransaction();
                        deleteDetailsCommand.Transaction = transaction;
                        deleteOrderCommand.Transaction = transaction;

                        try
                        {
                            // Удалить детали заказа
                            int rowsAffectedDetails = deleteDetailsCommand.ExecuteNonQuery();

                            // Удалить заказ
                            int rowsAffectedOrder = deleteOrderCommand.ExecuteNonQuery();

                            if (rowsAffectedDetails > 0 || rowsAffectedOrder > 0)
                            {
                                transaction.Commit();
                                LoadData();
                                MessageBox.Show("Запись успешно удалена");
                            }
                            else
                            {
                                transaction.Rollback();
                                MessageBox.Show("Не удалось удалить запись");
                            }
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Ошибка при удалении записи: " + ex.Message);
                        }
                        finally
                        {
                            transaction.Dispose(); // Закрываем транзакцию после ее использования
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите запись для удаления");
            }
        }

       
    }
}
