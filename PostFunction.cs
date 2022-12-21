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
    public static class PostFunction
    {
        [FunctionName("PostFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
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
                if (String.IsNullOrEmpty(item.Product))
                {
                    return new BadRequestObjectResult("Missing Product in Item");
                }
                if (item.Quantity == null)
                {
                    return new BadRequestObjectResult("Missing Quantity in Item");
                }
                if (rgxQuantity.IsMatch(item.Quantity.ToString()))
                {
                    return new BadRequestObjectResult("Not one decimal in Quantity");
                }
                if (item.Price == null)
                {
                    return new BadRequestObjectResult("Missing Price in Item");
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

            if (String.IsNullOrEmpty(jsonbody.Customer.OrderDate))
            {
                return new BadRequestObjectResult("Missing OrderDate");
            }
            if (rgxDate.IsMatch(jsonbody.Customer.OrderDate))
            {
                return new BadRequestObjectResult("Error in OrderDate");
            }
            if (jsonbody.Customer.ShippingAddress == null)
            {
                return new BadRequestObjectResult("Missing ShippingAddress");
            }
            if (String.IsNullOrEmpty(jsonbody.Customer.ShippingAddress.Name))
            {
                return new BadRequestObjectResult("Missing Name in ShippingAddress");
            }
            if (String.IsNullOrEmpty(jsonbody.Customer.ShippingAddress.Address))
            {
                return new BadRequestObjectResult("Missing Address in ShippingAddress");
            }
            if (String.IsNullOrEmpty(jsonbody.Customer.ShippingAddress.City))
            {
                return new BadRequestObjectResult("Missing City in ShippingAddress");
            }
            if (String.IsNullOrEmpty(jsonbody.Customer.ShippingAddress.Postalcode))
            {
                return new BadRequestObjectResult("Missing PostalCode in ShippingAddress");
            }
            if (String.IsNullOrEmpty(jsonbody.Customer.ShippingAddress.Country))
            {
                return new BadRequestObjectResult("Missing Country in ShippingAddress");
            }

            if (jsonbody.Customer.Items == null)
            {
                return new BadRequestObjectResult("Missing Items");
            }
            if (jsonbody.Customer.Items.Item == null)
            {
                return new BadRequestObjectResult("Missing Item in Items");
            }
            else
            {
                //        // Set the type of response, sets the content type.
                //        res.type('application/json');

                //        // Set the status code of the response.
                //        res.status(200);

                var jsonresponse = new PurchaseOrderResponse()
                {
                    CreatedOn = jsonbody.Customer.OrderDate,
                    OrderId = jsonbody.Customer.OrderNumber,
                    Value = Math.Round(TotalPriceSum * 100) / 100,
                    Shipping = new Shipping(){
                        Name = jsonbody.Customer.ShippingAddress.Name,
                        Street = jsonbody.Customer.ShippingAddress.Address,
                        City = jsonbody.Customer.ShippingAddress.City,
                        Zip = jsonbody.Customer.ShippingAddress.Postalcode,
                        Country = jsonbody.Customer.ShippingAddress.Country
                    }
                };

                return new OkObjectResult(jsonresponse);
            }
        }
    }
}




