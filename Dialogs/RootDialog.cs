using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Location;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace BaconBot.Dialogs
{
    [Serializable]
    public class RootDialog: IDialog<object>
    {
        private readonly string channelId;
        //private Place _address = new Place(); 

        public async Task StartAsync(IDialogContext context)
        {
            context.Call(new WelcomeDialog(), ResumeAfterWelcome); 
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            try
            {
                var message = await result; 
               
                //if (message.Type == ActivityTypes.ContactRelationUpdate || message.Type == ActivityTypes.ConversationUpdate)
                //{
                //    await context.PostAsync("Welcome to Bacon Bot");
                //    context.Call(new WelcomeDialog(), ResumeAfterWelcome);
                //    //context.Wait(this.MessageReceivedAsync);
                //    return;
                //}

                var messageText = message.Text;
                if (message.Text.ToLower().Contains("help") ||
                    message.Text.ToLower().Contains("support") ||
                    message.Text.ToLower().Contains("problem") ||
                    message.Text.ToLower().Contains("try again"))
                {
                    await context.PostAsync($"Say Bacon to start your order");
                    context.Wait(this.MessageReceivedAsync);
                    return;
                }
                else if (message.Text.ToLower().Contains("bacon"))
                {
                    //context.Wait(this.DisplayLocationDialog);
                    context.Call(new CheckLocationDialog(), ResumeAfterLocationCheckDialog);
                }
                else
                {
                    await context.PostAsync($"Welcome to Bacon Bot, say 'bacon' to start your order");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                await context.PostAsync($"Failed with message: {ex.Message}");
                context.Wait(this.MessageReceivedAsync);
            }
            
            
        }

        private async Task ResumeAfterLocationCheckDialog(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result; 
            
            await context.PostAsync(message); 
            throw new NotImplementedException();
        }

        private async Task ResumeAfterWelcome(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            try
            {
                var message = await result;
                context.Wait(this.MessageReceivedAsync); 

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                await context.PostAsync($"Failed with message: {ex.Message}");
                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task DisplayBaconOptions(IDialogContext context)
        {
            try
            {
                var reply = context.MakeMessage();
                reply.Text = "Select your Bacon";
                reply.Summary = "Summary";
                reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                reply.Attachments = GetBaconCards();

                await context.PostAsync(reply);

                context.Wait(this.OnBaconOptionSelected);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                await context.PostAsync($"Failed with message: {ex.Message}");
                context.Wait(this.MessageReceivedAsync);
            }
   
        }

        private static IList<Attachment> GetBaconCards()
        {
            return new List<Attachment>()
            {
                GetHeroCard(
                    "Unooked Bacon",
                    "Massively scalable Bacon",
                    "1 Pound of Uncooked Bacon",
                    new CardImage(url: "https://upload.wikimedia.org/wikipedia/commons/e/e8/RawBacon.JPG"),
                    new CardAction(ActionTypes.ImBack, "Uncooked", value: "uncooked")),
                GetHeroCard(
                    "Cooked Bacon",
                    "Process events with Bacon",
                    "1 Pound of Bacon - Cooked",
                    new CardImage(url: "http://thefreshaussie.com/wp-content/uploads/2015/02/pile_of_bacon.jpg"),
                    new CardAction(ActionTypes.ImBack, "Cooked", value: "cooked")),
            };
        }

        private static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage },
                Buttons = new List<CardAction>() { cardAction },
            };

            return heroCard.ToAttachment();
        }

        private async Task OnBaconOptionSelected(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            try
            {
                var selectedItem = await result;
                var itemText = selectedItem.Text;

                if (itemText.Trim().ToLower() == "try again")
                {
                    context.Wait(this.MessageReceivedAsync);
                    return; 
                }

                var selection = itemText;
                context.ConversationData.SetValue("BaconChoice", selection);


                var baconChoice = "";
                var address = ""; 
                
                context.ConversationData.TryGetValue("BaconChoice", out baconChoice);
                context.ConversationData.TryGetValue("Address", out address); 

                await context.PostAsync($"{ baconChoice } good choice");
                await context.PostAsync($"Your bacon will be delivered here within the hour: { address } "); 
                                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                await context.PostAsync($"Failed with message: {ex.Message}");
            }
            finally
            {
                context.Wait(this.MessageReceivedAsync);
            }

        }


        public async Task DisplayLocationDialog(IDialogContext context, IAwaitable<IMessageActivity> result)
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
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                await context.PostAsync($"Failed with message: {ex.Message}");
                context.Wait(this.MessageReceivedAsync);
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
                await context.PostAsync("Great, you want bacon here: " + formatteAddress);

            }
            
            await context.PostAsync("Let's take a look at the menu..."); 
            await DisplayBaconOptions(context);
        }

       


    }
}