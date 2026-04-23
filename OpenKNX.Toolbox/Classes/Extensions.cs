using OpenKNX.Toolbox.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.Classes
{
    public static class Extensions
    {
        public static void Sort(this ObservableCollection<FileModel> source)
        {
            var sortedList = source.OrderBy(x => x.IsFile).ThenBy(y => y.Name).ToArray();
            source.Clear();
            foreach (var item in sortedList)
                source.Add(item);
        }
    }
}
