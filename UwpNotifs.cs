using Microsoft.Toolkit.Uwp.Notifications;

namespace GFrag
{
    internal class UwpNotifs
    {
        public async Task SendNotification(string title, string message, string iconurl = "https://img.freepik.com/premium-photo/retro-computer-setup-with-crt-monitors-keyboard_14117-1102887.jpg")
        {
            var conversationId = new Random().Next(1, 10000000).ToString();
            new ToastContentBuilder()
             .AddArgument("action", "viewConversation")
             .AddArgument("conversationId", conversationId)
             .AddHeroImage(new Uri(iconurl))
             .AddText(title, hintMaxLines: 1)
             .AddText(message, hintMaxLines: 2)
             .AddButton(new ToastButton()
                 .SetContent("View")
                 .SetBackgroundActivation())
             //.AddButton(new ToastButton()
             //    .SetContent("Close")
             //    .SetDismissActivation())
             .Show();
        }
    }
}