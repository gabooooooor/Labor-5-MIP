using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WindowsFormsApp6
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string Denumire { get; set; }

        [MaxLength(500)]
        public string Descriere { get; set; }

        public DateTime DataIntrarii { get; set; }
        public DateTime DataExpirarii { get; set; }
        public int Cantitate { get; set; }
    }
}