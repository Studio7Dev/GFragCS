using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace GFrag
{
    internal class PageHandler
    {
        private bool _isHidden = false;
        private ContentPage _page;

        public PageHandler(ContentPage page)
        {
            _page = page;
        }

        public void ToggleVisibility()
        {
            if (_isHidden)
            {
                ShowAllElements();
            }
            else
            {
                HideAllElements();
            }

            _isHidden = !_isHidden;
        }

        public void HideAllElements()
        {
            if (_page.Content is Layout layout)
            {
                HideLayout(layout);
            }
            else if (_page.Content is VisualElement element)
            {
                element.IsVisible = false;
            }
        }

        private void HideLayout(Layout layout)
        {
            foreach (var child in layout.Children)
            {
                if (child is Layout childLayout)
                {
                    HideLayout(childLayout);
                }
                else if (child is VisualElement element)
                {
                    element.IsVisible = false;
                }
            }
        }

        public void ShowAllElements()
        {
            if (_page.Content is Layout layout)
            {
                ShowLayout(layout);
            }
            else if (_page.Content is VisualElement element)
            {
                element.IsVisible = true;
            }
        }

        private void ShowLayout(Layout layout)
        {
            foreach (var child in layout.Children)
            {
                if (child is Layout childLayout)
                {
                    ShowLayout(childLayout);
                }
                else if (child is VisualElement element)
                {
                    element.IsVisible = true;
                }
            }
        }
    }
}