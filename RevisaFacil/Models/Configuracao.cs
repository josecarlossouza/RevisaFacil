namespace RevisaFacil.Models
{
    public class Configuracao
    {
        public int Id { get; set; } // Sempre 1
        public int Intervalo1 { get; set; } = 30;
        public int Intervalo2 { get; set; } = 60;
        public int Intervalo3 { get; set; } = 90;
        public int Intervalo4 { get; set; } = 120;
        public int Intervalo5 { get; set; } = 150;
    }
}