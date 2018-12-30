using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebApplication1.Areas.HelpPage.App_Start.DialogflowLogic.Enums;

namespace WebApplication1.Areas.HelpPage.App_Start.DialogflowLogic.Attribute
{
    [AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    public class DialogflowActionResolver : System.Attribute
    {
        [Required]
        public CodeRunnerType Type { get; set; }
        [Required]
        public string ActionName { get; set; }

        public bool RequiresInput { get; set; }

        public bool RequiresOutput { get; set; }

        public DialogflowActionResolver(CodeRunnerType Type, string ActionName)
        {
            this.Type = Type;
            this.ActionName = ActionName;
            RequiresInput = false;
            RequiresOutput = false;
        }
    }
}