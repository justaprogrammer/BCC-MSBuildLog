﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MSBLOC.Web.Models
{
    public class SubmissionData
    {
        [Required]
        public string ApplicationOwner { get; set; }
        [Required]
        public string ApplicationName { get; set; }
        [Required]
        public string CommitSha { get; set; }
        [Required]
        public string CloneRoot { get; set; }
        [Required]
        public string BinaryLogFile { get; set; }
    }
}
