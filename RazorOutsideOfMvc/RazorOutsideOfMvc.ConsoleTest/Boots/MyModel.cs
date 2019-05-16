using System;
using System.Collections.Generic;
using System.Text;

namespace RazorOutsideOfMvc.ConsoleTest.Boots
{
    // the model class. this is 100% specific to your context
    public class MyModel
    {
        // this will map to @Model.Name
        public string Name => "Killroy";
    }
}
