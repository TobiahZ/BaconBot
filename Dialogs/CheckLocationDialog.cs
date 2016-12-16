using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Location;
using System.Web.Configuration;

namespace BaconBot.Dialogs
{
    [Serializable]
    public class CheckLocationDialog : IDialog<IMessageActivity>
    {
        
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {     
            try
            {
                var message = await result;
                var messageText = message.Text;

                var apiKey = WebConfigurationManager.AppSettings["BingMapsApiKey"];
                var options = LocationOptions.UseNativeControl | LocationOptions.ReverseGeocode;

                var requiredFields = LocationRequiredFields.StreetAddress | LocationRequiredFields.Locality | LocationRequiredFields.Region | LocationRequiredFields.Country | LocationRequiredFields.PostalCode;

                var prompt = "Where should I ship your bacon?";
                var locationDialog = new LocationDialog(apiKey, context.Activity.ChannelId, prompt, options, requiredFields);

                context.Call(new LocationDialog(apiKey, context.Activity.ChannelId, prompt, options, requiredFields), ResumeAfterLocationDialog);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                await context.PostAsync($"Failed with message: {ex.Message}");
                context.Done("Broked");
            }
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

                context.ConversationData.SetValue("Address", formatteAddress);
                if (address.PostalCode == "94117")
                {
                    await context.PostAsync("Great, you want bacon here: " + formatteAddress);
                    context.ConversationData.SetValue("Address", formatteAddress);
                    context.Done(result); 
                }
                else
                {
                    context.ConversationData.SetValue("Address", ""); // Set address to empty string... 
                    await context.PostAsync("Sorry we don't deliver there... Yet.");
                    context.Done(result);  
                }
                
                

            }

            //await context.PostAsync("Let's take a look at the menu...");
            //await DisplayBaconOptions(context);
        }

        
    }
}