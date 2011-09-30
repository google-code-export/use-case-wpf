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
//*****************************Переменные***********************************
        Point InitMousePos;         //Позиция мышки при клике на объекте
        bool FlagArrow = false;     //флаг, указывающий на попытку добавления связи между объектами (true - связь в данный момент добавляется, false - не добаляется)
        bool FirstObject = true;    //флаг, указывающий какой объект выбран для связи (true - значит первый)
        bool SecondObject = false;  //флаг, указывающий какой объект выбран для связи (true - значит второй)
        Point StartPoint;           //точка из которой выходит связь
        Point EndPoint;             //конечная точка
        object First;               //первый выбранный объект при добавлении связей
//**************************************************************************

        public MainWindow()
        {
            InitializeComponent();
        }
//***************************Обработчики событий*****************************
        /// <summary>
        /// Обработчик события нажатия кнопки "Добавление актера"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnActor_Click(object sender, RoutedEventArgs e)
        {
            Actor.myActor actor = new Actor.myActor();
            actor.Margin = new Thickness(50, 170, 0, 0);
            actor.Text = "Text text text";
            myCanvas.Children.Add(actor);
            actor.MouseDown += myActor_Move_MouseDown;
            actor.MouseMove += myActor_MouseMove;
            
        }
        /// <summary>
        /// Обработчик события при клике на актере
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void myActor_Move_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (FlagArrow)                                                  //если происходит добавление связи
            {
                Actor.myActor actor = (Actor.myActor)sender;
                actor.MouseMove -= myActor_MouseMove;                       //удаляем обработчик события MouseMove
                if (FirstObject)                                            //если это первый выбранный объект (от которого проводится стрелка)
                {
                    FirstObject = false;                                    
                    SecondObject = true;
                    StartPoint.X = actor.Margin.Left + 50;                  //начальная точка - середина актера
                    StartPoint.Y = actor.Margin.Top + 110;

                    First = sender;

                }
                else if (SecondObject)                                      //если выбран второй объект
                {

                    EndPoint.X = actor.Margin.Left + 50;                    //конечная точка - середина актера
                    EndPoint.Y = actor.Margin.Top + 110;
                    if (StartPoint != EndPoint)                             //если выбраны разные объекты
                    {
                        //add association in DB

                        AddLine(First, sender, 1);                          //добавляем связь

                    }
                }
            }
            else                                                            //если происходит перемещение объекта
            {
                Mouse.Capture((Actor.myActor)sender);                       //захватываем мышь          
                InitMousePos.X = e.GetPosition(myCanvas).X;
                InitMousePos.Y = e.GetPosition(myCanvas).Y;
            }
        }
        /// <summary>
        /// Обработчик события перемещения актера 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void myActor_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var Actor = (Actor.myActor)sender;
                double mouseX = e.GetPosition(myCanvas).X, mouseY = e.GetPosition(myCanvas).Y;
                Point currentPoint = e.GetPosition(myCanvas);
                //если актер находится в пределах канваса и перемещение совершено на расстояние большее минимально предусмотренного
                if ((mouseX > 50) && (mouseX < myCanvas.ActualWidth - 50) && (mouseY > 110) && (mouseY < myCanvas.ActualHeight - 110)
                    && Math.Abs(currentPoint.X - InitMousePos.X) > SystemParameters.MinimumHorizontalDragDistance
                    && Math.Abs(currentPoint.Y - InitMousePos.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    //меняем позицию актера
                    Actor.Margin = new Thickness(e.GetPosition(myCanvas).X - 50, e.GetPosition(myCanvas).Y - 110, 0, 0);

                    //пока не удалять!!!
 
                    //Point[] p = new Point[8];
                    //p[0].X = Actor.Margin.Left;
                    //p[1].X = Actor.Margin.Left;
                    //p[2].X = Actor.Margin.Left;
                    //p[3].X = Actor.Margin.Left+50;
                    //p[4].X = Actor.Margin.Left+50;
                    //p[5].X = Actor.Margin.Left+100;                    
                    //p[6].X = Actor.Margin.Left+100;
                    //p[7].X = Actor.Margin.Left+100;
                    //p[0].Y = Actor.Margin.Top;
                    //p[1].Y = Actor.Margin.Top+110;
                    //p[2].Y = Actor.Margin.Top+220;
                    //p[3].Y = Actor.Margin.Top ;
                    //p[4].Y = Actor.Margin.Top +220;
                    //p[5].Y = Actor.Margin.Top ;
                    //p[6].Y = Actor.Margin.Top + 110;
                    //p[7].Y = Actor.Margin.Top + 220;
                    //ArrowLine line = (ArrowLine)myCanvas.Children[0];
                    //Point PointMin = new Point();
                    //PointMin = MinimumDistance(new Point(line.X1, line.Y1), p);
                    //line.X2 = PointMin.X;
                    //line.Y2 = PointMin.Y;

                }
            }
            else
                Mouse.Capture(null);//освобождаем захват мыши
        }
        /// <summary>
        /// Обработчик события при клике на прецедент
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void myPrecedent_Move_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture((Precedent.myPrecedent)sender);
            InitMousePos.X = e.GetPosition(myCanvas).X;
            InitMousePos.Y = e.GetPosition(myCanvas).Y;
        }
        /// <summary>
        /// Обработчик события перемещения прецедента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                    //Point[] p = new Point[8];
                    //p[0].X = Precedent.Margin.Left;
                    //p[1].X = Precedent.Margin.Left + 37.5;
                    //p[2].X = Precedent.Margin.Left + 37.5;
                    //p[3].X = Precedent.Margin.Left + 75;
                    //p[4].X = Precedent.Margin.Left + 75;
                    //p[5].X = Precedent.Margin.Left + 112.5;
                    //p[6].X = Precedent.Margin.Left + 112.5;
                    //p[7].X = Precedent.Margin.Left + 150;
                    //p[0].Y = Precedent.Margin.Top + 37.5;
                    //p[1].Y = Precedent.Margin.Top + 5;
                    //p[2].Y = Precedent.Margin.Top + 70;
                    //p[3].Y = Precedent.Margin.Top;
                    //p[4].Y = Precedent.Margin.Top + 75;
                    //p[5].Y = Precedent.Margin.Top + 5;
                    //p[6].Y = Precedent.Margin.Top + 70;
                    //p[7].Y = Precedent.Margin.Top + 37.5;

                    //ArrowLine line = (ArrowLine)myCanvas.Children[0];
                    //Point PointMin = new Point();
                    //PointMin = MinimumDistance(new Point(line.X1, line.Y1), p);

                    //line.X2 = PointMin.X;
                    //line.Y2 = PointMin.Y;

                }
            }
            else
                Mouse.Capture(null);
        }
        /// <summary>
        /// Обработчик события "Добавление комментария"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnComment_Click(object sender, RoutedEventArgs e)
        {
            Comment.myComment comment = new Comment.myComment();
            comment.Margin = new Thickness(50, 170, 0, 0);
            comment.Text = "Text text text";
            myCanvas.Children.Add(comment);
            comment.MouseDown += new MouseButtonEventHandler(myComment_Move_MouseDown);
            comment.MouseMove += new MouseEventHandler(myComment_MouseMove);
        }
        /// <summary>
        /// Обработчик события при клике на комментарий
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void myComment_Move_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture((Comment.myComment)sender);
            InitMousePos.X = e.GetPosition(myCanvas).X;
            InitMousePos.Y = e.GetPosition(myCanvas).Y;
        }
        /// <summary>
        /// Обработчик события перемещения комментария
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    //Point[] p = new Point[8];
                    //p[0].X = Comment.Margin.Left;
                    //p[1].X = Comment.Margin.Left;
                    //p[2].X = Comment.Margin.Left;
                    //p[3].X = Comment.Margin.Left + 65;
                    //p[4].X = Comment.Margin.Left + 65;
                    //p[5].X = Comment.Margin.Left + 125;
                    //p[6].X = Comment.Margin.Left + 130;
                    //p[7].X = Comment.Margin.Left + 130;
                    //p[0].Y = Comment.Margin.Top;
                    //p[1].Y = Comment.Margin.Top + 50;
                    //p[2].Y = Comment.Margin.Top + 100;
                    //p[3].Y = Comment.Margin.Top;
                    //p[4].Y = Comment.Margin.Top + 100;
                    //p[5].Y = Comment.Margin.Top + 5;
                    //p[6].Y = Comment.Margin.Top + 50;
                    //p[7].Y = Comment.Margin.Top + 100;

                    //ArrowLine line = (ArrowLine)myCanvas.Children[0];
                    //Point PointMin = new Point();
                    //PointMin = MinimumDistance(new Point(line.X1, line.Y1), p);

                    //line.X2 = PointMin.X;
                    //line.Y2 = PointMin.Y;

                    //Canvas.SetZIndex(line, 10);//to front
                }
            }
            else
                Mouse.Capture(null);
        }
        /// <summary>
        /// Обработчик события "Добавление прецедента"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPrecedent_Click(object sender, RoutedEventArgs e)
        {

            Precedent.myPrecedent precedent = new Precedent.myPrecedent();
            precedent.Margin = new Thickness(10, 70, 0, 0);
            precedent.Text = "Text text text";
            myCanvas.Children.Add(precedent);
            precedent.MouseDown += new MouseButtonEventHandler(myPrecedent_Move_MouseDown);
            precedent.MouseMove += new MouseEventHandler(myPrecedent_MouseMove);
        }

        /// <summary>
        /// Обработчик события добавления связи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAssociation_Click(object sender, RoutedEventArgs e)
        {
            FlagArrow = true;
            //запоминаем тип
        }
        /// <summary>
        /// Обработчик события добавления связи Extend
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExtend_Click(object sender, RoutedEventArgs e)
        {
            FlagArrow = true;
            //запоминием тип
        }
        /// <summary>
        /// Обработчик события добавления связи Include
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnInclude_Click(object sender, RoutedEventArgs e)
        {
            FlagArrow = true;
            //запоминаем тип
        }
        /// <summary>
        /// Обработчик события нажатия кнопки "Изменение цвета"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnColor_Click(object sender, RoutedEventArgs e)
        {

            WPFColorPickerLib.ColorDialog colorDialog = new WPFColorPickerLib.ColorDialog();
            colorDialog.Owner = this;
            if ((bool)colorDialog.ShowDialog())
            {
                Precedent.myPrecedent precedent = (Precedent.myPrecedent)myCanvas.Children[0];//вот тут удали 
                precedent.Color = colorDialog.SelectedColor;
                //перебрать все прецеденты и изменить им цвет

            }
        }
