using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Couchbase.Lite;
using Couchbase.Lite.Query;
using Couchbase.Lite.Sync;

namespace CouchbaseTodo.Services
{
    public class DatabaseService
    {
        private Database _database;
        private Collection _collection;
        private Replicator _replicator;

        public DatabaseService()
        {
            InitializeDatabase();
            InitializeReplicator();
        }

        private void InitializeDatabase()
        {
            _database = new Database("todotasks");
            _collection = _database.GetDefaultCollection();
        }

        private void InitializeReplicator()
        {
            var targetEndpoint = new URLEndpoint(new Uri("wss://s43dqjp7iquyroau.apps.cloud.couchbase.com:4984/todotasks"));
            var replConfig = new ReplicatorConfiguration(targetEndpoint);
            replConfig.AddCollection(_collection);

            replConfig.Authenticator = new BasicAuthenticator("todoUser", "Super12345!");

            _replicator = new Replicator(replConfig);
            _replicator.AddChangeListener((sender, args) =>
            {
                if (args.Status.Error != null)
                {
                    Console.WriteLine($"Replicator error: {args.Status.Error}");
                }
            });
            _replicator.Start();
        }

        public async Task AddTaskAsync(Models.ToDoTask task)
        {
            var mutableDoc = new MutableDocument(task.Id)
                .SetString("type", task.Type)
                .SetString("text", task.Text)
                .SetBoolean("completed", task.Completed)
                .SetString("createdAt", task.CreatedAt);

            _collection.Save((MutableDocument)mutableDoc);
            await Task.CompletedTask;
        }

        public async Task UpdateTaskAsync(Models.ToDoTask task)
        {
            var doc = _collection.GetDocument(task.Id);
            if (doc == null) return;

            var mutableDoc = doc.ToMutable();
            mutableDoc
                .SetString("type", task.Type)
                .SetString("text", task.Text)
                .SetBoolean("completed", task.Completed)
                .SetString("createdAt", task.CreatedAt);

            _collection.Save(mutableDoc);
            await Task.CompletedTask;
        }

        public async Task DeleteTaskAsync(string id)
        {
            try
            {
                var doc = _collection.GetDocument(id);
                if (doc != null)
                {
                    _collection.Delete(doc);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            await Task.CompletedTask;
        }

        public async Task<List<Models.ToDoTask>> GetAllTasksAsync()
        {
            var tasks = new List<Models.ToDoTask>();

            try
            {
                var query = QueryBuilder
                    .Select(SelectResult.All())
                    .From(DataSource.Collection(_collection))
                    .Where(Expression.Property("type").EqualTo(Expression.String("task")));

                var result = query.Execute();

                foreach (var row in result)
                {
                    var dict = row.GetDictionary(_collection.Name);
                    if (dict == null) continue;

                    var id = dict.GetString("id") ?? dict.GetString("_id");

                    if (string.IsNullOrWhiteSpace(id))
                        continue; 

                    var task = new Models.ToDoTask
                    {
                        Id = id,
                        Type = dict.GetString("type") ?? "task",
                        Text = dict.GetString("text") ?? "",
                        Completed = dict.GetBoolean("completed"),
                        CreatedAt = dict.GetString("createdAt") ?? DateTime.UtcNow.ToString("o")
                    };

                    tasks.Add(task);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAllTasksAsync] Error: {ex.Message}");
            }

            return await Task.FromResult(tasks);
        }
 

    }
}
