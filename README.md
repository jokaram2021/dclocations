# DC Universe Locations

## Project Overview

DC Universe Locations is a full-stack web application that allows users to browse, search, filter, favorite, and explore fictional locations from the DC universe.

The website supports user accounts, login/logout, favorite locations, location detail pages, Wiki links, featured locations, admin location management, and an Energy Match feature that consumes the instructor-provided read-only Pokémon Type API.

The project includes:

- Website frontend
- ASP.NET Core backend API
- MySQL database
- Docker / Docker Compose setup
- JWT bearer token authentication
- Role-based admin authorization
- Swagger API documentation
- Unit tests
- GitHub Actions workflow
- Integration with the instructor-provided read-only API

---

## Project Theme

The theme of this project is **DC Universe locations**.

Example locations include:

- Gotham City
- Metropolis
- Atlantis
- Themyscira
- Batcave
- Central City
- Arkham Asylum
- Oa
- Fortress of Solitude
- Hall of Justice

---

## Main Features

### Guest Features

Guests can:

- View the home page
- View featured locations
- Browse all DC locations
- Search locations by name
- Filter locations by category
- View location detail pages
- Open Wiki links
- Register for an account
- Log in

Guests cannot:

- Add locations to favorites
- View the favorites page
- Use admin create/edit/delete features

---

### Registered User Features

Registered users can:

- Log in
- Log out
- Browse all locations
- Search and filter locations
- View location details
- Add locations to favorites
- View saved favorite locations
- Remove locations from favorites
- Open Wiki links
- View the Energy Match on each detail page

Favorites are tied to the currently logged-in user.

---

### Admin Features

Admin users can:

- Add new DC locations
- Edit existing locations
- Delete locations
- Manage Wiki URLs
- Manage Pokémon Energy Match names
- View all regular user features

Admin-only API routes are protected using JWT bearer authentication and role-based authorization.

---


### Featured Locations



The app tracks how many times users open each location detail page. The featured section displays three featured locations based on that view data. If there is no view data yet, the app still displays featured locations so the home page is never empty.

### Wiki Links

Each location can include a Wiki URL. Users can click **Explore Wiki** from the locations page, detail page, favorites page, or featured locations section to learn more about that DC location.

### Energy Match

Each location has a Pokémon Energy Match. The backend calls the provided read-only Pokémon Type API and displays the returned Pokémon type data on the location detail page.

