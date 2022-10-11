//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using DataAccess.Core;
//using Microsoft.Reporting.WinForms;

//namespace ReportViewerLib
//{
//    //TODO: way to report back which ones have failed

//    public abstract class ReportGeneratorBase
//    {
//        public Func<string, string> CleanTimeString = s => DateTime.Parse(s).ToShortDateString().Replace(@"/", "-");

//        public Func<string, string> CleanFilePathString = 
//            s => s.Replace(@"\", "-")
//                .Replace(@"/", "-")
//                .Replace("%", "")
//                .Replace("<", "")
//                .Replace(">", "");

//        private ReportViewer _reportViewer;

//        protected ReportGeneratorBase()
//        {
//        }

//        protected ReportGeneratorBase(string reportServerUri, string reportPath)
//        {
//            ReportServerUri = reportServerUri;
//            ReportPath = reportPath;
//        }

//        /// <summary>
//        ///     E.g.
//        ///     $"StaticPoolReport_{Database.Trim()}_{AsOfMin.Trim()}_{AsOfMax.Trim()}_{CreditTierId.DisplayText.Trim()}_{CollateralGroupId.DisplayText.Trim()}_{LoanTypeGroupId.DisplayText.Trim()}"
//        ///     (without file extension)
//        /// </summary>
//        /// <returns></returns>
//        //public abstract string FileNameFunction();
//        public ObservableCollection<ReportParameterProcessTask> ReportProcessingList { get; } =
//            new ObservableCollection<ReportParameterProcessTask>();

//        /// <summary>
//        ///     Web Address of your report server (ex: http://rserver/reportserver)
//        /// </summary>
//        public virtual string ReportServerUri { get; set; }

//        public virtual string ReportPath { get; set; }

//        protected ReportViewer ReportViewer
//        {
//            get
//            {
//                if(_reportViewer == null)
//                    InitializeReportViewer();

//                return _reportViewer;
//            }
//            private set { _reportViewer = value; }
//        }

//        public virtual string ProgressText { get; set; }

//        /// <summary>
//        /// </summary>
//        /// <param name="reportServerUri">Web Address of your report server (ex: http://rserver/reportserver/ )</param>
//        private void InitializeReportViewer()
//        {
//            ReportViewer = new ReportViewer
//            {
//                ServerReport =
//                {
//                    ReportServerUrl = new Uri(ReportServerUri),
//                    ReportPath = ReportPath
//                }
//            };
//        }
        
//        public void Start(Action onComplete, IProgress<string> progress = null)
//        {
//            if(progress == null)
//                progress = new Progress<string>(s => { ProgressText = s; });

//            Action startAction = () => ProgressText = "Currently Processing";

//            progress.Report("Beginning report generation...");
            
//            StartReportProcessing(
//                startAction,
//                progress);

//            onComplete?.Invoke();
//        }

//        private void StartReportProcessing(Action start, IProgress<string> activityAction)
//        {
//            start();

//            ProcessReports(activityAction);
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="fileExportDirectory"></param>
//        /// <param name="reportOutputFormat"></param>
//        /// <param name="fileNameFunction"></param>
//        /// <param name="reportParameters"></param>
//        public void AddReport(string fileExportDirectory, ReportOutputFormat reportOutputFormat, Func<string> fileNameFunction,
//            List<ReportParameter> reportParameters)
//        {
//            var fileName = fileNameFunction();

//            if (Path.HasExtension(fileName) == false)
//            {
//                fileName += reportOutputFormat.GetFileExtension();
//            }

//            fileName = CleanFilePathString(fileName);
            
//            fileName = Path.GetInvalidFileNameChars()
//                .Aggregate(fileName, (current, chr) => current.Replace(chr.ToString(), ""));

//            ReportProcessingList.Add(new ReportParameterProcessTask
//            {
//                ReportParameters = reportParameters,
//                FileExportPath = Path.Combine(fileExportDirectory, fileName)
//            });
//        }

//        private void ProcessReports(IProgress<string> activityNotification)
//        {
//            var i = 1;
//            var tot = ReportProcessingList.Count;

//            foreach (var a in ReportProcessingList)
//            {
//                activityNotification?.Report($"Currently Processing: {i}/{tot} - {a.FileExportPath}");
//                ReportFunctions.GetReport(ReportServerUri, ReportPath, a.FileExportPath, a.ReportParameters);
//                i++;
//            }
//        }

//        protected void SetParam(string paramName, string value)
//        {
//            ReportViewer.ServerReport.SetParameters(new ReportParameter(paramName, value));
//        }

//        protected List<DropDownDisplay<string>> GetParamValidValues(string paramName)
//        {
//            var param = ReportViewer.ServerReport
//                .GetParameters()
//                .FirstOrDefault(p => p.Name == paramName);

//            var paramValues = param?.ValidValues;

//            if (paramValues == null)
//                return new List<DropDownDisplay<string>>();

//            return paramValues.Select(v => new DropDownDisplay<string>
//            {
//                DisplayText = v.Label,
//                Value = v.Value
//            }).ToList();
//        }
//    }
//}