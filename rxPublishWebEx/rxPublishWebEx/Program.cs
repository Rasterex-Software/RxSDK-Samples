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
         //Default publish options:
         RXCONVERTCOMLib.RX_WEBPUBLISH_OPTIONS options = RXCONVERTCOMLib.RX_WEBPUBLISH_OPTIONS.RX_WEB_NOPRINTIMAGES | RXCONVERTCOMLib.RX_WEBPUBLISH_OPTIONS.RX_WEB_VECTOREMBEDIMAGES;

         RxConverter myRxConverter = new RxConverter();
         RxEngine myRxEngine = new RxEngine();
         myRxEngine.Start(RXDOCCOMLib.RX_REGISTRY_KEY.RX_REGKEY_LOCAL_MACHINE, "SOFTWARE\\Rasterex\\RxFilters");

         if (args.Count() == 0)
         {
            Console.WriteLine("Usage:\nrxPublishWebEx.exe inputfile outputfolder");
            Console.WriteLine("Optional usage:\nrxPublishWebEx.exe inputfile outputfolder /nocache");
         }
         else
         {
            if ( args.Count()> 2 && args[2].Equals( "/nocache", StringComparison.OrdinalIgnoreCase) )
               options |= RXCONVERTCOMLib.RX_WEBPUBLISH_OPTIONS.RX_WEB_NOCACHEPATH;
            myRxConverter.PublishWebEx(args[1], args[0], options );
         }

         myRxEngine.Stop();

      }
   }
}
