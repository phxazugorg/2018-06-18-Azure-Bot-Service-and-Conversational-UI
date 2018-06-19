using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    public enum RideOptions
    {
        Compact,
        Sedan,
        Limousine
    }

    [Serializable]
    public class RideReservation
    {
        [Prompt("Where you want the ride to pick you up?")]
        public string Origin { get; set; }
        [Prompt("Where is your {&}?")]
        public string Destination { get; set; }
        [Prompt("What time do you want your ride to arrive?")]
        [Optional]
        [Template(TemplateUsage.NoPreference, "Now")]
        public DateTime? PickupTime { get; set; }
        [Prompt("What {&} do you want? {||}")]
        public RideOptions? VehicleType { get; set; }

        public static IForm<RideReservation> BuildForm()
        {
            return new FormBuilder<RideReservation>()
                .Prompter(MyPrompter)
                .Field(nameof(Origin))
                .Field(nameof(Destination))
                .Field(nameof(PickupTime))
                .Field(nameof(VehicleType))
                .AddRemainingFields()            
                .Confirm("Ready to submit your reservation? {*}")
                .Build();
        }

        private static async Task<FormPrompt> MyPrompter(IDialogContext context, FormPrompt prompt, RideReservation state, IField<RideReservation> field)
        {
            var preamble = context.MakeMessage();
            var promptMessage = context.MakeMessage();
            if (prompt.GenerateMessages(preamble, promptMessage))
            {
                await context.PostAsync(preamble);
            }
            if (field.Name.ToLower().StartsWith("confirmation"))
            {
                await GenerateHeroCardConfirmMessage(promptMessage, state);
            }
            await context.PostAsync(promptMessage);
            return prompt;
        }

        private static async Task GenerateHeroCardConfirmMessage(IMessageActivity confirmation, RideReservation state)
        {
            var messageText = "Ready to submit your reservation?";
            var cardText = confirmation.Text.Replace(messageText, "").Trim();

            confirmation.Text = messageText;

            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage(url: $"https://dev.virtualearth.net/REST/v1/Imagery/Map/Road/Routes?mapSize=400,200&wp.0={state.Origin};64;1&wp.1={state.Destination};66;2&key=An5x3zGAXYxr6cTaSvbsWilLxUBA75GoOXM3KndDNtQMn2ZAKRGjgnZw2XLMJYtl"));

            HeroCard confirmCard = new HeroCard()
            {
                Title = $"Trip Details",
                Subtitle = $"",
                Images = cardImages,
                Text = cardText
            };

            Attachment confirmAttachment = confirmCard.ToAttachment();
            confirmation.Attachments.Add(confirmAttachment);
        }

        private static async Task GenerateReceiptCardConfirmMessage(IMessageActivity confirmation, RideReservation state)
        {
            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage(url: $"https://dev.virtualearth.net/REST/v1/Imagery/Map/Road/Routes?mapSize=400,200&wp.0={state.Origin};64;1&wp.1={state.Destination};66;2&key=An5x3zGAXYxr6cTaSvbsWilLxUBA75GoOXM3KndDNtQMn2ZAKRGjgnZw2XLMJYtl"));

            HeroCard confirmCard = new HeroCard()
            {
                Title = $"Trip Summary",
                Subtitle = $"",
                Images = cardImages,
                Text = confirmation.Text
            };

            Attachment confirmAttachment = confirmCard.ToAttachment();
            confirmation.Attachments.Add(confirmAttachment);
        }

    }
}