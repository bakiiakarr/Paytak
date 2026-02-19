using System.Threading.Tasks;

namespace Paytak.Services
{
    public interface IMevzuatService
    {
        /// <summary>
        /// Kullanıcının sorusuna en çok benzeyen mevzuat bölümlerini tek bir metin olarak döner.
        /// </summary>
        /// <param name="question">Kullanıcının doğal dilde sorusu.</param>
        /// <returns>İlgili mevzuat metinlerinden derlenmiş özet metin (boş string olabilir).</returns>
        Task<string> GetRelevantMevzuatAsync(string question);
    }
}
