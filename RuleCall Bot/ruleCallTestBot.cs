using Beezlabs.RPAHive.Lib.V2;
using System;
using System.Collections.Generic;
using RestSharp;
using RestSharp.Authenticators;
using Newtonsoft.Json;
using System.Linq;
using Beezlabs.RPAHive.Lib.V2.Models;

namespace Beezlabs.RPA.Bots
{

    public class ruleCallTestBot : RPABotTemplate
    {
        BotExecutionModel botExecutionModel = null;
        string customerCode = null;

        protected override void BotLogic(BotExecutionModel botExecutionModel)
        {
            try
            {
                //System.Diagnostics.Debugger.Launch();
                //System.Diagnostics.Debugger.Break();
                this.botExecutionModel = botExecutionModel;
                string license = botExecutionModel.proposedBotInputs["licenseValue"].value.ToString();
                string valueFromRuleCall = rulecallMapping(license);
                AddVariable("RuleCallValue", valueFromRuleCall);
                Success("Bot Executed Sucessfully");
            }
            catch (Exception ex)
            {
                Failure($"Bot failed due to {ex.Message} {ex.StackTrace}");
            }

        }

        private string rulecallMapping(string license)
        {
            try
            {
                string clientUrl = botExecutionModel.proposedBotInputs["restClientUrl"].value.ToString();
                string clientSecret = botExecutionModel.proposedBotInputs["tulipGenericIdentityKey"].value.ToString();
                string usrname = botExecutionModel.identityList.SingleOrDefault(cred => cred.name.Equals(clientSecret)).credential.basicAuth.username;
                LogMessage(this.GetType().FullName, $"UserName fetched From Identity {usrname}");
                string password = botExecutionModel.identityList.SingleOrDefault(cred => cred.name.Equals(clientSecret)).credential.basicAuth.password;
                
                Dictionary<string, string> value = new Dictionary<string, string>();
                value.Add("value", license);
                Dictionary<string, object> lcode = new Dictionary<string, object>();
                lcode.Add("licenseecode", value);
                Dictionary<string, object> variable = new Dictionary<string, object>();
                variable.Add("variables", lcode);
                var json = JsonConvert.SerializeObject(variable);
                var client = new RestClient(clientUrl);
                client.Authenticator = new HttpBasicAuthenticator(usrname, password);
                var request = new RestRequest(RestSharp.Method.POST);
                request.AddHeader("Content-Type", "application/json; charset=utf-8");
                request.AddJsonBody(json);
                IRestResponse getresponse = client.Execute(request);
                if (getresponse.IsSuccessful)
                {
                    dynamic res = JsonConvert.DeserializeObject(getresponse.Content);
                    customerCode = res.ToString();
                    if (customerCode == "[]")
                    {
                        return "";
                    }
                    else
                    {
                        customerCode = res[0].sapcustcode.value.ToString();
                    }
                }

                return customerCode;

            }
            catch (Exception ex)
            {
                throw new Exception("while doing the rule call for licensee number " + ex.Message);
            }
        }
    }
}