using System.Collections.Generic;

namespace RaynetConnectorTest.Models
{
    public class RaynetBusinessCaseListWrapper
    {
        public bool success { get; set; }
        public int totalCount { get; set; }
        public List<RaynetBusinessCase> data { get; set; }
    }
}
