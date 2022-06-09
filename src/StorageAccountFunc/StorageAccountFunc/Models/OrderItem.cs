using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EShopFunc.Models;
public class OrderItem
{
    [JsonProperty("Id")]
    public int Id { get; set; }

    [JsonProperty("ItemOrdered")]
    public ItemOrdered ItemOrdered { get; set; }

    [JsonProperty("UnitPrice")]
    public decimal UnitPrice { get; set; }

    [JsonProperty("Units")]
    public int Units { get; set; }

}
