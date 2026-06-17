-- Резервная копия создана: 2026-06-17 03:29:53
-- Тип резервной копии: полной
-- Сервер: localhost
-- База данных: da
SET FOREIGN_KEY_CHECKS = 0;
SET AUTOCOMMIT = 0;

CREATE TABLE `categories` (
  `id_category` int(11) NOT NULL AUTO_INCREMENT,
  `category_name` varchar(255) NOT NULL,
  PRIMARY KEY (`id_category`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8;

INSERT INTO `categories` (`id_category`, `category_name`) VALUES (1, 'Салаты');
INSERT INTO `categories` (`id_category`, `category_name`) VALUES (2, 'Закуски');
INSERT INTO `categories` (`id_category`, `category_name`) VALUES (3, 'Пасты');
INSERT INTO `categories` (`id_category`, `category_name`) VALUES (4, 'Горячие блюда');
INSERT INTO `categories` (`id_category`, `category_name`) VALUES (5, 'Супы');
INSERT INTO `categories` (`id_category`, `category_name`) VALUES (6, 'Бургеры');
INSERT INTO `categories` (`id_category`, `category_name`) VALUES (7, 'Пиццы');


CREATE TABLE `certificates` (
  `id_certificate` int(11) NOT NULL AUTO_INCREMENT,
  `last_name` varchar(255) NOT NULL,
  `first_name` varchar(255) NOT NULL,
  `middle_name` varchar(255) DEFAULT NULL,
  `price` decimal(10,2) NOT NULL,
  `date` date NOT NULL,
  `id_status_certificate` int(11) DEFAULT NULL,
  `phone_number` varchar(20) NOT NULL,
  PRIMARY KEY (`id_certificate`),
  KEY `FK_id_status_certificate` (`id_status_certificate`),
  CONSTRAINT `certificates_ibfk_1` FOREIGN KEY (`id_status_certificate`) REFERENCES `status_certificates` (`id_status_certificate`)
) ENGINE=InnoDB AUTO_INCREMENT=55 DEFAULT CHARSET=utf8;

INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (1, 'Иванов', 'Иван', 'Иванович', 1000.00, '2026-02-22 00:00:00', 1, '+7 (910) 123-45-67');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (2, 'Петрова', 'Анна', 'Сергеевна', 1500.00, '2026-02-22 00:00:00', 1, '+7 (910) 234-56-78');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (3, 'Сидоров', 'Дмитрий', 'Владимирович', 2000.00, '2026-02-22 00:00:00', 1, '+7 (910) 345-67-89');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (4, 'Кузнецова', 'Мария', 'Петровна', 1200.00, '2026-02-22 00:00:00', 1, '+7 (910) 456-78-90');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (5, 'Васильев', 'Сергей', 'Алексеевич', 1800.00, '2026-02-22 00:00:00', 1, '+7 (910) 567-89-01');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (6, 'Морозов', 'Евгений', 'Константинович', 2500.00, '2026-02-22 00:00:00', 1, '+7 (910) 678-90-12');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (7, 'Николаева', 'Людмила', 'Васильевна', 1600.00, '2026-02-22 00:00:00', 1, '+7 (910) 789-01-23');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (8, 'Орлов', 'Александр', 'Григорьевич', 1900.00, '2026-02-22 00:00:00', 1, '+7 (910) 890-12-34');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (9, 'Павлова', 'Татьяна', 'Николаевна', 2200.00, '2026-02-22 00:00:00', 1, '+7 (910) 901-23-45');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (10, 'Романов', 'Михаил', 'Иванович', 0.00, '2026-02-22 00:00:00', 2, '+7 (910) 012-34-56');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (11, 'Соколов', 'Виктор', 'Евгеньевич', 1100.00, '2026-02-22 00:00:00', 1, '+7 (910) 112-23-34');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (12, 'Тихонова', 'Елена', 'Петровна', 1700.00, '2026-02-22 00:00:00', 1, '+7 (910) 223-34-45');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (13, 'Ушаков', 'Константин', 'Михайлович', 2300.00, '2026-02-22 00:00:00', 1, '+7 (910) 334-45-56');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (14, 'Федорова', 'Анна', 'Владимировна', 1300.00, '2026-02-22 00:00:00', 1, '+7 (910) 445-56-67');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (15, 'Хомяков', 'Сергей', 'Дмитриевич', 1950.00, '2026-02-22 00:00:00', 1, '+7 (910) 556-67-78');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (16, 'Волков', 'Александр', 'Михайлович', 2600.00, '2026-02-22 00:00:00', 2, '+7 (910) 667-78-89');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (17, 'Григорьева', 'Людмила', 'Петровна', 1750.00, '2026-02-22 00:00:00', 2, '+7 (910) 778-89-90');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (18, 'Денисов', 'Сергей', 'Константинович', 2700.00, '2026-02-22 00:00:00', 2, '+7 (910) 889-90-01');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (19, 'Егорова', 'Мария', 'Алексеевна', 1250.00, '2026-02-22 00:00:00', 2, '+7 (910) 990-01-12');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (20, 'Жуков', 'Владимир', 'Иванович', 2800.00, '2026-02-22 00:00:00', 2, '+7 (910) 101-12-23');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (21, 'Захарова', 'Елена', 'Владимировна', 1350.00, '2026-02-22 00:00:00', 2, '+7 (910) 211-22-33');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (22, 'Игнатьев', 'Александр', 'Сергеевич', 2900.00, '2026-02-22 00:00:00', 2, '+7 (910) 322-33-44');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (23, 'Кириллова', 'Мария', 'Петровна', 1500.00, '2026-02-22 00:00:00', 2, '+7 (910) 433-44-55');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (24, 'Лебедев', 'Дмитрий', 'Николаевич', 1600.00, '2026-02-22 00:00:00', 2, '+7 (910) 544-55-66');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (25, 'Морозова', 'Анна', 'Константиновна', 1800.00, '2026-02-22 00:00:00', 2, '+7 (910) 655-66-77');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (26, 'Новиков', 'Петр', 'Алексеевич', 2000.00, '2026-02-22 00:00:00', 2, '+7 (910) 766-77-88');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (27, 'Орлова', 'Татьяна', 'Григорьевна', 2100.00, '2026-02-22 00:00:00', 2, '+7 (910) 877-88-99');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (28, 'Павлов', 'Михаил', 'Иванович', 2200.00, '2026-02-22 00:00:00', 2, '+7 (910) 988-99-00');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (29, 'Романова', 'Елена', 'Сергеевна', 2300.00, '2026-02-22 00:00:00', 2, '+7 (910) 099-00-11');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (30, 'Семенов', 'Андрей', 'Владимирович', 2400.00, '2026-02-22 00:00:00', 2, '+7 (910) 100-11-22');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (31, 'Воробьев', 'Дмитрий', 'Михайлович', 2700.00, '2026-02-22 00:00:00', 3, '+7 (910) 211-22-44');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (32, 'Григорьев', 'Андрей', 'Петрович', 1450.00, '2026-02-22 00:00:00', 3, '+7 (910) 322-33-55');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (33, 'Денисова', 'Мария', 'Константиновна', 2800.00, '2026-02-22 00:00:00', 3, '+7 (910) 433-44-66');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (34, 'Егоров', 'Петр', 'Алексеевич', 1250.00, '2026-02-22 00:00:00', 3, '+7 (910) 544-55-77');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (35, 'Жукова', 'Елена', 'Ивановна', 2900.00, '2026-02-22 00:00:00', 3, '+7 (910) 655-66-88');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (36, 'Захаров', 'Сергей', 'Владимирович', 1350.00, '2026-02-22 00:00:00', 3, '+7 (910) 766-77-99');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (37, 'Игнатова', 'Анна', 'Сергеевна', 3000.00, '2026-02-22 00:00:00', 3, '+7 (910) 877-88-00');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (38, 'Кириллов', 'Михаил', 'Петрович', 1500.00, '2026-02-22 00:00:00', 3, '+7 (910) 988-99-11');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (39, 'Лебедева', 'Ольга', 'Николаевна', 1600.00, '2026-02-22 00:00:00', 3, '+7 (910) 099-00-22');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (40, 'Морозов', 'Александр', 'Константинович', 1800.00, '2026-02-22 00:00:00', 3, '+7 (910) 100-11-33');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (41, 'Новиков', 'Игорь', 'Алексеевич', 2000.00, '2026-02-22 00:00:00', 3, '+7 (910) 211-22-55');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (42, 'Орлов', 'Дмитрий', 'Григорьевич', 2100.00, '2026-02-22 00:00:00', 3, '+7 (910) 322-33-66');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (43, 'Павлова', 'Елена', 'Ивановна', 2200.00, '2026-02-22 00:00:00', 3, '+7 (910) 433-44-77');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (44, 'Романова', 'Екатерина', 'Сергеевна', 2300.00, '2026-02-22 00:00:00', 3, '+7 (910) 544-55-88');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (45, 'Семенов', 'Андрей', 'Владимирович', 2400.00, '2026-02-22 00:00:00', 3, '+7 (910) 655-66-99');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (46, 'Тихомирова', 'Татьяна', 'Константиновна', 1100.00, '2026-02-22 00:00:00', 3, '+7 (910) 766-77-00');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (47, 'Устинов', 'Сергей', 'Петрович', 1900.00, '2026-02-22 00:00:00', 3, '+7 (910) 877-88-11');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (48, 'Федорова', 'Наталья', 'Дмитриевна', 1400.00, '2026-02-22 00:00:00', 3, '+7 (910) 988-99-22');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (49, 'Хомяков', 'Владимир', 'Алексеевич', 2500.00, '2026-02-22 00:00:00', 3, '+7 (910) 099-00-33');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (50, 'Чернышева', 'Екатерина', 'Михайловна', 1550.00, '2026-02-22 00:00:00', 3, '+7 (910) 100-11-44');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (51, 'Самойлова', 'Диана', 'Дмитриевна', 5000.00, '2026-03-11 00:00:00', 3, '+7 (478) 324-32-87');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (52, 'Уцацуацуацу', 'Ацуацууц', 'Цуацуацу', 1000.00, '2026-06-17 00:00:00', 3, '+7 (232) 132-13-13');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (53, 'Ауцауца', 'Цуауц', 'Уцацу', 0.00, '2026-06-17 00:00:00', 2, '+7 (232) 132-12-21');
INSERT INTO `certificates` (`id_certificate`, `last_name`, `first_name`, `middle_name`, `price`, `date`, `id_status_certificate`, `phone_number`) VALUES (54, 'ВСЫЙВ', 'АЦУА', 'УЦАЦУ', 1000.00, '2026-06-17 00:00:00', 1, '+7 (312) 312-31-23');


CREATE TABLE `dishes` (
  `id_dish` int(11) NOT NULL AUTO_INCREMENT,
  `dish_name` varchar(100) NOT NULL,
  `compound` varchar(255) DEFAULT NULL,
  `id_category` int(11) NOT NULL,
  `price` decimal(10,2) NOT NULL,
  `photo` longblob,
  `weight_volume` varchar(20) NOT NULL,
  `cost` decimal(10,2) DEFAULT '0.00',
  PRIMARY KEY (`id_dish`),
  KEY `FK_id_category` (`id_category`),
  CONSTRAINT `dishes_ibfk_1` FOREIGN KEY (`id_category`) REFERENCES `categories` (`id_category`)
) ENGINE=InnoDB AUTO_INCREMENT=75 DEFAULT CHARSET=utf8;

INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (1, 'Салат Цезарь с курицей', 'Листья салата айсберг, нежное куриное филе в соусе песто, спелые томаты черри, хрустящие крутоны, сыр Пармезан, заправка соус Цезарь.', 1, 520.00, NULL, '210 г.', 234.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (2, 'Греческий', 'Огурцы, помидоры, сыр фета', 1, 280.00, NULL, '210 г.', 126.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (3, 'Оливье', 'Картофель, морковь, яйца', 1, 250.00, NULL, '210 г.', 112.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (4, 'Нисуаз', 'Тунец, овощи', 1, 320.00, NULL, '210 г.', 144.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (5, 'Салат цезарь с креветками', 'Листья салата, обжаренные тигровые креветки, спелые томаты черри, хрустящие крутоны, сыр Пармезан, заправка соус Цезарь.', 1, 580.00, NULL, '210 г.', 261.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (6, 'Витаминный', 'Овощи, зелень', 1, 220.00, NULL, '210 г.', 99.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (7, 'Мимоза', 'Рыба, картофель', 1, 270.00, NULL, '210 г.', 121.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (8, 'Кобб', 'Курица, бекон, авокадо', 1, 380.00, NULL, '210 г.', 171.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (9, 'Вальдорф', 'Яблоко, сельдерей', 1, 290.00, NULL, '210 г.', 130.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (10, 'Табуле', 'Питта, булгур', 1, 260.00, NULL, '210 г.', 117.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (11, 'Брускетта', 'Хлеб, томаты, базилик', 2, 220.00, NULL, '150 г.', 99.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (12, 'Карпаччо', 'Говядина, специи', 2, 450.00, NULL, '150 г.', 202.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (13, 'Тарталетки с икрой', 'Икра, крем', 2, 500.00, NULL, '100 г.', 225.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (14, 'Сырная тарелка', 'Разные сыры', 2, 600.00, NULL, '200 г.', 270.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (15, 'Рулетики из баклажанов', 'Баклажаны, сыр', 2, 280.00, NULL, '180 г.', 126.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (16, 'Карпаччо из тунца', 'Тунец, специи', 2, 420.00, NULL, '150 г.', 189.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (17, 'Креветки в соусе', 'Креветки, соус', 2, 350.00, NULL, '150 г.', 157.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (18, 'Сырные палочки', 'Сыр, панировка', 2, 240.00, NULL, '120 г.', 108.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (19, 'Рулетики из ветчины', 'Ветчина, сыр', 2, 260.00, NULL, '150 г.', 117.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (20, 'Сырные крокеты', 'Сыр, тесто', 2, 230.00, NULL, '120 г.', 103.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (21, 'Карбонара', 'Спагетти, бекон, сыр', 3, 450.00, NULL, '350 г.', 202.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (22, 'Болоньезе', 'Спагетти, мясной соус', 3, 420.00, NULL, '350 г.', 189.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (23, 'Паста с морепродуктами', 'Лапша, морепродукты', 3, 550.00, NULL, '350 г.', 247.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (24, 'Карри с курицей', 'Лапша, курица, специи', 3, 480.00, NULL, '350 г.', 216.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (25, 'Ризотто', 'Рис, грибы, сливки', 3, 420.00, NULL, '300 г.', 189.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (26, 'Паста примавера', 'Овощи, сливки', 3, 400.00, NULL, '350 г.', 180.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (27, 'Паста с лососем', 'Лосось, сливки', 3, 520.00, NULL, '350 г.', 234.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (28, 'Паста карбонара', 'Бекон, сыр', 3, 430.00, NULL, '350 г.', 193.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (29, 'Паста с грибами', 'Грибы, сливки', 3, 390.00, NULL, '350 г.', 175.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (30, 'Паста с курицей', 'Курица, соус', 3, 410.00, NULL, '350 г.', 184.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (31, 'Стейк Рибай', 'Говядина, специи', 4, 800.00, NULL, '300 г.', 360.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (32, 'Филе миньон', 'Говядина', 4, 900.00, NULL, '250 г.', 405.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (33, 'Пельмени', 'Тесто, мясо', 4, 310.00, NULL, '300 г.', 139.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (34, 'Ризотто с грибами', 'Рис, грибы, сливки', 4, 420.00, NULL, '300 г.', 189.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (35, 'Паэлья', 'Рис, морепродукты', 4, 500.00, NULL, '350 г.', 225.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (36, 'Жаркое по-домашнему', 'Мясо, овощи', 4, 450.00, NULL, '350 г.', 202.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (37, 'Оссобуко', 'Голяшка телятины', 4, 750.00, NULL, '300 г.', 337.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (38, 'Бефстроганов', 'Говядина, соус', 4, 480.00, NULL, '300 г.', 216.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (39, 'Цыплёнок табака', 'Курица, специи', 4, 420.00, NULL, '400 г.', 189.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (40, 'Свинина в кисло-сладком соусе', 'Свинина, овощи', 4, 400.00, NULL, '300 г.', 180.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (41, 'Форель запечённая', 'Рыба, специи', 4, 600.00, NULL, '350 г.', 270.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (42, 'Утка по-пекински', 'Утка, соус', 4, 700.00, NULL, '350 г.', 315.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (44, 'Телятина по-бургундски', 'Телятина, вино', 4, 650.00, NULL, '300 г.', 292.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (45, 'Борщ', 'Свекла, капуста, мясо', 5, 200.00, NULL, '350 г.', 90.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (46, 'Грибной суп', 'Грибы, картофель, сливки', 5, 220.00, NULL, '350 г.', 99.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (47, 'Том Ям', 'Креветки, кокосовое молоко', 5, 350.00, NULL, '350 г.', 157.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (48, 'Уха', 'Рыба, картофель, лук', 5, 240.00, NULL, '350 г.', 108.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (49, 'Щи', 'Капуста, мясо', 5, 210.00, NULL, '350 г.', 94.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (50, 'Суп-пюре из тыквы', 'Тыква, сливки', 5, 230.00, NULL, '350 г.', 103.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (51, 'Харчо', 'Говядина, специи', 5, 250.00, NULL, '350 г.', 112.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (52, 'Солянка', 'Мясо, колбаса, огурцы', 5, 260.00, NULL, '350 г.', 117.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (53, 'Крем-суп грибной', 'Грибы, сливки', 5, 245.00, NULL, '350 г.', 110.25);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (54, 'Суп лапша домашняя', 'Лапша, курица', 5, 225.00, NULL, '350 г.', 101.25);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (55, 'Чикенбургер', 'Курица, булка, овощи', 6, 250.00, NULL, '250 г.', 112.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (56, 'Биг Мак', 'Говядина, булка, соус', 6, 300.00, NULL, '300 г.', 135.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (57, 'Веджибургер', 'Овощи, булка', 6, 220.00, NULL, '250 г.', 99.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (58, 'Рыбный бургер', 'Рыба, булка', 6, 280.00, NULL, '250 г.', 126.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (59, 'Двойной чизбургер', 'Говядина, сыр', 6, 350.00, NULL, '300 г.', 157.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (60, 'Чизбургер классический', 'Говядина, сыр', 6, 270.00, NULL, '250 г.', 121.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (61, 'Бургер с беконом', 'Говядина, бекон', 6, 320.00, NULL, '300 г.', 144.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (62, 'Веганский бургер', 'Овощи, соя', 6, 240.00, NULL, '250 г.', 108.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (63, 'Бургер с креветками', 'Креветки, булка', 6, 310.00, NULL, '250 г.', 139.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (64, 'Бургер с индейкой', 'Индейка, овощи', 6, 290.00, NULL, '250 г.', 130.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (65, 'Маргарита', 'Тесто, помидоры, сыр', 7, 300.00, NULL, '400 г.', 135.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (66, 'Пепперони', 'Тесто, пепперони, сыр', 7, 350.00, NULL, '450 г.', 157.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (67, 'Четыре сыра', 'Тесто, 4 вида сыра', 7, 370.00, NULL, '400 г.', 166.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (68, 'Гавайская', 'Ананас, ветчина, сыр', 7, 320.00, NULL, '450 г.', 144.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (69, 'Мясная', 'Тесто, мясо, ветчина, бекон', 7, 380.00, NULL, '500 г.', 171.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (70, 'Морская', 'Морепродукты, сыр', 7, 400.00, NULL, '450 г.', 180.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (71, 'Вегетарианская', 'Овощи, грибы, сыр', 7, 330.00, NULL, '450 г.', 148.50);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (72, 'Диабло', 'Острые ингредиенты, пепперони', 7, 360.00, NULL, '450 г.', 162.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (73, 'Карбонара', 'Бекон, сыр, соус', 7, 340.00, NULL, '450 г.', 153.00);
INSERT INTO `dishes` (`id_dish`, `dish_name`, `compound`, `id_category`, `price`, `photo`, `weight_volume`, `cost`) VALUES (74, 'Феррара', 'Ветчина, грибы, сыр', 7, 355.00, NULL, '450 г.', 159.75);


CREATE TABLE `order_dish` (
  `id_order_dish` int(11) NOT NULL AUTO_INCREMENT,
  `id_order` int(11) NOT NULL,
  `id_dish` int(11) NOT NULL,
  `quantity` int(11) NOT NULL DEFAULT '1',
  `price_at_order` decimal(10,2) NOT NULL,
  `is_gift` tinyint(1) NOT NULL DEFAULT '0',
  `id_present` int(11) DEFAULT NULL,
  PRIMARY KEY (`id_order_dish`),
  KEY `id_order` (`id_order`),
  KEY `id_dish` (`id_dish`),
  KEY `id_present` (`id_present`),
  CONSTRAINT `order_dish_ibfk_1` FOREIGN KEY (`id_order`) REFERENCES `orders` (`id_order`) ON DELETE CASCADE,
  CONSTRAINT `order_dish_ibfk_2` FOREIGN KEY (`id_dish`) REFERENCES `dishes` (`id_dish`),
  CONSTRAINT `order_dish_ibfk_3` FOREIGN KEY (`id_present`) REFERENCES `present` (`id_present`)
) ENGINE=InnoDB AUTO_INCREMENT=338 DEFAULT CHARSET=utf8;

INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (151, 1, 1, 2, 520.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (152, 1, 21, 1, 450.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (153, 1, 45, 1, 200.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (154, 2, 5, 1, 580.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (155, 2, 33, 2, 310.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (160, 4, 2, 1, 280.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (161, 4, 8, 2, 380.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (162, 5, 46, 2, 220.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (163, 5, 56, 1, 300.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (164, 5, 70, 3, 400.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (165, 6, 3, 2, 250.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (166, 6, 22, 1, 420.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (167, 6, 57, 1, 220.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (172, 8, 4, 1, 320.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (173, 8, 32, 2, 900.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (174, 9, 9, 1, 290.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (175, 9, 34, 2, 420.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (176, 10, 10, 2, 260.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (177, 10, 24, 1, 480.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (178, 10, 49, 1, 210.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (179, 11, 11, 2, 220.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (180, 11, 25, 1, 420.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (181, 11, 50, 1, 230.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (182, 11, 66, 2, 350.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (186, 13, 14, 1, 600.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (187, 13, 36, 1, 450.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (188, 14, 6, 2, 220.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (189, 14, 15, 1, 280.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (190, 14, 26, 1, 400.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (191, 14, 69, 3, 380.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (192, 15, 16, 1, 420.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (193, 15, 37, 1, 750.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (194, 15, 51, 2, 250.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (199, 17, 18, 2, 240.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (200, 17, 39, 1, 420.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (201, 18, 19, 1, 260.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (202, 18, 40, 1, 400.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (203, 18, 53, 2, 245.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (204, 19, 20, 2, 230.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (205, 19, 41, 1, 600.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (206, 19, 54, 1, 225.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (207, 19, 72, 3, 360.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (208, 20, 1, 1, 520.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (209, 20, 42, 1, 700.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (213, 22, 3, 1, 250.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (214, 22, 44, 2, 650.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (215, 23, 4, 1, 320.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (216, 23, 5, 1, 580.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (217, 23, 45, 2, 200.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (218, 23, 56, 1, 300.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (222, 25, 7, 1, 270.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (223, 25, 47, 1, 350.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (224, 26, 8, 2, 380.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (225, 26, 48, 1, 240.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (226, 26, 58, 1, 280.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (227, 27, 9, 1, 290.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (228, 27, 49, 2, 210.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (229, 28, 10, 1, 260.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (230, 28, 21, 1, 450.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (231, 28, 31, 1, 800.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (232, 28, 59, 1, 350.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (238, 31, 13, 1, 500.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (239, 31, 24, 1, 480.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (240, 31, 32, 2, 900.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (241, 31, 61, 1, 280.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (242, 32, 14, 1, 600.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (243, 32, 25, 1, 420.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (244, 32, 62, 3, 240.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (247, 34, 16, 1, 420.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (248, 34, 26, 1, 400.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (249, 34, 34, 1, 420.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (250, 34, 63, 2, 310.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (251, 35, 17, 1, 350.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (252, 35, 27, 1, 520.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (253, 35, 64, 1, 290.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (254, 36, 18, 2, 240.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (255, 36, 35, 1, 500.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (260, 38, 20, 1, 230.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (261, 38, 29, 2, 390.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (262, 38, 66, 1, 350.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (263, 39, 1, 2, 520.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (264, 39, 37, 1, 750.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (265, 40, 2, 1, 280.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (266, 40, 30, 1, 410.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (267, 40, 38, 1, 480.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (268, 40, 67, 2, 370.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (272, 42, 4, 1, 320.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (273, 42, 40, 2, 400.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (274, 43, 5, 1, 580.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (275, 43, 41, 1, 600.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (276, 43, 50, 2, 230.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (277, 43, 69, 1, 380.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (278, 44, 6, 2, 220.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (279, 44, 42, 1, 700.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (280, 44, 51, 1, 250.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (283, 46, 8, 1, 380.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (284, 46, 44, 1, 650.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (285, 46, 52, 1, 260.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (286, 46, 70, 1, 400.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (287, 47, 9, 1, 290.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (288, 47, 45, 2, 200.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (289, 47, 53, 1, 245.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (290, 48, 10, 1, 260.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (291, 48, 46, 1, 220.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (292, 48, 54, 2, 225.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (293, 48, 71, 1, 330.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (297, 50, 12, 1, 450.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (298, 50, 48, 1, 240.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (299, 50, 56, 2, 300.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (300, 50, 72, 1, 360.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (326, 71, 1, 7, 520.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (327, 71, 1, 1, 0.00, 1, 4);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (328, 72, 1, 5, 520.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (329, 72, 1, 1, 0.00, 1, 3);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (330, 73, 1, 4, 520.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (331, 73, 1, 1, 0.00, 1, 1);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (332, 74, 1, 5, 520.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (333, 74, 1, 1, 0.00, 1, 3);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (334, 75, 1, 5, 520.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (335, 75, 1, 1, 0.00, 1, 3);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (336, 76, 1, 3, 520.00, 0, NULL);
INSERT INTO `order_dish` (`id_order_dish`, `id_order`, `id_dish`, `quantity`, `price_at_order`, `is_gift`, `id_present`) VALUES (337, 76, 1, 1, 0.00, 1, 2);


CREATE TABLE `order_statuses` (
  `id_status` int(11) NOT NULL AUTO_INCREMENT,
  `status_name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id_status`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8;

INSERT INTO `order_statuses` (`id_status`, `status_name`) VALUES (2, 'Принят');
INSERT INTO `order_statuses` (`id_status`, `status_name`) VALUES (4, 'Готов');
INSERT INTO `order_statuses` (`id_status`, `status_name`) VALUES (5, 'В пути');
INSERT INTO `order_statuses` (`id_status`, `status_name`) VALUES (6, 'Доставлен');
INSERT INTO `order_statuses` (`id_status`, `status_name`) VALUES (7, 'Отменён');


CREATE TABLE `orders` (
  `id_order` int(11) NOT NULL AUTO_INCREMENT,
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
  `created_at` timestamp NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_order`),
  KEY `id_status` (`id_status`),
  CONSTRAINT `orders_ibfk_1` FOREIGN KEY (`id_status`) REFERENCES `order_statuses` (`id_status`)
) ENGINE=InnoDB AUTO_INCREMENT=77 DEFAULT CHARSET=utf8;

INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (1, 'Иван Петров', '+7 (916) 123-45-67', 'ул. Ленина, 15, кв. 42', 2, '2026-02-18 00:00:00', '18:30:00', 'Позвонить за 15 минут', 'Карта', 7, 1690.00, '2026-02-17 07:15:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (2, 'Елена Смирнова', '+7 (916) 765-43-21', 'пр. Мира, 23, кв. 5', 2, '2026-02-18 00:00:00', '19:00:00', 'Домофон 5', 'Наличные', 2, 1200.00, '2026-02-17 07:23:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (4, 'Ольга Морозова', '+7 (916) 111-22-33', 'ул. Гагарина, 45, кв. 78', 3, '2026-02-18 00:00:00', '19:30:00', '3 подъезд', 'Перевод', 4, 1040.00, '2026-02-17 08:42:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (5, 'Дмитрий Волков', '+7 (916) 222-33-44', 'пр. Победы, 12, кв. 34', 2, '2026-02-18 00:00:00', '20:00:00', 'Оставить у двери', 'Карта', 5, 1940.00, '2026-02-17 09:15:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (6, 'Анна Новикова', '+7 (916) 333-44-55', 'ул. Лесная, 7, кв. 15', 4, '2026-02-18 00:00:00', '18:15:00', NULL, 'Наличные', 2, 1140.00, '2026-02-17 09:30:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (8, 'Татьяна Соколова', '+7 (916) 555-66-77', 'пр. Космонавтов, 56, кв. 91', 3, '2026-02-18 00:00:00', '20:30:00', 'Домофон 91', 'Перевод', 5, 2120.00, '2026-02-17 10:22:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (9, 'Михаил Федоров', '+7 (916) 666-77-88', 'ул. Пушкина, 10, кв. 3', 2, '2026-02-18 00:00:00', '18:00:00', NULL, 'Карта', 6, 1130.00, '2026-02-17 10:45:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (10, 'Наталья Мороз', '+7 (916) 777-88-99', 'ул. Кирова, 22, кв. 67', 1, '2026-02-18 00:00:00', '19:15:00', 'Код 67', 'Наличные', 2, 1210.00, '2026-02-17 11:10:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (11, 'Андрей Соловьев', '+7 (916) 888-99-00', 'пр. Ленинградский, 78, кв. 14', 4, '2026-02-18 00:00:00', '20:45:00', NULL, 'Карта', 4, 1790.00, '2026-02-17 11:28:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (13, 'Владимир Крылов', '+7 (916) 000-11-22', 'ул. Новая, 17, кв. 45', 3, '2026-02-18 00:00:00', '19:00:00', '2 подъезд', 'Карта', 5, 1050.00, '2026-02-17 12:15:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (14, 'Юлия Васильева', '+7 (916) 111-22-44', 'пр. Октябрьский, 34, кв. 89', 5, '2026-02-18 00:00:00', '19:30:00', NULL, 'Наличные', 2, 2260.00, '2026-02-17 12:33:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (15, 'Артем Кузнецов', '+7 (916) 222-33-55', 'ул. Северная, 9, кв. 12', 5, '2026-02-18 00:00:00', '20:00:00', 'Домофон 12', 'Карта', 5, 1670.00, '2026-02-17 12:48:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (17, 'Игорь Семенов', '+7 (916) 444-55-77', 'пр. Мичурина, 28, кв. 56', 4, '2026-02-18 00:00:00', '19:15:00', 'Код 56', 'Карта', 4, 900.00, '2026-02-17 13:20:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (18, 'Светлана Тихонова', '+7 (916) 555-66-88', 'ул. Полевая, 6, кв. 41', 2, '2026-02-18 00:00:00', '20:30:00', NULL, 'Наличные', 2, 1150.00, '2026-02-17 13:42:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (19, 'Олег Григорьев', '+7 (916) 666-77-99', 'ул. Зеленая, 13, кв. 7', 1, '2026-02-18 00:00:00', '18:00:00', 'Позвонить за 10 минут', 'Карта', 5, 2365.00, '2026-02-17 14:00:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (20, 'Людмила Ершова', '+7 (916) 777-88-00', 'пр. Парковый, 51, кв. 28', 4, '2026-02-18 00:00:00', '19:45:00', NULL, 'Перевод', 6, 1220.00, '2026-02-17 14:18:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (22, 'Виктория Зайцева', '+7 (916) 999-00-22', 'ул. Рабочая, 29, кв. 63', 3, '2026-02-19 00:00:00', '19:00:00', NULL, 'Наличные', 2, 1550.00, '2026-02-17 15:00:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (23, 'Денис Комаров', '+7 (916) 000-11-33', 'пр. Строителей, 16, кв. 5', 3, '2026-02-19 00:00:00', '19:30:00', 'Домофон 5', 'Карта', 4, 1600.00, '2026-02-17 15:22:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (25, 'Глеб Орлов', '+7 (916) 222-33-66', 'ул. Московская, 49, кв. 14', 2, '2026-02-19 00:00:00', '20:30:00', 'Код 14', 'Карта', 5, 620.00, '2026-02-17 16:05:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (26, 'Валерия Никонова', '+7 (916) 333-44-77', 'пр. Речной, 42, кв. 37', 4, '2026-02-19 00:00:00', '18:15:00', NULL, 'Наличные', 2, 1280.00, '2026-02-17 16:23:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (27, 'Константин Борисов', '+7 (916) 444-55-88', 'ул. Горького, 11, кв. 82', 2, '2026-02-19 00:00:00', '19:45:00', '3 подъезд', 'Карта', 5, 710.00, '2026-02-17 16:40:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (28, 'Полина Жукова', '+7 (916) 555-66-99', 'ул. Чехова, 24, кв. 51', 5, '2026-02-19 00:00:00', '20:15:00', NULL, 'Перевод', 6, 1860.00, '2026-02-17 17:00:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (31, 'Вадим Сычев', '+7 (916) 888-99-22', 'ул. Луговая, 8, кв. 68', 4, '2026-02-19 00:00:00', '19:30:00', 'Домофон 68', 'Карта', 2, 3060.00, '2026-02-17 18:00:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (32, 'Инна Калинина', '+7 (916) 999-00-33', 'пр. Шоссейный, 27, кв. 25', 2, '2026-02-19 00:00:00', '20:45:00', NULL, 'Перевод', 5, 1740.00, '2026-02-17 18:15:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (34, 'Зоя Филиппова', '+7 (916) 111-22-66', 'ул. Озерная, 21, кв. 73', 1, '2026-02-20 00:00:00', '18:30:00', NULL, 'Наличные', 2, 1860.00, '2026-02-17 18:48:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (35, 'Эдуард Щербаков', '+7 (916) 222-33-77', 'пр. Тихий, 4, кв. 16', 5, '2026-02-20 00:00:00', '19:15:00', '2 этаж', 'Карта', 4, 1160.00, '2026-02-17 19:05:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (36, 'Нелли Архипова', '+7 (916) 333-44-88', 'ул. Светлая, 31, кв. 58', 2, '2026-02-20 00:00:00', '19:45:00', NULL, 'Перевод', 5, 980.00, '2026-02-17 19:20:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (38, 'Лариса Ковалева', '+7 (916) 555-66-00', 'пр. Звездный, 47, кв. 29', 3, '2026-02-20 00:00:00', '18:45:00', NULL, 'Наличные', 2, 1360.00, '2026-02-17 20:00:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (39, 'Виталий Мельников', '+7 (916) 666-77-11', 'ул. Абрикосовая, 5, кв. 11', 1, '2026-02-20 00:00:00', '19:00:00', 'Домофон 11', 'Карта', 5, 1790.00, '2026-02-17 20:15:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (40, 'Диана Алексеева', '+7 (916) 777-88-22', 'ул. Вишневая, 26, кв. 55', 4, '2026-02-20 00:00:00', '20:00:00', NULL, 'Перевод', 6, 1910.00, '2026-02-17 20:30:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (42, 'Галина Рыбакова', '+7 (916) 999-00-44', 'ул. Ромашковая, 12, кв. 34', 3, '2026-02-20 00:00:00', '18:15:00', NULL, 'Наличные', 2, 1120.00, '2026-02-18 05:30:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (43, 'Станислав Петухов', '+7 (916) 000-11-55', 'ул. Лазурная, 41, кв. 19', 2, '2026-02-20 00:00:00', '19:30:00', '3 подъезд', 'Карта', 4, 2020.00, '2026-02-18 05:45:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (44, 'Алина Савельева', '+7 (916) 111-22-77', 'пр. Кленовый, 7, кв. 62', 4, '2026-02-20 00:00:00', '20:15:00', NULL, 'Перевод', 5, 1390.00, '2026-02-18 06:00:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (46, 'Маргарита Давыдова', '+7 (916) 333-44-99', 'ул. Речная, 9, кв. 24', 5, '2026-02-20 00:00:00', '19:45:00', NULL, 'Наличные', 2, 1690.00, '2026-02-18 06:40:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (47, 'Аркадий Титов', '+7 (916) 444-55-00', 'пр. Ботанический, 52, кв. 13', 4, '2026-02-20 00:00:00', '20:30:00', 'Домофон 13', 'Карта', 5, 935.00, '2026-02-18 07:00:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (48, 'Эльвира Гусева', '+7 (916) 555-66-11', 'ул. Первомайская, 6, кв. 39', 2, '2026-02-20 00:00:00', '18:00:00', NULL, 'Перевод', 6, 1260.00, '2026-02-18 07:18:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (50, 'Роза Тимофеева', '+7 (916) 777-88-33', 'пр. Коммунистический, 29, кв. 51', 5, '2026-02-20 00:00:00', '20:00:00', NULL, 'Наличные', 6, 1650.00, '2026-02-18 07:52:00');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (71, '', '+7 (323) 213-12-31', 'Самовывоз', 1, '2026-06-17 00:00:00', '20:35:55', '', 'Наличные', 2, 3640.00, '2026-06-17 00:36:05');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (72, '', '+7 (232) 131-23-12', 'Самовывоз', 1, '2026-06-17 00:00:00', '06:37:15', '', 'Наличные', 2, 2600.00, '2026-06-17 00:37:32');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (73, '', '+7 (432) 321-32-13', 'Самовывоз', 1, '2026-06-17 00:00:00', '02:01:00', '', 'Наличные', 2, 2080.00, '2026-06-17 00:42:11');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (74, '', '+7 (423) 423-42-34', 'Самовывоз', 1, '2026-06-17 00:00:00', '20:00:00', '', 'Наличные', 2, 2600.00, '2026-06-17 00:45:31');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (75, '', '+7 (232) 131-23-12', 'Самовывоз', 1, '2026-06-17 00:00:00', '08:00:00', '', 'Сертификат №53 + Наличные', 2, 1600.00, '2026-06-17 00:46:47');
INSERT INTO `orders` (`id_order`, `name_client`, `phone_number`, `address`, `number_persons`, `delivery_date`, `delivery_time`, `comment`, `payment_method`, `id_status`, `total_amount`, `created_at`) VALUES (76, '', '+7 (232) 132-13-12', 'Самовывоз', 1, '2026-06-18 00:00:00', '08:00:00', '', 'Карта', 2, 1560.00, '2026-06-17 02:48:47');


CREATE TABLE `present` (
  `id_present` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  `from_price` decimal(10,2) DEFAULT NULL,
  PRIMARY KEY (`id_present`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8;

INSERT INTO `present` (`id_present`, `name`, `from_price`) VALUES (1, 'Креветки в соусе', 2000.00);
INSERT INTO `present` (`id_present`, `name`, `from_price`) VALUES (2, 'Карбонара', 1500.00);
INSERT INTO `present` (`id_present`, `name`, `from_price`) VALUES (3, 'Борщ', 2500.00);
INSERT INTO `present` (`id_present`, `name`, `from_price`) VALUES (4, 'Чизбургер классический', 3000.00);


CREATE TABLE `roles` (
  `id_role` int(11) NOT NULL AUTO_INCREMENT,
  `role_name` varchar(50) NOT NULL,
  PRIMARY KEY (`id_role`),
  UNIQUE KEY `role_name` (`role_name`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;

INSERT INTO `roles` (`id_role`, `role_name`) VALUES (3, 'admin');
INSERT INTO `roles` (`id_role`, `role_name`) VALUES (2, 'director');
INSERT INTO `roles` (`id_role`, `role_name`) VALUES (1, 'manager');


CREATE TABLE `status_certificates` (
  `id_status_certificate` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id_status_certificate`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;

INSERT INTO `status_certificates` (`id_status_certificate`, `name`) VALUES (1, 'Активен');
INSERT INTO `status_certificates` (`id_status_certificate`, `name`) VALUES (2, 'Использован');
INSERT INTO `status_certificates` (`id_status_certificate`, `name`) VALUES (3, 'Возвращён');


CREATE TABLE `users` (
  `id_user` int(11) NOT NULL AUTO_INCREMENT,
  `FIO` varchar(100) NOT NULL,
  `id_role` int(11) NOT NULL,
  `login` varchar(50) NOT NULL,
  `password_hash` varchar(64) NOT NULL,
  PRIMARY KEY (`id_user`),
  UNIQUE KEY `login` (`login`),
  KEY `id_role` (`id_role`),
  CONSTRAINT `users_ibfk_1` FOREIGN KEY (`id_role`) REFERENCES `roles` (`id_role`)
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8;

INSERT INTO `users` (`id_user`, `FIO`, `id_role`, `login`, `password_hash`) VALUES (7, 'Иванов Иван Иванович', 2, 'director', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918');
INSERT INTO `users` (`id_user`, `FIO`, `id_role`, `login`, `password_hash`) VALUES (8, 'Сидоров Олег Александрович', 1, 'manager', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918');
INSERT INTO `users` (`id_user`, `FIO`, `id_role`, `login`, `password_hash`) VALUES (10, 'Самойлова Диана Дмитриевна', 3, 'admin', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918');
INSERT INTO `users` (`id_user`, `FIO`, `id_role`, `login`, `password_hash`) VALUES (11, 'админ админов', 3, 'admin2', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918');
INSERT INTO `users` (`id_user`, `FIO`, `id_role`, `login`, `password_hash`) VALUES (12, 'Петров Петр Петрович', 2, 'petrov.p', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918');
INSERT INTO `users` (`id_user`, `FIO`, `id_role`, `login`, `password_hash`) VALUES (13, 'Козлова Екатерина Дмитриевна', 2, 'kozlova.e', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918');
INSERT INTO `users` (`id_user`, `FIO`, `id_role`, `login`, `password_hash`) VALUES (14, 'Соколов Артем Владимирович', 3, 'sokolov.a', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918');
INSERT INTO `users` (`id_user`, `FIO`, `id_role`, `login`, `password_hash`) VALUES (15, 'Морозова Анна Сергеевна', 3, 'morozova.a', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918');
INSERT INTO `users` (`id_user`, `FIO`, `id_role`, `login`, `password_hash`) VALUES (16, 'Волков Дмитрий Алексеевич', 3, 'volkov.d', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918');
INSERT INTO `users` (`id_user`, `FIO`, `id_role`, `login`, `password_hash`) VALUES (17, 'Новикова Ольга Игоревна', 1, 'novikova.o', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918');
INSERT INTO `users` (`id_user`, `FIO`, `id_role`, `login`, `password_hash`) VALUES (18, 'Степанов Илья Михайлович', 1, 'stepanov.i', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918');
INSERT INTO `users` (`id_user`, `FIO`, `id_role`, `login`, `password_hash`) VALUES (19, 'Павлова Елена Александровна', 1, 'pavlova.e', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918');
INSERT INTO `users` (`id_user`, `FIO`, `id_role`, `login`, `password_hash`) VALUES (20, 'Андреев Максим Романович', 1, 'andreev.m', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918');
INSERT INTO `users` (`id_user`, `FIO`, `id_role`, `login`, `password_hash`) VALUES (21, 'Васильева Татьяна Владимировна', 1, 'vasilyeva.t', '8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918');


COMMIT;
SET FOREIGN_KEY_CHECKS = 1;
-- Конец резервной копии
