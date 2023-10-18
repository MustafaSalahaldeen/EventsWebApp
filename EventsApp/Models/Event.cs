using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventsApp.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }
        [ForeignKey(nameof(User))]
        public int ProviderId { get; set; }
        public bool Accepted { get; set; }

        public virtual Category Category { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<EventImage> Images { get; set; }
    }
}
