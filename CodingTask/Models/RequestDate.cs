﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodingTask.Models
{
    public class RequestDate
    {
        private DateTime _timestamp;
        public DateTime Timestamp { 
            get {
                return _timestamp;
            }
            set {
                _timestamp = new DateTime(value.Ticks, DateTimeKind.Utc);
            } 
        }
    }
}
