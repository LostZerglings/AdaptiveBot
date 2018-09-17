namespace SampleAdaptiveCard.Dialogs
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Web.Hosting;
    using AdaptiveCards;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // calculate something for us to return
            int length = (activity.Text ?? string.Empty).Length;

            //If user's message contains json load the json card, otherwise load the C# card. 
            if (activity.Text.ToLower().Contains("json"))
                await CreateAdapteiveCardFromJson(context, activity.Text);
            else
                await CreateAdaptiveCardInCSharp(context);

            context.Wait(MessageReceivedAsync);
        }

        private async Task CreateAdaptiveCardInCSharp(IDialogContext context)
        {
            //create the card
            AdaptiveCard adaptiveCard = new AdaptiveCard();

            //add a title
            adaptiveCard.Body.Add(new AdaptiveTextBlock()
            {
                Text = "Hello World",
                Weight = AdaptiveTextWeight.Bolder,
            });

            //add a body
            adaptiveCard.Body.Add(new AdaptiveTextBlock()
            {
                Text = "sample text goes here"
            });

            //create a comment card 
            AdaptiveCard commentCard = new AdaptiveCard();
            commentCard.Body.Add(new AdaptiveTextInput()
            {
                IsMultiline = true,
                Placeholder = "enter your comment here"
            });
            commentCard.Actions.Add(new AdaptiveOpenUrlAction()
            {
                //if you want to get information back from the card, use a submit action type
                //that will return a json value of the input type (id : value)
                Title = "OK",
                Url = new Uri("https://microsoft.com")
            });

            //add comment card to main card
            adaptiveCard.Actions.Add(new AdaptiveShowCardAction()
            {
                Title = "Add a comment",
                Id = "comment",
                Card = commentCard
            });

            //add a url action to the main card
            adaptiveCard.Actions.Add(new AdaptiveOpenUrlAction()
            {
                Title = "Visit Microsoft",
                Url = new Uri("https://microsoft.com")
            });

            //convert the card to an attachment
            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = adaptiveCard
            };

            //create the message, attach the attachment and send as a response.
            var message = context.MakeMessage();
            message.Attachments.Add(attachment);
            await context.PostAsync(message);
        }

        private async Task CreateAdapteiveCardFromJson(IDialogContext context, string text)
        {
            try
            {
                //This is the path to our card in the solution. if you had it in another folder you would map to that path ie ~\\Dialogs\\Cards\\SampleCard.json
                var path = HostingEnvironment.MapPath($"~\\Dialogs\\SampleCard.json");

                //verify the card exists
                if (!File.Exists(path))
                    await context.PostAsync("I'm sorry, a card at this path does not exist.");

                //open the file
                using (var file = File.OpenText(path))
                {
                    //convert the json to an adaptive card
                    var results = AdaptiveCard.FromJson(await file.ReadToEndAsync());
                    var card = results.Card;

                    //create a message to respond with and attach the adaptive card as an attachment
                    var message = context.MakeMessage();
                    message.Attachments.Add(new Attachment()
                    {
                        Content = card,
                        ContentType = AdaptiveCard.ContentType,
                        Name = "Card"
                    });

                    //post card
                    await context.PostAsync(message);

                }
            }
            catch (Exception error)
            {
                await context.PostAsync(error.ToString());
            }
        }
    }
}