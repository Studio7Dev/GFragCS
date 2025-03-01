using MauiIcons.Material.Outlined;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.Maui.LifecycleEvents;
using Windows.Graphics;

namespace GFrag
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
               .UseMauiApp<App>()
               .ConfigureFonts(fonts =>
               {
                   fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                   fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                   fonts.AddFont("Sans.ttf", "Sans");
               })
               .UseMaterialOutlinedMauiIcons()
               .UseMauiCommunityToolkit()
               .ConfigureMauiHandlers(handlers =>
               {

                   SwitchHandler.Mapper.AppendToMapping("Custom", (h, _) =>
                   {
#if WINDOWS
    h.PlatformView.OffContent = new Label().Text="";
    h.PlatformView.OnContent = new Label().Text="";
    h.PlatformView.Margin = new Microsoft.UI.Xaml.Thickness(100, 0, -100, 0);

#endif
                       // You can customize the switch control here, but you cannot set OffContent, OnContent, or MinWidth directly.
                       // You may need to use a custom renderer or a different approach to achieve your desired behavior.
                   });

               });



#if WINDOWS

            builder.ConfigureLifecycleEvents(events =>
            {
                events.AddWindows(lifeCycleBuilder =>
                {
                    lifeCycleBuilder.OnWindowCreated(w =>
                    {
                        w.ExtendsContentIntoTitleBar = false;
                        IntPtr wHandle = WinRT.Interop.WindowNative.GetWindowHandle(w);
                        WindowId windowId = Win32Interop.GetWindowIdFromWindow(wHandle);
                        AppWindow mauiWindow = AppWindow.GetFromWindowId(windowId);
                        mauiWindow.SetPresenter(AppWindowPresenterKind.Overlapped);  // TO SET THE APP INTO FULL SCREEN


                        //OR USE THIS LINE FOR YOUR CUSTOM POSITION
                        //  mauiWindow.MoveAndResize(YOUR DESIRED HOTIZONTAL POSITION, YOUR DESIRED VERTICAL POSITION, YOUR DESIRED WINDOW WIDTH, YOUR DESIRED WINDOW HEIGHT) ;
                    });
                });
            });

#endif


            //#if DEBUG
            builder.Logging.AddDebug();
//#endif

            return builder.Build();
        }
    }
}