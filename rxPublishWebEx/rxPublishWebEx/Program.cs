using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RXDOCCOMLib;
using RXCONVERTCOMLib;


namespace rxPublishWebEx
{
   class Program
   {
      static void Main(string[] args)
      {
         RxConverter myRxConverter = new RxConverter();
         RxEngine myRxEngine = new RxEngine();
         myRxEngine.Start(RXDOCCOMLib.RX_REGISTRY_KEY.RX_REGKEY_LOCAL_MACHINE, "SOFTWARE\\Rasterex\\RxFilters");

         if (args.Count() == 0)
         {
            Console.WriteLine("Usage:\rxPublishWebEx.exe inputfile outputfolder");
         }
         else
         {
            myRxConverter.PublishWebEx(args[1], args[0], 0);
         }

         myRxEngine.Stop();

      }
   }
}
