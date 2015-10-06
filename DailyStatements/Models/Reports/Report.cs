using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using AMPStatements.ReportExecutionService;

namespace AMPStatements.Models.Reports
{
    public class Report
    {
        // class members 
        private ReportExecutionService.ReportExecutionService rs = new ReportExecutionService.ReportExecutionService();
        private byte[][] _renderedReport;
        private Graphics.EnumerateMetafileProc _delegate = null;
        private MemoryStream _currentPageStream;
        private Metafile _metafile = null;
        private int _numberOfPages;
        private int _currentPrintingPage;
        private int _lastPrintingPage;

        private DatabaseConfiguration _config;
        private int _ACHBatchGroupID;
        private bool _SaveReports;
        private int _ClientID;
        private string _ReportPath = "/CREDITSOFT/";
        private string _Uri = "http://amp-dc/ReportServer/reportexecution2005.asmx?wsdl";
        private string _FilePath = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Daily Statements");

        public Report()
        {
            rs.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
        }

        public Report(int ClientID, DatabaseConfiguration DatabaseConfig, int ACHBatchGroupID, bool SaveReports)
        {
            _ClientID = ClientID;
            _config = DatabaseConfig;
            _ACHBatchGroupID = ACHBatchGroupID;
            _SaveReports = SaveReports;

            _SetReportPath();

#if DEBUG
            rs.Credentials = new System.Net.NetworkCredential("chris", "1b17cc90d", "AMP");
#else
            rs.Credentials = (System.Net.NetworkCredential)System.Net.CredentialCache.DefaultCredentials;
#endif
        }

        public Report(System.Net.NetworkCredential credentials)
        {
            rs.Credentials = credentials;
        }

        private void _SetReportPath()
        {
            _ReportPath = _ReportPath + _config.DatabaseName + "_Reports/Custom_ClientStatementsByID";
            rs.Url = _Uri;
        }

