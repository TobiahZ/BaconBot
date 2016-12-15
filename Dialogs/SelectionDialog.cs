using Microsoft.Bot.Builder.Dialogs;
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
    public class SelectionDialog : IDialog<string>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.DisplayBaconCarousel);           
        }

        public virtual async Task DisplayBaconCarousel(IDialogContext context, IAwaitable<string> result)
        {
            var reply = context.MakeMessage();

            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = GetCardsAttachments();

            var root = new RootDialog();

            await context.PostAsync(reply);


            context.Forward(root, root.ResumeAfterSelectionDialog, message, CancellationToken.none);
            //context.Done<string>(null);
            //context.Wait(this.MessageReceivedAsync);
        }

        private static IList<Attachment> GetCardsAttachments()
        {
            return new List<Attachment>()
            {
                GetHeroCard(
                    "Unooked Bacon",
                    "Massively scalable Bacon",
                    "1 Pound of Uncooked Bacon",
                    new CardImage(url: "https://acom.azurecomcdn.net/80C57D/cdn/mediahandler/docarticles/dpsmedia-prod/azure.microsoft.com/en-us/documentation/articles/storage-introduction/20160801042915/storage-concepts.png"),
                    new CardAction(ActionTypes.ImBack, "Uncooked", value: "https://azure.microsoft.com/en-us/services/storage/")),
                GetHeroCard(
                    "Cooked Bacon",
                    "Process events with Bacon",
                    "1 Pound of Bacon - Cooked",
                    new CardImage(url: "https://azurecomcdn.azureedge.net/cvt-8636d9bb8d979834d655a5d39d1b4e86b12956a2bcfdb8beb04730b6daac1b86/images/page/services/functions/azure-functions-screenshot.png"),
                    new CardAction(ActionTypes.PostBack, "Cooked", value: "https://azure.microsoft.com/en-us/services/functions/")),
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
    }
}