﻿using ELMS.WEB.Enums.Email;
using System;
using System.ComponentModel.DataAnnotations;

namespace ELMS.WEB.Models.Email.Request
{
    public class UpdateEmailTemplateRequest
    {
        [Required]
        public Guid UID { get; set; }

        [Required]
        public string Name { get; set; }

        public string Header { get; set; }

        public string Subheader { get; set; }

        [Required]
        public EmailTemplateType TemplateType { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }

        public string Footer { get; set; }
    }
}