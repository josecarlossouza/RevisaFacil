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

        private int _int1 = 30; public int Int1 { get => _int1; set { _int1 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev1)); } }
        private int _int2 = 60; public int Int2 { get => _int2; set { _int2 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev2)); } }
        private int _int3 = 90; public int Int3 { get => _int3; set { _int3 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev3)); } }
        private int _int4 = 120; public int Int4 { get => _int4; set { _int4 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev4)); } }
        private int _int5 = 150; public int Int5 { get => _int5; set { _int5 = value; OnPropertyChanged(); OnPropertyChanged(nameof(DataRev5)); } }

        [NotMapped] public DateTime DataRev1 => DataInicio.AddDays(Int1);
        [NotMapped] public DateTime DataRev2 => DataInicio.AddDays(Int2);
        [NotMapped] public DateTime DataRev3 => DataInicio.AddDays(Int3);
        [NotMapped] public DateTime DataRev4 => DataInicio.AddDays(Int4);
        [NotMapped] public DateTime DataRev5 => DataInicio.AddDays(Int5);

        public bool Rev1Concluida { get; set; }
        public bool Rev2Concluida { get; set; }
        public bool Rev3Concluida { get; set; }
        public bool Rev4Concluida { get; set; }
        public bool Rev5Concluida { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void NotificarDatas()
        {
            OnPropertyChanged(nameof(DataRev1)); OnPropertyChanged(nameof(DataRev2));
            OnPropertyChanged(nameof(DataRev3)); OnPropertyChanged(nameof(DataRev4));
            OnPropertyChanged(nameof(DataRev5));
        }
    }
}