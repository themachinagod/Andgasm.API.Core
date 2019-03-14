using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Andgasm.API.Core
{
    public class APIController : ControllerBase
    {
        #region Properties
        protected ILogger _logger { get; set; }
        #endregion

        #region Constructors
        public APIController() : base()
        {
        }
        #endregion

        #region Request Helpers
        protected IActionResult InvalidIdBadRequest(int id)
        {
            return BadRequest($"The specified id '{id}' was not valid");
        }

        protected IActionResult NoPayloadBadRequest()
        {
            return BadRequest("No payload data was recieved to action the request");
        }

        protected IActionResult NoTakeSpecifiedBadRequest()
        {
            return BadRequest("Request must specify number of records to retrieve in the take option, must be greater than 0!");
        }

        protected IActionResult NoSkipSpecifiedBadRequest()
        {
            return BadRequest("Request must specify number of records to retrieve in the take option, must be greater than 0!");
        }

        protected IActionResult IdNotFound(int id)
        {
            return NotFound($"The specified id '{id}' was not found in the data store");
        }

        protected IActionResult PrimaryKeyConflict(int id)
        {
            return Conflict($"Cannot store to data store: Primary key '{id}' already exists!");
        }
        #endregion
    }
}
