using LotoReg.Dtos;
using LotoReg.Helpers;
using LotoReg.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json; // Para desserializar o JSON da lista de sócios

namespace LotoReg.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CadastroEmpresaBetController : ControllerBase
    {
        private readonly ICadastroEmpresaBet _empresaService;

        public CadastroEmpresaBetController(ICadastroEmpresaBet empresaService)
        {
            _empresaService = empresaService;
        }

        /// <summary>
        ///ex: socios[{"Nome":"João"},{"Nome":"Maria"}]
        /// </summary>
        [Authorize]
        [HttpPost("Cadastrar")]
        public async Task<IActionResult> Cadastrar([FromForm] CadastroEmpresaBetDto dto, IFormFile contratoSocial)
        {
            try
            {
                var idUsuario = UserHelper.ObterIdUsuarioLogado(HttpContext);
                if (idUsuario <= 0)
                    return Unauthorized(new { mensagem = "Usuário não autenticado." });

                if (contratoSocial == null || contratoSocial.Length == 0)
                    return BadRequest(new { erro = "O contrato social (PDF) é obrigatório." });

                List<SocioDto>? socios = null;
                if (!string.IsNullOrWhiteSpace(dto.SociosJson))
                {
                    socios = JsonConvert.DeserializeObject<List<SocioDto>>(dto.SociosJson);
                }

                using var ms = new MemoryStream();
                await contratoSocial.CopyToAsync(ms);
                byte[] pdfBytes = ms.ToArray();

                var empresa = new CadastroEmpresaBetDto
                {
                    RazaoSocial = dto.RazaoSocial,
                    CNPJ = dto.CNPJ,
                    DataFundacao = dto.DataFundacao,
                    Estado = dto.Estado,
                    EnderecoCompleto = dto.EnderecoCompleto,
                    TelefoneComercial = dto.TelefoneComercial,
                    EmailContato = dto.EmailContato,
                    SitePlataforma = dto.SitePlataforma,
                    SociosJson = dto.SociosJson,
                    Socios = socios
                };

                await _empresaService.CadastrarEmpresa(empresa, pdfBytes, idUsuario);

                return Ok(new { mensagem = "Empresa cadastrada com sucesso!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("Atualizar")]
        public async Task<IActionResult> Atualizar([FromBody] AtualizarEmpresaBetDto dto)
        {
            try
            {
                var idUsuario = UserHelper.ObterIdUsuarioLogado(HttpContext);
                if (idUsuario <= 0)
                    return Unauthorized(new { mensagem = "Usuário não autenticado." });

                await _empresaService.AtualizarEmpresa(idUsuario, dto);
                return Ok(new { mensagem = "Dados da empresa atualizados com sucesso!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("ObterEmpresa")]
        public async Task<IActionResult> ObterEmpresa()
        {
            try
            {
                var idUsuario = UserHelper.ObterIdUsuarioLogado(HttpContext);
                if (idUsuario <= 0)
                    return Unauthorized(new { mensagem = "Usuário não autenticado." });

                var empresa = await _empresaService.ObterEmpresa(idUsuario);

                if (empresa == null)
                    return NotFound(new { mensagem = "Empresa não encontrada." });

                return Ok(empresa);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message });
            }
        }

    }

}
