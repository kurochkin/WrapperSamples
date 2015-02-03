using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            Assembly asmbl =
                Assembly.LoadFile(Path.Combine(currentDir, "SrcLib.dll"));
            var classes = asmbl.GetTypes().Where(t => t.IsClass);
            foreach (var @class in classes)
            {
                ProcessClass(@class, sb);
            }
            Console.WriteLine(sb.ToString());
            Console.ReadKey();
        }

        private static void ProcessClass(Type @class, StringBuilder sb)
        {

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
