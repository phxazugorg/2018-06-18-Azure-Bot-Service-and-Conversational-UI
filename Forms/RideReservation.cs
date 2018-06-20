using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using AdaptiveCards;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

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
        public string PickUpLocation { get; set; }
        [Prompt("Where you want to go?")]
        public string DropLocation { get; set; }
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
                .Field(nameof(PickUpLocation))
                .Field(nameof(DropLocation))
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

        private static Task GenerateHeroCardConfirmMessage(IMessageActivity confirmation, RideReservation state)
        {
            Regex geoCoordinateRegex = new Regex(@"(-?\d+.?\d+),(-?\d+.?\d+)", RegexOptions.IgnoreCase);

            var messageText = "Ready to submit your reservation?";
            var cardText = confirmation.Text.Replace(messageText, "").Trim();

            confirmation.Text = messageText;

            // check if pickup location is lat,lon  
            if(geoCoordinateRegex.IsMatch(state.PickUpLocation))
            {
                // replace coordinates with verbiage
                cardText = cardText.Replace(state.PickUpLocation, "Your current location");
            }

            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage(url: $"https://dev.virtualearth.net/REST/v1/Imagery/Map/Road/Routes?mapSize=400,200&wp.0={state.PickUpLocation};64;1&wp.1={state.DropLocation};66;2&key=An5x3zGAXYxr6cTaSvbsWilLxUBA75GoOXM3KndDNtQMn2ZAKRGjgnZw2XLMJYtl"));

            HeroCard confirmCard = new HeroCard()
            {
                Title = $"Trip Details",
                Subtitle = $"",
                Images = cardImages,
                Text = cardText
            };

            Attachment confirmAttachment = confirmCard.ToAttachment();
            confirmation.Attachments.Add(confirmAttachment);
            return Task.CompletedTask;
        }

        public static Task GenerateHeroCardNotificationMessage(IMessageActivity message, RideReservation state)
        {
            List<CardImage> cardImages = new List<CardImage>();
            cardImages.Add(new CardImage(url: $"https://dev.virtualearth.net/REST/v1/Imagery/Map/Road/Routes?mapSize=400,200&wp.0={state.PickUpLocation};64;1&wp.1={state.DropLocation};66;2&key=An5x3zGAXYxr6cTaSvbsWilLxUBA75GoOXM3KndDNtQMn2ZAKRGjgnZw2XLMJYtl"));

            HeroCard confirmCard = new HeroCard()
            {
                Title = $"Taxi Bot Notification",
                Subtitle = $"Driver arrived",
                Images = cardImages,
                Text = "Please meet the taxi at the pickup location."
            };

            Attachment confirmAttachment = confirmCard.ToAttachment();
            message.Attachments.Add(confirmAttachment);
            return Task.CompletedTask;
        }

        public static Task GenerateAdaptiveCardNotificationMessage(IMessageActivity message, RideReservation state)
        {
            try
            {
                var tripImageUrl = $"https://dev.virtualearth.net/REST/v1/Imagery/Map/Road/Routes?mapSize=400,200&wp.0={state.PickUpLocation};64;1&wp.1={state.DropLocation};66;2&key=An5x3zGAXYxr6cTaSvbsWilLxUBA75GoOXM3KndDNtQMn2ZAKRGjgnZw2XLMJYtl";

                // read the json in from our file
                WebClient client = new WebClient();
                Stream stream = client.OpenRead("https://vpbotlabssa.blob.core.windows.net/public-images/NotificationCard.json");
                StreamReader reader = new StreamReader(stream);
                string json = reader.ReadToEnd();
                // replace the trip url
                json = json.Replace("<TRIPURL>", tripImageUrl);
                // use Newtonsofts JsonConvert to deserialized the json into a C# AdaptiveCard object
                AdaptiveCard card = JsonConvert.DeserializeObject<AdaptiveCard>(json);                
                // put the adaptive card as an attachment to the reply message
                message.Attachments.Add(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card
                });
            }
            catch (Exception e)
            {
                // if an error occured add the error text as the message
                message.Text = e.Message;
            }
            return Task.CompletedTask;
        }
    }
}