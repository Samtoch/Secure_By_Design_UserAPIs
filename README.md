# SECURE BY DESIGN PROJECT (A USER CASE WITH USER MODULE/SERVICES)
This is a .Net 10 project for backend services for the demonstration of Secure by Design using Postgres, NLog, Automapper, Fluent Validator etc with Clean Architecture

# Prerequisites for running the project locally
- .NET 10 SDK
- Postgres Database
- Visual Studio 2022 or later / VS Code
- Postman or any API testing tool
- Docker (optional, for containerization)
- Swagger (for API documentation)
- Dapper (for database interactions)
- ASP.NET Core Identity (for user authentication and authorization)
- AutoMapper (for object mapping)
- FluentValidation (for input validation)
- NLog (for logging)

# Swagger UI
http://localhost:5132/swagger/index.html

# Database Objects
CREATE TABLE IF NOT EXISTS public.users
(
    id uuid NOT NULL,
    username text COLLATE pg_catalog."default" NOT NULL,
    password text COLLATE pg_catalog."default",
    passwordhash text COLLATE pg_catalog."default",
    role text COLLATE pg_catalog."default",
    name text COLLATE pg_catalog."default",
    email text COLLATE pg_catalog."default",
    normalizedemail text COLLATE pg_catalog."default",
    normalizedusername text COLLATE pg_catalog."default",
    firstname text COLLATE pg_catalog."default",
    lastname text COLLATE pg_catalog."default",
    phonenumber text COLLATE pg_catalog."default",
    accessfailedcount integer DEFAULT 0,
    concurrencystamp text COLLATE pg_catalog."default",
    securitystamp text COLLATE pg_catalog."default",
    emailconfirmed boolean DEFAULT false,
    phonenumberconfirmed boolean DEFAULT false,
    twofactorenabled boolean DEFAULT false,
    lockoutenabled boolean DEFAULT false,
    lockoutend timestamp without time zone DEFAULT (now() + '1 year'::interval),
    datecreated timestamp without time zone DEFAULT now(),
    datemodified timestamp without time zone,
    isdeleted smallint DEFAULT 0,
    islocked boolean DEFAULT false,
    neighborhoodid integer,
    lastlogintime timestamp with time zone,
    isloggedin smallint DEFAULT 0,
    city character varying(50) COLLATE pg_catalog."default",
    pictureurl character varying(250) COLLATE pg_catalog."default",
    coverurl character varying(250) COLLATE pg_catalog."default",
    CONSTRAINT users_pkey PRIMARY KEY (id),
    CONSTRAINT users_isdeleted_check CHECK (isdeleted = ANY (ARRAY[0, 1]))
)

CREATE TABLE IF NOT EXISTS public.audit_log
(
    id bigint NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 9223372036854775807 CACHE 1 ),
    logged_at timestamp with time zone NOT NULL DEFAULT now(),
    event character varying(50) COLLATE pg_catalog."default" NOT NULL,
    user_id uuid,
    role character varying(30) COLLATE pg_catalog."default",
    endpoint character varying(300) COLLATE pg_catalog."default",
    status integer NOT NULL,
    ip character varying(45) COLLATE pg_catalog."default",
    correlation_id character varying(100) COLLATE pg_catalog."default",
    CONSTRAINT audit_log_pkey PRIMARY KEY (id)
)

AppSettings Values
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Domain": "https://test.com",
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=db.domain.com;Port=25060;Database=defaultdb;Username=db;Password=xx;SslMode=Require;TrustServerCertificate=true"
  },
  "JwtSettings": {
    "Issuer": "Securebydesign.Api",
    "Audience": "Securebydesign.Client",
    "ExpiryMinutes": 30
  },
  "SecureToken": {
    "ExpiryMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": [ "https://Securebydesign.com", "https://www.Securebydesign.com" ]
  },
  "ForwardedHeaders": {
    "KnownProxies": [] // e.g. [ "10.0.0.5" ] if Nginx is on another host
  },
  "PasswordHashing": {
    "Algorithm": "Argon2id", // or "SHA256"
    "Argon2id": {
      "MemorySizeKb": 512456,
      "Iterations": 60,
      "Parallelism": 1
    }
  }
}
