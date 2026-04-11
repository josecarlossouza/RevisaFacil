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
    }
}