using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andgasm.API.Core
{
    public class EventGridControllerBase : ControllerBase
    {
        protected ILogger _logger { get; set; }

        public EventGridControllerBase(ILogger<ReportableControllerBase> logger) : base()
        {
            _logger = logger;
        }

        public virtual async Task<IActionResult> HandleEvents(string jsonContent)
        {
            throw new NotImplementedException("No base implementation exists for handling of event grid messages!");
        }

        public async Task<JsonResult> HandleValidation(string jsonContent)
        {
            var gridEvent = await Task.Run(() => JsonConvert.DeserializeObject<List<GridEvent<Dictionary<string, string>>>>(jsonContent).First());
            var validationCode = gridEvent.Data["validationCode"];
            return new JsonResult(new
            {
                validationResponse = validationCode
            });
        }
    }
}
