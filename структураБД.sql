-- phpMyAdmin SQL Dump
-- Скрипт для восстановления структуры БД с пустыми таблицами
-- Все INSERT удалены, кроме таблицы roles (роли должны быть всегда)

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- База данных: `da`
--
DROP DATABASE IF EXISTS `da`;
CREATE DATABASE `da` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `da`;

-- --------------------------------------------------------

--
-- Структура таблицы `categories`
--
CREATE TABLE `categories` (
  `id_category` int(11) NOT NULL,
  `category_name` varchar(255) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `certificates`
--
CREATE TABLE `certificates` (
  `id_certificate` int(11) NOT NULL,
  `last_name` varchar(255) NOT NULL,
  `first_name` varchar(255) NOT NULL,
  `middle_name` varchar(255) DEFAULT NULL,
  `price` decimal(10,2) NOT NULL,
  `date` date NOT NULL,
  `id_status_certificate` int(11) DEFAULT NULL,
  `phone_number` varchar(20) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `dishes`
--
CREATE TABLE `dishes` (
  `id_dish` int(11) NOT NULL,
  `dish_name` varchar(100) NOT NULL,
  `compound` varchar(255) DEFAULT NULL,
  `id_category` int(11) NOT NULL,
  `price` decimal(10,2) NOT NULL,
  `photo` longblob,
  `weight_volume` varchar(20) NOT NULL,
  `cost` decimal(10,2) DEFAULT '0.00'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `orders`
--
CREATE TABLE `orders` (
  `id_order` int(11) NOT NULL,
  `name_client` varchar(255) NOT NULL,
  `phone_number` varchar(20) NOT NULL,
  `address` varchar(255) NOT NULL,
  `number_persons` int(11) DEFAULT NULL,
  `delivery_date` date NOT NULL,
  `delivery_time` time NOT NULL,
  `comment` varchar(255) DEFAULT NULL,
  `payment_method` varchar(50) NOT NULL DEFAULT 'Наличные',
  `id_status` int(11) NOT NULL,
  `total_amount` decimal(10,2) NOT NULL DEFAULT '0.00',
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `order_dish`
--
CREATE TABLE `order_dish` (
  `id_order_dish` int(11) NOT NULL,
  `id_order` int(11) NOT NULL,
  `id_dish` int(11) NOT NULL,
  `quantity` int(11) NOT NULL DEFAULT '1',
  `price_at_order` decimal(10,2) NOT NULL,
  `is_gift` tinyint(1) NOT NULL DEFAULT '0',
  `id_present` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `order_statuses`
--
CREATE TABLE `order_statuses` (
  `id_status` int(11) NOT NULL,
  `status_name` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `present`
--
CREATE TABLE `present` (
  `id_present` int(11) NOT NULL,
  `name` varchar(255) DEFAULT NULL,
  `from_price` decimal(10,2) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `roles` (СОДЕРЖИТ ДАННЫЕ - ОБЯЗАТЕЛЬНО)
--
CREATE TABLE `roles` (
  `id_role` int(11) NOT NULL,
  `role_name` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Дамп данных таблицы `roles` (ТОЛЬКО ЭТА ТАБЛИЦА СОДЕРЖИТ ДАННЫЕ)
--
INSERT INTO `roles` (`id_role`, `role_name`) VALUES
(1, 'manager'),
(2, 'director'),
(3, 'admin');

-- --------------------------------------------------------

--
-- Структура таблицы `status_certificates`
--
CREATE TABLE `status_certificates` (
  `id_status_certificate` int(11) NOT NULL,
  `name` varchar(255) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Структура таблицы `users`
--
CREATE TABLE `users` (
  `id_user` int(11) NOT NULL,
  `FIO` varchar(100) NOT NULL,
  `id_role` int(11) NOT NULL,
  `login` varchar(50) NOT NULL,
  `password_hash` varchar(64) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Индексы таблиц
--

ALTER TABLE `categories`
  ADD PRIMARY KEY (`id_category`);

ALTER TABLE `certificates`
  ADD PRIMARY KEY (`id_certificate`),
  ADD KEY `FK_id_status_certificate` (`id_status_certificate`);

ALTER TABLE `dishes`
  ADD PRIMARY KEY (`id_dish`),
  ADD KEY `FK_id_category` (`id_category`);

ALTER TABLE `orders`
  ADD PRIMARY KEY (`id_order`),
  ADD KEY `id_status` (`id_status`);

ALTER TABLE `order_dish`
  ADD PRIMARY KEY (`id_order_dish`),
  ADD KEY `id_order` (`id_order`),
  ADD KEY `id_dish` (`id_dish`),
  ADD KEY `id_present` (`id_present`);

ALTER TABLE `order_statuses`
  ADD PRIMARY KEY (`id_status`);

ALTER TABLE `present`
  ADD PRIMARY KEY (`id_present`);

ALTER TABLE `roles`
  ADD PRIMARY KEY (`id_role`),
  ADD UNIQUE KEY `role_name` (`role_name`);

ALTER TABLE `status_certificates`
  ADD PRIMARY KEY (`id_status_certificate`);

ALTER TABLE `users`
  ADD PRIMARY KEY (`id_user`),
  ADD UNIQUE KEY `login` (`login`),
  ADD KEY `id_role` (`id_role`);

--
-- AUTO_INCREMENT для таблиц
--

ALTER TABLE `categories`
  MODIFY `id_category` int(11) NOT NULL AUTO_INCREMENT;

ALTER TABLE `certificates`
  MODIFY `id_certificate` int(11) NOT NULL AUTO_INCREMENT;

ALTER TABLE `dishes`
  MODIFY `id_dish` int(11) NOT NULL AUTO_INCREMENT;

ALTER TABLE `orders`
  MODIFY `id_order` int(11) NOT NULL AUTO_INCREMENT;

ALTER TABLE `order_dish`
  MODIFY `id_order_dish` int(11) NOT NULL AUTO_INCREMENT;

ALTER TABLE `order_statuses`
  MODIFY `id_status` int(11) NOT NULL AUTO_INCREMENT;

ALTER TABLE `present`
  MODIFY `id_present` int(11) NOT NULL AUTO_INCREMENT;

ALTER TABLE `roles`
  MODIFY `id_role` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

ALTER TABLE `status_certificates`
  MODIFY `id_status_certificate` int(11) NOT NULL AUTO_INCREMENT;

ALTER TABLE `users`
  MODIFY `id_user` int(11) NOT NULL AUTO_INCREMENT;

--
-- Ограничения внешнего ключа
--

ALTER TABLE `certificates`
  ADD CONSTRAINT `certificates_ibfk_1` FOREIGN KEY (`id_status_certificate`) REFERENCES `status_certificates` (`id_status_certificate`);

ALTER TABLE `dishes`
  ADD CONSTRAINT `dishes_ibfk_1` FOREIGN KEY (`id_category`) REFERENCES `categories` (`id_category`);

ALTER TABLE `orders`
  ADD CONSTRAINT `orders_ibfk_1` FOREIGN KEY (`id_status`) REFERENCES `order_statuses` (`id_status`);

ALTER TABLE `order_dish`
  ADD CONSTRAINT `order_dish_ibfk_1` FOREIGN KEY (`id_order`) REFERENCES `orders` (`id_order`) ON DELETE CASCADE,
  ADD CONSTRAINT `order_dish_ibfk_2` FOREIGN KEY (`id_dish`) REFERENCES `dishes` (`id_dish`),
  ADD CONSTRAINT `order_dish_ibfk_3` FOREIGN KEY (`id_present`) REFERENCES `present` (`id_present`);

ALTER TABLE `users`
  ADD CONSTRAINT `users_ibfk_1` FOREIGN KEY (`id_role`) REFERENCES `roles` (`id_role`);

COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;