using Newtonsoft.Json;
using System.Collections.Generic;

namespace FileBrowsing.Models
{
    /// <summary>
    /// Data Model
    /// </summary>
    [JsonObject]
    public class DataModel
    {
        /// <summary>
        /// Amount of files with size less than 10 Mb
        /// </summary>
        [JsonProperty("CountFilesLessThan10Mb")]
        public int CountFilesLessThan10Mb { get; set; }

        /// <summary>
        /// Amount of files with size between 10 and 50 Mb
        /// </summary>
        [JsonProperty("CountFilesBetween10And50Mb")]
        public int CountFilesBetween10And50Mb { get; set; }

        /// <summary>
        /// Amount of files with size gross than 100 Mb
        /// </summary>
        [JsonProperty("CountFilesMoreThan100Mb")]
        public int CountFilesMoreThan100Mb { get; set; }

        /// <summary>
        /// Current location path
        /// </summary>
        [JsonProperty("CurrentPath")]
        public string CurrentPath { get; set; }

        /// <summary>
        /// List of children-folders
        /// </summary>
        [JsonProperty("SubdirsList")]
        public List<string> SubdirsList { get; set; }
    }
}