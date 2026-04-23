using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.Models
{
    public class FileModel
    {
        public string Name { get; set; } = "Unbenannt";
        public bool IsFile { get; set; } = false;
        public string FullPath { get; set; } = string.Empty;

        public ObservableCollection<FileModel> Items { get; } = new ObservableCollection<FileModel>();
    }
}
