using kalendar.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;

namespace kalendar.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Dictionary<int, List<Days>> EventsByDay { get; set; } = new();

        public void OnGet()
        {
            LoadEvents();
        }

        private void LoadEvents()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            const string sql = "SELECT id, date, category, event FROM events WHERE date >= '2025-11-01' AND date <= '2025-11-30'";

            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var id = reader.GetInt32("id");
                var date = reader.GetFieldValue<DateTime>("date");
                var category = reader.GetString("category");
                var eventText = reader.GetString("event");
                int day = date.Day;

                if (!EventsByDay.ContainsKey(day))
                    EventsByDay[day] = new List<Days>();

                EventsByDay[day].Add(new Days
                {
                    Id = id,
                    Date = date,
                    Category = category,
                    Event = eventText
                });
            }
        }

        public JsonResult OnGetGetEventsByDay(int day)
        {
            var date = new DateTime(2025, 11, day);
            var events = new List<Days>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand("SELECT id, category, event FROM events WHERE date = @date", conn);
            cmd.Parameters.AddWithValue("@date", date);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                events.Add(new Days
                {
                    Id = reader.GetInt32("id"),
                    Category = reader.GetString("category"),
                    Event = reader.GetString("event")
                });
            }

            return new JsonResult(events);
        }
        public IActionResult OnPostAddEvent(int Day, string Category, string Event)
        {
            var date = new DateTime(2025, 11, Day);
            ExecuteNonQuery("INSERT INTO events (date, category, event) VALUES (@date, @cat, @evt)",
                ("@date", date), ("@cat", Category), ("@evt", Event));
            return RedirectToPage();
        }

        public IActionResult OnPostEditEvent(int Id, int Day, string Category, string Event)
        {
            ExecuteNonQuery("UPDATE events SET category = @cat, event = @evt WHERE id = @id",
                ("@id", Id), ("@cat", Category), ("@evt", Event));
            return RedirectToPage();
        }

        public IActionResult OnPostDeleteEvent(int id)
        {
            ExecuteNonQuery("DELETE FROM events WHERE id = @id", ("@id", id));
            return RedirectToPage();
        }

        private void ExecuteNonQuery(string sql, params (string name, object value)[] parameters)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(sql, conn);
            foreach (var (name, value) in parameters)
                cmd.Parameters.AddWithValue(name, value);
            cmd.ExecuteNonQuery();
        }
    }
}