using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RXDOCCOMLib;
using RXCONVERTCOMLib;

namespace rxThumbnail
{
   class Program
   {
      static void Main(string[] args)
      {
         //Initialize Rasterex Components
         RxDoc myRxDocument = new RxDoc();
         RxConverter myRxConverter = new RxConverter();
         RxEngine myRxEngine = new RxEngine();
         myRxEngine.Start(RXDOCCOMLib.RX_REGISTRY_KEY.RX_REGKEY_LOCAL_MACHINE, "SOFTWARE\\Rasterex\\RxFilters");

         if (args.Count() < 5)
         {
            Console.WriteLine("Usage:\nrxThumbnail.exe width height inputfile outputfile format\n\nThe width and height parameters define the thumbnail size in pixels\nPlease include full path for bot input and utput file names\nSupported thumbnail formats: PNG or JPEG");
         }
         else
         {
            try
            {
               int      nImageWidth  = int.Parse(args[0]);
               int      nImageHeight = int.Parse(args[1]);
               string   outputfilename = args[3];
               string   outputformat;

               if (args[4] == "PNG" || args[4] == "png")
                  outputformat = "Portable Network Graphics Format"; //Use PNG
               else
                  outputformat = "JPEG File Interchange Format";     //Use JPEG

               myRxDocument.Open( args[2] );

               if (myRxDocument.NumLayouts > 1)
               {
                  //Multilayout - DWG - output each layout (each may contain one or more pages/views)
                  for (int layout = 0; layout < myRxDocument.NumLayouts; layout++)
                  {
                     myRxDocument.ActiveLayout = layout;
                     for (int page = 0; page < myRxDocument.Pages; page++)
                     {
                        myRxDocument.ActivePage = page;
                        string localfilename = outputfilename;
                        localfilename = localfilename.Replace(".", "." + (layout + 1) + "." + (page + 1) + ".");
                        myRxConverter.RasterFileFit(localfilename, myRxDocument, outputformat, nImageWidth, nImageHeight, 24, 0x00FFFFFF, 96);
                     }
                  }
               }
               else if (myRxDocument.Pages > 1)
               {
                  //Multipage - PDF, DWF, PLT and more
                  for (int page = 0; page < myRxDocument.Pages; page++)
                  {
                     myRxDocument.ActivePage = page;
                     string localfilename = outputfilename;
                     localfilename = localfilename.Replace(".", "." + (page + 1) + ".");
                     myRxConverter.RasterFileFit(localfilename, myRxDocument, outputformat, nImageWidth, nImageHeight, 24, 0x00FFFFFF, 96);
                  }
               }
               else
               {
                  //Single page document
                  myRxConverter.RasterFileFit(outputfilename, myRxDocument, outputformat, nImageWidth, nImageHeight, 24, 0x00FFFFFF, 96);
               }
               myRxDocument.Close();
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
