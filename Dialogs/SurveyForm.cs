using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.FormFlow;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    public enum DepartmentOptions
    {
        Accounting,
        AdministrativeSupport,
        IT
    }

    [Serializable]
    public class SurveyForm
    {
        [Prompt("Please enter your {&}.")]
        public string Name { get; set; }
        [Prompt("Please enter your {&}.")]
        public string Phone { get; set; }
        [Prompt("Please enter your {&}.")]
        [Pattern(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}")]
        public string Email { get; set; }
        [Prompt("What {&} do you work in? {||}.")]
        public DepartmentOptions? Department { get; set; }

        public static IForm<SurveyForm> BuildForm()
        {
            return new FormBuilder<SurveyForm>().Build();
        }
    }
}