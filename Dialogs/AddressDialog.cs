using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Location;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace BaconBot.Dialogs
{
    [Serializable]
    public class AddressDialog: IDialog<string>
    {
        
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(CreateLocationMessage);
        }

        public virtual async Task CreateLocationMessage(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            //var message = await argument; 
            var apiKey = WebConfigurationManager.AppSettings["BingMapsApiKey"];
            var options = LocationOptions.UseNativeControl | LocationOptions.ReverseGeocode;

            var requiredFields = LocationRequiredFields.StreetAddress | LocationRequiredFields.Locality | LocationRequiredFields.Region | LocationRequiredFields.Country | LocationRequiredFields.PostalCode;

            var prompt = "Where should I ship your bacon?";
            var locationDialog = new LocationDialog(apiKey, context.Activity.ChannelId, prompt, options, requiredFields);

            context.Call(new LocationDialog(apiKey, context.Activity.ChannelId, prompt, options, requiredFields), ResumeAfterLocationDialog); 
            //await context.Forward(new LocationDialog(apiKey, context.Activity.ChannelId, prompt, options, requiredFields), ResumeAfterLocationDialog, message, System.Threading.CancellationToken.None);
        }

        private async Task ResumeAfterLocationDialog(IDialogContext context, IAwaitable<Place> result)
        {
            var place = await result;

            if (place != null)
            {
                var address = place.GetPostalAddress();
                var formatteAddress = string.Join(",", new[]
                {
                    address.StreetAddress,
                    address.Locality,
                    address.Region,
                    address.PostalCode,
                    address.Country
                }.Where(x => !string.IsNullOrEmpty(x)));

                
                await context.PostAsync("Thanks, I will ship it to " + formatteAddress);
            }
            context.Done<string>(null);
        }

    }
}