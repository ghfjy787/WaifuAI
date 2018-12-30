using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Areas.HelpPage.App_Start.DialogflowLogic.Attribute
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class DialogflowClassResolver : System.Attribute
    {
        public DialogflowClassResolver() { }
    }
}