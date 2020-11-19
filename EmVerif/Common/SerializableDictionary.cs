using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace EmVerif.Common
{
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                return;
            }
            reader.Read();

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("KeyValuePair");
                reader.ReadStartElement("Key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();
                reader.ReadStartElement("Value");
                TValue val = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();
                reader.ReadEndElement();
                Add(key, val);
                reader.MoveToContent();
            }

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("KeyValuePair");
                writer.WriteStartElement("Key");
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();
                writer.WriteStartElement("Value");
                valueSerializer.Serialize(writer, this[key]);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }
    }
}
