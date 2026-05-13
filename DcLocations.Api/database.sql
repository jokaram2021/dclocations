CREATE DATABASE IF NOT EXISTS dc_universe_locations;

USE dc_universe_locations;

DROP TABLE IF EXISTS favorites;
DROP TABLE IF EXISTS locations;
DROP TABLE IF EXISTS users;

CREATE TABLE users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(20) NOT NULL DEFAULT 'User',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE locations (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    category VARCHAR(50) NOT NULL,
    description TEXT NOT NULL,
    associated_hero VARCHAR(100),
    universe_region VARCHAR(100),
    first_appearance VARCHAR(100),
    image_url VARCHAR(500),
    wiki_url VARCHAR(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
        ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE favorites (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    location_id INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_favorites_user
        FOREIGN KEY (user_id)
        REFERENCES users(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_favorites_location
        FOREIGN KEY (location_id)
        REFERENCES locations(id)
        ON DELETE CASCADE,

    CONSTRAINT uq_user_location
        UNIQUE (user_id, location_id)
);

INSERT INTO users
(
    username,
    email,
    password_hash,
    role
)
VALUES
(
    'admin',
    'admin@dc.com',
    'EF92B778BAFE771E89245B89ECBC08A44A4E166C06659911881F383D4473E94F',
    'Admin'
),
(
    'batfan',
    'batfan@dc.com',
    'EF92B778BAFE771E89245B89ECBC08A44A4E166C06659911881F383D4473E94F',
    'User'
),
(
    'supergirl',
    'supergirl@dc.com',
    'EF92B778BAFE771E89245B89ECBC08A44A4E166C06659911881F383D4473E94F',
    'User'
),
(
    'flashfan',
    'flashfan@dc.com',
    'EF92B778BAFE771E89245B89ECBC08A44A4E166C06659911881F383D4473E94F',
    'User'
);

INSERT INTO locations
(
    name,
    category,
    description,
    associated_hero,
    universe_region,
    first_appearance,
    image_url,
    wiki_url
)
VALUES
(
    'Gotham City',
    'City',
    'A dark and crime-filled city protected by Batman.',
    'Batman',
    'Earth',
    'Detective Comics #27',
    '',
    'https://dc.fandom.com/wiki/Gotham_City'
),
(
    'Metropolis',
    'City',
    'The futuristic home city of Superman.',
    'Superman',
    'Earth',
    'Action Comics #1',
    '',
    'https://dc.fandom.com/wiki/Metropolis'
),
(
    'Themyscira',
    'Island',
    'Hidden homeland of the Amazons and Wonder Woman.',
    'Wonder Woman',
    'Earth',
    'All Star Comics #8',
    '',
    'https://dc.fandom.com/wiki/Themyscira'
),
(
    'Atlantis',
    'Kingdom',
    'The underwater kingdom ruled by Aquaman.',
    'Aquaman',
    'Earth',
    'More Fun Comics #73',
    '',
    'https://dc.fandom.com/wiki/Atlantis'
),
(
    'Batcave',
    'Base',
    'Batmans secret underground headquarters.',
    'Batman',
    'Gotham',
    'Detective Comics #83',
    '',
    'https://dc.fandom.com/wiki/Batcave'
),
(
    'Central City',
    'City',
    'The fast-moving home city of The Flash.',
    'Flash',
    'Earth',
    'Flash Comics #1',
    '',
    'https://dc.fandom.com/wiki/Central_City'
),
(
    'Star City',
    'City',
    'Modern city protected by Green Arrow.',
    'Green Arrow',
    'Earth',
    'More Fun Comics #73',
    '',
    'https://dc.fandom.com/wiki/Star_City'
),
(
    'Smallville',
    'Town',
    'The peaceful hometown of Clark Kent.',
    'Superman',
    'Kansas',
    'Superboy #2',
    '',
    'https://dc.fandom.com/wiki/Smallville'
),
(
    'Hall of Justice',
    'Headquarters',
    'Main headquarters of the Justice League.',
    'Justice League',
    'Earth',
    'Super Friends',
    '',
    'https://dc.fandom.com/wiki/Hall_of_Justice'
),
(
    'Arkham Asylum',
    'Institution',
    'High security psychiatric hospital for Gotham criminals.',
    'Batman',
    'Gotham',
    'Batman #258',
    '',
    'https://dc.fandom.com/wiki/Arkham_Asylum'
),
(
    'Oa',
    'Planet',
    'Homeworld of the Green Lantern Corps.',
    'Green Lantern',
    'Space Sector 0',
    'Showcase #22',
    '',
    'https://dc.fandom.com/wiki/Oa'
),
(
    'Fortress of Solitude',
    'Base',
    'Supermans private Arctic sanctuary.',
    'Superman',
    'Arctic',
    'Action Comics #241',
    '',
    'https://dc.fandom.com/wiki/Fortress_of_Solitude'
);

INSERT INTO favorites
(
    user_id,
    location_id
)
VALUES
(
    2,
    1
),
(
    2,
    5
),
(
    3,
    3
);