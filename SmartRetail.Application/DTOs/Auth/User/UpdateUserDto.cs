using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartRetail.Application.DTOs.Auth.User
{
    public class UpdateUserDto : UpdateMeDto
    {
        public bool? IsActive { get; set; }
    }
}
