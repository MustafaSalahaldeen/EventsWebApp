using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventsApp.Models
{
    public class EventUserRelation
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        [ForeignKey(nameof(Event))]
        public int EventId { get; set; }

        public virtual Event Event { get; set; }
        public virtual User User { get; set; }

    }
}
