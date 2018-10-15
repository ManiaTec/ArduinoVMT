using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/* Ligação de funções API */
namespace ArduinoV
{
    [InheritedExport]
    public abstract class pll
    {
            public abstract void Start();
            public abstract void cmdd(String comando);
    }
}
