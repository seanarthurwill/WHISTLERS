-- =============================================
-- Whistl3r PostgreSQL Aurora RDS Database Schema
-- Converted from SQL Server to PostgreSQL
-- Target: AWS RDS Aurora PostgreSQL
-- =============================================

-- Create database (run this separately as superuser)
-- CREATE DATABASE whistl3r_data WITH ENCODING='UTF8' LC_COLLATE='en_US.UTF-8' LC_CTYPE='en_US.UTF-8' TEMPLATE=template0;

-- Connect to the database
\c whistl3r_data;

-- Enable UUID extension for GUID/uniqueidentifier support
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Enable PostGIS if needed for spatial calculations (optional)
-- CREATE EXTENSION IF NOT EXISTS postgis;

-- =============================================
-- TABLES
-- =============================================

-- Users Table (Primary/Core table - create first due to FK dependencies)
CREATE TABLE users (
    user_id SERIAL PRIMARY KEY,
    tenant_id UUID NOT NULL DEFAULT uuid_generate_v4(),
    role_id INTEGER,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    phone VARCHAR(20),
    password_hash VARCHAR(500) NOT NULL,
    date_of_birth DATE,
    user_type VARCHAR(20) CHECK (user_type IN ('Official', 'Assignor', 'Parent', 'Mentor')),
    is_active BOOLEAN DEFAULT TRUE,
    email_verified BOOLEAN DEFAULT FALSE,
    phone_verified BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    last_login TIMESTAMPTZ
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_tenant ON users(tenant_id);
CREATE INDEX idx_users_user_type ON users(user_type);

-- Roles Table
CREATE TABLE roles (
    role_id SERIAL PRIMARY KEY,
    role_name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    is_system_role BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- Permissions Table
CREATE TABLE permissions (
    permission_id SERIAL PRIMARY KEY,
    permission_name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE
);

-- RolePermissions Table
CREATE TABLE role_permissions (
    role_permission_id SERIAL PRIMARY KEY,
    role_id INTEGER NOT NULL,
    permission_id INTEGER NOT NULL,
    UNIQUE(role_id, permission_id)
);

-- Sports Table
CREATE TABLE sports (
    sport_id SERIAL PRIMARY KEY,
    sport_name VARCHAR(100) NOT NULL UNIQUE,
    sport_code VARCHAR(20) UNIQUE,
    is_active BOOLEAN DEFAULT TRUE
);

-- AgeLevels Table
CREATE TABLE age_levels (
    age_level_id SERIAL PRIMARY KEY,
    sport_id INTEGER NOT NULL,
    age_level_name VARCHAR(50) NOT NULL,
    min_age INTEGER,
    max_age INTEGER,
    display_order INTEGER,
    is_active BOOLEAN,
    UNIQUE(sport_id, age_level_name)
);

-- Languages Table
CREATE TABLE languages (
    language_id SERIAL PRIMARY KEY,
    language_code VARCHAR(10) NOT NULL UNIQUE,
    language_name VARCHAR(100) NOT NULL,
    is_active BOOLEAN DEFAULT TRUE
);

-- Organizations Table
CREATE TABLE organizations (
    organization_id SERIAL PRIMARY KEY,
    tenant_id UUID NOT NULL DEFAULT uuid_generate_v4(),
    organization_name VARCHAR(200) NOT NULL,
    organization_type VARCHAR(100),
    website VARCHAR(500),
    contact_email VARCHAR(255),
    contact_phone VARCHAR(20),
    address_line1 VARCHAR(200),
    address_line2 VARCHAR(200),
    city VARCHAR(100),
    state_province VARCHAR(100),
    postal_code VARCHAR(20),
    country VARCHAR(100),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_organizations_tenant ON organizations(tenant_id);

-- Venues Table
CREATE TABLE venues (
    venue_id SERIAL PRIMARY KEY,
    organization_id INTEGER NOT NULL,
    venue_name VARCHAR(200) NOT NULL,
    address_line1 VARCHAR(200),
    address_line2 VARCHAR(200),
    city VARCHAR(100),
    state_province VARCHAR(100),
    postal_code VARCHAR(20),
    country VARCHAR(100),
    latitude DECIMAL(10, 8),
    longitude DECIMAL(11, 8),
    timezone VARCHAR(100),
    is_active BOOLEAN DEFAULT TRUE
);

CREATE INDEX idx_venues_organization ON venues(organization_id);
CREATE INDEX idx_venues_location ON venues(latitude, longitude);

-- VenueSports Table
CREATE TABLE venue_sports (
    venue_sport_id SERIAL PRIMARY KEY,
    venue_id INTEGER NOT NULL,
    sport_id INTEGER NOT NULL,
    number_of_fields INTEGER DEFAULT 1,
    UNIQUE(venue_id, sport_id)
);

-- VenueAgeLevels Table
CREATE TABLE venue_age_levels (
    venue_age_level_id SERIAL PRIMARY KEY,
    venue_id INTEGER NOT NULL,
    age_level_id INTEGER NOT NULL,
    is_restricted BOOLEAN DEFAULT FALSE,
    UNIQUE(venue_id, age_level_id)
);

-- Leagues Table
CREATE TABLE leagues (
    league_id SERIAL PRIMARY KEY,
    sport_id INTEGER NOT NULL,
    league_name VARCHAR(200) NOT NULL,
    season VARCHAR(50),
    start_date DATE,
    end_date DATE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_leagues_sport ON leagues(sport_id);
CREATE INDEX idx_leagues_season ON leagues(season);

-- LeagueOrganizations Table
CREATE TABLE league_organizations (
    league_organization_id SERIAL PRIMARY KEY,
    league_id INTEGER NOT NULL,
    organization_id INTEGER NOT NULL,
    joined_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    UNIQUE(league_id, organization_id)
);

-- LeagueAgeLevels Table
CREATE TABLE league_age_levels (
    league_age_level_id SERIAL PRIMARY KEY,
    league_id INTEGER NOT NULL,
    age_level_id INTEGER NOT NULL,
    default_game_length_minutes INTEGER,
    UNIQUE(league_id, age_level_id)
);

-- Tournaments Table
CREATE TABLE tournaments (
    tournament_id SERIAL PRIMARY KEY,
    organization_id INTEGER NOT NULL,
    league_id INTEGER,
    pay_scale_template_id INTEGER,
    tournament_name VARCHAR(200) NOT NULL,
    start_date DATE,
    end_date DATE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- Positions Table
CREATE TABLE positions (
    position_id SERIAL PRIMARY KEY,
    sport_id INTEGER NOT NULL,
    position_name VARCHAR(100) NOT NULL,
    position_code VARCHAR(20),
    display_order INTEGER DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    UNIQUE(sport_id, position_name)
);

-- PayScaleTemplates Table
CREATE TABLE pay_scale_templates (
    pay_scale_template_id SERIAL PRIMARY KEY,
    organization_id INTEGER NOT NULL,
    template_name VARCHAR(200) NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    created_by INTEGER NOT NULL
);

-- PayScaleRules Table
CREATE TABLE pay_scale_rules (
    pay_scale_rule_id SERIAL PRIMARY KEY,
    pay_scale_template_id INTEGER,
    sport_id INTEGER,
    age_level_id INTEGER,
    position_id INTEGER,
    league_id INTEGER,
    base_pay_amount DECIMAL(10, 2) NOT NULL,
    pay_multiplier DECIMAL(5, 2) DEFAULT 1.00,
    pay_per_km DECIMAL(10, 2) DEFAULT 0.00,
    priority INTEGER DEFAULT 100,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_pay_scale_rules_template ON pay_scale_rules(pay_scale_template_id);
CREATE INDEX idx_pay_scale_rules_sport ON pay_scale_rules(sport_id);

-- Games Table
CREATE TABLE games (
    game_id SERIAL PRIMARY KEY,
    organization_id INTEGER NOT NULL,
    league_id INTEGER,
    tournament_id INTEGER,
    venue_id INTEGER NOT NULL,
    age_level_id INTEGER NOT NULL,
    home_team VARCHAR(200) NOT NULL,
    away_team VARCHAR(200) NOT NULL,
    game_date DATE NOT NULL,
    game_time TIME NOT NULL,
    game_length_minutes INTEGER,
    override_game_length_minutes INTEGER,
    pay_scale_template_id INTEGER,
    status VARCHAR(20) DEFAULT 'Scheduled' 
        CHECK (status IN ('Scheduled', 'Assigned', 'InProgress', 'Completed', 'Cancelled')),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    created_by INTEGER NOT NULL
);

CREATE INDEX idx_games_organization ON games(organization_id);
CREATE INDEX idx_games_date ON games(game_date);
CREATE INDEX idx_games_venue ON games(venue_id);
CREATE INDEX idx_games_status ON games(status);

-- Assignors Table
CREATE TABLE assignors (
    assignor_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL UNIQUE,
    is_super_admin BOOLEAN DEFAULT FALSE
);

-- AssignorOrganizations Table
CREATE TABLE assignor_organizations (
    assignor_organization_id SERIAL PRIMARY KEY,
    assignor_id INTEGER NOT NULL,
    organization_id INTEGER NOT NULL,
    role_level VARCHAR(50) CHECK (role_level IN ('Admin', 'Manager', 'Viewer')),
    assigned_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    assigned_by INTEGER,
    is_active BOOLEAN DEFAULT TRUE,
    UNIQUE(assignor_id, organization_id)
);

-- AssignorLeagues Table
CREATE TABLE assignor_leagues (
    assignor_league_id SERIAL PRIMARY KEY,
    assignor_id INTEGER NOT NULL,
    league_id INTEGER NOT NULL,
    can_create_matches BOOLEAN DEFAULT TRUE,
    can_assign_officials BOOLEAN DEFAULT TRUE,
    can_manage_pay BOOLEAN DEFAULT TRUE,
    assigned_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE,
    UNIQUE(assignor_id, league_id)
);

-- Officials Table
CREATE TABLE officials (
    official_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL UNIQUE,
    preferred_language_id INTEGER,
    latitude DECIMAL(10, 8),
    longitude DECIMAL(11, 8),
    measurement_system VARCHAR(20) CHECK (measurement_system IN ('Metric', 'Imperial')),
    bio TEXT,
    approved_by INTEGER,
    approved_at TIMESTAMPTZ
);

CREATE INDEX idx_officials_location ON officials(latitude, longitude);

-- OfficialPositions Table
CREATE TABLE official_positions (
    official_position_id SERIAL PRIMARY KEY,
    official_id INTEGER NOT NULL,
    position_id INTEGER NOT NULL,
    years_experience INTEGER,
    is_preferred BOOLEAN DEFAULT FALSE,
    UNIQUE(official_id, position_id)
);

-- OfficialAgeLevels Table
CREATE TABLE official_age_levels (
    official_age_level_id SERIAL PRIMARY KEY,
    official_id INTEGER NOT NULL,
    age_level_id INTEGER NOT NULL,
    is_preferred BOOLEAN DEFAULT FALSE,
    UNIQUE(official_id, age_level_id)
);

-- OfficialLeagues Table
CREATE TABLE official_leagues (
    official_league_id SERIAL PRIMARY KEY,
    official_id INTEGER NOT NULL,
    league_id INTEGER NOT NULL,
    enrollment_status VARCHAR(20) DEFAULT 'Pending' 
        CHECK (enrollment_status IN ('Pending', 'Approved', 'Denied', 'Inactive')),
    enrolled_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    approved_by INTEGER,
    approved_at TIMESTAMPTZ,
    UNIQUE(official_id, league_id)
);

-- OfficialLanguages Table
CREATE TABLE official_languages (
    official_language_id SERIAL PRIMARY KEY,
    official_id INTEGER NOT NULL,
    language_id INTEGER NOT NULL,
    proficiency_level VARCHAR(20) CHECK (proficiency_level IN ('Basic', 'Intermediate', 'Fluent', 'Native')),
    UNIQUE(official_id, language_id)
);

-- OfficialSportPreferences Table
CREATE TABLE official_sport_preferences (
    official_sport_preference_id SERIAL PRIMARY KEY,
    official_id INTEGER NOT NULL,
    sport_id INTEGER NOT NULL,
    max_games_per_day INTEGER DEFAULT 3,
    max_games_per_week INTEGER,
    max_distance_km INTEGER,
    is_preferred BOOLEAN DEFAULT TRUE,
    UNIQUE(official_id, sport_id)
);

-- OfficialRatings Table
CREATE TABLE official_ratings (
    official_rating_id SERIAL PRIMARY KEY,
    official_id INTEGER NOT NULL,
    sport_id INTEGER NOT NULL,
    overall_rating DECIMAL(3, 2),
    total_games INTEGER DEFAULT 0,
    last_updated TIMESTAMPTZ,
    UNIQUE(official_id, sport_id)
);

-- OfficialGroups Table
CREATE TABLE official_groups (
    group_id SERIAL PRIMARY KEY,
    sport_id INTEGER NOT NULL,
    group_name VARCHAR(200) NOT NULL,
    description TEXT,
    created_by INTEGER NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- GroupMembers Table
CREATE TABLE group_members (
    group_member_id SERIAL PRIMARY KEY,
    group_id INTEGER NOT NULL,
    official_id INTEGER NOT NULL,
    joined_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(group_id, official_id)
);

-- GameAssignments Table
CREATE TABLE game_assignments (
    game_assignment_id SERIAL PRIMARY KEY,
    game_id INTEGER NOT NULL,
    official_id INTEGER NOT NULL,
    position_id INTEGER NOT NULL,
    assignment_status VARCHAR(20) DEFAULT 'Assigned' 
        CHECK (assignment_status IN ('Assigned', 'Accepted', 'Declined', 'Completed', 'NoShow')),
    assigned_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    assigned_by INTEGER NOT NULL,
    accepted_at TIMESTAMPTZ,
    completed_at TIMESTAMPTZ,
    base_pay_amount DECIMAL(10, 2),
    travel_pay_amount DECIMAL(10, 2),
    multiplier_applied DECIMAL(5, 2),
    final_pay_amount DECIMAL(10, 2),
    distance_km DECIMAL(10, 2),
    UNIQUE(game_id, official_id, position_id)
);

CREATE INDEX idx_game_assignments_game ON game_assignments(game_id);
CREATE INDEX idx_game_assignments_official ON game_assignments(official_id);
CREATE INDEX idx_game_assignments_status ON game_assignments(assignment_status);

-- GameClaims Table
CREATE TABLE game_claims (
    game_claim_id SERIAL PRIMARY KEY,
    game_id INTEGER NOT NULL,
    official_id INTEGER NOT NULL,
    position_id INTEGER NOT NULL,
    claim_status VARCHAR(20) DEFAULT 'Pending' 
        CHECK (claim_status IN ('Pending', 'Approved', 'Denied', 'Withdrawn')),
    claimed_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    reviewed_by INTEGER,
    reviewed_at TIMESTAMPTZ,
    notes TEXT,
    UNIQUE(game_id, official_id, position_id)
);

-- NoteTypes Table
CREATE TABLE note_types (
    note_type_id SERIAL PRIMARY KEY,
    note_type_name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    is_active BOOLEAN DEFAULT TRUE
);

-- GameNotes Table
CREATE TABLE game_notes (
    game_note_id SERIAL PRIMARY KEY,
    game_id INTEGER NOT NULL,
    author_id INTEGER NOT NULL,
    note_type_id INTEGER NOT NULL,
    note_text TEXT NOT NULL,
    is_private BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_game_notes_game ON game_notes(game_id);

-- AssignorRatings Table
CREATE TABLE assignor_ratings (
    assignor_rating_id SERIAL PRIMARY KEY,
    assignor_id INTEGER NOT NULL,
    official_id INTEGER NOT NULL,
    rating DECIMAL(3, 2) CHECK (rating >= 1 AND rating <= 5),
    notes TEXT,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    UNIQUE(assignor_id, official_id)
);

-- Mentors Table
CREATE TABLE mentors (
    mentor_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL UNIQUE,
    bio TEXT
);

-- MentorAuthorizations Table
CREATE TABLE mentor_authorizations (
    mentor_authorization_id SERIAL PRIMARY KEY,
    mentor_id INTEGER NOT NULL,
    sport_id INTEGER NOT NULL,
    league_id INTEGER,
    authorized_by INTEGER NOT NULL,
    authorized_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMPTZ,
    is_active BOOLEAN DEFAULT TRUE
);

-- PerformanceReviews Table
CREATE TABLE performance_reviews (
    performance_review_id SERIAL PRIMARY KEY,
    game_assignment_id INTEGER NOT NULL,
    reviewer_id INTEGER NOT NULL,
    knowledge_of_rules INTEGER CHECK (knowledge_of_rules >= 1 AND knowledge_of_rules <= 5),
    positioning INTEGER CHECK (positioning >= 1 AND positioning <= 5),
    communication INTEGER CHECK (communication >= 1 AND communication <= 5),
    game_management INTEGER CHECK (game_management >= 1 AND game_management <= 5),
    professionalism INTEGER CHECK (professionalism >= 1 AND professionalism <= 5),
    overall_rating INTEGER CHECK (overall_rating >= 1 AND overall_rating <= 5),
    strengths TEXT,
    areas_for_improvement TEXT,
    additional_comments TEXT,
    is_public BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- Parents Table
CREATE TABLE parents (
    parent_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL UNIQUE
);

-- ParentOfficials Table
CREATE TABLE parent_officials (
    parent_official_id SERIAL PRIMARY KEY,
    parent_id INTEGER NOT NULL,
    official_id INTEGER NOT NULL,
    relationship_type VARCHAR(50) CHECK (relationship_type IN ('Parent', 'Guardian', 'Manager')),
    can_view_schedule BOOLEAN DEFAULT TRUE,
    can_view_pay BOOLEAN DEFAULT TRUE,
    can_view_reviews BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(parent_id, official_id)
);

-- Coaches Table
CREATE TABLE coaches (
    coach_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL UNIQUE
);

-- CoachesGames Table
CREATE TABLE coaches_games (
    coaches_games_id SERIAL PRIMARY KEY,
    coach_id INTEGER NOT NULL UNIQUE,
    game_id INTEGER NOT NULL UNIQUE
);

-- PaymentMethods Table
CREATE TABLE payment_methods (
    payment_method_id SERIAL PRIMARY KEY,
    official_id INTEGER NOT NULL,
    method_type VARCHAR(50) CHECK (method_type IN ('BankTransfer', 'Stripe', 'PayPal', 'Manual')),
    method_details JSONB,
    is_default BOOLEAN DEFAULT FALSE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- Payments Table
CREATE TABLE payments (
    payment_id SERIAL PRIMARY KEY,
    match_assignment_id INTEGER NOT NULL,
    official_id INTEGER NOT NULL,
    amount DECIMAL(10, 2) NOT NULL,
    fee DECIMAL(10, 2) DEFAULT 0.00,
    currency VARCHAR(10) DEFAULT 'USD',
    payment_method_id INTEGER,
    status VARCHAR(20) DEFAULT 'Pending' 
        CHECK (status IN ('Pending', 'Processing', 'Paid', 'Failed', 'Cancelled')),
    transaction_reference VARCHAR(200),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,
    processed_at TIMESTAMPTZ,
    created_by INTEGER
);

CREATE INDEX idx_payments_official ON payments(official_id);
CREATE INDEX idx_payments_status ON payments(status);

-- PaymentTransactions Table
CREATE TABLE payment_transactions (
    payment_transaction_id SERIAL PRIMARY KEY,
    payment_id INTEGER NOT NULL,
    provider VARCHAR(100),
    payload JSONB,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

-- AuditLog Table
CREATE TABLE audit_log (
    audit_id SERIAL PRIMARY KEY,
    tenant_id UUID NOT NULL,
    user_id INTEGER NOT NULL,
    table_name VARCHAR(100) NOT NULL,
    record_id INTEGER NOT NULL,
    action VARCHAR(20) NOT NULL CHECK (action IN ('INSERT', 'UPDATE', 'DELETE')),
    old_values JSONB,
    new_values JSONB,
    change_timestamp TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_audit_log_tenant ON audit_log(tenant_id);
CREATE INDEX idx_audit_log_user ON audit_log(user_id);
CREATE INDEX idx_audit_log_table ON audit_log(table_name);
CREATE INDEX idx_audit_log_timestamp ON audit_log(change_timestamp);

-- ActivityLog Table
CREATE TABLE activity_log (
    activity_id SERIAL PRIMARY KEY,
    tenant_id UUID NOT NULL,
    user_id INTEGER NOT NULL,
    activity_type VARCHAR(100) NOT NULL,
    description TEXT,
    ip_address VARCHAR(50),
    user_agent TEXT,
    activity_timestamp TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_activity_log_tenant ON activity_log(tenant_id);
CREATE INDEX idx_activity_log_user ON activity_log(user_id);
CREATE INDEX idx_activity_log_timestamp ON activity_log(activity_timestamp);

-- =============================================
-- FOREIGN KEY CONSTRAINTS
-- =============================================

ALTER TABLE users ADD CONSTRAINT fk_users_roles FOREIGN KEY (role_id) REFERENCES roles(role_id);
ALTER TABLE role_permissions ADD CONSTRAINT fk_role_permissions_role FOREIGN KEY (role_id) REFERENCES roles(role_id);
ALTER TABLE role_permissions ADD CONSTRAINT fk_role_permissions_permission FOREIGN KEY (permission_id) REFERENCES permissions(permission_id);

ALTER TABLE age_levels ADD CONSTRAINT fk_age_levels_sport FOREIGN KEY (sport_id) REFERENCES sports(sport_id);
ALTER TABLE venues ADD CONSTRAINT fk_venues_organization FOREIGN KEY (organization_id) REFERENCES organizations(organization_id);
ALTER TABLE venue_sports ADD CONSTRAINT fk_venue_sports_venue FOREIGN KEY (venue_id) REFERENCES venues(venue_id);
ALTER TABLE venue_sports ADD CONSTRAINT fk_venue_sports_sport FOREIGN KEY (sport_id) REFERENCES sports(sport_id);
ALTER TABLE venue_age_levels ADD CONSTRAINT fk_venue_age_levels_venue FOREIGN KEY (venue_id) REFERENCES venues(venue_id);
ALTER TABLE venue_age_levels ADD CONSTRAINT fk_venue_age_levels_age_level FOREIGN KEY (age_level_id) REFERENCES age_levels(age_level_id);

ALTER TABLE leagues ADD CONSTRAINT fk_leagues_sport FOREIGN KEY (sport_id) REFERENCES sports(sport_id);
ALTER TABLE league_organizations ADD CONSTRAINT fk_league_organizations_league FOREIGN KEY (league_id) REFERENCES leagues(league_id);
ALTER TABLE league_organizations ADD CONSTRAINT fk_league_organizations_organization FOREIGN KEY (organization_id) REFERENCES organizations(organization_id);
ALTER TABLE league_age_levels ADD CONSTRAINT fk_league_age_levels_league FOREIGN KEY (league_id) REFERENCES leagues(league_id);
ALTER TABLE league_age_levels ADD CONSTRAINT fk_league_age_levels_age_level FOREIGN KEY (age_level_id) REFERENCES age_levels(age_level_id);

ALTER TABLE tournaments ADD CONSTRAINT fk_tournaments_organization FOREIGN KEY (organization_id) REFERENCES organizations(organization_id);
ALTER TABLE tournaments ADD CONSTRAINT fk_tournaments_league FOREIGN KEY (league_id) REFERENCES leagues(league_id);
ALTER TABLE tournaments ADD CONSTRAINT fk_tournaments_pay_scale_template FOREIGN KEY (pay_scale_template_id) REFERENCES pay_scale_templates(pay_scale_template_id);

ALTER TABLE positions ADD CONSTRAINT fk_positions_sport FOREIGN KEY (sport_id) REFERENCES sports(sport_id);

ALTER TABLE pay_scale_templates ADD CONSTRAINT fk_pay_scale_templates_organization FOREIGN KEY (organization_id) REFERENCES organizations(organization_id);
ALTER TABLE pay_scale_templates ADD CONSTRAINT fk_pay_scale_templates_created_by FOREIGN KEY (created_by) REFERENCES assignors(assignor_id);

ALTER TABLE pay_scale_rules ADD CONSTRAINT fk_pay_scale_rules_template FOREIGN KEY (pay_scale_template_id) REFERENCES pay_scale_templates(pay_scale_template_id);
ALTER TABLE pay_scale_rules ADD CONSTRAINT fk_pay_scale_rules_sport FOREIGN KEY (sport_id) REFERENCES sports(sport_id);
ALTER TABLE pay_scale_rules ADD CONSTRAINT fk_pay_scale_rules_age_level FOREIGN KEY (age_level_id) REFERENCES age_levels(age_level_id);
ALTER TABLE pay_scale_rules ADD CONSTRAINT fk_pay_scale_rules_position FOREIGN KEY (position_id) REFERENCES positions(position_id);
ALTER TABLE pay_scale_rules ADD CONSTRAINT fk_pay_scale_rules_league FOREIGN KEY (league_id) REFERENCES leagues(league_id);

ALTER TABLE games ADD CONSTRAINT fk_games_organization FOREIGN KEY (organization_id) REFERENCES organizations(organization_id);
ALTER TABLE games ADD CONSTRAINT fk_games_league FOREIGN KEY (league_id) REFERENCES leagues(league_id);
ALTER TABLE games ADD CONSTRAINT fk_games_tournament FOREIGN KEY (tournament_id) REFERENCES tournaments(tournament_id);
ALTER TABLE games ADD CONSTRAINT fk_games_venue FOREIGN KEY (venue_id) REFERENCES venues(venue_id);
ALTER TABLE games ADD CONSTRAINT fk_games_age_level FOREIGN KEY (age_level_id) REFERENCES age_levels(age_level_id);
ALTER TABLE games ADD CONSTRAINT fk_games_pay_scale_template FOREIGN KEY (pay_scale_template_id) REFERENCES pay_scale_templates(pay_scale_template_id);
ALTER TABLE games ADD CONSTRAINT fk_games_created_by FOREIGN KEY (created_by) REFERENCES assignors(assignor_id);

ALTER TABLE assignors ADD CONSTRAINT fk_assignors_user FOREIGN KEY (user_id) REFERENCES users(user_id);
ALTER TABLE assignor_organizations ADD CONSTRAINT fk_assignor_organizations_assignor FOREIGN KEY (assignor_id) REFERENCES assignors(assignor_id);
ALTER TABLE assignor_organizations ADD CONSTRAINT fk_assignor_organizations_organization FOREIGN KEY (organization_id) REFERENCES organizations(organization_id);
ALTER TABLE assignor_organizations ADD CONSTRAINT fk_assignor_organizations_assigned_by FOREIGN KEY (assigned_by) REFERENCES assignors(assignor_id);
ALTER TABLE assignor_leagues ADD CONSTRAINT fk_assignor_leagues_assignor FOREIGN KEY (assignor_id) REFERENCES assignors(assignor_id);
ALTER TABLE assignor_leagues ADD CONSTRAINT fk_assignor_leagues_league FOREIGN KEY (league_id) REFERENCES leagues(league_id);

ALTER TABLE officials ADD CONSTRAINT fk_officials_user FOREIGN KEY (user_id) REFERENCES users(user_id);
ALTER TABLE officials ADD CONSTRAINT fk_officials_preferred_language FOREIGN KEY (preferred_language_id) REFERENCES languages(language_id);
ALTER TABLE officials ADD CONSTRAINT fk_officials_approved_by FOREIGN KEY (approved_by) REFERENCES assignors(assignor_id);

ALTER TABLE official_positions ADD CONSTRAINT fk_official_positions_official FOREIGN KEY (official_id) REFERENCES officials(official_id);
ALTER TABLE official_positions ADD CONSTRAINT fk_official_positions_position FOREIGN KEY (position_id) REFERENCES positions(position_id);
ALTER TABLE official_age_levels ADD CONSTRAINT fk_official_age_levels_official FOREIGN KEY (official_id) REFERENCES officials(official_id);
ALTER TABLE official_age_levels ADD CONSTRAINT fk_official_age_levels_age_level FOREIGN KEY (age_level_id) REFERENCES age_levels(age_level_id);
ALTER TABLE official_leagues ADD CONSTRAINT fk_official_leagues_official FOREIGN KEY (official_id) REFERENCES officials(official_id);
ALTER TABLE official_leagues ADD CONSTRAINT fk_official_leagues_league FOREIGN KEY (league_id) REFERENCES leagues(league_id);
ALTER TABLE official_leagues ADD CONSTRAINT fk_official_leagues_approved_by FOREIGN KEY (approved_by) REFERENCES assignors(assignor_id);
ALTER TABLE official_languages ADD CONSTRAINT fk_official_languages_official FOREIGN KEY (official_id) REFERENCES officials(official_id);
ALTER TABLE official_languages ADD CONSTRAINT fk_official_languages_language FOREIGN KEY (language_id) REFERENCES languages(language_id);
ALTER TABLE official_sport_preferences ADD CONSTRAINT fk_official_sport_preferences_official FOREIGN KEY (official_id) REFERENCES officials(official_id);
ALTER TABLE official_sport_preferences ADD CONSTRAINT fk_official_sport_preferences_sport FOREIGN KEY (sport_id) REFERENCES sports(sport_id);
ALTER TABLE official_ratings ADD CONSTRAINT fk_official_ratings_official FOREIGN KEY (official_id) REFERENCES officials(official_id);
ALTER TABLE official_ratings ADD CONSTRAINT fk_official_ratings_sport FOREIGN KEY (sport_id) REFERENCES sports(sport_id);

ALTER TABLE official_groups ADD CONSTRAINT fk_official_groups_sport FOREIGN KEY (sport_id) REFERENCES sports(sport_id);
ALTER TABLE official_groups ADD CONSTRAINT fk_official_groups_created_by FOREIGN KEY (created_by) REFERENCES assignors(assignor_id);
ALTER TABLE group_members ADD CONSTRAINT fk_group_members_group FOREIGN KEY (group_id) REFERENCES official_groups(group_id);
ALTER TABLE group_members ADD CONSTRAINT fk_group_members_official FOREIGN KEY (official_id) REFERENCES officials(official_id);

ALTER TABLE game_assignments ADD CONSTRAINT fk_game_assignments_game FOREIGN KEY (game_id) REFERENCES games(game_id);
ALTER TABLE game_assignments ADD CONSTRAINT fk_game_assignments_official FOREIGN KEY (official_id) REFERENCES officials(official_id);
ALTER TABLE game_assignments ADD CONSTRAINT fk_game_assignments_position FOREIGN KEY (position_id) REFERENCES positions(position_id);
ALTER TABLE game_assignments ADD CONSTRAINT fk_game_assignments_assigned_by FOREIGN KEY (assigned_by) REFERENCES assignors(assignor_id);

ALTER TABLE game_claims ADD CONSTRAINT fk_game_claims_game FOREIGN KEY (game_id) REFERENCES games(game_id);
ALTER TABLE game_claims ADD CONSTRAINT fk_game_claims_official FOREIGN KEY (official_id) REFERENCES officials(official_id);
ALTER TABLE game_claims ADD CONSTRAINT fk_game_claims_position FOREIGN KEY (position_id) REFERENCES positions(position_id);
ALTER TABLE game_claims ADD CONSTRAINT fk_game_claims_reviewed_by FOREIGN KEY (reviewed_by) REFERENCES assignors(assignor_id);

ALTER TABLE game_notes ADD CONSTRAINT fk_game_notes_game FOREIGN KEY (game_id) REFERENCES games(game_id);
ALTER TABLE game_notes ADD CONSTRAINT fk_game_notes_author FOREIGN KEY (author_id) REFERENCES users(user_id);
ALTER TABLE game_notes ADD CONSTRAINT fk_game_notes_note_type FOREIGN KEY (note_type_id) REFERENCES note_types(note_type_id);

ALTER TABLE assignor_ratings ADD CONSTRAINT fk_assignor_ratings_assignor FOREIGN KEY (assignor_id) REFERENCES assignors(assignor_id);
ALTER TABLE assignor_ratings ADD CONSTRAINT fk_assignor_ratings_official FOREIGN KEY (official_id) REFERENCES officials(official_id);

ALTER TABLE mentors ADD CONSTRAINT fk_mentors_user FOREIGN KEY (user_id) REFERENCES users(user_id);
ALTER TABLE mentor_authorizations ADD CONSTRAINT fk_mentor_authorizations_mentor FOREIGN KEY (mentor_id) REFERENCES mentors(mentor_id);
ALTER TABLE mentor_authorizations ADD CONSTRAINT fk_mentor_authorizations_sport FOREIGN KEY (sport_id) REFERENCES sports(sport_id);
ALTER TABLE mentor_authorizations ADD CONSTRAINT fk_mentor_authorizations_league FOREIGN KEY (league_id) REFERENCES leagues(league_id);
ALTER TABLE mentor_authorizations ADD CONSTRAINT fk_mentor_authorizations_authorized_by FOREIGN KEY (authorized_by) REFERENCES assignors(assignor_id);

ALTER TABLE performance_reviews ADD CONSTRAINT fk_performance_reviews_game_assignment FOREIGN KEY (game_assignment_id) REFERENCES game_assignments(game_assignment_id);
ALTER TABLE performance_reviews ADD CONSTRAINT fk_performance_reviews_reviewer FOREIGN KEY (reviewer_id) REFERENCES mentors(mentor_id);

ALTER TABLE parents ADD CONSTRAINT fk_parents_user FOREIGN KEY (user_id) REFERENCES users(user_id);
ALTER TABLE parent_officials ADD CONSTRAINT fk_parent_officials_parent FOREIGN KEY (parent_id) REFERENCES parents(parent_id);
ALTER TABLE parent_officials ADD CONSTRAINT fk_parent_officials_official FOREIGN KEY (official_id) REFERENCES officials(official_id);

ALTER TABLE coaches ADD CONSTRAINT fk_coaches_user FOREIGN KEY (user_id) REFERENCES users(user_id);
ALTER TABLE coaches_games ADD CONSTRAINT fk_coaches_games_coach FOREIGN KEY (coach_id) REFERENCES coaches(coach_id);
ALTER TABLE coaches_games ADD CONSTRAINT fk_coaches_games_game FOREIGN KEY (game_id) REFERENCES games(game_id);

ALTER TABLE payment_methods ADD CONSTRAINT fk_payment_methods_official FOREIGN KEY (official_id) REFERENCES officials(official_id);
ALTER TABLE payments ADD CONSTRAINT fk_payments_match_assignment FOREIGN KEY (match_assignment_id) REFERENCES game_assignments(game_assignment_id);
ALTER TABLE payments ADD CONSTRAINT fk_payments_official FOREIGN KEY (official_id) REFERENCES officials(official_id);
ALTER TABLE payments ADD CONSTRAINT fk_payments_payment_method FOREIGN KEY (payment_method_id) REFERENCES payment_methods(payment_method_id);
ALTER TABLE payments ADD CONSTRAINT fk_payments_created_by FOREIGN KEY (created_by) REFERENCES assignors(assignor_id);
ALTER TABLE payment_transactions ADD CONSTRAINT fk_payment_transactions_payment FOREIGN KEY (payment_id) REFERENCES payments(payment_id);

ALTER TABLE audit_log ADD CONSTRAINT fk_audit_log_user FOREIGN KEY (user_id) REFERENCES users(user_id);
ALTER TABLE activity_log ADD CONSTRAINT fk_activity_log_user FOREIGN KEY (user_id) REFERENCES users(user_id);

-- =============================================
-- STORED FUNCTIONS/PROCEDURES
-- =============================================

-- Calculate distance using Haversine formula
CREATE OR REPLACE FUNCTION calculate_distance_km(
    lat1 DECIMAL, lon1 DECIMAL,
    lat2 DECIMAL, lon2 DECIMAL
) RETURNS DECIMAL AS $$
DECLARE
    dlat DECIMAL;
    dlon DECIMAL;
    a DECIMAL;
    c DECIMAL;
BEGIN
    dlat := RADIANS(lat2 - lat1);
    dlon := RADIANS(lon2 - lon1);
    
    a := POWER(SIN(dlat / 2), 2) + 
         COS(RADIANS(lat1)) * COS(RADIANS(lat2)) * 
         POWER(SIN(dlon / 2), 2);
    c := 2 * ATAN2(SQRT(a), SQRT(1 - a));
    
    RETURN 6371 * c; -- Earth's radius in km
END;
$$ LANGUAGE plpgsql IMMUTABLE;

-- Calculate Game Pay Function
CREATE OR REPLACE FUNCTION calculate_game_pay(
    p_game_id INTEGER,
    p_official_id INTEGER,
    p_position_id INTEGER
) RETURNS TABLE (
    base_pay_amount DECIMAL(10,2),
    travel_pay_amount DECIMAL(10,2),
    multiplier_applied DECIMAL(5,2),
    final_pay_amount DECIMAL(10,2),
    distance_km DECIMAL(10,2)
) AS $$
DECLARE
    v_base_pay DECIMAL(10,2) := 0;
    v_travel_pay DECIMAL(10,2) := 0;
    v_multiplier DECIMAL(5,2) := 1.00;
    v_final_pay DECIMAL(10,2) := 0;
    v_distance DECIMAL(10,2) := 0;
    v_pay_per_km DECIMAL(10,2) := 0;
    
    v_league_id INTEGER;
    v_sport_id INTEGER;
    v_age_level_id INTEGER;
    v_venue_lat DECIMAL(10,8);
    v_venue_lon DECIMAL(11,8);
    v_official_lat DECIMAL(10,8);
    v_official_lon DECIMAL(11,8);
    v_pay_scale_template_id INTEGER;
BEGIN
    -- Get game details
    SELECT g.league_id, g.age_level_id, v.latitude, v.longitude,
           COALESCE(g.pay_scale_template_id, t.pay_scale_template_id)
    INTO v_league_id, v_age_level_id, v_venue_lat, v_venue_lon, v_pay_scale_template_id
    FROM games g
    INNER JOIN venues v ON g.venue_id = v.venue_id
    LEFT JOIN tournaments t ON g.tournament_id = t.tournament_id
    WHERE g.game_id = p_game_id;
    
    -- Get official location
    SELECT latitude, longitude
    INTO v_official_lat, v_official_lon
    FROM officials
    WHERE official_id = p_official_id;
    
    -- Get sport ID
    IF v_league_id IS NOT NULL THEN
        SELECT sport_id INTO v_sport_id FROM leagues WHERE league_id = v_league_id;
    ELSE
        SELECT sport_id INTO v_sport_id FROM age_levels WHERE age_level_id = v_age_level_id;
    END IF;
    
    -- Calculate distance
    IF v_official_lat IS NOT NULL AND v_official_lon IS NOT NULL 
       AND v_venue_lat IS NOT NULL AND v_venue_lon IS NOT NULL THEN
        v_distance := calculate_distance_km(v_official_lat, v_official_lon, v_venue_lat, v_venue_lon);
    END IF;
    
    -- Find applicable pay scale rule
    SELECT psr.base_pay_amount, psr.pay_per_km, psr.pay_multiplier
    INTO v_base_pay, v_pay_per_km, v_multiplier
    FROM pay_scale_rules psr
    WHERE psr.is_active = TRUE
        AND (v_pay_scale_template_id IS NULL OR psr.pay_scale_template_id = v_pay_scale_template_id OR psr.pay_scale_template_id IS NULL)
        AND (psr.league_id = v_league_id OR psr.league_id IS NULL)
        AND (psr.sport_id = v_sport_id OR psr.sport_id IS NULL)
        AND (psr.age_level_id = v_age_level_id OR psr.age_level_id IS NULL)
        AND (psr.position_id = p_position_id OR psr.position_id IS NULL)
    ORDER BY
        psr.priority ASC,
        CASE WHEN psr.pay_scale_template_id IS NOT NULL THEN 0 ELSE 1 END,
        CASE WHEN psr.league_id IS NOT NULL THEN 0 ELSE 1 END,
        CASE WHEN psr.position_id IS NOT NULL THEN 0 ELSE 1 END,
        CASE WHEN psr.age_level_id IS NOT NULL THEN 0 ELSE 1 END,
        CASE WHEN psr.sport_id IS NOT NULL THEN 0 ELSE 1 END
    LIMIT 1;
    
    -- Calculate travel pay
    IF v_distance > 0 THEN
        v_travel_pay := v_distance * v_pay_per_km;
    END IF;
    
    -- Calculate final pay
    v_final_pay := (v_base_pay + v_travel_pay) * v_multiplier;
    
    RETURN QUERY SELECT v_base_pay, v_travel_pay, v_multiplier, v_final_pay, v_distance;
END;
$$ LANGUAGE plpgsql;

-- Create Payment For Assignment Function
CREATE OR REPLACE FUNCTION create_payment_for_assignment(
    p_game_assignment_id INTEGER,
    p_created_by_assignor_id INTEGER DEFAULT NULL,
    p_payment_method_id INTEGER DEFAULT NULL,
    p_amount DECIMAL(10,2) DEFAULT NULL,
    p_currency VARCHAR(10) DEFAULT 'USD'
) RETURNS INTEGER AS $$
DECLARE
    v_final_amount DECIMAL(10,2);
    v_official_id INTEGER;
    v_payment_id INTEGER;
BEGIN
    -- Get assignment details
    SELECT final_pay_amount, official_id
    INTO v_final_amount, v_official_id
    FROM game_assignments
    WHERE game_assignment_id = p_game_assignment_id;
    
    -- Override amount if provided
    IF p_amount IS NOT NULL THEN
        v_final_amount := p_amount;
    END IF;
    
    -- Select payment method if not provided
    IF p_payment_method_id IS NULL THEN
        SELECT payment_method_id INTO p_payment_method_id
        FROM payment_methods
        WHERE official_id = v_official_id 
          AND is_default = TRUE 
          AND is_active = TRUE
        ORDER BY created_at DESC
        LIMIT 1;
        
        IF p_payment_method_id IS NULL THEN
            SELECT payment_method_id INTO p_payment_method_id
            FROM payment_methods
            WHERE official_id = v_official_id 
              AND is_active = TRUE
            ORDER BY created_at DESC
            LIMIT 1;
        END IF;
    END IF;
    
    -- Insert payment
    INSERT INTO payments (
        match_assignment_id, official_id, amount, currency, 
        payment_method_id, status, created_at, created_by
    ) VALUES (
        p_game_assignment_id, v_official_id, v_final_amount, p_currency,
        p_payment_method_id, 'Pending', CURRENT_TIMESTAMP, p_created_by_assignor_id
    ) RETURNING payment_id INTO v_payment_id;
    
    RETURN v_payment_id;
END;
$$ LANGUAGE plpgsql;

-- Update Payment Status Function
CREATE OR REPLACE FUNCTION update_payment_status(
    p_payment_id INTEGER,
    p_status VARCHAR(20),
    p_transaction_reference VARCHAR(200) DEFAULT NULL,
    p_provider VARCHAR(100) DEFAULT NULL,
    p_payload JSONB DEFAULT NULL,
    p_processed_at TIMESTAMPTZ DEFAULT NULL
) RETURNS INTEGER AS $$
DECLARE
    v_rows_affected INTEGER;
BEGIN
    UPDATE payments
    SET status = p_status,
        transaction_reference = p_transaction_reference,
        processed_at = COALESCE(p_processed_at, CURRENT_TIMESTAMP)
    WHERE payment_id = p_payment_id;
    
    GET DIAGNOSTICS v_rows_affected = ROW_COUNT;
    
    INSERT INTO payment_transactions (payment_id, provider, payload, created_at)
    VALUES (p_payment_id, p_provider, p_payload, CURRENT_TIMESTAMP);
    
    RETURN v_rows_affected;
END;
$$ LANGUAGE plpgsql;

-- =============================================
-- VIEWS (Optional - for common queries)
-- =============================================

CREATE OR REPLACE VIEW v_game_assignments_detailed AS
SELECT 
    ga.game_assignment_id,
    ga.game_id,
    g.game_date,
    g.game_time,
    g.home_team,
    g.away_team,
    g.status AS game_status,
    v.venue_name,
    v.city AS venue_city,
    o.organization_name,
    ga.official_id,
    u.first_name || ' ' || u.last_name AS official_name,
    u.email AS official_email,
    u.phone AS official_phone,
    pos.position_name,
    ga.assignment_status,
    ga.final_pay_amount,
    ga.distance_km,
    s.sport_name,
    al.age_level_name
FROM game_assignments ga
INNER JOIN games g ON ga.game_id = g.game_id
INNER JOIN venues v ON g.venue_id = v.venue_id
INNER JOIN organizations o ON g.organization_id = o.organization_id
INNER JOIN officials off ON ga.official_id = off.official_id
INNER JOIN users u ON off.user_id = u.user_id
INNER JOIN positions pos ON ga.position_id = pos.position_id
INNER JOIN age_levels al ON g.age_level_id = al.age_level_id
INNER JOIN sports s ON al.sport_id = s.sport_id;

-- =============================================
-- COMMENTS
-- =============================================

COMMENT ON DATABASE whistl3r_data IS 'Whistl3r Sports Officials Management System - PostgreSQL Aurora Compatible';
COMMENT ON TABLE users IS 'Core user table for all user types in the system';
COMMENT ON TABLE games IS 'Sports games/matches requiring official assignments';
COMMENT ON TABLE game_assignments IS 'Officials assigned to specific game positions';
COMMENT ON TABLE payments IS 'Payment records for official compensation';

-- =============================================
-- GRANT PERMISSIONS (Adjust as needed)
-- =============================================

-- Example: Create application user and grant permissions
-- CREATE USER whistl3r_app WITH PASSWORD 'your_secure_password';
-- GRANT CONNECT ON DATABASE whistl3r_data TO whistl3r_app;
-- GRANT USAGE ON SCHEMA public TO whistl3r_app;
-- GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO whistl3r_app;
-- GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO whistl3r_app;

-- =============================================
-- END OF SCHEMA
-- =============================================
