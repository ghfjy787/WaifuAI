using Api.Ai.Domain.DataTransferObject.Request;
using Api.Ai.Domain.DataTransferObject.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Areas.HelpPage.App_Start.DialogflowLogic.Attribute;
using WebApplication1.Areas.HelpPage.DialogflowLogic.App_Start;

namespace WebApplication1.Areas.HelpPage.App_Start.DialogflowLogic.ActionResolver
{
    [DialogflowClassResolver]
    public static class GenericResolver
    {
        [DialogflowActionResolver(Enums.CodeRunnerType.AfterRequest, "input.welcome", RequiresOutput = true)]
        public static string WelcomeAction(QueryResponse response, Dictionary<string, CodeRunnerAction.OutputAction> outputActions, Dictionary<string, Dictionary<string, object>> sessions)
        {
            Dictionary<string, object> currentSession = null;
            if(sessions.ContainsKey(response.SessionId.ToString()))
            {
                currentSession = sessions[response.SessionId.ToString()];
            }

            if(currentSession.ContainsKey("name"))
            {
                return response.Result.Fulfillment.Speech + $" '{currentSession["name"]}'";
            }

            return response.Result.Fulfillment.Speech + ". Come ti chiami?";
        }

        /*[DialogflowActionResolver(Enums.CodeRunnerType.BeforeRequest, "action.welcome")]
        public static QueryRequest WelcomeAction(string text, QueryResponse lastResponse)
        {
            return null;
        }*/
    }
}