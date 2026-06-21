using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dump
{
    /// <summary>
    /// Статический класс для хранения информации о текущем авторизованном пользователе.
    /// Обеспечивает доступ к данным пользователя из любого места приложения.
    /// </summary>
    public static class CurrentUser
    {
        /// <summary>
        /// Уникальный идентификатор пользователя.
        /// </summary>
        public static int UserId { get; set; }

        /// <summary>
        /// Логин пользователя.
        /// </summary>
        public static string Username { get; set; }

        /// <summary>
        /// Полное имя пользователя (ФИО).
        /// </summary>
        public static string FIO { get; set; }

        /// <summary>
        /// Идентификатор роли пользователя.
        /// </summary>
        public static int RoleId { get; set; }

        /// <summary>
        /// Название роли пользователя.
        /// </summary>
        public static string RoleName { get; set; }

        /// <summary>
        /// Указывает, авторизован ли пользователь в системе.
        /// Возвращает true, если UserId больше 0.
        /// </summary>
        public static bool IsAuthenticated => UserId > 0;

        /// <summary>
        /// Указывает, является ли пользователь системным администратором.
        /// </summary>
        public static bool IsSystemAdmin { get; private set; }

        /// <summary>
        /// Инициализирует данные текущего пользователя.
        /// </summary>
        /// <param name="userId">Уникальный идентификатор пользователя.</param>
        /// <param name="username">Логин пользователя.</param>
        /// <param name="fio">Полное имя пользователя (ФИО).</param>
        /// <param name="roleId">Идентификатор роли пользователя.</param>
        /// <param name="roleName">Название роли пользователя.</param>
        public static void Initialize(int userId, string username, string fio, int roleId, string roleName)
        {
            UserId = userId;
            Username = username;
            FIO = fio;
            RoleId = roleId;
            RoleName = roleName;
            IsSystemAdmin = (username == "sisadmin" && roleId == 99);
        }

        /// <summary>
        /// Очищает данные текущего пользователя при выходе из системы.
        /// </summary>
        public static void Clear()
        {
            UserId = 0;
            Username = string.Empty;
            FIO = string.Empty;
            RoleId = 0;
            RoleName = string.Empty;
            IsSystemAdmin = false;
        }
    }
}