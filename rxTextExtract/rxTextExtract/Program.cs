using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RXDOCCOMLib;
using RXTEXTCOMLib;

namespace rxTextExtract
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() < 2)
            {
                Console.WriteLine("Rasterex SDK Sample");
                Console.WriteLine("rxTextExtract");
                Console.WriteLine("Demonstrate how to extract text from any supported file format and save as a text file.");
                Console.WriteLine("Usage:\nrxExtractText.exe inputfile output_text_file");
                Console.WriteLine("Sample Command Line:\nrxExtractText.exe c:\\pdf\\5tour.pdf c:\\text\\5tour.txt");
                return;
            }

            RxDoc myRxDocument = new RxDoc();
            RxEngine myRxEngine = new RxEngine();
            if (myRxDocument == null)
            {
                Console.WriteLine("RxSDK is not installed on this system.");
                return;
            }
            RxText myRxText = new RxText();

            string extracted_text = "";
            myRxEngine.LicenseCode = "";
            myRxEngine.Start(RXDOCCOMLib.RX_REGISTRY_KEY.RX_REGKEY_LOCAL_MACHINE, "SOFTWARE\\Rasterex\\RxFilters");

            try
            {
                myRxDocument.Open(args[0]);      //First argument, arg0, is input file name
                myRxText.TextExtract(myRxDocument, -1, ref extracted_text);
                myRxDocument.Close();

                using (StreamWriter sw = File.CreateText((args[1])))
                {
                    sw.Write(extracted_text);
                    sw.Close();
                }
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
