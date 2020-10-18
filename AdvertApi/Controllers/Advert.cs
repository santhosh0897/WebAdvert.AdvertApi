using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvertApi.Models;
using AdvertApi.Models.Message;
using AdvertApi.Services;
using Amazon.DynamoDBv2;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Util;
//using AutoMapper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace AdvertApi.Controllers
{

    [ApiController]
    [Route("adverts/v1")]
    [Produces("application/json")]
    public class Advert : ControllerBase
    {
        private readonly IAdvertStorageService _advertStorageService;
        private readonly IConfiguration Configuration;

        public Advert(IAdvertStorageService advertStorageService, IConfiguration configuration )
        {

            _advertStorageService = advertStorageService;
            Configuration = configuration;
           
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

        private async Task RaiseConfirmAdvertMessage(ConfirmAdvertModel models)
        {
            var typeArn = Configuration.GetValue<string>("TopicArn");
            var dbmodel = await _advertStorageService.GetIdAsync(models.Id);

          using (var client = new AmazonSimpleNotificationServiceClient())
            {
                var message = new ConfirmAdvertMessage {
                    id = models.Id,
                    Title = dbmodel.Title
                };

                var messagejson = JsonConvert.SerializeObject(message);
                await client.PublishAsync(typeArn, messagejson);
            }
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
                await RaiseConfirmAdvertMessage(model);
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
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Get(string id)
        {
            try
            {
                var advert = await _advertStorageService.GetIdAsync(id);
                return new JsonResult(advert);
            }
            catch (KeyNotFoundException)
            {
                return new NotFoundResult();
            }
            catch (Exception)
            {
                return new StatusCodeResult(500);
            }
        }
    }
}
