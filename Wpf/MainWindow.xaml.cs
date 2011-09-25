using System;
using System.Collections.Generic;
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

namespace Wpf
{
    public partial class MainWindow : Window
    {

        Point InitMousePos;
        public MainWindow()
        {
            InitializeComponent();
        }
        void AddEllipse()
        {
            
            //PRECEDENT

            Precedent.myPrecedent precedent = new Precedent.myPrecedent();
            precedent.Margin = new Thickness(10, 70, 0, 0);
            precedent.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
            precedent.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            precedent.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            precedent.VerticalContentAlignment = System.Windows.VerticalAlignment.Top;

            precedent.Text = "Text text text";

            myCanvas.Children.Add(precedent);
            precedent.MouseDown += new MouseButtonEventHandler(myPrecedent_Move_MouseDown);
            precedent.MouseMove += new MouseEventHandler(myPrecedent_MouseMove);

        }
        void AddLine()
        {
            ArrowLine aline1 = new ArrowLine();
            aline1.Stroke = Brushes.Black;
            aline1.StrokeThickness = 3;
            aline1.StrokeDashArray.Add(8.0);
            aline1.X1 = 400;
            aline1.Y1 = 400;
            aline1.X2 = 500;
            aline1.Y2 = 500;
            
            myCanvas.Children.Add(aline1);
            
        }

