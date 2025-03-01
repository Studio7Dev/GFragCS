using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFrag
{
    public interface IAlertService
    {
        Task DisplayAlertAsync(string title, string message, string cancel);
    }

    public class AlertService : IAlertService
    {
        private readonly Page _page;

        public AlertService(Page page)
        {
            _page = page;
        }

        public async Task DisplayAlertAsync(string title, string message, string cancel)
        {
            await _page.DisplayAlert(title, message, cancel);
        }
    }

    public interface IActionSheetService
    {
        Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, params string[] buttons);
    }

    public class ActionSheetService : IActionSheetService
    {
        private readonly Page _page;

        public ActionSheetService(Page page)
        {
            _page = page;
        }

        public async Task<string> DisplayActionSheetAsync(string title, string cancel, string destruction, params string[] buttons)
        {
            return await _page.DisplayActionSheet(title, cancel, destruction, buttons);
        }
    }
}
