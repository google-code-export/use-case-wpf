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

namespace Comment
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class myComment : Comment
    {
        public myComment()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(myComment));

        public string Text
        {
            get { return TextBox.Text; }

            set { TextBox.Text = value; }
        }
        public static DependencyProperty IdProperty = DependencyProperty.Register("Id", typeof(string), typeof(myComment));
        public int Id
        { get; set; }

        public static DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility", typeof(string), typeof(myComment));
        public Visibility Visibility
        {
            get {return rectangle1.Visibility ;}
            set { rectangle1.Visibility = value;} 
        }

        public static DependencyProperty WProperty = DependencyProperty.Register("W", typeof(string), typeof(myComment));
        public double W
        {
            get
            {
                return Width;
            }
            set
            {
                if (value >= 50)
                {
                    Width = value;
                    rectangle1.Width = value - 10;
                    rectangle2.Width = value - 20;
                    TextBox.Width = value - 35;
                }
            }
        }
        public static DependencyProperty HProperty = DependencyProperty.Register("H", typeof(string), typeof(myComment));
        public double H
        {
            get
            {
                return Height;
            }
            set
            {
                if (value >= 50)
                {
                    Height = value;
                    rectangle1.Height = value - 10;
                    rectangle2.Height = value - 20;
                    TextBox.Height = value - 35;
                }
            }
        }
        public static DependencyProperty EllipProperty = DependencyProperty.Register("Ellip", typeof(string), typeof(myComment));
        public Ellipse Ellip
        {
            get { return ellipse1; }
        }
    }
}
