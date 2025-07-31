using Syncfusion.Windows.Controls.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace SfTextBoxExt_Async
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly string[] _database = [
            "apple",
            "banana",
            "cherry",
            "date",
            "elderberry",
            "fig",
            "grape",
            "honeydew",
            "kiwi",
            "lemon",
            "mango",
            "nectarine",
            "orange",
            "papaya",
            "quince",
            "raspberry",
            "strawberry",
            "tangerine",
            "ugli fruit",
            "vanilla bean"
            ];

        private readonly ObservableCollection<string> _autoMatchTable = [];
        private readonly HashSet<string> _autoMatchSearched = new(StringComparer.CurrentCultureIgnoreCase);    // All search strings that are in automatch table.
        private readonly HashSet<string> _autoMatchTablePresentProperties = new(StringComparer.CurrentCultureIgnoreCase);    // All properties present in _autoMatchTable

        public MainWindow()
        {
            InitializeComponent();

            textBoxExt.AutoCompleteSource = _autoMatchTable;
            textBoxExt.AutoCompleteMode = AutoCompleteMode.Suggest;
            textBoxExt.SuggestionMode = SuggestionMode.Contains;
            textBoxExt.TextChanged += TextBox_TextChanged;
        }

        private void TextBox_TextChanged(object? sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Update suggestion data
            _ = RefreshAutoSuggestions();
        }

        private bool _inRefresh;
        private bool _refreshRequested;

        private async Task RefreshAutoSuggestions()
        {
            _refreshRequested = true;
            if (_inRefresh)
                return;
            try
            {
                _inRefresh = true;
                while (_refreshRequested)
                {
                    _refreshRequested = false;

                    if (!textBoxExt.IsFocused ||
                        textBoxExt.IsReadOnly)    // Only if focused and not readonly. No sense in fetching this otherwise.
                        return;
                    string text = textBoxExt.Text;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        text = text.Trim();
                        bool added = false;
                        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                        // If not already added
                        if (_autoMatchSearched.Add(text))
                        {
                            foreach (var data in _database.Where(s => s.Contains(text)))
                            {
                                if (chkSynch.IsChecked == false)
                                    await Task.Delay(1000); // Simulate async delay
                                if (_autoMatchTablePresentProperties.Add(data))
                                {
                                    added = true;
                                    _autoMatchTable.Add(data);
                                }
                                if (added && stopwatch.ElapsedMilliseconds > 100)   // Update every 100ms
                                {
                                    added = false;
                                    textBoxExt.FilterSuggestions();   // Update display
                                    stopwatch.Restart();
                                }
                            }
                        }
                        if (added)
                        {
                            textBoxExt.FilterSuggestions();
                            System.Diagnostics.Debug.WriteLine($"**** FilterSuggestions called - isopen: {textBoxExt.IsSuggestionOpen}, count: {_autoMatchTable.Count}.");
                        }
                    }
                }
            }
            finally
            {
                _inRefresh = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            textBoxExt.Text = string.Empty;
            _autoMatchTable.Clear();
            _autoMatchSearched.Clear();
            _autoMatchTablePresentProperties.Clear();
        }
    }
}