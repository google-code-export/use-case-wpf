using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Actor
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class myActor : Actor
    {
        public myActor()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(myActor));
        public string Text
        {
            get { return TextBox.Text; }

            set { TextBox.Text = value; }
        }
        public static readonly DependencyProperty IdProperty = DependencyProperty.Register("Id", typeof(string), typeof(myActor));
        public int Id
        { get; set; }

    }
}
