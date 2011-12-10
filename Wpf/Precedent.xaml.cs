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

namespace Precedent
{
    /// <summary>
    /// Логика взаимодействия для UserControl1.xaml
    /// </summary>
    public partial class myPrecedent : Precedent
    {
        public myPrecedent()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(myPrecedent));
        
        public string Text
        {
            get { return TextBox.Text; }

            set { TextBox.Text = value; }
        }


        public static readonly DependencyProperty ColorProperty =
        DependencyProperty.Register("Color", typeof(Color), typeof(myPrecedent),
        new FrameworkPropertyMetadata(new Color(), FrameworkPropertyMetadataOptions.AffectsRender));

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set
            {
                SetValue(ColorProperty, value);
                TextBox.Background = new SolidColorBrush(value);
                ellipse.Fill = new SolidColorBrush(value);
            }
        }
        public static readonly DependencyProperty IdProperty = DependencyProperty.Register("Id", typeof(string), typeof(myPrecedent));
        public int Id
        { get; set; }

        public static DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility", typeof(string), typeof(myPrecedent));
        public Visibility Visibility
        {
            get { return rectangle1.Visibility; }
            set { rectangle1.Visibility = value; }
        }

        //public static readonly MouseButtonEventArgs MouseDownEvent = EventManager.RegisterRoutedEvent("MouseDown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(myPrecedent));

        //public event RoutedEventHandler MouseDown
        //{
        //   add { AddHandler(MouseDownEvent, value); }
        //   remove { RemoveHandler(MouseDownEvent, value); }
        //}

    }
}

