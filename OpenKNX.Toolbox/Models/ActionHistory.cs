using OpenKNX.Toolbox.Classes.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.Models
{
    public class ActionHistory
    {
        public IAction Action { get; set; }
        public bool Succeeded { get; set; }
        public string ExceptionMessage { get; set; } = string.Empty;
        public DateTime TimeStamp { get; set; } = DateTime.Now;

        public ActionHistory(IAction action, string exceptionMessage = "")
        {
            Action = action;
            Succeeded = string.IsNullOrEmpty(exceptionMessage);
            ExceptionMessage = exceptionMessage;
        }
    }
}
