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
        Point InitMousePosObject;   //Позиция мышки при клике относительно объекта
        bool FlagArrow = false;     //флаг, указывающий на попытку добавления связи между объектами (true - связь в данный момент добавляется, false - не добаляется)
        bool FirstObject = true;    //флаг, указывающий какой объект выбран для связи (true - значит первый)
        bool SecondObject = false;  //флаг, указывающий какой объект выбран для связи (true - значит второй)
        Point StartPoint;           //точка из которой выходит связь
        Point EndPoint;             //конечная точка
        object First;               //первый выбранный объект при добавлении связей
        object Second;
        bool isMove;
        Model bred;
        String TypeAssociation;
        //**************************************************************************

        public MainWindow()
        {
            bred = new Model();
            //Model bred1 = new Model();
            //int x1 = bred.add_object(1, 2, "");
            //int x2 = bred.add_object(1, 2, "");
            //bred.add_relation(x1, x2, "");
            //bred.delete_object(x1);
            //Serializer ololo = new Serializer();
            //ololo.SaveData("ololo.ololo", bred);
            //bred1 = ololo.LoadData("ololo.ololo");
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
            actor.Width = 100;
            actor.Height = 220;

            actor.Id = bred.add_object(50, 70, "Text text text","Actor");
            myCanvas.Children.Add(actor);
            actor.MouseDown += myActor_Move_MouseDown;
            actor.MouseMove += myActor_MouseMove;
            actor.MouseUp += Object_MouseUp;

        }
        /// <summary>
        /// Обработчик события при клике на актере
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void myActor_Move_MouseDown(object sender, MouseButtonEventArgs e)
        {
            

            Actor.myActor actor = (Actor.myActor)sender;
            if (this.Cursor != Cursors.Cross)
            {
                if (!FlagArrow)
                {
                                    
                    Mouse.Capture((Actor.myActor)sender);                       //захватываем мышь          
                    InitMousePos.X = e.GetPosition(myCanvas).X;
                    InitMousePos.Y = e.GetPosition(myCanvas).Y;

                    InitMousePosObject.X = e.GetPosition(actor).X;
                    InitMousePosObject.Y = e.GetPosition(actor).Y;
                    isMove = true;
                    return;
                }
                else
                //если происходит добавление связи
                {
                    //actor.MouseMove -= myActor_MouseMove;                       //удаляем обработчик события MouseMove
                    if (FirstObject)                                            //если это первый выбранный объект (от которого проводится стрелка)
                    {
                        FirstObject = false;
                        SecondObject = true;



                        //StartPoint.X = actor.Margin.Left + 50;                  //начальная точка - середина актера
                        //StartPoint.Y = actor.Margin.Top + 110;
                        First = sender;
                        return;
                    }
                    else if (SecondObject)                                      //если выбран второй объект
                    {

                        //EndPoint.X = actor.Margin.Left + 50;                    //конечная точка - середина актера
                        //EndPoint.Y = actor.Margin.Top + 110;
                        //if (StartPoint != EndPoint)                             //если выбраны разные объекты
                        if(First!=sender)
                        {
                            Second = sender;
                            //add association in DB
                                                      //добавляем связь
                            int id = AddLineInDB(First, Second,TypeAssociation);
                            AddLine(First, Second, TypeAssociation, id);
                        }
                    }
                }
            }
            else                                                                //Удаление актера
            {
                myCanvas.Children.Remove(actor);
                this.Cursor = Cursors.Arrow;
            }
        }
        /// <summary>
        /// Обработчик события перемещения актера 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void myActor_MouseMove(object sender, MouseEventArgs e)
        {
            if(isMove) 
            {
                var Actor = (Actor.myActor)sender;
                double mouseX = e.GetPosition(myCanvas).X, mouseY = e.GetPosition(myCanvas).Y;
                Point currentPoint = e.GetPosition(myCanvas);
                //если актер находится в пределах канваса и перемещение совершено на расстояние большее минимально предусмотренного
                if ((mouseX > InitMousePosObject.X) && (mouseX < myCanvas.ActualWidth - (Actor.Width - InitMousePosObject.X)) && (mouseY > InitMousePosObject.Y) && (mouseY < myCanvas.ActualHeight - (Actor.Height - InitMousePosObject.Y))
                    && Math.Abs(currentPoint.X - InitMousePos.X) > SystemParameters.MinimumHorizontalDragDistance
                    && Math.Abs(currentPoint.Y - InitMousePos.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    //меняем позицию актера
                    Actor.Margin = new Thickness(e.GetPosition(myCanvas).X - InitMousePosObject.X, e.GetPosition(myCanvas).Y - InitMousePosObject.Y, 0, 0);
                    
                    
                    //bred.edit_x_by_id(Actor.Id, Actor.Margin.Left);
                    //bred.edit_y_by_id(Actor.Id, Actor.Margin.Top);
                    //Переместить все связанные связи
                    List<int> ListIdIn =  bred.get_all_relations_id_in(Actor.Id);
                    List<int> ListIdOut = bred.get_all_relations_id_out(Actor.Id);

                    for (int i = 0; i < myCanvas.Children.Count; ++i)
                    {
                        if (myCanvas.Children[i] is ArrowLine)
                        {
                            ArrowLine aline = (ArrowLine)myCanvas.Children[i];

                            if (ListIdIn.Contains(Convert.ToInt32(aline.Uid)))
                            {
                                int id = Convert.ToInt32(aline.Uid);
                                myCanvas.Children.Remove(aline);
                                if (bred.relations_list[id].type != "Generalization")
                                    myCanvas.Children.Remove((TextBox)getChildrenById(id-65000));
                                AddLine(getChildrenById(bred.relations_list[id].from), getChildrenById(bred.relations_list[id].to), bred.relations_list[id].type, id);   
                            }
                            if (ListIdOut.Contains(Convert.ToInt32(aline.Uid)))
                            {
                                int id = Convert.ToInt32(aline.Uid);
                                myCanvas.Children.Remove(aline);
                                if (bred.relations_list[id].type != "Generalization")
                                    myCanvas.Children.Remove((TextBox)getChildrenById(id - 65000));
                                AddLine(getChildrenById(bred.relations_list[id].from), getChildrenById(bred.relations_list[id].to), bred.relations_list[id].type, id);

                            }
                        }
                        if (myCanvas.Children[i] is Line)
                        {
                            Line line = (Line)myCanvas.Children[i];
                            if (ListIdIn.Contains(Convert.ToInt32(line.Uid)))
                            {
                                int id = Convert.ToInt32(line.Uid);
                                myCanvas.Children.Remove(line);
                                AddLine(getChildrenById(bred.relations_list[id].from), getChildrenById(bred.relations_list[id].to), bred.relations_list[id].type, id);

                            }
                            if (ListIdOut.Contains(Convert.ToInt32(line.Uid)))
                            {
                                int id = Convert.ToInt32(line.Uid);
                                myCanvas.Children.Remove(line);
                                AddLine(getChildrenById(bred.relations_list[id].from), getChildrenById(bred.relations_list[id].to), bred.relations_list[id].type, id);

                            }
                        }
                    }


                }
            }
        }
        /// <summary>
        /// Обработчик события отпускания кнопки мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Object_MouseUp(object sender, MouseEventArgs e)
        {
            if (isMove)
            {
                isMove = false;
                Mouse.Capture(null);//освобождаем захват мыши
            }
        }

        /// <summary>
        /// Обработчик события при клике на прецедент
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void myPrecedent_Move_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Precedent.myPrecedent precedent = (Precedent.myPrecedent)sender;
            if (this.Cursor != Cursors.Cross)
            {
                Mouse.Capture((Precedent.myPrecedent)sender);
                InitMousePos.X = e.GetPosition(myCanvas).X;
                InitMousePos.Y = e.GetPosition(myCanvas).Y;

                InitMousePosObject.X = e.GetPosition(precedent).X;
                InitMousePosObject.Y = e.GetPosition(precedent).Y;
                isMove = true;

            }
            else
            {
                myCanvas.Children.Remove(precedent);
                this.Cursor = Cursors.Arrow;
            }
        }
        /// <summary>
        /// Обработчик события перемещения прецедента
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void myPrecedent_MouseMove(object sender, MouseEventArgs e)
        {

            if (isMove)
            {
                var Precedent = (Precedent.myPrecedent)sender;
                double mouseX = e.GetPosition(myCanvas).X, mouseY = e.GetPosition(myCanvas).Y;
                Point currentPoint = e.GetPosition(myCanvas);
                if ((mouseX > InitMousePosObject.X) && (mouseX < myCanvas.ActualWidth - (Precedent.Width - InitMousePosObject.X)) && (mouseY > InitMousePosObject.Y) && (mouseY < myCanvas.ActualHeight - (Precedent.Height - InitMousePosObject.Y))
                    && Math.Abs(currentPoint.X - InitMousePos.X) > SystemParameters.MinimumHorizontalDragDistance
                    && Math.Abs(currentPoint.Y - InitMousePos.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
       
                    Precedent.Margin = new Thickness(e.GetPosition(myCanvas).X - InitMousePosObject.X, e.GetPosition(myCanvas).Y - InitMousePosObject.Y, 0, 0);
                }
            }
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
            comment.Width = 130;
            comment.Height = 100;
            comment.Text = "Text text text";
            comment.Id = bred.add_object(50, 170, "Text text text","Comment");
            myCanvas.Children.Add(comment);
            comment.MouseDown += myComment_Move_MouseDown;
            comment.MouseMove += myComment_MouseMove;
            comment.MouseUp += Object_MouseUp;
        }
        /// <summary>
        /// Обработчик события при клике на комментарий
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void myComment_Move_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Comment.myComment comment = (Comment.myComment)sender;
            if (this.Cursor != Cursors.Cross)
            {
                Mouse.Capture((Comment.myComment)sender);
                InitMousePos.X = e.GetPosition(myCanvas).X;
                InitMousePos.Y = e.GetPosition(myCanvas).Y;

                InitMousePosObject.X = e.GetPosition(comment).X;
                InitMousePosObject.Y = e.GetPosition(comment).Y;
                isMove = true;
                return;
            }
            else
            {
                myCanvas.Children.Remove(comment);
                this.Cursor = Cursors.Arrow;
            }
        }
        /// <summary>
        /// Обработчик события перемещения комментария
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void myComment_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMove)
            {
                var Comment = (Comment.myComment)sender;
                double mouseX = e.GetPosition(myCanvas).X, mouseY = e.GetPosition(myCanvas).Y;
                Point currentPoint = e.GetPosition(myCanvas);
                if ((mouseX > InitMousePosObject.X) && (mouseX < myCanvas.ActualWidth - (Comment.Width - InitMousePosObject.X)) && (mouseY > InitMousePosObject.Y) && (mouseY < myCanvas.ActualHeight - (Comment.Height - InitMousePosObject.Y))
                    && Math.Abs(currentPoint.X - InitMousePos.X) > SystemParameters.MinimumHorizontalDragDistance
                    && Math.Abs(currentPoint.Y - InitMousePos.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    
                    Comment.Margin = new Thickness(e.GetPosition(myCanvas).X - InitMousePosObject.X, e.GetPosition(myCanvas).Y - InitMousePosObject.Y, 0, 0);
                }
            }
        }
        /// <summary>
        /// Обработчик события при клике на связь
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void myAline_Move_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ArrowLine aline = (ArrowLine)sender;
            if (this.Cursor == Cursors.Cross)
            {
                myCanvas.Children.Remove(aline);
                //+ удаление label

                this.Cursor = Cursors.Arrow;
            }
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
            precedent.Width = 150;
            precedent.Height = 75;
            precedent.Text = "Text text text";
            precedent.Id = bred.add_object(10, 70, "Text text text","Precedent");
            myCanvas.Children.Add(precedent);
            precedent.MouseDown += myPrecedent_Move_MouseDown;
            precedent.MouseMove += myPrecedent_MouseMove;
            precedent.MouseUp += Object_MouseUp;
        }

        /// <summary>
        /// Обработчик события добавления связи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAssociation_Click(object sender, RoutedEventArgs e)
        {
            FlagArrow = true;
            TypeAssociation = "Association";
            //запоминаем тип
        }
        /// <summary>
        /// Обобщение
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnGeneralization_Click(object sender, RoutedEventArgs e)
        {
            FlagArrow = true;
            TypeAssociation = "Generalization";
        }
        /// <summary>
        /// Обработчик события добавления связи Extend
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExtend_Click(object sender, RoutedEventArgs e)
        {
            FlagArrow = true;
            TypeAssociation = "Extend";
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
            TypeAssociation = "Include";
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
        /// <summary>
        /// Обработчик события нажатия кнопки "Удаление объекта"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Cross;
        }
        /// <summary>
        /// Обработчик события нажатия клавиш. Служит для "отмены удаления"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Cursor = Cursors.Arrow;
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
        void AddLine(object First, object Second, String type, int id)
        {
            Point[] PStart = new Point[8];
            Point[] PEnd = new Point[8];

            if (First is Actor.myActor)
            {
                Actor.myActor Actor = (Actor.myActor)First;
                PStart = GetMassPointActor(Actor);
            }
            if (Second is Actor.myActor)
            {
                Actor.myActor Actor = (Actor.myActor)Second;
                PEnd = GetMassPointActor(Actor);
                //EndPoint = MinimumDistance(new Point(StartPoint.X, StartPoint.Y), p);
            }
            if (First is Precedent.myPrecedent)
            {
                Precedent.myPrecedent Precedent = (Precedent.myPrecedent)First;
                PStart = GetMassPointPrecedent(Precedent);
            }
            if (Second is Precedent.myPrecedent)
            {
                Precedent.myPrecedent Precedent = (Precedent.myPrecedent)Second;
                PEnd = GetMassPointPrecedent(Precedent);
            }
            if (First is Comment.myComment)
            {
                Comment.myComment Comment = (Comment.myComment)First;
                PStart = GetMassPointComment(Comment);
            }
            if (Second is Comment.myComment)
            {
                Comment.myComment Comment = (Comment.myComment)Second;
                PEnd = GetMassPointComment(Comment);
            }
            //min 
            MinimumDistance(PStart, PEnd); 

            //добаится еще для прецедентов и комментариев
            //добавится тип
            // 
            TextBox TextBox  = new TextBox();
            ArrowLine aline = new ArrowLine();
            TextBox.BorderThickness = new Thickness(0);
            TextBox.IsReadOnly = true;
            aline.Stroke = Brushes.Black;
            aline.StrokeThickness = 3;
            if (type == "Association")
            {
                Line line =new Line();
                line.Stroke = Brushes.Black;
                line.StrokeThickness = 3;
                line.X1 = StartPoint.X;
                line.Y1 = StartPoint.Y;
                line.X2 = EndPoint.X;
                line.Y2 = EndPoint.Y;
                FirstObject = true;
                SecondObject = false;
                FlagArrow = false;
  
                line.Uid = id.ToString();
                line.MouseDown += myAline_Move_MouseDown;
                myCanvas.Children.Add(line);
                return;
            }
            if (type == "Generalization")
            {

            }
            if (type == "Extend")
            {
                TextBox.Margin = new Thickness(Math.Abs(StartPoint.X + EndPoint.X) / 2, Math.Abs(StartPoint.Y + EndPoint.Y) / 2, 0, 0);
                TextBox.Text = "<<Extend>>";
                TextBox.Uid = (id-65000).ToString();
                aline.StrokeDashArray.Add(8.0);
                TextBox.IsEnabled = false;
                myCanvas.Children.Add(TextBox);
            }
            if (type == "Include")
            {
                TextBox.Margin = new Thickness(Math.Abs(StartPoint.X + EndPoint.X) / 2, Math.Abs(StartPoint.Y + EndPoint.Y) / 2, 0, 0);
                TextBox.Text = "<<Include>>";
                TextBox.Uid = (id - 65000).ToString();
                aline.StrokeDashArray.Add(8.0);
                TextBox.IsEnabled = false;
                myCanvas.Children.Add(TextBox);
            }
            aline.X1 = StartPoint.X;
            aline.Y1 = StartPoint.Y;
            aline.X2 = EndPoint.X;
            aline.Y2 = EndPoint.Y;
            aline.Uid = id.ToString();
            
            
            //AddMove(First);
            //AddMove(Second);

            FirstObject = true;
            SecondObject = false;
            FlagArrow = false;
            aline.MouseDown += myAline_Move_MouseDown;
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
        /// <returns></returns>
        private void MinimumDistance(Point[] StartPoints, Point[] EndPoints)
        {
            StartPoint = StartPoints[0];
            EndPoint = EndPoints[0];
            double MinDistance = Math.Sqrt(Math.Pow(StartPoints[0].X - EndPoints[0].X, 2) + Math.Pow(StartPoints[0].Y - EndPoints[0].Y, 2));
            for (int i = 0; i < StartPoints.Length; ++i)
            {
                for (int j = 0; j < EndPoints.Length; ++j)
                {
                    double CerrentDistance = Math.Sqrt(Math.Pow(StartPoints[i].X - EndPoints[j].X, 2) + Math.Pow(StartPoints[i].Y - EndPoints[j].Y, 2));
                    if (MinDistance > CerrentDistance)
                    {
                        StartPoint = StartPoints[i];
                        EndPoint = EndPoints[j];
                        MinDistance = CerrentDistance;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Object"></param>
        /// <returns></returns>
        private Point[] GetMassPointActor(object Object)
        {
            Point[] p = new Point[8];
            Actor.myActor Actor = (Actor.myActor)Object;
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
            return p;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Object"></param>
        /// <returns></returns>
        private Point[] GetMassPointPrecedent(object Object)
        {
            Point[] p = new Point[8];
            Precedent.myPrecedent Precedent = (Precedent.myPrecedent)Object;
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
            return p;
        }
        private Point[] GetMassPointComment(object Object)
        {
            Point[] p = new Point[8];
            Comment.myComment Comment = (Comment.myComment)Object;
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
            return p;
        }
        private int AddLineInDB(object First, object Second, string type)
        {
            int id_from = 0;
            int id_to = 0;
            if (First is Actor.myActor)
            {
                id_from = ((Actor.myActor)First).Id;
            }
            if (Second is Actor.myActor)
            {
                id_to = ((Actor.myActor)Second).Id;
            }

            if (First is Precedent.myPrecedent)
            {
                id_from = ((Precedent.myPrecedent)First).Id;
            }
            if (Second is Precedent.myPrecedent)
            {
                id_to = ((Precedent.myPrecedent)Second).Id;
            }

            if (First is Comment.myComment)
            {
                id_from = ((Comment.myComment)First).Id;
            }
            if (Second is Comment.myComment)
            {
                id_to = ((Comment.myComment)Second).Id;
            }
            return bred.add_relation(id_from, id_to, type);
        }
        private object getChildrenById(int id)
        {
            for (int i = 0; i < myCanvas.Children.Count; ++i)
            {
                if (myCanvas.Children[i] is Actor.myActor)
                {
                    Actor.myActor actor = (Actor.myActor)myCanvas.Children[i];
                    if (actor.Id == id)
                    {
                        return (object)actor;
                    }
                }
                if (myCanvas.Children[i] is Precedent.myPrecedent)
                {
                    Precedent.myPrecedent precedent = (Precedent.myPrecedent)myCanvas.Children[i];
                    if (precedent.Id == id)
                    {
                        return (object)precedent;
                    }
                }
                if (myCanvas.Children[i] is Comment.myComment)
                {
                    Comment.myComment comment = (Comment.myComment)myCanvas.Children[i];
                    if (comment.Id == id)
                    {
                        return (object)comment;
                    }
                }
                if (myCanvas.Children[i] is ArrowLine)
                {
                    ArrowLine aline = (ArrowLine)myCanvas.Children[i];
                    if (aline.Uid == id.ToString())
                    {
                        return (object)aline;
                    }
                }
                if (myCanvas.Children[i] is Line)
                {
                    Line line = (Line)myCanvas.Children[i];
                    if (line.Uid == id.ToString())
                    {
                        return (object)line;
                    }
                }
                if (myCanvas.Children[i] is TextBox)
                {
                    TextBox TextBox = (TextBox)myCanvas.Children[i];
                    if (TextBox.Uid == id.ToString())
                    {
                        return (object)TextBox;
                    }
                }
            }
            object o = new object();
            return o;
        }
    }
}
