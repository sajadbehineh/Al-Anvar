﻿using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml.Media;

namespace AlAnvar.UI.Pages;

public sealed partial class SettingsPage : Page
{
    private MainWindow mainWindow = MainWindow.Instance;
    private ShellPage shellPage = ShellPage.Instance;

    private Version CurrentVersion;

    private string ChangeLog = string.Empty;
    public SettingsPage()
    {
        this.InitializeComponent();
        ThemeHelper.SetThemeRadioButtonChecked(ThemePanel);
        Loaded += SettingsPage_Loaded;
    }

    private void SettingsPage_Loaded(object sender, RoutedEventArgs e)
    {
        var assembly = typeof(App).GetTypeInfo().Assembly;
        var assemblyVersion = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
        CurrentVersion = new Version(assemblyVersion);
        txtCurrentVersion.Header = $"نسخه فعلی {CurrentVersion}";
        txtLastUpdateChecke.Text = Settings.LastUpdateCheck;
        btnOpenDefaultFolderPath.Content = Settings.AppFolderPath;

        Task.Run(() => {
            LoadFontsInCombobox();
            GetDefaultColors();
            GetDefaultFonts();
            LoadTranslationsInCombobox();
            LoadQarisInCombobox();
            LoadTafsirInCombobox();
        });
    }
    private void LoadTafsirInCombobox()
    {
        DispatcherQueue.TryEnqueue(async () =>
        {
            using var db = new AlAnvarDBContext();
            cmbTafsir.ItemsSource = await db.TafsirNames.ToListAsync();
            cmbTafsir.SelectedItem = cmbTafsir.Items.Where(trans => ((TafsirName) trans).Name == Settings.QuranTafsir?.Name)?.FirstOrDefault();
        });
    }
    private void cmbTafsir_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Settings.QuranTafsir = cmbTafsir.SelectedItem as TafsirName;
    }
    private void OnThemeRadioButtonChecked(object sender, RoutedEventArgs e)
    {
        ThemeHelper.OnThemeRadioButtonChecked(sender);
    }

    private async void WindowsColorSettings_Click(object sender, RoutedEventArgs e)
    {
        _= await Launcher.LaunchUriAsync(new Uri("ms-settings:colors"));
    }

    private void LoadFontsInCombobox()
    {
        DispatcherQueue.TryEnqueue(() => {
            cmbFonts.Items.Clear();
            if (chkSystemFonts.IsChecked.Value)
            {
                var systemFonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies();
                foreach (var item in systemFonts)
                {
                    cmbFonts.Items.Add(item);
                }
            }
            else
            {
                //Todo:
                cmbFonts.Items.Add("وزیرمتن رگولار");
                cmbFonts.Items.Add("وزیرمتن مدیوم");
                cmbFonts.Items.Add("وزیرمتن بولد");
            }
        });
    }

    private void chkSystemFonts_Checked(object sender, RoutedEventArgs e)
    {
        LoadFontsInCombobox();
    }

    private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
    {
        var selectedColor = new SolidColorBrush(args.NewColor);
        if (rbAyat.IsChecked.Value)
        {
            txtAyat.Foreground = selectedColor;
            Settings.AyatForeground = args.NewColor.ToString();
        }
        else if (rbAyatNumber.IsChecked.Value)
        {
            txtAyatNumber.Foreground = selectedColor;
            Settings.AyatNumberForeground = args.NewColor.ToString();
        }
        else if (rbTranslation.IsChecked.Value)
        {
            txtTranslation.Foreground = selectedColor;
            Settings.TranslationForeground = args.NewColor.ToString();
        }
    }
    private void GetDefaultColors()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (Settings.AyatForeground is not null)
            {
                txtAyat.Foreground = new SolidColorBrush(ColorHelper.ToColor(Settings.AyatForeground));
            }

            if (Settings.AyatNumberForeground is not null)
            {
                txtAyatNumber.Foreground = new SolidColorBrush(ColorHelper.ToColor(Settings.AyatNumberForeground));
            }

            if (Settings.TranslationForeground is not null)
            {
                txtTranslation.Foreground = new SolidColorBrush(ColorHelper.ToColor(Settings.TranslationForeground));
            }
        });
    }

    private void LoadTranslationsInCombobox()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (Directory.Exists(Settings.TranslationsPath))
            {
                var files = Directory.GetFiles(Settings.TranslationsPath, "*.ini", SearchOption.AllDirectories);
                if (files.Count() > 0)
                {
                    foreach (var file in files)
                    {
                        var trans = file.DeserializeFromJson<QuranTranslation>();
                        if (trans is not null)
                        {
                            cmbTranslators.Items.Add(trans);
                        }
                    }
                }

                cmbTranslators.SelectedItem = cmbTranslators.Items.Where(trans => ((QuranTranslation) trans).TranslationId == Settings.QuranTranslation?.TranslationId).FirstOrDefault();
            }
        });
    }
    private void LoadQarisInCombobox()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (Directory.Exists(Settings.AudiosPath))
            {
                var files = Directory.GetFiles(Settings.AudiosPath, "*.ini", SearchOption.AllDirectories);
                if (files.Count() > 0)
                {
                    foreach (var file in files)
                    {
                        var audios = file.DeserializeFromJson<QuranAudio>();
                        if (audios is not null)
                        {
                            cmbQaris.Items.Add(audios);
                        }
                    }
                }

                cmbQaris.SelectedItem = cmbQaris.Items.Where(audio => ((QuranAudio) audio).DirName == Settings.QuranAudio?.DirName).FirstOrDefault();
            }
        });
    }
    private void GetDefaultFonts()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            txtAyat2.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily(Settings.AyatFontFamilyName);
            txtAyatNumber2.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily(Settings.AyatNumberFontFamilyName);
            txtTranslation2.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily(Settings.TranslationFontFamilyName);

            txtAyat2.FontSize = Settings.AyatFontSize;
            txtAyatNumber2.FontSize = Settings.AyatNumberFontSize;
            txtTranslation2.FontSize = Settings.TranslationFontSize;
        });
    }
    private void btnResetColors_Click(object sender, RoutedEventArgs e)
    {
        Settings.AyatForeground = null;
        Settings.AyatNumberForeground = null;
        Settings.TranslationForeground = null;
        GetDefaultColors();
    }

    private void cmbFonts_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedFont = cmbFonts.SelectedItem;
        FontFamily fontFamily;
        if (chkSystemFonts.IsChecked.Value)
        {
            fontFamily = new FontFamily(selectedFont as string);
        }
        else
        {
            fontFamily = GetFontFamilyFromFontName(selectedFont as string);
        }

        switch (rbFont.SelectedIndex)
        {
            case 0:
                txtAyat2.FontFamily = fontFamily;
                Settings.AyatFontFamilyName = selectedFont as string;
                break;
            case 1:
                txtTranslation2.FontFamily = fontFamily;
                Settings.TranslationFontFamilyName = selectedFont as string;
                break;
            case 2:
                txtAyatNumber2.FontFamily = fontFamily;
                Settings.AyatNumberFontFamilyName = selectedFont as string;
                break;
        }
    }
    private FontFamily GetFontFamilyFromFontName(string fontname)
    {
        switch (fontname)
        {
            case "وزیرمتن رگولار":
                return new FontFamily(Constants.FONT_VAZIRMATN_REGULAR);
            case "وزیرمتن مدیوم":
                return new FontFamily(Constants.FONT_VAZIRMATN_MEDIUM);
            case "وزیرمتن بولد":
                return new FontFamily(Constants.FONT_VAZIRMATN_BOLD);
        }
        return new FontFamily(Constants.FONT_VAZIRMATN_REGULAR);
    }
    private void nbFontSize_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if (rbFont == null)
        {
            return;
        }
        switch (rbFont.SelectedIndex)
        {
            case 0:
                txtAyat2.FontSize = args.NewValue;
                Settings.AyatFontSize = args.NewValue;
                break;
            case 1:
                txtTranslation2.FontSize = args.NewValue;
                Settings.TranslationFontSize = args.NewValue;
                break;
            case 2:
                txtAyatNumber2.FontSize = args.NewValue;
                Settings.AyatNumberFontSize = args.NewValue;
                break;
        }
    }

    private void btnResetFont_Click(object sender, RoutedEventArgs e)
    {
        Settings.AyatFontFamilyName = Constants.AYAT_DEFAULT_FONT_NAME;
        Settings.AyatNumberFontFamilyName = Constants.AYAT_NUMBER_DEFAULT_FONT_NAME;
        Settings.TranslationFontFamilyName = Constants.TRANSLATION_DEFAULT_FONT_NAME;
        Settings.AyatFontSize = Constants.AYAT_DEFAULT_FONT_SIZE;
        Settings.AyatNumberFontSize = Constants.AYAT_NUMBER_DEFAULT_FONT_SIZE;
        Settings.TranslationFontSize = Constants.TRANSLATION_DEFAULT_FONT_SIZE;
        GetDefaultFonts();
    }

    private void rbColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        switch (rbColor.SelectedIndex)
        {
            case 0:
                if (Settings.AyatForeground is not null)
                {
                    colorPicker.Color = ColorHelper.ToColor(Settings.AyatForeground);
                }
                break;
            case 1:
                if (Settings.TranslationForeground is not null)
                {
                    colorPicker.Color = ColorHelper.ToColor(Settings.TranslationForeground);
                }
                break;
            case 2:
                if (Settings.AyatNumberForeground is not null)
                {
                    colorPicker.Color = ColorHelper.ToColor(Settings.AyatNumberForeground);
                }
                break;
        }
    }

    private void rbFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        switch (rbFont.SelectedIndex)
        {
            case 0:
                double ayatFontSize = Settings.AyatFontSize;
                txtAyat2.FontSize = ayatFontSize;
                nbFontSize.Value = ayatFontSize;
                txtAyat2.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily(Settings.AyatFontFamilyName);
                cmbFonts.SelectedItem = Settings.AyatFontFamilyName;
                break;
            case 1:
                double translationFontSize = Settings.TranslationFontSize;
                txtTranslation2.FontSize = translationFontSize;
                nbFontSize.Value = translationFontSize;
                txtTranslation2.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily(Settings.TranslationFontFamilyName);
                cmbFonts.SelectedItem = Settings.TranslationFontFamilyName;
                break;
            case 2:
                double ayatNumberFontSize = Settings.AyatNumberFontSize;
                txtAyatNumber2.FontSize = ayatNumberFontSize;
                nbFontSize.Value = ayatNumberFontSize;
                txtAyatNumber2.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily(Settings.AyatNumberFontFamilyName);
                cmbFonts.SelectedItem = Settings.AyatNumberFontFamilyName;
                break;
        }
    }

    private void cmbTranslators_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Settings.QuranTranslation = cmbTranslators.SelectedItem as QuranTranslation;
    }

    private void btnMoreTranslation_Click(object sender, RoutedEventArgs e)
    {
        shellPage.Navigate(typeof(TranslationPage));
    }

    private async void btnCheckUpdate_Click(object sender, RoutedEventArgs e)
    {
        if (GeneralHelper.IsNetworkAvailable())
        {
            txtLastUpdateChecke.Text = DateTime.Now.ToShortDateString();
            Settings.LastUpdateCheck = DateTime.Now.ToShortDateString();

            downloadPanel.Children.Clear();
            updateErrorInfo.IsOpen = false;
            updateDownloadInfo.IsOpen = false;
            updateInfo.IsOpen = false;
            prgUpdate.IsActive = true;
            txtUpdate.Visibility = Visibility.Visible;
            try
            {
                var update = await UpdateHelper.CheckUpdateAsync("ghost1372", "Al-Anvar", CurrentVersion);
                if (update.IsExistNewVersion)
                {
                    txtReleaseNote.Visibility = Visibility.Visible;
                    ChangeLog = update.Changelog;
                    updateDownloadInfo.Message = $"ما یک نسخه جدید پیدا کردیم {update.TagName} در تاریخ {update.CreatedAt} ایجاد و در تاریخ {update.PublishedAt} منتشر شده است.";
                    foreach (var item in update.Assets)
                    {
                        var btn = new Button
                        {
                            Content = $"دانلود {Path.GetFileName(item.Url).Replace("AlAnvar.Package._", "")}",
                            MinWidth = 300,
                            Margin = new Thickness(10)
                        };

                        btn.Click += async (s, e) =>
                        {
                            await Launcher.LaunchUriAsync(new Uri(item.Url));
                        };

                        downloadPanel.Children.Add(btn);
                    }

                    updateDownloadInfo.IsOpen = true;
                }
                else
                {
                    updateInfo.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                updateErrorInfo.Title = null;
                updateErrorInfo.Message = ex.Message;
                updateErrorInfo.IsOpen = true;
            }

            prgUpdate.IsActive = false;
            txtUpdate.Visibility = Visibility.Collapsed;
        }
        else
        {
            updateErrorInfo.Title = "خطا در اتصال";
            updateErrorInfo.Message = "شما به اینترنت متصل نیستید و یا ارتباط فعالی وجود ندارد. بعدا دوباره امتحان کنید.";
            updateErrorInfo.IsOpen = true;
        }
    }

    private async void txtReleaseNote_Click(object sender, RoutedEventArgs e)
    {
        ContentDialog dialog = new ContentDialog()
        {
            Title = "یادداشت انتشار",
            CloseButtonText = "بستن",
            Content = new ScrollViewer { Content = new MarkdownTextBlock { Text = ChangeLog }, Margin = new Thickness(10) },
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = Content.XamlRoot
        };

        await dialog.ShowAsyncQueue();
    }

    private void cmbQaris_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Settings.QuranAudio = cmbQaris.SelectedItem as QuranAudio;
    }

    private void btnMoreQaris_Click(object sender, RoutedEventArgs e)
    {
        shellPage.Navigate(typeof(QariPage));
    }

    private async void btnOpenDefaultFolderPath_Click(object sender, RoutedEventArgs e)
    {
        string folderPath = btnOpenDefaultFolderPath.Content.ToString();
        StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(folderPath);
        await Launcher.LaunchFolderAsync(folder);
    }

    private async void btnChooseFolder_Click(object sender, RoutedEventArgs e)
    {
        FolderPicker folderPicker = new();
        folderPicker.FileTypeFilter.Add("*");

        WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, WindowHelper.GetWindowHandleForCurrentWindow(mainWindow));

        StorageFolder folder = await folderPicker.PickSingleFolderAsync();
        if (folder is not null)
        {
            Settings.AppFolderPath = Path.Combine(folder.Path, Constants.AppName);
            mainWindow.SetDefaultFoldersPath();
            btnOpenDefaultFolderPath.Content = Settings.AppFolderPath;
        }
    }
}
