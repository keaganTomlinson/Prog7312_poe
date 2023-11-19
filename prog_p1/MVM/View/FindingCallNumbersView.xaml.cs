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
using System.Windows.Controls;
using System.IO;
using prog_p1.Data;
using System.Reflection;
using System.Windows.Threading;
using System.Data.SqlClient;

namespace prog_p1.MVM.View
{
    /// <summary>
    /// Interaction logic for FindingCallNumbersView.xaml
    /// </summary>
    public partial class FindingCallNumbersView : UserControl
    {
        private string userId;

        private TreeNode currentQuestionNode;
        private DeweyTreeBuilder deweyTreeBuilder;
        private TreeNode deweyTreeRoot;
        private string currentQ;
        int score=0;
        int incorect = 0;
        bool ActiveQuiz= false;
        private string connectionString = "Server=tcp:progpoe2023.database.windows.net,1433;Initial Catalog=progpoe;Persist Security Info=False;User ID=keagan;Password=CZe78vfn;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private DispatcherTimer timer;
        private int totalTime = 60; // Total time in seconds

        public FindingCallNumbersView()
        {
            InitializeComponent();
            userId = Application.Current.Properties["UserID"] as string;
            // Get the current directory where the executable is located
            string currentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            currentDirectory = currentDirectory.Replace("\\bin\\Debug\\", "\\Data\\");



            Console.WriteLine("Current Directory: " + Directory.GetCurrentDirectory());

            // Combine the current directory with the "Data" folder and the file name
            string filePath = System.IO.Path.Combine(currentDirectory, "DeweyData.txt");

            deweyTreeBuilder = new DeweyTreeBuilder();
            deweyTreeRoot = deweyTreeBuilder.BuildTreeFromFile(filePath);
            InitializeTimer();





        }
        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // Update every second
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Update the timer display
            totalTime--;
            timerDisplay.Text = "Time: " + totalTime.ToString("D2");

            // Update the progress bar
            double progressValue = (double)totalTime / 60 * 100;
            progressBar.Value = progressValue;

            scoreLabel.Content= "Score : "+ score;
            incorectLabel.Content = "incorect : " + incorect;
            if (totalTime <= 0)
            {
                timer.Stop();
                MessageBox.Show("Time's up! Quiz is over.", "Quiz Over", MessageBoxButton.OK, MessageBoxImage.Information);

                // Reset quiz state

                AddScoreToDatabase(score - incorect);
                score = 0;
                incorect = 0;
                scoreLabel.Content = "Score: " + score;
                incorectLabel.Content = "incorect : " + incorect;
                totalTime = 60; // Reset the timer to 60 seconds
                timerDisplay.Text = "Time: " + totalTime.ToString("D2");
                questionLabel.Content = "Error: Random call number not found.";
                optionButton1.Content = "";
                optionButton2.Content = "";
                optionButton3.Content = "";
                optionButton4.Content = "";
                ActiveQuiz = false;
                startButton.IsEnabled = true;
            }
        }

        private void AddScoreToDatabase(int points)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Insert the new high score into the HighScores table
                    string insertUserQuery = "INSERT INTO HighScores (UserID, quiz_score) VALUES (@UserID, @quiz_score\r\n);";
                    using (SqlCommand insertUserCommand = new SqlCommand(insertUserQuery, connection))
                    {
                        insertUserCommand.Parameters.AddWithValue("@UserID", userId);
                        insertUserCommand.Parameters.AddWithValue("@quiz_score", points);
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

        


        private void OptionButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;

            // Get the selected answer from the button's content
            string selectedAnswer = clickedButton.Content.ToString();

            // Find the corresponding child node based on the selected answer
            TreeNode selectedNode = SearchTree(deweyTreeRoot, currentQ);

            // Check if selectedNode is null
            if (selectedNode == null)
            {
                // Handle the case where the selected node is not found
                MessageBox.Show("Error: Selected node not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // You might want to handle this situation appropriately, such as resetting the quiz or taking other actions.
                return;
            }

            if (ActiveQuiz == false)
            {
                
                return;
            }
            

            // Use a different approach to find the node with the selected answer
            if (selectedNode.Description == selectedAnswer)
            {
                MessageBox.Show("Correct Answer!", "Congratulations", MessageBoxButton.OK, MessageBoxImage.Information);
                score++;
                DisplayQuestion();
            }
            else
            {
                MessageBox.Show("Incorrect Answer. Try Again.", "Sorry", MessageBoxButton.OK, MessageBoxImage.Error);
                incorect++;
                DisplayQuestion();
            }



        }

        private void StartQuiz()
        {
            ActiveQuiz = true;
            currentQuestionNode = deweyTreeRoot;
            DisplayQuestion();
        }

        private void DisplayQuestion()
        {
            var random = new Random();

            // Generate a random three-digit number
            int randomNumber = random.Next(100, 1000);
            string randomCallNumber = randomNumber.ToString();

            // Search for the node with the random call number
            TreeNode randomNode = SearchTree(deweyTreeRoot, randomCallNumber);
            currentQ = randomCallNumber.ToString();

            if (randomNode != null)
            {
                // Set the call number in the questionLabel
                questionLabel.Content = $"What is the category for {randomNode.CallNumber}?";

                // Get the list of child nodes (answer options) for the random question
                List<string> answerOption = new List<string>();
                answerOption.Add(randomNode.Description);

                // Assuming your tree nodes contain integer values
                var answerOptions = currentQuestionNode.Children.Values.ToList();
                answerOptions = answerOptions.OrderBy(x => random.Next()).ToList();

                // Add all answer options, then shuffle
                answerOption.Add(answerOptions[0].Description);
                answerOption.Add(answerOptions[1].Description);
                answerOption.Add(answerOptions[2].Description);

                // shuffle for a more random order
                answerOption = answerOption.OrderBy(x => random.Next()).ToList();
                answerOption = answerOption.OrderBy(x => random.Next()).ToList();
                answerOption = answerOption.OrderBy(x => random.Next()).ToList();
                answerOption = answerOption.OrderBy(x => random.Next()).ToList();


                // Display the answer options on the buttons
                optionButton1.Content = answerOption[0];
                optionButton2.Content = answerOption[1];
                optionButton3.Content = answerOption[2];
                optionButton4.Content = answerOption[3];
            }
            else
            {
                // Handle the case where the random call number was not found
                questionLabel.Content = "Error: Random call number not found.";
                optionButton1.Content = "";
                optionButton2.Content = "";
                optionButton3.Content = "";
                optionButton4.Content = "";
                DisplayQuestion();
            }

            if (ActiveQuiz == false)
            {
                questionLabel.Content = "please click the start button";
                optionButton1.Content = "";
                optionButton2.Content = "";
                optionButton3.Content = "";
                optionButton4.Content = "";
            }
        }







       


        public TreeNode SearchTree(TreeNode root, string targetCallNumber)
        {
            // Start the search from the root of the tree
            return SearchNode(root, targetCallNumber);
        }

        private TreeNode SearchNode(TreeNode current, string targetCallNumber)
        {
            if (current == null)
            {
                // Node not found
                return null;
            }

            if (current.CallNumber == targetCallNumber)
            {
                // Node with the target call number found
                return current;
            }

            // Recursively search in the children
            foreach (var child in current.Children.Values)
            {
                var result = SearchNode(child, targetCallNumber);
                if (result != null)
                {
                    // Node found in one of the children
                    return result;
                }
            }

            // Node not found in the current node or its children
            return null;
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            //currentQuestionNode = deweyTreeRoot;
            StartQuiz();
            timer.Start(); 
            startButton.IsEnabled = false;
        }
    }
}