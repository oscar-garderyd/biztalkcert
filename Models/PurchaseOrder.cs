using Newtonsoft.Json;
using System.Collections.Generic;

namespace BTCert
{
    public partial class PurchaseOrder
    {
        [JsonProperty("Customer")]
        public Customer Customer { get; set; }
    }

    public partial class Customer
    {
        [JsonProperty("OrderNumber")]
        public string OrderNumber { get; set; }

        [JsonProperty("OrderDate")]
        public string OrderDate { get; set; }

        [JsonProperty("ShippingAddress")]
        public IngAddress ShippingAddress { get; set; }

        [JsonProperty("BillingAddress")]
        public IngAddress BillingAddress { get; set; }

        [JsonProperty("DeliveryNotes")]
        public string DeliveryNotes { get; set; }

        [JsonProperty("TotalPrice")]
        public double TotalPrice { get; set; }

        [JsonProperty("Items")]
        public Items Items { get; set; }
    }

    public partial class IngAddress
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Address")]
        public string Address { get; set; }

        [JsonProperty("City")]
        public string City { get; set; }

        [JsonProperty("Postalcode")]
        public string Postalcode { get; set; }

        [JsonProperty("Country")]
        public string Country { get; set; }
    }

    public partial class Items
    {
        [JsonProperty("Item")]
        public List<Item> Item { get; set; }
    }

    public partial class Item
    {
        [JsonProperty("Product")]
        public string Product { get; set; }

        [JsonProperty("Quantity")]
        public long? Quantity { get; set; }

        [JsonProperty("Price")]
        public double? Price { get; set; }

        [JsonProperty("ShipDate")]
        public string ShipDate { get; set; }

        [JsonProperty("Comment")]
        public string Comment { get; set; }
    }

    public partial class PurchaseOrderResponse
    {
        [JsonProperty("OrderId")]
        public string OrderId { get; set; }

        [JsonProperty("CreatedOn")]
        public string CreatedOn { get; set; }

        [JsonProperty("Value")]
        public double Value { get; set; }

        [JsonProperty("Shipping")]
        public Shipping Shipping { get; set; }
    }

    public partial class Shipping
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Street")]
        public string Street { get; set; }

        [JsonProperty("City")]
        public string City { get; set; }

        [JsonProperty("Zip")]
        public string Zip { get; set; }

        [JsonProperty("Country")]
        public string Country { get; set; }
    }
}
