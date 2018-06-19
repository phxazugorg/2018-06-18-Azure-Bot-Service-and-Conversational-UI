using System;
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

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            if (message.Text == "reset")
            {
                PromptDialog.Confirm(
                    context,
                    AfterResetAsync,
                    "Are you sure you want to reset the count?",
                    "Didn't get that!",
                    promptStyle: PromptStyle.Auto);
            }
            else if (message.Text.ToLower() == "ride")
            {
                var reservation = new FormDialog<RideReservation>(new RideReservation(), RideReservation.BuildForm, FormOptions.PromptInStart, null);
                context.Call<RideReservation>(reservation, ResumeAfterReservation);
            }
            else
            {
                await context.PostAsync($"{this.count++}: You said {message.Text}");
                context.Wait(MessageReceivedAsync);
            }
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

        public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                this.count = 1;
                await context.PostAsync("Reset count.");
            }
            else
            {
                await context.PostAsync("Did not reset count.");
            }
            context.Wait(MessageReceivedAsync);
        }

    }
}