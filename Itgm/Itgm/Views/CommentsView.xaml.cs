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
    /// Interaction logic for MediasPage.xaml
    /// </summary>
    public partial class CommentsView : UserControl
    {
        private ScrollViewer _scrollViewer;

        public CommentsView()
        {
            InitializeComponent();
        }

        //private void TweetUrlClick(object sender, RoutedEventArgs e)
        //{
        //    // Открываем ссылку в браузере
        //    var button = (Button)sender;
        //    Uri link = new Uri(button.Content.ToString());
        //    Process.Start(new ProcessStartInfo(link.AbsoluteUri));
        //    e.Handled = true;
        //}

        //private void ScrollToTop_Click(object sender, RoutedEventArgs e)
        //{
        //    if (Medias.Items.Count != 0)
        //    {
        //        Medias.ScrollIntoView(Medias.Items[0]);
        //    }
        //}

        //// Подгружаем твиты при достижении конца области скрола внизу
        //#region ScrollViewer
        //private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        //{
        //    var count = Medias.Items.Count;
        //    var offset = (int)e.VerticalOffset;
        //    var maxOffset = (int)_scrollViewer.ScrollableHeight;
        //    if (offset == maxOffset
        //        && count != 0)
        //    {
        //        //AddMediasButton.Command.Execute(Medias.Items[count - 1]);
        //    }
        //}

        //private void Medias_Loaded(object sender, RoutedEventArgs e)
        //{
        //    if (_scrollViewer == null)
        //    {
        //        var border = VisualTreeHelper.GetChild(Medias, 0);
        //        _scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
        //    }

        //    if (_scrollViewer != null)
        //    {
        //        _scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
        //    }
        //}

        //private void Medias_Unloaded(object sender, RoutedEventArgs e)
        //{
        //    if (_scrollViewer != null)
        //    {
        //        _scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
        //    }
        //}
        //#endregion
    }
}
