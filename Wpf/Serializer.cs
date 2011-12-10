using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

namespace Wpf
{
    class Serializer
    {
        /*Сохранить объект модели в xml файл
         * @path путь к файлу
         * @msc объект модели
         */
        public void SaveData(string path, Model msc)
        {
            XmlTextWriter xw = new XmlTextWriter(path, Encoding.UTF8);
            xw.Formatting = Formatting.Indented;
            XmlDictionaryWriter writer = XmlDictionaryWriter.CreateDictionaryWriter(xw);
            DataContractSerializer ser = new DataContractSerializer(typeof(Model));
            ser.WriteObject(writer, msc);
            writer.Close();
            xw.Close();
        }
        /*Загрузить объект модели из xml файла
         * @path путь к файлу
         * @return объект модели
         */
        public Model LoadData(string path)
        {
            Model msc = null; using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, Encoding.UTF8, new XmlDictionaryReaderQuotas(), null);
                DataContractSerializer ser = new DataContractSerializer(typeof(Model));
                msc = (Model)ser.ReadObject(reader);
            }

            return msc;
        }
        public string Serialize(object objectToSerialize)
        {
            string serialString = null;
            using (System.IO.MemoryStream ms1 = new System.IO.MemoryStream())
            {
                BinaryFormatter b = new BinaryFormatter();
                b.Serialize(ms1, objectToSerialize);
                byte[] arrayByte = ms1.ToArray();
                serialString = Convert.ToBase64String(arrayByte);
            }
            return serialString;
        }
        public object DeSerialize(string serializationString)
        {
            object deserialObject = null;
            byte[] arrayByte = Convert.FromBase64String(serializationString);
            using (System.IO.MemoryStream ms1 = new System.IO.MemoryStream(arrayByte))
            {
                BinaryFormatter b = new BinaryFormatter();
                deserialObject = b.Deserialize(ms1);
            }
            return deserialObject;
        }
    }
}
