using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Location;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BaconBot.Dialogs
{
    [Serializable]
    public class RootDialog: IDialog<object>
    {
        private readonly string channelId;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (message.Text.ToLower().Contains("location"))
            {
                await context.Forward(new AddressDialog(), ResumeAfterLocationDialogAsync, message, CancellationToken.None);  
                //context.Call(() => new AddressDialog(), ResumeAfterLocationDialogAsync);
            }
            else
            {
                await context.PostAsync($"Say bacon to start your order.");
                context.Wait(MessageReceivedAsync);
            }
            
        }

        private async Task ResumeAfterLocationDialogAsync(IDialogContext context, IAwaitable<string> result)
        {
            context.Done(true);
        }


    }
}