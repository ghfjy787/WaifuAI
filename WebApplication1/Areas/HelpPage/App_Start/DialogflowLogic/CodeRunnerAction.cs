using Api.Ai.Domain.DataTransferObject.Request;
using Api.Ai.Domain.DataTransferObject.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebApplication1.Areas.HelpPage.App_Start.DialogflowLogic.Attribute;
using WebApplication1.Areas.HelpPage.App_Start.DialogflowLogic.Enums;

namespace WebApplication1.Areas.HelpPage.DialogflowLogic.App_Start
{
    public class CodeRunnerAction
    {
        public const string NOT_RECOGNIZED_RESPONSE = "Non saprei come risponderti, oni-chan";
        public static CodeRunnerAction Instance { get; private set; }
        public delegate QueryRequest InputAction(Dictionary<string, object> parameters);
        public delegate string OutputAction(QueryResponse lastResponse);
        private Dictionary<string, InputAction> InputActions { get; } = new Dictionary<string, InputAction>();
        private Dictionary<string, OutputAction> OutputActions { get; } = new Dictionary<string, OutputAction>();
        private readonly Dictionary<string, Dictionary<string, object>> sessionsVariable;

        public CodeRunnerAction()
        {
            if (Instance != null)
                throw new Exception("CodeRunner is already instantied");

            Instance = this;
            sessionsVariable = new Dictionary<string, Dictionary<string, object>>();
            RegisterActionResolvers();
        }