        private void BtnActor_Click(object sender, RoutedEventArgs e)
        {
            Actor.myActor actor = new Actor.myActor();
            actor.Margin = new Thickness(50, 170, 0, 0);
            actor.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
            actor.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            actor.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            actor.VerticalContentAlignment = System.Windows.VerticalAlignment.Top;

            actor.Text = "Text text text";

            myCanvas.Children.Add(actor);
            actor.MouseDown += new MouseButtonEventHandler(myActor_Move_MouseDown);
            actor.MouseMove += new MouseEventHandler(myActor_MouseMove);
            
        }
        void myActor_Move_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture((Actor.myActor)sender);
            InitMousePos.X = e.GetPosition(myCanvas).X;
            InitMousePos.Y = e.GetPosition(myCanvas).Y;
        }
        void myActor_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var Actor = (Actor.myActor)sender;
                double mouseX = e.GetPosition(myCanvas).X, mouseY = e.GetPosition(myCanvas).Y;
                Point currentPoint = e.GetPosition(myCanvas);
                if ((mouseX > 50) && (mouseX < myCanvas.ActualWidth - 50) && (mouseY > 110) && (mouseY < myCanvas.ActualHeight - 110)
                    && Math.Abs(currentPoint.X - InitMousePos.X) > SystemParameters.MinimumHorizontalDragDistance
                    && Math.Abs(currentPoint.Y - InitMousePos.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    Actor.Margin = new Thickness(e.GetPosition(myCanvas).X - 50, e.GetPosition(myCanvas).Y - 110, 0, 0);
                    Point[] p = new Point[8];
                    p[0].X = Actor.Margin.Left;
                    p[1].X = Actor.Margin.Left;
                    p[2].X = Actor.Margin.Left;
                    p[3].X = Actor.Margin.Left+50;
                    p[4].X = Actor.Margin.Left+50;
                    p[5].X = Actor.Margin.Left+100;                    
                    p[6].X = Actor.Margin.Left+100;
                    p[7].X = Actor.Margin.Left+100;
                    p[0].Y = Actor.Margin.Top;
                    p[1].Y = Actor.Margin.Top+110;
                    p[2].Y = Actor.Margin.Top+220;
                    p[3].Y = Actor.Margin.Top ;
                    p[4].Y = Actor.Margin.Top +220;
                    p[5].Y = Actor.Margin.Top ;
                    p[6].Y = Actor.Margin.Top + 110;
                    p[7].Y = Actor.Margin.Top + 220;

                    ArrowLine line = (ArrowLine)myCanvas.Children[0];
                    Point PointMin = new Point();
                    PointMin = MinimumDistance(new Point(line.X1, line.Y1), p);
                    
                    line.X2 = PointMin.X;
                    line.Y2 = PointMin.Y;

                }
            }
            else
                Mouse.Capture(null);
        }
        
        void myPrecedent_Move_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture((Precedent.myPrecedent)sender);
            InitMousePos.X = e.GetPosition(myCanvas).X;
            InitMousePos.Y = e.GetPosition(myCanvas).Y;
        }
        //перемещение PRECEDENT
        void myPrecedent_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var Precedent = (Precedent.myPrecedent)sender;
                double mouseX = e.GetPosition(myCanvas).X, mouseY = e.GetPosition(myCanvas).Y;
                Point currentPoint = e.GetPosition(myCanvas);
                if ((mouseX > 75) && (mouseX < myCanvas.ActualWidth - 75) && (mouseY > 37.5) && (mouseY < myCanvas.ActualHeight - 37.5)
                    && Math.Abs(currentPoint.X - InitMousePos.X) > SystemParameters.MinimumHorizontalDragDistance
                    && Math.Abs(currentPoint.Y - InitMousePos.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    Precedent.Margin = new Thickness(e.GetPosition(myCanvas).X - 75, e.GetPosition(myCanvas).Y - 37.5, 0, 0);
                    Point[] p = new Point[8];
                    p[0].X = Precedent.Margin.Left;
                    p[1].X = Precedent.Margin.Left + 37.5;
                    p[2].X = Precedent.Margin.Left + 37.5;
                    p[3].X = Precedent.Margin.Left + 75;
                    p[4].X = Precedent.Margin.Left + 75;
                    p[5].X = Precedent.Margin.Left + 112.5;
                    p[6].X = Precedent.Margin.Left + 112.5;
                    p[7].X = Precedent.Margin.Left + 150;
                    p[0].Y = Precedent.Margin.Top + 37.5;
                    p[1].Y = Precedent.Margin.Top + 5;
                    p[2].Y = Precedent.Margin.Top + 70;
                    p[3].Y = Precedent.Margin.Top;
                    p[4].Y = Precedent.Margin.Top + 75;
                    p[5].Y = Precedent.Margin.Top + 5;
                    p[6].Y = Precedent.Margin.Top + 70;
                    p[7].Y = Precedent.Margin.Top + 37.5;

                    ArrowLine line = (ArrowLine)myCanvas.Children[0];
                    Point PointMin = new Point();
                    PointMin = MinimumDistance(new Point(line.X1, line.Y1), p);

                    line.X2 = PointMin.X;
                    line.Y2 = PointMin.Y;

                }
            }
            else
                Mouse.Capture(null);
        }

        private void BtnComment_Click(object sender, RoutedEventArgs e)
        {
            Comment.myComment comment = new Comment.myComment();
            comment.Margin = new Thickness(50, 170, 0, 0);
            comment.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
            comment.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            comment.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            comment.VerticalContentAlignment = System.Windows.VerticalAlignment.Top;

            comment.Text = "Text text text";

            myCanvas.Children.Add(comment);
            comment.MouseDown += new MouseButtonEventHandler(myComment_Move_MouseDown);
            comment.MouseMove += new MouseEventHandler(myComment_MouseMove);
        }
        void myComment_Move_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture((Comment.myComment)sender);
            InitMousePos.X = e.GetPosition(myCanvas).X;
            InitMousePos.Y = e.GetPosition(myCanvas).Y;
        }
        void myComment_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var Comment = (Comment.myComment)sender;
                double mouseX = e.GetPosition(myCanvas).X, mouseY = e.GetPosition(myCanvas).Y;
                Point currentPoint = e.GetPosition(myCanvas);
                if ((mouseX > 65) && (mouseX < myCanvas.ActualWidth - 65) && (mouseY > 50) && (mouseY < myCanvas.ActualHeight - 50)
                    && Math.Abs(currentPoint.X - InitMousePos.X) > SystemParameters.MinimumHorizontalDragDistance
                    && Math.Abs(currentPoint.Y - InitMousePos.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    Comment.Margin = new Thickness(e.GetPosition(myCanvas).X - 65, e.GetPosition(myCanvas).Y - 50, 0, 0);
                    Point[] p = new Point[8];
                    p[0].X = Comment.Margin.Left;
                    p[1].X = Comment.Margin.Left;
                    p[2].X = Comment.Margin.Left;
                    p[3].X = Comment.Margin.Left + 65;
                    p[4].X = Comment.Margin.Left + 65;
                    p[5].X = Comment.Margin.Left + 125;
                    p[6].X = Comment.Margin.Left + 130;
                    p[7].X = Comment.Margin.Left + 130;
                    p[0].Y = Comment.Margin.Top;
                    p[1].Y = Comment.Margin.Top + 50;
                    p[2].Y = Comment.Margin.Top + 100;
                    p[3].Y = Comment.Margin.Top;
                    p[4].Y = Comment.Margin.Top + 100;
                    p[5].Y = Comment.Margin.Top + 5;
                    p[6].Y = Comment.Margin.Top + 50;
                    p[7].Y = Comment.Margin.Top + 100;

                    ArrowLine line = (ArrowLine)myCanvas.Children[0];
                    Point PointMin = new Point();
                    PointMin = MinimumDistance(new Point(line.X1, line.Y1), p);

                    line.X2 = PointMin.X;
                    line.Y2 = PointMin.Y;

                    Canvas.SetZIndex(line, 10);//to front
                }
            }
            else
                Mouse.Capture(null);
        }

        private void BtnPrecedent_Click(object sender, RoutedEventArgs e)
        {
            AddLine();
            AddEllipse();
        }

        private Point MinimumDistance( Point StartPoint, Point[] p)
        {
            Point PointMin = p[0];
            double MinDistance = Math.Sqrt(Math.Pow(StartPoint.X-p[0].X,2)+Math.Pow(StartPoint.Y-p[0].Y,2));
            for (int i = 1; i < p.Length; ++i)
            {
                double CerrentDistance =  Math.Sqrt(Math.Pow(StartPoint.X - p[i].X, 2) + Math.Pow(StartPoint.Y - p[i].Y, 2));
                if (MinDistance > CerrentDistance)
                {
                    PointMin = p[i];
                    MinDistance = CerrentDistance;
                }
            }
            return PointMin;
        }
    }
}
