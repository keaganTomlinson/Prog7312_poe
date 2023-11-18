using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

namespace prog_p1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string connectionString = "Server=tcp:progpoe2023.database.windows.net,1433;Initial Catalog=progpoe;Persist Security Info=False;User ID=keagan;Password=CZe78vfn;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        public MainWindow()
        {
            InitializeComponent();

        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string username = usernameTextBox.Text;
                    string password = passwordBox.Password;

                    // Query the database to retrieve the hashed password and user ID for the provided username
                    string getUserInfoQuery = "SELECT PasswordHash, UserId FROM Users WHERE Username = @Username";
                    using (SqlCommand getUserInfoCommand = new SqlCommand(getUserInfoQuery, connection))
                    {
                        getUserInfoCommand.Parameters.AddWithValue("@Username", username);
                        SqlDataReader reader = getUserInfoCommand.ExecuteReader();

                        if (reader.Read())
                        {
                            string storedPasswordHash = reader["PasswordHash"] as string;
                            int userId = Convert.ToInt32(reader["UserId"]);

                            // Hash the provided password for comparison
                            string hashedPassword = HashPassword(password);

                            // Compare the hashed password with the stored password hash
                            if (hashedPassword == storedPasswordHash)
                            {
                                MessageBox.Show("Login Successful!");

                                // Open the dashboard and pass the user ID
                               OpenDashBoard(userId.ToString());

                            }
                            else
                            {
                                MessageBox.Show("Login failed. Invalid username or password.");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Login failed. Invalid username or password.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login failed. Error: " + ex.Message);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the application
            Application.Current.Shutdown();

        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of the Register window
            Register registerWindow = new Register();

            // Show the Register window
            registerWindow.Show();
            this.Close();
        }
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        private void OpenDashBoard(string userId)
        {
            Application.Current.Properties["UserID"] = userId;

            New_DashBoard dashBoardWindow = new New_DashBoard();
            dashBoardWindow.Show();
            this.Close(); // Close the current window if needed
        }

    }
}
