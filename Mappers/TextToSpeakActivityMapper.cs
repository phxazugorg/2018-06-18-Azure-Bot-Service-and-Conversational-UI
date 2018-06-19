using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    public class TextToSpeakActivityMapper : IMessageActivityMapper
    {
        public IMessageActivity Map(IMessageActivity message)
        {
            // only set the speak if it is not set by the developer.
            var channelCapability = new ChannelCapability(Address.FromActivity(message));

            if (channelCapability.SupportsSpeak() && string.IsNullOrEmpty(message.Speak))
            {
                SetSpeech(message);

                // set InputHint to ExpectingInput if text is a question
                var isQuestion = message.Text?.EndsWith("?");
                if (isQuestion.GetValueOrDefault())
                {
                    message.InputHint = InputHints.ExpectingInput;
                }
                else
                {
                    message.InputHint = InputHints.AcceptingInput;
                }
            }

            return message;
        }

        private void SetSpeech(IMessageActivity message)
        {
            if(!string.IsNullOrEmpty(message.Text))
            {
                message.Speak = message.Text;
            }
            else
            {
                if(message.Attachments.Any())
                {
                    var attachment = message.Attachments.First();
                    dynamic content = (dynamic)attachment.Content;
                    if(!string.IsNullOrEmpty(content.Text))
                    {
                        message.Speak = content.Text;
                    }
                }
            }
        }
    }
}