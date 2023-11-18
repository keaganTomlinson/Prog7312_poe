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
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Windows;
using System.Security.Cryptography;
using System.Text;

namespace prog_p1
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        private string connectionString = "Server=tcp:progpoe2023.database.windows.net,1433;Initial Catalog=progpoe;Persist Security Info=False;User ID=keagan;Password=CZe78vfn;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        public Register()
        {
            InitializeComponent();
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string username = usernameTextBox.Text;
                    string password = passwordBox.Password;

                    // Check if the username already exists
                    string checkUsernameQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                    using (SqlCommand checkUsernameCommand = new SqlCommand(checkUsernameQuery, connection))
                    {
                        checkUsernameCommand.Parameters.AddWithValue("@Username", username);
                        int existingUserCount = (int)checkUsernameCommand.ExecuteScalar();
                        if (existingUserCount > 0)
                        {
                            MessageBox.Show("Username already exists. Please choose a different one.");
                            return;
                        }
                    }

                    // Hash the password before storing it
                    string hashedPassword = HashPassword(password);

                    // Insert the new user into the Users table
                    string insertUserQuery = "INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)";
                    using (SqlCommand insertUserCommand = new SqlCommand(insertUserQuery, connection))
                    {
                        insertUserCommand.Parameters.AddWithValue("@Username", username);
                        insertUserCommand.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                        insertUserCommand.ExecuteNonQuery();
                    }

                    MessageBox.Show("Registration Successful!");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Registration failed. Error: " + ex.Message);
            }
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
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the registration window when the "Exit" button is clicked
            this.Close();
        }

    }
}
