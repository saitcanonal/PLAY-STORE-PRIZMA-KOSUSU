public static class Schema {
    public static readonly string[] Statements = [
        "CREATE TABLE IF NOT EXISTS progression (id INT PRIMARY KEY, payload LONGTEXT NOT NULL) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin",
        "CREATE TABLE IF NOT EXISTS players (id VARCHAR(32) PRIMARY KEY, name VARCHAR(28) NOT NULL, token_hash CHAR(64) NOT NULL UNIQUE, score INT NOT NULL DEFAULT 0, legacy_score INT NOT NULL DEFAULT 0, record_at VARCHAR(40) NOT NULL, entitlements LONGTEXT NOT NULL, backup LONGTEXT NULL, INDEX ix_score(score)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin",
        "CREATE TABLE IF NOT EXISTS tickets (id VARCHAR(32) PRIMARY KEY, player_id VARCHAR(32) NOT NULL, seed INT NOT NULL, created VARCHAR(40) NOT NULL, result LONGTEXT NULL, INDEX ix_player(player_id), CONSTRAINT fk_ticket_player FOREIGN KEY(player_id) REFERENCES players(id)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin",
        "CREATE TABLE IF NOT EXISTS purchases (transaction_key VARCHAR(255) PRIMARY KEY, player_id VARCHAR(32) NOT NULL, CONSTRAINT fk_purchase_player FOREIGN KEY(player_id) REFERENCES players(id)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin"
    ];
}
