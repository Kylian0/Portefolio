# Environnement Docker de développement

Cette configuration lance le front-end Blazor WebAssembly, l'API ASP.NET Core, MySQL et phpMyAdmin. Les deux projets .NET utilisent `dotnet watch run` et les sources locales sont montées dans leurs conteneurs.

## Préparation

Depuis le dossier `Conteneurisation`, créez le fichier local `.env` :

```powershell
Copy-Item .env.example .env
```

Modifiez ensuite les deux mots de passe dans `.env`. Ce fichier est ignoré par Git.

## Lancement

```powershell
docker compose up --build
```

Pour lancer les services en arrière-plan :

```powershell
docker compose up --build -d
```

Pour arrêter l'environnement :

```powershell
docker compose down
```

Pour arrêter l'environnement et supprimer également les données MySQL :

```powershell
docker compose down --volumes
```

## URL locales

- Front-end : http://localhost:5200
- Back-end : http://localhost:5097
- Swagger UI : http://localhost:5097/swagger
- Documentation OpenAPI du back-end en développement : http://localhost:5097/openapi/v1.json
- phpMyAdmin : http://localhost:8081
- MySQL depuis la machine hôte : `localhost:3307`

Dans le réseau Docker, MySQL est accessible avec le nom d'hôte `mysql` et le port `3306`. Le back-end n'utilise volontairement pas encore cette base de données.

## Hot Reload

Les dossiers `../FrontEnd` et `../BackEnd` sont montés directement dans les conteneurs. Les volumes séparés pour `bin` et `obj` évitent de mélanger les artefacts Linux des conteneurs avec ceux de la machine hôte. Les modifications de sources sont détectées par `dotnet watch` en mode polling.
