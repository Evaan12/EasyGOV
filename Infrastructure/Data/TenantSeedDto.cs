using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Data
{
    public class TenantSeedDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int TenantType { get; set; }
        public string ParentId { get; set; }
        public string ProvinceId { get; set; }
        public string DistrictId { get; set; }
        public string MunicipalityId { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsDefault { get; set; }
    }
}
