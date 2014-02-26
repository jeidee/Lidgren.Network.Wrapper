using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lidgren.Message.Compiler
{
    struct Data
    {
        public string type;
        public string name;
        public int array;
        public string desc;
    }


    class Message
    {
        public string name;
        public UInt32 id;
        public List<Data> data_list;

        public Message()
        {
            data_list = new List<Data>();
        }
    }


    class Flag
    {
        public string name;
        public int value;
        public string desc;
    }


    class Import
    {
        public string name;
    }
    

    class Protocol
    {
        public string name;
        public int number;
        public int version;

        public List<Flag> flag_list = new List<Flag>();
        public List<Import> import_list = new List<Import>();
        public List<Message> message_list = new List<Message>();

        public Protocol()
        {
        }
    }
}
