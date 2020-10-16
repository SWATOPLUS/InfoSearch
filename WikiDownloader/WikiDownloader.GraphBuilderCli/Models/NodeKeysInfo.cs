using System.Collections.Generic;

namespace WikiDownloader.GraphBuilderCli.Models
{
    public class NodeKeysInfo
    {
        public HashSet<string> Regular { get; set; }

        public Dictionary<string, string> References { get; set; }

        public HashSet<string> BadTargetReference { get; set; }

        public HashSet<string> CycleReference { get; set; }
    }
}
