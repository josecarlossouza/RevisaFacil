using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
            {
                _botClient = new TelegramBotClient(_botToken);
            }
        }

        private void CarregarConfiguracoes()
        {
            string caminhoArquivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "telegram_config.ini");

            try
            {
                if (!File.Exists(caminhoArquivo))
                {
                    System.Diagnostics.Debug.WriteLine("Arquivo .ini não encontrado.");
                    return;
                }

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
    }
}