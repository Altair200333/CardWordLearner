using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordLearner
{
    class DbManager : IDisposable
    {
        private string provider;
        private string connectionString;

        private DbProviderFactory factory;
        private DbConnection connection;

        private DbDataAdapter adapter;
        private DataTable table;

        //my DB design sucks so this is the maxim number of translations each word can have
        private readonly int maxVariantsCount = 3;

        public DbManager()
        {
            provider = ConfigurationManager.AppSettings["provider"];
            connectionString = ConfigurationManager.AppSettings["connectionString"];

            factory = DbProviderFactories.GetFactory(provider);

            connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;

            connection.Open();
        }

        public List<TranslatedWord> getTranslations()
        {
            List<TranslatedWord> translations = new List<TranslatedWord>();

            DbCommand command = factory.CreateCommand();
            command.Connection = connection;
            command.CommandText = "Select * From Translations";

            string[] buffer = new string[maxVariantsCount];

            using (DbDataReader dataReader = command.ExecuteReader())
            {
                while (dataReader.Read())
                {
                    TranslatedWord translation = new TranslatedWord();

                    var word = (string) dataReader["word"];
                    int count = Math.Min((int) dataReader["count"], maxVariantsCount);

                    for (int i = 0; i < count; i++)
                    {
                        string id = "v" + (i + 1);
                        buffer[i] = dataReader[id] is DBNull ? "" : (string) dataReader[id];
                    }

                    translation.word = word;
                    for (int i = 0; i < count; i++)
                    {
                        translation.translations.Add(buffer[i]);
                    }

                    translations.Add(translation);
                }
            }

            return translations;
        }

        public void init()
        {
            string queryString = "SELECT * FROM Translations";
            DbCommand command = factory.CreateCommand();
            command.CommandText = queryString;
            command.Connection = connection;

            adapter = factory.CreateDataAdapter();
            adapter.SelectCommand = command;

            DbCommandBuilder builder = factory.CreateCommandBuilder();
            builder.DataAdapter = adapter;

            // Get the insert, update and delete commands.
            adapter.InsertCommand = builder.GetInsertCommand();
            adapter.UpdateCommand = builder.GetUpdateCommand();
            adapter.DeleteCommand = builder.GetDeleteCommand();

            table = new DataTable();
            adapter.Fill(table);

            DataColumn[] keyColumns = new DataColumn[1];
            keyColumns[0] = table.Columns["word"];
            table.PrimaryKey = keyColumns;
        }

        public void addTranslation(TranslatedWord translation)
        {
            var existing = table.Rows.Find(translation.word);


            DataRow newRow = existing ?? table.NewRow();

            newRow["word"] = translation.word;
            newRow["count"] = translation.translations.Count;
            newRow["memory"] = 0;

            for (int i = 0; i < translation.translations.Count; i++)
            {
                string id = "v" + (i + 1);
                newRow[id] = translation.translations[i];
            }

            if (existing == null)
            {
                table.Rows.Add(newRow);
            }

            table.Select("word='" + translation.word + "'");
        }

        public bool setMemory(string key, float value)
        {
            DataRow dr = table.Select("word='" + key + "'")
                .FirstOrDefault(); // finds all rows with id==2 and selects first or null if haven't found any

            if (dr != null)
            {
                dr["memory"] = value; //changes the Product_name

                return true;
            }

            return false;
        }

        public void flushTranslations()
        {
            adapter.Update(table);
        }

        public void Dispose()
        {
            connection?.Dispose();
        }
    }
}