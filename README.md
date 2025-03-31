# CharityEventApp – Test projekt

See on heategevusürituse rakenduse backend. Kasutaja (kassa - müüja) saab hallata tooteid, lisada neid ostukorvi ja sooritada makseid.

---

## Kasutatud tehnoloogiad

- **.NET 8 / ASP.NET Core Web API**
- **PostgreSQL**
- **Docker / Docker Compose**
- **EF Core (Code-First)**
- **Swagger**

---

## 🚀 Käivitamine (Docker Compose)

1. Veendu, et Docker on paigaldatud.
2. Ava terminal projektikaustas.
3. Käivita:

```bash
docker-compose up --build
```

---

Rakendus peaks olema kättesaadav aadressil: http://localhost:8080

Swagger UI dokumentatsioon: http://localhost:8080/swagger

Second-hand eseme kogust saab sisestada endpointil: api/Products/update-stock/{id}

---

## API Endpoint'id

| Meetod | Endpoint | Kirjeldus |
|--------|----------|-----------|
| GET    | /api/products | Tagastab kõik tooted |
| PUT    | /api/products/update-stock/{id} | Uuendab toote kogust |
| POST   | /api/products/add-to-cart | Lisa toode ostukorvi |
| POST   | /api/products/checkout | Kinnita makse |
| POST   | /api/products/reset/{transactionId} | Tühista tehing |
| GET    | /api/products/transaction-id/{sessionId} | Tagasta aktiivne transactionId |

---
## Diagrammid

Component & Use case diagrams

![image](https://github.com/user-attachments/assets/54d509c2-9c92-40d1-8390-ffae7f468307) ![image](https://github.com/user-attachments/assets/002fd5d9-cb89-4e49-b093-922ac8b011a3)

---

## Nõuded
Nõuded täidetud koos boonusega - lugeda tooted andmebaasi failist.

Kuidas sai lahendatud mitme kasutja probleem?

Iga kassa kasutab oma unikaalset sessionId, sellega tagan, et toote lisamine ostukorvi toimub ainult ühe tehingu piires. Kui toode on juba ostukorvis, siis lihtsalt suurendatakse kogust, mitte ei lisata uut toodet.

Laoseisu uuendatakse pärast toote lisamist ostukorvi, vältides olukorda, kus mitmed kassad saaksid sama toote korraga osta. Seega sessionId kaudu on tagatud, et iga kassa saab hallata ainult oma ostukorvi.

Kui ostukorv tühistatakse, siis uuendatakse laoseisu ehk suurendatakse toote laoseisu vastavalt ostukorvile.

Ostukorvil on olemas kolm staatust - töös, täidetud ja tühistatud. Esimese toote lisamisel tehakse transaction objekt, mis on töös staatuses.

---

## Testimine - Mocks

Testid on tehtud toodete pärimisele ja toote koguse muutmisele.

---

## Võimalikud täiendused

See projekt ei sisalda autentimist ega rolle. Kastasin sessiooni id-d, et trackida ühe kassa ostukorvi.
