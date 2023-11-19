using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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

namespace prog_p1.MVM.View
{
    /// <summary>
    /// Interaction logic for MatchTheColumnsView.xaml
    /// </summary>
    /// 
    public partial class MatchTheColumnsView : UserControl
    {
        private string connectionString = "Server=tcp:progpoe2023.database.windows.net,1433;Initial Catalog=progpoe;Persist Security Info=False;User ID=keagan;Password=CZe78vfn;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private List<DisplayItem> questionItems = new List<DisplayItem>();
        private List<DisplayItem> answerItems = new List<DisplayItem>();
        private DispatcherTimer timer;
        private int score;
        static Random random = new Random();
        private string userId;
        private System.Timers.Timer myTimer;
        private int elapsedTimeSeconds;
        public class DisplayItem
        {
            public string Question { get; set; } // This property should hold the question text
            public string CorrectAnswer { get; set; } // This property should hold the correct answer text
        }


        public MatchTheColumnsView()
        {
            InitializeComponent();
            userId = Application.Current.Properties["UserID"] as string;
            refreshButton.Click += RefreshButton_Click;
            checkAnswerButton.Click += CheckAnswerButton_Click;
            // Initialize the timer
            myTimer = new System.Timers.Timer();

            // Set the interval in milliseconds (e.g., 1000 milliseconds = 1 second)
            myTimer.Interval = 1000;

            // Hook up the Elapsed event for the timer
            myTimer.Elapsed += OnTimedEvent;

            // Set the timer to repeat (true) or one-time (false)
            myTimer.AutoReset = true;

        }


        




        private void CheckAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            // Get selected items from the ListViews
            var selectedQuestion = questionListView.SelectedItem as DisplayItem;
            var selectedAnswer = answerListView.SelectedItem as DisplayItem;



            if (selectedQuestion != null && selectedAnswer != null)
            {
                // Check if the selected answer matches the correct answer
                if (selectedQuestion.CorrectAnswer == selectedAnswer.CorrectAnswer)
                {
                    // Correct answer
                    MessageBox.Show("Correct Answer!");
                    score++; // Increase the score
                    scoreTextBlock.Text = $"Score: {score}"; // Update the score display

                    questionItems.Remove(selectedQuestion);
                    answerItems.Remove(selectedAnswer);

                    questionListView.Items.Refresh();
                    answerListView.Items.Refresh();
                }
                else
                {
                    // Incorrect answer
                    MessageBox.Show($"Incorrect Answer");
                    questionItems.Remove(selectedQuestion);
                    answerItems.Remove(selectedAnswer);

                    questionListView.Items.Refresh();
                    answerListView.Items.Refresh();
                }


            }
            else
            {
                // User hasn't selected both a question and an answer
                MessageBox.Show("Please select a question and an answer.");
            }

            if (questionItems.Count < 1)
            {
                MessageBox.Show("game over you have scored :" + score + "points well done ");
                questionItems.Clear();
                answerItems.Clear();

                score = 0; // Increase the score
                scoreTextBlock.Text = $"Score: {score}"; // Update the score display

                AddHighScoreMatch(userId);

                myTimer.Stop();
                // Reset the elapsed time
                elapsedTimeSeconds = 0;
                questionListView.Items.Refresh();
                answerListView.Items.Refresh();
            }

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // TODO: Implement logic to update the timer and handle end-game conditions
        }

       

        

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            questionItems.Clear();
            answerItems.Clear();

            LoadQuestionsAndAnswers();