        public bool PrintReport(ParameterValue[] parameters, int fromPage, int toPage)
        {
            this._renderedReport = this.RenderReport(parameters);
            try
            {
                // Wait for the report to completely render.
                if (_numberOfPages < 1)
                {
                    return false;
                }

                //You can set the Required Printer Settings(Paper Size, Page Source, Orientation etc) in printerSettings object defined below
                PrinterSettings printerSettings = new PrinterSettings();
                printerSettings.MaximumPage = _numberOfPages;
                printerSettings.MinimumPage = 1;
                printerSettings.PrintRange = PrintRange.AllPages;//PrintRange.SomePages;

                PrintDocument pd = new PrintDocument();
                pd.PrintController = new StandardPrintController();

                if (toPage != -1 && fromPage != -1)
                {
                    _currentPrintingPage = fromPage;
                    _lastPrintingPage = toPage;
                    if (_numberOfPages < toPage)
                    {
                        toPage = _numberOfPages;
                        _lastPrintingPage = toPage;
                    }
                    if (_numberOfPages < fromPage)
                    {
                        fromPage = _numberOfPages;
                        _currentPrintingPage = fromPage;
                    }
                    printerSettings.FromPage = fromPage;
                    printerSettings.ToPage = toPage;
                }
                else
                {
                    _currentPrintingPage = 1;
                    _lastPrintingPage = _numberOfPages;
                }
                using (WindowsImpersonationContext wic = WindowsIdentity.Impersonate(IntPtr.Zero))
                {
                    //code to send printdocument to the printer
                    pd.PrinterSettings = printerSettings;
                    pd.PrintPage += this.pd_PrintPage;

                    pd.Print();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }



        /// <summary>
        /// This method renders the report as multidimentional byte array.
        /// </summary>
        private byte[][] RenderReport(ParameterValue[] parameters)
        {
            if (_SaveReports)
            {
                if ( ! System.IO.Directory.Exists(_FilePath))
                {
                    System.IO.Directory.CreateDirectory(_FilePath);
                }
            }

            // Private variables for rendering
            string historyId = null;
            ExecutionHeader execHeader = new ExecutionHeader();

            try
            {
                rs.Timeout = 300000;
                rs.ExecutionHeaderValue = execHeader;
                rs.LoadReport(_ReportPath, historyId);
                if ((parameters != null))
                {
                    rs.SetExecutionParameters(parameters, "en_us");
                }


                byte[][] pages = new Byte[0][];
                string format = "IMAGE";
                int numberOfPages = 1;
                byte[] currentPageStream = new byte[1] { 0x00 }; // put a byte to get the loop started
                string extension = null;
                string encoding = null;
                string mimeType = null;
                string[] streamIDs = null;
                Warning[] warnings = null;

                while (currentPageStream.Length > 0)
                {
                    string deviceInfo = String.Format(@"<DeviceInfo><OutputFormat>EMF</OutputFormat><PrintDpiX>200</PrintDpiX><PrintDpiY>200</PrintDpiY>" + "<StartPage>{0}</StartPage></DeviceInfo>", numberOfPages);
                    //rs.Render will render the page defined by deviceInfo's <StartPage>{0}</StartPage> tag
                    currentPageStream = rs.Render(format, deviceInfo, out extension, out encoding, out mimeType, out warnings, out streamIDs);

                    if (currentPageStream.Length == 0 && numberOfPages == 1)
                    {
                        //renderException = EnumRenderException.ZERO_LENGTH_STREAM;
                        break;
                    }

                    //Add the byte stream of current page in pages[] array so that we can have complete report in pages[][] array
                    if (currentPageStream.Length > 0)
                    {
                        Array.Resize(ref pages, pages.Length + 1);
                        pages[pages.Length - 1] = currentPageStream;
                        numberOfPages++;
                    }
                }

                _numberOfPages = numberOfPages - 1;

                return pages;
            }
            catch (System.Web.Services.Protocols.SoapException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                // Console.WriteLine("Number of pages: {0}", pages.Length);
            }

            return null;
        }



        /// <summary>
        /// Handle the Printing of each page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ev"></param>
        private void pd_PrintPage(object sender, PrintPageEventArgs ev)
        {
            ev.HasMorePages = false;
            if (_currentPrintingPage <= _lastPrintingPage && MoveToPage(_currentPrintingPage))
            {
                // Draw the page
                ReportDrawPage(ev.Graphics);
                // If the next page is less than or equal to the last page,
                // print another page.
                if (System.Threading.Interlocked.Increment(ref _currentPrintingPage) <= _lastPrintingPage)
                {
                    ev.HasMorePages = true;
                }
            }
        }



        // Method to draw the current emf memory stream
        private void ReportDrawPage(Graphics g)
        {
            if (_currentPageStream == null || 0 == _currentPageStream.Length || _metafile == null)
            {
                return;
            }
            lock (this)
            {
                // Set the metafile delegate.
                int width = _metafile.Width;

                int height = _metafile.Height;
                _delegate = new Graphics.EnumerateMetafileProc(MetafileCallback);

                // Draw in the rectangle
                Point[] points = new Point[3];
                Point destPoint = new Point(0, 0);
                Point destPoint1 = new Point(width / 2, 0);
                Point destPoint2 = new Point(0, height / 2);

                points[0] = destPoint;
                points[1] = destPoint1;
                points[2] = destPoint2;
                g.EnumerateMetafile(_metafile, points, _delegate);
                // Clean up
                _delegate = null;
            }
        }



        private bool MoveToPage(int page)
        {
            // Check to make sure that the current page exists in
            // the array list
            if (_renderedReport[_currentPrintingPage - 1] == null)
            {
                return false;
            }
            // Set current page stream equal to the rendered page
            _currentPageStream = new MemoryStream(_renderedReport[_currentPrintingPage - 1]);
            // Set its postion to start.
            _currentPageStream.Position = 0;
            // Initialize the metafile
            if (_metafile != null)
            {
                _metafile.Dispose();
                _metafile = null;
            }
            // Load the metafile image for this page
            _metafile = new Metafile((Stream)_currentPageStream);

            if (_SaveReports)
                _metafile.Save(Path.Combine(_FilePath, "_" + _ClientID.ToString() + "(" + page + ").emf"));

            return true;
        }

        private bool MetafileCallback(EmfPlusRecordType recordType, int flags, int dataSize, IntPtr data, PlayRecordCallback callbackData)
        {
            byte[] dataArray = null;
            // Dance around unmanaged code.
            if (data != IntPtr.Zero)
            {
                // Copy the unmanaged record to a managed byte buffer
                // that can be used by PlayRecord.
                dataArray = new byte[dataSize];
                Marshal.Copy(data, dataArray, 0, dataSize);
            }
            // play the record.
            _metafile.PlayRecord(recordType, flags, dataSize, dataArray);
            return true;
        }
    }
}