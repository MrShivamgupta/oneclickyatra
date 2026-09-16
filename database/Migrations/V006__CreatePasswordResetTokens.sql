CREATE TABLE PasswordResetTokens
(
    Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_PasswordResetTokens PRIMARY KEY,
    UserId       UNIQUEIDENTIFIER NOT NULL,
    TokenHash    NVARCHAR(128)    NOT NULL,
    ExpiresAtUtc DATETIME2        NOT NULL,
    UsedAtUtc    DATETIME2        NULL,
    CreatedAt    DATETIME2        NOT NULL CONSTRAINT DF_PasswordResetTokens_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_PasswordResetTokens_Users FOREIGN KEY (UserId) REFERENCES Users (Id)
);
GO

CREATE UNIQUE INDEX UX_PasswordResetTokens_TokenHash ON PasswordResetTokens (TokenHash);
GO
