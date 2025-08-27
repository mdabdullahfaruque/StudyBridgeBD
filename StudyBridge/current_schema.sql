CREATE TABLE "Roles" (
    "Id" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "Description" character varying(500) NOT NULL,
    "SystemRole" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Roles" PRIMARY KEY ("Id")
);


CREATE TABLE "UserProfiles" (
    "Id" uuid NOT NULL,
    "UserId" text NOT NULL,
    "FirstName" character varying(100),
    "LastName" character varying(100),
    "PhoneNumber" character varying(20),
    "DateOfBirth" timestamp with time zone,
    "Country" character varying(100),
    "City" character varying(100),
    "ProfilePictureUrl" character varying(500),
    "Bio" character varying(1000),
    "PreferredLanguage" character varying(10),
    "TimeZone" character varying(50),
    "IsEmailVerified" boolean NOT NULL,
    "IsPhoneVerified" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_UserProfiles" PRIMARY KEY ("Id")
);


CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "GoogleSub" text,
    "Email" text NOT NULL,
    "DisplayName" text NOT NULL,
    "AvatarUrl" character varying(500),
    "PasswordHash" character varying(500),
    "FirstName" character varying(100),
    "LastName" character varying(100),
    "EmailConfirmed" boolean NOT NULL,
    "LastLoginAt" timestamp with time zone,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);


CREATE TABLE "UserSubscriptions" (
    "Id" uuid NOT NULL,
    "UserId" text NOT NULL,
    "SubscriptionType" integer NOT NULL,
    "StartDate" timestamp with time zone NOT NULL,
    "EndDate" timestamp with time zone NOT NULL,
    "IsActive" boolean NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "PaymentReference" character varying(200),
    "Notes" character varying(1000),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_UserSubscriptions" PRIMARY KEY ("Id")
);


CREATE TABLE "RolePermissions" (
    "Id" uuid NOT NULL,
    "RoleId" uuid NOT NULL,
    "Permission" integer NOT NULL,
    "IsGranted" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_RolePermissions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RolePermissions_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE
);


CREATE TABLE "UserRoles" (
    "Id" uuid NOT NULL,
    "UserId" text NOT NULL,
    "RoleId" uuid NOT NULL,
    "AssignedAt" timestamp with time zone NOT NULL,
    "AssignedBy" text,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_UserRoles" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserRoles_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE CASCADE
);


CREATE UNIQUE INDEX "IX_RolePermissions_RoleId_Permission" ON "RolePermissions" ("RoleId", "Permission");


CREATE INDEX "IX_Roles_Name" ON "Roles" ("Name");


CREATE UNIQUE INDEX "IX_Roles_SystemRole" ON "Roles" ("SystemRole");


CREATE UNIQUE INDEX "IX_UserProfiles_UserId" ON "UserProfiles" ("UserId");


CREATE INDEX "IX_UserRoles_RoleId" ON "UserRoles" ("RoleId");


CREATE UNIQUE INDEX "IX_UserRoles_UserId_RoleId" ON "UserRoles" ("UserId", "RoleId");


CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");


CREATE UNIQUE INDEX "IX_Users_GoogleSub" ON "Users" ("GoogleSub");


CREATE INDEX "IX_UserSubscriptions_UserId" ON "UserSubscriptions" ("UserId");


CREATE INDEX "IX_UserSubscriptions_UserId_IsActive" ON "UserSubscriptions" ("UserId", "IsActive");


