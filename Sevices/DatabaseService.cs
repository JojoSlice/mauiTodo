using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Couchbase.Lite;
using Couchbase.Lite.Query;
using Couchbase.Lite.Sync;

namespace CouchbaseTodo.Services
{
    public class DatabaseService : IDisposable
    {
        private readonly Database _database;
        private readonly Collection _collection;
        private readonly Replicator _replicator;

        public DatabaseService()
        {
            _database = new Database("todotasks");
            _collection = _database.GetDefaultCollection();

            _replicator = SetupReplicator();
            _replicator.Start();
        }

        private Replicator SetupReplicator()
        {
            var targetEndpoint = new URLEndpoint(new Uri("wss://s43dqjp7iquyroau.apps.cloud.couchbase.com:4984/todotasks"));
            var config = new ReplicatorConfiguration(targetEndpoint)
            {
                ReplicatorType = ReplicatorType.PushAndPull,
                Continuous = true,
                Authenticator = new BasicAuthenticator("todoUser", "Super12345!")
            };

            config.AddCollection(_collection);

            var replicator = new Replicator(config);
            replicator.AddChangeListener((sender, args) =>
            {
                if (args.Status.Error != null)
                {
                    Console.WriteLine($"[Replicator] Error: {args.Status.Error}");
                }
                else
                {
                    Console.WriteLine($"[Replicator] Status: {args.Status.Activity} - Completed: {args.Status.Progress.Completed} / {args.Status.Progress.Total}");
                }
            });

            return replicator;
        }

        // CREATE
        public async Task AddTaskAsync(Models.ToDoTask task)
        {
            try
            {
                var mutableDoc = new MutableDocument(task.Id);
                    mutableDoc.SetString("type", "task")
                    .SetString("text", task.Text)
                    .SetBoolean("completed", task.Completed)
                    .SetString("createdAt", task.CreatedAt);

                await Task.Run(() => _collection.Save(mutableDoc));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddTaskAsync] Error: {ex.Message}");
            }
        }

        // READ ALL
        public async Task<List<Models.ToDoTask>> GetAllTasksAsync()
        {
            var tasks = new List<Models.ToDoTask>();

            try
            {
                var query = QueryBuilder
                    .Select(SelectResult.All())
                    .From(DataSource.Collection(_collection))
                    .Where(Expression.Property("type").EqualTo(Expression.String("task")));

                var resultSet = await Task.Run(() => query.Execute());

                foreach (var row in resultSet)
                {
                    var dict = row.GetDictionary(_collection.Name);
                    if (dict == null) continue;

                    var id = dict.GetString("id") ?? dict.GetString("_id");
                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    tasks.Add(new Models.ToDoTask
                    {
                        Id = id,
                        Type = dict.GetString("type") ?? "task",
                        Text = dict.GetString("text") ?? string.Empty,
                        Completed = dict.GetBoolean("completed"),
                        CreatedAt = dict.GetString("createdAt") ?? DateTime.UtcNow.ToString("o")
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAllTasksAsync] Error: {ex.Message}");
            }

            return tasks;
        }

        // READ BY ID
        public async Task<Models.ToDoTask?> GetTaskByIdAsync(string id)
        {
            return await Task.Run(() =>
            {
                var doc = _collection.GetDocument(id);
                if (doc == null) return null;

                return new Models.ToDoTask
                {
                    Id = doc.Id,
                    Type = doc.GetString("type") ?? "task",
                    Text = doc.GetString("text") ?? string.Empty,
                    Completed = doc.GetBoolean("completed"),
                    CreatedAt = doc.GetString("createdAt") ?? DateTime.UtcNow.ToString("o")
                };
            });
        }

        // UPDATE
        public async Task UpdateTaskAsync(Models.ToDoTask task)
        {
            try
            {
                var doc = await Task.Run(() => _collection.GetDocument(task.Id));
                if (doc == null)
                {
                    Console.WriteLine($"[UpdateTaskAsync] Document with id {task.Id} not found.");
                    return;
                }

                var mutableDoc = doc.ToMutable();
                    mutableDoc.SetString("text", task.Text)
                    .SetBoolean("completed", task.Completed)
                    .SetString("createdAt", task.CreatedAt);

                await Task.Run(() => _collection.Save(mutableDoc));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateTaskAsync] Error: {ex.Message}");
            }
        }

        // DELETE
        public async Task DeleteTaskAsync(string id)
        {
            try
            {
                var doc = await Task.Run(() => _collection.GetDocument(id));
                if (doc == null)
                {
                    Console.WriteLine($"[DeleteTaskAsync] Document with id {id} not found.");
                    return;
                }

                await Task.Run(() => _collection.Delete(doc));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteTaskAsync] Error: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _replicator?.Stop();
            _replicator?.Dispose();
            _database?.Dispose();
        }
    }
}
