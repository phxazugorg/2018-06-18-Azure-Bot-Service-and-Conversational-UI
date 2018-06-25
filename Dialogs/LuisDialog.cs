using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.FormFlow;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [LuisModel("0527ac73-9787-4b1d-876a-f0ecd87cbde7", "b4efdc96406f4e2f9d9476925ce8c2cb")]
    [Serializable]
    public class LuisDialog : LuisDialog<object>
    {
        //[NonSerialized]
        //Timer t;

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.Forward(new FaqDialog(), ResumeAfterFAQ, context.Activity, CancellationToken.None);
        }
        private Task ResumeAfterFAQ(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            context.Wait(this.MessageReceived);
            return Task.CompletedTask;
        }

        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            string message = $"Hello!";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Cancel")]
        public async Task Cancel(IDialogContext context, LuisResult result)
        {
            string message = $"Cancelling reservation.";
            await context.PostAsync(message);
            await ConversationStarter.Cancel(context.Activity.Conversation.Id, context.Activity.ChannelId); 
            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Help")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            string message = $"I can help you with questions about our service and schedule ride reservations.";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("RideReservation")]
        public async Task ReserveRide(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var entities = await PreProcessEntities(activity, result);
            var reservation = new FormDialog<RideReservation>(new RideReservation(), RideReservation.BuildForm, FormOptions.PromptInStart, entities);
            context.Call<RideReservation>(reservation, ResumeAfterReservation);
        }

        private async Task<IList<EntityRecommendation>> PreProcessEntities(IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var entities = result.Entities.ToList();

            // inject location from Cortana if necessary
            var message = await activity;
            var userInfo = message.Entities.FirstOrDefault(e => e.Type.Equals("UserInfo"));
            if (userInfo != null)
            {
                var currentLocation = userInfo.Properties["current_location"];

                if (currentLocation != null)
                {
                    var hub = currentLocation["Hub"];

                    var lat = hub.Value<double>("Latitude");
                    var lon = hub.Value<double>("Longitude");
                    var address = hub.Value<string>("address");

                    if(!entities.Any(e => e.Type.Equals("PickUpLocation")))
                    {
                        var pickupEntity = new EntityRecommendation("PickUpLocation", score: 1.0);
                        pickupEntity.Entity = !string.IsNullOrEmpty(address) ? address : $"{lat},{lon}";
                        entities.Add(pickupEntity);
                    }
                }
            }

            // rename the pickup time
            var pickUpTimeEntity = entities.FirstOrDefault(e => e.Type.StartsWith("builtin.datetimeV2"));
            if (pickUpTimeEntity != null)
            {
                pickUpTimeEntity.Type = "PickupTime";
            }

            return entities;
        }

        private async Task ResumeAfterReservation(IDialogContext context, IAwaitable<RideReservation> result)
        {
            try
            {
                var reservation = await result;
                SaveReservation(context.Activity.AsMessageActivity(), reservation);
                //SetTimer(context.Activity.AsMessageActivity(), reservation);
                await context.PostAsync("Thank you for your reservation! You will receive a notification when the driver arrives.");
            }
            catch (FormCanceledException<RideReservation> e)
            {
                string reply;
                if (e.InnerException == null)
                {
                    reply = $"You quit on {e.Last} -- maybe you can finish next time!";
                }
                else
                {
                    reply = "Sorry, I've had a short circuit. Please try again.";
                }
                await context.PostAsync(reply);
            }
            context.Wait(this.MessageReceived);
        }

        #region proactive messages
        private void SaveReservation(IMessageActivity message, RideReservation reservation)
        {
            //We need to keep this data so we know who to send the message to. Assume this would be stored somewhere, e.g. an Azure Table
            ConversationStarter.toId = message.From.Id;
            ConversationStarter.toName = message.From.Name;
            ConversationStarter.fromId = message.Recipient.Id;
            ConversationStarter.fromName = message.Recipient.Name;
            ConversationStarter.serviceUrl = message.ServiceUrl;
            ConversationStarter.channelId = message.ChannelId;
            ConversationStarter.conversationId = message.Conversation.Id;
            ConversationStarter.reservation = reservation;
        }

        //private void SetTimer(IMessageActivity message, RideReservation reservation)
        //{
        //    //We need to keep this data so we know who to send the message to. Assume this would be stored somewhere, e.g. an Azure Table
        //    ConversationStarter.toId = message.From.Id;
        //    ConversationStarter.toName = message.From.Name;
        //    ConversationStarter.fromId = message.Recipient.Id;
        //    ConversationStarter.fromName = message.Recipient.Name;
        //    ConversationStarter.serviceUrl = message.ServiceUrl;
        //    ConversationStarter.channelId = message.ChannelId;
        //    ConversationStarter.conversationId = message.Conversation.Id;
        //    ConversationStarter.reservation = reservation;

        //    //We create a timer to simulate some background process or trigger
        //    t = new Timer(new TimerCallback(TimerEvent));
        //    t.Change(15000, Timeout.Infinite);
        //}

        //private void TimerEvent(object state)
        //{
        //    t.Dispose();
        //    ConversationStarter.Resume(ConversationStarter.conversationId, ConversationStarter.channelId); //We don't need to wait for this, just want to start the interruption here
        //}
        #endregion
    }
}