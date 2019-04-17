﻿using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Addon.Views
{
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
