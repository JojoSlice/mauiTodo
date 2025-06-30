using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CouchbaseTodo.Models
{
    public class ToDoTask
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = "task";

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("completed")]
        public bool Completed { get; set; }

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; }

        public ToDoTask() 
        {
        }

        public ToDoTask(string? id = null, string text = "", bool completed = false, DateTime? createdAt = null)
        {
            Id = id ?? GenerateId();
            Text = text;
            Completed = completed;
            CreatedAt = (createdAt ?? DateTime.UtcNow).ToString("o"); // ISO 8601
        }

        private string GenerateId()
        {
            return $"task_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{new Random().Next(10000)}";
        }

        public static ToDoTask FromJson(string json)
        {
            return JsonSerializer.Deserialize<ToDoTask>(json);
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

    }
}