//**************************************************************************


//*******************************МЕТОДЫ*************************************
        /// <summary>
        /// Метод добавляет связь определенного типа между 2-мя объектами
        /// </summary>
        /// <param name="First">Первый объект (из которого выходит стрелка)</param>
        /// <param name="Second">Второй объект (к которому подводится стрелка)</param>
        /// <param name="type">Тип связи</param>
        void AddLine(object First, object Second, int type)
        {
            if (First is Actor.myActor)
            {
                Actor.myActor actor = (Actor.myActor)First;
                StartPoint.X = actor.Margin.Left + 50;
                StartPoint.Y = actor.Margin.Top + 110;
            }
            if (Second is Actor.myActor)
            {
                Actor.myActor Actor = (Actor.myActor)Second;
                Point[] p = new Point[8];
                p[0].X = Actor.Margin.Left;
                p[1].X = Actor.Margin.Left;
                p[2].X = Actor.Margin.Left;
                p[3].X = Actor.Margin.Left + 50;
                p[4].X = Actor.Margin.Left + 50;
                p[5].X = Actor.Margin.Left + 100;
                p[6].X = Actor.Margin.Left + 100;
                p[7].X = Actor.Margin.Left + 100;
                p[0].Y = Actor.Margin.Top;
                p[1].Y = Actor.Margin.Top + 110;
                p[2].Y = Actor.Margin.Top + 220;
                p[3].Y = Actor.Margin.Top;
                p[4].Y = Actor.Margin.Top + 220;
                p[5].Y = Actor.Margin.Top;
                p[6].Y = Actor.Margin.Top + 110;
                p[7].Y = Actor.Margin.Top + 220;


                EndPoint = MinimumDistance(new Point(StartPoint.X, StartPoint.Y), p);

            }
            //добаится еще для прецедентов и комментариев
            //добавится тип

            ArrowLine aline = new ArrowLine();
            aline.Stroke = Brushes.Black;
            aline.StrokeThickness = 3;
            aline.StrokeDashArray.Add(8.0);
            aline.X1 = StartPoint.X;
            aline.Y1 = StartPoint.Y;
            aline.X2 = EndPoint.X;
            aline.Y2 = EndPoint.Y;

            AddMove(First);
            AddMove(Second);

            FirstObject = true;
            SecondObject = false;
            FlagArrow = false;


            myCanvas.Children.Add(aline);

        }
        /// <summary>
        /// Метод добавляет обработчик события MouseMove.
        /// </summary>
        /// <param name="Object">Объект, к которому добавляется обработчик события</param>
        private void AddMove(object Object)
        {
            if (Object is Actor.myActor)
            {
                Actor.myActor actor = (Actor.myActor)Object;
                actor.MouseMove += myActor_MouseMove;
            }
            if (Object is Precedent.myPrecedent)
            {
                Precedent.myPrecedent precedent = (Precedent.myPrecedent)Object;
                precedent.MouseMove += myPrecedent_MouseMove;
            }
            if (Object is Comment.myComment)
            {
                Comment.myComment comment = (Comment.myComment)Object;
                comment.MouseMove += myComment_MouseMove;
            }
        }
        /// <summary>
        /// Метод, ищущий минимальное расстояние мужду точкой StartPoint и точками из массива p
        /// </summary>
        /// <param name="StartPoint">Первая точка</param>
        /// <param name="p">Массив из 8-ми точек, лежащих по краям объека</param>
        /// <returns>Возвращает одну из 8-ми точек из массива р, до которой расстояние минимально</returns>
        private Point MinimumDistance(Point StartPoint, Point[] p)
        {
            Point PointMin = p[0];
            double MinDistance = Math.Sqrt(Math.Pow(StartPoint.X - p[0].X, 2) + Math.Pow(StartPoint.Y - p[0].Y, 2));
            for (int i = 1; i < p.Length; ++i)
            {
                double CerrentDistance = Math.Sqrt(Math.Pow(StartPoint.X - p[i].X, 2) + Math.Pow(StartPoint.Y - p[i].Y, 2));
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
