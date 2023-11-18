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
using System.Windows.Threading;

namespace prog_p1.MVM.View
{
    /// <summary>
    /// Interaction logic for MatchTheColumnsView.xaml
    /// </summary>
    public partial class MatchTheColumnsView : UserControl
    {
        private DispatcherTimer timer;
        private int score;

        public MatchTheColumnsView()
        {
            InitializeComponent();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement logic to refresh the game (e.g., shuffle questions and answers)
        }

        private void InitializeGame()
        {
            // TODO: Implement logic to populate questionListView and answerListView with data
            // For example, you might want to bind them to ObservableCollection objects.

            // Initialize timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            // TODO: Initialize other game-related variables

            // Update UI
            UpdateScore();
            UpdateTimer();
        }



        private void CheckAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement logic to check the selected answers and update the score
            // You may need to access the selected items from questionListView and answerListView.
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // TODO: Implement logic to update the timer and handle end-game conditions
        }

        private void UpdateScore()
        {
            scoreTextBlock.Text = $"Score: {score}";
        }

        private void UpdateTimer()
        {
            // TODO: Implement logic to update the timerTextBlock based on the timer value
        }

        // You may need other helper methods depending on your game logic

        // Make sure to handle the appropriate events for buttons, list views, etc.

    }
}

