﻿using System;
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

namespace ShareX_windows.Pages
{
    /// <summary>
    /// Interaction logic for CallLogsPage.xaml
    /// </summary>
    public partial class CallLogsPage : Page
    {
        public CallLogsPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LGB.EndPoint = new Point(double.Parse(tb.Text), 0);
        }
    }
}