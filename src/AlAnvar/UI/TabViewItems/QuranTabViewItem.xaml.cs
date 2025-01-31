﻿using Downloader;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Printing;
using Windows.Media.Core;
using PrintHelper = SettingsUI.Helpers.PrintHelper;
using PrintHelperOptions = SettingsUI.Helpers.PrintHelperOptions;

namespace AlAnvar.UI.TabViewItems;

public sealed partial class QuranTabViewItem : TabViewItem, INotifyPropertyChanged
{
    #region DependencyProperty
    public static readonly DependencyProperty ChapterProperty =
        DependencyProperty.Register("Chapter", typeof(ChapterProperty), typeof(QuranTabViewItem),
        new PropertyMetadata(null));

    public ChapterProperty Chapter
    {
        get { return (ChapterProperty) GetValue(ChapterProperty); }
        set { SetValue(ChapterProperty, value); }
    }
    #endregion

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    #region Property

    #region Translation
    private SolidColorBrush _TranslationForeground;

    public SolidColorBrush TranslationForeground
    {
        get { return _TranslationForeground; }
        set
        {
            _TranslationForeground = value;
            OnPropertyChanged();
        }
    }
    private FontFamily _TranslationFontFamily;

    public FontFamily TranslationFontFamily
    {
        get { return _TranslationFontFamily; }
        set
        {
            _TranslationFontFamily = value;
            OnPropertyChanged();
        }
    }
    private double _TranslationFontSize;

    public double TranslationFontSize
    {
        get { return _TranslationFontSize; }
        set
        {
            _TranslationFontSize = value;
            OnPropertyChanged();
        }
    }
    #endregion

    #region Ayat
    private SolidColorBrush _AyatForeground;

    public SolidColorBrush AyatForeground
    {
        get { return _AyatForeground; }
        set
        {
            _AyatForeground = value;
            OnPropertyChanged();
        }
    }
    private FontFamily _AyatFontFamily;

    public FontFamily AyatFontFamily
    {
        get { return _AyatFontFamily; }
        set
        {
            _AyatFontFamily = value;
            OnPropertyChanged();
        }
    }
    private double _AyatFontSize;

