// Disciplina.cs
// MODIFICAÇÃO: Adicionados campos opcionais de QuantidadeRevisoes e Intervalo1..30.
// Quando preenchidos, substituem os valores globais da tabela Configuracoes
// para os assuntos desta disciplina específica.
// Todos os campos são nullable (int?) para distinguir "não configurado" de "zero".

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

        // ── Configurações específicas desta disciplina (nullable = usa o global) ──

        /// <summary>
        /// Quantidade de revisões ativas para esta disciplina.
        /// Se null, usa o valor global de Configuracao.QuantidadeRevisoes.
        /// </summary>
        public int? QuantidadeRevisoes { get; set; }

        // Intervalos específicos por disciplina. Int1 = dias após DataInicio.
        // Int2+ = dias após a revisão anterior (lógica encadeada).
        // Se null, usa o valor global de Configuracao.IntervaloN.
        public int? Intervalo1 { get; set; }
        public int? Intervalo2 { get; set; }
        public int? Intervalo3 { get; set; }
        public int? Intervalo4 { get; set; }
        public int? Intervalo5 { get; set; }
        public int? Intervalo6 { get; set; }
        public int? Intervalo7 { get; set; }
        public int? Intervalo8 { get; set; }
        public int? Intervalo9 { get; set; }
        public int? Intervalo10 { get; set; }
        public int? Intervalo11 { get; set; }
        public int? Intervalo12 { get; set; }
        public int? Intervalo13 { get; set; }
        public int? Intervalo14 { get; set; }
        public int? Intervalo15 { get; set; }
        public int? Intervalo16 { get; set; }
        public int? Intervalo17 { get; set; }
        public int? Intervalo18 { get; set; }
        public int? Intervalo19 { get; set; }
        public int? Intervalo20 { get; set; }
        public int? Intervalo21 { get; set; }
        public int? Intervalo22 { get; set; }
        public int? Intervalo23 { get; set; }
        public int? Intervalo24 { get; set; }
        public int? Intervalo25 { get; set; }
        public int? Intervalo26 { get; set; }
        public int? Intervalo27 { get; set; }
        public int? Intervalo28 { get; set; }
        public int? Intervalo29 { get; set; }
        public int? Intervalo30 { get; set; }

        // Uma disciplina pode ter vários assuntos
        public virtual ICollection<Assunto> Assuntos { get; set; } = new List<Assunto>();

        // ── Helpers de acesso dinâmico ────────────────────────────────────────────

        /// <summary>
        /// Retorna o intervalo específico desta disciplina para a revisão N,
        /// ou null se não houver configuração específica (nesse caso, use o global).
        /// </summary>
        public int? GetIntervalo(int n) => n switch
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
            _ => null
        };

        /// <summary>
        /// Define o intervalo específico desta disciplina para a revisão N.
        /// </summary>
        public void SetIntervalo(int n, int? value)
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
