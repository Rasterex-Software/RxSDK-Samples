using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RXDOCCOMLib;
using RXPRINTCOMLib;


namespace rxPrintDocument
{
   class Program
   {
      static void Main(string[] args)
      {
         RxDoc myRxDocument = new RxDoc();
         RxPrinter myRxPrinter = new RxPrinter();
         RxEngine myRxEngine = new RxEngine();
         myRxEngine.Start(RXDOCCOMLib.RX_REGISTRY_KEY.RX_REGKEY_LOCAL_MACHINE, "SOFTWARE\\Rasterex\\RxFilters");

         if (args.Count() < 1)
         {
            Console.WriteLine("Usage:\nrxPrintCOM.exe /command inputfile #");
            Console.WriteLine("Available commands :");
            Console.WriteLine("/l  list all installed printers.");
            Console.WriteLine("/p  print a file to default printer.");
            Console.WriteLine("/pn print a file to printer with ID number # (from list printers).");
         }
         else
         {
            try
            {
               if (args[0] == "/l")
               {
                  Console.WriteLine("Number of printers installed : " + myRxPrinter.InstalledPrinters);
                  for (int i=0; i<myRxPrinter.InstalledPrinters; i++)
                  {
                     string PrinterName = null;
                     myRxPrinter.GetPrinterInfo( i, ref PrinterName );
                     Console.WriteLine(i + " : " + PrinterName);
                  }
               }
               else if (args[0] == "/p" || args[0] == "/pn")
               {
                  myRxDocument.Open(args[1]);

                  if (args[0] == "/pn")
                  {
                     //We have a printer index
                     int PrinterID = Convert.ToInt32( args[2] );
                     string PrinterName = null;
                     myRxPrinter.GetPrinterInfo(PrinterID, ref PrinterName);
                     myRxPrinter.SelectPrinter(PrinterName);
                  }
                  else
                  {
                     myRxPrinter.UseDefaultPrinter();
                  }
   
                  myRxPrinter.StartDoc("Test");

                  myRxPrinter.SetPDFConfig(enumPDFPrintConfig.rxPDFPrintDisableAntialiase, 1);   //Turn off antialiase during printing,

                  //Get information from printer device
                  int PaperW = myRxPrinter.PaperWidth;      //size in pixels
                  int PaperH = myRxPrinter.PaperHeight;

                  //Now print each page in the document - we use scale to fit for all pages
                  for (int i = 0; i < myRxDocument.Pages; i++)
                  {
                     myRxDocument.ActivePage = i;
                     double DocW = myRxDocument.Width;
                     double DocH = myRxDocument.Height;

                     double sx = (double)PaperW / DocW;
                     double sy = (double)PaperH / DocH;
                     double printscale = Math.Min(sx, sy);

                     //Calculate offsets to center document on paper
                     int offsetx = ((PaperW) - (int)(DocW * printscale)) / 2;
                     int offsety = ((PaperH) - (int)(DocH * printscale)) / 2;

                     myRxPrinter.StartPage();
                     myRxPrinter.Print(myRxDocument, 0.0, offsetx, offsety, 0.0, 0.0, printscale, printscale);
                     myRxPrinter.EndPage();
                  }

                  myRxPrinter.EndDoc();
                  myRxDocument.Close();
               }
            }
            catch (Exception ex)
            {
               Console.WriteLine("RxDocument Last Error: " + myRxDocument.LastError);
               Console.WriteLine(ex.Message);
               Console.WriteLine(ex.StackTrace);
            }
         }
         //Cleanup
         myRxEngine.Stop();
      }
   }
}
