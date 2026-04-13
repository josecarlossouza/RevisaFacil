// Assunto.cs
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

        // Intervalos em dias (padrão: 30, 60, 90, ..., 300)
        private int _int1 = 30; public int Int1 { get => _int1; set { _int1 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev1)); } }
        private int _int2 = 60; public int Int2 { get => _int2; set { _int2 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev2)); } }
        private int _int3 = 90; public int Int3 { get => _int3; set { _int3 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev3)); } }
        private int _int4 = 120; public int Int4 { get => _int4; set { _int4 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev4)); } }
        private int _int5 = 150; public int Int5 { get => _int5; set { _int5 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev5)); } }
        private int _int6 = 180; public int Int6 { get => _int6; set { _int6 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev6)); } }
        private int _int7 = 210; public int Int7 { get => _int7; set { _int7 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev7)); } }
        private int _int8 = 240; public int Int8 { get => _int8; set { _int8 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev8)); } }
        private int _int9 = 270; public int Int9 { get => _int9; set { _int9 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev9)); } }
        private int _int10 = 300; public int Int10 { get => _int10; set { _int10 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev10)); } }
        private int _int11 = 330; public int Int11 { get => _int11; set { _int11 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev11)); } }
        private int _int12 = 360; public int Int12 { get => _int12; set { _int12 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev12)); } }
        private int _int13 = 390; public int Int13 { get => _int13; set { _int13 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev13)); } }
        private int _int14 = 420; public int Int14 { get => _int14; set { _int14 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev14)); } }
        private int _int15 = 450; public int Int15 { get => _int15; set { _int15 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev15)); } }
        private int _int16 = 480; public int Int16 { get => _int16; set { _int16 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev16)); } }
        private int _int17 = 510; public int Int17 { get => _int17; set { _int17 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev17)); } }
        private int _int18 = 540; public int Int18 { get => _int18; set { _int18 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev18)); } }
        private int _int19 = 570; public int Int19 { get => _int19; set { _int19 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev19)); } }
        private int _int20 = 600; public int Int20 { get => _int20; set { _int20 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev20)); } }
        private int _int21 = 630; public int Int21 { get => _int21; set { _int21 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev21)); } }
        private int _int22 = 660; public int Int22 { get => _int22; set { _int22 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev22)); } }
        private int _int23 = 690; public int Int23 { get => _int23; set { _int23 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev23)); } }
        private int _int24 = 720; public int Int24 { get => _int24; set { _int24 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev24)); } }
        private int _int25 = 750; public int Int25 { get => _int25; set { _int25 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev25)); } }
        private int _int26 = 780; public int Int26 { get => _int26; set { _int26 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev26)); } }
        private int _int27 = 810; public int Int27 { get => _int27; set { _int27 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev27)); } }
        private int _int28 = 840; public int Int28 { get => _int28; set { _int28 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev28)); } }
        private int _int29 = 870; public int Int29 { get => _int29; set { _int29 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev29)); } }
        private int _int30 = 900; public int Int30 { get => _int30; set { _int30 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev30)); } }

        // Datas calculadas — [NotMapped] é válido aqui pois são propriedades
        [NotMapped] public DateTime DataRev1 => DataInicio.AddDays(Int1);
        [NotMapped] public DateTime DataRev2 => DataInicio.AddDays(Int2);
        [NotMapped] public DateTime DataRev3 => DataInicio.AddDays(Int3);
        [NotMapped] public DateTime DataRev4 => DataInicio.AddDays(Int4);
        [NotMapped] public DateTime DataRev5 => DataInicio.AddDays(Int5);
        [NotMapped] public DateTime DataRev6 => DataInicio.AddDays(Int6);
        [NotMapped] public DateTime DataRev7 => DataInicio.AddDays(Int7);
        [NotMapped] public DateTime DataRev8 => DataInicio.AddDays(Int8);
        [NotMapped] public DateTime DataRev9 => DataInicio.AddDays(Int9);
        [NotMapped] public DateTime DataRev10 => DataInicio.AddDays(Int10);
        [NotMapped] public DateTime DataRev11 => DataInicio.AddDays(Int11);
        [NotMapped] public DateTime DataRev12 => DataInicio.AddDays(Int12);
        [NotMapped] public DateTime DataRev13 => DataInicio.AddDays(Int13);
        [NotMapped] public DateTime DataRev14 => DataInicio.AddDays(Int14);
        [NotMapped] public DateTime DataRev15 => DataInicio.AddDays(Int15);
        [NotMapped] public DateTime DataRev16 => DataInicio.AddDays(Int16);
        [NotMapped] public DateTime DataRev17 => DataInicio.AddDays(Int17);
        [NotMapped] public DateTime DataRev18 => DataInicio.AddDays(Int18);
        [NotMapped] public DateTime DataRev19 => DataInicio.AddDays(Int19);
        [NotMapped] public DateTime DataRev20 => DataInicio.AddDays(Int20);
        [NotMapped] public DateTime DataRev21 => DataInicio.AddDays(Int21);
        [NotMapped] public DateTime DataRev22 => DataInicio.AddDays(Int22);
        [NotMapped] public DateTime DataRev23 => DataInicio.AddDays(Int23);
        [NotMapped] public DateTime DataRev24 => DataInicio.AddDays(Int24);
        [NotMapped] public DateTime DataRev25 => DataInicio.AddDays(Int25);
        [NotMapped] public DateTime DataRev26 => DataInicio.AddDays(Int26);
        [NotMapped] public DateTime DataRev27 => DataInicio.AddDays(Int27);
        [NotMapped] public DateTime DataRev28 => DataInicio.AddDays(Int28);
        [NotMapped] public DateTime DataRev29 => DataInicio.AddDays(Int29);
        [NotMapped] public DateTime DataRev30 => DataInicio.AddDays(Int30);

        // Status de conclusão
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

        // ── Acesso dinâmico por índice (métodos normais, sem [NotMapped]) ─────────

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

        private void NotificarDatas()
        {
            for (int i = 1; i <= 30; i++)
                OnPropertyChanged($"DataRev{i}");
        }
    }
}
