using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CodeWrapperGenerator
{
    class Program
    {
        static void Main(string[] args)
        {

            var sb = new StringBuilder();
            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (currentDir == null) throw new ArgumentException("path null");

            Assembly asmbl =
                Assembly.LoadFile(Path.Combine(currentDir, "SrcLib.dll"));
            var classes = asmbl.GetTypes().Where(t => t.IsClass);
            foreach (var @class in classes)
            {
                ProcessClass(@class, sb);
            }

            using (var sw = new StreamWriter(Path.Combine(currentDir, "out.cs"), false, Encoding.UTF8))
            {
                sw.Write(sb.ToString());
            }
            Console.WriteLine("Done");
            Console.ReadKey();
        }

        private static Guid GetDeterministicGuid(string input)
        {

            //use MD5 hash to get a 16-byte hash of the string: 

            var provider = new MD5CryptoServiceProvider();
            byte[] inputBytes = Encoding.Default.GetBytes(input);
            byte[] hashBytes = provider.ComputeHash(inputBytes);
            //generate a guid from the hash: 
            var hashGuid = new Guid(hashBytes);

            return hashGuid;

        }

        private static void ProcessClass(Type @class, StringBuilder sb)
        {
            Console.WriteLine("Processing class: " + @class.FullName );
            sb.AppendLine();
            var guidClass = GetDeterministicGuid(@class.Name);
            sb.AppendLine("[ComVisible(true)]");
            sb.AppendLine("[InterfaceType(ComInterfaceType.InterfaceIsDual)]");
            sb.AppendFormat("[Guid(\"{0}\")]\n", guidClass);
            sb.AppendFormat("[ProgId(\"Pyramid.{0}\")]\n", @class.Name);
            sb.AppendFormat("public class {0} {{\n", @class.Name);
            sb.AppendFormat("private {0} _wrappedInstance;\n", @class.FullName);
            var properties = @class.GetProperties();
            foreach (var propertyInfo in properties)
            {
                sb.AppendFormat("public {0} {1} {{\nget \n{{\n return _wrappedInstance.{1};\n}} \n set \n {{ \n _wrappedInstance.{1} = value;\n}}\n", propertyInfo.PropertyType.Name, propertyInfo.Name);
                //Console.WriteLine("Prop: " + propertyInfo.PropertyType.Name);
            }
            sb.AppendLine("}");

        }
    }
}
