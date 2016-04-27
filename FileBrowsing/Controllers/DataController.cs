using FileBrowsing.Models;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;

namespace FileBrowsing.Controllers
{
    /// <summary>
    /// Data Controller
    /// </summary>
    public class DataController : ApiController
    {
        private Logger _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Gets Data Model for browsed directory
        /// </summary>
        /// <param name="name">Name of subdirectory to browse</param>
        /// <param name="path">Current path</param>
        /// <returns>Data Model as JObject</returns>
        [HttpGet]
        public JObject GetModel(string name = "", string path = "")
        {
            _log.Trace("GetModel() action called");
            _log.Info("Requested get model for name = \"{0}\", path = \"{1}\"", name, path);

            try
            {
                _log.Trace("Trying to check request parameters...");

                if (name == "..." && (string.IsNullOrEmpty(path) || path.Trim() == string.Empty))
                    throw new Exception("Bad request got, must be path parameter");

                _log.Trace("Request parameters correct!");
                _log.Trace("Trying to create DataModel...");
                DataModel model = new DataModel();
                _log.Trace("DataModel created!");

                _log.Debug("Preparing DataModel...");
                if (name == string.Empty || (name == "..." && path[path.Length - 1] == '\\')) 
                    GetRootModel(ref model);
                else
                {
                    string curPath = string.Empty;

                    if (name == "...")
                    {
                        int lastIndex = path.LastIndexOf('\\');

                        if (lastIndex == path.IndexOf('\\'))
                            lastIndex++;

                        curPath = path.Substring(0, lastIndex);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(path) || path.Trim() == string.Empty)
                            curPath = name;
                        else
                        {
                            if (path[path.Length - 1] == '\\')
                                curPath = string.Concat(path, name);
                            else
                                curPath = string.Concat(path, '\\', name);
                        }
                    }

                    _log.Trace("Trying to check is path valid...");
                    var arr = System.IO.Path.GetInvalidPathChars();

                    foreach (var item in arr)
                        if (curPath.Contains(item))
                            throw new Exception("Bad path got");

                    _log.Trace("Path is valid!");
                    _log.Trace("Trying to check is directory exists...");

                    if (!Directory.Exists(curPath))
                        throw new Exception(string.Format("Directory with path: {0} is doesn't exists", curPath));

                    _log.Trace("Directory exists!");
                    GetModel(ref model, curPath);
                }

                _log.Info("Model is ready. Return it");

                return JObject.FromObject(model);
            }
            catch (UnauthorizedAccessException uae)
            {
                _log.Error("[GetModel] {0}", uae.Message);
                string error = string.Format("{{\"Error\": \"Access to browse directory: {0} denied\"}}", name);
                return JObject.Parse(error);
            }
            catch (Exception e)
            {
                _log.Error("[GetModel] {0}", e.Message);
                string error = string.Format("{{\"Error\": \"{0}\"}}", e.Message);
                return JObject.Parse(error);
            }
        }

        private void GetRootModel(ref DataModel model)
        {
            _log.Trace("GetRootModel() called");
            _log.Trace("Preparing RootModel...");
            var drives = DriveInfo.GetDrives();
            var drivesList = drives.Where(item => item.DriveType == DriveType.Fixed).ToList();            
            model.SubdirsList = drivesList.Select(item => item.Name).ToList();
            FillModel(ref model);
            _log.Debug("RootModel is ready!");
        }

        private void GetModel(ref DataModel model, string path)
        {
            _log.Trace("GetModel() method called");
            _log.Trace("Preparing Model...");
            var di = new DirectoryInfo(path);
            var subdirsList = di.GetDirectories().Where(sd => !sd.Attributes.ToString().Contains(FileAttributes.System.ToString())).ToList();
            var list = new List<string>();
            list.Add("...");
            list.AddRange(subdirsList.Select(item => item.Name));
            model.SubdirsList = list;
            FillModel(ref model, path);
            _log.Debug("Model is ready!");
        }

        private void FillModel(ref DataModel model, string path = "")
        {
            _log.Trace("FillModel() called");
            _log.Trace("Filling Model...");
            int CountFilesLessThan10Mb, CountFilesBetween10And50Mb, CountFilesMoreThan100Mb;
            Count(out CountFilesLessThan10Mb, out CountFilesBetween10And50Mb, out CountFilesMoreThan100Mb, path);
            _log.Debug("Files counted!");
            model.CountFilesLessThan10Mb = CountFilesLessThan10Mb;
            model.CountFilesBetween10And50Mb = CountFilesBetween10And50Mb;
            model.CountFilesMoreThan100Mb = CountFilesMoreThan100Mb;
            model.CurrentPath = path;
            _log.Debug("Model filled!");
        }

        private void Count(out int CountFilesLessThan10Mb, out int CountFilesBetween10And50Mb,
            out int CountFilesMoreThan100Mb, string path = "")
        {
            _log.Trace("Count() called");
            CountFilesLessThan10Mb = 0;
            CountFilesBetween10And50Mb = 0;
            CountFilesMoreThan100Mb = 0;
            _log.Trace("Trying to check is path null or empty...");

            if (string.IsNullOrEmpty(path) || path.Trim() == string.Empty)
                return;
            
            _log.Trace("Path is not null and not empty!");
            CountFiles(ref CountFilesLessThan10Mb, ref CountFilesBetween10And50Mb, ref CountFilesMoreThan100Mb, path);
            _log.Trace("Trying to count files in subdirectories...");
            var dirs = new DirectoryInfo(path).GetDirectories();

            foreach (var di in dirs)
            {
                try
                {
                    if (!di.Attributes.ToString().Contains(FileAttributes.System.ToString()))
                        CountFiles(ref CountFilesLessThan10Mb, ref CountFilesBetween10And50Mb, ref CountFilesMoreThan100Mb, di.FullName);
                }
                catch (UnauthorizedAccessException uae)
                {
                    _log.Error("[Count] Can't count files. {0}", uae.Message);
                }
            }
        }

        private void CountFiles(ref int CountFilesLessThan10Mb, ref int CountFilesBetween10And50Mb,
            ref int CountFilesMoreThan100Mb, string path = "")
        {
            _log.Trace("CountFiles() called");
            _log.Trace("Trying to count files in current directory...");
            int oneMb = 1024 * 1024;
            var files = new DirectoryInfo(path).GetFiles();

            foreach (var fi in files)
            {
                double fileSizeInMb = fi.Length / (double)oneMb;

                if (fileSizeInMb <= 10)
                    CountFilesLessThan10Mb++;
                else if (fileSizeInMb > 10 && fileSizeInMb <= 50)
                    CountFilesBetween10And50Mb++;
                else if (fileSizeInMb >= 100)
                    CountFilesMoreThan100Mb++;
            }
            _log.Trace("Files in current directory counted!");
        }
    }
}