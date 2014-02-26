using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lidgren.Message.Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("사용법 :");
                Console.WriteLine("MessageCompiler.exe -pChat.xml -o./");
                Environment.Exit(0);
            }

            string protocol_file = args[0].Substring(2);
            string outpath = args[1].Substring(2);

            Parser parser = new Parser();
            Protocol protocol = parser.Parse(protocol_file);
            Generator generator = new Generator();
            generator.Generate(protocol, outpath);

            Console.Write("끝!");
        }
    }
}
