using System;
using System.ComponentModel.DataAnnotations;

namespace RevisaFacil.Models
{
    public class NotaCalendario
    {
        [Key]
        public int Id { get; set; }

        public DateTime Data { get; set; }

        public string Conteudo { get; set; }

        // Novo campo para vincular a nota a um assunto específico
        // Se for null, é uma nota manual. Se tiver ID, é uma revisão automática.
        public int? AssuntoId { get; set; }
    }
}