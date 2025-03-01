using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;

namespace GFrag
{
    public static class BackgroundManager
    {
        public static string BGName { get; set; }

        public static void UpdateAllWebViews()
        {
            // Call Reload on all pages' WebViews
            foreach (var webView in WebViewRegistry)
            {
                webView.Reload();

                
            }
        }

        // Registry to keep track of all WebViews
        public static List<WebView> WebViewRegistry { get; } = new List<WebView>();
    }
}
