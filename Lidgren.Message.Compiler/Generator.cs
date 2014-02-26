using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Lidgren.Message.Compiler
{
    class Generator
    {
        public void Generate(Protocol protocol,
            string output_path)
        {
            GenerateMessageCode(protocol, output_path);
            GenerateProxyCode(protocol, output_path);
            GenerateStubCode(protocol, output_path);
        }


        private void GenerateMessageCode(Protocol protocol, string path)
        {
            string output_file = String.Format("{0}/{1}.Message.cs",
                path, protocol.name);
            using (StreamWriter sw = new StreamWriter(output_file))
            {
                foreach (Import import in protocol.import_list)
                {
                    sw.WriteLine("using {0};", import.name);
                }

                sw.WriteLine("namespace {0}", protocol.name);
                sw.WriteLine("{");
                sw.WriteLine("\tpublic class Message");
                sw.WriteLine("\t{");

                sw.WriteLine("\t\tpublic const int Version = {0};",
                    protocol.version);
                sw.WriteLine();

                /// FLAG
                sw.WriteLine("\t\tpublic enum Flag");
                sw.WriteLine("\t\t{");
                foreach (Flag flag in protocol.flag_list)
                {
                    sw.WriteLine("\t\t\tkFlag{0} = {1},", flag.name, flag.value);
                }
                sw.WriteLine("\t\t};");
                sw.WriteLine("");

                foreach (Message message in protocol.message_list)
                {
                    sw.WriteLine("\t\tpublic struct {0}", message.name);
                    sw.WriteLine("\t\t{");
                    foreach (Data data in message.data_list)
                    {
                        if (data.array > 0)
                            sw.WriteLine("\t\t\tpublic List<{0}> {1};\t{2}",
                                data.type, data.name,
                                data.desc == string.Empty ? "" : "//" + data.desc,
                                data.array);
                        else
                            sw.WriteLine("\t\t\tpublic {0} {1};\t{2}", data.type,
                                data.name,
                                data.desc == string.Empty ? "" : "//" + data.desc);
                    }
                    sw.WriteLine("\t\t}");
                }

                sw.WriteLine("\t}");
                sw.WriteLine("}");
            }
        }


        private void GenerateProxyCode(Protocol protocol, string path)
        {
            string output_file = String.Format("{0}/{1}.Proxy.cs",
                path, protocol.name);
            using (StreamWriter sw = new StreamWriter(output_file))
            {
                foreach (Import import in protocol.import_list)
                {
                    sw.WriteLine("using {0};", import.name);
                }

                sw.WriteLine("namespace {0}", protocol.name);
                sw.WriteLine("{");
                sw.WriteLine("\tpublic class Proxy : MessageProxy");
                sw.WriteLine("\t{");

                sw.WriteLine("\t\tpublic const int Version = {0};",
                    protocol.version);
                sw.WriteLine();

                foreach (Message message in protocol.message_list)
                {
                    string param_list = "NetConnection connection, ";
                    foreach (Data data in message.data_list)
                    {
                        if (data.array > 0)
                        {
                            param_list += string.Format("List<{0}> {1}, ",
                                data.type, data.name);
                        }
                        else
                        {
                            param_list += string.Format("{0} {1}, ",
                                data.type, data.name);
                        }
                    }
                    if (param_list.Length > 2)
                    {
                        param_list = param_list.Substring(0, param_list.Length - 2);
                    }


                    sw.WriteLine("\t\tpublic bool {0}({1})", message.name, param_list);
                    sw.WriteLine("\t\t{");

                    sw.WriteLine("\t\t\tif (peer == null || connection == null) return false;");
                    sw.WriteLine("\t\t\tif (peer.ConnectionsCount < 1 ||");
                    sw.WriteLine("\t\t\t\tconnection.Status != NetConnectionStatus.Connected)");
                    sw.WriteLine("\t\t\t\treturn false;");

                    sw.WriteLine("\t\t\tNetOutgoingMessage om = peer.CreateMessage();");
                    sw.WriteLine("\t\t\tom.Write((UInt32){0});", message.id);

                    foreach (Data data in message.data_list)
                    {
                        if (data.array > 0)
                        {
                            sw.WriteLine("\t\t\tom.Write({0}.Count);", data.name);
                            sw.WriteLine("\t\t\tforeach (var v in {0})", data.name);
                            sw.WriteLine("\t\t\t{");
                            sw.WriteLine("\t\t\t\tom.Write(v);");
                            sw.WriteLine("\t\t\t}");

                        }
                        else
                        {
                            sw.WriteLine("\t\t\tom.Write({0});", data.name);
                        }
                    }

                    sw.WriteLine("\t\t\tNetSendResult result = peer.SendMessage(om, connection,");
                    sw.WriteLine("\t\t\t\tNetDeliveryMethod.ReliableOrdered);");
                    sw.WriteLine("\t\t\tif (result == NetSendResult.FailedNotConnected ||");
                    sw.WriteLine("\t\t\t\tresult == NetSendResult.Dropped)");
                    sw.WriteLine("\t\t\t\treturn false;");
                    sw.WriteLine();
                    sw.WriteLine("\t\t\tpeer.FlushSendQueue();");
                    sw.WriteLine("\t\t\treturn true;");

                    sw.WriteLine("\t\t}");
                }

                sw.WriteLine("\t}");
                sw.WriteLine("}");
            }
        }


        private void GenerateStubCode(Protocol protocol, string path)
        {
            string output_file = String.Format("{0}/{1}.Stub.cs",
                path, protocol.name);
            using (StreamWriter sw = new StreamWriter(output_file))
            {
                foreach (Import import in protocol.import_list)
                {
                    sw.WriteLine("using {0};", import.name);
                }

                sw.WriteLine("namespace {0}", protocol.name);
                sw.WriteLine("{");
                sw.WriteLine("\tpublic class Stub : MessageStub");
                sw.WriteLine("\t{");

                sw.WriteLine("\t\tpublic const int Version = {0};",
                    protocol.version);
                sw.WriteLine();

                foreach (Message message in protocol.message_list)
                {
                    sw.WriteLine("\t\tpublic delegate void {1}Delegate(NetIncomingMessage im, {0}.Message.{1} data);",
                        protocol.name, message.name);
                    sw.WriteLine("\t\tpublic event {0}Delegate On{0};", message.name);

                    sw.WriteLine("\t\t[RpcStubAttribute({0})]", message.id);
                    sw.WriteLine("\t\tpublic virtual {0}.Message.{1} {1}(NetIncomingMessage im)",
                        protocol.name, message.name);
                    sw.WriteLine("\t\t{");

                    sw.WriteLine("\t\t\tMessage.{0} data = new Message.{0}();", message.name);

                    foreach (Data data in message.data_list)
                    {
                        if (data.array > 0)
                        {
                            sw.WriteLine("\t\t\tint count = im.ReadInt32();");
                            sw.WriteLine("\t\t\tdata.{0} = new List<{1}>();", data.name, data.type);
                            sw.WriteLine("\t\t\tfor (int i = 0; i < count; ++i)");
                            sw.WriteLine("\t\t\t{");
                            sw.WriteLine("\t\t\t\tdata.{0}.Add(im.Read{1}());", data.name, data.type);
                            sw.WriteLine("\t\t\t}");

                        }
                        else
                        {
                            sw.WriteLine("\t\t\tdata.{0} = im.Read{1}();", data.name, data.type);
                        }
                    }

                    sw.WriteLine("\t\t\tif(On{0} != null) On{0}(im, data);", message.name);
                    sw.WriteLine();
                    sw.WriteLine("\t\t\treturn data;");

                    sw.WriteLine("\t\t}");
                }

                sw.WriteLine("\t}");
                sw.WriteLine("}");
            }
        }
    }
}