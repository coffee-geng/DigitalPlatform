using Coffee.DigitalPlatform.CommWPF;
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

namespace Coffee.DigitalPlatform.Views
{
    /// <summary>
    /// DataPager.xaml 的交互逻辑
    /// </summary>
    public partial class DataPager : UserControl
    {
        public DataPager()
        {
            InitializeComponent();
        }

        public ListCollectionView Source
        {
            get { return (ListCollectionView)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ListCollectionView), typeof(DataPager), new PropertyMetadata(null, OnSourceChanged));
        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pager = d as DataPager;
            var vm = pager.DataContext as IDataPager;
            var view = e.NewValue as ListCollectionView;
            pager.refreshForPageCount(view, pager.PageSize);
            pager.CurrentPage = 1;
            view?.Refresh();
        }
        private void refreshForPageCount(ListCollectionView view, int pageSize)
        {
            if (view != null && view.SourceCollection != null)
            {
                int totalItems = view.SourceCollection.Cast<object>().ToList().Count;
                PageCount = (int)Math.Ceiling((double)view.SourceCollection.Cast<object>().Count() / pageSize);
            }
            else
            {
                PageCount = 0;
            }
        }

        public DataPagingDisplayModes DisplayMode
        {
            get { return (DataPagingDisplayModes)GetValue(DisplayModeProperty); }
            set { SetValue(DisplayModeProperty, value); }
        }
        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register("DisplayMode", typeof(DataPagingDisplayModes), typeof(DataPager), new PropertyMetadata(Coffee.DigitalPlatform.Views.DataPagingDisplayModes.FirstLastPreviousNextNumeric));

        public bool IsShowPageSize
        {
            get { return (bool)GetValue(IsShowPageSizeProperty); }
            set { SetValue(IsShowPageSizeProperty, value); }
        }
        public static readonly DependencyProperty IsShowPageSizeProperty =
            DependencyProperty.Register("IsShowPageSize", typeof(bool), typeof(DataPager), new PropertyMetadata(true));

        public int CurrentPage
        {
            get { return (int)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", typeof(int), typeof(DataPager), new PropertyMetadata(0, null, OnCurrentCoerced));

        private static object OnCurrentCoerced(DependencyObject d, object baseValue)
        {
            var pager = d as DataPager;
            if (baseValue == null || !int.TryParse(baseValue.ToString(), out int currentPage))
            {
                return baseValue;
            }
            pager.btnFirst.IsEnabled = currentPage > 1;
            pager.btnPrev.IsEnabled = currentPage > 1;
            pager.btnNext.IsEnabled = currentPage < pager.PageCount;
            pager.btnLast.IsEnabled = currentPage < pager.PageCount;

            //当前页百分比计算规则是：第一页为0%，最后一页为100%，中间页根据位置线性计算百分比
            if (pager.PageCount > 1)
            {
                pager._currentPagePercent = pager.PageCount > 0 ? (double)(currentPage - 1) / (pager.PageCount - 1) : 0;
            }
            else
            {
                pager._currentPagePercent = 0; //只有一页或没有数据时，当前页百分比为0
            }
            return baseValue;
        }

        private double _currentPagePercent = 0; // 当前页在总页数中的百分比

        public int PageSize
        {
            get { return (int)GetValue(PageSizeProperty); }
            set { SetValue(PageSizeProperty, value); }
        }
        public static readonly DependencyProperty PageSizeProperty =
            DependencyProperty.Register("PageSize", typeof(int), typeof(DataPager), new PropertyMetadata(10, OnPageSizeChanged));
        private static void OnPageSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pager = d as DataPager;
            var pSize = (int)e.NewValue;
            pager.refreshForPageCount(pager.Source, pSize);

            if (pager.Source != null && pager.Source.SourceCollection != null)
            {
                pager.CurrentPage = pager._currentPagePercent > 0 ? (int)Math.Round(pager._currentPagePercent * (pager.PageCount - 1)) + 1 : 1; // 根据当前页百分比计算新的当前页
            }
            else
            {
                pager.CurrentPage = 1;
            }
        }

        public int PageCount
        {
            get { return (int)GetValue(PageCountProperty); }
            set { SetValue(PageCountProperty, value); }
        }
        public static readonly DependencyProperty PageCountProperty =
            DependencyProperty.Register("PageCount", typeof(int), typeof(DataPager), new PropertyMetadata(0));

        private void btnFirst_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext != null && DataContext is IDataPager pager)
            {
                pager.CurrentPage = 1;
                Source.Refresh();
            }
        }

        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext != null && DataContext is IDataPager pager)
            {
                int prevPageIndex = Math.Min(CurrentPage - 1, 1);
                pager.CurrentPage = prevPageIndex;
                Source.Refresh();
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext != null && DataContext is IDataPager pager)
            {
                int nextPageIndex = Math.Min(CurrentPage + 1, PageCount);
                pager.CurrentPage = nextPageIndex;
                Source.Refresh();
            }
        }

        private void btnLast_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext != null && DataContext is IDataPager pager)
            {
                pager.CurrentPage = PageSize;
                Source.Refresh();
            }
        }

        private void btnJump_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext != null && DataContext is IDataPager pager)
            {
                pager.CurrentPage = CurrentPage;
                Source.Refresh();
            }
        }

        private void combPagesize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0 && e.AddedItems[0] is int pageSize)
            {
                PageSize = pageSize;
            }
        }
    }

    public enum DataPagingDisplayModes
    {
        FirstLastPreviousNextNumeric, //首页、末页、上一页、下一页、页码跳转
        FirstLastPreviousNext,
        PreviousNextNumeric, //上一页、下一页、页码跳转
        PreviousNext
    }

    public class PageSizeSelectedIndexConverter : IMultiValueConverter
    {
        public static PageSizeSelectedIndexConverter Instance { get; } = new PageSizeSelectedIndexConverter();

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values == null || values.Length < 2)
            {
                return 0;
            }
            if (values[0] is int pageSize && values[1] != null && values[1] is IEnumerable<int> pageSizeOptions)
            {
                return pageSizeOptions.ToList().IndexOf(pageSize);
            }
            return 0;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
