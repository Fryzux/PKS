using System;

namespace WpfHttpApp.Models
{
    /// <summary>POST-message stored by the server.</summary>
    public class MessageEntry
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Message { get; init; } = "";
        public DateTime ReceivedAt { get; init; } = DateTime.Now;
    }
}
