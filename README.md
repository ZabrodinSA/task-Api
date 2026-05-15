# TaskApi

## Описание проекта
TaskApi — это веб-приложение, разработанное на C# с использованием ASP.NET Core, которое взаимодействует с PostgreSQL и RabbitMQ.  
Приложение предоставляет API для управления задачами и публикует события завершения задач в RabbitMQ.

---

## Требования
- Установлены **Docker** и **Docker Compose**
- Настроенный и доступный **PostgreSQL**
- Файл `.env` для хранения переменных окружения

---

## Настройка проекта

### 1. Конфигурация переменных окружения

Создайте файл `.env` в корне проекта и добавьте:

```env
# PostgreSQL
DB_USERNAME=postgres
DB_PASSWORD=password
DB_HOST=localhost

# RabbitMQ
RABBITMQ_HOST=localhost