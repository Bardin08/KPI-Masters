CREATE TABLE plastics (
                          id SERIAL PRIMARY KEY,
                          country TEXT NOT NULL,
                          year INTEGER NOT NULL,
                          parent_company TEXT,
                          empty NUMERIC,
                          hdpe NUMERIC,
                          ldpe NUMERIC,
                          o NUMERIC,
                          pet NUMERIC,
                          pp NUMERIC,
                          ps NUMERIC,
                          pvc NUMERIC,
                          grand_total NUMERIC,
                          num_events INTEGER,
                          volunteers INTEGER
);

COPY plastics (country, year, parent_company, empty, hdpe, ldpe, o, pet, pp, ps, pvc, grand_total, num_events, volunteers)
    FROM 'ext-data/plastics.csv'
    DELIMITER ','
    CSV HEADER
    NULL 'NA';
