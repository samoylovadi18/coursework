-- MySQL dump 10.13  Distrib 8.0.43, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: damp
-- ------------------------------------------------------
-- Server version	5.6.37-log

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `categories`
--

DROP TABLE IF EXISTS `categories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `categories` (
  `id_category` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  PRIMARY KEY (`id_category`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `categories`
--

LOCK TABLES `categories` WRITE;
/*!40000 ALTER TABLE `categories` DISABLE KEYS */;
INSERT INTO `categories` VALUES (1,'Салаты'),(2,'Закуски'),(3,'Пасты'),(4,'Горячие блюда'),(5,'Супы'),(6,'Бургеры'),(7,'Пиццы');
/*!40000 ALTER TABLE `categories` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `certificates`
--

DROP TABLE IF EXISTS `certificates`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `certificates` (
  `id_certificate` int(11) NOT NULL AUTO_INCREMENT,
  `last_name` varchar(255) DEFAULT NULL,
  `first_name` varchar(255) DEFAULT NULL,
  `middle_name` varchar(255) DEFAULT NULL,
  `price` decimal(10,2) DEFAULT NULL,
  `date` date DEFAULT NULL,
  `FK_id_status_certificate` int(11) DEFAULT NULL,
  PRIMARY KEY (`id_certificate`),
  KEY `FK_id_status_certificate` (`FK_id_status_certificate`),
  CONSTRAINT `certificates_ibfk_1` FOREIGN KEY (`FK_id_status_certificate`) REFERENCES `status_certificates` (`id_status_certificate`)
) ENGINE=InnoDB AUTO_INCREMENT=51 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `certificates`
--

LOCK TABLES `certificates` WRITE;
/*!40000 ALTER TABLE `certificates` DISABLE KEYS */;
INSERT INTO `certificates` VALUES (1,'Иванов','Иван','Иванович',1000.00,'2025-09-23',1),(2,'Петрова','Анна','Сергеевна',1500.00,'2025-09-22',1),(3,'Сидоров','Дмитрий','Владимирович',2000.00,'2025-09-21',1),(4,'Кузнецова','Мария','Петровна',1200.00,'2025-09-20',1),(5,'Васильев','Сергей','Алексеевич',1800.00,'2025-09-19',1),(6,'Морозов','Евгений','Константинович',2500.00,'2025-09-18',1),(7,'Николаева','Людмила','Васильевна',1600.00,'2025-09-17',1),(8,'Орлов','Александр','Григорьевич',1900.00,'2025-09-16',1),(9,'Павлова','Татьяна','Николаевна',2200.00,'2025-09-15',1),(10,'Романов','Михаил','Иванович',1400.00,'2025-09-14',1),(11,'Соколов','Виктор','Евгеньевич',1100.00,'2025-09-13',1),(12,'Тихонова','Елена','Петровна',1700.00,'2025-09-12',1),(13,'Ушаков','Константин','Михайлович',2300.00,'2025-09-11',1),(14,'Федорова','Анна','Владимировна',1300.00,'2025-09-10',1),(15,'Хомяков','Сергей','Дмитриевич',1950.00,'2025-09-09',1),(16,'Волков','Александр','Михайлович',2600.00,'2025-09-01',2),(17,'Григорьева','Людмила','Петровна',1750.00,'2025-08-31',2),(18,'Денисов','Сергей','Константинович',2700.00,'2025-08-30',2),(19,'Егорова','Мария','Алексеевна',1250.00,'2025-08-29',2),(20,'Жуков','Владимир','Иванович',2800.00,'2025-08-28',2),(21,'Захарова','Елена','Владимировна',1350.00,'2025-08-27',2),(22,'Игнатьев','Александр','Сергеевич',2900.00,'2025-08-26',2),(23,'Кириллова','Мария','Петровна',1500.00,'2025-08-25',2),(24,'Лебедев','Дмитрий','Николаевич',1600.00,'2025-08-24',2),(25,'Морозова','Анна','Константиновна',1800.00,'2025-08-23',2),(26,'Новиков','Петр','Алексеевич',2000.00,'2025-08-22',2),(27,'Орлова','Татьяна','Григорьевна',2100.00,'2025-08-21',2),(28,'Павлов','Михаил','Иванович',2200.00,'2025-08-20',2),(29,'Романова','Елена','Сергеевна',2300.00,'2025-08-19',2),(30,'Семенов','Андрей','Владимирович',2400.00,'2025-08-18',2),(31,'Воробьев','Дмитрий','Михайлович',2700.00,'2025-08-06',3),(32,'Григорьев','Андрей','Петрович',1450.00,'2025-08-05',3),(33,'Денисова','Мария','Константиновна',2800.00,'2025-08-04',3),(34,'Егоров','Петр','Алексеевич',1250.00,'2025-08-03',3),(35,'Жукова','Елена','Ивановна',2900.00,'2025-08-02',3),(36,'Захаров','Сергей','Владимирович',1350.00,'2025-08-01',3),(37,'Игнатова','Анна','Сергеевна',3000.00,'2025-07-31',3),(38,'Кириллов','Михаил','Петрович',1500.00,'2025-07-30',3),(39,'Лебедева','Ольга','Николаевна',1600.00,'2025-07-29',3),(40,'Морозов','Александр','Константинович',1800.00,'2025-07-28',3),(41,'Новиков','Игорь','Алексеевич',2000.00,'2025-07-27',3),(42,'Орлов','Дмитрий','Григорьевич',2100.00,'2025-07-26',3),(43,'Павлова','Елена','Ивановна',2200.00,'2025-07-25',3),(44,'Романова','Екатерина','Сергеевна',2300.00,'2025-07-24',3),(45,'Семенов','Андрей','Владимирович',2400.00,'2025-07-23',3),(46,'Тихомирова','Татьяна','Константиновна',1100.00,'2025-07-22',3),(47,'Устинов','Сергей','Петрович',1900.00,'2025-07-21',3),(48,'Федорова','Наталья','Дмитриевна',1400.00,'2025-07-20',3),(49,'Хомяков','Владимир','Алексеевич',2500.00,'2025-07-19',3),(50,'Чернышева','Екатерина','Михайловна',1550.00,'2025-07-18',3);
/*!40000 ALTER TABLE `certificates` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `dishes`
--

DROP TABLE IF EXISTS `dishes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `dishes` (
  `id_dish` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `compound` text,
  `FK_id_category` int(11) DEFAULT NULL,
  `price` decimal(10,2) DEFAULT NULL,
  PRIMARY KEY (`id_dish`),
  KEY `FK_id_category` (`FK_id_category`),
  CONSTRAINT `dishes_ibfk_1` FOREIGN KEY (`FK_id_category`) REFERENCES `categories` (`id_category`)
) ENGINE=InnoDB AUTO_INCREMENT=75 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `dishes`
--

