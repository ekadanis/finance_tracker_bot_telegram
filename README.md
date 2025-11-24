# 💰 Finance Tracker Telegram Bot

> Personal finance management bot built with .NET 9, PostgreSQL, and Clean Architecture

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-336791?logo=postgresql)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)

Track your income and expenses directly through Telegram with automatic categorization, real-time balance, and period recaps.

---

## ✨ Features

- 💸 **Income & Expense Tracking** - `/in gaji private 5000000` or `/out makan nasgor 15000`
- 📊 **Real-time Balance** - Check your financial status with `/saldo`
- 📅 **Period Recap** - View summaries with `/recap 01/01/2025 31/01/2025`
- 🏷️ **Auto-categorization** - Categories created automatically
- 🔔 **Daily Recap** - Scheduled summary at 23:00 WIB (production mode)

---

## 🏗️ Architecture

This project follows **Clean Architecture** principles with **Command Pattern** for telegram command handling.

```
┌─────────────────────────────────────────────────────────┐
│                   Telegram Bot API                       │
└────────────────────┬────────────────────────────────────┘
                     │
         ┌───────────▼──────────┐
         │  TelegramController   │
         └───────────┬───────────┘
                     │
         ┌───────────▼───────────┐
         │   TelegramService      │ (Router)
         └───────────┬────────────┘
                     │
       ┌─────────────┴─────────────┐
       │                           │
┌──────▼──────┐           ┌────────▼────────┐
│ Command     │           │  Command        │
│ Handlers    │           │  Handlers       │
│ (/in, /out) │           │ (/saldo, /recap)│
└──────┬──────┘           └────────┬────────┘
       │                           │
       └─────────────┬─────────────┘
                     │
         ┌───────────▼───────────┐
         │   Service Layer       │
         │ (User, Transaction,   │
         │  Category Services)   │
         └───────────┬───────────┘
                     │
         ┌───────────▼───────────┐
         │   EF Core DbContext   │
         └───────────┬───────────┘
                     │
         ┌───────────▼───────────┐
         │   PostgreSQL Database │
         └───────────────────────┘
```

**Tech Stack:**

- **Backend:** .NET 9 Web API, Entity Framework Core
- **Database:** PostgreSQL 15
- **Architecture:** Clean Architecture + Command Pattern
- **Deployment:** Docker + Cloudflare Tunnel

---

## 📱 Bot Commands

| Command       | Example                        | What it does      |
| ------------- | ------------------------------ | ----------------- |
| `/start`      | `/start`                       | Welcome message   |
| `/help`       | `/help`                        | Show all commands |
| `/in`         | `/in gaji januari 5000000`     | Record income     |
| `/out`        | `/out makan nasgor 15000`      | Record expense    |
| `/saldo`      | `/saldo`                       | Check balance     |
| `/recap`      | `/recap 01/01/2025 31/01/2025` | Period summary    |
| `/categories` | `/categories`                  | List categories   |

---

## 🚀 Setup Guide

### Choose Your Path:

