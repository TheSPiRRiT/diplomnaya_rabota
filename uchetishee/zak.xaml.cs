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
    /// Логика взаимодействия для zak.xaml
    /// </summary>
    public partial class zak : Window
    {
        private const string connectionString = "Data Source=DESKTOP-HDQ72AK\\SQLEXPRESS;Initial Catalog=prodaji;Integrated Security=True";
        
        public zak()
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
                string query = "SELECT z.id_zakaz, k.imya_kli, k.familiya_kli, s.naim_status_zakaz, ss.spos_opl, v.valuta, m.imya_manager, m.familiya_manager, data_zakaza" +
                    "FROM zakaz z JOIN status_zakaz s ON z.id_status_zakaz = s.id_status_zakaz JOIN spos_opl ss ON z.id_spos_opl = ss.id_spos_opl JOIN valuta v ON z.id_valuta = v.id_valuta" +
                     "JOIN manager m ON z.id_manager = m.id_manager JOIN klient k ON z.id_kli = k.id_kli;";
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
                string query = @"SELECT z.id_zakaz, k.imya_kli, k.familiya_kli, s.naim_status_zakaz, ss.spos_opl, v.valuta, m.imya_manager, m.familiya_manager, data_zakaza
                         FROM zakaz z
                         JOIN status_zakaz s ON z.id_status_zakaz = s.id_status_zakaz
                         JOIN spos_opl ss ON z.id_spos_opl = ss.id_spos_opl
                         JOIN valuta v ON z.id_valuta = v.id_valuta
                         JOIN manager m ON z.id_manager = m.id_manager
                         JOIN klient k ON z.id_kli = k.id_kli";

                if (orderId != null)
                {
                    query += " WHERE z.id_zakaz = @OrderId";
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
            dobavlenie dob = new dobavlenie();
            dob.Show();
            this.Close();
        }

        private void search_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void tov_Click(object sender, RoutedEventArgs e)
        {
            tovari tovari = new tovari(); 
            tovari.Show();
            this.Close();
        }

        private void udal_Click(object sender, RoutedEventArgs e)
        {
            if (dtgridzak.SelectedItem != null)
            {
                DataRowView row = (DataRowView)dtgridzak.SelectedItem;
                int idToDelete = Convert.ToInt32(row["id_zakaz"]); // Предполагается, что у вас есть столбец "id_zakaz"

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string deleteDetailsQuery = "DELETE FROM detali_zakaza WHERE id_zakaz = @Id";
                    string deleteOrderQuery = "DELETE FROM zakaz WHERE id_zakaz = @Id";

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
                            if (transaction.Connection != null)
                            {
                                if (transaction.Connection.State == ConnectionState.Open)
                                {
                                    transaction.Connection.Close();
                                }
                                transaction.Dispose();
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите запись для удаления");
            }
        }

        private void dtgridzak_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            DataRowView rowView = (DataRowView)e.Row.Item;
            DataRow row = rowView.Row;

            // Получение значений столбцов для обновления
            int idToUpdate = Convert.ToInt32(row["id_zakaz"]);
            string imyaKli = row["imya_kli"].ToString();
            string familiyaKli = row["familiya_kli"].ToString();
            // Продолжите для других столбцов

            if (idToUpdate > 0)
            {
                // Формирование запроса UPDATE с параметрами для всех столбцов
                string query = @"UPDATE zakaz 
                        SET imya_kli = @ImyaKli, familiya_kli = @FamiliyaKli
                        -- Продолжите для других столбцов
                        WHERE id_zakaz = @Id";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Добавление параметров для каждого столбца
                        command.Parameters.AddWithValue("@ImyaKli", imyaKli);
                        command.Parameters.AddWithValue("@FamiliyaKli", familiyaKli);
                        // Продолжите для других параметров

                        command.Parameters.AddWithValue("@Id", idToUpdate);

                        try
                        {
                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Изменения сохранены");
                            }
                            else
                            {
                                MessageBox.Show("Не удалось сохранить изменения");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка при сохранении изменений: " + ex.Message);
                        }
                    }
                }
            }
        }


    }
    }



