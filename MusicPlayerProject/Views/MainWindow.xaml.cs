using MusicPlayerProject.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace MusicPlayerProject.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private readonly MainViewModel _viewModel;
        private readonly DispatcherTimer _timer;

        #endregion Fields

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = ViewModel;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _timer.Tick += TimerOnTick;
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            WinMain_MouseUp(null, null);
            _timer.Stop();
        }

        private void MainBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void SliderThumb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _viewModel.IsDragging = true;
        }

        private void WinMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (_viewModel.IsDragging)
            {
                _timer.Stop();
                _viewModel.OnSliderValueChange();
                _timer.Start();
            }
        }

        private void WinMain_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _viewModel.IsDragging = false;
        }
    }
}