- **[Local Development](#-local-development)** - Best for coding & debugging
- **[Docker Deployment](#-docker-deployment)** - Best for production

---

## 🤖 STEP 1: Create Telegram Bot (Required for Both)

**Do this first before anything else!**

### 1.1 Get Bot Token

1. Open Telegram → Search **@BotFather**
2. Send `/newbot`
3. Name your bot: `Finance Tracker Bot`
4. Username: `your_finance_tracker_bot`
5. **COPY THE TOKEN:** `1234567890:ABCdefGHIjklMNOpqrsTUVwxyz`

### 1.2 Setup Commands

1. Send `/setcommands` to @BotFather
2. Select your bot
3. Paste this:

```
start - Welcome message and bot introduction
help - Show all available commands
in - Record income transaction
out - Record expense transaction
saldo - Check current balance
recap - View transaction summary for a period
categories - List all your categories
```

✅ **Done! Now choose Local Development or Docker below.**

---

## 💻 Local Development

**Best for:** Development, debugging, learning

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 15](https://www.postgresql.org/download/)
- [ngrok account](https://ngrok.com/signup) (free)

### STEP 2: Clone Project

```bash
git clone https://github.com/ekadanis/finance_tracker_bot_telegram.git
cd finance_tracker_bot_telegram/FinanceTracker.Api
```

### STEP 3: Setup ngrok

```bash
# Get authtoken from: https://dashboard.ngrok.com/get-started/your-authtoken
ngrok config add-authtoken YOUR_AUTH_TOKEN

# Start tunnel (KEEP THIS RUNNING)
ngrok http 5222
```

**Copy the HTTPS URL:** `https://abc123.ngrok-free.app`

### STEP 4: Setup Database

```bash
psql -U postgres
CREATE DATABASE finance_tracker_dev;
\q
```

### STEP 5: Configure App

```bash
# Copy template
cp appsettings.json appsettings.Development.json

# Edit file
nano appsettings.Development.json
```

**Paste this (replace values):**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Username=postgres;Password=YOUR_POSTGRES_PASSWORD;Database=finance_tracker_dev"
  },
  "TelegramSettings": {
    "BotToken": "1234567890:ABCdefGHIjklMNOpqrsTUVwxyz",
    "WebhookUrl": "https://abc123.ngrok-free.app/api/telegram/webhook"
  }
}
```

### STEP 6: Run Migrations

```bash
dotnet ef database update
```

### STEP 7: Start API

```bash
dotnet watch run --environment Development
```

✅ **API running at:** `http://localhost:5222`

### STEP 8: Set Webhook

**Open NEW terminal:**

```bash
curl -X POST http://localhost:5222/api/telegram/setwebhook
```

### STEP 9: Test! 🎉

Open Telegram → Send `/start` to your bot!

**Try:**

```
/in gaji bulanan 5000000
/out makan nasgor 15000
/saldo
```

---

## 🐳 Docker Deployment

**Best for:** Production, VPS deployment

### Prerequisites

- [Docker](https://www.docker.com/get-started) installed
- A server (VPS, DigitalOcean, etc.)

### STEP 2: Clone Project (on server)

```bash
git clone https://github.com/ekadanis/finance_tracker_bot_telegram.git
cd finance_tracker_bot_telegram/FinanceTracker.Api
```

### STEP 3: Create `.env`

```bash
nano .env
```

**Paste this:**

```env
ASPNETCORE_ENVIRONMENT=Production
POSTGRES_PASSWORD=your_strong_password
TELEGRAM_BOT_TOKEN=1234567890:ABCdefGHIjklMNOpqrsTUVwxyz
```

**Save:** Ctrl+X → Y → Enter

### STEP 4: Start Docker

```bash
docker-compose up -d
```

**This starts:**

- PostgreSQL database
- .NET API (with daily recap scheduler)
- Cloudflare Tunnel (auto HTTPS)

### STEP 5: Get Webhook URL

```bash
docker-compose logs cloudflared | grep "https://"
```

**Copy URL:** `https://random-name-1234.trycloudflare.com`

### STEP 6: Update `.env`

```bash
nano .env
```

**Add this line:**

```env
WEBHOOK_URL=https://random-name-1234.trycloudflare.com/api/telegram/webhook
```

### STEP 7: Restart API

```bash
docker-compose restart api
```

### STEP 8: Set Webhook

```bash
curl -X POST http://localhost:8081/api/telegram/setwebhook
```

### STEP 9: Test! 🎉

Send `/start` to your bot in Telegram!

**Your bot is now LIVE 24/7!**

---

## 🔧 Troubleshooting

### Bot not responding?

```bash
# Check API logs
docker-compose logs -f api

# Check webhook status
curl http://localhost:8081/api/telegram/webhookinfo

# Re-set webhook
curl -X POST http://localhost:8081/api/telegram/deletewebhook
curl -X POST http://localhost:8081/api/telegram/setwebhook
```

### Database error?

```bash
# Check DB container
docker-compose ps

# View DB logs
docker-compose logs db
```

---

## 📊 Local vs Docker

| Feature         | Local Development   | Docker Production      |
| --------------- | ------------------- | ---------------------- |
| **Setup Time**  | 15 min              | 10 min                 |
| **Debugging**   | ✅ Full breakpoints | ❌ Logs only           |
| **Hot Reload**  | ✅ Auto-restart     | ❌ Manual rebuild      |
| **Daily Recap** | ❌ Disabled         | ✅ Enabled (23:00 WIB) |
| **URL**         | ngrok (temporary)   | Cloudflare (permanent) |
| **Use Case**    | Development         | Production             |

---
