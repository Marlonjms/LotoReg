using LotoReg.Helpers;
using LotoReg.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LotoReg.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisaoGeralController : ControllerBase
    {
        private readonly IVisaoGeral _visaoGeralService;

        public VisaoGeralController(IVisaoGeral visaoGeralService)
        {
            _visaoGeralService = visaoGeralService;
        }

      
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ObterVisaoGeral()
        {
            try
            {
                var idUsuario = UserHelper.ObterIdUsuarioLogado(HttpContext);

                if (idUsuario <= 0)
                    return Unauthorized(new { mensagem = "Usuário não autenticado." });

                var resultado = await _visaoGeralService.VisaoGeral(idUsuario);

                if (resultado == null)
                    return NotFound(new { mensagem = "Nenhum dado encontrado para este usuário." });

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = $"Erro ao obter visão geral: {ex.Message}" });
            }
        }
    }
}
