﻿using AdvertApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvertApi.Services
{
   public interface IAdvertStorageService
    {

        Task<string> Add(AdvertModels advertModels);

        Task<string> Confirm(ConfirmAdvertModel confirmAdvertModel);

        Task<AdvertModels> GetIdAsync(string id);

        Task<bool> CheckHealthAsync();
    }
}
