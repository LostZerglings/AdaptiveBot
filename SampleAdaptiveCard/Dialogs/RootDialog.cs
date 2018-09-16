namespace SampleAdaptiveCard.Dialogs
{
    using System;
    using System.Threading.Tasks;
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
            await context.PostAsync("I'm sorry this code hasn't been added yet... Please check back later.");
        }
    }
}