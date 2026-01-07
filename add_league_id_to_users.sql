-- Add league_id column to users table
ALTER TABLE users ADD COLUMN league_id INTEGER;

-- Add foreign key constraint
ALTER TABLE users ADD CONSTRAINT fk_users_league 
    FOREIGN KEY (league_id) REFERENCES leagues(league_id);

-- Create index for better query performance
CREATE INDEX idx_users_league ON users(league_id);
