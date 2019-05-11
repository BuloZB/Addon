﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Addon.Views
{
    //
    // https://stackoverflow.com/questions/22505461/binding-html-string-content-to-webview-in-xaml
    //
    public class MyExtensions
    {
        // "HtmlString" attached property for a WebView
        public static readonly DependencyProperty HtmlStringProperty =
           DependencyProperty.RegisterAttached("HtmlString", typeof(string), typeof(MyExtensions), new PropertyMetadata("", OnHtmlStringChanged));

        // Getter and Setter
        public static string GetHtmlString(DependencyObject obj) { return (string)obj.GetValue(HtmlStringProperty); }
        public static void SetHtmlString(DependencyObject obj, string value) { obj.SetValue(HtmlStringProperty, value); }

        private static void OnHtmlStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //  Debug.WriteLine("OnHTMLChanged");
            if (d is WebView webView)
            {

                var head = "<head><meta charset=\"UTF-8\"></head>";
                var styles = "<style>html{font-family:'Segoe UI'; font-size:14px;}</style>";
                var html = (string)e.NewValue;

                if (Window.Current.Content is Page rootFrame && rootFrame.RequestedTheme == ElementTheme.Dark)
                {
                    styles = "<style>html{font-family:'Segoe UI';  font-size:14px; color:#fff;}</style>";
                }

                webView.NavigateToString(head + styles + html);

            }
        }
    }

    //
    // Use this to prevent Hyperlinkbutton to crash on empty value
    //
    public class StringToUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var uri = value as string;
            if (string.IsNullOrEmpty(uri))
            {
                return new Uri("https://www.curseforge.com/wow/addons");
            }
            return new Uri(uri);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public sealed partial class MasterDetailDetailControl : UserControl
    {




        public Core.Models.Addon MasterMenuItem
        {
            get { return GetValue(MasterMenuItemProperty) as Core.Models.Addon; }
            set { SetValue(MasterMenuItemProperty, value); }
        }

        public static readonly DependencyProperty MasterMenuItemProperty = DependencyProperty.Register("MasterMenuItem", typeof(Core.Models.Addon), typeof(MasterDetailDetailControl), new PropertyMetadata(null, OnMasterMenuItemPropertyChanged));

        public MasterDetailDetailControl()
        {
            InitializeComponent();

        }

        private static void OnMasterMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MasterDetailDetailControl;
            control.ForegroundElement.ChangeView(0, 0, 1);
        }

        //private async void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        //{
        //    if (sender is WebView webView)
        //    {
        //       // await webView.InvokeScriptAsync("eval", new[] { DisableScrollingJs });
        //        //var heightString = await webView.InvokeScriptAsync("eval", new[] { "document.body.scrollHeight.toString()" });
        //        //int height;
        //        //if (int.TryParse(heightString, out height))
        //        //{
        //        //    webView.Height = height+50;
        //        //}
               
        //    }
        //}

        //string DisableScrollingJs = @"function RemoveScrolling()  
        //                      {  
        //                          var styleElement = document.createElement('style');  
        //                          var styleText = 'body, html { overflow: hidden; }'  
        //                          var headElements = document.getElementsByTagName('head');  
        //                          styleElement.type = 'text/css';  
        //                          if (headElements.length == 1)  
        //                          {  
        //                              headElements[0].appendChild(styleElement);  
        //                          }  
        //                          else if (document.head)  
        //                          {  
        //                              document.head.appendChild(styleElement);  
        //                          }  
        //                          if (styleElement.styleSheet)  
        //                          {  
        //                              styleElement.styleSheet.cssText = styleText;  
        //                          }  
        //                      }";



        //private async void WebView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        //{

        //    var heightString = await sender.InvokeScriptAsync("eval", new[] { "document.body.scrollHeight.toString()" });

        //    int height;
        //    Debug.WriteLine("Navigation completed " + heightString);
        //    if (int.TryParse(heightString, out height))
        //    {
        //        if (height < 10000)
        //        {
        //            sender.Height = height;
        //        }
        //    }
        //}






        //private void ForegroundElement_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        //{

        //   // Debug.WriteLine("pressed");
        //    isPressed = true;
        //    xPrev = e.GetCurrentPoint(sender as UIElement).Position.X;
        //    //var grid = this.Parent as Grid;
        //    //var gridParent = grid.Parent;

        //    //Debug.WriteLine(gridParent.ToString());
        //    //var elements = grid.Children;
        //    //foreach (var item in elements)
        //    //{
        //    //    Debug.WriteLine(item.ToString());
        //    //}

        //}

        //private void ForegroundElement_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        //{
        //    if (isPressed)
        //    {
        //        var x = e.GetCurrentPoint(sender as UIElement).Position.X;
        //        xDelta = x - xPrev;
        //        xPrev = x;
        //        //Debug.WriteLine("x: " + x + ", xprev: " + xPrev + ", xdelta; " + xDelta);
        //        MasterDetailPage.MyMasterDetailPage.MyResize(xDelta);
        //    }
        //}

        //private void ForegroundElement_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        //{
        //    //Debug.WriteLine("released");
        //    isPressed = false;
        //}

    }
}
