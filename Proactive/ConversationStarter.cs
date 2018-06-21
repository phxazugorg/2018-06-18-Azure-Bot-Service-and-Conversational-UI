using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    public class ConversationStarter
    {
        //Note: Of course you don't want these here. Eventually you will need to save these in some table
        //Having them here as static variables means we can only remember one user :)
        public static string fromId;
        public static string fromName;
        public static string toId;
        public static string toName;
        public static string serviceUrl;
        public static string channelId;
        public static string conversationId;
        public static RideReservation reservation;


        //This will send an adhoc message to the user
        public static async Task Resume(string conversationId, string channelId)
        {
            var userAccount = new ChannelAccount(toId, toName);
            var botAccount = new ChannelAccount(fromId, fromName);
            var connector = new ConnectorClient(new Uri(serviceUrl));

            IMessageActivity message = Activity.CreateMessageActivity();
            if (!string.IsNullOrEmpty(conversationId) && !string.IsNullOrEmpty(channelId))
            {
                message.ChannelId = channelId;
            }
            else
            {
                conversationId = (await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount)).Id;
            }
            message.From = botAccount;
            message.Recipient = userAccount;
            message.Conversation = new ConversationAccount(id: conversationId);
            message.Locale = "en-Us";
            message.Text = "Driver Arrived Notification";
            //await RideReservation.GenerateHeroCardNotificationMessage(message, reservation);
            await RideReservation.GenerateAdaptiveCardNotificationMessage(message, reservation);
            await connector.Conversations.SendToConversationAsync((Activity)message);
        }
    }
}