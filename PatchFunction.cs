using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace BTCert
{
    public static class PatchFunction
    {
        [FunctionName("PatchFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "patch", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var jsonbody = JsonConvert.DeserializeObject<PurchaseOrder>(requestBody);
            log.LogInformation($"Received request: {requestBody}");

            var regexDate = "/[0 - 9]{ 2}/[0 - 9]{ 2}/[0 - 9]{ 4}/";
            var regexQuantity = "/ ^(d)*(.)?([0 - 9]{ 1})?$/";
            var regexPrice = "/ ^[0 - 9] +.[0 - 9]{ 2}$/";
            Regex rgxDate = new Regex(regexDate);
            Regex rgxQuantity = new Regex(regexQuantity);
            Regex rgxPrice = new Regex(regexPrice);
            double TotalPriceSum = 0;

            // Check the request, make sure it is a compatible type
            if (req.ContentType != "application/json")
            {
                return new BadRequestObjectResult("Invalid content type, expected application/json");
            }

            foreach (var item in jsonbody.Customer.Items.Item)
            {
                var a = rgxQuantity.IsMatch(item.Quantity.ToString());
                if (rgxQuantity.IsMatch(item.Quantity.ToString()))
                {
                    return new BadRequestObjectResult("Not one decimal in Quantity");
                }
                if (rgxPrice.IsMatch(item.Price.ToString()))
                {
                    return new BadRequestObjectResult("Not two decimals in Price");
                }
                if (!String.IsNullOrEmpty(item.ShipDate))
                {
                    if (rgxDate.IsMatch(item.ShipDate))
                    {
                        return new BadRequestObjectResult("Error in ShipDate");
                    }
                }

                TotalPriceSum += (double)item.Price * (long)item.Quantity;
                TotalPriceSum = Math.Round(TotalPriceSum * 100) / 100;
            }

            if (jsonbody.Customer.TotalPrice != Math.Round(TotalPriceSum * 100) / 100)
            {
                return new BadRequestObjectResult("Wrong sum in TotalPrice");
            }
            if (String.IsNullOrEmpty(jsonbody.Customer.OrderNumber))
            {
                return new BadRequestObjectResult("Missing OrderNumber");
            }
            if (!jsonbody.Customer.OrderNumber.StartsWith("SE"))
            {
                return new BadRequestObjectResult("Error in OrderNumber");
            }
            else
            {
                // Set the type of response, sets the content type.
                //res.type('application/json');

                var OrderDate = "";
                if (!String.IsNullOrEmpty(jsonbody.Customer.OrderDate))
                {
                    OrderDate = jsonbody.Customer.OrderDate;
                    if (rgxDate.IsMatch(OrderDate))
                    {
                        return new BadRequestObjectResult("Error in OrderDate");
                    }
                }
                var Name = "";
                var Street = "";
                var City = "";
                var Zip = "";
                var Country = "";

                if (jsonbody.Customer.ShippingAddress!= null)
                {
                    if (!String.IsNullOrEmpty(jsonbody.Customer.ShippingAddress.Name))
                    {
                        Name = jsonbody.Customer.ShippingAddress.Name;
                    }

                    if (!String.IsNullOrEmpty(jsonbody.Customer.ShippingAddress.Address))
                    {
                        Street = jsonbody.Customer.ShippingAddress.Address;
                    }

                    if (!String.IsNullOrEmpty(jsonbody.Customer.ShippingAddress.City))
                    {
                        City = jsonbody.Customer.ShippingAddress.City;
                    }

                    if (!String.IsNullOrEmpty(jsonbody.Customer.ShippingAddress.Postalcode))
                    {
                        Zip = jsonbody.Customer.ShippingAddress.Postalcode;
                    }

                    if (!String.IsNullOrEmpty(jsonbody.Customer.ShippingAddress.Country))
                    {
                        Country = jsonbody.Customer.ShippingAddress.Country;
                    }
                }

                if (!double.IsNaN(jsonbody.Customer.TotalPrice) && jsonbody.Customer.Items != null)
                {
                    if (jsonbody.Customer.TotalPrice != Math.Round(TotalPriceSum * 100) / 100)
                    {
                        return new BadRequestObjectResult("Wrong sum in TotalPrice");
                    }
                }
                if (!double.IsNaN(jsonbody.Customer.TotalPrice))
                {
                    TotalPriceSum = jsonbody.Customer.TotalPrice;
                }

                // Send the response body.
                var jsonresponse = new PurchaseOrderResponse()
                {
                    OrderId = jsonbody.Customer.OrderNumber,
                    CreatedOn = jsonbody.Customer.OrderDate,
                    Value = Math.Round(TotalPriceSum * 100) / 100,
                    Shipping = new Shipping()
                    {
                        Name = Name,
                        Street = Street,
                        City = City,
                        Zip = Zip,
                        Country = Country
                    }
                };

                return new OkObjectResult(jsonresponse);
            }
        }
    }
}
