using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            // check if activity is of type message
            if (activity != null && activity.GetActivityType() == ActivityTypes.Message)
            {
                // this is to handle Cortana empty invocations(Text property is empty and no conversation update is sent)
                if (string.IsNullOrEmpty(activity.Text))
                {
                    await SendGreetingMessage(activity);
                }
                else
                {
                    await Conversation.SendAsync(activity, () => new EchoDialog());
                    //await Conversation.SendAsync(activity, () => new LuisDialog());
                }
            }
            else
            {
                await HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

        private async Task HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
                IConversationUpdateActivity update = message;
                if (update.MembersAdded.Any())
                {
                    foreach (var newMember in update.MembersAdded)
                    {
                        // the bot is always added as a user of the conversation, since we don't
                        // want to display the message twice ignore the conversation update triggered by the bot
                        if (newMember.Id == message.Recipient.Id)
                        {
                            await SendGreetingMessage(message);
                        }
                    }
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }
        }

        // greeting message
        private async Task SendGreetingMessage(Activity message)
        {
            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
            {
                // only set the speak if it suports it.
                var channelCapability = new ChannelCapability(Address.FromActivity(message));
                var reply = message.CreateReply();

                var client = scope.Resolve<IConnectorClient>();
                try
                {
                    // send a welcome message to the user
                    reply.Text = "Welcome to the Taxi Reservation Bot! I can answer questions about our service and schedule ride reservations.";
                }
                catch (Exception e)
                {
                    // if an error occured add the error text as the message
                    reply.Text = e.Message;
                }
                if (channelCapability.SupportsSpeak())
                {
                    reply.Speak = reply.Text;
                    reply.InputHint = InputHints.ExpectingInput;
                }
                await client.Conversations.ReplyToActivityAsync(reply);
            }
        }
    }
}