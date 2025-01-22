-- Criação do banco de dados
CREATE DATABASE CatFactsExplorerDb;
GO

-- Selecionar o banco de dados
USE CatFactsExplorerDb;
GO

-- Criação da tabela CatFacts
CREATE TABLE CatFacts (
    Id INT PRIMARY KEY IDENTITY,       -- Identificador único
    Fact NVARCHAR(500) NOT NULL,       -- Fato sobre gatos
    Length INT NOT NULL                -- Comprimento do fato
);
GO