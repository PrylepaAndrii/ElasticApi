using ElasticApi.DTOs;
using ElasticApi.Services;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ElasticApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SearchController : ControllerBase
    {
        ISuggesstionService _service;
        public SearchController(ISuggesstionService service )
        {
            _service = service;
        }
        [HttpGet("SearchLine")]
        public async Task<IActionResult> Search(string SearchLine)
        {
            var result = await _service.suggest(SearchLine);
            if (result.Item2 == null && result.Item1 != null)
                return Ok(result.Item1);
            else if (result.Item1 == null && result.Item2 != null)
                return Ok(result.Item2);
            else
                return NotFound("No suuch infor, search again");
        }
        [HttpGet("AutocompleteLine")]
        public async Task<IActionResult> AutoComplete(string AutocompleteLine)
        {
            var result = await _service.autocompletePlace(AutocompleteLine);
            if (result != null)
                return Ok(result);
            else
                return NotFound();
        }
    }
}
