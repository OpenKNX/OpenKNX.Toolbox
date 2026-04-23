using OpenKNX.Toolbox.Lib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.Models
{
    public class ApplicationModel
    {
        public string AppId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string HelpThread { get; set; } = string.Empty;
        public List<ReleaseModel> Releases { get; set; } = new();

        public ApplicationModel(Application app)
        {
            AppId = app.AppId;
            Name = app.Name;
            Label = app.Label;
            Description = app.Description;
            HelpThread = app.HelpThread;
        }
    }
}
