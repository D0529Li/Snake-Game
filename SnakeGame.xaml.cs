﻿using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Snake_Game
{
    /// <summary>
    /// Interaction logic for StartGame.xaml
    /// </summary>
    public partial class SnakeGame : Window
    {
        private readonly Snake snake = new Snake();
        private readonly SnakeGameViewModel snakeGameVM;
        private SpeedOptions speed = SpeedOptions.Not_Selected;

        public SnakeGame()
        {
            InitializeComponent();
            DataContext = snakeGameVM = new SnakeGameViewModel();
            BuildGameGrid();
            snake.SnakeMoved += SnakeMovedHandler;
            snake.SnakeDies += SnakeDiesHandler;
            snake.SnakeEatsCandy += SnakeEatsCandyHandler;
            snakeGameVM.ArrowKeyPressed += snake.BufferDirection;
        }

        private void RecordsButton_Click(object sender, RoutedEventArgs e)
        {
            var result = "No records found.";

            if (File.Exists("Records.xml"))
            {
                Records? records;
                using (var stream = File.OpenRead("Records.xml"))
                {
                    var serializer = new XmlSerializer(typeof(Records));
                    records = serializer.Deserialize(stream) as Records;
                }
                if (records == null)
                    return;

                if (records.RecordPlayer_Slow.Score != 0)
                    result = $"Slow: {records.RecordPlayer_Slow.Name} - {records.RecordPlayer_Slow.Score}";
                if (records.RecordPlayer_Medium.Score != 0)
                    result += $"\nMedium: {records.RecordPlayer_Medium.Name} - {records.RecordPlayer_Medium.Score}";
                if (records.RecordPlayer_Fast.Score != 0)
                    result += $"\nFast: {records.RecordPlayer_Fast.Name} - {records.RecordPlayer_Fast.Score}";
            }
            MessageBox.Show(result);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SpeedSlowButton_Click(object sender, RoutedEventArgs e)
        {
            SpeedSlowButton.Background = new SolidColorBrush(Colors.YellowGreen);
            SpeedMediumButton.Background = new SolidColorBrush(Colors.Gray);
            SpeedFastButton.Background = new SolidColorBrush(Colors.Gray);
            speed = SpeedOptions.Slow;
        }
        private void SpeedMediumButton_Click(object sender, RoutedEventArgs e)
        {
            SpeedSlowButton.Background = new SolidColorBrush(Colors.Gray);
            SpeedMediumButton.Background = new SolidColorBrush(Colors.YellowGreen);
            SpeedFastButton.Background = new SolidColorBrush(Colors.Gray);
            speed = SpeedOptions.Medium;
        }
        private void SpeedFastButton_Click(object sender, RoutedEventArgs e)
        {
            SpeedSlowButton.Background = new SolidColorBrush(Colors.Gray);
            SpeedMediumButton.Background = new SolidColorBrush(Colors.Gray);
            SpeedFastButton.Background = new SolidColorBrush(Colors.YellowGreen);
            speed = SpeedOptions.Fast;
        }

        private void SnakeMovedHandler(Coordinate oldPos, Coordinate newPos)
        {
            RenderSnake(oldPos, newPos);
        }

        private void BuildGameGrid(int rows = SnakeConstants.DEFAULT_GRID_SIZE, int cols = SnakeConstants.DEFAULT_GRID_SIZE)
        {
            GameGrid.RowDefinitions.Clear();
            GameGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < rows; i++)
            {
                var newRow = new RowDefinition
                {
                    Height = new GridLength(45),
                    Name = $"GameGridRow{i}"
                };
                GameGrid.RowDefinitions.Add(newRow);
                GameGrid.RegisterName(newRow.Name, newRow);
            }
            for (int i = 0; i < cols; i++)
            {
                var newColumn = new ColumnDefinition
                {
                    Width = new GridLength(45),
                    Name = $"GameGridColumn{i}"
                };
                GameGrid.ColumnDefinitions.Add(newColumn);
                GameGrid.RegisterName(newColumn.Name, newColumn);
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var newBorder = new Border
                    {
                        BorderBrush = new SolidColorBrush(Colors.Black),
                        BorderThickness = new Thickness(1),
                        Name = $"GameGridBorderR{i}C{j}"
                    };
                    Grid.SetRow(newBorder, i);
                    Grid.SetColumn(newBorder, j);
                    GameGrid.Children.Add(newBorder);
                    GameGrid.RegisterName(newBorder.Name, newBorder);
                }
            }
        }

        private void ReRenderGameGrid()
        {
            foreach (var position in GameGrid.Children)
            {
                Dispatcher.Invoke(() =>
                {
                    if (position is Border border)
                        border.Background = new SolidColorBrush(Colors.Transparent);
                });
            }
        }

        private void RenderNewSnake()
        {
            foreach (var position in snake.GetBodyPositions())
            {
                Dispatcher.Invoke(() =>
                {
                    if (GameGrid.FindName($"GameGridBorderR{position.X}C{position.Y}") is Border border)
                        border.Background = new SolidColorBrush(Colors.Black);
                });
            }
        }
        private void RenderSnake(Coordinate oldPos, Coordinate newPos)
        {
            Dispatcher.Invoke(() =>
            {
                if (GameGrid.FindName($"GameGridBorderR{oldPos.X}C{oldPos.Y}") is Border oldBorder)
                    oldBorder.Background = new SolidColorBrush(Colors.Transparent);

                if (GameGrid.FindName($"GameGridBorderR{newPos.X}C{newPos.Y}") is Border newBorder)
                    newBorder.Background = new SolidColorBrush(Colors.Black);
            });
        }

        private void RenderCandy(Candy? oldCandy = null)
        {
            Dispatcher.Invoke(() =>
            {
                if (oldCandy != null && oldCandy.Coordinate.IsValid())
                {
                    if (GameGrid.FindName($"GameGridBorderR{oldCandy.Coordinate.X}C{oldCandy.Coordinate.Y}") is Border oldCandyBorder)
                        oldCandyBorder.Background = new SolidColorBrush(Colors.Black);
                }

                if (GameGrid.FindName($"GameGridBorderR{snake.Candy.Coordinate.X}C{snake.Candy.Coordinate.Y}") is Border newCandyBorder)
                    newCandyBorder.Background = new SolidColorBrush(Colors.YellowGreen);
            });
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            var name = PlayerNameTextBox.Text;

            if (name == null)
            {
                MessageBox.Show("Please enter a valid name. ");
                return;
            }
            if (speed == SpeedOptions.Not_Selected)
            {
                MessageBox.Show("Please select a speed option.");
                return;
            }

            snake.Initialize(speed);
            ReRenderGameGrid();
            RenderNewSnake();
            RenderCandy();
            snakeGameVM.StartGame(name, speed);
            snake.StartGame();
        }

        private void SnakeDiesHandler(SnakeDiesExceptions ex)
        {
            MessageBox.Show(ex.Message + "\nYour score is " + $"{snakeGameVM.Score}");
            snakeGameVM.StopGame();
        }

        private void SnakeEatsCandyHandler(Candy oldCandy)
        {
            RenderCandy(oldCandy);
            snakeGameVM.Score++;
        }

        private void RemoveRecordsRequest(object sender, RoutedEventArgs e)
        {
            var authentication = new RecordsRemovalAuthentication();
            authentication.ShowDialog();
            if (authentication.DialogResult == true) RemoveAllRecords();
        }

        private static void RemoveAllRecords()
        {
            if (File.Exists("Records.xml"))
            {
                File.Delete("Records.xml");
                MessageBox.Show("All records have been removed.");
            }
            else
            {
                MessageBox.Show("No record found.");
            }
        }

        private void PauseGameButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PauseGameButton.Visibility = Visibility.Collapsed;
            ResumeGameButton.Visibility = Visibility.Visible;
            snake.PauseGame();
        }

        private void ResumeGameButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PauseGameButton.Visibility = Visibility.Visible;
            ResumeGameButton.Visibility = Visibility.Collapsed;
            snake.ResumeGame();
        }

        private void StopGameButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            snakeGameVM.StopGame();
            snake.StopGame();
        }

        private void AboutGameButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://en.wikipedia.org/wiki/Snake_(video_game_genre)",
                UseShellExecute = true
            });
        }
        private void AboutMeButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.linkedin.com/in/yuchen-li-418a68158/",
                UseShellExecute = true
            });
        }
    }
}