            questionListView.Items.Refresh();
            answerListView.Items.Refresh();

        }

        private Dictionary<string, MatchTheColumnsQuestion> matchTheColumnsQuestions = new Dictionary<string, MatchTheColumnsQuestion>
        {
    // Add questions and answers with unique IDs
    { "000", new MatchTheColumnsQuestion { Question = "What is Generalities?", CorrectAnswer = "000" } },
    { "100", new MatchTheColumnsQuestion { Question = "What is Philosophy & Psychology?", CorrectAnswer = "100" } },
    { "200", new MatchTheColumnsQuestion { Question = "What is Religion?", CorrectAnswer = "200" } },
    { "300", new MatchTheColumnsQuestion { Question = "What is Social Sciences?", CorrectAnswer = "300" } },
    { "400", new MatchTheColumnsQuestion { Question = "What is Language?", CorrectAnswer = "400" } },
    { "500", new MatchTheColumnsQuestion { Question = "What is Natural Sciences & Mathematics?", CorrectAnswer = "500" } },
    { "600", new MatchTheColumnsQuestion { Question = "What is Technology (Applied Sciences)?", CorrectAnswer = "600" } },
    { "700", new MatchTheColumnsQuestion { Question = "What is The Arts?", CorrectAnswer = "700" } },
    { "800", new MatchTheColumnsQuestion { Question = "What is Literature & Rhetoric?", CorrectAnswer = "800" } },
    { "900", new MatchTheColumnsQuestion { Question = "What is Geography & History?", CorrectAnswer = "900" } }
};

        private class QuestionWithAnswers
        {

            public string Question { get; set; }
            public List<string> Answers { get; set; }
        }

        private void LoadQuestionsAndAnswers()
        {


            List<string> selectedQuestionKeys = matchTheColumnsQuestions.Keys.OrderBy(x => random.Next()).Take(4).ToList();

            foreach (string key in selectedQuestionKeys)
            {
                int temp = random.Next(); // Generates a random integer

                if (temp % 2 != 0)
                {
                    String AwnsTemp = matchTheColumnsQuestions[key].CorrectAnswer;
                    matchTheColumnsQuestions[key].CorrectAnswer = matchTheColumnsQuestions[key].Question;
                    matchTheColumnsQuestions[key].Question = AwnsTemp;
                }
            }

            foreach (string key in selectedQuestionKeys)
            {
                MatchTheColumnsQuestion question = matchTheColumnsQuestions[key];




                // Create separate instances for question and answer DisplayItems
                DisplayItem questionDisplayItem = new DisplayItem
                {
                    Question = question.Question,
                    CorrectAnswer = question.CorrectAnswer
                };

                DisplayItem answerDisplayItem = new DisplayItem
                {
                    Question = question.Question, // Set the question as the answer display item's question for illustration
                    CorrectAnswer = question.CorrectAnswer
                };

                // Add the DisplayItem instances to the respective lists
                questionItems.Add(questionDisplayItem);
                answerItems.Add(answerDisplayItem);
            }

            selectedQuestionKeys = matchTheColumnsQuestions.Keys.OrderBy(x => random.Next()).Take(3).ToList();

            foreach (string key in selectedQuestionKeys)
            {
                MatchTheColumnsQuestion question = matchTheColumnsQuestions[key];

                // Create separate instances for question and answer DisplayItems
                DisplayItem questionDisplayItem = new DisplayItem
                {
                    Question = question.Question,
                    CorrectAnswer = question.CorrectAnswer
                };

                DisplayItem answerDisplayItem = new DisplayItem
                {
                    Question = question.Question, // Set the question as the answer display item's question for illustration
                    CorrectAnswer = question.CorrectAnswer
                };

                // Add the DisplayItem instances to the respective lists
                answerItems.Add(answerDisplayItem);
            }

            ShuffleList(questionItems);
            ShuffleList(answerItems);

            // Set the item sources for questionListView and answerListView
            questionListView.ItemsSource = questionItems;
            answerListView.ItemsSource = answerItems;
        }

        private void ShuffleList<T>(List<T> list)
        {
            Random random = new Random();
            int n = list.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        public void AddHighScoreMatch(String userId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Insert the new high score into the HighScores table
                    string insertUserQuery = "INSERT INTO HighScores (UserID, matched_score) VALUES (@UserID, @matched_score);";
                    using (SqlCommand insertUserCommand = new SqlCommand(insertUserQuery, connection))
                    {
                        insertUserCommand.Parameters.AddWithValue("@UserID", userId);
                        insertUserCommand.Parameters.AddWithValue("@matched_score", score);
                        insertUserCommand.ExecuteNonQuery();
                    }

                    MessageBox.Show("Score has been added successfully!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Adding the score failed. Error: " + ex.Message);
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            // Stop the timer
            myTimer.Stop();

            // Clear the items in the observable collections bound to ListView
            questionItems.Clear();
            answerItems.Clear();
            score = 0;



            // Reset the elapsed time
            elapsedTimeSeconds = 0;
            LoadQuestionsAndAnswers();

            questionListView.Items.Refresh();
            answerListView.Items.Refresh();
            myTimer.Start();
        }


        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            // Increment the elapsed time
            elapsedTimeSeconds++;

            // Calculate minutes and remaining seconds
            int minutes = elapsedTimeSeconds / 60;
            int seconds = elapsedTimeSeconds % 60;

            // Ensure UI updates are done on the UI thread
            Dispatcher.Invoke(() =>
            {
                // Update the Text property of your TextBlock with the elapsed time in minutes and seconds
                timerTextBlock.Text = $"Elapsed Time: {minutes:D2}:{seconds:D2}";
            });
        }

    }
}

