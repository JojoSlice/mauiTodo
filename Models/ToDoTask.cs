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
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("type")]
        public string Type { get; set; } = "task";

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("completed")]
        public bool Completed { get; set; } = false;

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; } = DateTime.UtcNow.ToString("o");
        [JsonIgnore]
        public string Icon => Completed ? "checkbox.png" : "box.png";

        public ToDoTask() 
        {
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
