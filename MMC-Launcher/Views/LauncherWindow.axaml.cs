using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.MesterMC.Launcher.Helpers;
using Tavstal.MesterMC.Launcher.Models;
using Tavstal.MesterMC.Launcher.Views.Models;

namespace Tavstal.MesterMC.Launcher.Views;

[RequiresUnreferencedCode("This class uses code that may be removed during trimming.")]
public partial class LauncherWindow : KonkordWindow<LauncherViewModel>
{
    private CoreLogger _logger = CoreLogger.WithModuleType(typeof(LauncherWindow));
    
    [RequiresUnreferencedCode("This constructor uses code that may be removed during trimming.")]
    public LauncherWindow()
    {
        InitializeComponent();
        
#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif

        
        DataContext ??= new LauncherViewModel();
        // Temporal news item shown while loading, otherwise the content area would be empty
        /*DataContext.NewsItems.Add(new NewsModel(
            $"Béta Verzió - {App.Version} - {App.BuildDate}",
            "Üdvözlünk a MesterMC Launcher Béta verziójában! Kérlek vedd figyelembe, hogy ez még egy fejlesztés alatt álló kiadás, így előfordulhatnak hibák és hiányzó funkciók. Köszönjük a türelmed és támogatásod!" +
                   "\n\nAz alábbi funkciók szándékosan ki vannak kapcsolva a béta tesztelés idejére:" +
                   "\n- Hírek betöltése az internetről" +
                   "\n- Automatikus frissítések" +
                   "\n- Bejelentkezés a fiókba, csak offline mód érhető el, de így is tudsz csatlakozni a szerverre." +
                   "\n\nHa bármilyen problémába ütközöl, kérlek jelezd nekünk a Discord szerverünkön keresztül: https://discord.gg/mestermc" +
                   "\n\nFigyelem! Mivel a launcher még nem rendelkezik code signing tanúsítvánnyal, az antivírus szoftverek hamis pozitív eredményeket adhatnak. Kérlek győződj meg róla, hogy a letöltött fájl a hivatalos forrásból származik.",
            ImageHelper.LoadFromResource(new Uri("avares://MMC-Launcher/Assets/posts/post_image_beta.png"))
        ));*/
        DataContext.NewsItems.Add(new NewsModel(
            "Betöltés...",
            "Hírek betöltése folyamatban, kérlek várj...",
            ImageHelper.LoadFromResource(new Uri("avares://MMC-Launcher/Assets/posts/post_image_01.jpg"))
        ));
        DataContext.SelectedNewsItem = DataContext.NewsItems[0];
        DataContext.SelectedNewsIndex = 0;
        
        
        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.HideWindowInteraction.RegisterHandler(action =>
            {
                Hide();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.OpenFolderPicker.RegisterHandler(async action =>
            {
                var result = await OpenFolderPickerAsync();
                action.SetOutput(result);
            }).DisposeWith(disposables);
        });
        
        Dispatcher.UIThread.InvokeAsync(async () => await InitializeAsync());
    }
    
    private async Task InitializeAsync()
    {
        if (DataContext == null)
            return;
        
        try
        {
            var settings = await LauncherHelper.GetLauncherSettingsAsync();
            if (settings.Users.Count > 0)
            {
                var keys = settings.Users.Keys;
                if (keys.Count - 1 >= settings.LastUser && settings.LastUser >= 0)
                    DataContext.Username = keys.ElementAt(settings.LastUser);
                else
                    DataContext.Username = settings.Users.Keys.ElementAt(0);
            }
            
            var items = await LauncherHelper.GetNewsAsync(settings.Launcher.CacheDirectoryPath);
            
            if (items.Count == 0)
            {
                AddFallbackNews();
                return;
            }

            bool oldNewsRemoved = false;
            foreach (var item in items)
            {
                Bitmap? image;

                try
                {
                    _logger.Warn("Loading news image from URL: " + item.BannerUrl);
                    image = await ImageHelper.LoadFromWeb(new Uri(item.BannerUrl), _logger);
                    if (image == null)
                    {
                        _logger.Error("Failed to load news image from URL, using fallback image.");
                        ImageHelper.LoadFromResource(new Uri("avares://MMC-Launcher/Assets/posts/post_image_01.jpg"));
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to load news image.\n" + ex);
                    image = ImageHelper.LoadFromResource(new Uri("avares://MMC-Launcher/Assets/posts/post_image_01.jpg"));
                }

                if (!oldNewsRemoved)
                {
                    DataContext.NewsItems.Clear();
                    oldNewsRemoved = true;
                }
                DataContext.NewsItems.Add(new NewsModel(item.Title, item.Content, image));
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to load news items.\n" + ex);
            AddFallbackNews();
        }

        if (DataContext.NewsItems.Count > 0)
        {
            DataContext.SelectedNewsItem = DataContext.NewsItems[0];
            DataContext.SelectedNewsIndex = 0;
        }
    }
    
    private void AddFallbackNews()
    {
        if (DataContext == null)
            return;
        
        DataContext.NewsItems.Clear();
        DataContext.NewsItems.Add(new NewsModel(
            "Hiba",
            "Váratlan hiba történt a hírek betöltése során.",
            ImageHelper.LoadFromResource(new Uri("avares://MMC-Launcher/Assets/posts/post_image_01.jpg"))
        ));
    }


    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        FitToDisplay();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        App.ClearRPC();
    }

    private void DragStart_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Start moving the window when left mouse button is pressed
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }
    
    private void Username_GotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is ComboBox comboBox)
        {
            var textBox = comboBox.GetVisualDescendants()
                .OfType<TextBox>()
                .FirstOrDefault();
        
            if (textBox != null)
            {
                textBox.SelectionStart = textBox.Text?.Length ?? 0;
                textBox.SelectionEnd = textBox.SelectionStart;
            }
        }
    }
    
    private void FitToDisplay()
    {
        double screenWidth = (double)App.ScreenWidth;
        double screenHeight = (double)App.ScreenHeight;
        
        // Base design resolution
        double baseWidth = 1280;
        double baseHeight = 665;

        // Calculate scale factor relative to screen
        double scaleX = screenWidth / baseWidth;
        double scaleY = screenHeight / baseHeight;
        double scale = Math.Min(scaleX, scaleY) * 0.5;

        // Apply scaled window size
        Width = baseWidth * scale;
        Height = baseHeight * scale;

        // Center window on screen
        Position = new PixelPoint(
            (int)((screenWidth - Width) / 2),
            (int)((screenHeight - Height) / 2)
        );
    }
    
    private async Task<string?> OpenFolderPickerAsync()
    {
        // Ensure the VisualRoot is a TopLevel object
        if (VisualRoot is not TopLevel topLevel)
            return null;

        var storageProvider = topLevel.StorageProvider;

        // Check if folder picking is supported on the current platform
        if (!storageProvider.CanPickFolder)
        {
            _logger.Error("Folder picking is not supported on this platform.");
            return null;
        }
    
        // Configure folder picker options
        var options = new FolderPickerOpenOptions
        {
            Title = "Válassz egy mappát",
            AllowMultiple = false
        };

        // Open the folder picker dialog
        IReadOnlyList<IStorageFolder> folders = await storageProvider.OpenFolderPickerAsync(options);

        // Return null if no folders were selected
        if (!folders.Any())
            return null;
    
        // Get the first selected folder
        IStorageFolder? selectedFolder = folders.FirstOrDefault();
        if (selectedFolder == null)
        {
            _logger.Error("No folder was selected.");
            return null;
        }
    
        // Return the local path of the selected folder
        return selectedFolder.Path.LocalPath;
    }
}