using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [Serializable]
    //[QnAMaker("16603d3c-9fa7-4e23-b189-0ddb45c89015", "236c7cf3-a55a-4492-8df7-44652b71ea51", defaultMessage: "Sorry I don't understand, please rephrase your question.", scoreThreshold:0.4, endpointHostName: "qna-cognitive-service.azurewebsites.net")]
    public class FaqDialog : QnAMakerDialog
    {
        public FaqDialog() : base(new QnAMakerService(new QnAMakerAttribute(ConfigurationManager.AppSettings["QnAAuthKey"],
            ConfigurationManager.AppSettings["QnAKnowledgebaseId"],
        "Sorry I don't understand, please rephrase your question.",
        0.3, 1, ConfigurationManager.AppSettings["QnAHostname"])))
        { }
    }
}