using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RazorOutsideOfMvc.ConsoleTest.Boots
{
    // the sample base template class. It's not mandatory but I think it's much easier.
    public abstract class MyTemplate
    {
        // this will map to @Model (property name)
        public MyModel Model => new MyModel();

        public void WriteLiteral(string literal)
        {
            // replace that by a text writer for example
            Console.Write(literal);
        }

        public void Write(object obj)
        {
            // replace that by a text writer for example
            Console.Write(obj);
        }

        public async virtual Task ExecuteAsync()
        {
            await Task.Yield(); // whatever, we just need something that compiles...
        }
    }
}
