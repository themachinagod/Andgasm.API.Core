using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andgasm.API.Core
{
    public class EventGridControllerBase : ControllerBase
    {
        private bool EventTypeSubcriptionValidation
            => HttpContext.Request.Headers["aeg-event-type"].FirstOrDefault() ==
               "SubscriptionValidation";

        private bool EventTypeNotification
            => HttpContext.Request.Headers["aeg-event-type"].FirstOrDefault() ==
               "Notification";

        protected ILogger _logger { get; set; }

        public EventGridControllerBase(ILogger<ReportableControllerBase> logger) : base()
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("I am running!");
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var jsonContent = await reader.ReadToEndAsync();
                if (EventTypeSubcriptionValidation)
                {
                    return await HandleValidation(jsonContent);
                }
                else if (EventTypeNotification)
                {
                    return await HandleEvents(jsonContent);
                }
                return BadRequest();
            }
        }

        public virtual async Task<IActionResult> HandleEvents(string jsonContent)
        {
            await Task.CompletedTask;
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
