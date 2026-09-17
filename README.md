# Logistikk – Backend API

Backend for skoleprosjektet Logistikk, bygget med ASP.NET Core (.NET 10). 
API-et brukes av både webfrontend og mobilappen.

## Kom i gang

1. Klon repoet:git clone https://github.com/AliPluss/logistikk.git

2. Åpne `logistikk.slnx` i Visual Studio.
3. Sett opp hemmeligheter lokalt (se under "Oppsett av secrets").
4. Trykk `F5` for å starte.
5. Åpne API-dokumentasjonen i nettleseren: https://localhost:7121/scalar


## Oppsett av secrets (kreves for at prosjektet skal starte)

Prosjektet bruker JWT for innlogging. Nøkkelen ligger IKKE i koden av sikkerhetshensyn, 
så hver utvikler må legge inn sin egen lokalt:

1. Høyreklikk på prosjektet `logistikk` i Solution Explorer.
2. Velg **Manage User Secrets**.
3. Lim inn følgende, og bytt ut verdien med en tilfeldig tekst på minst 64 tegn:
```json
   {
     "Jwt": {
       "Key": "sett-inn-din-egen-lange-tilfeldige-nokkel-her"
     }
   }
```

Uten dette vil prosjektet stoppe med feilmeldingen `Jwt:Key mangler i konfigurasjonen`.

## Teknologi

- ASP.NET Core 10.0 Web API
- Entity Framework Core (InMemory database – midlertidig, byttes ut senere)
- ASP.NET Core Identity (brukere og roller)
- JWT for autentisering
- Scalar for API-dokumentasjon

## Base-URL (lokalt) https://localhost:7121


## Tilgjengelige endepunkter

| Metode | Endepunkt | Beskrivelse | Krever token |
|---|---|---|---|
| POST | `/api/auth/register` | Registrer ny bruker | Nei |
| POST | `/api/auth/login` | Logg inn, returnerer JWT-token | Nei |
| GET | `/api/auth/me` | Testendepunkt for innlogget bruker | Ja |

Full og oppdatert dokumentasjon med eksempler finner du alltid på `/scalar` når prosjektet kjører.

## Slik bruker dere JWT-tokenet

Etter innlogging får dere et token i responsen:
```json
{ "token": "eyJhbGciOiJIUzI1NiIs..." }
```

Dette tokenet må legges ved i alle senere kall som krever innlogging, i headeren: Authorization: Bearer <token>


Tokenet er gyldig i 60 minutter. Etter det må brukeren logge inn på nytt.

## Feilhåndtering

API-et bruker standard HTTP-statuskoder:

- `200 OK` – vellykket forespørsel
- `201 Created` – ny ressurs opprettet
- `400 Bad Request` – ugyldige data, se `errors` i responsen for detaljer
- `401 Unauthorized` – mangler eller ugyldig token
- `404 Not Found` – ressursen finnes ikke

Eksempel på valideringsfeil ved registrering:
```json
{
  "errors": {
    "PasswordRequiresUpper": ["Passwords must have at least one uppercase ('A'-'Z')."]
  }
}
```

## Viktig å vite

- Databasen kjører foreløpig i minnet (InMemory). All data forsvinner når serveren restartes. 
  Dette endres når gruppen har bestemt hvilken database som skal brukes i produksjon.
- Endepunktene og responsformatet vil forbli de samme etter at databasen byttes ut.
- Repoet er foreløpig offentlig (public). Ingen passord eller hemmeligheter skal noen gang 
  legges i kode eller commits – bruk User Secrets lokalt.

## Kontakt

Backend utvikles av Ali (Milan). Ta kontakt ved spørsmål om API-et.