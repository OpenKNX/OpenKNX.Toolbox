using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.Classes.Actions
{
    public interface IAction
    {
        public string ActionName { get; }
        public string Name { get; }
        public int Progress { get; set; }
        public bool IsIndeterminate { get; set; }
        public Task Begin(CancellationToken token);
    }
}
