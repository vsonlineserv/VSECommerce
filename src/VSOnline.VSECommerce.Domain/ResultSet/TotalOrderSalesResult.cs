﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class TotalOrderSalesResult
    {
        public int OrderCount { get; set; }
        public decimal? SalesTotal { get; set; }
    }
}
