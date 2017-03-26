using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Itgm.Views
{
    /// <summary>
    /// Interaction logic for TweetsPage.xaml
    /// </summary>
    public partial class GeneralContentPage : UserControl
    {
        private ScrollViewer _scrollViewer;

        public GeneralContentPage()
        {
            InitializeComponent();
        }

        private void TweetUrlClick(object sender, RoutedEventArgs e)
        {
            // Открываем ссылку в браузере
            var button = (Button)sender;
            Uri link = new Uri(button.Content.ToString());
            Process.Start(new ProcessStartInfo(link.AbsoluteUri));
            e.Handled = true;
        }

        private void ContentImage_Click(object sender, RoutedEventArgs e)
        {
            // открываем твит в твиттере
            var button = (Button)sender;
            Uri link = new Uri(button.Tag.ToString());
            Process.Start(new ProcessStartInfo(link.AbsoluteUri));
            e.Handled = true;
        }

        private void ScrollToTop_Click(object sender, RoutedEventArgs e)
        {
            if (Tweets.Items.Count != 0)
            {
                Tweets.ScrollIntoView(Tweets.Items[0]);
            }
        }

        // Подгружаем твиты при достижении конца области скрола внизу
        #region ScrollViewer
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var count = Tweets.Items.Count;
            var offset = (int)e.VerticalOffset;
            var maxOffset = (int)_scrollViewer.ScrollableHeight;
            if (offset == maxOffset
                && count != 0)
            {
                AddTweetsButton.Command.Execute(Tweets.Items[count - 1]);
            }
        }

        private void Tweets_Loaded(object sender, RoutedEventArgs e)
        {
            if (_scrollViewer == null)
            {
                var border = VisualTreeHelper.GetChild(Tweets, 0);
                _scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
            }

            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            }
        }

        private void Tweets_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            }
        }
        #endregion
    }
}
