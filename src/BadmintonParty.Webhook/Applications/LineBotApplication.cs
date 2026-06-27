using Line.Messaging.Webhooks;
using Line.Messaging;

namespace BadmintonParty.Webhook.Applications
{
    public interface ILineBotApplication
    {
        public Task RunAsync(IEnumerable<WebhookEvent> webhookEvents);
    }

    public class LineBotApplication : WebhookApplication, ILineBotApplication
    {
        private readonly LineMessagingClient _messagingClient;
        private readonly TextEventHandler _textEventHandler;
        public LineBotApplication(LineMessagingClient messagingClient, TextEventHandler textEventHandler) 
        {
            _messagingClient = messagingClient;
            _textEventHandler = textEventHandler;
        }

        protected override async Task OnMessageAsync(MessageEvent ev)
        {
            List<ISendMessage> result = new List<ISendMessage>();

            string userId = ev.Source.UserId;
            string channelId = ev.Source.Id;

            switch (ev.Message)
            {
                case TextEventMessage textMessage:
                    await _textEventHandler.SetReply(userId, textMessage.Text, result);
                    break;
            }

            if (result.Count != 0)
                await _messagingClient.ReplyMessageAsync(ev.ReplyToken, result);
        }
    }
}
