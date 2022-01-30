using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Google.Api.Gax.ResourceNames;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Google.Cloud.Translate.V3;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.Common;
using System.Configuration;
using System.Data;
using System.Threading;

namespace WordLearner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DbManager manager;

        private List<TranslatedWord> translations;

        private List<Button> variantButtons = new List<Button>();

        Color defaultTextColor = Colors.White;
        Color rightTextColor = Colors.GreenYellow;
        Color wrongTextColor = Colors.Red;

        enum QuestionStatus
        {
            NotLoaded,
            WaitingForAnswer,
            Pending,
        }
        private QuestionStatus currentStatus = QuestionStatus.NotLoaded;
        private TranslatedWord currentWord;
        private List<string> correctAnswers = new List<string>();

        Random r = new Random();
        public MainWindow()
        {
            InitializeComponent();
            Console.OutputEncoding = Encoding.UTF8;

            manager = new DbManager();
            manager.init();

            loadWords.Click += LoadWordsOnClick;
            
            assignButtons();

            initializeWords();
        }

        private void LoadWordsOnClick(object sender, RoutedEventArgs e)
        {
            if (currentStatus == QuestionStatus.NotLoaded)
                return;

            currentStatus = QuestionStatus.NotLoaded;
            initializeWords(true);
        }

        private void variantSelectedClick(object sender, RoutedEventArgs e)
        {
            int id = variantButtons.IndexOf(sender as Button);
            if(id == -1)
                return;

            submitAnswer(variantButtons[id].Content as string);
        }

        private void submitAnswer(string answer)
        {
            if(currentStatus == QuestionStatus.NotLoaded || answer == null)
                return;

            if (currentStatus == QuestionStatus.Pending)
            {
                nextWord();
            }
            else if(currentStatus == QuestionStatus.WaitingForAnswer)
            {
                foreach (var btn in variantButtons)
                {
                    string text = btn.Content as string;
                    if (text == null)
                        continue;

                    if (currentWord.translations.Contains(text))
                    {
                        btn.Foreground = new SolidColorBrush(rightTextColor);
                    }
                }

                if (currentWord.translations.Contains(answer))
                {
                    //remember this word as known
                }
                else
                {
                    //mark word as poorly known
                    var selectedBtn = variantButtons.Where(x => ((string) x.Content) == answer);
                    foreach (var button in selectedBtn)
                    {
                        button.Foreground = new SolidColorBrush(wrongTextColor);
                    }
                }

                currentStatus = QuestionStatus.Pending;

            }
        }


        async void initializeWords(bool forceAsk = false)
        {
            Title.Text = "Loading...";

            Console.WriteLine("checkDatabase");

            translations = await LoadTranslationsAsync(forceAsk);

            Console.WriteLine("ended");

            Title.Text = "Loaded";

            nextWord();
        }
        
        private void nextWord()
        {
            currentStatus = QuestionStatus.WaitingForAnswer;

            bool found = false;
            while (!found)
            {
                int index = r.Next(0, translations.Count);
                currentWord = translations[index];
                
                if (currentWord.translations.Count > 0)
                {
                    found = true;
                }
            }

            var answer = currentWord.translations[0];
            correctAnswers.Add(answer);

            currentWordLabel.Text = currentWord.word;

            int answerButtonId = r.Next(0, variantButtons.Count);
            variantButtons[answerButtonId].Content = answer;

            for (int i = 0; i < variantButtons.Count; i++)
            {
                variantButtons[i].Foreground = new SolidColorBrush(defaultTextColor);

                if (i == answerButtonId)
                    continue;

                bool generated = false;

                while (!generated)
                {
                    int index = r.Next(0, translations.Count);
                    var variant = translations[index];

                    if (variant.translations.Count > 0)
                    {
                        int translationId = r.Next(0, variant.translations.Count);

                        variantButtons[i].Content = variant.translations[translationId];

                        generated = true;
                    }
                }
            }
        }

        async Task<List<TranslatedWord>> LoadTranslationsAsync(bool forceAsk = false)
        {
            var items = manager.getTranslations();
            if (items.Count == 0 || forceAsk)
            {
                items = await getWordsFromUser();

                manager.clear();
                manager.flushTranslations();

                foreach (var translatedWord in items)
                {
                    manager.addTranslation(translatedWord);
                }

                manager.flushTranslations();
            }

            return items;
        }

        private async Task<List<TranslatedWord>> getWordsFromUser()
        {
            List<TranslatedWord> items;
            var path = Utils.dialog("");

            if (String.IsNullOrEmpty(path))
                throw new NotImplementedException();

            items = await getTranslations(path);
            return items;
        }
        
        object locker = new  object();
        private int counter = 0;
        private int totalWords = 0;

        async Task<List<TranslatedWord>> getTranslations(string fileName = "words.txt")
        {
            ConcurrentBag<TranslatedWord> translations = new ConcurrentBag<TranslatedWord>();
            MicrosoftTranslatorApi.init();

            List<string> words = new List<string>();

            foreach (string line in System.IO.File.ReadLines(fileName))
            {
                words.Add(line);
            }

            counter = 0;
            totalWords = words.Count;
            await Task.WhenAll(words.Select(x=>JobDispatcher(translations, x)));

            return translations.ToList();
        }
        async Task JobDispatcher(ConcurrentBag<TranslatedWord> translations, string word)
        {
            var res = await MicrosoftTranslatorApi.translate(word).ConfigureAwait(false);
            if (res != null)
            {
                translations.Add(res);
            }

            report();
        }

        async void report()
        {
            Interlocked.Increment(ref counter);
            Title.Dispatcher.Invoke(() =>
            {
                Title.Text = ((float)counter / totalWords).ToString("0.00");

            });
        }
        private void assignButtons()
        {
            variantButtons.Add(v1);
            variantButtons.Add(v2);
            variantButtons.Add(v3);
            variantButtons.Add(v4);
            variantButtons.Add(v5);
            variantButtons.Add(v6);
            
            for (int i = 0; i < variantButtons.Count; i++)
            {
                variantButtons[i].Click += variantSelectedClick;
            }
        }
    }
}