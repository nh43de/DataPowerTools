//using System;
//using System.Collections.Generic;
//using System.Data.Entity;
//using System.Windows;
//using DataMigrationTools.Extensions;
//using DataMigrationTools.FileOut;

//namespace DataMigrationTools.Wpf.Data
//{
//    /// <summary>
//    ///     Interaction logic for ReportFunctionsViewer.xaml
//    /// </summary>
//    public partial class ReportFunctionsViewer : Window
//    {
//        /// <summary>
//        ///     Initializes a new report functions viewer. To create this, we want to know which schemas to show, and a way of
//        ///     creating a dbcontext (EF repository).
//        /// </summary>
//        /// <param name="schemas"></param>
//        /// <param name="repoFac"></param>
//        public ReportFunctionsViewer(IEnumerable<string> schemas, Func<DbContext> repoFac)
//        {
//            InitializeComponent();

//            _viewModel = new ReportFunctionsViewerViewModel(schemas, repoFac);

//            DataContext = _viewModel;
//        }

//        public ReportFunctionsViewerViewModel _viewModel { get; private set; }

//        private void DataToExcel_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                ToExcelWithDialog.FromList((IEnumerable<object>) _viewModel.ReportFunctionData,
//                    _viewModel.ReportFunctionName, ToFileWithDialog.DefaultFileNameFormat.DateTime);
//            }
//            catch (Exception ex)
//            {
//                ex.DisplayInners("Error occurred exporting data to Excel.");
//            }
//        }

//        private void Go_Click(object sender, RoutedEventArgs e)
//        {
//            try
//            {
//                _viewModel.Go();
//            }
//            catch (Exception ex)
//            {
//                ex.DisplayInners("Error occurred processing report.");
//            }
//        }
//    }
//}