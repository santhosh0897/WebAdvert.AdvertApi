using AdvertApi.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdvertApi.Services
{
    public class DynamoDbStorageService : IAdvertStorageService
    {
        private readonly IMapper _mapper;

        public DynamoDbStorageService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<string> Add(AdvertModels advertModels)
        {
            var dbmodel = _mapper.Map<AdvertDbModel>(advertModels);

            dbmodel.Id = new Guid().ToString();
            dbmodel.MyCreationDateTime = DateTime.UtcNow;
            dbmodel.status = AdvertStatus.pending;

            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    await context.SaveAsync(dbmodel);

                }
            }
            return (dbmodel.Id);
        }

        public async Task<string> Confirm(ConfirmAdvertModel confirmAdvertModel)
        {
            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    var result = await context.LoadAsync<AdvertDbModel>(confirmAdvertModel);

                    if (result == null)
                    {
                        throw new KeyNotFoundException($"The record with ID = {confirmAdvertModel.Id} not found ");
                    }

                    if (confirmAdvertModel.Status == AdvertStatus.Active)
                    {
                        result.status = AdvertStatus.Active;
                        await context.SaveAsync(result);

                    }
                    else
                    {
                        await context.DeleteAsync(result);
                    }
                }
            }
            return (confirmAdvertModel.Id);
        }
    }
}
