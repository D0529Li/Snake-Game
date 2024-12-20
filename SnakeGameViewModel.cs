﻿using File_Organizer;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Snake_Game
{
    public class SnakeGameViewModel : INotifyPropertyChanged
    {
        private Player Player { get; set; } = new Player();
        public int Score
        {
            get { return Player.Score; }
            set
            {
                Player.Score = value;
                OnPropertyChanged(nameof(Score));
                OnPropertyChanged(nameof(DisplayedScore));
            }
        }

        public string DisplayedScore
        {
            get
            {
                return $"Score: {Score}";
            }
        }

        private bool _mainControlPanelVisibility;
        private bool _newGamePanelVisibility;
        private bool _gamePanelVisibility;


        public bool MainControlPanelVisibility
        {
            get => _mainControlPanelVisibility;
            set
            {
                _mainControlPanelVisibility = value;
                OnPropertyChanged(nameof(MainControlPanelVisibility));
            }
        }

        public bool NewGamePanelVisibility
        {
            get => _newGamePanelVisibility;
            set
            {
                _newGamePanelVisibility = value;
                OnPropertyChanged(nameof(NewGamePanelVisibility));
            }
        }

        public bool GamePanelVisibility
        {
            get => _gamePanelVisibility;
            set
            {
                _gamePanelVisibility = value;
                OnPropertyChanged(nameof(GamePanelVisibility));
            }
        }

        public delegate void ArrowKeyPressedHandler(Direction direction);
        public event ArrowKeyPressedHandler? ArrowKeyPressed;

        #region Commands

        public ICommand LeftArrowKeyCommand { get; set; }
        public ICommand RightArrowKeyCommand { get; set; }
        public ICommand UpArrowKeyCommand { get; set; }
        public ICommand DownArrowKeyCommand { get; set; }
        public ICommand MakeNewGameCommand { get; set; }

        #endregion Commands

        public SnakeGameViewModel()
        {
            MakeNewGameCommand = new DelegateCommand<object>(MakeNewGame);
            LeftArrowKeyCommand = new DelegateCommand<object>(OnPressLeftArrowKey);
            RightArrowKeyCommand = new DelegateCommand<object>(OnPressRightArrowKey);
            UpArrowKeyCommand = new DelegateCommand<object>(OnPressUpArrowKey);
            DownArrowKeyCommand = new DelegateCommand<object>(OnPressDownArrowKey);

            MainControlPanelVisibility = true;
            NewGamePanelVisibility = false;
            GamePanelVisibility = false;
        }

        public void MakeNewGame(object param)
        {
            NewGamePanelVisibility = true;
        }

        public void StartGame(string name, SpeedOptions speed)
        {
            Player.Name = name;
            Player.Speed = speed;
            MainControlPanelVisibility = false;
            NewGamePanelVisibility = false;
            GamePanelVisibility = true;
            Score = 4;
        }

        public void StopGame()
        {
            SaveRecord();
            Score = 0;
            MainControlPanelVisibility = true;
            NewGamePanelVisibility = true;
            GamePanelVisibility = false;
        }

        public void SaveRecord()
        {
            if (File.Exists("Records.xml"))
            {
                Records records;
                using (var stream = File.OpenRead("Records.xml"))
                {
                    var serializer = new XmlSerializer(typeof(Records));
                    records = serializer.Deserialize(stream) as Records;
                }
                records.ModifyRecords(Player);
                using (var stream = File.Open("Records.xml", FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(Records));
                    serializer.Serialize(stream, records);
                }
            }
            else
            {
                using (var stream = File.Open("Records.xml", FileMode.Create))
                {
                    var serializer = new XmlSerializer(typeof(Records));
                    serializer.Serialize(stream, new Records(Player));
                }
            }
        }

        #region Keyboard events

        public void OnPressLeftArrowKey(object param)
        {
            ArrowKeyPressed?.Invoke(Direction.Left);
        }

        public void OnPressRightArrowKey(object param)
        {
            ArrowKeyPressed?.Invoke(Direction.Right);
        }

        public void OnPressUpArrowKey(object param)
        {
            ArrowKeyPressed?.Invoke(Direction.Up);
        }

        public void OnPressDownArrowKey(object param)
        {
            ArrowKeyPressed?.Invoke(Direction.Down);
        }

        #endregion

        #region INotifyPropertyChanged Implements

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion
    }
}
