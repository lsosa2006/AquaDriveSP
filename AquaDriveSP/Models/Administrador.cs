using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaDriveSP.Models
{
    [Table("administrador", Schema = "AquaDriveSP")]
    public class Administrador
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long administradorid { get; set; }
        public long usuarioid { get; set; }

        public virtual Usuario usuario { get; set; }
    }
}