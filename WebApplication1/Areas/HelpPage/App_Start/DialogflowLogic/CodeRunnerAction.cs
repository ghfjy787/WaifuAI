using Api.Ai.Domain.DataTransferObject.Request;
using Api.Ai.Domain.DataTransferObject.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Areas.HelpPage.App_Start.DialogflowLogic.Enums;

namespace WebApplication1.Areas.HelpPage.DialogflowLogic.App_Start
{
    public class CodeRunnerAction
    {
        public const string NOT_RECOGNIZED_RESPONSE = "Non saprei come risponderti, oni-chan";
        public static CodeRunnerAction Instance { get; private set; }
        private delegate QueryRequest InputAction(Dictionary<string, object> parameters);
        private delegate string OutputAction(QueryResponse queryResponse);
        private Dictionary<string, InputAction> InputActions { get; } = new Dictionary<string, InputAction>();
        private Dictionary<string, OutputAction> OutputActions { get; } = new Dictionary<string, OutputAction>();

        public CodeRunnerAction()
        {
            if (Instance != null)
                throw new Exception("CodeRunner is already instantied");

            Instance = this;
        }

        public QueryRequest ResolveInputAction(string action, Dictionary<string, object> parameters)
        {
            if (action != null)
            {
                if (InputActions.ContainsKey(action))
                {
                    return InputActions[action](parameters);
                }
            }

            if (!parameters.ContainsKey("sessionId"))
            {
                return null;
            }

            if (!parameters.ContainsKey("message"))
            {
                return null;
            }

            return new QueryRequest
            {
                SessionId = parameters["sessionId"].ToString(),
                Query = new string[] { parameters["message"].ToString() },
                Lang = Api.Ai.Domain.Enum.Language.Italian
            };
        }

        public string ResolveOutputAction(string action, QueryResponse response)
        {
            if(action == null)
            {
                return NOT_RECOGNIZED_RESPONSE;
            }

            if(OutputActions.ContainsKey(action))
            {
                return OutputActions[action](response);
            }

            if(response.Result != null)
            {
                if(response.Result.Fulfillment != null)
                {
                    return response.Result.Fulfillment.Speech ?? NOT_RECOGNIZED_RESPONSE;
                }
            }

            return NOT_RECOGNIZED_RESPONSE;
        }
    }
}