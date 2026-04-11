using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RevisaFacil.Models
{
    public class Disciplina
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        // Uma disciplina pode ter vários assuntos
        public virtual ICollection<Assunto> Assuntos { get; set; } = new List<Assunto>();
    }
}