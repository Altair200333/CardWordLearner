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

namespace WordLearner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DbManager manager;

        public MainWindow()
        {
            InitializeComponent();
            Console.OutputEncoding = Encoding.UTF8;
            manager = new DbManager();
            manager.init();

            longTask();
            
            //def();
        }

        async void longTask()
        {
            Console.WriteLine("checkDatabase");

            var res = await HeavyMethodAsync();

            Console.WriteLine("ended");
        }
        async Task<List<TranslatedWord>> HeavyMethodAsync()
        {
            var items = manager.getTranslations();
            if (items.Count == 0)
            {
                items = await getTranslations();
            }
            else
            {
                
            }

            return items;
        }
        private static void test(DbManager manager)
        {
            manager.init();
            manager.clear();
            manager.flushTranslations();
            var items = manager.getTranslations();

            for (int i = 0; i < items.Count; i++)
            {
                Console.WriteLine(items[i].word);
            }

            manager.addTranslation(new TranslatedWord()
            {
                word = "kurwa",
                translations = new List<string>() {"da"},
            });

            manager.flushTranslations();

            manager.setMemory("kurwa", 3.0f);

            manager.flushTranslations();

            items = manager.getTranslations();
        }

        async Task<List<TranslatedWord>> getTranslations()
        {
            ConcurrentBag<TranslatedWord> translations = new ConcurrentBag<TranslatedWord>();
            MicrosoftTranslatorApi.init();

            List<string> words = new List<string>();
            string fileName = "words.txt";
            foreach (string line in System.IO.File.ReadLines(fileName))
            {
                words.Add(line);
            }

            await Task.WhenAll(words.Select(x=>JobDispatcher(translations, x)));

            return translations.ToList();
        }
        static async Task JobDispatcher(ConcurrentBag<TranslatedWord> translations, string word)
        {
            var res = await MicrosoftTranslatorApi.translate(word).ConfigureAwait(false);
            if (res != null)
            {
                translations.Add(res);
            }
        }
    }
}