CREATE TABLE RefreshTokens
(
    Id                   UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_RefreshTokens PRIMARY KEY,
    UserId               UNIQUEIDENTIFIER NOT NULL,
    TokenHash            NVARCHAR(128)    NOT NULL,
    ExpiresAtUtc         DATETIME2        NOT NULL,
    RevokedAtUtc         DATETIME2        NULL,
    ReplacedByTokenHash  NVARCHAR(128)    NULL,
    CreatedByIp          NVARCHAR(64)     NOT NULL,
    CreatedAt            DATETIME2        NOT NULL CONSTRAINT DF_RefreshTokens_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users (Id)
);
GO

CREATE UNIQUE INDEX UX_RefreshTokens_TokenHash ON RefreshTokens (TokenHash);
GO

CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens (UserId);
GO
