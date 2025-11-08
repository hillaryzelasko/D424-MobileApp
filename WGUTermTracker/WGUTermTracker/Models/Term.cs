using System;
using SQLite;

namespace WGUTermTracker.Models
{
    [Table("Terms")]
    public class Term
    {
        [PrimaryKey,AutoIncrement] public int ID { get; set; }

        public string Termname { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Today;

        public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(3);

    }
}
