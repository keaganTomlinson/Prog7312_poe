using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Threading;

namespace prog_p1.MVM.View
{
    /// <summary>
    /// Interaction logic for ReplacingbookView.xaml
    /// </summary>
    public partial class ReplacingbookView : UserControl
    {
        private string userId;
        private string connectionString = "Server=tcp:progpoe2023.database.windows.net,1433;Initial Catalog=progpoe;Persist Security Info=False;User ID=keagan;Password=CZe78vfn;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private Point startPoint = new Point();
        private int startIndex = -1;
        private ObservableCollection<WorkItem> Items = new ObservableCollection<WorkItem>();
        private DispatcherTimer timer;
        private int comparisonIndex = 0;
        private DateTime startTime;
        static Random random = new Random();
        private TimeSpan elapsedTime = TimeSpan.Zero; // Initialize the elapsed time to zero






        public ReplacingbookView()
        {
            InitializeComponent();
            InitializeListView();
            InitializeTimer();
            userId = Application.Current.Properties["UserID"] as string;


            Instructions.Text = "Welcome to the Dewey Decimal Sorting Game!\r\n\r\nObjective: Your goal is to sort a collection of records according to the Dewey Decimal System as quickly and accurately as possible.\r\n\r\nInstructions:" +
             "\r\n\r\nSorting Interface: You'll see a list of records on the screen. Each record represents a book or item in the library. Your task is to arrange them in the correct order based on their Dewey Decimal numbers.\r\n\r\n" +
             "Navigation Buttons: Use the \"Up\" and \"Down\" buttons to move records up or down in the list. You can also drag and drop records if your device supports it for a more interactive experience.\r\n\r\nSorting Rules: Remember that" +
             " in the Dewey Decimal System, books are sorted numerically from left to right. The whole numbers represent categories, while decimals represent subcategories. So, 100 comes before 200, and 123.45 comes before 123.5.\r\n\r\nLoading " +
             "Bar: The loading bar at the top of the screen indicates your progress. It will fill up as you correctly sort records. Your goal is to complete the sorting as quickly as possible.\r\n\r\nTimer: A timer is running to keep track of how " +
             "long it takes you to complete the sorting task. Try to finish as fast as you can for a high score.\r\n\r\nScoring: Your score is based on both the time it takes you to sort the records and the accuracy of your sorting. Try to achieve " +
             "the highest score possible.";
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
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

            // Store the start time when the timer begins
            startTime = DateTime.Now;
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }




        private void lstView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Get current mouse position
            startPoint = e.GetPosition(null);
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

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




        public class WorkItem : INotifyPropertyChanged
        {
            private string _note;
            public string Note
            {
                get { return _note; }
                set
                {
                    if (_note != value)
                    {
                        _note = value;
                        OnPropertyChanged(nameof(Note));
                    }
                }
            }

            private bool _isCorrectlyOrdered;
            public bool IsCorrectlyOrdered
            {
                get { return _isCorrectlyOrdered; }
                set
                {
                    if (_isCorrectlyOrdered != value)
                    {
                        _isCorrectlyOrdered = value;
                        OnPropertyChanged(nameof(IsCorrectlyOrdered));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public WorkItem(string note, bool isCorrectlyOrdered)
            {
                Note = note;
                IsCorrectlyOrdered = isCorrectlyOrdered;
            }
        }



    }

}
