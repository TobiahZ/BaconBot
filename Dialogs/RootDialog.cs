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
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            try
            {
                var message = await result; 
                if (message.Type == ActivityTypes.ContactRelationUpdate || message.Type == ActivityTypes.ConversationUpdate)
                {
                    await context.PostAsync("Welcome to Bacon Bot"); 
                    context.Wait(this.MessageReceivedAsync);
                    return;
                }

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
                    context.Wait(this.DisplayLocationDialog); 
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
                    new CardImage(url: "https://acom.azurecomcdn.net/80C57D/cdn/mediahandler/docarticles/dpsmedia-prod/azure.microsoft.com/en-us/documentation/articles/storage-introduction/20160801042915/storage-concepts.png"),
                    new CardAction(ActionTypes.ImBack, "Uncooked", value: "uncooked")),
                GetHeroCard(
                    "Cooked Bacon",
                    "Process events with Bacon",
                    "1 Pound of Bacon - Cooked",
                    new CardImage(url: "https://azurecomcdn.azureedge.net/cvt-8636d9bb8d979834d655a5d39d1b4e86b12956a2bcfdb8beb04730b6daac1b86/images/page/services/functions/azure-functions-screenshot.png"),
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

                await context.PostAsync($"{ itemText } good choice");
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
 
                await context.PostAsync("Great, you want bacon here: " + formatteAddress);

            }
            await context.PostAsync("Let's take a look at the menu..."); 
            await DisplayBaconOptions(context);
        }

        //Old Code 
        //private static IList<Attachment> GetCardsAttachments()
        //{
        //    var cardList = new List<Attachment>();
        //    foreach (var searchResult in searchResults)
        //    {
        //        var category = searchResult.CategoryName;
        //        foreach (var article in searchResult.KnowledgeBaseArticles)
        //        {
        //            var heroCard = new HeroCard
        //            {
        //                Title = article.Name,
        //                Subtitle = category,
        //                Text = article.Description,
        //                //Images = new List<CardImage>() { cardImage },
        //                Buttons = new List<CardAction>() { new CardAction(ActionTypes.ImBack, "Run Fix", null, article.Name) },
        //            };
        //            cardList.Add(heroCard.ToAttachment());
        //        }
        //    }
        //    return cardList;
        //}

        //private async Task ResumeAfterLocationDialogAsync(IDialogContext context, IAwaitable<string> result)
        //{
        //    // context.Done(true);
        //    var message = await result;
        //    await context.Forward(new SelectionDialog(), ResumeAfterSelectionDialog, message, CancellationToken.None);
        //}

        //public async Task ResumeAfterSelectionDialog(IDialogContext context, IAwaitable<object> result)
        //{
        //    var message = await result;
        //    await context.PostAsync("In Resume After Selection Dialog and Done"); 
        //    context.Done(true);  
        //}

        //private async Task BeginBaconDialog(IDialogContext context, IAwaitable<IMessageActivity> result)
        //{
        //    try
        //    {
        //        var message = await result;
        //        var searchString = message.Text;



        //    }
        //    catch(Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(ex.Message);
        //        await context.PostAsync($"Failed with message: {ex.Message}");
        //        context.Wait(this.MessageReceivedAsync);
        //    }
        //}

        //private async Task DisplayLocationDialog(IDialogContext context, IAwaitable<IMessageActivity> result)
        //{
        //    try
        //    {
        //        var message = await result; 



        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine(ex.Message);
        //        await context.PostAsync($"Failed with message: {ex.Message}");
        //        context.Wait(this.MessageReceivedAsync);
        //    }

        //}

        //var message = await result;

        //if (message.Text.ToLower().Contains("location"))
        //{
        //    await context.Forward(new AddressDialog(), ResumeAfterLocationDialogAsync, message, CancellationToken.None);  
        //    //context.Call(() => new AddressDialog(), ResumeAfterLocationDialogAsync);
        //}
        //else
        //{
        //    await context.PostAsync($"Say bacon to start your order.");
        //    context.Wait(MessageReceivedAsync);
        //}



    }
}