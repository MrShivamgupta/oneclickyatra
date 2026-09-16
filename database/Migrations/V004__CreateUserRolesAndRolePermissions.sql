CREATE TABLE UserRoles
(
    UserId    UNIQUEIDENTIFIER NOT NULL,
    RoleId    UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2        NOT NULL CONSTRAINT DF_UserRoles_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users (Id),
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles (Id)
);
GO

CREATE TABLE RolePermissions
(
    RoleId       UNIQUEIDENTIFIER NOT NULL,
    PermissionId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt    DATETIME2        NOT NULL CONSTRAINT DF_RolePermissions_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, PermissionId),
    CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (RoleId) REFERENCES Roles (Id),
    CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES Permissions (Id)
);
GO
