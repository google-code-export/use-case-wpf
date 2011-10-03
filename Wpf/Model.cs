using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wpf
{
    class Model
    {
        //////////////////////
        private struct Relation
        {
            public int id;
            public int from;
            public int to;
            public string type;
            void relation(int from_in, int to_in, string type_in)
            {
                from = from_in;
                to = to_in;
                type = type_in;
            }
        }

        private struct  Uml_object
        {
            public int id;
            public int x;
            public int y;
            public string text;
            public string color = "";

            public void uml_object(int x_in, int y_in, string text_in)
            {
                x = x_in;
                y = y_in;
                text = text_in;
            }
        }

        private int max_id;
        private List <Uml_object> objects_list;
        private List <Relation> relations_list;

        ////////////////////////
        // Конструктор
        public void model()
        {
            max_id = 0;
            List<Uml_object> objects_list = new List <Uml_object>();
            List<Relation> relations_list = new List<Relation>();
        }
      
        public int add_object(int x_in, int y_in, string text_in);
        public void edit_x_by_id(int id_in, int x_in);
        public void edit_y_by_id(int id_in, int y_in);
        public void edit_text_by_id(int id_in, string text_in);
        public void edit_color_by_id(int id_in, int text_in);
        public List <int> get_all_relations_id (int id_in);

        public void delete_object(int id_in);
        public void add_relation(int from_in, int to_in, string type_in);
        public void delete_relation(int id_in);

        public void save(string filename);
        public void load(string filename);
    }
}
