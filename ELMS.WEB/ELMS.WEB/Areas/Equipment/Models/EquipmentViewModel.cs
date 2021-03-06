﻿using ELMS.WEB.Enums.Equipment;
using ELMS.WEB.Models.General.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ELMS.WEB.Areas.Equipment.Models
{
    public class EquipmentViewModel
    {
        [Required]
        public string OwnerUID { get; set; }

        [Required]
        public Guid UID { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; } = "No description set.";

        [Required]
        [Display(Name = "Serial Number")]
        public string SerialNumber { get; set; }

        [Display(Name = "Purchase Price (£)")]
        public double PurchasePrice { get; set; }

        [Display(Name = "Replacement Price (£)")]
        public double ReplacementPrice { get; set; }

        [Display(Name = "Purchase Date")]
        public DateTime PurchaseDate { get; set; }

        [Required]
        [Display(Name = "Warranty Expiration Date")]
        public DateTime WarrantyExpirationDate { get; set; }

        [Required]
        public Status Status { get; set; } = Status.Available;

        public DateTime CreatedTimestamp { get; set; }
        public DateTime AmendedTimestamp { get; set; }
        public IList<BlobResponse> Blobs { get; set; } = new List<BlobResponse>();
    }
}