    public double AyatFontSize
    {
        get { return _AyatFontSize; }
        set
        {
            _AyatFontSize = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Ayat Number
    private SolidColorBrush _AyatNumberForeground;

    public SolidColorBrush AyatNumberForeground
    {
        get { return _AyatNumberForeground; }
        set
        {
            _AyatNumberForeground = value;
            OnPropertyChanged();
        }
    }
    private FontFamily _AyatNumberFontFamily;

    public FontFamily AyatNumberFontFamily
    {
        get { return _AyatNumberFontFamily; }
        set
        {
            _AyatNumberFontFamily = value;
            OnPropertyChanged();
        }
    }

    private double _AyatNumberFontSize;
    public double AyatNumberFontSize
    {
        get { return _AyatNumberFontSize; }
        set
        {
            _AyatNumberFontSize = value;
            OnPropertyChanged();
        }
    }

    #endregion

    private bool _IsTranslationAvailable = true;

    public bool IsTranslationAvailable
    {
        get { return _IsTranslationAvailable; }
        set
        {
            _IsTranslationAvailable = value;
            OnPropertyChanged();
        }
    }

    private bool _IsSurahTextAvailable = true;

    public bool IsSurahTextAvailable
    {
        get { return _IsSurahTextAvailable; }
        set
        {
            _IsSurahTextAvailable = value;
            OnPropertyChanged();
        }
    }

    private int _ayaCountInPage;

    public int AyaCountInPage
    {
        get { return _ayaCountInPage; }
        set
        {
            _ayaCountInPage = value;
            OnPropertyChanged();
        }
    }

    private int _ayaFrom;

    public int AyaFrom
    {
        get { return _ayaFrom; }
        set
        {
            _ayaFrom = value;
            OnPropertyChanged();
        }
    }

    private int _ayaTo;

    public int AyaTo
    {
        get { return _ayaTo; }
        set
        {
            _ayaTo = value;
            OnPropertyChanged();
        }
    }
    #endregion

    public ObservableCollection<QuranItem> QuranCollection { get; set; } = new ObservableCollection<QuranItem>();
    private List<TranslationItem> TranslationCollection { get; set; } = new List<TranslationItem>();
    private List<Quran> AyahCollection { get; set; } = new List<Quran>();
    private MainPage mainPage = MainPage.Instance;
    private ShellPage shellPage = ShellPage.Instance;
    private PrintHelper _printHelper;
    internal static QuranTabViewItem Instance { get; private set; }

    #region MediaPlayer
    List<AudioModel> audioList = new List<AudioModel>();
    private bool CanPlay = true;
    private DownloadService downloadService;
    private AudioModel ayaSound;
    #endregion
    public QuranTabViewItem()
    {
        this.InitializeComponent();
        Instance = this;
        DataContext = this;
        Loaded += QuranTabViewItem_Loaded;
        CloseRequested += QuranTabViewItem_CloseRequested;
        mediaPlayerElement.MediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
    }

    private void QuranTabViewItem_CloseRequested(TabViewItem sender, TabViewTabCloseRequestedEventArgs args)
    {
        btnStop_Click(null, null);
    }

    private async void QuranTabViewItem_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Run(() => {
            GetDefaultForeground();
            GetDefaultFont();
            GetSurahFromDB();
            GetTranslationText();
            GetSuraText();
            LoadTranslationsInCombobox();
            LoadQarisInCombobox();
        });
        prgLoading.IsActive = false;
    }
    private void GetDefaultForeground()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (Settings.AyatForeground is not null)
            {
                AyatForeground = new SolidColorBrush(ColorHelper.ToColor(Settings.AyatForeground));
            }
            if (Settings.AyatNumberForeground is not null)
            {
                AyatNumberForeground = new SolidColorBrush(ColorHelper.ToColor(Settings.AyatNumberForeground));
            }
            if (Settings.TranslationForeground is not null)
            {
                TranslationForeground = new SolidColorBrush(ColorHelper.ToColor(Settings.TranslationForeground));
            }
        });
    }
    private void GetDefaultFont()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            AyatFontFamily = new FontFamily(Settings.AyatFontFamilyName);
            AyatNumberFontFamily = new FontFamily(Settings.AyatNumberFontFamilyName);
            TranslationFontFamily = new FontFamily(Settings.TranslationFontFamilyName);

            AyatFontSize = Settings.AyatFontSize;
            AyatNumberFontSize = Settings.AyatNumberFontSize;
            TranslationFontSize = Settings.TranslationFontSize;
        });
    }
    private void GetSurahFromDB()
    {
        DispatcherQueue.TryEnqueue(async () =>
        {
            AyahCollection?.Clear();
            using var db = new AlAnvarDBContext();
            AyahCollection = await db.Qurans.Where(x => x.SurahId == Chapter.Id).ToListAsync();
        });
    }
    public void GetSuraText(bool isTranslationAvailable = true)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            QuranCollection?.Clear();

            foreach (var item in AyahCollection)
            {
                QuranCollection.Add(new QuranItem
                {
                    Audio = item.Audio,
                    AyahNumber = item.AyahNumber,
                    AyahText = item.AyahText,
                    Hizb = item.Hizb,
                    Id = item.Id,
                    Juz = item.Juz,
                    SurahId = Chapter.Id,
                    SurahName = Chapter.Name,
                    TotalAyah = Chapter.Ayas,
                    AyaDetail = $"({item.AyahNumber}:{Chapter.Ayas})",
                    TranslationText = isTranslationAvailable ? TranslationCollection.Where(x => x.SurahId == Chapter.Id && x.Aya == item.AyahNumber).FirstOrDefault()?.Translation : null
                });
            }
            quranListView.ItemsSource = QuranCollection;
        });
    }

    #region Translation
    public void GetTranslationText()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            TranslationCollection = new(GetQuranTranslation(cmbTranslators));
            if (TranslationCollection.Count == 0)
            {
                IsTranslationAvailable = false;
            }
        });
    }

    private void LoadTranslationsInCombobox()
    {
        if (Directory.Exists(Settings.TranslationsPath))
        {
            var items = new ObservableCollection<QuranTranslation>();
            var files = Directory.GetFiles(Settings.TranslationsPath, "*.ini", SearchOption.AllDirectories);
            if (files.Count() > 0)
            {
                foreach (var file in files)
                {
                    var trans = file.DeserializeFromJson<QuranTranslation>();
                    if (trans is not null)
                    {
                        items.Add(trans);
                    }
                }
                DispatcherQueue.TryEnqueue(() => {
                    cmbTranslators.ItemsSource = items;
                    cmbTranslators.SelectedItem = cmbTranslators.Items.Where(trans => ((QuranTranslation) trans).TranslationId == Settings.QuranTranslation?.TranslationId).FirstOrDefault();
                });
            }
        }
    }
    private void cmbTranslators_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Settings.QuranTranslation = cmbTranslators.SelectedItem as QuranTranslation;

        var itemIndex = GetListViewSelectedIndex();
        TranslationCollection = new(GetQuranTranslation(cmbTranslators));
        GetSuraText();
        ScrollIntoView(itemIndex);
    }
    private void chkOnlyTranslationText_Checked(object sender, RoutedEventArgs e)
    {
        IsTranslationAvailable = !chkOnlyTranslationText.IsChecked.Value;
    }
    #endregion

    #region Qari
    private void LoadQarisInCombobox()
    {
        if (Directory.Exists(Settings.AudiosPath))
        {
            var items = new ObservableCollection<QuranAudio>();
            var files = Directory.GetFiles(Settings.AudiosPath, "*.ini", SearchOption.AllDirectories);
            if (files.Count() > 0)
            {
                foreach (var file in files)
                {
                    var audio = file.DeserializeFromJson<QuranAudio>();
                    if (audio is not null)
                    {
                        items.Add(audio);
                    }
                }
                DispatcherQueue.TryEnqueue(() => {
                    cmbQari.ItemsSource = items;
                    cmbQari.SelectedItem = cmbQari.Items.Where(trans => ((QuranAudio) trans).DirName == Settings.QuranAudio?.DirName).FirstOrDefault();
                });
            }
        }
    }
    private void cmbQari_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        audioList?.Clear();
        var qari = cmbQari.SelectedItem as QuranAudio;

        Settings.QuranAudio = qari;

        var qariPath = Path.Combine(Settings.AudiosPath, qari.DirName);

        if (Directory.Exists(qariPath))
        {
            var audios = Directory.GetFiles(qariPath, "*.mp3");

            foreach (var audio in audios)
            {
                var fileName = Path.GetFileNameWithoutExtension(audio);
                var surah = Convert.ToInt32(fileName.Substring(0, 3));
                var aya = Convert.ToInt32(fileName.Substring(3));
                audioList.Add(new AudioModel { SurahId = surah, AyaId = aya, FileName = fileName, FullPath = audio });
            }
        }
    }
    #endregion

    #region ListView
    private void quranListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var item = quranListView.SelectedItem as QuranItem;
        if (item is not null)
        {
            SetSurahStatus($"سوره: {item.SurahName} - آیه: {item.AyahNumber}");
            UpdateMediaPlayerButtons(quranListView.SelectedIndex, quranListView.Items.Count - 1);
        }
        ScrollIntoView(quranListView.SelectedIndex);
    }
    private void quranListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        menuFlyout.ShowAt(quranListView, e.GetPosition(quranListView));
    }
    public int GetListViewLastIndex()
    {
        return quranListView.Items.Count - 1;
    }
    public QuranItem GetListViewSelectedItem()
    {
        return quranListView.SelectedItem as QuranItem;
    }
    public void GoToListViewNextItem()
    {
        var currentIndex = quranListView.SelectedIndex;
        if (currentIndex == -1)
        {
            quranListView.SelectedIndex = 0;
        }
        else if (currentIndex != quranListView.Items.Count - 1)
        {
            quranListView.SelectedIndex += 1;
        }
    }

    public int GetListViewSelectedIndex()
    {
        return quranListView.SelectedIndex;
    }
    public void GoToListViewPreviousItem()
    {
        var currentIndex = quranListView.SelectedIndex;
        if (currentIndex == -1)
        {
            quranListView.SelectedIndex = 0;
        }
        else if (currentIndex != 0)
        {
            quranListView.SelectedIndex -= 1;
        }
    }
    public void ScrollIntoView(int index)
    {
        quranListView.SelectedIndex = index;
        quranListView.ScrollIntoView(quranListView.SelectedItem);
    }
    #endregion

    #region Downloader
    private void Downloader_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            audioList.Add(ayaSound);
            prgDownload.Value = 0;
        });
    }
    private void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            prgDownload.Value = e.ProgressPercentage;
        });
    }

    #endregion

    #region MediaPlayer
    private void MediaPlayer_MediaEnded(Windows.Media.Playback.MediaPlayer sender, object args)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            // Go To Play Next File
            if (!chkRepeat.IsChecked.Value)
            {
                GoToListViewNextItem();
            }
            else
            {
                CanPlay = true;
            }

            if (CanPlay)
            {
                PlayQuran();
            }

            var currentIndex = GetListViewSelectedIndex();
            var lastIndex = GetListViewLastIndex();

            if (currentIndex == lastIndex)
            {
                btnStop.IsEnabled = false;
                btnPlay.IsEnabled = true;
            }
        });
    }
    public void SetSurahStatus(string status)
    {
        txtStatus.Text = status;
    }

    private void chkOnlyAyaText_Checked(object sender, RoutedEventArgs e)
    {
        IsSurahTextAvailable = !chkOnlyAyaText.IsChecked.Value;
    }

    #region Buttons
    private void LoadMediaFromString(string path)
    {
        try
        {
            Uri pathUri = new Uri(path);
            mediaPlayerElement.Source = MediaSource.CreateFromUri(pathUri);
        }
        catch (Exception ex)
        {
            if (ex is FormatException)
            {
                // handle exception.
                // For example: Log error or notify user problem with file
            }
        }
    }
    private void btnStop_Click(object sender, RoutedEventArgs e)
    {
        mediaPlayerElement.Source = null;
        if (downloadService is not null)
        {
            downloadService.CancelAsync();
        }

        if (Temp.downloadService_Temp is not null)
        {
            Temp.downloadService_Temp?.CancelAsync();
        }

        btnPlay.IsEnabled = true;
        btnStop.IsEnabled = false;
    }
    private void btnPrevious_Click(object sender, RoutedEventArgs e)
    {
        btnStop_Click(null, null);
        GoToListViewPreviousItem();
        PlayQuran();
    }
    private void btnNext_Click(object sender, RoutedEventArgs e)
    {
        btnStop_Click(null, null);
        GoToListViewNextItem();
        PlayQuran();
    }
    private async void btnPlay_Click(object sender, RoutedEventArgs e)
    {
        btnStop_Click(null, null);
        var qari = cmbQari.SelectedItem as QuranAudio;
        if (qari is not null)
        {
            if (GetListViewSelectedIndex() == -1)
            {
                GoToListViewNextItem();
            }

            PlayQuran();
        }
        else
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = "قاری انتخاب نشده",
                CloseButtonText = "بستن",
                Content = new ScrollViewer { Content = "به صفحه قاری ها رفته و حداقل صوت یک قاری را دانلود کنید.", Margin = new Thickness(10) },
                DefaultButton = ContentDialogButton.Primary,
                PrimaryButtonText = "دانلود صوت قاری",
                XamlRoot = mainPage.XamlRoot
            };

            var result = await dialog.ShowAsyncQueue();
            if (result == ContentDialogResult.Primary)
            {
                shellPage.Navigate(typeof(QariPage));
            }
        }
    }
    public async void PlayQuran()
    {
        var selectedItem = GetListViewSelectedItem();
        ayaSound = audioList.Where(x => x.SurahId == selectedItem.SurahId && x.AyaId == selectedItem.AyahNumber)?.FirstOrDefault();

        if (ayaSound is null)
        {
            if (Settings.IsAutoDownloadSound && GeneralHelper.IsNetworkAvailable())
            {
                if (downloadService is not null && downloadService.IsCancelled)
                {
                    return;
                }
                else
                {
                    btnStop.IsEnabled = true;
                    btnPlay.IsEnabled = false;
                    var audioUrl = Path.Combine(Settings.QuranAudio.Url, $"{selectedItem.Audio}.mp3");
                    var dirPath = Path.Combine(Settings.AudiosPath, Settings.QuranAudio.DirName);

                    ayaSound = new AudioModel { AyaId = Convert.ToInt32(selectedItem.Audio.Substring(3)), SurahId = selectedItem.SurahId, FileName = selectedItem.Audio, FullPath = $@"{dirPath}\{selectedItem.Audio}.mp3" };
                    downloadService = new DownloadService();
                    Temp.downloadService_Temp = downloadService;
                    downloadService.DownloadFileCompleted += Downloader_DownloadFileCompleted;
                    downloadService.DownloadProgressChanged += Downloader_DownloadProgressChanged;
                    await downloadService.DownloadFileTaskAsync(audioUrl, new DirectoryInfo(dirPath));
                }
            }
            else
            {
                return;
            }
        }

        var currentIndex = GetListViewSelectedIndex();
        var lastIndex = GetListViewLastIndex();

        if (currentIndex == lastIndex)
        {
            CanPlay = false;
        }

        btnStop.IsEnabled = true;
        btnPlay.IsEnabled = false;
        LoadMediaFromString(ayaSound.FullPath);
        mediaPlayerElement.MediaPlayer.Play();
    }
    public void UpdateMediaPlayerButtons(int currentIndex, int lastIndex)
    {
        if (currentIndex == -1)
        {
            btnNext.IsEnabled = false;
            btnPrevious.IsEnabled = false;
        }
        else if (currentIndex == 0)
        {
            btnNext.IsEnabled = true;
            btnPrevious.IsEnabled = false;
        }
        else if (currentIndex == lastIndex)
        {
            btnNext.IsEnabled = false;
            btnPrevious.IsEnabled = true;
        }
        else
        {
            btnNext.IsEnabled = true;
            btnPrevious.IsEnabled = true;
        }
    }
    #endregion

    #endregion

    private void mnuToolbar_Click(object sender, RoutedEventArgs e)
    {
        switch ((sender as AppBarButton).Tag)
        {
            case "GoToFont":
                shellPage.Navigate(typeof(SettingsPage));
                break;
            case "AddNote":

                break;
            case "Subject":

                break;
            case "Tafsir":
                GoToTafsir();
                break;

            case "Print":
                ShowPrintDialog();
                break;
        }
    }

    private void GoToTafsir()
    {
        var quranItem = quranListView.SelectedItem as QuranItem;
        if (quranItem is not null)
        {
            mainPage.AddNewSingleTafsirTab(quranItem, Chapter);
        }
    }

    private void menuFlyout_Click(object sender, RoutedEventArgs e)
    {
        var selectedItem = quranListView.SelectedItem as QuranItem;
        DataPackage dataPackage = new DataPackage();
        dataPackage.RequestedOperation = DataPackageOperation.Copy;
        switch ((sender as MenuFlyoutItem).Tag)
        {
            case "Play":
                btnPlay_Click(null, null);
                break;
            case "CopyTranslation":
                dataPackage.SetText(selectedItem.TranslationText);
                Clipboard.SetContent(dataPackage);
                break;
            case "CopyAya":
                dataPackage.SetText(selectedItem.AyahText);
                Clipboard.SetContent(dataPackage);
                break;
            case "Tafsir":
                GoToTafsir();
                break;
            case "Print":
                ShowPrintDialog();
                break;
        }
    }

    #region Print
    public async void ShowPrintDialog()
    {
        printDialog.XamlRoot = this.XamlRoot;
        AyaTo = quranListView.Items.Count;
        txtAyaFrom.Maximum = quranListView.Items.Count;
        txtAyaTo.Maximum = quranListView.Items.Count;
        txtAyaFrom.Value = 1;
        txtAyaCount.Value = 7;
        if (await printDialog.ShowAsyncQueue() == ContentDialogResult.Primary)
        {
            Print();
        }
    }
    private async void Print()
    {
        if (PrintManager.IsSupported())
        {
            _printHelper = new PrintHelper(Container);

            _printHelper.OnPrintCanceled += PrintHelper_OnPrintCanceled;
            _printHelper.OnPrintFailed += PrintHelper_OnPrintFailed;
            _printHelper.OnPrintSucceeded += PrintHelper_OnPrintSucceeded;

            for (int i = AyaFrom - 1; i < AyaTo; i = i + AyaCountInPage)
            {
                var grid = new Grid();
                // Main content with layout from data template
                var listView = new ListView();
                listView.ItemTemplate = QuranItemTemplate;
                listView.ItemsSource = quranListView.Items.Skip(i).Take(AyaCountInPage);
                grid.Children.Add(listView);
                listView.FlowDirection = FlowDirection.RightToLeft;
                grid.FlowDirection = FlowDirection.RightToLeft;
                _printHelper.AddFrameworkElementToPrint(grid);
            }

            var printHelperOptions = new PrintHelperOptions(false);
            printHelperOptions.Orientation = PrintOrientation.Default;
            printHelperOptions.AddDisplayOption(StandardPrintTaskOptions.Orientation);

            await _printHelper.ShowPrintUIAsync(WindowHelper.GetWindowHandleForCurrentWindow(MainWindow.Instance), "AlAnvar", printHelperOptions);
        }
        else
        {
            // Printing is not supported on this device
            ContentDialog noPrintingDialog = new ContentDialog()
            {
                XamlRoot = this.XamlRoot,
                Title = "عدم پشتیبانی از پرینت",
                Content = "\nمتاسفانه دستگاه شما از پرینت پشتیبانی نمی کند",
                PrimaryButtonText = "تایید"
            };
            await noPrintingDialog.ShowAsyncQueue();
        }
    }
    private void ReleasePrintHelper()
    {
        _printHelper.Dispose();
    }

    private async void PrintHelper_OnPrintSucceeded()
    {
        ReleasePrintHelper();
        ContentDialog noPrintingDialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = "پرینت موفقیت آمیز",
            Content = "\nپرینت صفحات با موفقیت انجام شد",
            PrimaryButtonText = "تایید"
        };
        await noPrintingDialog.ShowAsyncQueue();
    }

    private async void PrintHelper_OnPrintFailed()
    {
        ReleasePrintHelper();
        ContentDialog noPrintingDialog = new ContentDialog()
        {
            XamlRoot = this.XamlRoot,
            Title = "خطای پرینت",
            Content = "\nمتاسفانه پرینت با خطا مواجه شد",
            PrimaryButtonText = "تایید"
        };
        await noPrintingDialog.ShowAsyncQueue();
    }
    private void PrintHelper_OnPrintCanceled()
    {
        ReleasePrintHelper();
    }
    #endregion

    private void txtAyaTo_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        if ((int)txtAyaTo.Value < (int)txtAyaCount.Value)
        {
            txtAyaCount.Value = (int)txtAyaTo.Value;
        }

        if ((int) txtAyaTo.Value < (int) txtAyaFrom.Value)
        {
            txtAyaFrom.Value = (int) txtAyaTo.Value;
        }
    }
}
