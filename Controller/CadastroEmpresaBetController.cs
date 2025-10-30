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

    

        [HttpPost("Cadastrar")]
        public async Task<IActionResult> Cadastrar([FromForm] CadastroEmpresaBetDto dto, IFormFile contratoSocial)
        {
            if (contratoSocial == null || contratoSocial.Length == 0)
                return BadRequest(new { erro = "O contrato social (PDF) é obrigatório." });

            // Desserializa os sócios do JSON, se existir
            List<SocioDto>? socios = null;
            if (!string.IsNullOrWhiteSpace(dto.SociosJson))
            {
                socios = JsonConvert.DeserializeObject<List<SocioDto>>(dto.SociosJson);
            }

            using var ms = new MemoryStream();
            await contratoSocial.CopyToAsync(ms);
            byte[] pdfBytes = ms.ToArray();

            // Adiciona sócios desserializados no DTO
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

            await _empresaService.CadastrarEmpresa(empresa, pdfBytes);

            return Ok(new { mensagem = "Empresa cadastrada com sucesso!" });
        }

     
        [HttpPut("Atualizar")]
        public async Task<IActionResult> Atualizar(int idEmpresaBet, [FromBody] AtualizarEmpresaBetDto dto)
        {
            try
            {

                await _empresaService.AtualizarEmpresa(idEmpresaBet, dto);
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

        [HttpGet("ObterEmpresa")]
        public async Task<IActionResult> ObterEmpresa(int idEmpresaBet)
        {
            try
            {
                var empresa = await _empresaService.ObterEmpresa(idEmpresaBet);

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
