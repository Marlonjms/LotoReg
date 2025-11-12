using LotoReg.Dtos;
using LotoReg.Helpers;
using LotoReg.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LotoReg.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class RequerimentoSorteioController : ControllerBase
    {
        private readonly IRequerimentoSorteio _requerimentoService;

        public RequerimentoSorteioController(IRequerimentoSorteio requerimentoService)
        {
            _requerimentoService = requerimentoService;
        }


        [Authorize]
        [HttpPost("enviar")]
        public async Task<IActionResult> EnviarRequerimento([FromForm] EnviarRequerimentoDto dto)
        {
            try
            {
                var idUsuario = UserHelper.ObterIdUsuarioLogado(HttpContext);
                if (idUsuario <= 0)
                    return Unauthorized(new { mensagem = "Usuário não autenticado." });

                await _requerimentoService.EnviarRequerimento(dto, idUsuario);
                return Ok(new { mensagem = "Requerimento enviado com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }



        [Authorize]
        [HttpGet("BuscarTodos")]
        public async Task<IActionResult> ObterTodosRequerimentos()
        {
            try
            {
                var idUsuario = UserHelper.ObterIdUsuarioLogado(HttpContext);
                if (idUsuario <= 0)
                    return Unauthorized(new { mensagem = "Usuário não autenticado." });

                var requerimentos = await _requerimentoService.ObterTodosRequerimentos(idUsuario);
                return Ok(requerimentos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("ModelosRequerimeto")]
        [AllowAnonymous] // os modelos podem ser públicos
        public async Task<IActionResult> ObterTodosModelos()
        {
            try
            {
                var modelos = await _requerimentoService.ObterTodosModelos();
                return Ok(modelos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }


        [AllowAnonymous] // deixa assim, sem o [Authorize], já que o [AllowAnonymous] já libera
        [HttpGet("Baixar-ModeloRequerimeto/{id}")]
        public async Task<IActionResult> BaixarModeloPorId(int id)
        {
            try
            {
                var modelo = await _requerimentoService.BaixarModeloPorId(id);

                // usa o nome do banco, garantindo a extensão .pdf
                var nomeArquivo = modelo.Nome.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                    ? modelo.Nome
                    : $"{modelo.Nome}.pdf";

                return File(modelo.Arquivo, "application/pdf", nomeArquivo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("Admin-CadastrarModeloRequerimeto")]
        public async Task<IActionResult> CadastrarModelo([FromForm] ModeloRequerimentoDto dto)
        {
            try
            {
                await _requerimentoService.CadastrarModeloRequerimento(dto);
                return Ok(new { mensagem = "Modelo de requerimento cadastrado com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }


        [Authorize]
        [HttpDelete("Admin-DeletarModeloRequerimeto/{id}")]
        public async Task<IActionResult> DeletarModelo(int id)
        {
            try
            {
                await _requerimentoService.DeletarModeloPorId(id);
                return Ok(new { mensagem = "Modelo excluído com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}
