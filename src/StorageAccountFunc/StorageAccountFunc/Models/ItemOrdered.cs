using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EShopFunc.Models;
public class ItemOrdered
{
    [JsonProperty("CatalogItemId")]
    public int CatalogItemId { get; set; }

    [JsonProperty("ProductName")]
    public string ProductName { get; set; }

    [JsonProperty("PictureUri")]
    public string PictureUri { get; set; }
}
