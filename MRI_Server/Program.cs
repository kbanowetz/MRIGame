using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRIServer
{
    class Program
    {
        static void Main(string[] args)
        {
			Console.WriteLine ("Debugging Main");
            Server mriServer = new Server();
            mriServer.ListenAndReport();

        }
    }
}
