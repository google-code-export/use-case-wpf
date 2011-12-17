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
using Microsoft.Win32;
using System.IO;
using System.Globalization;
using System.Threading;

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
        Model dataObject;
        String TypeAssociation;
        Serializer dataSaver;
        bool changeColor = false;
        Color currentColor;
        List<int> SelectedObjcet;
        int currentId = 0;
        int maxId = 0;
        bool ZoomFlag = false;
        bool AltFlag = false;
        bool CtrlFlag = false;
        bool ShiftFlag = false;
        bool SelectingZone = false;
        Point StartZonePoint;
        Rectangle selectedZone = new Rectangle();
        //**************************************************************************

        public MainWindow()
        {
            
            dataObject = new Model();
            dataSaver = new Serializer();
            InitializeComponent();
            Directory.CreateDirectory("c:/log");
            dataSaver.SaveData("c:/log/0", dataObject);
        }
        //***************************Обработчики событий*****************************
        /// <summary>
        /// Обработчик события нажатия кнопки "Добавление актера"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnActor_Click(object sender, RoutedEventArgs e)
        {
            int id = dataObject.add_object(50, 170, "Text", "Actor");
            addActorToCanvas(50, 170, id, "Text");
            IncStatus();
        }


        private void addActorToCanvas(int left, int top, int iden, string text)
        {
            Actor.myActor actor = new Actor.myActor();

 
            Canvas.SetLeft(actor, left);
            Canvas.SetTop(actor, top);

            actor.Text = text;
            actor.Width = 75;
            actor.Height = 165;
            actor.Id = iden;


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

            if (AltFlag)
            {
                int id = dataObject.add_object(Convert.ToInt32(e.GetPosition(myCanvas).X - e.GetPosition(actor).X), Convert.ToInt32(e.GetPosition(myCanvas).Y - e.GetPosition(actor).Y), actor.Text, "Actor");
                addActorToCanvas(Convert.ToInt32(e.GetPosition(myCanvas).X - e.GetPosition(actor).X), Convert.ToInt32(e.GetPosition(myCanvas).Y - e.GetPosition(actor).Y), id, actor.Text);
                IncStatus();

            }


            SelectObject(actor);

            if (!FlagArrow)
            {
                Mouse.Capture(actor);                       //захватываем мышь          
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

                if (FirstObject)                                            //если это первый выбранный объект (от которого проводится стрелка)
                {

                    FirstObject = false;
                    SecondObject = true;
                    First = sender;
                    return;
                }
                else if (SecondObject)                                      //если выбран второй объект
                {
                    if (First != sender)
                    {
                        SecondObject = false;
                        Second = sender;

                        int id = AddLineInDB(First, Second, TypeAssociation);
                        AddLine(First, Second, TypeAssociation, id);
                        IncStatus();
                    }
                }
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
                    Canvas.SetLeft(Actor, e.GetPosition(myCanvas).X - InitMousePosObject.X);
                    Canvas.SetTop(Actor, e.GetPosition(myCanvas).Y - InitMousePosObject.Y);

                    //Actor.Margin = new Thickness(e.GetPosition(myCanvas).X - InitMousePosObject.X, e.GetPosition(myCanvas).Y - InitMousePosObject.Y, 0, 0);
                    //Переместить все связанные связи
                    MoveRelation(Actor.Id, true);
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
            //Если флаг = true, то сохраняем все выделенные элементы в массив 
            //Флаг = false

            int id = 0;
            int x = 0;
            int y = 0;


            if (isMove)
            {
                isMove = false;
                Mouse.Capture(null);//освобождаем захват мыши

                if (sender is Actor.myActor)
                {
                    Actor.myActor obj = (Actor.myActor)sender;
                    x = (int)Canvas.GetLeft(obj);
                    y = (int)Canvas.GetTop(obj);
                    id = obj.Id;
                }
                else if (sender is Precedent.myPrecedent)
                {
                    Precedent.myPrecedent obj = (Precedent.myPrecedent)sender;
                    x = (int)Canvas.GetLeft(obj);
                    y = (int)Canvas.GetTop(obj);
                    id = obj.Id;
                }
                else if (sender is Comment.myComment)
                {
                    if (ZoomFlag)
                    {
                        ZoomFlag = false;
                        Mouse.Capture(null);//освобождаем захват мыши
                        
                    }
                    Comment.myComment obj = (Comment.myComment)sender;
                    x = (int)Canvas.GetLeft(obj);
                    y = (int)Canvas.GetTop(obj);
                    id = obj.Id;
                }
                dataObject.edit_x_by_id(id, x);
                dataObject.edit_y_by_id(id, y);
                IncStatus();
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
            if (AltFlag)
            {
                int id = dataObject.add_object(Convert.ToInt32(e.GetPosition(myCanvas).X - e.GetPosition(precedent).X), Convert.ToInt32(e.GetPosition(myCanvas).Y - e.GetPosition(precedent).Y), precedent.Text, "Precedent");
                dataObject.edit_color_by_id(id,precedent.Color);
                addPrecedentToCanvas(Convert.ToInt32(e.GetPosition(myCanvas).X - e.GetPosition(precedent).X), Convert.ToInt32(e.GetPosition(myCanvas).Y - e.GetPosition(precedent).Y), id, precedent.Text,precedent.Color);
                IncStatus();

            }
            SelectObject(precedent);//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                if (changeColor == true)
                {
                    precedent.Color = currentColor;
                    dataObject.edit_color_by_id(precedent.Id, precedent.Color);
                    changeColor = false;
                    IncStatus();
                }
                if (!FlagArrow)
                {
                    Mouse.Capture(precedent);                       //захватываем мышь          
                    InitMousePos.X = e.GetPosition(myCanvas).X;
                    InitMousePos.Y = e.GetPosition(myCanvas).Y;

                    InitMousePosObject.X = e.GetPosition(precedent).X;
                    InitMousePosObject.Y = e.GetPosition(precedent).Y;
                    isMove = true;
                    return;
                }
                else
                //если происходит добавление связи
                {
                   if (FirstObject)                                            //если это первый выбранный объект (от которого проводится стрелка)
                {

                    FirstObject = false;
                    SecondObject = true;
                    First = sender;
                    return;
                }
                else if (SecondObject)                                      //если выбран второй объект
                {
                    if (First != sender)
                    {
                        SecondObject = false;
                        Second = sender;

                        int id = AddLineInDB(First, Second, TypeAssociation);
                        AddLine(First, Second, TypeAssociation, id);
                        IncStatus();
                    }
                }
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
       
                    //Precedent.Margin = new Thickness(e.GetPosition(myCanvas).X - InitMousePosObject.X, e.GetPosition(myCanvas).Y - InitMousePosObject.Y, 0, 0);
                    Canvas.SetLeft(Precedent, e.GetPosition(myCanvas).X - InitMousePosObject.X);
                    Canvas.SetTop(Precedent, e.GetPosition(myCanvas).Y - InitMousePosObject.Y);

                    MoveRelation(Precedent.Id, true);
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
            int id = dataObject.add_object(50, 170, "Text","Comment");
            addCommentToCanvas(50, 170, id, "Text", 140, 110);
            IncStatus();
        }
        private void addCommentToCanvas(int left, int top, int iden, string text, double width, double height)
        {

            Comment.myComment comment = new Comment.myComment();
            //comment.Margin = new Thickness(left, top, 0, 0);
            Canvas.SetLeft(comment, left);
            Canvas.SetTop(comment, top);
            comment.Text = text;
            comment.Id = iden;
            comment.W = width;
            comment.H = height;
            myCanvas.Children.Add(comment);
            comment.MouseDown += myComment_Move_MouseDown;
            comment.MouseMove += myComment_MouseMove;
            comment.MouseUp += Object_MouseUp;
            comment.Ellip.MouseDown += commentZoomClick;
            comment.MouseEnter += commentHelp;
            comment.MouseLeave += commentCloseHelp;

        }
        /// <summary>
        /// Удаление подсказки комментария
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void commentCloseHelp(object sender, MouseEventArgs e)
        {
            Comment.myComment comment = (Comment.myComment)sender;
            comment.ToolTip = null;

        }
        /// <summary>
        /// Подсказка комментария
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        void commentHelp(object sender, MouseEventArgs e)
        {
            Comment.myComment comment = (Comment.myComment)sender;
            
            String text = comment.Text;
            Typeface myTypeface = new Typeface("Helvetica");
            FormattedText ft = new FormattedText(text, CultureInfo.CurrentCulture, 
            FlowDirection.LeftToRight, myTypeface, 16, Brushes.Red);
            
            if (comment.TextBox.Width*comment.TextBox.Height < ft.Width*11)
            {

               
                ToolTip tp = new System.Windows.Controls.ToolTip();
                string str = "";
                tp.Content = text;
                for (int i = 0; i < text.Length; i+=20)
                {
                    if (text.Length >= i + 20)
                    {
                        str += text.Substring(i, 20);
                        str += System.Environment.NewLine;
                    }
                    else str += text.Substring(i);
                }
                    //tp.Width = 150;
                    //tp.Content = "First line" + System.Environment.NewLine + "Second line";
                
                comment.ToolTip = str;
                
                
            }
        }
        void commentZoomClick(object sender, MouseButtonEventArgs e)
        {
         
            ZoomFlag = true;
        }
        /// <summary>
        /// Обработчик события при клике на комментарий
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void myComment_Move_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Если зажат ctrl или shift
                //Если объект выделен, то снять выделение
                //Если объект не выделен, то выделить его
                
            // Иначе очищаем выделение и выделяем объект

            Comment.myComment comment = (Comment.myComment)sender;
            if (AltFlag)
            {
                int id = dataObject.add_object(Convert.ToInt32(e.GetPosition(myCanvas).X - e.GetPosition(comment).X), Convert.ToInt32(e.GetPosition(myCanvas).Y - e.GetPosition(comment).Y), comment.Text, "Comment");
                addCommentToCanvas(Convert.ToInt32(e.GetPosition(myCanvas).X - e.GetPosition(comment).X), Convert.ToInt32(e.GetPosition(myCanvas).Y - e.GetPosition(comment).Y), id, comment.Text, comment.W, comment.H);
                IncStatus();

            }
            SelectObject(comment);

                if (!ZoomFlag)
                {
                    if (!FlagArrow)
                    {
                        Mouse.Capture(comment);                       //захватываем мышь          
                        InitMousePos.X = e.GetPosition(myCanvas).X;
                        InitMousePos.Y = e.GetPosition(myCanvas).Y;
                        InitMousePosObject.X = e.GetPosition(comment).X;
                        InitMousePosObject.Y = e.GetPosition(comment).Y;
                        isMove = true;
                        return;
                    }
                    else
                    //если происходит добавление связи
                    {
                        if (FirstObject)                                            //если это первый выбранный объект (от которого проводится стрелка)
                        {

                            FirstObject = false;
                            SecondObject = true;
                            First = sender;
                            return;
                        }
                        else if (SecondObject)                                      //если выбран второй объект
                        {
                            if (First != sender)
                            {
                                SecondObject = false;
                                Second = sender;

                                int id = AddLineInDB(First, Second, TypeAssociation);
                                AddLine(First, Second, TypeAssociation, id);
                                IncStatus();
                            }
                        }
                    }
                }
                else
                    Mouse.Capture(comment);  

        }
        /// <summary>
        /// Обработчик события перемещения комментария
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void myComment_MouseMove(object sender, MouseEventArgs e)
        {
            var Comment = (Comment.myComment)sender;
            if (ZoomFlag)
            {
                Comment.W = e.GetPosition(Comment).X+10;
                Comment.H = e.GetPosition(Comment).Y+10;
                dataObject.edit_zoom_by_id(Comment.Id, Comment.W, Comment.H);
                MoveRelation(Comment.Id, true);
               
            }
            else
            {
                if (isMove)
                {
                    
                    double mouseX = e.GetPosition(myCanvas).X, mouseY = e.GetPosition(myCanvas).Y;
                    Point currentPoint = e.GetPosition(myCanvas);
                    if ((mouseX > InitMousePosObject.X) && (mouseX < myCanvas.ActualWidth - (Comment.Width - InitMousePosObject.X)) && (mouseY > InitMousePosObject.Y) && (mouseY < myCanvas.ActualHeight - (Comment.Height - InitMousePosObject.Y))
                        && Math.Abs(currentPoint.X - InitMousePos.X) > SystemParameters.MinimumHorizontalDragDistance
                        && Math.Abs(currentPoint.Y - InitMousePos.Y) > SystemParameters.MinimumVerticalDragDistance)
                    {
                        Canvas.SetLeft(Comment, e.GetPosition(myCanvas).X - InitMousePosObject.X);
                        Canvas.SetTop(Comment, e.GetPosition(myCanvas).Y - InitMousePosObject.Y);
                        MoveRelation(Comment.Id, true);
                    }
                }
            }
        }
        /// <summary>
        /// Обработчик события при клике на связь ср стрелкой
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void myAline_Move_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ArrowLine aline = (ArrowLine)sender;
            if (this.Cursor == Cursors.Cross)
            {
                int id = Convert.ToInt32(aline.Uid);
                myCanvas.Children.Remove(aline);
                if (dataObject.relations_list[id].type == "Include" || dataObject.relations_list[id].type == "Extend")
                    myCanvas.Children.Remove((Label)getChildrenById(id));
                dataObject.delete_relation(id);
                this.Cursor = Cursors.Arrow;
            }
        }
        /// <summary>
        /// Обработчик события при клике на Association
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void myline_Move_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Line line = (Line)sender;
            if (this.Cursor == Cursors.Cross)
            {
                int id = Convert.ToInt32(line.Uid);
                myCanvas.Children.Remove(line);
                if (dataObject.relations_list[id].type == "Include" || dataObject.relations_list[id].type == "Extend")
                    myCanvas.Children.Remove((Label)getChildrenById(id));
                dataObject.delete_relation(id);
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
            int id = dataObject.add_object(10, 70, "Text", "Precedent");
            Color color = new Color();
            color.R = 0;
            color.G = 0;
            color.B = 0;
            dataObject.edit_color_by_id(id, color);
            addPrecedentToCanvas(10, 70, id, "Text", color);
            IncStatus();
        }
        private void addPrecedentToCanvas(int left, int top, int iden, string text, Color color)
        {
            Precedent.myPrecedent precedent = new Precedent.myPrecedent();
            precedent.Id = iden;
            Canvas.SetLeft(precedent, left);
            Canvas.SetTop(precedent, top);

            precedent.Width = 150;
            precedent.Height = 80;
            precedent.Text = text;
            myCanvas.Children.Add(precedent);
            precedent.MouseDown += myPrecedent_Move_MouseDown;
            precedent.MouseMove += myPrecedent_MouseMove;
            precedent.MouseUp += Object_MouseUp;
            precedent.Color = color;
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
                currentColor = colorDialog.SelectedColor;
                changeColor = true;
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
                this.Cursor = Cursors.Arrow;
            if (e.Key == Key.System)
                AltFlag = true;
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                CtrlFlag = true;
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                ShiftFlag = true;
            if (e.Key == Key.Delete)
            {
                List<int> allSelected = dataObject.get_selected_ids();
                foreach (int i in allSelected)
                {
                    MoveRelation(i, false);
                    dataObject.delete_object(i);
                    myCanvas.Children.Remove((UIElement)getChildrenById(i));
                }
                IncStatus();
            }
            if (CtrlFlag && e.Key == Key.C)
            {
                Model copy = new Model();
                copy = (Model)dataObject.DeepClone(dataObject);
                
                copy.clear_for_copy();
                Clipboard.SetData("CustomerFormat", copy);
                copy.clear();
            }
            if (CtrlFlag && e.Key == Key.X)
            {
                Model copy = new Model();
                copy = (Model)dataObject.DeepClone(dataObject);
                copy.clear_for_copy();
                Clipboard.SetData("CustomerFormat", copy);
                copy.clear();

                List<int> allSelected = dataObject.get_selected_ids();
                foreach (int i in allSelected)
                {
                    MoveRelation(i, false);
                    dataObject.delete_object(i);
                    myCanvas.Children.Remove((UIElement)getChildrenById(i));
                }
                IncStatus();

            }
            if (CtrlFlag && e.Key == Key.V)
            {
                Model CopyModel = new Model();
                if (Clipboard.ContainsData("CustomerFormat"))
                {
                    CopyModel = Clipboard.GetData("CustomerFormat") as Model;
                    CopyModel.editIds(dataObject.max_id+1);
                    
                    ResetAllSelected();
                    dataObject.reset_flags();
                    dataObject.merge(CopyModel);
                    ShowInCanvas(CopyModel);
                    List <int> newSelectedIds = dataObject.get_selected_ids();
                    foreach (int i in newSelectedIds)
                    {
                        SelectObject(getChildrenById(i));
                    }
                    IncStatus();
                }
            }
            if (CtrlFlag && e.Key == Key.A)
            {
                for (int i = 0; i < myCanvas.Children.Count; ++i)
                {
                    SelectObject(myCanvas.Children[i]);
                }
                dataObject.select_all_flags();
            }
            if (CtrlFlag && e.Key == Key.Z)
            {
                Undo();
            }
            if (CtrlFlag && e.Key == Key.Y)
            {
                Redo();
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            Directory.Delete("c:/log", true);
        }
        /// <summary>
        /// Отжате кнопок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System)
                AltFlag = false;
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                CtrlFlag = false;
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                ShiftFlag = false;
        }

        /// <summary>
        /// Сохранение в .png
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSaveImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "Uml image (.png)|*.png"; // Filter files by extension
            bool? result = dlg.ShowDialog();
            if (result.Value)
                ExportToPng(dlg.FileName, myCanvas);
        }
        /// <summary>
        /// Zoom in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            myCanvas.Width += 100;
            myCanvas.Height += 100;
            dataObject.widght = myCanvas.Width;
            dataObject.height = myCanvas.Height;
        }
        /// <summary>
        /// Zoom out
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (myCanvas.Height > 666)
            {
                myCanvas.Width -= 100;
                myCanvas.Height -= 100;
                dataObject.widght = myCanvas.Width;
                dataObject.widght = myCanvas.Height;
            }
        }
        /// <summary>
        /// Нажатие кнопки мыши на канвасе
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void myCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
                if (!(CtrlFlag || ShiftFlag || isMove))
                {
                    ResetAllSelected();
                    dataObject.reset_flags();
                }
            if (!isMove&&!ZoomFlag)
            {
                // Если не зажат Ctrl или Shift то снять выделение
                // Начальные координаты прямоугольника + флаг рисования области

                StartZonePoint = e.GetPosition(myCanvas);
                Mouse.Capture(myCanvas);
                SelectingZone = true;

                selectedZone = new Rectangle
                {
                    Opacity = 0.2,
                    Fill = Brushes.LightBlue,
                    Stroke = Brushes.LightBlue,
                    StrokeThickness = 2
                };
                Canvas.SetLeft(selectedZone, StartZonePoint.X);
                Canvas.SetTop(selectedZone, StartZonePoint.X);
                myCanvas.Children.Add(selectedZone);
            }
            
        }

        private void ClearBlyadoLines()
        {
            List<UIElement> forDel = new List<UIElement>();
            foreach (UIElement i in myCanvas.Children)
            {
                if (i.Uid == "-1")
                {
                    forDel.Add(i);
                }
            }
            for (int i = 0; i < forDel.Count; i++)
            {
                myCanvas.Children.Remove((UIElement)getChildrenById(-1));
                
            }
        }
        /// <summary>
        /// Перемещение мышки по канвасу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void myCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            ClearBlyadoLines();
            if (SecondObject)
            {

                    Point[] PEnd = new Point[4];

                    PEnd[0].X = e.GetPosition(myCanvas).X - 5;
                    PEnd[0].Y = e.GetPosition(myCanvas).Y ;
                    PEnd[1].X = e.GetPosition(myCanvas).X + 5;
                    PEnd[1].Y = e.GetPosition(myCanvas).Y;
                    PEnd[2].X = e.GetPosition(myCanvas).X;
                    PEnd[2].Y = e.GetPosition(myCanvas).Y - 5;
                    PEnd[3].X = e.GetPosition(myCanvas).X ;
                    PEnd[3].Y = e.GetPosition(myCanvas).Y + 5 ;

                    MinimumDistance(GetPointStart(First), PEnd);
                    DrawLine(TypeAssociation, -1, true);
               
 
            }
            if (SelectingZone)
            {
                if (e.LeftButton == MouseButtonState.Released || selectedZone == null)
                    return;

                var pos = e.GetPosition(myCanvas);

                var x = Math.Min(pos.X, StartZonePoint.X);
                var y = Math.Min(pos.Y, StartZonePoint.Y);

                var w = Math.Max(pos.X, StartZonePoint.X) - x;
                var h = Math.Max(pos.Y, StartZonePoint.Y) - y;

                selectedZone.Width = w;
                selectedZone.Height = h;

                Canvas.SetLeft(selectedZone, x);
                Canvas.SetTop(selectedZone, y);
            }
        }
        /// <summary>
        /// Отжатие кнопки мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void myCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Если флаг = true, то сохраняем все выделенные элементы в массив 
            //Флаг = false
            if (ZoomFlag)
            {
                ZoomFlag = false;
                IncStatus();
            }
            if (SelectingZone)
            {
                
                for (int i = 0; i < myCanvas.Children.Count; i++)
                {
                    Rect rect = new Rect();
                    rect.X = Canvas.GetLeft(myCanvas.Children[i]);
                    rect.Y = Canvas.GetTop(myCanvas.Children[i]);
                    rect.Width = myCanvas.Children[i].RenderSize.Width;
                    rect.Height = myCanvas.Children[i].RenderSize.Height;
                    Quadrilateral redQuad = new Quadrilateral(rect);
                    rect.X = Canvas.GetLeft(selectedZone);
                    rect.Y = Canvas.GetTop(selectedZone);
                    rect.Width = selectedZone.Width;
                    rect.Height = selectedZone.Height;
                    Quadrilateral greenQuad = new Quadrilateral(rect);
                    if (IntersectionTest.CheckRectRectIntersection(redQuad, greenQuad) && rect.Width > 0)
                    {
                        SelectObject(myCanvas.Children[i]);
                    }
                }
                SelectingZone = false;
                myCanvas.Children.Remove(selectedZone);
                
                
            }
            Mouse.Capture(null);
        }



        /// <summary>
        /// Undo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {

            Undo();
        }

        private void Undo()
        {
            if (currentId > 0)
            {
                //BtnRedo.IsEnabled = true;
                ImageBrush myBrush = new ImageBrush();
                string packUri = "pack://application:,,,/Wpf;component/Images/BtnRedo.png";
                myBrush.ImageSource = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                BtnRedo.Background = myBrush;

                currentId--;

                myCanvas.Children.Clear();
                Model model = new Model();
                model = (Model)model.DeepClone(dataSaver.LoadData("c:/log/" + currentId.ToString()));
                dataObject = model;
                ShowInCanvas(dataObject);
                if (currentId == 0)
                {
                    myBrush = new ImageBrush();
                    packUri = "pack://application:,,,/Wpf;component/Images/BtnUndoNotActive.png";
                    myBrush.ImageSource = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                    BtnUndo.Background = myBrush;
                }
            }
            else
            {
                ImageBrush myBrush = new ImageBrush();
                string packUri = "pack://application:,,,/Wpf;component/Images/BtnUndoNotActive.png";
                myBrush.ImageSource = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                BtnUndo.Background = myBrush;
            }
        }
        /// <summary>
        /// Redo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnRedo_Click(object sender, RoutedEventArgs e)
        {
            Redo();
        }

        private void Redo()
        {
            if (currentId < maxId)
            {
                ImageBrush myBrush1 = new ImageBrush();
                string packUri1 = "pack://application:,,,/Wpf;component/Images/BtnUndo.png";
                myBrush1.ImageSource = new ImageSourceConverter().ConvertFromString(packUri1) as ImageSource;
                BtnUndo.Background = myBrush1;

                currentId++;

                myCanvas.Children.Clear();
                ShowInCanvas(dataSaver.LoadData("c:/log/" + currentId.ToString()));
                if (currentId == maxId)
                {
                    ImageBrush myBrush3 = new ImageBrush();
                    string packUri3 = "pack://application:,,,/Wpf;component/Images/BtnRedoNotActive.png";
                    myBrush3.ImageSource = new ImageSourceConverter().ConvertFromString(packUri3) as ImageSource;
                    BtnRedo.Background = myBrush3;
                }
            }
            else
            {
                ImageBrush myBrush = new ImageBrush();
                string packUri = "pack://application:,,,/Wpf;component/Images/BtnRedoNotActive.png";
                myBrush.ImageSource = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
                BtnRedo.Background = myBrush;
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


            Point[] PStart = GetPointStart(First);

            Point[] PEnd = new Point[8];
            if (Second is Actor.myActor)
            {
                Actor.myActor Actor = (Actor.myActor)Second;
                PEnd = GetMassPointActor(Actor);
            }

            if (Second is Precedent.myPrecedent)
            {
                Precedent.myPrecedent Precedent = (Precedent.myPrecedent)Second;
                PEnd = GetMassPointPrecedent(Precedent);
            }

            if (Second is Comment.myComment)
            {
                Comment.myComment Comment = (Comment.myComment)Second;
                PEnd = GetMassPointComment(Comment);
            }
         
            MinimumDistance(PStart, PEnd);

            DrawLine(type, id,false);
      
            
        }

        private Point[] GetPointStart(object First)
        {
            Point[] PStart = new Point[8];
            if (First is Actor.myActor)
            {
                Actor.myActor Actor = (Actor.myActor)First;
                PStart = GetMassPointActor(Actor);
            }
            if (First is Precedent.myPrecedent)
            {
                Precedent.myPrecedent Precedent = (Precedent.myPrecedent)First;
                PStart = GetMassPointPrecedent(Precedent);
            }
            if (First is Comment.myComment)
            {
                Comment.myComment Comment = (Comment.myComment)First;
                PStart = GetMassPointComment(Comment);
            }
            return PStart;
        }

        private void DrawLine(String type, int id, bool animation)
        {
            Label Label = new Label();
            ArrowLine aline = new ArrowLine();
            Label.BorderThickness = new Thickness(0);
            aline.Stroke = Brushes.Black;
            aline.StrokeThickness = 2;
            if (type == "Association")
            {
                Line line = new Line();
                line.Stroke = Brushes.Black;
                line.StrokeThickness = 2;
                line.X1 = StartPoint.X;
                line.Y1 = StartPoint.Y;
                line.X2 = EndPoint.X;
                line.Y2 = EndPoint.Y;
                line.Uid = id.ToString();
                if (!animation)
                {
                    FirstObject = true;
                    SecondObject = false;
                    FlagArrow = false;
                    
                    line.MouseDown += myline_Move_MouseDown;
                }
                
                myCanvas.Children.Add(line);
                return;
            }
            if (type == "Generalization")
            {
            }
            if (type == "Extend")
            {
                Label.Margin = new Thickness(Math.Abs(StartPoint.X + EndPoint.X) / 2, Math.Abs(StartPoint.Y + EndPoint.Y) / 2, 0, 0);
                Label.Content = "<<Extend>>";
                Label.Uid = id.ToString();
                aline.StrokeDashArray.Add(12);
                Label.IsEnabled = false;
                myCanvas.Children.Add(Label);
            }
            if (type == "Include")
            {
                Label.Margin = new Thickness(Math.Abs(StartPoint.X + EndPoint.X) / 2, Math.Abs(StartPoint.Y + EndPoint.Y) / 2, 0, 0);
                Label.Content = "<<Include>>";
                Label.Uid = id.ToString();
                aline.StrokeDashArray.Add(12);
                Label.IsEnabled = false;
                myCanvas.Children.Add(Label);

            }
            aline.X1 = StartPoint.X;
            aline.Y1 = StartPoint.Y;
            aline.X2 = EndPoint.X;
            aline.Y2 = EndPoint.Y;
            aline.Uid = id.ToString();
            if (!animation)
            {
                
                FirstObject = true;
                SecondObject = false;
                FlagArrow = false;
                aline.MouseDown += myAline_Move_MouseDown;
            }
            myCanvas.Children.Add(aline);
        }
        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            myCanvas.Children.Clear();
            dataObject.clear();
            Directory.Delete("c:/log/", true);
            Directory.CreateDirectory("c:/log/");
            dataSaver.SaveData("c:/log/0", dataObject);
            maxId = 0;
            currentId = 0;
            myCanvas.Width = 1200;
            myCanvas.Height = 666;
            dataObject.widght = 1200;
            dataObject.height = 666;


        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "Uml documents (.usd)|*.usd"; // Filter files by extension
            if (dlg.ShowDialog().Value)
            {
                SaveText();
                dataSaver.SaveData(dlg.FileName, dataObject);
            }
        }

        private void SaveText()
        {
            for (int i = 0; i < myCanvas.Children.Count; ++i)
            {
                if (myCanvas.Children[i] is Actor.myActor)
                {
                    dataObject.edit_text_by_id(((Actor.myActor)myCanvas.Children[i]).Id, ((Actor.myActor)myCanvas.Children[i]).Text);
                }
                if (myCanvas.Children[i] is Precedent.myPrecedent)
                {
                    dataObject.edit_text_by_id(((Precedent.myPrecedent)myCanvas.Children[i]).Id, ((Precedent.myPrecedent)myCanvas.Children[i]).Text);
                }
                if (myCanvas.Children[i] is Comment.myComment)
                {
                    dataObject.edit_text_by_id(((Comment.myComment)myCanvas.Children[i]).Id, ((Comment.myComment)myCanvas.Children[i]).Text);
                }
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Text documents (.usd)|*.usd"; // Filter files by extension
            if (dlg.ShowDialog().Value)
            {
                int index = dlg.FileName.LastIndexOf(".");
                string str = dlg.FileName.Remove(0, index+1);
                if (str == "usd")
                {
                    File.Copy(dlg.FileName, "c:/log/0", true);
                    currentId = 0;
                    maxId = 0;

                    myCanvas.Children.Clear();
                    dataObject = dataSaver.LoadData(dlg.FileName);

                    ShowInCanvas(dataObject);
                }
                else
                {
                    MessageBox.Show("Неверное расширение!");
                }
            }

        }

        private void ShowInCanvas(Model model)
        {
            SortedList<int, UmlObject> objects_list = model.objects_list;
            SortedList<int, Relation> relations_list = model.relations_list;
            myCanvas.Width = model.widght;
            myCanvas.Height = model.height;

            foreach (KeyValuePair<int, UmlObject> i in objects_list)
            {
                if (i.Value.type == "Actor")
                {
                    addActorToCanvas(i.Value.x, i.Value.y, i.Key, i.Value.text);
                    int x = 0;
                    x++;
                }
                else if (i.Value.type == "Precedent")
                {
                    addPrecedentToCanvas(i.Value.x, i.Value.y, i.Key, i.Value.text, i.Value.getColor());
                }
                else if (i.Value.type == "Comment")
                {
                    addCommentToCanvas(i.Value.x, i.Value.y, i.Key, i.Value.text, i.Value.widht, i.Value.height);
                }
            }
            foreach (KeyValuePair<int, Relation> i in relations_list)
            {
                AddLine(getChildrenById(i.Value.from), getChildrenById(i.Value.to), i.Value.type, i.Key);
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
            var p = new Point[4];
            var actor = (Actor.myActor)Object;
            p[0].X = Canvas.GetLeft(actor) + 37.5;
            p[1].X = Canvas.GetLeft(actor) + 5;
            p[2].X = Canvas.GetLeft(actor) + 70;
            p[3].X = Canvas.GetLeft(actor) + 37.5;
            p[0].Y = Canvas.GetTop(actor);
            p[1].Y = Canvas.GetTop(actor) + 32;
            p[2].Y = Canvas.GetTop(actor) + 32;
            p[3].Y = Canvas.GetTop(actor) + 100;

            return p;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Object"></param>
        /// <returns></returns>
        private Point[] GetMassPointPrecedent(object Object)
        {
            var p = new Point[8];
            var precedent = (Precedent.myPrecedent)Object;
            p[0].X = Canvas.GetLeft(precedent);
            p[1].X = Canvas.GetLeft(precedent) + 37.5;
            p[2].X = Canvas.GetLeft(precedent) + 37.5;
            p[3].X = Canvas.GetLeft(precedent) + 75;
            p[4].X = Canvas.GetLeft(precedent) + 75;
            p[5].X = Canvas.GetLeft(precedent) + 112.5;
            p[6].X = Canvas.GetLeft(precedent) + 112.5;
            p[7].X = Canvas.GetLeft(precedent) + 150;
            p[0].Y = Canvas.GetTop(precedent) + 40;
            p[1].Y = Canvas.GetTop(precedent) + 8;
            p[2].Y = Canvas.GetTop(precedent) + 74;
            p[3].Y = Canvas.GetTop(precedent);
            p[4].Y = Canvas.GetTop(precedent) + 80;
            p[5].Y = Canvas.GetTop(precedent) + 8;
            p[6].Y = Canvas.GetTop(precedent) + 74;
            p[7].Y = Canvas.GetTop(precedent) + 40;
            return p;
        }
        private Point[] GetMassPointComment(object Object)
        {
            var p = new Point[8];
            var comment = (Comment.myComment)Object;
            p[0].X = Canvas.GetLeft(comment) + 10;
            p[1].X = Canvas.GetLeft(comment) + 10;
            p[2].X = Canvas.GetLeft(comment) + 10;
            p[3].X = Canvas.GetLeft(comment) + comment.W / 2;
            p[4].X = Canvas.GetLeft(comment) + comment.W / 2;
            p[5].X = Canvas.GetLeft(comment) + comment.W - 20;
            p[6].X = Canvas.GetLeft(comment) + comment.W - 10;
            p[7].X = Canvas.GetLeft(comment) + comment.W - 10;
            p[0].Y = Canvas.GetTop(comment) + 10;
            p[1].Y = Canvas.GetTop(comment) + comment.H / 2 - 10;
            p[2].Y = Canvas.GetTop(comment) + comment.H - 10;
            p[3].Y = Canvas.GetTop(comment) + 10;
            p[4].Y = Canvas.GetTop(comment) + comment.H - 10;
            p[5].Y = Canvas.GetTop(comment) + 20;
            p[6].Y = Canvas.GetTop(comment) + comment.H / 2 - 10;
            p[7].Y = Canvas.GetTop(comment) + comment.H - 10;
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
            return dataObject.add_relation(id_from, id_to, type);
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
                if (myCanvas.Children[i] is Label)
                {
                    Label Label = (Label)myCanvas.Children[i];
                    if (Label.Uid == id.ToString())
                    {
                        return (object)Label;
                    }
                }
            }
            object o = new object();
            return o;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="IdObject"></param>
        /// <param name="MoveRemove">Если true - перемещение, false - удаление</param>
        private void MoveRelation(int IdObject, bool MoveRemove)
        {
            List<int> ListIdIn = dataObject.get_all_relations_id_in(IdObject);
            List<int> ListIdOut = dataObject.get_all_relations_id_out(IdObject);

            List<int> ListRemoveId = new List<int>();   //список удаляемых объектов с указанными id

            for (int i = 0; i < myCanvas.Children.Count; ++i)
            {
                if (myCanvas.Children[i] is ArrowLine)
                {
                    ArrowLine aline = (ArrowLine)myCanvas.Children[i];

                    if (ListIdIn.Contains(Convert.ToInt32(aline.Uid)))
                    {
                        int id = Convert.ToInt32(aline.Uid);
                        if (MoveRemove)//перемещение
                        {
                            myCanvas.Children.Remove(aline);
                            if (dataObject.relations_list[id].type == "Include" || dataObject.relations_list[id].type == "Extend")
                                myCanvas.Children.Remove((Label)getChildrenById(id));
                            AddLine(getChildrenById(dataObject.relations_list[id].from), getChildrenById(dataObject.relations_list[id].to), dataObject.relations_list[id].type, id);
                        }
                        else            //удаление
                        {
                            myCanvas.Children.Remove(aline);
                            if (dataObject.relations_list[id].type == "Include" || dataObject.relations_list[id].type == "Extend")
                                myCanvas.Children.Remove((Label)getChildrenById(id));
                            dataObject.delete_relation(id);
                            i = 0;
                        }
                    }
                    if (ListIdOut.Contains(Convert.ToInt32(aline.Uid)))
                    {
                        int id = Convert.ToInt32(aline.Uid);
                        if (MoveRemove)//перемещение
                        {
                            myCanvas.Children.Remove(aline);
                            if (dataObject.relations_list[id].type == "Include" || dataObject.relations_list[id].type == "Extend")
                                myCanvas.Children.Remove((Label)getChildrenById(id));
                            AddLine(getChildrenById(dataObject.relations_list[id].from), getChildrenById(dataObject.relations_list[id].to), dataObject.relations_list[id].type, id);
                        }
                        else            //удаление
                        {
                            myCanvas.Children.Remove(aline);
                            if (dataObject.relations_list[id].type == "Include" || dataObject.relations_list[id].type == "Extend")
                                myCanvas.Children.Remove((Label)getChildrenById(id));
                            dataObject.delete_relation(id);
                            i = 0;
                        }
                    }
                }
                if (myCanvas.Children[i] is Line)
                {
                    Line line = (Line)myCanvas.Children[i];
                    if (ListIdIn.Contains(Convert.ToInt32(line.Uid)))
                    {
                        int id = Convert.ToInt32(line.Uid);
                        if (MoveRemove)//перемещение
                        {
                            myCanvas.Children.Remove(line);
                            if (dataObject.relations_list[id].type == "Include" || dataObject.relations_list[id].type == "Extend")
                                myCanvas.Children.Remove((Label)getChildrenById(id));
                            AddLine(getChildrenById(dataObject.relations_list[id].from), getChildrenById(dataObject.relations_list[id].to), dataObject.relations_list[id].type, id);
                        }
                        else            //удаление
                        {
                            myCanvas.Children.Remove(line);
                            if (dataObject.relations_list[id].type == "Include" || dataObject.relations_list[id].type == "Extend")
                                myCanvas.Children.Remove((Label)getChildrenById(id));
                            dataObject.delete_relation(id);
                            i = 0;
                        }

                    }
                    if (ListIdOut.Contains(Convert.ToInt32(line.Uid)))
                    {
                        int id = Convert.ToInt32(line.Uid);
                        if (MoveRemove)//перемещение
                        {
                            myCanvas.Children.Remove(line);
                            if (dataObject.relations_list[id].type == "Include" || dataObject.relations_list[id].type == "Extend")
                                myCanvas.Children.Remove((Label)getChildrenById(id));
                            AddLine(getChildrenById(dataObject.relations_list[id].from), getChildrenById(dataObject.relations_list[id].to), dataObject.relations_list[id].type, id);
                        }
                        else            //удаление
                        {
                            myCanvas.Children.Remove(line);
                            if (dataObject.relations_list[id].type == "Include" || dataObject.relations_list[id].type == "Extend")
                                myCanvas.Children.Remove((Label)getChildrenById(id));
                            dataObject.delete_relation(id);
                            i = 0;
                        }
                    }
                }
            }
        }



        public void ExportToPng(String path, Canvas canvas)
        {
            
            if (path == null) return;
            // Save current canvas transform
            Transform transform = canvas.LayoutTransform;
            Thickness oldMargin = canvas.Margin;
            // reset current transform (in case it is scaled or rotated)
            canvas.LayoutTransform = null;
            canvas.Margin = new Thickness(0);
            // Get the size of canvas
            double w = canvas.Width.CompareTo(double.NaN) == 0 ? canvas.ActualWidth : canvas.Width;
            double h = canvas.Height.CompareTo(double.NaN) == 0 ? canvas.ActualHeight : canvas.Height;
            Size size = new Size(w, h);
            // Measure and arrange the canvas
            canvas.Measure(size);
            canvas.Arrange(new Rect(size));
            // Create a render bitmap and push the canvas to it
            RenderTargetBitmap renderBitmap =
              new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(canvas);
            // Create a file stream for saving image
            using (FileStream outStream = new FileStream(path, FileMode.Create))
            {
                // Use png encoder for our data
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                // push the rendered bitmap to it
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                // save the data to the stream
                encoder.Save(outStream);
            }
            // Restore previously saved layout
            canvas.Margin = oldMargin;
            canvas.LayoutTransform = transform;
        }

        public void SelectObject(object obj)
        {       
       
            if (obj is Actor.myActor)
            {
                Actor.myActor actor = (Actor.myActor)obj;
                if (actor.Visibility == Visibility.Visible && (CtrlFlag||ShiftFlag))
                {
                    actor.Visibility = Visibility.Hidden;
                    dataObject.edit_selected_by_id(actor.Id, false);
                }
                else 
                {
                    if (!(CtrlFlag || ShiftFlag|| SelectingZone ))
                    {
                        ResetAllSelected();
                        dataObject.reset_flags();
                    }
                    actor.Visibility = Visibility.Visible;
                    dataObject.edit_selected_by_id(actor.Id, true);
                }
            }
            if (obj is Precedent.myPrecedent)
            {
                Precedent.myPrecedent precedent = (Precedent.myPrecedent)obj;
                if (precedent.Visibility == Visibility.Visible && (CtrlFlag || ShiftFlag))
                {
                    precedent.Visibility = Visibility.Hidden;
                    dataObject.edit_selected_by_id(precedent.Id, false);
                }
                else
                {
                    if (!(CtrlFlag || ShiftFlag || SelectingZone))
                    {
                        ResetAllSelected();
                        dataObject.reset_flags();
                    }
                    precedent.Visibility = Visibility.Visible;
                    dataObject.edit_selected_by_id(precedent.Id, true);
                }
            }
            if (obj is Comment.myComment)
            {
                Comment.myComment comment = (Comment.myComment)obj;
                if (comment.Visibility == Visibility.Visible && (CtrlFlag || ShiftFlag))
                {
                    comment.Visibility = Visibility.Hidden;
                    dataObject.edit_selected_by_id(comment.Id, false);
                }
                else
                {
                    if (!(CtrlFlag || ShiftFlag || SelectingZone))
                    {
                        ResetAllSelected();
                        dataObject.reset_flags();
                    }
                    comment.Visibility = Visibility.Visible;
                    dataObject.edit_selected_by_id(comment.Id, true);
                }
            }
        }

        private void ResetAllSelected()
        {
            List<int> ids = dataObject.get_selected_ids();
            foreach (var i in ids)
            {
                object objToReset = getChildrenById(i);

                if (objToReset is Actor.myActor)
                {
                    Actor.myActor a;
                    a = (Actor.myActor)objToReset;
                    a.Visibility = Visibility.Hidden;
                }
                if (objToReset is Precedent.myPrecedent)
                {
                    Precedent.myPrecedent p;
                    p = (Precedent.myPrecedent)objToReset;
                    p.Visibility = Visibility.Hidden;
                }
                if (objToReset is Comment.myComment)
                {
                    Comment.myComment c;
                    c = (Comment.myComment)objToReset;
                    c.Visibility = Visibility.Hidden;
                }
            }
        }
        private void IncStatus()
        {
            ImageBrush myBrush = new ImageBrush();
            string packUri = "pack://application:,,,/Wpf;component/Images/BtnUndo.png";
            myBrush.ImageSource = new ImageSourceConverter().ConvertFromString(packUri) as ImageSource;
            BtnUndo.Background = myBrush;

            ImageBrush myBrush2 = new ImageBrush();
            string packUri2 = "pack://application:,,,/Wpf;component/Images/BtnRedoNotActive.png";
            myBrush2.ImageSource = new ImageSourceConverter().ConvertFromString(packUri2) as ImageSource;
            BtnRedo.Background = myBrush2;

            //BtnUndo.IsEnabled = true;
            //BtnRedo.IsEnabled = false;
            currentId++;
          
            maxId = currentId;
            SaveText();
            dataSaver.SaveData("c:/log/" + currentId.ToString(), dataObject);
        }

        private void BtnQuestion_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(this, "Горячие клавиши: \n\n ctrl-z шаг назад\n ctrl-y шаг вперед\n ctrl-a выделить все \n ctrl-x вырезать\n ctrl-с копировать  \n ctrl-v вставаить\n del - удалить выделенные элементы \n\n Разработчики:\n\n Буртовой А. \n Кузнецов М. \n Салем Салех Али ");

        }

    }
}
