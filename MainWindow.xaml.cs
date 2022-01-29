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
        private string key = "c1207a4441224386aaced999f5aeb646";
        private string location = "eastasia";
        private static readonly string endpoint = "https://api.cognitive.microsofttranslator.com/";

        public MainWindow()
        {
            InitializeComponent();
            Console.OutputEncoding = Encoding.UTF8;
            DbManager manager = new DbManager();

            manager.init();
            manager.addTranslation(new TranslatedWord()
            {
                word = "kurwa",
                translations = new List<string>(){"da"},
            });
            
            manager.flushTranslations();

            manager.setMemory("kurwa", 2.0f);

            manager.flushTranslations();



            var items = manager.getTranslations();
            
            //def();
        }

        async void def()
        {
            ConcurrentBag<TranslatedWord> translations = new ConcurrentBag<TranslatedWord>();
            MicrosoftTranslatorApi.init();

            List<string> words = new List<string>();
            string fileName = "words.txt";
            foreach (string line in System.IO.File.ReadLines(fileName))
            {
                words.Add(line);
            }

            Parallel.ForEach(words, async (word) =>
            {
                var res = await MicrosoftTranslatorApi.translate(word).ConfigureAwait(false);
                if (res != null)
                {
                    translations.Add(res);
                }
            });
        }
    }
}