        public QueryRequest ResolveInputAction(Dictionary<string, object> parameters)
        {
            /*if (parameters.ContainsKey("action"))
            {
                if (InputActions.ContainsKey(parameters["action"].ToString()))
                {
                    return InputActions[parameters["action"].ToString()](parameters);
                }
            }*/

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

        public void RegisterActionResolvers()
        {
            List<MethodInfo> methodInfoResolvers = AppDomain.CurrentDomain.GetAssemblies()
                       .SelectMany(t => t.GetTypes())
                       .Where(t => t.IsClass && t.Namespace == "WebApplication1.Areas.HelpPage.App_Start.DialogflowLogic.ActionResolver"
                                 && t.GetCustomAttributes(typeof(DialogflowClassResolver), false).Length > 0)
                       .SelectMany(t => t.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                       .Where(t => t.GetCustomAttributes(typeof(DialogflowActionResolver), false).Length > 0).ToList();

            foreach(MethodInfo method in methodInfoResolvers)
            {
                DialogflowActionResolver resolverAttribute = method.GetCustomAttribute(typeof(DialogflowActionResolver), false) as DialogflowActionResolver;
                
                if (String.IsNullOrEmpty(resolverAttribute.ActionName) || resolverAttribute.Type == default(CodeRunnerType))
                {
                    throw new Exception($"An action name and a CodeRunnerType must be defined in DialogflowActionResolver attribute. Method name : {method.Name}");
                }

                if(method.GetParameters().Length > 5)
                {
                    throw new Exception($"An input action can't have more than 5 parameters. Method name : {method.Name}");
                }

                if(resolverAttribute.Type == CodeRunnerType.BeforeRequest)
                {
                    List<object> parameterMethod = new List<object>();
                    if (method.ReturnType != typeof(QueryRequest))
                    {
                        throw new Exception($"The return type of an input resolver should be QueryRequest and not {method.ReturnType.ToString()}. Method name: {method.Name}");
                    }

                    ParameterInfo[] parameters = method.GetParameters();
                    if(parameters.Length < 1)
                    {
                        throw new Exception($"Not enought parameters, input resolvers should have a dictionary<string, object> as first parameters. Method name: {method.Name}");
                    }
                    
                    if(resolverAttribute.RequiresInput)
                    {
                        if(parameters.Length > 1)
                        {
                            ParameterInfo inputActions = parameters[1];
                            if(inputActions.ParameterType != typeof(Dictionary<string, InputAction>))
                            {
                                throw new Exception($"An input action resolver second member must be a Dictionary<string, InputAction> if defined. Method name: {method.Name}");
                            }

                            parameterMethod.Add(InputActions);
                        } else
                        {
                            throw new Exception($"Requires input is true but there are not enought parameters. Method name: {method.Name}");
                        }
                    }

                    if (resolverAttribute.RequiresOutput)
                    {
                        if (parameters.Length > 2)
                        {
                            ParameterInfo outputActions = parameters[2];
                            if (outputActions.ParameterType != typeof(Dictionary<string, OutputAction>))
                            {
                                throw new Exception($"An input action resolver third member must be a Dictionary<string, OutputAction> if defined. Method name: {method.Name}");
                            }

                            parameterMethod.Add(OutputActions);
                        }
                        else
                        {
                            throw new Exception($"Requires output is true but there are not enought parameters. Method name: {method.Name}");
                        }
                    }

                    if (parameters.Length > 3)
                    {
                        ParameterInfo outputActions = parameters[3];
                        if (outputActions.ParameterType != typeof(Dictionary<string, Dictionary<string, object>>))
                        {
                            throw new Exception($"An input action resolver fourth member must be a Dictionary<string, Dictionary<string, object>> if defined. Method name: {method.Name}");
                        }

                        parameterMethod.Add(sessionsVariable);
                    }

                    InputActions.Add(resolverAttribute.ActionName, (_parameters) => {
                        parameterMethod.Insert(0, _parameters);
                        return (QueryRequest)method.Invoke(null, parameterMethod.ToArray());
                    });

                } else if(resolverAttribute.Type == CodeRunnerType.AfterRequest)
                {
                    List<object> parameterMethod = new List<object>();
                    if (method.ReturnType != typeof(string))
                    {
                        throw new Exception($"The return type of an input resolver should be string and not {method.ReturnType.ToString()}. Method name: {method.Name}");
                    }

                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length < 1)
                    {
                        throw new Exception($"Not enought parameters, input resolvers should have QueryResponse as first parameter. Method name: {method.Name}");
                    }

                    if (resolverAttribute.RequiresInput)
                    {
                        if (parameters.Length > 1)
                        {
                            ParameterInfo inputActions = parameters[1];
                            if (inputActions.ParameterType != typeof(Dictionary<string, InputAction>))
                            {
                                throw new Exception($"An input action resolver second member must be a Dictionary<string, InputAction> if defined. Method name: {method.Name}");
                            }

                            parameterMethod.Add(InputActions);
                        }
                        else
                        {
                            throw new Exception($"Requires input is true but there are not enought parameters. Method name: {method.Name}");
                        }
                    }

                    if (resolverAttribute.RequiresOutput)
                    {
                        if (parameters.Length > 2 - (resolverAttribute.RequiresInput ? 0 : 1))
                        {
                            ParameterInfo outputActions = parameters[2 - (resolverAttribute.RequiresInput ? 0 : 1)];
                            if (outputActions.ParameterType != typeof(Dictionary<string, OutputAction>))
                            {
                                throw new Exception($"An input action resolver third member must be a Dictionary<string, OutputAction> if defined. Method name: {method.Name}");
                            }

                            parameterMethod.Add(OutputActions);
                        }
                        else
                        {
                            throw new Exception($"Requires output is true but there are not enought parameters. Method name: {method.Name}");
                        }
                    }

                    if (parameters.Length > 3 - (resolverAttribute.RequiresInput ? 0 : 1) - (resolverAttribute.RequiresOutput ? 0 : 1))
                    {
                        ParameterInfo outputActions = parameters[3 - (resolverAttribute.RequiresInput ? 0 : 1) - (resolverAttribute.RequiresOutput ? 0 : 1)];
                        if (outputActions.ParameterType != typeof(Dictionary<string, Dictionary<string, object>>))
                        {
                            throw new Exception($"An input action resolver fourth member must be a Dictionary<string, Dictionary<string, object>> if defined. Method name: {method.Name}");
                        }

                        parameterMethod.Add(sessionsVariable);
                    }

                    OutputActions.Add(resolverAttribute.ActionName, (_parameters) => {
                        parameterMethod.Insert(0, _parameters);
                        return (string)method.Invoke(null, parameterMethod.ToArray());
                    });
                }
            }
        }
    }
}