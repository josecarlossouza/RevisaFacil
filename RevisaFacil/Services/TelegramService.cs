using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using RevisaFacil.Data;

namespace RevisaFacil.Services
{
    public class TelegramService
    {
        private readonly ITelegramBotClient _botClient;
        private string _botToken;
        private string _chatId;
        private bool _estaConfigurado = false;

        public TelegramService()
        {
            CarregarConfiguracoes();
            if (_estaConfigurado)
                _botClient = new TelegramBotClient(_botToken);
        }

        private void CarregarConfiguracoes()
        {
            string caminhoArquivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "telegram_config.ini");
            try
            {
                if (!File.Exists(caminhoArquivo)) return;

                var linhas = File.ReadAllLines(caminhoArquivo);
                foreach (var linha in linhas)
                {
                    string linhaLimpa = linha.Trim();
                    if (string.IsNullOrWhiteSpace(linhaLimpa) || linhaLimpa.StartsWith("#") || linhaLimpa.StartsWith("["))
                        continue;

                    var partes = linhaLimpa.Split('=', 2);
                    if (partes.Length < 2) continue;

                    string chave = partes[0].Trim().ToLower();
                    string valor = partes[1].Trim();

                    if (chave == "token") _botToken = valor;
                    if (chave == "chatid") _chatId = valor;
                }

                if (!string.IsNullOrEmpty(_botToken) && _botToken != "seu_token_aqui" &&
                    !string.IsNullOrEmpty(_chatId) && _chatId != "seu_chat_id_aqui")
                {
                    _estaConfigurado = true;
                }
            }
            catch (Exception ex)
            {
                _estaConfigurado = false;
                System.Diagnostics.Debug.WriteLine($"[AVISO TELEGRAM] Erro na config: {ex.Message}");
            }
        }

        // Método original: alerta de revisões pendentes/atrasadas
        public async Task EnviarAlerta(string mensagem)
        {
            if (!_estaConfigurado) return;
            try
            {
                await _botClient.SendMessage(
                    chatId: _chatId,
                    text: mensagem,
                    parseMode: ParseMode.Markdown
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERRO TELEGRAM] {ex.Message}");
            }
        }

        // Novo método: envia o resumo das anotações do calendário para o dia atual
        public async Task EnviarResumoCalendarioHoje()
        {
            if (!_estaConfigurado) return;

            try
            {
                var hoje = DateTime.Today;

                using (var db = new EstudoDbContext())
                {
                    // Busca todas as notas do dia (revisões automáticas e manuais)
                    var notasHoje = db.NotasCalendario
                        .Where(n => n.Data.Date == hoje)
                        .ToList();

                    if (!notasHoje.Any()) return; // Sem notas, não envia nada

                    var sb = new StringBuilder();
                    sb.AppendLine($"📅 *AGENDA DO DIA — {hoje:dd/MM/yyyy}*");
                    sb.AppendLine();

                    // Revisões automáticas (AssuntoId = -1)
                    var notaRevisao = notasHoje.FirstOrDefault(n => n.AssuntoId == -1);
                    if (notaRevisao != null && !string.IsNullOrWhiteSpace(notaRevisao.Conteudo))
                    {
                        sb.AppendLine("📚 *REVISÕES DO DIA:*");
                        foreach (var linha in notaRevisao.Conteudo.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                            sb.AppendLine($"• {linha.Trim()}");
                        sb.AppendLine();
                    }

                    // Anotação manual do usuário (AssuntoId = -2)
                    var notaManual = notasHoje.FirstOrDefault(n => n.AssuntoId == -2);
                    if (notaManual != null && !string.IsNullOrWhiteSpace(notaManual.Conteudo))
                    {
                        sb.AppendLine("📝 *ANOTAÇÕES PESSOAIS:*");
                        sb.AppendLine(notaManual.Conteudo.Trim());
                    }

                    await _botClient.SendMessage(
                        chatId: _chatId,
                        text: sb.ToString(),
                        parseMode: ParseMode.Markdown
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERRO TELEGRAM - Calendário] {ex.Message}");
            }
        }
    }
}