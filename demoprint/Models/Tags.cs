using System;
using System.Collections.Generic;

namespace demoprint.Models
{
    public class Tag
    {
		public string ItemNumber { get; set; }
		public string ItemDescription { get; set; }
		public int Quantity { get; set; }
		public int OriginalQuantity { get; set; }
		public string FormattedCustomer { get; set; }

		public string Upc { get; set; }
		public string RetailUnit { get; set; }
		public string Size { get; set; }
		public string RetailPrice { get; set; }
		public string ItemNumberAndCheckDigit { get; set; }

		public bool IsRetailPrinting { get; set; } = false;
	}

	public class TagSet
    {
		public string SetTitle { get; set; }
		public DateTime CreatedOnDate { get; set; }
		public List<Tag> AllTags { get; set; }
    }
}
