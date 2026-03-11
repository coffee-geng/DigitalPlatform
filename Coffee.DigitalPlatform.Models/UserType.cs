using Coffee.DigitalPlatform.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class UserType : IEquatable<UserType>
    {
        public UserType(UserTypes typeId, string typeName)
        {
            TypeId = typeId;
            TypeName = typeName;
        }

        public UserTypes TypeId { get; set; }
        public string TypeName { get; set; }

        public bool Equals(UserType? other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other)) 
                return true;
            return this.TypeId == other.TypeId;
        }
    }
}
