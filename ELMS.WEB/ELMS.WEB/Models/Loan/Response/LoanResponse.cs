﻿using ELMS.WEB.Enums.Loan;
using ELMS.WEB.Models.Base.Response;
using System;
using System.Collections.Generic;

namespace ELMS.WEB.Models.Loan.Response
{
    public class LoanResponse : BaseResponse
    {
        public Guid UID { get; set; }
        public string Name { get; set; } = "Untitled";
        public Guid LoanerUID { get; set; }

        public Guid LoaneeUID { get; set; } = Guid.Empty;
        public string LoaneeEmail { get; set; }
        public DateTime FromTimestamp { get; set; } = DateTime.Now;
        public DateTime ExpiryTimestamp { get; set; }
        public Status Status { get; set; } = Status.Pending;
        public bool AcceptedTermsAndConditions { get; set; } = false;
        public IList<Guid> EquipmentList { get; set; }
        public DateTime CreatedTimestamp { get; set; }
        public DateTime AmendedTimestamp { get; set; }
    }
}