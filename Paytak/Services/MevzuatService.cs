using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Hosting;

namespace Paytak.Services
{
    public class MevzuatService : IMevzuatService
    {
        private readonly string _mevzuatDirectory;
        private readonly object _cacheLock = new();
        private bool _isLoaded;
        private readonly List<(string FileName, string Content)> _mevzuatCache = new();

        public MevzuatService(IHostEnvironment environment)
        {
            // wwwroot/mevzuatlar klasörü
            _mevzuatDirectory = Path.Combine(environment.ContentRootPath, "wwwroot", "mevzuatlar");
        }

        public Task<string> GetRelevantMevzuatAsync(string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                return Task.FromResult(string.Empty);

            EnsureLoaded();

            if (_mevzuatCache.Count == 0)
                return Task.FromResult(string.Empty);

            // Çok basit bir anahtar kelime skorlama yaklaşımı
            var keywords = question
                .Split(new[] { ' ', '\t', '\r', '\n', ',', '.', ';', ':', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(k => k.Length > 3)
                .Select(k => k.ToLowerInvariant())
                .Distinct()
                .ToList();

            if (keywords.Count == 0)
                return Task.FromResult(string.Empty);

            var scored = _mevzuatCache
                .Select(m => new
                {
                    m.FileName,
                    m.Content,
                    Score = ScoreText(m.Content, keywords)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Take(3) // En alakalı ilk 3 doküman
                .ToList();

            if (scored.Count == 0)
                return Task.FromResult(string.Empty);

            var sb = new StringBuilder();
            foreach (var item in scored)
            {
                sb.AppendLine($"[Kaynak: {item.FileName}]");

                // Token şişmesini engellemek için her dokümandan ilk ~2000 karakteri al
                var content = item.Content;
                var maxLen = 2000;
                if (content.Length > maxLen)
                {
                    content = content[..maxLen] + "...";
                }

                sb.AppendLine(content);
                sb.AppendLine();
            }

            return Task.FromResult(sb.ToString());
        }

        private void EnsureLoaded()
        {
            if (_isLoaded)
                return;

            lock (_cacheLock)
            {
                if (_isLoaded)
                    return;

                if (!Directory.Exists(_mevzuatDirectory))
                {
                    _isLoaded = true;
                    return;
                }

                // Şimdilik sadece .docx dosyalarını güvenilir şekilde okuyabiliyoruz
                var files = Directory.GetFiles(_mevzuatDirectory, "*.docx");

                foreach (var file in files)
                {
                    try
                    {
                        var text = ExtractTextFromDocx(file);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            _mevzuatCache.Add((Path.GetFileName(file), text));
                        }
                    }
                    catch
                    {
                        // Tek bir dosya okunamazsa tüm sistemi bozmamak için hatayı yut
                    }
                }

                _isLoaded = true;
            }
        }

        private static int ScoreText(string content, List<string> keywords)
        {
            if (string.IsNullOrWhiteSpace(content))
                return 0;

            var lower = content.ToLowerInvariant();
            var score = 0;

            foreach (var keyword in keywords)
            {
                var index = 0;
                while (true)
                {
                    index = lower.IndexOf(keyword, index, StringComparison.Ordinal);
                    if (index == -1)
                        break;

                    score++;
                    index += keyword.Length;
                }
            }

            return score;
        }

        private static string ExtractTextFromDocx(string path)
        {
            using var doc = WordprocessingDocument.Open(path, false);
            var body = doc.MainDocumentPart?.Document.Body;
            if (body == null)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var paragraph in body.Descendants<Paragraph>())
            {
                var text = string.Concat(paragraph.Descendants<Text>().Select(t => t.Text));
                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine(text);
                }
            }

            return sb.ToString();
        }
    }
}

