using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;       // ObservableCollection class
using System.Text;
using System.Windows.Threading;
using System.ComponentModel;
using System.Data.SqlClient;

namespace prog_p1
{
    public partial class DashBoard : Window
    {
        private string userId;
        private List<string> callNumbers; // List to store call numbers
        private List<string> shuffledCallNumbers; // List to store shuffled call numbers
        private bool isOrderCorrect = false;
        private string draggedItem;
        private Point startPoint = new Point();
        private ObservableCollection<WorkItem> Items = new ObservableCollection<WorkItem>();
        private int startIndex = -1;
        static Random random = new Random();
        private DispatcherTimer timer;
        private int comparisonIndex = 0;
        private ObservableCollection<WorkItem> ItemsSorted = new ObservableCollection<WorkItem>();
        private TimeSpan elapsedTime = TimeSpan.Zero; // Initialize the elapsed time to zero
        private string connectionString = "Server=tcp:progpoe2023.database.windows.net,1433;Initial Catalog=progpoe;Persist Security Info=False;User ID=keagan;Password=CZe78vfn;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private DateTime startTime;
        private List<KeyValuePair<string, string>> shuffledCategories;
        private KeyValuePair<string, string> currentQuestion;
        private MatchTheColumnsQuestion currentMatchQuestion;
        private int score = 0;
        private List<MatchTheColumnsQuestion> remainingQuestions;
        private List<DisplayItem> questionItems = new List<DisplayItem>();
        private List<DisplayItem> answerItems = new List<DisplayItem>();

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



        public DashBoard(string userId)
        {
            InitializeComponent();
            InitializeListView();
            InitializeTimer();
            List<HighScore> highScores = GetHighScoresFromDatabase();
            highScoresListView.ItemsSource = highScores;
            this.userId = userId;


            Instructions.Text = "Welcome to the Dewey Decimal Sorting Game!\r\n\r\nObjective: Your goal is to sort a collection of records according to the Dewey Decimal System as quickly and accurately as possible.\r\n\r\nInstructions:" +
                "\r\n\r\nSorting Interface: You'll see a list of records on the screen. Each record represents a book or item in the library. Your task is to arrange them in the correct order based on their Dewey Decimal numbers.\r\n\r\n" +
                "Navigation Buttons: Use the \"Up\" and \"Down\" buttons to move records up or down in the list. You can also drag and drop records if your device supports it for a more interactive experience.\r\n\r\nSorting Rules: Remember that" +
                " in the Dewey Decimal System, books are sorted numerically from left to right. The whole numbers represent categories, while decimals represent subcategories. So, 100 comes before 200, and 123.45 comes before 123.5.\r\n\r\nLoading " +
                "Bar: The loading bar at the top of the screen indicates your progress. It will fill up as you correctly sort records. Your goal is to complete the sorting as quickly as possible.\r\n\r\nTimer: A timer is running to keep track of how " +
                "long it takes you to complete the sorting task. Try to finish as fast as you can for a high score.\r\n\r\nScoring: Your score is based on both the time it takes you to sort the records and the accuracy of your sorting. Try to achieve " +
                "the highest score possible.";


            // Set up timer



            refreshButton.Click += RefreshButton_Click;
            checkAnswerButton.Click += CheckAnswerButton_Click;

            LoadQuestionsAndAnswers();


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

                LoadQuestionsAndAnswers();
                questionListView.Items.Refresh();
                answerListView.Items.Refresh();
            }

        }





        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            questionItems.Clear();
            answerItems.Clear();

            LoadQuestionsAndAnswers();

