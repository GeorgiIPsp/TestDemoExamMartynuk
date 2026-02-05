using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamenDem_First.Models
{
    public class UserRole
    {
        public int Id { get; set; }

        // Статическое поле для хранения текущего пользователя
        public static UserRole CurrentUser { get; set; }

        // Метод для установки текущего пользователя
        public static void SetCurrentUser(int id)
        {
            CurrentUser = new UserRole { Id = id };
        }
    }

}
