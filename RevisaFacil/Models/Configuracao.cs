// Configuracao.cs
namespace RevisaFacil.Models
{
    public class Configuracao
    {
        public int Id { get; set; } // Sempre 1

        // Quantidade de revisões ativas (1 a 30), padrão 10
        public int QuantidadeRevisoes { get; set; } = 10;

        public int Intervalo1 { get; set; } = 30;
        public int Intervalo2 { get; set; } = 60;
        public int Intervalo3 { get; set; } = 90;
        public int Intervalo4 { get; set; } = 120;
        public int Intervalo5 { get; set; } = 150;
        public int Intervalo6 { get; set; } = 180;
        public int Intervalo7 { get; set; } = 210;
        public int Intervalo8 { get; set; } = 240;
        public int Intervalo9 { get; set; } = 270;
        public int Intervalo10 { get; set; } = 300;
        public int Intervalo11 { get; set; } = 330;
        public int Intervalo12 { get; set; } = 360;
        public int Intervalo13 { get; set; } = 390;
        public int Intervalo14 { get; set; } = 420;
        public int Intervalo15 { get; set; } = 450;
        public int Intervalo16 { get; set; } = 480;
        public int Intervalo17 { get; set; } = 510;
        public int Intervalo18 { get; set; } = 540;
        public int Intervalo19 { get; set; } = 570;
        public int Intervalo20 { get; set; } = 600;
        public int Intervalo21 { get; set; } = 630;
        public int Intervalo22 { get; set; } = 660;
        public int Intervalo23 { get; set; } = 690;
        public int Intervalo24 { get; set; } = 720;
        public int Intervalo25 { get; set; } = 750;
        public int Intervalo26 { get; set; } = 780;
        public int Intervalo27 { get; set; } = 810;
        public int Intervalo28 { get; set; } = 840;
        public int Intervalo29 { get; set; } = 870;
        public int Intervalo30 { get; set; } = 900;

        public int GetIntervalo(int n) => n switch
        {
            1 => Intervalo1,
            2 => Intervalo2,
            3 => Intervalo3,
            4 => Intervalo4,
            5 => Intervalo5,
            6 => Intervalo6,
            7 => Intervalo7,
            8 => Intervalo8,
            9 => Intervalo9,
            10 => Intervalo10,
            11 => Intervalo11,
            12 => Intervalo12,
            13 => Intervalo13,
            14 => Intervalo14,
            15 => Intervalo15,
            16 => Intervalo16,
            17 => Intervalo17,
            18 => Intervalo18,
            19 => Intervalo19,
            20 => Intervalo20,
            21 => Intervalo21,
            22 => Intervalo22,
            23 => Intervalo23,
            24 => Intervalo24,
            25 => Intervalo25,
            26 => Intervalo26,
            27 => Intervalo27,
            28 => Intervalo28,
            29 => Intervalo29,
            30 => Intervalo30,
            _ => 30
        };

        public void SetIntervalo(int n, int value)
        {
            switch (n)
            {
                case 1: Intervalo1 = value; break;
                case 2: Intervalo2 = value; break;
                case 3: Intervalo3 = value; break;
                case 4: Intervalo4 = value; break;
                case 5: Intervalo5 = value; break;
                case 6: Intervalo6 = value; break;
                case 7: Intervalo7 = value; break;
                case 8: Intervalo8 = value; break;
                case 9: Intervalo9 = value; break;
                case 10: Intervalo10 = value; break;
                case 11: Intervalo11 = value; break;
                case 12: Intervalo12 = value; break;
                case 13: Intervalo13 = value; break;
                case 14: Intervalo14 = value; break;
                case 15: Intervalo15 = value; break;
                case 16: Intervalo16 = value; break;
                case 17: Intervalo17 = value; break;
                case 18: Intervalo18 = value; break;
                case 19: Intervalo19 = value; break;
                case 20: Intervalo20 = value; break;
                case 21: Intervalo21 = value; break;
                case 22: Intervalo22 = value; break;
                case 23: Intervalo23 = value; break;
                case 24: Intervalo24 = value; break;
                case 25: Intervalo25 = value; break;
                case 26: Intervalo26 = value; break;
                case 27: Intervalo27 = value; break;
                case 28: Intervalo28 = value; break;
                case 29: Intervalo29 = value; break;
                case 30: Intervalo30 = value; break;
            }
        }
    }
}
