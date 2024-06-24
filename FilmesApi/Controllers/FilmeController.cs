using AutoMapper;
using FilmesApi.Data;
using FilmesApi.Data.Dtos;
using FilmesApi.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FilmesApi.Controllers;

[ApiController]
[Route("[controller]")]
public class FilmeController : ControllerBase
{

    private FilmeContext _context;
    private IMapper _mapper;

    public FilmeController(FilmeContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Adiciona um filme ao banco de dados
    /// </summary>
    /// <param name="filmeDto">Objeto com os campos necessários para criação de um filme</param>
    /// <returns>IActionResult</returns>
    /// <response code="201">Inserção realizada com sucesso</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult AdicionaFilme([FromBody] CreateFilmeDto filmeDto)
    {
        try
        {
            Filme filme = _mapper.Map<Filme>(filmeDto);
            _context.Filmes.Add(filme);
            _context.SaveChanges();
            return CreatedAtAction(nameof(RecuperaFilmePorId), new { id = filme.Id },
                filme);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao adicionar o filme");
        }

    }

    /// <summary>
    /// Recupera um enumerável de filmes armazenados no banco de dados
    /// </summary>
    /// <param name="skip">O índice inicial a partir do qual os filmes serão recuperados.</param>
    /// <param name="take">O número de filmes a serem recuperados a partir do índice de início.</param>
    /// <returns>IActionResult</returns>
    /// <response code="200">Consulta realizada com sucesso</response>
    /// <response code="500">Erro interno do servidor.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult RecuperaFilmes([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        try
        {
            var filmes = _mapper.Map<List<ReadFilmeDto>>(_context.Filmes.Skip(skip).Take(take));
            return Ok(filmes);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao recuperar filmes");
        }
    }

    /// <summary>
    /// Recupera um filme armazenado no banco de dados pelo seu ID.
    /// </summary>
    /// <param name="id">ID do filme a ser recuperado</param>
    /// <returns>IActionResult</returns>
    /// <response code="200">Consulta realizada com sucesso</response>
    /// <response code="404">Filme não encontrado ou não existente</response>
    /// <response code="500">Erro interno do servidor.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult RecuperaFilmePorId(int id)
    {
        try
        {
            var filme = _context.Filmes
            .FirstOrDefault(filme => filme.Id == id);
            if (filme == null) return NotFound();
            var filmeDto = _mapper.Map<ReadFilmeDto>(filme);
            return Ok(filmeDto);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao recuperar filmes");
        }
    }

    /// <summary>
    /// Atualiza os dados de um filme armazenado no banco de dados 
    /// </summary>
    /// <param name="id">ID do filme a ser atualizado totalmente</param>
    /// <param name="filmeDto">Objeto com os campos necessários para a atualização total dos dados de um filme</param>
    /// <returns>IActionResult</returns>
    /// <response code="204">Atualização total realizada com sucesso</response>
    /// <response code="404">Filme não encontrado ou não existente</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult AtualizarFilme(int id,
        [FromBody] UpdateFilmeDto filmeDto)
    {
        try
        {
            var filme = _context.Filmes.
            FirstOrDefault(filme => filme.Id == id);
            if (filme == null) return NotFound();
            _mapper.Map(filmeDto, filme);
            _context.SaveChanges();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao atualizar filmes");
        }
    }

    /// <summary>
    /// Atualiza parcialmente os dados de um filme armazenado no banco de dados 
    /// </summary>
    /// <param name="id">ID do filme a ser atualizado parcialmente</param>
    /// <param name="patch">Objeto com os campos necessários para a atualização parcial dos dados de um filme</param>
    /// <returns>IActionResult</returns>
    /// <response code="204">Atualização parcial realizada com sucesso</response>
    /// <response code="400">Ocorreu um erro com os dados fornecidos</response>
    /// <response code="404">Filme não encontrado ou não existente</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult AtualizaFilmeParcial(int id,
        JsonPatchDocument<UpdateFilmeDto> patch)
    {
        try
        {
            var filme = _context.Filmes.
            FirstOrDefault(filme => filme.Id == id);
            if (filme == null) return NotFound();
            var filmeParaAtualizar = _mapper.Map<UpdateFilmeDto>(filme);
            patch.ApplyTo(filmeParaAtualizar, ModelState);

            if (!TryValidateModel(filmeParaAtualizar))
            {
                return ValidationProblem();
            }
            _mapper.Map(filmeParaAtualizar, filme);
            _context.SaveChanges();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao atualizar o filme");
        }
        
    }

    /// <summary>
    /// Deleta um filme do banco de dados
    /// </summary>
    /// <param name="id">ID do filme a ser deletado do banco de dados</param>
    /// <returns>IActionResult</returns>
    /// <response code="204">Deleção dos dados realizada com sucesso</response>
    /// <response code="404">Filme não encontrado ou não existente</response>
    /// <response code="500">Erro interno do servidor.</response>
    [HttpDelete("{id}")]
    public IActionResult DeletaFilme(int id)
    {
        try
        {
            var filme = _context.Filmes.
            FirstOrDefault(filmes => filmes.Id == id);
            if (filme == null) return NotFound();
            _context.Remove(filme);
            _context.SaveChanges();
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao deletar o filme");

        }
    }
}
