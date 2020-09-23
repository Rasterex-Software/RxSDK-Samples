using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RXDOCCOMLib;
using RXCONVERTCOMLib;
using RXREDCOMLib;
using RxPDFComLib;
using RXCONFIGCOMLib;

/*
 * Test arguments:
 * C:\Testfiles\Markup\ASESMP.DWG C:\Temp\ASESMP_annotations.pdf C:\Testfiles\Markup\ASESMP.XCM
 * C:\Testfiles\Markup\ASESMP.DWG C:\Temp\ASESMP_burnin.pdf C:\Testfiles\Markup\ASESMP.XCM /burnin
 * C:\Testfiles\Markup\F_16.TIF C:\Temp\F_16.pdf C:\Testfiles\Markup\F_16.XCM
 */

namespace rxPublishPDF
{
   class Program
   {
      [STAThread]
      static void Main(string[] args)
      {
         //Crate needed objects
         RxDoc          myRxDocument = new RxDoc();
         RxConverter    myRxConverter = new RxConverter();
         RxEngine       myRxEngine = new RxEngine();
         RxPDF          myRxPDF = new RxPDF();
         RxRedline      myRxRedline = new RxRedline();
         RxSaveSettings mySettings = new RxSaveSettings();

         if (args.Count() < 2)
         {
            Console.WriteLine("Convert any format to a PDF file with or without markup.");
            Console.WriteLine("Usage:\nrxPublishPDF.exe inputfile outputfile markupfile (/burnin)");
            return;
         }


         if (myRxDocument == null || myRxEngine == null)
         {
            Console.WriteLine("RxSDK is not installed on this system.");
            return;
         }

         //Start RxEngine - use your license code below if you have one, if no license code is given, RxEngine will check out a RxSDK FlexLM license
         //myRxEngine.LicenseCode = "";
         myRxEngine.Start(RXDOCCOMLib.RX_REGISTRY_KEY.RX_REGKEY_LOCAL_MACHINE, "SOFTWARE\\Rasterex\\RxFilters");

         //Turn off PDF/A (default standard selection)
         //Available options : 0 : Standard PDF, 1 : PDF/A-1B, 2: PDF/A-2B
         mySettings.SetFilterLongValue(enumLongSettings.rxPDFStandardSelect, 0);    //Select Standard PDF output
         myRxEngine.SetSaveFilterConfig(mySettings);

         //Publish input file as PDF
         try
         {
            myRxDocument.Open(args[0]);      //First argument, arg0, is input file name
            
            //Extract some information about the given file
            double   dFileConversionScale = 25.4;   //assume mm - used to apply markup
            string   Name = null;
            string   Compression = null;
            int      dpi = 0;
            double   dscale = 1.0, doffsetx = 0.0, doffsety = 0.0, dw = 0, dh = 0;
            myRxDocument.GetPageInfo(0, ref Name, ref Compression, ref dpi, ref dscale, ref doffsetx, ref doffsety, ref dw, ref dh);
            RX_DOCUMENT_TYPE type = myRxDocument.Type;
            if (type == RX_DOCUMENT_TYPE.RX_DOCTYPE_VECTOR_2D)
            {
               //We will force conversion to a standard paper format (minimum A4 and maximum A0)
               if (dpi != 0)
               {
                  //to mm
                  dw = dw / dpi * 25.4;
                  dh = dh / dpi * 25.4;
               }
               else if (dscale != 0.0)
               {
                  //Note: For this conversion we do assume that the file is in MM
                  dw = dw / dscale;
                  dh = dh / dscale;
               }
               double scale = 1.0;

               //Find largest extent
               double maxextent = Math.Max(dw, dh);
               if (maxextent > 1189)      //Larger than A0 . probably AutoCAD or other CAD format not using paper size
               {
                  //Use A0 size if our file is larger
                  double pw = 841.0;
                  double ph = 1189.0;
                  if (dw > dh)
                  {
                     ph = 841.0;
                     pw = 1189.0;
                  }
                  double sx = pw / dw;
                  double sy = ph / dh;
                  scale = Math.Min(sx, sy);
               }
               else if (maxextent < 210)   //Smaller than A4. could be  Acad, Mi10 or other format
               {
                  //Use A4
                  double pw = 210.0;
                  double ph = 297.0;
                  if (dw > dh)
                  {
                     ph = 210.0;
                     pw = 297.0;
                  }
                  double sx = pw / dw;
                  double sy = ph / dh;
                  scale = Math.Min(sx, sy);
               }

               if (dpi == 0)
               {
                  if (scale > 0)
                  {
                     dFileConversionScale = 25.4 / scale;  //Assume mm and apply to scaling
                  }
               }
               else
               {
                  if (scale > 0)
                  {
                     dFileConversionScale = dpi / scale;
                  }
               }

               //Create our new PDF file - VectorSaveTransformEx will make sure we keep vector geometry.
               myRxConverter.VectorSaveTransformEx( args[1], myRxDocument, "Acrobat PDF", dFileConversionScale, 1.0, 0.0, 1);

               //scale from mm to pdf units, 72 dpi (for markup later)
               dFileConversionScale = 1.0 / dscale * (1.0 / dFileConversionScale * 72);
            }
            else    //All other document types end up here - we will rasterize!
            {
               bool converttopdf = true;
               //First check if the original document is a PDF - we will not have to convert PDF to raster 
               if (type == RX_DOCUMENT_TYPE.RX_DOCTYPE_TEXT)
               {
                  if (myRxDocument.FilterName == "RxFilterDynaPDF")
                  {
                     dFileConversionScale = 72.0 / 600.0;   //From internal RimEngine DPI to actual PDF dpi - used for markup scaling
                     converttopdf = false;
                     //Just copy the original pdf to the new location
                     System.IO.File.Copy(args[0], args[1], true);
                  }
               }

               if (converttopdf)
               {
                  int rasterdpi = 200;   //For this sample we will use 200 dpi for raster images
                  double dimagescaling = (double)rasterdpi / (double)dpi;
                  int width = (int)(dw * dimagescaling + .9);
                  int height = (int)(dh * dimagescaling + .9);
                  int whitebg = 0x00ffffff;
                  int bitsperpixel = 24;  //default - true color

                  if (type == RX_DOCUMENT_TYPE.RX_DOCTYPE_RASTER)
                  {
                     //This is a raster file, do we actually need 24 bit? Check all pages and use largest bitsperpixel found
                     int bpp = 1;
                     for (int i = 0; i < myRxDocument.Pages; i++)
                     {
                        myRxDocument.ActivePage = i;
                        if (myRxDocument.BitsPerPixel > bpp)
                           bpp = myRxDocument.BitsPerPixel;
                     }
                     bitsperpixel = bpp;
                  }
                  myRxConverter.RasterFileMP(args[1], myRxDocument, "Acrobat PDF", width, height, bitsperpixel, whitebg, rasterdpi, 0, 0, 0, dimagescaling, dimagescaling);
                  dFileConversionScale = 72.0 / dpi;   //From office or raster dpi to pdf - used for markup scaling
               }
            }

            //do we have markup data - yes we have if 3 or more arguments given
            if (args.Count() >= 3)
            {
               //apply markup now
               myRxRedline.OpenEx(myRxDocument, args[2]);
               myRxRedline.PrepareConversion(myRxDocument); //Load and rescale for each page in document
               //Add markup elements to PDF, either "burned in" or as PDF annotations:
               myRxPDF.Start(myRxEngine);
               if (args.Count() == 4 && args[3] == "/burnin" )
                  myRxPDF.PDFMarkupBurnIn(args[1], myRxDocument, myRxRedline, dFileConversionScale);
               else
                  myRxPDF.ExportPDFMarkupEx(args[1], myRxDocument, myRxRedline, dFileConversionScale);
            }

            myRxDocument.Close();
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
         }

         //Stop RxEngine - no longer needed - will also check in license if FlexLM is used.
         myRxEngine.Stop();
      }
   }
}
