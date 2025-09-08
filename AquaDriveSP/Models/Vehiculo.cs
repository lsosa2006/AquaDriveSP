using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaDriveSP.Models
{
    [Table("vehiculo", Schema = "AquaDriveSP")]
    public class Vehiculo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string placa { get; set; }

        public long clienteid { get; set; }
        public string marca { get; set; }
        public string modelo { get; set; }
        public string color { get; set; }

        public virtual Cliente cliente { get; set; }
    }
}