LOCK TABLES `dishes` WRITE;
/*!40000 ALTER TABLE `dishes` DISABLE KEYS */;
INSERT INTO `dishes` VALUES (1,'Цезарь','Курица, салат, сыр, соус',1,350.00),(2,'Греческий','Огурцы, помидоры, сыр фета',1,280.00),(3,'Оливье','Картофель, морковь, яйца',1,250.00),(4,'Нисуаз','Тунец, овощи',1,320.00),(5,'Цезарь с креветками','Креветки, салат, сыр',1,400.00),(6,'Витаминный','Овощи, зелень',1,220.00),(7,'Мимоза','Рыба, картофель',1,270.00),(8,'Кобб','Курица, бекон, авокадо',1,380.00),(9,'Вальдорф','Яблоко, сельдерей',1,290.00),(10,'Табуле','Питта, булгур',1,260.00),(11,'Брускетта','Хлеб, томаты, базилик',2,220.00),(12,'Карпаччо','Говядина, специи',2,450.00),(13,'Тарталетки с икрой','Икра, крем',2,500.00),(14,'Сырная тарелка','Разные сыры',2,600.00),(15,'Рулетики из баклажанов','Баклажаны, сыр',2,280.00),(16,'Карпаччо из тунца','Тунец, специи',2,420.00),(17,'Креветки в соусе','Креветки, соус',2,350.00),(18,'Сырные палочки','Сыр, панировка',2,240.00),(19,'Рулетики из ветчины','Ветчина, сыр',2,260.00),(20,'Сырные крокеты','Сыр, тесто',2,230.00),(21,'Карбонара','Спагетти, бекон, сыр',3,450.00),(22,'Болоньезе','Спагетти, мясной соус',3,420.00),(23,'Паста с морепродуктами','Лапша, морепродукты',3,550.00),(24,'Карри с курицей','Лапша, курица, специи',3,480.00),(25,'Ризотто','Рис, грибы, сливки',3,420.00),(26,'Паста примавера','Овощи, сливки',3,400.00),(27,'Паста с лососем','Лосось, сливки',3,520.00),(28,'Паста карбонара','Бекон, сыр',3,430.00),(29,'Паста с грибами','Грибы, сливки',3,390.00),(30,'Паста с курицей','Курица, соус',3,410.00),(31,'Стейк Рибай','Говядина, специи',4,800.00),(32,'Филе миньон','Говядина',4,900.00),(33,'Пельмени','Тесто, мясо',4,310.00),(34,'Ризотто с грибами','Рис, грибы, сливки',4,420.00),(35,'Паэлья','Рис, морепродукты',4,500.00),(36,'Жаркое по-домашнему','Мясо, овощи',4,450.00),(37,'Оссобуко','Голяшка телятины',4,750.00),(38,'Бефстроганов','Говядина, соус',4,480.00),(39,'Цыплёнок табака','Курица, специи',4,420.00),(40,'Свинина в кисло-сладком соусе','Свинина, овощи',4,400.00),(41,'Форель запечённая','Рыба, специи',4,600.00),(42,'Утка по-пекински','Утка, соус',4,700.00),(43,'Баранина с овощами','Баранина, овощи',4,550.00),(44,'Телятина по-бургундски','Телятина, вино',4,650.00),(45,'Борщ','Свекла, капуста, мясо',5,200.00),(46,'Грибной суп','Грибы, картофель, сливки',5,220.00),(47,'Том Ям','Креветки, кокосовое молоко',5,350.00),(48,'Уха','Рыба, картофель, лук',5,240.00),(49,'Щи','Капуста, мясо',5,210.00),(50,'Суп-пюре из тыквы','Тыква, сливки',5,230.00),(51,'Харчо','Говядина, специи',5,250.00),(52,'Солянка','Мясо, колбаса, огурцы',5,260.00),(53,'Крем-суп грибной','Грибы, сливки',5,245.00),(54,'Суп лапша домашняя','Лапша, курица',5,225.00),(55,'Чикенбургер','Курица, булка, овощи',6,250.00),(56,'Биг Мак','Говядина, булка, соус',6,300.00),(57,'Веджибургер','Овощи, булка',6,220.00),(58,'Рыбный бургер','Рыба, булка',6,280.00),(59,'Двойной чизбургер','Говядина, сыр',6,350.00),(60,'Чизбургер классический','Говядина, сыр',6,270.00),(61,'Бургер с беконом','Говядина, бекон',6,320.00),(62,'Веганский бургер','Овощи, соя',6,240.00),(63,'Бургер с креветками','Креветки, булка',6,310.00),(64,'Бургер с индейкой','Индейка, овощи',6,290.00),(65,'Маргарита','Тесто, помидоры, сыр',7,300.00),(66,'Пепперони','Тесто, пепперони, сыр',7,350.00),(67,'Четыре сыра','Тесто, 4 вида сыра',7,370.00),(68,'Гавайская','Ананас, ветчина, сыр',7,320.00),(69,'Мясная','Тесто, мясо, ветчина, бекон',7,380.00),(70,'Морская','Морепродукты, сыр',7,400.00),(71,'Вегетарианская','Овощи, грибы, сыр',7,330.00),(72,'Диабло','Острые ингредиенты, пепперони',7,360.00),(73,'Карбонара','Бекон, сыр, соус',7,340.00),(74,'Феррара','Ветчина, грибы, сыр',7,355.00);
/*!40000 ALTER TABLE `dishes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `order_dish`
--

DROP TABLE IF EXISTS `order_dish`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `order_dish` (
  `id_order` int(11) NOT NULL AUTO_INCREMENT,
  `id_dish` int(11) DEFAULT NULL,
  `amount` int(11) DEFAULT NULL,
  `price` decimal(10,2) DEFAULT NULL,
  `id_status` int(11) DEFAULT NULL,
  PRIMARY KEY (`id_order`),
  KEY `id_dish` (`id_dish`),
  KEY `id_status` (`id_status`),
  CONSTRAINT `order_dish_ibfk_1` FOREIGN KEY (`id_dish`) REFERENCES `dishes` (`id_dish`),
  CONSTRAINT `order_dish_ibfk_2` FOREIGN KEY (`id_status`) REFERENCES `status` (`id_status`)
) ENGINE=InnoDB AUTO_INCREMENT=66 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `order_dish`
--

LOCK TABLES `order_dish` WRITE;
/*!40000 ALTER TABLE `order_dish` DISABLE KEYS */;
INSERT INTO `order_dish` VALUES (1,1,1,350.00,1),(2,2,2,450.00,1),(3,3,1,250.00,1),(4,4,1,150.00,1),(5,5,1,200.00,1),(6,6,2,400.00,1),(7,7,1,800.00,1),(8,8,2,120.00,1),(9,9,1,100.00,1),(10,10,1,300.00,1),(11,11,1,280.00,2),(12,12,1,420.00,2),(13,13,2,320.00,2),(14,14,1,500.00,2),(15,15,1,220.00,2),(16,16,1,390.00,2),(17,17,1,450.00,2),(18,18,1,240.00,2),(19,19,1,310.00,2),(20,20,1,260.00,2),(21,21,1,330.00,3),(22,22,1,430.00,3),(23,23,1,380.00,3),(24,24,1,290.00,3),(25,25,1,550.00,3),(26,26,1,480.00,3),(27,27,1,400.00,3),(28,28,1,270.00,3),(29,29,1,360.00,3),(30,30,1,230.00,3),(31,31,1,900.00,4),(32,32,1,600.00,4),(33,33,1,700.00,4),(34,34,1,550.00,4),(35,35,1,650.00,4),(36,36,1,245.00,4),(37,37,1,225.00,4),(38,38,1,280.00,4),(39,39,1,300.00,4),(40,40,1,350.00,4),(41,41,1,320.00,4),(42,42,1,290.00,4),(43,43,1,330.00,4),(44,44,1,260.00,4),(45,45,1,310.00,4),(46,46,1,350.00,5),(47,47,1,280.00,5),(48,48,1,300.00,5),(49,49,1,245.00,5),(50,50,1,225.00,5),(51,51,1,280.00,5),(52,52,1,300.00,5),(53,53,1,350.00,5),(54,54,1,245.00,5),(55,55,1,225.00,5),(56,56,1,350.00,6),(57,57,1,280.00,6),(58,58,1,300.00,6),(59,59,1,245.00,6),(60,60,1,225.00,6),(61,61,1,280.00,6),(62,62,1,300.00,6),(63,63,1,350.00,6),(64,64,1,245.00,6),(65,65,1,225.00,6);
/*!40000 ALTER TABLE `order_dish` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `orders`
--

DROP TABLE IF EXISTS `orders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `orders` (
  `id_order` int(11) NOT NULL AUTO_INCREMENT,
  `phone_number` varchar(20) DEFAULT NULL,
  `address` text,
  `number_persons` int(11) DEFAULT NULL,
  `date` date DEFAULT NULL,
  `comment` text,
  `payment` varchar(255) DEFAULT NULL,
  `id_status` int(11) DEFAULT NULL,
  `name_client` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id_order`),
  KEY `fk_status` (`id_status`),
  CONSTRAINT `fk_status` FOREIGN KEY (`id_status`) REFERENCES `status` (`id_status`)
) ENGINE=InnoDB AUTO_INCREMENT=50 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `orders`
--

LOCK TABLES `orders` WRITE;
/*!40000 ALTER TABLE `orders` DISABLE KEYS */;
INSERT INTO `orders` VALUES (1,'+79101234567','ул. Центральная, 15',2,'2025-09-24','Срочно!','Наличные',1,'Иван'),(2,'+79107654321','пр. Ленина, 25',4,'2025-09-24',NULL,'Карта',1,'Петр'),(3,'+79101112233','ул. Садовая, 10',1,'2025-09-24','Без спешки','Перевод',1,'Анна'),(4,'+79104445566','пер. Цветочный, 7',6,'2025-09-24','Нужна детская кроватка','Карта',1,'Дмитрий'),(5,'+79107778899','бульв. Победы, 30',3,'2025-09-24',NULL,'Наличные',1,'Мария'),(6,'+79102223344','ул. Мира, 12',2,'2025-09-24','Особые пожелания','Перевод',1,'Сергей'),(7,'+79103334455','пр. Комсомольский, 5',4,'2025-09-24',NULL,'Карта',1,'Ольга'),(8,'+79106667788','ул. Парковая, 20',1,'2025-09-24','Срочно!','Наличные',1,'Андрей'),(9,'+79109990011','пер. Садовый, 15',3,'2025-09-24',NULL,'Перевод',1,'Елена'),(10,'+79105556677','ул. Советская, 10',2,'2025-09-24','Без спешки','Карта',1,'Дмитрий'),(11,'+79102226666','ул. Ленина, 12',2,'2025-09-24','Срочно!','Перевод',2,'Анна'),(12,'+79103337777','пр. Комсомольский, 5',4,'2025-09-24',NULL,'Карта',2,'Ольга'),(13,'+79106668888','ул. Парковая, 20',1,'2025-09-24','Без спешки','Наличные',2,'Андрей'),(14,'+79109990000','пер. Садовый, 15',3,'2025-09-24','Особые пожелания','Перевод',2,'Елена'),(15,'+79105559999','ул. Советская, 10',2,'2025-09-24',NULL,'Карта',2,'Дмитрий'),(16,'+79108881111','пр. Октябрьский, 25',4,'2025-09-24','Нужна детская кроватка','Карта',3,'Анна'),(17,'+79101238888','ул. Зеленая, 15',2,'2025-09-24',NULL,'Наличные',3,'Мария'),(18,'+79104562222','пер. Тихий, 7',1,'2025-09-24','Срочно!','Перевод',3,'Алексей'),(19,'+79107895555','ул. Молодежная, 10',6,'2025-09-24',NULL,'Карта',3,'Ольга'),(20,'+79101116666','пр. Мира, 30',3,'2025-09-24','Особые пожелания','Наличные',3,'Сергей'),(21,'+79102227777','ул. Ленина, 12',2,'2025-09-24',NULL,'Перевод',3,'Анна'),(22,'+79103338888','пр. Комсомольский, 5',4,'2025-09-24','Без спешки','Карта',3,'Ольга'),(23,'+79106669999','ул. Парковая, 20',1,'2025-09-24',NULL,'Наличные',3,'Андрей'),(24,'+79109991111','пер. Садовый, 15',3,'2025-09-24','Нужна детская кроватка','Перевод',3,'Елена'),(25,'+79105550000','ул. Советская, 10',2,'2025-09-24','Срочно!','Карта',3,'Дмитрий'),(26,'+79108882222','пр. Октябрьский, 25',4,'2025-09-24',NULL,'Карта',4,'Екатерина'),(27,'+79101239999','ул. Северная, 15',2,'2025-09-24','Праздничный стол','Наличные',4,'Виктор'),(28,'+79104563333','пер. Лесной, 7',1,'2025-09-24',NULL,'Перевод',4,'Наталья'),(29,'+79107896666','ул. Восточная, 10',6,'2025-09-24','VIP-обслуживание','Карта',4,'Александр'),(30,'+79101117777','пр. Западный, 30',3,'2025-09-24',NULL,'Наличные',4,'Юлия'),(31,'+79102228888','ул. Южная, 12',2,'2025-09-24','Без спешки','Перевод',4,'Максим'),(32,'+79103339999','пр. Восточный, 5',4,'2025-09-24',NULL,'Карта',4,'Татьяна'),(33,'+79106660000','ул. Набережная, 20',1,'2025-09-24','Срочно!','Наличные',4,'Игорь'),(34,'+79109992222','пер. Школьный, 15',3,'2025-09-24','Особые пожелания','Перевод',4,'Ольга'),(35,'+79105551111','ул. Парковая, 10',2,'2025-09-24',NULL,'Карта',4,'Сергей'),(36,'+79103330000','пр. Комсомольский, 5',4,'2025-09-24',NULL,'Карта',5,'Татьяна'),(37,'+79106661111','ул. Зелёная, 20',1,'2025-09-24','Срочно!','Наличные',5,'Игорь'),(38,'+79109993333','пер. Школьный, 15',3,'2025-09-24','Особые пожелания','Перевод',5,'Ольга'),(39,'+79105552222','ул. Парковая, 10',2,'2025-09-24',NULL,'Карта',5,'Сергей'),(40,'+79108884444','пр. Советский, 25',4,'2025-09-24',NULL,'Карта',6,'Анна'),(41,'+79101231111','ул. Новая, 15',2,'2025-09-24','Праздничный стол','Наличные',6,'Мария'),(42,'+79104565555','пер. Старый, 7',1,'2025-09-24',NULL,'Перевод',6,'Алексей'),(43,'+79107898888','ул. Молодёжная, 10',6,'2025-09-24','VIP-обслуживание','Карта',6,'Ольга'),(44,'+79101119999','пр. Новый, 30',3,'2025-09-24',NULL,'Наличные',6,'Сергей'),(45,'+79102220000','ул. Центральная, 12',2,'2025-09-24','Без спешки','Перевод',6,'Анна'),(46,'+79103331111','пр. Комсомольский, 5',4,'2025-09-24',NULL,'Карта',6,'Татьяна'),(47,'+79106662222','ул. Зелёная, 20',1,'2025-09-24','Срочно!','Наличные',6,'Игорь'),(48,'+79109994444','пер. Школьный, 15',3,'2025-09-24','Особые пожелания','Перевод',6,'Ольга'),(49,'+79105553333','ул. Парковая, 10',2,'2025-09-24',NULL,'Карта',6,'Сергей');
/*!40000 ALTER TABLE `orders` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `other_orders`
--

DROP TABLE IF EXISTS `other_orders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `other_orders` (
  `id_other` int(11) NOT NULL AUTO_INCREMENT,
  `id_order` int(11) DEFAULT NULL,
  `id_status` int(11) DEFAULT NULL,
  PRIMARY KEY (`id_other`),
  KEY `id_order` (`id_order`),
  KEY `other_orders_ibfk_1` (`id_status`),
  CONSTRAINT `other_orders_ibfk_1` FOREIGN KEY (`id_status`) REFERENCES `status` (`id_status`),
  CONSTRAINT `other_orders_ibfk_2` FOREIGN KEY (`id_status`) REFERENCES `status` (`id_status`)
) ENGINE=InnoDB AUTO_INCREMENT=61 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `other_orders`
--

LOCK TABLES `other_orders` WRITE;
/*!40000 ALTER TABLE `other_orders` DISABLE KEYS */;
INSERT INTO `other_orders` VALUES (1,1,1),(2,2,1),(3,3,1),(4,4,1),(5,5,1),(6,6,1),(7,7,1),(8,8,1),(9,9,1),(10,10,1),(11,11,2),(12,12,2),(13,13,2),(14,14,2),(15,15,2),(16,16,2),(17,17,2),(18,18,2),(19,19,2),(20,20,2),(21,21,3),(22,22,3),(23,23,3),(24,24,3),(25,25,3),(26,26,3),(27,27,3),(28,28,3),(29,29,3),(30,30,3),(31,31,4),(32,32,4),(33,33,4),(34,34,4),(35,35,4),(36,36,4),(37,37,4),(38,38,4),(39,39,4),(40,40,4),(41,41,5),(42,42,5),(43,43,5),(44,44,5),(45,45,5),(46,46,5),(47,47,5),(48,48,5),(49,49,5),(50,50,5),(51,51,6),(52,52,6),(53,53,6),(54,54,6),(55,55,6),(56,56,6),(57,57,6),(58,58,6),(59,59,6),(60,60,6);
/*!40000 ALTER TABLE `other_orders` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `present`
--

DROP TABLE IF EXISTS `present`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `present` (
  `id_present` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  `from_price` decimal(10,2) DEFAULT NULL,
  PRIMARY KEY (`id_present`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `present`
--

LOCK TABLES `present` WRITE;
/*!40000 ALTER TABLE `present` DISABLE KEYS */;
INSERT INTO `present` VALUES (1,'Креветки в соусе',2000.00),(2,'Карбонара',1500.00),(3,'Борщ',2500.00),(4,'Чизбургер классический',3000.00);
/*!40000 ALTER TABLE `present` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `id_role` int(11) NOT NULL AUTO_INCREMENT,
  `role_name` varchar(50) NOT NULL,
  PRIMARY KEY (`id_role`),
  UNIQUE KEY `role_name` (`role_name`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `roles`
--

LOCK TABLES `roles` WRITE;
/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles` VALUES (3,'admin'),(2,'director'),(1,'manager');
/*!40000 ALTER TABLE `roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `status`
--

DROP TABLE IF EXISTS `status`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `status` (
  `id_status` int(11) NOT NULL AUTO_INCREMENT,
  `status_name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id_status`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `status`
--

LOCK TABLES `status` WRITE;
/*!40000 ALTER TABLE `status` DISABLE KEYS */;
INSERT INTO `status` VALUES (1,'В обработке'),(2,'Принят'),(3,'В приготовлении'),(4,'Готов'),(5,'В пути'),(6,'Доставлен');
/*!40000 ALTER TABLE `status` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `status_certificates`
--

DROP TABLE IF EXISTS `status_certificates`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `status_certificates` (
  `id_status_certificate` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id_status_certificate`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `status_certificates`
--

LOCK TABLES `status_certificates` WRITE;
/*!40000 ALTER TABLE `status_certificates` DISABLE KEYS */;
INSERT INTO `status_certificates` VALUES (1,'Активен'),(2,'Использован'),(3,'Возвращён');
/*!40000 ALTER TABLE `status_certificates` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
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
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (1,'Петров Петр Петрович',3,'admin','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918'),(2,'Иванов Иван Иванович',2,'director','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918'),(3,'Сидорова Анна Петровна',1,'manager1','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918'),(4,'Кузнецов Дмитрий Алексеевич',1,'manager2','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918'),(5,'Васильева Мария Игоревна',1,'manager3','8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918');
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-10-01  7:35:43
