using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvertApi.Models;
using AdvertApi.Services;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AdvertApi.Controllers
{

    [ApiController]
    [Route("adverts/v1")]
    public class Advert : ControllerBase
    {
        private readonly IAdvertStorageService _advertStorageService;

        public Advert(IAdvertStorageService advertStorageService)
        {

            _advertStorageService = advertStorageService;
        }

        [HttpPost]
        [Route("Create")]
        [ProducesResponseType(404)]
        [ProducesResponseType(201, Type =typeof( CreateAdvertResponse))]


        public async Task <IActionResult> Create(AdvertModels models)
        {
            string recordId;
            try
            {
                 recordId = await _advertStorageService.Add(models);
            }
            catch (KeyNotFoundException)
            {
                return new NotFoundResult();
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
                
            }
            return StatusCode(201, new CreateAdvertResponse { Id = recordId });
        }

        [HttpPut]
        [Route("Confirm")]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Confirm(ConfirmAdvertModel model)
        {
            try
            {
                var result = await _advertStorageService.Confirm(model);
            }

            catch (KeyNotFoundException)
            {
                return new NotFoundResult();
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
            return new OkResult();
        }

    }
}
