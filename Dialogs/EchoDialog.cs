using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using Microsoft.Bot.Builder.FormFlow;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        protected int count = 1;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            if (message.Text.ToLower().StartsWith("reserve"))
            {
                var reservation = new FormDialog<RideReservation>(new RideReservation(), RideReservation.BuildForm, FormOptions.PromptInStart, null);
                context.Call<RideReservation>(reservation, ResumeAfterReservation);
            }
            else
            {
                await context.Forward(new FaqDialog(), ResumeAfterFAQ, message, CancellationToken.None);
            }
        }

        private Task ResumeAfterFAQ(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        private async Task ResumeAfterReservation(IDialogContext context, IAwaitable<RideReservation> result)
        {
            try
            {
                var ReservationResult = await result;
                await context.PostAsync("Thank you!");
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
            context.Wait(MessageReceivedAsync);
        }
    }
}