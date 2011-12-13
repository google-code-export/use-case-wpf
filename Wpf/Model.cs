using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Collections;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Wpf
{

    [DataContract]
    [Serializable]
    public class Relation
    {
        [DataMember]
        public int from { get; set; }
        [DataMember]
        public int to { get; set; }
        [DataMember]
        public string type { get; set; }

        public Relation(int from_in, int to_in, string type_in)
        {
            from = from_in;
            to = to_in;
            type = type_in;
        }
    }

    [DataContract]
    [Serializable]
    public class UmlObject
    {
        [DataMember]
        public int x { get; set; }
        [DataMember]
        public int y { get; set; }
        [DataMember]
        public string text { get; set; }
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public String color { get; set; }

        [DataMember]
        public double widht { get; set; }
        [DataMember]
        public double height { get; set; }
       
        public int selected { get; set; }

        public UmlObject()
        {

        }

        public UmlObject(int x_in, int y_in, string text_in, string type_in, int sel)
        {
            x = x_in;
            y = y_in;
            text = text_in;
            type = type_in;
            selected = sel;
            color = Colors.White.ToString();
        }

        public Color getColor()
        {
            return (Color)ColorConverter.ConvertFromString(color);
        }
        public void setColor(Color value)
        {
            color = value.ToString();
        }
    }

    [DataContract]
    [Serializable]
    public class Model
    {
        [DataMember]
        public double widght { get; set; }
        [DataMember]
        public double height { get; set; }
        [DataMember]
        public int max_id;
        [DataMember]
        public SortedList<int, UmlObject> objects_list { get; set; }
        [DataMember]
        public SortedList<int, Relation> relations_list { get; set; }

        // Конструктор
        public Model()
        {
            widght = 1200;
            height = 666;
            max_id = -1;
            objects_list = new SortedList<int, UmlObject>();
            relations_list = new SortedList<int, Relation>();
        }
        public Model(Model model)
        {
            max_id = model.max_id;
            objects_list = model.objects_list;
            relations_list = model.relations_list;
        }

        public object DeepClone(object obj)
        {
            if (obj == null) { return null; }
            object result = null;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, obj);
                ms.Position = 0;
                result = bf.Deserialize(ms);
            }
            return result;
        }




        /* 
         * Добовлет объект и возвращает id добавленного элемента
         * @x  - координата
         * @y - координата
         * @text - описание
         * @return - id добавленного объекта
        */
        public int add_object(int x, int y, string text, string type)
        {
            max_id++;
            UmlObject obj = new UmlObject(x, y, text, type, 0);
            objects_list.Add(max_id, obj);

            return max_id;
        }
        /* 
        * Изменить координату x у объекта
        * @x - координата
        * @id - изменяемый объект
       */
        public void edit_x_by_id(int id, int x)
        {
            //Uml_object obj = new Uml_object();
            objects_list[id].x = x;
        }
        /* 
        * Изменить координату Y у объекта
        * @x - координата
        * @id - изменяемый объект
        */
        public void edit_y_by_id(int id, int y)
        {
            objects_list[id].y = y;
        }
        /* 
        * Изменить описание объекта
        * @text - описание
        * @id  - изменяемый объект
        */
        public void edit_text_by_id(int id, string text)
        {

            objects_list[id].text = text;
        }
        /* 
        * Изменить цвет объекта
        * @color  - цвет
        * @id  - изменяемый объект
        */
        public void edit_color_by_id(int id, Color color)
        {
            objects_list[id].color = color.ToString();
        }

        public void edit_selected_by_id(int id, bool value)
        {
            if (value)
                objects_list[id].selected = 1;
            else
                objects_list[id].selected = 0;
        }
        public bool check_selected(int id)
        {
            if (objects_list[id].selected == 1)
                return true;
            else
                return false;
        }

        /* Вернуть id связей которые входят в объект
         * @id - объект
         * @return List<int> список айдишников которые входят в объект
         */
        public List<int> get_all_relations_id_in(int id)
        {
            List<int> ids_list = new List<int>();
            foreach (KeyValuePair<int, Relation> i in relations_list)
            {
                if (i.Value.to == id)
                {
                    ids_list.Add(i.Key);
                }
            }
            return ids_list;
        }
        /* Вернуть id связей которые выходят из объекта
        * @id - объект
        * @return List<int> список айдишников которые выходят из объекта
        */
        public List<int> get_all_relations_id_out(int id)
        {
            List<int> ids_list = new List<int>();
            foreach (KeyValuePair<int, Relation> i in relations_list)
            {
                if (i.Value.from == id)
                {
                    ids_list.Add(i.Key);
                }
            }
            return ids_list;
        }
        /* Удалить объект 
         * @id -объект
         */
        public void delete_object(int id_in) // Удаляет все связи с этим объектом
        {
            List<int> ids_list = new List<int>();
            objects_list.Remove(id_in);
            foreach (KeyValuePair<int, Relation> i in relations_list)
            {
                if (i.Value.to == id_in || i.Value.from == id_in)
                {
                    ids_list.Add(i.Key);
                }
            }
            foreach (int i in ids_list)
            {
                relations_list.Remove(i);
            }
        }
        /* Добавить связь
         * @from айди элемента откуда идет связь
         * @to айди элемента куда идет связь
         * @type тип связи
         * @return id айди добавленного элемента
        */
        public int add_relation(int from, int to, string type)
        {
            max_id++;
            Relation obj = new Relation(from, to, type);
            relations_list.Add(max_id, obj);
            return max_id;
        }
        /* Удалить связь 
         * @id -связь
         */
        public void delete_relation(int id_in)
        {
            relations_list.Remove(id_in);
        }

        public void edit_zoom_by_id(int id, double w, double h)
        {
            objects_list[id].widht = w;
            objects_list[id].height = h;
        }

        public void clear()
        {
            objects_list.Clear();
            relations_list.Clear();
            max_id = -1;
        }
        public void reset_flags()
        {
            foreach (KeyValuePair<int, UmlObject> i in objects_list)
            {
                i.Value.selected = 0;
            }
        }
        public void select_all_flags()
        {
            foreach (KeyValuePair<int, UmlObject> i in objects_list)
            {
                i.Value.selected = 1;
            }
        }
        public void clear_for_copy()
        {
            List<int> ids_list = new List<int>();
            foreach (KeyValuePair<int, UmlObject> i in objects_list)
            {
                if (i.Value.selected == 0)
                {
                    ids_list.Add(i.Key);
                }
            }
            foreach (int i in ids_list)
            {
                delete_object(i);
            }

        }
        /// <summary>
        /// 123456789765432
        /// </summary>
        /// <returns></returns>
        public List<int> get_selected_ids()
        {
            List<int> ids = new List<int>();
            foreach (KeyValuePair<int, UmlObject> i in objects_list)
            {
                if (i.Value.selected == 1)
                {
                    ids.Add(i.Key);
                }
            }
            return ids;
        }

        private SortedList<int, UmlObject> editUmlObjectsIds(int max)
        {
            SortedList<int, UmlObject> objects_list_buf = new SortedList<int, UmlObject>();
            foreach (KeyValuePair<int, UmlObject> i in objects_list)
            {
                UmlObject buf = new UmlObject(i.Value.x + 10, i.Value.y + 10, i.Value.text, i.Value.type, i.Value.selected);
                buf.widht = i.Value.widht;
                buf.height = i.Value.height;
                buf.color = i.Value.color;
                objects_list_buf.Add(i.Key + max, buf);

            }
            return objects_list_buf;
        }
        private SortedList<int, Relation> editRelationsIds(int max)
        {
            SortedList<int, Relation> relations_list_buf = new SortedList<int, Relation>();
            foreach (KeyValuePair<int, Relation> i in relations_list)
            {
                Relation buf = new Relation(i.Value.from + max, i.Value.to + max, i.Value.type);
                relations_list_buf.Add(i.Key + max, buf);
            }
            return relations_list_buf;
        }
        public void editIds(int max)
        {
            relations_list = editRelationsIds(max);
            objects_list = editUmlObjectsIds(max);
        }
        public void merge(Model obj)
        {

            foreach (KeyValuePair<int, UmlObject> i in obj.objects_list)
            {
                objects_list.Add(i.Key, i.Value);
            }
            foreach (KeyValuePair<int, Relation> i in obj.relations_list)
            {
                relations_list.Add(i.Key, i.Value);
            }
            max_id += obj.max_id + 1;
        }
    }
}
