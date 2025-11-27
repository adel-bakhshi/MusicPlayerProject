using Microsoft.Win32;
using MusicPlayerProject.Commands;
using MusicPlayerProject.Models;
using MusicPlayerProject.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MusicPlayerProject.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Fields

        private readonly DispatcherTimer _timer;
        private double _totalSeconds;
        private double _elapsedSeconds;
        private MediaElement _mediaElement;

        #endregion Fields

        #region Properties

        public ICommand CloseCommand { get; set; }
        public ICommand OpenFilesCommand { get; set; }
        public ICommand ShuffleCommand { get; set; }
        public ICommand BackwardCommand { get; set; }
        public ICommand PreviousCommand { get; set; }
        public ICommand PlayStopCommand { get; set; }
        public ICommand NextCommand { get; set; }
        public ICommand ForwardCommand { get; set; }
        public ICommand RepeatCommand { get; set; }

        private BitmapImage _currentMusicCover = null;

        public BitmapImage CurrentMusicCover
        {
            get => _currentMusicCover;
            set
            {
                _currentMusicCover = value;
                OnPropertyChanged(nameof(CurrentMusicCover));
            }
        }

        private string _title = "";

        public string Title
        {
            get => _title;
            set
            {
                _title = value ?? "";
                OnPropertyChanged(nameof(Title));
            }
        }

        private string _caption = "";

        public string Caption
        {
            get => _caption;
            set
            {
                _caption = value ?? "";
                OnPropertyChanged(nameof(Caption));
            }
        }

        private double _sliderValue = 0;

        public double SliderValue
        {
            get => _sliderValue;
            set
            {
                _sliderValue = value;
                OnPropertyChanged(nameof(SliderValue));
            }
        }

        private string _duration = "00:00";

        public string Duration
        {
            get => _duration;
            set
            {
                _duration = value ?? "00:00";
                OnPropertyChanged(nameof(Duration));
            }
        }

        private string _elapsed = "00:00";

        public string Elapsed
        {
            get => _elapsed;
            set
            {
                _elapsed = value ?? "00:00";
                OnPropertyChanged(nameof(Elapsed));
            }
        }

        private bool _isShuffleEnabled = false;

        public bool IsShuffleEnabled
        {
            get => _isShuffleEnabled;
            set
            {
                _isShuffleEnabled = value;
                OnPropertyChanged(nameof(IsShuffleEnabled));
            }
        }

        private bool _isReapetEnabled = false;

        public bool IsRepeatEnabled
        {
            get => _isReapetEnabled;
            set
            {
                _isReapetEnabled = value;
                OnPropertyChanged(nameof(IsRepeatEnabled));
            }
        }

        private bool _isStoped = true;

        public bool IsStoped
        {
            get => _isStoped;
            set
            {
                _isStoped = value;
                OnPropertyChanged(nameof(IsStoped));
            }
        }

        private ObservableCollection<Music> _musics;

        public ObservableCollection<Music> Musics
        {
            get => _musics;
            set
            {
                _musics = value ?? new ObservableCollection<Music>();
                OnPropertyChanged(nameof(Musics));
            }
        }

        private Music _selectedMusic = null;

        public Music SelectedMusic
        {
            get => _selectedMusic;
            set
            {
                _selectedMusic = value;
                if (value != null)
                    OpenNewMusic();

                OnPropertyChanged(nameof(SelectedMusic));
            }
        }

        public bool IsDragging { get; set; }

        #endregion Properties

        public MainViewModel()
        {
            CloseCommand = new RelayCommand(CloseWindow);
            OpenFilesCommand = new RelayCommand(OpenFilesSelector);
            ShuffleCommand = new RelayCommand(ToggleShuffleSelection);
            BackwardCommand = new RelayCommand(OpenFirstMusic);
            PreviousCommand = new RelayCommand(OpenPreviousMusic);
            PlayStopCommand = new RelayCommand(PlayStopMusic);
            NextCommand = new RelayCommand(OpenNextMusic);
            ForwardCommand = new RelayCommand(OpenLastMusic);
            RepeatCommand = new RelayCommand(ToggleRepeatCurrentMusic);

            Musics = new ObservableCollection<Music>();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1),
            };

            _timer.Tick += TimerOnTick;

            _totalSeconds = 0;
            _elapsedSeconds = 0;
            _mediaElement = null;
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            _elapsedSeconds++;
            if (_elapsedSeconds < _totalSeconds)
            {
                var timeSpan = TimeSpan.FromSeconds(_elapsedSeconds);
                SliderValue = (_elapsedSeconds / _totalSeconds) * 100;
                Elapsed = $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            }
            else
            {
                _timer.Stop();
                SliderValue = 0;
                Elapsed = "00:00";

                if (IsRepeatEnabled)
                {
                    OpenNewMusic();
                }
                else if (IsShuffleEnabled)
                {
                    var random = new Random();
                    int randomIndex = random.Next(0, Musics.Count);
                    SelectedMusic = Musics[randomIndex];
                }
                else
                {
                    int currentMusicIndex = Musics.IndexOf(SelectedMusic);
                    if (currentMusicIndex != -1)
                    {
                        if (currentMusicIndex + 1 == Musics.Count)
                        {
                            SelectedMusic = Musics.FirstOrDefault();
                        }
                        else
                        {
                            SelectedMusic = Musics[currentMusicIndex + 1];
                        }
                    }
                }
            }
        }

        private void ToggleRepeatCurrentMusic(object obj)
        {
            if (SelectedMusic != null)
                IsRepeatEnabled = !IsRepeatEnabled;
            else
                IsRepeatEnabled = false;
        }

        private void OpenLastMusic(object obj)
        {
            if (SelectedMusic != Musics.LastOrDefault())
                SelectedMusic = Musics.LastOrDefault();
        }

        private void OpenNextMusic(object obj)
        {
            int index = Musics.IndexOf(SelectedMusic);
            if (index != -1 && index < Musics.Count - 1)
                SelectedMusic = Musics[index + 1];
        }

        private void PlayStopMusic(object obj)
        {
            if (_mediaElement == null)
            {
                var window = Application.Current.MainWindow;
                if (window == null)
                    return;

                var mediaElement = (window as MainWindow)?.MelMusicPlayer;
                if (mediaElement == null)
                    return;

                if (SelectedMusic == null)
                    return;

                mediaElement.Source = new Uri(SelectedMusic.Source);
                _mediaElement = mediaElement;
            }

            if (_mediaElement == null)
                return;

            if (IsStoped)
            {
                _timer.Start();
                _mediaElement.Play();
            }
            else
            {
                _timer.Stop();
                _mediaElement.Pause();
            }

            IsStoped = !IsStoped;
        }

        private void OpenPreviousMusic(object obj)
        {
            int index = Musics.IndexOf(SelectedMusic);
            if (index != -1 && index > 0)
                SelectedMusic = Musics[index - 1];
        }

        private void OpenFirstMusic(object obj)
        {
            if (SelectedMusic != Musics.FirstOrDefault())
                SelectedMusic = Musics.FirstOrDefault();
        }

        private void ToggleShuffleSelection(object obj)
        {
            IsShuffleEnabled = !IsShuffleEnabled;
        }

        private void OpenFilesSelector(object obj)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "MP3 | *.mp3";
                dialog.Multiselect = true;
                dialog.Title = "Select MP3 file(s)";
                var result = dialog.ShowDialog();

                var musicList = new List<Music>();
                if (result == true && dialog.FileNames.Length > 0)
                {
                    foreach (var fileName in dialog.FileNames)
                    {
                        var file = TagLib.File.Create(fileName);
                        if (file == null)
                            continue;

                        var pictureData = file.Tag.Pictures?.FirstOrDefault()?.Data?.Data ?? null;
                        BitmapImage musicCover = null;
                        if (pictureData != null)
                        {
                            using (var ms = new MemoryStream(pictureData))
                            {
                                musicCover = new BitmapImage();
                                musicCover.BeginInit();
                                musicCover.CacheOption = BitmapCacheOption.OnLoad;
                                musicCover.StreamSource = ms;
                                musicCover.EndInit();
                            }
                        }

                        var music = new Music
                        {
                            Cover = musicCover,
                            Title = file.Tag.Title ?? Path.GetFileNameWithoutExtension(fileName),
                            Caption = file.Tag.FirstAlbumArtist ?? "<No information about artist>",
                            Duration = $"{file.Properties.Duration.Minutes:00}:{file.Properties.Duration.Seconds:00}",
                            Source = fileName,
                        };

                        musicList.Add(music);
                    }
                }

                Musics = new ObservableCollection<Music>(musicList);

                if (musicList.Any())
                    SelectedMusic = musicList[0];
            }
            catch
            {
                Musics = new ObservableCollection<Music>();
                SelectedMusic = null;
            }
        }

        private void CloseWindow(object obj)
        {
            Application.Current.MainWindow.Close();
        }

        private void OpenNewMusic()
        {
            _timer.Stop();
            _mediaElement?.Stop();
            _mediaElement = null;
            _elapsedSeconds = 0;
            _totalSeconds = 0;

            string durationStr = SelectedMusic.Duration;
            _totalSeconds = (int.Parse(durationStr.Split(':')[0]) * 60) + int.Parse(durationStr.Split(':')[1]);

            CurrentMusicCover = SelectedMusic.Cover;
            Title = SelectedMusic.Title;
            Caption = SelectedMusic.Caption;
            SliderValue = 0;
            Duration = durationStr;
            Elapsed = "00:00";
            IsStoped = true;

            PlayStopMusic(null);
        }

        public void OnSliderValueChange()
        {
            _elapsedSeconds = (SliderValue / 100) * _totalSeconds;
            var timeSpan = TimeSpan.FromSeconds(_elapsedSeconds);
            Elapsed = $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";

            if (_mediaElement != null && _mediaElement.Position.TotalSeconds != timeSpan.TotalSeconds)
            {
                _mediaElement.Position = timeSpan;
            }
        }
    }
}