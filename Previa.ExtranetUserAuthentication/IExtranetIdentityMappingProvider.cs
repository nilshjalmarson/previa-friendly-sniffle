using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using se.previa.schemas.baseobjects.v1;
using Previa.ExtranetIdentityMapping.Models;

namespace Previa.ExtranetUserAuthentication
{
    public interface IExtranetIdentityMappingProvider
    {
        MappingResponse MapLocalUser(OrganizationRegistrationNumber orgNo, string localUserName);
        MappingResponse MapLocalUser(MappingRequest mappingRequest);
    }
}
