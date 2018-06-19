using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.FormFlow;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [LuisModel("4954d9d0-e747-4f94-a5ea-a5112d9d1054", "f06612b74bb744c6b0b0a0f3e153be74")]
    [Serializable]
    public class LuisDialog : LuisDialog<object>
    {
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Sorry, I did not understand '{result.Query}'. Type 'help' if you need assistance.";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }


        [LuisIntent("RideReservation")]
        public Task ReserveRide(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var reservation = new FormDialog<RideReservation>(new RideReservation(), RideReservation.BuildForm, FormOptions.PromptInStart, result.Entities);
            context.Call<RideReservation>(reservation, ResumeAfterReservation);
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
            context.Wait(this.MessageReceived);
        }
    }
}