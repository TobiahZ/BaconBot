using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BaconBot.Dialogs
{
    [Serializable]
    public class WelcomeDialog : IDialog<IMessageActivity>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(WelcomeUser);

        }

        public virtual async Task WelcomeUser(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            await context.PostAsync("Welcome to Bacon bot");
            await context.PostAsync("We're open from 9am-9pm Friday - Sunday");
            await context.PostAsync("Right now we only deliver in the Haight...");
            context.Done(result); 
        }

    }
}