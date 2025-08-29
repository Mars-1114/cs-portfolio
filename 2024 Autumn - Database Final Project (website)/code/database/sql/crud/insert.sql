INSERT INTO Athlete (athlete_id, name, sex, born, height, weight, country, country_noc)
VALUES (?, ?, ?, ?, ?, ?, ?, ?);

INSERT INTO AthleteRecords (sport, athlete_id, country, name, grade, year, ascend)
VALUES
(?, ?, ?, ?, ?, ?, ?);

INSERT INTO Medal (edition_id, edition, year, country, country_noc, gold, silver, bronze)
VALUES
(?, ?, ?, ?, ?,? , ?, ?);

INSERT INTO Games (edition, edition_id, edition_url, year, city, country_flag_url, country_noc, start_date, end_date, competition_date, isHeld)
VALUES
(?, ?, ?, ?, ?, ?, ?, ?, ?);

INSERT INTO Details (edition, edition_id, country_noc, sport, event, result_id, athlete, athlete_id, pos, medal, isTeamSport)
VALUES
(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);


INSERT INTO Results (result_id, event_title, edition, edition_id, sport, sport_url, result_date, result_location, result_participants, result_format, result_detail, result_description)
VALUES
(?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);


INSERT INTO Country (noc, country)
VALUES
(?, ?);


