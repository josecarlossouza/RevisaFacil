// Assunto.cs
// MODIFICAÇÃO: Datas de revisão agora são encadeadas.
// Rev1 = DataInicio + Int1
// Rev2 = DataRev1  + Int2
// Rev3 = DataRev2  + Int3  ... e assim por diante.
// Isso significa que Int2 em diante representa "dias após a revisão anterior",
// e não mais "dias após o início".

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace RevisaFacil.Models
{
    public class Assunto : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public int DisciplinaId { get; set; }
        public virtual Disciplina Disciplina { get; set; }
        public bool IsDestacado { get; set; }

        private DateTime _dataInicio = DateTime.Now;
        public DateTime DataInicio
        {
            get => _dataInicio;
            set { _dataInicio = value; OnPropertyChanged(); NotificarDatas(); }
        }

        // ── Intervalos em dias ────────────────────────────────────────────────────
        // Int1 = dias após DataInicio
        // Int2..30 = dias após a revisão ANTERIOR
        private int _int1 = 30; public int Int1 { get => _int1; set { _int1 = value; OnPropertyChanged(); NotificarDatasAPartirDe(1); } }
        private int _int2 = 30; public int Int2 { get => _int2; set { _int2 = value; OnPropertyChanged(); NotificarDatasAPartirDe(2); } }
        private int _int3 = 30; public int Int3 { get => _int3; set { _int3 = value; OnPropertyChanged(); NotificarDatasAPartirDe(3); } }
        private int _int4 = 30; public int Int4 { get => _int4; set { _int4 = value; OnPropertyChanged(); NotificarDatasAPartirDe(4); } }
        private int _int5 = 30; public int Int5 { get => _int5; set { _int5 = value; OnPropertyChanged(); NotificarDatasAPartirDe(5); } }
        private int _int6 = 30; public int Int6 { get => _int6; set { _int6 = value; OnPropertyChanged(); NotificarDatasAPartirDe(6); } }
        private int _int7 = 30; public int Int7 { get => _int7; set { _int7 = value; OnPropertyChanged(); NotificarDatasAPartirDe(7); } }
        private int _int8 = 30; public int Int8 { get => _int8; set { _int8 = value; OnPropertyChanged(); NotificarDatasAPartirDe(8); } }
        private int _int9 = 30; public int Int9 { get => _int9; set { _int9 = value; OnPropertyChanged(); NotificarDatasAPartirDe(9); } }
        private int _int10 = 30; public int Int10 { get => _int10; set { _int10 = value; OnPropertyChanged(); NotificarDatasAPartirDe(10); } }
        private int _int11 = 30; public int Int11 { get => _int11; set { _int11 = value; OnPropertyChanged(); NotificarDatasAPartirDe(11); } }
        private int _int12 = 30; public int Int12 { get => _int12; set { _int12 = value; OnPropertyChanged(); NotificarDatasAPartirDe(12); } }
        private int _int13 = 30; public int Int13 { get => _int13; set { _int13 = value; OnPropertyChanged(); NotificarDatasAPartirDe(13); } }
        private int _int14 = 30; public int Int14 { get => _int14; set { _int14 = value; OnPropertyChanged(); NotificarDatasAPartirDe(14); } }
        private int _int15 = 30; public int Int15 { get => _int15; set { _int15 = value; OnPropertyChanged(); NotificarDatasAPartirDe(15); } }
        private int _int16 = 30; public int Int16 { get => _int16; set { _int16 = value; OnPropertyChanged(); NotificarDatasAPartirDe(16); } }
        private int _int17 = 30; public int Int17 { get => _int17; set { _int17 = value; OnPropertyChanged(); NotificarDatasAPartirDe(17); } }
        private int _int18 = 30; public int Int18 { get => _int18; set { _int18 = value; OnPropertyChanged(); NotificarDatasAPartirDe(18); } }
        private int _int19 = 30; public int Int19 { get => _int19; set { _int19 = value; OnPropertyChanged(); NotificarDatasAPartirDe(19); } }
        private int _int20 = 30; public int Int20 { get => _int20; set { _int20 = value; OnPropertyChanged(); NotificarDatasAPartirDe(20); } }
        private int _int21 = 30; public int Int21 { get => _int21; set { _int21 = value; OnPropertyChanged(); NotificarDatasAPartirDe(21); } }
        private int _int22 = 30; public int Int22 { get => _int22; set { _int22 = value; OnPropertyChanged(); NotificarDatasAPartirDe(22); } }
        private int _int23 = 30; public int Int23 { get => _int23; set { _int23 = value; OnPropertyChanged(); NotificarDatasAPartirDe(23); } }
        private int _int24 = 30; public int Int24 { get => _int24; set { _int24 = value; OnPropertyChanged(); NotificarDatasAPartirDe(24); } }
        private int _int25 = 30; public int Int25 { get => _int25; set { _int25 = value; OnPropertyChanged(); NotificarDatasAPartirDe(25); } }
        private int _int26 = 30; public int Int26 { get => _int26; set { _int26 = value; OnPropertyChanged(); NotificarDatasAPartirDe(26); } }
        private int _int27 = 30; public int Int27 { get => _int27; set { _int27 = value; OnPropertyChanged(); NotificarDatasAPartirDe(27); } }
        private int _int28 = 30; public int Int28 { get => _int28; set { _int28 = value; OnPropertyChanged(); NotificarDatasAPartirDe(28); } }
        private int _int29 = 30; public int Int29 { get => _int29; set { _int29 = value; OnPropertyChanged(); NotificarDatasAPartirDe(29); } }
        private int _int30 = 30; public int Int30 { get => _int30; set { _int30 = value; OnPropertyChanged(); NotificarDatasAPartirDe(30); } }

        // ── Datas calculadas ENCADEADAS ───────────────────────────────────────────
        // Rev1 conta a partir do Início. Rev2+ conta a partir da revisão anterior.
        [NotMapped] public DateTime DataRev1 => DataInicio.AddDays(Int1);
        [NotMapped] public DateTime DataRev2 => DataRev1.AddDays(Int2);
        [NotMapped] public DateTime DataRev3 => DataRev2.AddDays(Int3);
        [NotMapped] public DateTime DataRev4 => DataRev3.AddDays(Int4);
        [NotMapped] public DateTime DataRev5 => DataRev4.AddDays(Int5);
        [NotMapped] public DateTime DataRev6 => DataRev5.AddDays(Int6);
        [NotMapped] public DateTime DataRev7 => DataRev6.AddDays(Int7);
        [NotMapped] public DateTime DataRev8 => DataRev7.AddDays(Int8);
        [NotMapped] public DateTime DataRev9 => DataRev8.AddDays(Int9);
        [NotMapped] public DateTime DataRev10 => DataRev9.AddDays(Int10);
        [NotMapped] public DateTime DataRev11 => DataRev10.AddDays(Int11);
        [NotMapped] public DateTime DataRev12 => DataRev11.AddDays(Int12);
        [NotMapped] public DateTime DataRev13 => DataRev12.AddDays(Int13);
        [NotMapped] public DateTime DataRev14 => DataRev13.AddDays(Int14);
        [NotMapped] public DateTime DataRev15 => DataRev14.AddDays(Int15);
        [NotMapped] public DateTime DataRev16 => DataRev15.AddDays(Int16);
        [NotMapped] public DateTime DataRev17 => DataRev16.AddDays(Int17);
        [NotMapped] public DateTime DataRev18 => DataRev17.AddDays(Int18);
        [NotMapped] public DateTime DataRev19 => DataRev18.AddDays(Int19);
        [NotMapped] public DateTime DataRev20 => DataRev19.AddDays(Int20);
        [NotMapped] public DateTime DataRev21 => DataRev20.AddDays(Int21);
        [NotMapped] public DateTime DataRev22 => DataRev21.AddDays(Int22);
        [NotMapped] public DateTime DataRev23 => DataRev22.AddDays(Int23);
        [NotMapped] public DateTime DataRev24 => DataRev23.AddDays(Int24);
        [NotMapped] public DateTime DataRev25 => DataRev24.AddDays(Int25);
        [NotMapped] public DateTime DataRev26 => DataRev25.AddDays(Int26);
        [NotMapped] public DateTime DataRev27 => DataRev26.AddDays(Int27);
        [NotMapped] public DateTime DataRev28 => DataRev27.AddDays(Int28);
        [NotMapped] public DateTime DataRev29 => DataRev28.AddDays(Int29);
        [NotMapped] public DateTime DataRev30 => DataRev29.AddDays(Int30);

        // ── Status de conclusão ───────────────────────────────────────────────────
        public bool Rev1Concluida { get; set; }
        public bool Rev2Concluida { get; set; }
        public bool Rev3Concluida { get; set; }
        public bool Rev4Concluida { get; set; }
        public bool Rev5Concluida { get; set; }
        public bool Rev6Concluida { get; set; }
        public bool Rev7Concluida { get; set; }
        public bool Rev8Concluida { get; set; }
        public bool Rev9Concluida { get; set; }
        public bool Rev10Concluida { get; set; }
        public bool Rev11Concluida { get; set; }
        public bool Rev12Concluida { get; set; }
        public bool Rev13Concluida { get; set; }
        public bool Rev14Concluida { get; set; }
        public bool Rev15Concluida { get; set; }
        public bool Rev16Concluida { get; set; }
        public bool Rev17Concluida { get; set; }
        public bool Rev18Concluida { get; set; }
        public bool Rev19Concluida { get; set; }
        public bool Rev20Concluida { get; set; }
        public bool Rev21Concluida { get; set; }
        public bool Rev22Concluida { get; set; }
        public bool Rev23Concluida { get; set; }
        public bool Rev24Concluida { get; set; }
        public bool Rev25Concluida { get; set; }
        public bool Rev26Concluida { get; set; }
        public bool Rev27Concluida { get; set; }
        public bool Rev28Concluida { get; set; }
        public bool Rev29Concluida { get; set; }
        public bool Rev30Concluida { get; set; }

        // ── Acesso dinâmico por índice ────────────────────────────────────────────

        public DateTime GetDataRev(int n) => n switch
        {
            1 => DataRev1,
            2 => DataRev2,
            3 => DataRev3,
            4 => DataRev4,
            5 => DataRev5,
            6 => DataRev6,
            7 => DataRev7,
            8 => DataRev8,
            9 => DataRev9,
            10 => DataRev10,
            11 => DataRev11,
            12 => DataRev12,
            13 => DataRev13,
            14 => DataRev14,
            15 => DataRev15,
            16 => DataRev16,
            17 => DataRev17,
            18 => DataRev18,
            19 => DataRev19,
            20 => DataRev20,
            21 => DataRev21,
            22 => DataRev22,
            23 => DataRev23,
            24 => DataRev24,
            25 => DataRev25,
            26 => DataRev26,
            27 => DataRev27,
            28 => DataRev28,
            29 => DataRev29,
            30 => DataRev30,
            _ => DataInicio
        };

        public bool GetRevConcluida(int n) => n switch
        {
            1 => Rev1Concluida,
            2 => Rev2Concluida,
            3 => Rev3Concluida,
            4 => Rev4Concluida,
            5 => Rev5Concluida,
            6 => Rev6Concluida,
            7 => Rev7Concluida,
            8 => Rev8Concluida,
            9 => Rev9Concluida,
            10 => Rev10Concluida,
            11 => Rev11Concluida,
            12 => Rev12Concluida,
            13 => Rev13Concluida,
            14 => Rev14Concluida,
            15 => Rev15Concluida,
            16 => Rev16Concluida,
            17 => Rev17Concluida,
            18 => Rev18Concluida,
            19 => Rev19Concluida,
            20 => Rev20Concluida,
            21 => Rev21Concluida,
            22 => Rev22Concluida,
            23 => Rev23Concluida,
            24 => Rev24Concluida,
            25 => Rev25Concluida,
            26 => Rev26Concluida,
            27 => Rev27Concluida,
            28 => Rev28Concluida,
            29 => Rev29Concluida,
            30 => Rev30Concluida,
            _ => false
        };

        public void SetRevConcluida(int n, bool value)
        {
            switch (n)
            {
                case 1: Rev1Concluida = value; break;
                case 2: Rev2Concluida = value; break;
                case 3: Rev3Concluida = value; break;
                case 4: Rev4Concluida = value; break;
                case 5: Rev5Concluida = value; break;
                case 6: Rev6Concluida = value; break;
                case 7: Rev7Concluida = value; break;
                case 8: Rev8Concluida = value; break;
                case 9: Rev9Concluida = value; break;
                case 10: Rev10Concluida = value; break;
                case 11: Rev11Concluida = value; break;
                case 12: Rev12Concluida = value; break;
                case 13: Rev13Concluida = value; break;
                case 14: Rev14Concluida = value; break;
                case 15: Rev15Concluida = value; break;
                case 16: Rev16Concluida = value; break;
                case 17: Rev17Concluida = value; break;
                case 18: Rev18Concluida = value; break;
                case 19: Rev19Concluida = value; break;
                case 20: Rev20Concluida = value; break;
                case 21: Rev21Concluida = value; break;
                case 22: Rev22Concluida = value; break;
                case 23: Rev23Concluida = value; break;
                case 24: Rev24Concluida = value; break;
                case 25: Rev25Concluida = value; break;
                case 26: Rev26Concluida = value; break;
                case 27: Rev27Concluida = value; break;
                case 28: Rev28Concluida = value; break;
                case 29: Rev29Concluida = value; break;
                case 30: Rev30Concluida = value; break;
            }
        }

        public int GetIntervalo(int n) => n switch
        {
            1 => Int1,
            2 => Int2,
            3 => Int3,
            4 => Int4,
            5 => Int5,
            6 => Int6,
            7 => Int7,
            8 => Int8,
            9 => Int9,
            10 => Int10,
            11 => Int11,
            12 => Int12,
            13 => Int13,
            14 => Int14,
            15 => Int15,
            16 => Int16,
            17 => Int17,
            18 => Int18,
            19 => Int19,
            20 => Int20,
            21 => Int21,
            22 => Int22,
            23 => Int23,
            24 => Int24,
            25 => Int25,
            26 => Int26,
            27 => Int27,
            28 => Int28,
            29 => Int29,
            30 => Int30,
            _ => 30
        };

        public void SetIntervalo(int n, int value)
        {
            switch (n)
            {
                case 1: Int1 = value; break;
                case 2: Int2 = value; break;
                case 3: Int3 = value; break;
                case 4: Int4 = value; break;
                case 5: Int5 = value; break;
                case 6: Int6 = value; break;
                case 7: Int7 = value; break;
                case 8: Int8 = value; break;
                case 9: Int9 = value; break;
                case 10: Int10 = value; break;
                case 11: Int11 = value; break;
                case 12: Int12 = value; break;
                case 13: Int13 = value; break;
                case 14: Int14 = value; break;
                case 15: Int15 = value; break;
                case 16: Int16 = value; break;
                case 17: Int17 = value; break;
                case 18: Int18 = value; break;
                case 19: Int19 = value; break;
                case 20: Int20 = value; break;
                case 21: Int21 = value; break;
                case 22: Int22 = value; break;
                case 23: Int23 = value; break;
                case 24: Int24 = value; break;
                case 25: Int25 = value; break;
                case 26: Int26 = value; break;
                case 27: Int27 = value; break;
                case 28: Int28 = value; break;
                case 29: Int29 = value; break;
                case 30: Int30 = value; break;
            }
        }

        // ── INotifyPropertyChanged ────────────────────────────────────────────────

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// Notifica TODAS as revisões a partir do índice N (encadeamento em cascata).
        /// Quando Int3 muda, Rev3 muda, e como Rev4 depende de Rev3, Rev4 também muda, etc.
        /// </summary>
        private void NotificarDatasAPartirDe(int n)
        {
            for (int i = n; i <= 30; i++)
                OnPropertyChanged($"DataRev{i}");
        }

        private void NotificarDatas() => NotificarDatasAPartirDe(1);
    }
}
