using Microsoft.AspNetCore.Mvc;
using system_copsoq_api.Models; // Precisa disso para o Enum
using System; // Precisa disso para o Enum

namespace system_copsoq_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilsController : ControllerBase
    {
        // GET: api/utils/setores
        [HttpGet("setores")]
        public IActionResult GetSetoresAtuacao()
        {
            // Pega todos os nomes (como string) do seu Enum
            var setores = Enum.GetNames(typeof(SetorAtuacao)); 
            
            // Retorna o array de strings: 
            // ["Industria", "Comercio", "Saude", ...]
            return Ok(setores); 
        }
    }
}