using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dump
{
    public static class CurrentUser
    {
        public static int UserId { get; set; }
        public static string Username { get; set; }
        public static string FIO { get; set; }
        public static int RoleId { get; set; }
        public static string RoleName { get; set; }
        public static bool IsAuthenticated => UserId > 0;
        public static bool IsSystemAdmin { get; private set; }  // ДОБАВЛЕНО: флаг системного администратора

        public static void Initialize(int userId, string username, string fio, int roleId, string roleName)
        {
            UserId = userId;
            Username = username;
            FIO = fio;
            RoleId = roleId;
            RoleName = roleName;
            IsSystemAdmin = (username == "sisadmin" && roleId == 99);  // ДОБАВЛЕНО: проверка на системного админа
        }

        public static void Clear()
        {
            UserId = 0;
            Username = string.Empty;
            FIO = string.Empty;
            RoleId = 0;
            RoleName = string.Empty;
            IsSystemAdmin = false;  // ДОБАВЛЕНО: сброс флага
        }
    }
}