            questionListView.Items.Refresh();
            answerListView.Items.Refresh();

        }

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









        private List<HighScore> GetHighScoresFromDatabase()
        {
            List<HighScore> highScores = new List<HighScore>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT U.Username, H.ScoreTime, H.matched_score FROM Users U INNER JOIN HighScores H ON U.UserID = H.UserID ORDER BY ScoreTime DESC;";

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

                                HighScore highScore = new HighScore
                                {
                                    username = username,
                                    ScoreTime = scoreTime,
                                    matched_score = matchedScore
                                };

                                highScores.Add(highScore);
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





        public class HighScore
        {
            public string username { get; set; }
            public int ScoreTime { get; set; }

            public int matched_score { get; set; }


        }
        static int CalculateScore(int totalTimeInSeconds)
        {
            // You can define your scoring logic here
            // For example, let's say you want to assign a score inversely proportional to time
            // The longer the time, the lower the score
            // You can adjust this formula as needed for your scoring criteria
            int maxScore = 5000; // Maximum score you want to assign
            int minTimeInSeconds = 0; // Minimum time for maximum score (2 minutes)
            int score = maxScore - (totalTimeInSeconds - minTimeInSeconds);

            // Make sure the score is within a reasonable range
            score = Math.Max(0, score);

            return score;
        }

        public void AddHighScore(String userId, TimeSpan Int)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Calculate the score time by subtracting the elapsed time from the current time
                    int score = CalculateScore((int)elapsedTime.TotalSeconds);

                    // Insert the new high score into the HighScores table
                    string insertUserQuery = "INSERT INTO HighScores (UserID, ScoreTime) VALUES (@UserID, @ScoreTime);";
                    using (SqlCommand insertUserCommand = new SqlCommand(insertUserQuery, connection))
                    {
                        insertUserCommand.Parameters.AddWithValue("@UserID", userId);
                        insertUserCommand.Parameters.AddWithValue("@ScoreTime", score);
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

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // Set the timer interval to 1 second
            timer.Tick += Timer_Tick;

            // Store the start time when the timer begins
            startTime = DateTime.Now;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            // Update the progress bar based on the comparison index and total items
            progressBar.Value = (double)comparisonIndex / Items.Count * 100;

            elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1));

            // Display the elapsed time in the TextBlock
            timerText.Text = elapsedTime.ToString(@"mm\:ss");
        }

        private void Timer_Tick2(object sender, EventArgs e)
        {
            // Update the progress bar based on the comparison index and total items
            progressBar.Value = (double)comparisonIndex / Items.Count * 100;

            elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1));

            // Display the elapsed time in the TextBlock
            timerTextBlock.Text = elapsedTime.ToString(@"mm\:ss");
        }

        private void SortAndCompare()
        {
            // Sort the ObservableCollection by the Note property
            List<WorkItem> sortedItems = Items.OrderBy(item => item.Note).ToList();
            comparisonIndex = 0;

            // Initialize a variable to track correctness
            bool allCorrectlyOrdered = true;

            // Compare each item in the sorted list against the original user-sorted list
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Note != sortedItems[i].Note)
                {
                    // Items are not in the same order at index i
                    Items[i].IsCorrectlyOrdered = false;
                    allCorrectlyOrdered = false; // At least one item is not correctly ordered
                }
                else
                {
                    Items[i].IsCorrectlyOrdered = true; // Set the IsCorrectlyOrdered property to true
                    comparisonIndex++;
                }
            }

            // Start the timer after the comparison
            timer.Start();

            // Optionally, display a message if not all items are correctly ordered
            if (!allCorrectlyOrdered)
            {
                // MessageBox.Show("Not all items are correctly ordered.");
            }
            else
            {
                timer.Stop();


                TimeSpan elapsedTime = DateTime.Now - startTime;
                int score = CalculateScore((int)elapsedTime.TotalSeconds);
                MessageBox.Show("well done you have completed the sorting challenge in , " + timerText.Text + "total score is " + score);

                AddHighScore(userId, elapsedTime);
            }



        }

        private void AdminSort_Click(object sender, RoutedEventArgs e)
        {
            // Implement your quick sort logic here
            List<WorkItem> sortedItems = Items.OrderBy(item => item.Note).ToList();

            // Update the ObservableCollection with the sorted items
            Items.Clear();
            foreach (var item in sortedItems)
            {
                Items.Add(item);
            }
            SortAndCompare();


        }


        private void InitializeListView()
        {
            // Clear data
            lstView.Items.Clear();
            Items.Clear();
            // Add rows
            for (int i = 0; i < 10; i++)
            {
                Items.Add(new WorkItem(GenerateRandomDeweyCallNumber(), false));

            }

            lstView.ItemsSource = Items;
        }

        static string GenerateRandomDeweyCallNumber()
        {
            // Generate a random number with a decimal point
            double randomNumber = random.NextDouble() * 1000.0; // Adjust the range as needed

            // Generate three random letters
            string randomLetters = GenerateRandomLetters(3);

            // Format the Dewey call number
            string deweyCallNumber = $"{randomNumber:F3} {randomLetters}";

            return deweyCallNumber;
        }

        static string GenerateRandomLetters(int length)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                int randomIndex = random.Next(alphabet.Length);
                sb.Append(alphabet[randomIndex]);
            }

            return sb.ToString();
        }

        private void lstView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Get current mouse position
            startPoint = e.GetPosition(null);
        }

        // Helper to search up the VisualTree
        private static T FindAnchestor<T>(DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        private void lstView_MouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                       Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                // Get the dragged ListViewItem
                ListView listView = sender as ListView;
                ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
                if (listViewItem == null) return;           // Abort
                                                            // Find the data behind the ListViewItem
                WorkItem item = (WorkItem)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
                if (item == null) return;                   // Abort
                                                            // Initialize the drag & drop operation
                startIndex = lstView.SelectedIndex;
                DataObject dragData = new DataObject("WorkItem", item);
                DragDrop.DoDragDrop(listViewItem, dragData, DragDropEffects.Copy | DragDropEffects.Move);
            }
        }

        private void lstView_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("WorkItem") || sender != e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void lstView_Drop(object sender, DragEventArgs e)
        {
            int index = -1;

            if (e.Data.GetDataPresent("WorkItem") && sender == e.Source)
            {
                // Get the drop ListViewItem destination
                ListView listView = sender as ListView;
                ListViewItem listViewItem = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
                if (listViewItem == null)
                {
                    // Abort
                    e.Effects = DragDropEffects.None;
                    return;
                }
                // Find the data behind the ListViewItem
                WorkItem item = (WorkItem)listView.ItemContainerGenerator.ItemFromContainer(listViewItem);
                // Move item into observable collection 
                // (this will be automatically reflected to lstView.ItemsSource)
                e.Effects = DragDropEffects.Move;
                index = Items.IndexOf(item);
                if (startIndex >= 0 && index >= 0)
                {
                    Items.Move(startIndex, index);
                }
                startIndex = -1;        // Done!
            }
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            WorkItem item = null;
            int index = -1;

            if (lstView.SelectedItems.Count != 1) return;
            item = (WorkItem)lstView.SelectedItems[0];
            index = Items.IndexOf(item);
            if (index > 0)
            {
                Items.Move(index, index - 1);
            }
            SortAndCompare();
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            WorkItem item = null;
            int index = -1;

            if (lstView.SelectedItems.Count != 1) return;
            item = (WorkItem)lstView.SelectedItems[0];
            index = Items.IndexOf(item);
            if (index < Items.Count - 1)
            {
                Items.Move(index, index + 1);
            }
            SortAndCompare();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }


    public class DisplayItem
    {
        public string Question { get; set; } // This property should hold the question text
        public string CorrectAnswer { get; set; } // This property should hold the correct answer text
    }


    public class WorkItem
    {
        public string Note { get; set; }
        public bool IsCorrectlyOrdered { get; set; }

        public WorkItem(string note, bool isCorrectlyOrdered)
        {
            Note = note;
            IsCorrectlyOrdered = isCorrectlyOrdered;
        }
    }

    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> list, Random random)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }


}