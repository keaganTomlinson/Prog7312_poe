using prog_p1.Data;
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
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace prog_p1.MVM.View
{
    /// <summary>
    /// Interaction logic for LeaderBoard.xaml
    /// </summary>
    public partial class LeaderBoard : UserControl
    {
        private string connectionString = "Server=tcp:progpoe2023.database.windows.net,1433;Initial Catalog=progpoe;Persist Security Info=False;User ID=keagan;Password=CZe78vfn;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        public LeaderBoard()
        {
            InitializeComponent();
            List<HighScoreData> highScores = GetHighScoresFromDatabase();
            highScoresListView.ItemsSource = highScores;
            
        }


        private List<HighScoreData> GetHighScoresFromDatabase()
        {
            List<HighScoreData> highScores = new List<HighScoreData>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT U.Username, H.ScoreTime, H.matched_score ,H.quiz_score FROM Users U INNER JOIN HighScores H ON U.UserID = H.UserID ORDER BY ScoreTime DESC;";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string username = reader.IsDBNull(0) ? null : reader.GetString(0);
                                int? scoreTimeNullable = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1);
                                int? matchedScoreNullable = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2);

                                int scoreTime = scoreTimeNullable ?? 0; // Default value is 0 if scoreTimeNullable is null
                                int matchedScore = matchedScoreNullable ?? 0; // Default value is 0 if matchedScoreNullable is null

                                int? quizScoreNullable = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3);
                                int quizScore = quizScoreNullable ?? 0; // Default value is 0 if quizScoreNullable is null

                                HighScoreData highScoreData = new HighScoreData
                                {
                                    Username = username,
                                    ScoreTime = scoreTime,
                                    MatchedScore = matchedScore,
                                    quizscore = quizScore // Set the value of quiz_score
                                };

                                highScores.Add(highScoreData);  // Corrected variable name

                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any database connection or query errors here
                MessageBox.Show("Error retrieving high scores: " + ex.Message);
            }

            return highScores;
        }

    }
}
