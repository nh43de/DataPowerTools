//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data.Entity;
//using System.Linq;
//using System.Reflection;
//using System.Runtime.CompilerServices;
//using CodeFirstStoreFunctions;
//using DataAccess.Core;
//using DataMigrationTools.Reflection;

//namespace DataMigrationTools.Wpf.Data
//{
//    public class ReportFunctionsViewerViewModel : INotifyPropertyChanged
//    {
//        private DropDownDisplay<MethodInfo>[] _methodInfoDropDowns;
//        private object _reportFunctionData;
//        private SimpleExtendedDynamic _reportFunctionParams;
//        private MethodInfo _selectedMethod;

//        public ReportFunctionsViewerViewModel(IEnumerable<string> schemas, Func<DbContext> repoFac)
//        {
//            RepoFac = repoFac;

//            using (var repo = repoFac())
//            {
//                MethodInfoDropDowns = GetRptMethods(repo, schemas).OrderBy(m => m.Name)
//                    .Select(m => new DropDownDisplay<MethodInfo>(m.Name, m)).ToArray();
//            }
//        }


//        public string ReportFunctionName => SelectedMethod.Name;

//        /// <summary>
//        ///     The reporting methods that are available
//        /// </summary>
//        public DropDownDisplay<MethodInfo>[] MethodInfoDropDowns
//        {
//            get { return _methodInfoDropDowns; }
//            set
//            {
//                _methodInfoDropDowns = value;
//                OnPropertyChanged();
//            }
//        }

//        public MethodInfo SelectedMethod
//        {
//            get { return _selectedMethod; }
//            set
//            {
//                _selectedMethod = value;
//                SelectedMethodChanged();
//            }
//        }

//        private Func<DbContext> RepoFac { get; set; }

//        /// <summary>
//        ///     This is the dynamic object that is bound to the property grid on the form
//        /// </summary>
//        public SimpleExtendedDynamic ReportFunctionParams
//        {
//            get { return _reportFunctionParams; }
//            set
//            {
//                _reportFunctionParams = value;
//                OnPropertyChanged();
//            }
//        }

//        /// <summary>
//        ///     This is the result of the function call (should be a ICollection of sorts, of objects).
//        /// </summary>
//        public object ReportFunctionData
//        {
//            get { return _reportFunctionData; }
//            set
//            {
//                _reportFunctionData = value;
//                OnPropertyChanged();
//            }
//        }

//        public event PropertyChangedEventHandler PropertyChanged;

//        public static MethodInfo[] GetRptMethods(DbContext repo, IEnumerable<string> schemas)
//        {
//            var publicMethods = repo.GetType().GetMethods();

//            return publicMethods.Join(
//                schemas,
//                method => method.GetCustomAttribute<DbFunctionDetailsAttribute>()?.DatabaseSchema ?? "",
//                schema => schema, (methodInfo, schema) => methodInfo).ToArray();
//        }

//        public void Go()
//        {
//            var repo = RepoFac();

//            ReportFunctionData = MethodInvoker.ExecuteMethod(repo, SelectedMethod, ReportFunctionParams);
//        }


//        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
//        {
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//        }

//        private void SelectedMethodChanged()
//        {
//            ReportFunctionParams = MethodInvoker.GetMethodParamObject(_selectedMethod);
//        }
//    }
//}