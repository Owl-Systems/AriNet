# AriNetClient  
### Modern WebSocket Event Client for PBX Systems (.NET)

AriNetClient is a **modern, event-driven, strongly-typed WebSocket client** designed to integrate PBX systems (such as **Wazo** and **Asterisk**) into **ASP.NET Core / Web API** applications.

The library abstracts all WebSocket complexity and exposes PBX events as **clean domain events**, allowing developers to focus purely on **business logic**.

---

## 🚀 Why AriNetClient?

Traditional PBX integrations require developers to:
- Manually manage WebSocket connections
- Parse raw JSON messages
- Handle reconnections and failures
- Mix infrastructure logic with business logic

**AriNetClient solves this by design.**

> You never deal with WebSockets, JSON, or server-specific formats.  
> You only handle **strongly-typed events**.

---

## ✨ Key Features

- ✅ Strongly typed domain events
- ✅ Event-driven architecture (similar to AsterNET.ARI, but modern)
- ✅ Built-in Dependency Injection support
- ✅ Automatic event dispatching
- ✅ Multiple handlers per event
- ✅ BackgroundService friendly
- ✅ Auto-reconnect and fault tolerance
- ✅ Clean separation of concerns
- ✅ Extensible for multiple PBX servers

---

## 🧠 Design Philosophy

> **Unify your domain, not the PBX servers**

Each PBX server emits different event names and payloads.  
AriNetClient normalizes them into **domain-level events** that your application depends on.

### Responsibility Boundaries

| Layer | Responsibility |
|-----|---------------|
| WebSocketClient | Connection lifecycle |
| Event Parser | Parse raw server messages |
| Event Adapter | Convert to domain events |
| Event Dispatcher | Route events |
| Event Handlers | Business logic |
| Your App | CRM, Billing, Rules, Notifications |

---

## ⚙️ Configuration

### appsettings.json

```
json
{
  "Wazo": {
    "WebSocket": {
      "Url": "wss://your-wazo-server:443/ari/events",
      "AuthToken": "your-auth-token",
      "ApplicationName": "my-web-api-app",
      "AutoReconnect": true
    }
  }
}
```

---

## 📦 Installation

Add the package to your project:

```xml
<PackageReference Include="AriNetClient" Version="1.0.0" />


```bash
# بناء المكتبة
dotnet build -c Release

# إنشاء حزمة NuGet
dotnet pack -c Release

# نشر إلى NuGet
dotnet nuget push bin/Release/WazoNet.1.0.0.nupkg -k YOUR_API_KEY -s https://api.nuget.org/v3/index.json