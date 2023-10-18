using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventsApp.Models
{
    public class EventImage
    {
        [Key]
        public int Id { get; set; }
        public string Url { get; set; }

        [ForeignKey(nameof(Event))]
        public int EventId { get; set; }
        public virtual Event Event { get; set; }

    }
}
