using OpenKNX.Toolbox.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.ViewModels
{
    public class NewConnectionDialogViewModel
    {
        public string Title { get; } = Properties.Resources.NewConnectionTitle;
        public bool CanContinue { get; set; } = true;

        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public int IpPort { get; set; }
        public bool IsTCP { get; set; }
    }
}
