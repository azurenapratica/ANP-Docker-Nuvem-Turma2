using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SiteContagem.Pages
{
    public class IndexModel : PageModel
    {
        private static readonly Contador _CONTADOR = new Contador();

        public void OnGet([FromServices]ILogger<IndexModel> logger,
            [FromServices]IConfiguration configuration)
        {
            lock (_CONTADOR)
            {
                _CONTADOR.Incrementar();
                logger.LogInformation($"Contador - Valor atual: {_CONTADOR.ValorAtual}");

                TempData["Contador"] = _CONTADOR.ValorAtual;
                TempData["Local"] = _CONTADOR.Local;
                TempData["Kernel"] = _CONTADOR.Kernel;
                TempData["TargetFramework"] = _CONTADOR.TargetFramework;
                TempData["MensagemFixa"] = "Teste";
                TempData["MensagemVariavel"] = configuration["MensagemVariavel"];
            }            
        }
    }
}