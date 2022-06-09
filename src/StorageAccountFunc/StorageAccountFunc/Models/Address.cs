using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EShopFunc.Models;
public class Address
{
    [JsonProperty("Street")]
    public string Street { get; set; }

    [JsonProperty("City")]
    public string City { get; set; }

    [JsonProperty("State")]
    public string State { get; set; }

    [JsonProperty("Country")]
    public string Country { get; set; }

    [JsonProperty("ZipCode")]
    public string ZipCode { get; set; }
}
