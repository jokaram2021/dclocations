
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
    image_url VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);


CREATE TABLE favorites (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    location_id INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    -- Foreign Keys
    CONSTRAINT fk_favorites_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_favorites_location
        FOREIGN KEY (location_id) REFERENCES locations(id)
        ON DELETE CASCADE,

 
    CONSTRAINT uq_user_location UNIQUE (user_id, location_id)
);




INSERT INTO users (username, email, password_hash, role)
VALUES
('admin', 'admin@dc.com', 'password123', 'Admin'),
('batfan', 'batfan@dc.com', 'password123', 'User');


INSERT INTO locations (name, category, description, associated_hero, universe_region, first_appearance, image_url)
VALUES
('Gotham City', 'City', 'A dark and crime-filled city protected by Batman.', 'Batman', 'Earth', 'Detective Comics #27', '/images/gotham.png'),
('Metropolis', 'City', 'The home city of Superman.', 'Superman', 'Earth', 'Action Comics #1', '/images/metropolis.png'),
('Themyscira', 'Island', 'Hidden homeland of the Amazons.', 'Wonder Woman', 'Earth', 'All Star Comics #8', '/images/themyscira.png'),
('Atlantis', 'Kingdom', 'An underwater kingdom ruled by Aquaman.', 'Aquaman', 'Earth', 'More Fun Comics #73', '/images/atlantis.png'),
('Batcave', 'Base', 'Batmans secret headquarters.', 'Batman', 'Gotham', 'Detective Comics #83', '/images/batcave.png');


INSERT INTO favorites (user_id, location_id)
VALUES
(2, 1),
(2, 5);


-- EXAMPLE QUERIES



-- GET ALL LOCATIONS

-- SELECT * FROM locations;


-- GET LOCATION BY ID

-- SELECT * FROM locations WHERE id = 1;


-- SEARCH LOCATIONS

-- SELECT * FROM locations
-- WHERE name LIKE '%gotham%';


-- FILTER BY CATEGORY

-- SELECT * FROM locations
-- WHERE category = 'City';


-- GET USER FAVORITES

-- SELECT l.*
-- FROM favorites f
-- JOIN locations l ON f.location_id = l.id
-- WHERE f.user_id = 2;


-- ADD FAVORITE

-- INSERT INTO favorites (user_id, location_id)
-- VALUES (2, 3);



-- DELETE FROM favorites
-- WHERE user_id = 2 AND location_id = 3;


-- CREATE NEW LOCATION

-- INSERT INTO locations
-- (name, category, description, associated_hero, universe_region, first_appearance, image_url)
-- VALUES
-- ('Central City', 'City', 'Home of the Flash.', 'Flash', 'Earth', 'Flash Comics #1', '/images/central.png');

-- UPDATE LOCATION

-- UPDATE locations
-- SET description = 'Updated description'
-- WHERE id = 1;

-- DELETE LOCATION

-- DELETE FROM locations
-- WHERE id = 5;
