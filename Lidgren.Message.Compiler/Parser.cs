using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Lidgren.Message.Compiler
{
    class Parser
    {
        public Protocol Parse(string packet_file)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(packet_file);

            Protocol protocol = new Protocol();
            XmlNode protocol_node = doc.SelectSingleNode("/Protocol");
            protocol.name = protocol_node.Attributes["name"].InnerText;
            protocol.number = int.Parse(protocol_node.Attributes["number"].InnerText);
            protocol.version = int.Parse(protocol_node.Attributes["version"].InnerText);


            /// Using
            XmlNodeList using_list = protocol_node.SelectNodes("Import");
            foreach (XmlNode node in using_list)
            {
                Import import = new Import();
                import.name = node.Attributes["name"].InnerText;

                protocol.import_list.Add(import);
            }


            /// FLAG
            XmlNodeList flag_list = protocol_node.SelectNodes("Flag");
            foreach (XmlNode node in flag_list)
            {
                Flag flag = new Flag();
                flag.name = node.Attributes["name"].InnerText;
                flag.value = int.Parse(node.Attributes["value"].InnerText);
                if (node.Attributes["desc"] != null)
                    flag.desc = node.Attributes["desc"].InnerText;
                else
                    flag.desc = "";

                protocol.flag_list.Add(flag);
            }

            /// Message
            XmlNodeList message_list = protocol_node.SelectNodes("Message");
            UInt32 last_id = 0;
            foreach (XmlNode node in message_list)
            {
                Message message = new Message();
                message.name = node.Attributes["name"].InnerText;
                if (node.Attributes["id"] == null)
                {
                    message.id = ++last_id;
                }
                else
                {
                    message.id = UInt32.Parse(node.Attributes["id"].InnerText);
                    last_id = message.id;
                }

                XmlNodeList data_list = node.SelectNodes("Data");
                foreach (XmlNode data_node in data_list)
                {
                    Data data = new Data();
                    data.type = data_node.Attributes["type"].InnerText;
                    data.name = data_node.Attributes["name"].InnerText;
                    if (data_node.Attributes["array"] != null)
                        data.array = int.Parse(data_node.Attributes["array"].InnerText);
                    else
                        data.array = 0;
                    if (data_node.Attributes["desc"] != null)
                        data.desc = data_node.Attributes["desc"].InnerText;
                    else
                        data.desc = "";

                    message.data_list.Add(data);
                }

                protocol.message_list.Add(message);
            }

            return protocol;
        }
    }
}
