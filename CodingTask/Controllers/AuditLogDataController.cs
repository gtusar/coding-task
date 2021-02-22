using CodingTask.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodingTask.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuditLogDataController : ControllerBase
    {
        [HttpGet]
        public ActionResult<List<AuditLogTimestamp>> GetAll()
        {
            return OrderbookLog.GetAvailableTimestamps();
        }
    }
}
